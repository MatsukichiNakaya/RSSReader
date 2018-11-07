using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project.DataBase;
using RSSReader.Model;

using static RSSReader.Define;

namespace RSSReader.Pages
{
    /// <summary>
    /// RSSEditPage.xaml の相互作用ロジック
    /// </summary>
    public partial class RSSEditPage : Page
    {
        /// <summary>RSS feedの編集状態</summary>
        private EditMode EditMode { get; set; }

        /// <summary>RSS feedの編集しているアイテムの番号</summary>
        private Int32 EditingNo { get; set; }

        /// <summary>DBのInsert, Updateの種別</summary>
        private enum DBCommandType
        {
            Insert = 0,
            Update,
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="page">RSS表示ページ</param>
        /// <param name="sites">サイト一覧</param>
        public RSSEditPage(IEnumerable<RssSiteInfo> sites)
        {
            InitializeComponent();

            this.FavEditBox.ItemsSource = sites;
            ChangeEditMode(EditMode.None);
        }

        /// <summary>
        /// 追加ボタン
        /// </summary>
        private void AddButton_Click(Object sender, RoutedEventArgs e)
        {
            String url = this.RssInputBox.Text;

            if (String.IsNullOrWhiteSpace(url)) {
                MessageBox.Show("Please, Input the RSS feed location.");
                return;
            }

            if (!UpdateRSS(DBCommandType.Insert, url, out Int32 id, out String title)) {
                return;
            }
            // 改めて要素を取得  リストへ追加して表示する
            var items = new List<RssSiteInfo>(GetEditItems()) {
                new RssSiteInfo() {
                    ID = id,
                    SiteName = title,
                    Link = url,
                }
            };
            this.FavEditBox.ItemsSource = items;
            this.RssInputBox.Text = "";
        }

        /// <summary>
        /// 編集ボタン
        /// </summary>
        private void EditButton_Click(Object sender, RoutedEventArgs e)
        {
            if (!(this.FavEditBox.SelectedItem is RssSiteInfo item)) { return; }

            this.RssInputBox.Text = item.Link;
            this.EditingNo = item.ID;
            this.SiteName.Text = item.SiteName;

            // 編集状態へ
            ChangeEditMode(EditMode.Editing);
        }

        /// <summary>
        /// 削除ボタン
        /// </summary>
        private void DelButton_Click(Object sender, RoutedEventArgs e)
        {
            if (!(this.FavEditBox.SelectedItem is RssSiteInfo item)) { return; }
            if (MessageBoxResult.Cancel
                == MessageBox.Show($"{item.SiteName}\r\nDelete RSS feed.", "message",
                                   MessageBoxButton.OKCancel)) {
                return;
            }

            using (var db = new SQLite(MASTER_PATH)) {
                db.Open();
                db.BeginTransaction();
                try {
                    // rss_masterからの削除
                    db.Update($"delete from rss_master where id={item.ID}");
                    // logからの削除
                    db.Update($"delete from log where master_id={item.ID}");
                    // syncからの削除
                    db.Update($"delete from sync where master_id={item.ID}");
                    // コミット
                    db.EndTransaction(true);
                }
                catch (Exception) {
                    // ロールバック
                    db.EndTransaction(false);
                }
            }
            // 表示から削除
            this.FavEditBox.ItemsSource = GetEditItems(item.ID);
        }

