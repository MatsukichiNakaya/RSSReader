using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace RSSReader.Model
{
    /// <summary>
    /// RSSデータ読み込みクラス
    /// </summary>
    public static class RSS
    {
        /// <summary>
        /// RSSに設定してあるサイトのタイトルを取得する
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static String ReadFeedTitle(String url)
        {
            try {
                var xml = XmlDocumentLoad(url);
                var elem = xml.DocumentElement;

                // atom 形式
                if (elem.Name == Define.RSS_HEADER_ATOM) {
                    return elem["title"].InnerText;
                }
                // RSS 1.0, 2.0 形式
                return elem["channel"]["title"].InnerText;
            }
            catch (Exception) {
                // いずれでもない
                return null;
            }
        }

        /// <summary>
        /// RSS feed から記事項目一覧を取得する。
        /// </summary>
        /// <param name="url">RSS feed の URL</param>
        /// <returns>記事項目一覧</returns>
        public static List<FeedItem> ReadFeedItems(String url)
        {
            // xml に使用されている名前空間の取得
            var namespaceList = GetNamespace(url);

            // Xmlを解析してItemまたはentry要素を取得する。
            GetElements(url, out List<Dictionary<String, MarkupElement>> elementItems);

            // 返り値とするアイテムの配列
            var itemList = new List<FeedItem>();
            ConvertResult(elementItems, namespaceList, ref itemList);

            // サムネイル画像の取得処理
            SetThumbUri(itemList);

            return itemList;
        }

        /// <summary>
        /// Xmlの内容を読み込む
        /// </summary>
        /// <param name="url">XmlのURL</param>
        /// <returns>XmlDocumentのデータ</returns>
        private static XmlDocument XmlDocumentLoad(String url)
        {
            var client = new WebClient() { Encoding = Encoding.UTF8 };
            var xmlString = client.DownloadString(url);
            var xml = new XmlDocument();

            // 無効な文字列を削除してデータを読み込む
            xml.LoadXml(Sanitize(xmlString));
            return xml;
        }

        /// <summary>
        /// XmlTextReaderとしてxmlの内容を読み込む
        /// </summary>
        /// <param name="url">XmlのURL</param>
        /// <returns>XmlTextReaderのデータ</returns>
        private static XmlTextReader XmlTextReaderLoad(String url)
        {
            var xml = XmlDocumentLoad(url);
            TextWriter tw = new StringWriter();
            XmlWriter xw = new XmlTextWriter(tw);
            xml.WriteTo(xw);
            // 無効な文字列を削除してデータを読み込む
            return new XmlTextReader(new StringReader(Sanitize(tw.ToString())));
        }

        /// <summary>
        /// Xml無効な文字を削除
        /// </summary>
        /// <param name="xml">xmlの内容</param>
        /// <returns>無効文字を削除した内容</returns>
        private static String Sanitize(String xml)
        {
            var sb = new StringBuilder();

            foreach (var c in xml) {
                var code = (Int32)c;

                if (code == 0x9 ||
                    code == 0xa ||
                    code == 0xd ||
                    (0x20 <= code && code <= 0xd7ff) ||
                    (0xe000 <= code && code <= 0xfffd) ||
                    (0x10000 <= code && code <= 0x10ffff)) {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Xmlから名前空間の取得
        /// </summary>
        /// <param name="url">RSS feed の URL</param>
        /// <returns>名前空間一覧</returns>
        private static String[] GetNamespace(String url)
        {
            var xml = XmlDocumentLoad(url);

            var elem = xml.DocumentElement;
            var results = new List<String>();

            foreach (XmlAttribute att in elem.Attributes) {
                // xmlnsはいらないので後半のみ取得する
                var split = att.Name.Split(':');
                if (1 < split.Length) {
                    results.Add(split[1]);
                }
            }
            return results.ToArray(); ;
        }

        /// <summary>
        /// xmlを解析してそれぞれの要素を取得
        /// </summary>
        /// <param name="url">RSS feed の URL</param>
        /// <param name="elementItems">記事項目一覧(整形無し)</param>
        private static void GetElements(String url,
                                        out List<Dictionary<String, MarkupElement>> elementItems)
        {
            elementItems = new List<Dictionary<String, MarkupElement>>();
            Dictionary<String, MarkupElement> currentItem = null;
            var reader = XmlTextReaderLoad(url);
            String name = String.Empty;

            while (reader.Read()) {
                // CDATAのデータは前に読み込んだタグを親として設定する
                if (reader.NodeType != XmlNodeType.Element) {
                    if (reader.NodeType == XmlNodeType.CDATA) {
                        if (currentItem != null && !String.IsNullOrEmpty(name)) {
                            // CDATAの値を親ノードの値に設定
                            currentItem[name].Value = reader.Value;
                        }
                    }
                    continue;
                }
                name = reader.Name;

                //   "item" RSS1.0, RSS2.0  "entry" atom
                if (name.ToLowerInvariant() == "item" || name.ToLowerInvariant() == "entry") {
                    if (currentItem != null) {
                        elementItems.Add(currentItem);
                    }
                    currentItem = new Dictionary<String, MarkupElement>();
                }
                else if (currentItem != null) {
                    // タグの値を読み込むのと同時に、設定されている属性情報を取得する
                    Dictionary<String, String> att = null;
                    if (reader.HasAttributes) {
                        att = new Dictionary<String, String>();
                        while (reader.MoveToNextAttribute()) {
                            att.Add(reader.Name, reader.Value);
                        }
                    }
                    reader.Read();
                    if (!currentItem.Keys.Contains(name)) {
                        currentItem.Add(name, new MarkupElement(name, reader.Value, att));
                    }
                }
            }
        }

        /// <summary>
        /// FeedItemクラスのデータに変換する。
        /// </summary>
        /// <param name="elementItems">xml要素のデータ</param>
        /// <param name="namespaceList">名前空間のリスト</param>
        /// <param name="resultItems">記事項目一覧</param>
        private static void ConvertResult(List<Dictionary<String, MarkupElement>> elementItems,
                                          IEnumerable<String> namespaceList,
                                          ref List<FeedItem> resultItems)
        {
            foreach (Dictionary<String, MarkupElement> item in elementItems) {
                var gfitem = new FeedItem {
                    ExtraItems = new List<MarkupElement>()
                };

                foreach (String k in item.Keys) {
                    var key = k;
                    // 名前空間を考慮して項目名を設定する
                    String[] temp = k.Split(':');
                    if (1 < temp.Length) {
                        foreach (var nm in namespaceList) {
                            if (temp[0] == nm) { key = temp[1]; }
                        }
                    }
                    // 個々の名称を見て各要素のプロパティに設定。
                    // フォーマットによる名称の違いを定義する。
                    switch (key) {
                        // 表題
                        case "title":
                            gfitem.Title = item[k].Value.Replace('\'', '’');
                            break;
                        // webページへのリンク
                        case "link":
                            gfitem.Link = new Uri(item[k].Attributes == null
                                                    ? item[k].Value
                                                    : item[k].Attributes["href"]);
                            break;
                        // 更新日付要素
                        case "published":
                        case "pubDate":
                        case "issued":
                        case "date":
                            DateTime.TryParse(item[k].Value, out DateTime dt);
                            gfitem.PublishDate = (dt != DateTime.MinValue
                                                    ? dt : DateTime.Now)
                                                    .ToString(FeedItem.DATE_FORMAT);
                            break;
                        // 小見出しなどコンテンツ要素,サマリー
                        case "summary":
                        case "description":
                            gfitem.Summary = item[k].Value;
                            break;
                        // その他の要素
                        default:
                            gfitem.ExtraItems.Add(new MarkupElement() {
                                Name = k,
                                Value = item[k].Value,
                                Attributes = item[k].Attributes,
                            });
                            break;
                    }
                }
                resultItems.Add(gfitem);
            }
        }

        /// <summary>
        /// フィードの要素からサムネイルを取得する
        /// </summary>
        /// <param name="feedItems">記事項目一覧</param>
        private static void SetThumbUri(IEnumerable<FeedItem> feedItems)
        {
            foreach (var feed in feedItems) {
                // サイトごとの定義が必要
                switch (feed.Host) {
                    case FeedItem.HOST_YOUTUBE:
                        feed.ThumbUri = GetYoutubeThumb(feed.ExtraItems);
                        break;
                    default:
                        feed.ThumbUri = GetGenericThumb(feed.ExtraItems);
                        break;
                }
            }
        }

        /// <summary>
        /// youtubeのサムネイル取得
        /// </summary>
        /// <param name="elem">タグデータ</param>
        /// <returns>サムネイルのUrl</returns>
        private static Uri GetYoutubeThumb(IEnumerable<MarkupElement> elem)
        {
            foreach (var item in elem) {
                // 専用のタグがある
                if (item.Name == "media:thumbnail") {

                    return new Uri(item.Attributes["url"]);
                }
            }
            return null;
        }

        /// <summary>
        /// その他サイトの取得
        /// </summary>
        /// <param name="elem">タグデータ</param>
        /// <returns>サムネイルのUri</returns>
        private static Uri GetGenericThumb(IEnumerable<MarkupElement> elem)
        {
            foreach (var item in elem) {
                String[] temp = item.Name.Split(':');
                String key = 1 < temp.Length ? temp[1] : item.Name;

                if (key == "encoded" || key == "content") {
                    if (String.IsNullOrWhiteSpace(item.Value)) {
                        continue;
                    }
                    return GetImgTagSource(item.Value);
                }
            }
            return null;
        }

        /// <summary>
        /// パターンマッチングを使用してソースを取得
        /// </summary>
        /// <param name="document">HTMLソース</param>
        /// <returns>サムネイルのUri</returns>
        private static Uri GetImgTagSource(String document)
        {
            var imgPattern = new Regex(@"<img\s*.*?\s*src=""(?<text>.*?)""");
            String ext;
            foreach (Match item in imgPattern.Matches(document)) {

                ext = Path.GetExtension(item.Groups["text"].Value);
                // 画像のリンクに拡張子があるか否かを判定する
                if (String.IsNullOrEmpty(ext)) {
                    continue;
                }
                if (ext == @".png" || ext == @".jpg" || ext == @".jpeg") {
                    return new Uri(item.Groups["text"].Value);
                }
            }
            return null;
        }
    }
}

