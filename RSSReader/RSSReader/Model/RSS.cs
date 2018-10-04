using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace RSSReader.Model
{
    public static class RSS
    {
        /// <summary>
        /// RSSに設定してあるサイトのタイトルを取得する
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static String ReadFeedTitle(String url)
        {
            try
            {
                var xml = new XmlDocument();
                xml.Load(url);
                var elem = xml.DocumentElement;

                // atom 形式
                if (elem.Name == Define.RSS_HEADER_ATOM)
                {
                    return elem["title"].InnerText;
                }
                // RSS 1.0, 2.0 形式
                return elem["channel"]["title"].InnerText;
            }
            catch (Exception)
            {
                // いずれでもない
                return null;
            }
        }

#if false
        /// <summary>
        /// RSSのデータからFeedItemを読み込む
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IEnumerable<FeedItem> ReadFeedItems(String url)
        {

            var xml = new XmlDocument();
            xml.Load(url);
            XmlElement elem = xml.DocumentElement;
            String rssRootNodeTag = elem.Name;

            switch (rssRootNodeTag)
            {   // RSS1.0の場合、ルートノードはrdf:RDFタグ
                case Define.RSS_HEADER_10:
                    return ReadRSS10(url);
                // RSS2.0の場合、ルートノードはrssタグである。
                case Define.RSS_HEADER_20:
                // atomの場合、ルートノードはfeedタグである。
                case Define.RSS_HEADER_ATOM:
                    // RSS 2.0 と atomは同ロジックで読み込める
                    return ReadRSS20(url);
            }
            return null;

        }

        /// <summary>
        /// RSS 1.0 読み込み
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IEnumerable<FeedItem> ReadRSS10(String url)
        {
            var xml = new XmlDocument();

            xml.Load(url);
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            nsmgr.AddNamespace("rss", "http://purl.org/rss/1.0/");
            nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

            var items = new List<FeedItem>();

            foreach (XmlElement node in xml.SelectNodes("/rdf:RDF/rss:item", nsmgr))
            {
                yield return FeedItem.ReadRSS10(node, nsmgr);
            }
        }

        /// <summary>
        /// RSS 2.0 読み込み
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IEnumerable<FeedItem> ReadRSS20(String url)
        {
            using (var rdr = XmlReader.Create(url))
            {
                var feed = SyndicationFeed.Load(rdr);
                foreach (var item in feed.Items)
                {
                    yield return FeedItem.ReadRSS20(item);
                }
            }
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static List<FeedItem> ReadFeedItems(String url)
        {
            // Xmlを解析してItemまたはentry要素を取得する。
            GetElements(url,
                out List<Dictionary<String, MarkupElement>> items,
                out List<String> namespaceList);

            // now create a List of type GenericFeedItem
            var itemList = new List<FeedItem>();

            ConvertResult(items, namespaceList, ref itemList);

            SetThumbnail(itemList, namespaceList);

            return itemList;
        }

        /// <summary>
        /// xmlを解析してそれぞれの要素を取得
        /// </summary>
        /// <param name="url"></param>
        /// <param name="items"></param>
        /// <param name="namespaceList"></param>
        private static void GetElements(String url,
                                        out List<Dictionary<String, MarkupElement>> items,
                                        out List<String> namespaceList)
        {
            items = new List<Dictionary<String, MarkupElement>>();
            Dictionary<String, MarkupElement> currentItem = null;
            var reader = new XmlTextReader(url);
            namespaceList = new List<String>();

            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                { continue; }
                String name = reader.Name;

                if (name.ToLowerInvariant() == "feed" ||
                    name.ToLowerInvariant() == "rdf:rdf" ||
                    name.ToLowerInvariant() == "rss")
                {
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            var split = reader.Name.Split(':');
                            if (1 < split.Length)
                            {
                                namespaceList.Add(split[1]);
                            }
                        }
                    }
                }

                if (name.ToLowerInvariant() == "item" || name.ToLowerInvariant() == "entry")
                {
                    // Save previous item
                    if (currentItem != null)
                    {
                        items.Add(currentItem);
                    }
                    // Create new item
                    currentItem = new Dictionary<String, MarkupElement>();
                }
                else if (currentItem != null)
                {
                    Dictionary<String, String> att = null;
                    if (reader.HasAttributes)
                    {
                        att = new Dictionary<String, String>();
                        while (reader.MoveToNextAttribute())
                        {
                            att.Add(reader.Name, reader.Value);
                        }
                    }
                    reader.Read();
                    // some feeds can have duplicate keys, so we don't want to blow up here:
                    if (!currentItem.Keys.Contains(name))
                    {
                        currentItem.Add(name, new MarkupElement(reader.Value, att));
                    }
                }
            }
        }

        /// <summary>
        /// FeedItemクラスのデータに変換する。
        /// </summary>
        /// <param name="elementItems">xml要素のデータ</param>
        /// <param name="namespaceList">名前空間のリスト</param>
        /// <param name="resultItems">返り値　FeedItemクラス</param>
        private static void ConvertResult(List<Dictionary<String, MarkupElement>> elementItems,
                                          List<String> namespaceList,
                                          ref List<FeedItem> resultItems)
        {
            foreach (Dictionary<String, MarkupElement> item in elementItems)
            {
                var gfitem = new FeedItem {
                    ExtraItems = new List<MarkupElement>()
                };

                foreach (String k in item.Keys)
                {
                    var key = k;
                    String[] temp = k.Split(':');
                    if (1 < temp.Length)
                    {
                        foreach (var nm in namespaceList)
                        {
                            if (temp[0] == nm) { key = temp[1]; }
                        }
                    }

                    switch (key)
                    {
                        case "title":
                            gfitem.Title = item[k].Value;
                            break;
                        case "link":
                            gfitem.Link = new Uri(item[k].Attributes == null
                                                    ? item[k].Value
                                                    : item[k].Attributes["href"]);
                            break;
                        case "published":
                        case "pubDate":
                        case "issued":
                        case "date":
                            DateTime.TryParse(item[k].Value, out DateTime dt);
                            gfitem.PublishDate = (dt != DateTime.MinValue
                                                    ? dt : DateTime.Now)
                                                    .ToString(FeedItem.DATE_FORMAT);
                            break;
                        case "content":
                        case "description":
                            gfitem.Summary = item[k].Value;
                            break;
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
        /// <param name="feedItems"></param>
        private static void SetThumbnail(List<FeedItem> feedItems, List<String> namespaceList)
        {
            foreach (var feed in feedItems)
            {
                switch (feed.Host)
                {
                    case FeedItem.HOST_YOUTUBE:
                        feed.ThumbUri = GetYoutubeThumb(feed.ExtraItems);
                        break;
                    default:
                        feed.ThumbUri = GetGenericThumb(feed.ExtraItems, namespaceList);
                        break;
                }
            }
        }

        /// <summary>
        /// youtubeのサムネイル取得
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        private static Uri GetYoutubeThumb(List<MarkupElement> elem)
        {
            foreach (var item in elem)
            {
                if (item.Name == "media:thumbnail")
                {
                    
                    return new Uri(item.Attributes["url"]);
                }
            }
            return null;
        }

        /// <summary>
        /// その他サイトの取得
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="namespaceList"></param>
        /// <returns></returns>
        private static Uri GetGenericThumb(List<MarkupElement> elem, List<String> namespaceList)
        {
            foreach (var item in elem)
            {
                String[] temp = item.Name.Split(':');
                String key = 1 < temp.Length ? temp[1] : item.Name;

                if (key == "encoded")
                {
                    if (String.IsNullOrWhiteSpace(item.Value))
                    {
                        return null;
                    }
                    
                    return GetImgTagSource(item.Value);
                }
            }
            return null;
        }

        /// <summary>
        /// パターンマッチングを使用してソースを取得
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private static Uri GetImgTagSource(String document)
        {
            //var imgPattern = new Regex(@"<img src=""(?<text>.*?)"".*?>");
            //var imgPattern = new Regex(@"<img\s*.*?\s*src=""(?<text>.*?)"".*?>");
            var imgPattern = new Regex(@"<img\s*.*?\s*src=""(?<text>.*?)""");
            String ext;
            foreach (Match item in imgPattern.Matches(document))
            {
                ext = Path.GetExtension(item.Groups["text"].Value);
                // 画像のリンクに拡張子があるか否かを判定する
                if (String.IsNullOrEmpty(ext))
                {
                    continue;
                }
                if (ext == @".png" || ext == @".jpg" || ext == @".jpeg")
                {
                    return new Uri(item.Groups["text"].Value);
                }
            }
            return null;
        }
    }
}
