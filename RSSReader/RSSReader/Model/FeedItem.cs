using System;
using System.IO;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace RSSReader.Model
{
    public class FeedItem
    {
        /// <summary>FeedItemで使用する日付のフォーマット</summary>
        public const String DATE_FORMAT = "yyyy/MM/dd HH:mm:ss";

        public const String CHASH_DIR = "chash";

        /// <summary>YouTubeのホスト名</summary>
        public const String HOST_YOUTUBE = "www.youtube.com";

        /// <summary>記事のタイトル</summary>
        public String Title { get; set; }
        /// <summary>更新日時</summary>
        public String PublishDate { get; set; }
        /// <summary>サマリー</summary>
        public String Summary { get; set; }
        /// <summary>記事へのリンク</summary>
        public Uri Link { get; set; }
        /// <summary>既読有無</summary>
        public Boolean IsRead { get; set; }
        /// <summary>記事元のホスト名</summary>
        public String Host { get { return this.Link.Host; } }
        /// <summary>サムネイルのUrl</summary>
        public Uri ThumbUri { get; set; }
        /// <summary>サムネイル画像のソース</summary>
        public ImageSource Thumbnail { get; set; }
        /// <summary>サムネイルの横幅</summary>
        public Int32 ThumbWidth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public FeedItem()
        {
            this.Title = null;
            this.PublishDate = null;
            this.Summary = null;
            this.Link = null;
            this.IsRead = false;

            this.ThumbUri = null;
            this.Thumbnail = null;
            this.ThumbWidth = 0;
        }

        /// <summary>
        /// RSS 2.0 (または、atom)形式で読み込んだデータをFeedItemとして読み込む
        /// </summary>
        /// <param name="item"></param>
        /// <param name="feedUri"></param>
        /// <returns></returns>
        public static FeedItem ReadRSS20(SyndicationItem item)
        {
            var result = new FeedItem() {
                Title = item.Title.Text,
                PublishDate = item.PublishDate.ToString(DATE_FORMAT),
                Summary = item.Summary?.Text ?? String.Empty,
                Link = 0 < item.Links.Count ? item.Links[0].Uri : null,
            };

            // youtube 用サムネ　読み込み
            XmlElement element = null;
            for (Int32 i = 0; i < item.ElementExtensions.Count; i++)
            {
                if (item.ElementExtensions[i].OuterName == "group")
                {
                    element = item.ElementExtensions[i].GetObject<XmlElement>();
                    // データが有ればセットする
                    result.Summary = element.InnerText;
                    break;
                }
                else if (item.ElementExtensions[i].OuterName == "encoded")
                {
                    element = item.ElementExtensions[i].GetObject<XmlElement>();
                    break;
                }
            }
            if (element == null) { return result; }

            XmlNode node = null;
            for (Int32 i = 0; i < element.ChildNodes.Count; i++)
            {
                if (element.ChildNodes[i].LocalName == "thumbnail")
                {
                    node = element.ChildNodes[i];
                    String uri = node?.Attributes["url"]?.InnerText ?? String.Empty;
                    if (String.IsNullOrEmpty(uri)) { return result; }

                    result.ThumbUri = new Uri(uri);
                    return result;
                }
                else if (element.ChildNodes[i].LocalName == "#text")
                {
                    node = element.ChildNodes[i];
                    String uri = GetImgTagSource(node.InnerText);
                    if (String.IsNullOrEmpty(uri)) { return result; }

                    result.ThumbUri = new Uri(uri);
                    return result;
                }
            }
            return result;
        }

        /// <summary>
        /// RSS 1.0 形式で読み込んだデータをFeedItemとして読み込む
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        public static FeedItem ReadRSS10(XmlElement node, XmlNamespaceManager nsmgr)
        {
            if (node == null)　{ return null; }
            if (nsmgr == null) { return null; }
            String url = GetContents(node, nsmgr);
            return new FeedItem() {
                Title = node.SelectNodes("rss:title", nsmgr)[0].InnerText,
                PublishDate = DateTime.Parse(node.SelectNodes("dc:date", nsmgr)[0].InnerText).ToString(DATE_FORMAT),
                Summary = node.SelectNodes("rss:description", nsmgr)[0].InnerText,
                Link = new Uri(node.SelectNodes("rss:link", nsmgr)[0].InnerText),
                // サムネイル情報
                ThumbUri = String.IsNullOrEmpty(url) ? null : new Uri(url),
                ThumbWidth = String.IsNullOrEmpty(url) ? 0 : 160
            };
        }

        /// <summary>
        /// サムネイル画像のパス情報の取得
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        protected static String GetContents(XmlElement node, XmlNamespaceManager nsmgr)
        {
            foreach (var item in node.ChildNodes)
            {
                if (item is XmlElement element)
                {
                    if (element.LocalName == "encoded")
                    {
                        return GetImgTagSource(element.InnerText);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// パターンマッチングを使用してソースを取得
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        protected static String GetImgTagSource(String document)
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
                    return item.Groups["text"].Value;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Boolean ExistsChashDirectory(String dir)
        {
            String chashDir = $@"{CHASH_DIR}\{dir}";
            if (!Directory.Exists(chashDir))
            {
                Directory.CreateDirectory(chashDir);
            }
            return true;
        }

        /// <summary>
        /// サムネイルキャッシュの保存先のパスを取得する
        /// </summary>
        /// <param name="url"></param>
        /// <param name="masterID"></param>
        /// <returns></returns>
        public static String GetChashPath(String url, Int32 masterID, String host)
        {
            switch (host)
            {
                case HOST_YOUTUBE:
                    // URLからファイル名と親ディレクトリ名を取得
                    var uri = new Uri(url);
                    var subDir = uri.Segments[uri.Segments.Length - 2].Replace("/", "");
                    var localPath = $@"{CHASH_DIR}\{masterID}\{subDir}";

                    if (!Directory.Exists(localPath))
                    {
                        Directory.CreateDirectory(localPath);
                    }
                    return $@"{localPath}\{Path.GetFileName(url)}";
            }
            return $@".\{CHASH_DIR}\{masterID}\{Path.GetFileName(url)}";
        }

        /// <summary>
        /// キャッシュからサムネを呼び出す
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BitmapImage ReadChashThumb(String path)
        {
            if (String.IsNullOrEmpty(path)) { return null; }

            var imageSource = new BitmapImage();

            try
            {
                imageSource.BeginInit();
                imageSource.UriSource = new Uri(Path.GetFullPath(path), UriKind.RelativeOrAbsolute);
                imageSource.EndInit();

                imageSource.DownloadCompleted += new EventHandler((Object sender, EventArgs e) => {
                    // 必要あれば画像読み込み後の処理を入れる
                });
            }
            catch (Exception)
            {
                imageSource = null;
            }

            return imageSource;
        }

        /// <summary>
        /// 読み込み処理(画像をダウンロードして取得)
        /// </summary>
        /// <remarks>
        /// ダウンロードした画像はキャッシュ保存される
        /// </remarks>
        public static BitmapImage DownloadThumb(String url, Int32 masterID, String host)
        {
            if (String.IsNullOrEmpty(url))　{ return null; }

            var imageSource = new BitmapImage();
            try
            {
                imageSource.BeginInit();
                imageSource.UriSource = new Uri(url);
                imageSource.EndInit();

                // ダウンロード完了しないと保存できるデータがないので完了イベントで保存を行う
                imageSource.DownloadCompleted += new EventHandler((Object sender, EventArgs e) => {
                    if (sender is ImageSource source)
                    {
                        try
                        {   // ローカルパス取得
                            String localPath = System.Web.HttpUtility.UrlDecode(GetChashPath(url, masterID, host));

                            using (var stream = new FileStream(localPath, FileMode.Create))
                            {
                                var enc = new JpegBitmapEncoder();
                                enc.Frames.Add(BitmapFrame.Create((BitmapSource)source));
                                enc.Save(stream);
                            }
                        }
                        catch (Exception) { Console.WriteLine("Error Download"); }
                    }
                });
            }
            catch (Exception)
            {
                Console.WriteLine("Error File path");
                imageSource = null;
            }
            return imageSource;
        }
    }
}
