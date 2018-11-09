using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Project.DataBase;
using Project.IO;
using RSSReader.Model;

using static RSSReader.Define;

namespace RSSReader.Pages
{
    /// <summary>
    /// PickupPage.xaml の相互作用ロジック
    /// </summary>
    public partial class PickupPage : Page
    {
        /// <summary>
        /// google chrome ブラウザパス
        /// </summary>
        /// <remarks>
        /// Todo : デフォルトブラウザとの切替
        /// </remarks>
        private String ChromePath { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PickupPage()
        {
            InitializeComponent();

            // Chromeのパス読み込み
            this.ChromePath = Registry.GetValue(ChromeRegKey, null) as String;

            // ピックアップアイテムの取得
            this.FeedList.ItemsSource = GetFeedPickItems();
        }

        /// <summary>
        /// 記事ページ ブラウザ起動処理 ダブルクリック
        /// </summary>
        private void ListBoxItem_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item) {
                if (item.Content is FeedItem feed) {
                    CommFunc.StartBrowser(this.ChromePath, feed);
                }
            }
        }

        /// <summary>
        /// 記事ページ ブラウザ起動処理 エンターキー
        /// </summary>
        private void FeedList_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) { return; }

            if (sender is ListBox box) {
                if (box.SelectedValue is FeedItem feed) {
                    CommFunc.StartBrowser(this.ChromePath, feed);
                }
            }
        }

        /// <summary>
        /// ピックアップアイテムの項目を取得
        /// </summary>
        /// <returns>ピックアップアイテム</returns>
        private IEnumerable<FeedItem> GetFeedPickItems()
        {
            IEnumerable<FeedItem> items = null;
            using (var db = new SQLite(MASTER_PATH)) {
                db.Open();
                try {
                    // IDのリストを取得
                    var ret = db.Select("select * from pickup")["log_id"];
                    // アイテムを抽出
                    var sql = $"select * from log where (log_id) in ({String.Join(",", ret)})";
                    items = CommFunc.GetLogItems(db, sql)?.ToArray();
                }
                catch (Exception) {
                }
                db.Close();
            }
            // サムネイル読み込み
            if (items != null) {
                if (App.Configure?.IsShowImage ?? false) {
                    Int32 masterID = 0;
                    foreach (var item in items) {
                        masterID = Int32.Parse(item.MasterID);
                        item.Thumbnail = CommFunc.GetImage(item.ThumbUri, masterID, item.Host);
                        if (item.ThumbUri != null) {
                            item.ThumbWidth = DEFAULT_PIC_WIDTH;
                        }
                    }
                }
                else {
                    foreach (var item in items) {
                        item.ThumbWidth = 0;
                    }
                }
            }
            return items;
        }
    }
}