        /// <summary>
        /// 戻るボタン
        /// </summary>
        private void ReturnButton_Click(Object sender, RoutedEventArgs e)
        {
            // 編集中の場合は確認をとる
            if (this.EditMode == EditMode.Editing) {
                if (MessageBoxResult.Cancel == MessageBox.Show(
                                                "Editing RSS. Do you want to end it?", "Exit",
                                                MessageBoxButton.OKCancel)) {
                    return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ListBoxItem_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// DBに指定データの有無を確認する
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="url">検索するURL</param>
        /// <returns>データ有無</returns>
        private Boolean SiteExists(SQLite db, String url)
        {
            Int32 result = 1;

            var ret = db.Select($"select id, count(id) as cnt from rss_master where url='{url}'");
            result = Int32.Parse(ret["cnt"][0]);

            return result != 0;
        }

        /// <summary>
        /// サイトDBに登録
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="title">webサイト名</param>
        /// <param name="url">webサイトURL</param>
        /// <returns>DB上のマスターID</returns>
        private Int32 SiteRegist(SQLite db, String title, String url)
        {
            db.Update($"insert into rss_master(site, url) values('{title}', '{url}')");

            // 登録したデータから自動設定のID取得する
            var ret = db.Select($"select * from rss_master where url = '{url}'");
            String masterID = ret["id"][0];

            // すぐに更新できるように指定の時刻分の余裕をもってDBに登録する
            var registDate = DateTime.Now - new TimeSpan(0, INTERVAL_TIME + 1, 0);

            db.Update($"insert into sync(master_id, last_update) values({masterID}, "
                        + $"'{registDate.ToString(FeedItem.DATE_FORMAT)}')");

            return Int32.Parse(masterID);
        }

        /// <summary>
        /// リストボックスのアイテムをクラスに変換して取得
        /// </summary>
        /// <param name="removeID">削除項目のID</param>
        /// <returns>リストボックスの項目一覧</returns>
        private IEnumerable<RssSiteInfo> GetEditItems(Int32? removeID = null)
        {
            if (removeID == null) {
                // 削除指定がないのでそのまま返す
                foreach (var item in this.FavEditBox.Items) {
                    yield return item as RssSiteInfo;
                }
            }
            else {
                // 指定の項目以外を返す
                Int32 no = (Int32)removeID;
                foreach (var item in this.FavEditBox.Items) {
                    if (item is RssSiteInfo info) {
                        if (info.ID == no) { continue; }
                        yield return info;
                    }
                }
            }
        }

        /// <summary>
        /// 編集モードを変更する
        /// </summary>
        /// <param name="mode">編集モード</param>
        private void ChangeEditMode(EditMode mode)
        {
            this.EditMode = mode;
            SwitchButton(mode);
        }

        /// <summary>
        /// 編集モードに応じたボタンの切り替え
        /// </summary>
        /// <param name="mode">編集モード</param>
        private void SwitchButton(EditMode mode)
        {
            if (mode == EditMode.None) {
                this.AddButton.Visibility = Visibility.Visible;
                this.EditButton.IsEnabled = true;
                this.DelButton.IsEnabled = true;
                this.AppendButton.Visibility = Visibility.Hidden;
                this.CancelButton.Visibility = Visibility.Hidden;
            }
            else {
                this.AddButton.Visibility = Visibility.Hidden;
                this.EditButton.IsEnabled = false;
                this.DelButton.IsEnabled = false;
                this.AppendButton.Visibility = Visibility.Visible;
                this.CancelButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 編集適用ボタン
        /// </summary>
        private void AppendButton_Click(Object sender, RoutedEventArgs e)
        {
            var items = new List<RssSiteInfo>(GetEditItems());
            Int32 id = this.EditingNo;

            foreach (var item in items) {
                if (item.ID == id) {
                    // DB更新
                    if (UpdateRSS(DBCommandType.Update, this.RssInputBox.Text, out id, out _)) {
                        // アイテム欄更新
                        item.Link = this.RssInputBox.Text;
                    }
                }
            }

            this.SiteName.Text = "";
            this.RssInputBox.Text = "";
            ChangeEditMode(EditMode.None);
        }

        /// <summary>
        /// RSSのサイト登録テーブルを更新する
        /// </summary>
        /// <param name="type">DB更新種別</param>
        /// <param name="url">webサイトURL</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <param name="title">webサイト名</param>
        /// <returns>更新成功有無</returns>
        private Boolean UpdateRSS(DBCommandType type, String url,
                                    out Int32 masterID, out String title)
        {
            masterID = ERROR_RESULT;
            title = null;
            try {
                using (var db = new SQLite(MASTER_PATH)) {

                    db.Open();

                    // DB登録有無確認 
                    if (SiteExists(db, url)) {
                        MessageBox.Show("It is already registered.");
                        return false;
                    }
                    // RSSを一度取得する
                    title = RSS.ReadFeedTitle(url);
                    if (title == null) {
                        MessageBox.Show("Failed to get information.");
                        return false;
                    }

                    if (type == DBCommandType.Insert) {
                        masterID = SiteRegist(db, title, url);
                    }
                    else {
                        db.Update($"update rss_master set url='{url}' where id={masterID}");
                    }
                }
            }
            catch (Exception) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 編集キャンセルボタン
        /// </summary>
        private void CancelButton_Click(Object sender, RoutedEventArgs e)
        {
            this.SiteName.Text = "";
            this.RssInputBox.Text = "";
            ChangeEditMode(EditMode.None);
        }
    }
}