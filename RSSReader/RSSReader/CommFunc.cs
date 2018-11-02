
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Project.DataBase;
using Project.Serialization.Xml;
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
            }
        }
    }
}
