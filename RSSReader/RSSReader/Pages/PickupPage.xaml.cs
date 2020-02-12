using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            this.ArticleBlock.Text = this.FeedList.Items.Count.ToString();
        }

        /// <summary>
        /// 記事ページ ブラウザ起動処理 ダブルクリック
        /// </summary>
        private void ListBoxItem_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item) {
                if (item.Content is FeedItem feed) {
                    //CommFunc.StartBrowser(this.ChromePath, feed);
                    CommFunc.NavigateMyBrowser(feed);
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
                    //CommFunc.StartBrowser(this.ChromePath, feed);
                    CommFunc.NavigateMyBrowser(feed);
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

                    foreach (var item in items) {
                        var idRet = db.Select($"select * from rss_master where id={item.MasterID}");
                        item.SiteName = idRet["site"][0];
                    }
                }
                catch (Exception) {
                }
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

        /// <summary>
        /// 既読の項目を削除するボタン
        /// </summary>
        private void ReadReleaseButton_Click(Object sender, RoutedEventArgs e)
        {
            if (!(this.FeedList.ItemsSource is IEnumerable<FeedItem> items)) { return; }

            // 既読のログIDを取得する。
            var logIDs = items.Where(x => x.IsRead).Select(x => x.ID);

            // 削除候補のIDをまとめて削除する
            CommFunc.DBCommit(
                $"delete from pickup where (log_id) in ({String.Join(",", logIDs)})");

            // ピックアップアイテムの取得
            this.FeedList.ItemsSource = GetFeedPickItems();

            this.ArticleBlock.Text = this.FeedList.Items.Count.ToString();
        }

        /// <summary>
        /// 一つの項目を削除
        /// </summary>
        private void MenuItemRelease_Click(Object sender, RoutedEventArgs e)
        {
            if (!(this.FeedList.ItemsSource is IEnumerable<FeedItem> items)) { return; }
            if (!(this.FeedList.SelectedItem is FeedItem target)){ return; }

            // DBから登録削除
            CommFunc.DBCommit($"delete from pickup where log_id = {target.ID}");

            this.FeedList.ItemsSource = ExceptID(items, target.ID);

            this.ArticleBlock.Text = this.FeedList.Items.Count.ToString();
        }

        /// <summary>
        /// 指定のIDを除いたリストを返す
        /// </summary>
        /// <param name="itemList">FeedItemのリスト</param>
        /// <param name="id">除外対象のID</param>
        /// <returns>指定のIDを除いたリスト</returns>
        private IEnumerable<FeedItem> ExceptID(IEnumerable<FeedItem> itemList, String id)
        {
            foreach (var item in itemList) {
                if(id == item.ID) { continue; }
                yield return item;
            }
        }
    }
}
