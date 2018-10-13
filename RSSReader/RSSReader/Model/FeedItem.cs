using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RSSReader.Model
{
    /// <summary>
    /// RSS feed のデータクラス
    /// </summary>
    public class FeedItem
    {
        /// <summary>FeedItemで使用する日付のフォーマット</summary>
        public const String DATE_FORMAT = "yyyy/MM/dd HH:mm:ss";

        /// <summary></summary>
        public const String CHASH_DIR = "chash";

        /// <summary>YouTubeのホスト名</summary>
        public const String HOST_YOUTUBE = "www.youtube.com";

        /// <summary>log ID</summary>
        public String ID { get; set; }

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

        /// <summary>その他の項目</summary>
        public List<MarkupElement> ExtraItems { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeedItem()
        {
            this.ID = null; 
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
        /// キャッシュ用のディレクトリ有無確認
        /// </summary>
        /// <param name="dir">ディレクトリパス</param>
        /// <returns>ディレクトリ有無</returns>
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
        /// <param name="picUrl">画像URL</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <param name="host">webサイトのホスト名</param>
        /// <returns>キャッシュディレクトリのパス</returns>
        public static String GetChashPath(String picUrl, Int32 masterID, String host)
        {
            // サイトによってURLの仕様が違うのでケース別に対処する
            switch (host) {
                // Youtubeは画像名称が同じなのでディレクトリを一階層持たせる
                case HOST_YOUTUBE:
                    if (String.IsNullOrEmpty(picUrl)) { return null; }
                    var uri = new Uri(picUrl);
                    var subDir = uri.Segments[uri.Segments.Length - 2].Replace("/", "");
                    var localPath = $@"{CHASH_DIR}\{masterID}\{subDir}";

                    if (!Directory.Exists(localPath)) {
                        Directory.CreateDirectory(localPath);
                    }
                    return $@"{localPath}\{Path.GetFileName(picUrl)}";
            }
            
            return $@".\{CHASH_DIR}\{masterID}\{Path.GetFileName(picUrl)}";
        }

        /// <summary>
        /// キャッシュからサムネを取得する
        /// </summary>
        /// <param name="picPath">画像のパス</param>
        /// <returns>画像データ</returns>
        public static BitmapImage ReadChashThumb(String picPath)
        {
            if (String.IsNullOrEmpty(picPath)) { return null; }
            var bmpImage = new BitmapImage();
            try {
                bmpImage.BeginInit();
                bmpImage.UriSource = new Uri(Path.GetFullPath(picPath), UriKind.RelativeOrAbsolute);
                bmpImage.EndInit();

                bmpImage.DownloadCompleted += new EventHandler((Object sender, EventArgs e) => {
                    // 必要あれば画像読み込み後の処理を入れる
                });
            }
            catch (Exception) {
                bmpImage = null;
            }
            return bmpImage;
        }

        /// <summary>
        /// web上から画像をダウンロードしてサムネを取得する。
        /// </summary>
        /// <param name="picUrl">画像URL</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <param name="host">webサイトのホスト名</param>
        /// <returns>画像データ</returns>
        /// <remarks>
        /// ダウンロードした画像はキャッシュ保存する。
        /// </remarks>
        public static BitmapImage DownloadThumb(String picUrl, Int32 masterID, String host)
        {
            if (String.IsNullOrEmpty(picUrl)) { return null; }

            var imageSource = new BitmapImage();
            try {
                imageSource.BeginInit();
                imageSource.UriSource = new Uri(picUrl);
                imageSource.EndInit();

                // ダウンロード完了しないと保存できるデータがないので完了イベントで保存を行う
                imageSource.DownloadCompleted += new EventHandler((Object sender, EventArgs e) => {
                    if (sender is ImageSource source) {
                        try {
                            String localPath = HttpUtility.UrlDecode(GetChashPath(picUrl, masterID, host));
                            if (String.IsNullOrEmpty(localPath)) { return; }

                            using (var stream = new FileStream(localPath, FileMode.Create)) {
                                var enc = new JpegBitmapEncoder();
                                enc.Frames.Add(BitmapFrame.Create((BitmapSource)source));
                                enc.Save(stream);
                            }
                        }
                        catch (Exception) { Console.WriteLine("Error Download"); }
                    }
                });
            }
            catch (Exception) {
                Console.WriteLine("Error File path");
                imageSource = null;
            }
            return imageSource;
        }
    }
}