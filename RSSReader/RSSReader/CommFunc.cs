
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Project.DataBase;
using Project.Serialization.Xml;
using Project.Windows;
using RSSReader.Model;

using static RSSReader.Define;

namespace RSSReader
{
    internal static class CommFunc
    {
        /// <summary>
        /// 設定ファイルの読み込み
        /// </summary>
        /// <typeparam name="T">読み込むクラスの型名</typeparam>
        /// <param name="confPath">設定ファイルのパス</param>
        /// <returns>設定ファイルの読み込み値</returns>
        public static RssConfigure ConfigLoad()
        {
            if (File.Exists(XML_PATH)) {
                return XmlSerializer.Load<RssConfigure>(XML_PATH);
            }

            // 読み込めない場合は初期値設定の値を返す。
            var result = new RssConfigure();
            XmlSerializer.Save(result, XML_PATH);
            
            return result;
        }

        /// <summary>
        /// 画像の読み込み
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>イメージソース</returns>
        public static ImageSource ReadImage(String path)
        {
            if ( ! File.Exists(path)) {
                return null;
            }
            var bmpImage = new BitmapImage();
            try {
                bmpImage.BeginInit();
                bmpImage.UriSource = new Uri(Path.GetFullPath(path),
                                             UriKind.RelativeOrAbsolute);
                bmpImage.EndInit();
            }
            catch (Exception) {
                bmpImage = null;
            }
            return bmpImage;
        }

        /// <summary>
        /// DBファイルが無い場合に作成する。
        /// </summary>
        public static void CreateDB()
        {
            SQLite.CreateDB(MASTER_PATH);

            using (var db = new SQLite(MASTER_PATH)) {
                db.Open();
                // サイトのRSS feed URLを保存するテーブル
                db.CreateTable("CREATE TABLE rss_master(" +
                    "id integer primary key," +
                    " site text not null," +
                    " url text not null);");
                // ダウンロードしたfeedをログとして保存するためのテーブル
                db.CreateTable("CREATE TABLE log(" +
                    "log_id integer primary key," +
                    " master_id integer not null," +
                    " title text," +
                    " page_url text not null," +
                    " summary text," +
                    " is_read integer not null," +
                    " reg_date text not null," +
                    " thumb_url text);");
                // 更新時刻を記録するためのテーブル
                db.CreateTable("CREATE TABLE sync(" +
                    "sync_id integer primary key," +
                    " master_id integer not null," +
                    " last_update text not null);");
                // 後で見るためのアイテムを登録するテーブル
                db.CreateTable("CREATE TABLE pickup(" +
                    "pick_id integer primary key," +
                    " log_id integer not null);");
            }
        }

        /// <summary>
        /// コンボボックスに設定するデータを取得する
        /// </summary>
        /// <returns>登録サイト一覧</returns>
        public static IEnumerable<RssSiteInfo> GetSiteInfo()
        {
            var cmbItems = new List<RssSiteInfo>();
            using (var db = new SQLite(MASTER_PATH)) {

                db.Open();

                var ret = db.Select("select * from rss_master");
                if (ret.Count == 0) {
                    return cmbItems;
                }
                Int32 count = ret["id"].Count;
                for (Int32 i = 0; i < count; i++) {
                    cmbItems.Add(new RssSiteInfo() {
                        ID = Int32.Parse(ret["id"][i]),
                        SiteName = ret["site"][i],
                        Link = ret["url"][i],
                    });
                }
            }
            return cmbItems;
        }

        /// <summary>
        /// DBからFeedItemを取得する
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="sql">実行するSQL</param>
        /// <returns>feed項目一覧</returns>
        public static IEnumerable<FeedItem> GetLogItems(SQLite db, String sql)
        {
            var ret = db.Select(sql);

            if (0 < ret.Count) {
                Int32 count = ret["log_id"].Count;
                for (Int32 i = 0; i < count; i++) {
                    yield return new FeedItem() {
                        ID = ret["log_id"][i],
                        MasterID = ret["master_id"][i],
                        Title = ret["title"][i],
                        PublishDate = ret["reg_date"][i],
                        Summary = ret["summary"][i]?.Replace("'", ""),
                        Link = new Uri(ret["page_url"][i]),
                        IsRead = Int32.Parse(ret["is_read"][i]) == 1,
                        ThumbUri = String.IsNullOrEmpty(ret["thumb_url"][i])
                                        ? null : new Uri(ret["thumb_url"][i]),
                    };
                }
            }
        }

        /// <summary>
        /// サムネ画像をダウンロード、または、キャッシュから読み込む
        /// </summary>
        /// <param name="url">サムネイルのUrl</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <param name="host">webサイトのホスト名</param>
        /// <returns>画像データ</returns>
        public static ImageSource GetImage(Uri url, Int32 masterID, String host)
        {
            String localPath = FeedItem.GetChashPath(url?.AbsoluteUri, masterID, host);

            if (File.Exists(localPath)) {
                // chashから読み込み
                return FeedItem.ReadChashThumb(localPath);
            }
            else {
                // ダウンロード
                return FeedItem.DownloadThumb(url?.AbsoluteUri, masterID, host);
            }
        }

        /// <summary>
        /// Uriをもとにブラウザを起動する。
        /// </summary>
        /// <param name="item"></param>
        public static void StartBrowser(String browserPath, FeedItem item)
        {
            item.IsRead = true;

            // 既読履歴を更新
            UpdateReadHistory(item);

            // ブラウザを起動
            Process.Start(browserPath, $"{App.Configure?.BrowserOption ?? ""} {item.Link}");

            // 自動で最小化するオプション
            if (App.Configure?.IsAutoMinimize ?? false) {
                var bgw = WindowInfo.FindWindowByName(null, TITLE);
                WinMessage.Send(bgw, Window_MIN_MESSAGE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        /// <summary>
        /// 既読済みの設定を行う
        /// </summary>
        /// <param name="item">feed項目</param>
        private static void UpdateReadHistory(FeedItem item)
        {
            var masterID = Int32.Parse(item.MasterID);
            if (masterID < 0) { return; }
            // 既読は [ 1 ] を設定する。
            DBCommit($"update log set is_read = 1 where log_id = {item.ID}");
        }

        /// <summary>
        /// DBへの書き込み処理
        /// </summary>
        /// <param name="sql">SQL文</param>
        /// <returns>書き込み成否</returns>
        public static Boolean DBCommit(String sql)
        {
            var isCommit = false;
            using (var db = new SQLite(MASTER_PATH)) {
                db.Open();
                try {
                    db.BeginTransaction();
                    db.Update(sql);
                    isCommit = true;
                }
                catch (Exception) {
                    isCommit = false;
                }
                finally {
                    db.EndTransaction(isCommit);
                }
            }
            return isCommit;
        }
    }
}
