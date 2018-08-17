using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project.DataBase;
using RSSReader.Model;

namespace RSSReader.Pages
{
    /// <summary>
    /// RSSEditPage.xaml の相互作用ロジック
    /// </summary>
    public partial class RSSEditPage : Page
    {
        private FeedViewPage InnerViewPage { get; set; }

        private Define.EditMode EditMode { get; set; }
        private Int32 EditingNo { get; set; }

        private enum DBCommandType
        {
            Insert = 0,
            Update,
        }

        public RSSEditPage(FeedViewPage page, IEnumerable<RssSiteInfo> sites)
        {
            InitializeComponent();
            this.InnerViewPage = page;
            this.FavEditBox.ItemsSource = sites;
            ChangeEditMode(Define.EditMode.None);
        }

        /// <summary>
        /// 追加ボタン
        /// </summary>
        private void AddButton_Click(Object sender, RoutedEventArgs e)
        {
            String url = this.RssInputBox.Text;

            if (String.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Please, Input the RSS feed location.");
                return;
            }

            if (!UpdateRSS(DBCommandType.Insert, url, out Int32 id, out String title))
            {
                return;
            }
            // 改めて要素を取得
            var items = new List<RssSiteInfo>(GetEditItems());
            // リストへ追加して表示する
            items.Add(new RssSiteInfo() {
                ID = id,
                SiteName = title,
                Link = url,
            });
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

            ChangeEditMode(Define.EditMode.Editing);
        }

        /// <summary>
        /// 削除ボタン
        /// </summary>
        private void DelButton_Click(Object sender, RoutedEventArgs e)
        {
            if (!(this.FavEditBox.SelectedItem is RssSiteInfo item)) { return; }
            
            using (var db = new SQLite(Define.MASTER_PATH))
            {
                db.Open();
                db.BeginTransaction();
                try
                {
                    // rss_masterからの削除
                    db.Update($"delete from rss_master where id={item.ID}");
                    // logからの削除
                    db.Update($"delete from log where master_id={item.ID}");
                    // syncからの削除
                    db.Update($"delete from sync where master_id={item.ID}");

                    // コミット
                    db.EndTransaction(true);
                }
                catch (Exception)
                {
                    // ロールバック
                    db.EndTransaction(false);
                }
            } //*/

            // 表示から削除
            this.FavEditBox.ItemsSource = GetEditItems(item.ID);
        }

        /// <summary>
        /// 戻るボタン
        /// </summary>
        private void ReturnButton_Click(Object sender, RoutedEventArgs e)
        {
            if (this.EditMode == Define.EditMode.Editing)
            {
                if (MessageBoxResult.OK == MessageBox.Show("Editing RSS. Do you want to end it?", "Exit",
                                                            MessageBoxButton.OKCancel))
                {
                    this.InnerViewPage.ReLoadSiteItems();
                    this.NavigationService.Navigate(this.InnerViewPage);
                }
            }
            else
            {
                this.InnerViewPage.ReLoadSiteItems();
                this.NavigationService.Navigate(this.InnerViewPage);
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
        /// <param name="db"></param>
        /// <param name="url"></param>
        /// <returns></returns>
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
        private Int32 SiteRegist(SQLite db, String title, String url)
        {
            db.Update($"insert into rss_master(site, url) values('{title}', '{url}')");
            
            // 登録したデータから自動設定のID取得する
            var ret = db.Select($"select * from rss_master where url = '{url}'");
            String masterID = ret["id"][0];

            // すぐに更新できるように指定の時刻分の余裕をもってDBに登録する
            db.Update($"insert into sync(master_id, last_update) values({masterID}, "
                        + $"'{(DateTime.Now - (new TimeSpan(0, 5, 0))).ToString(FeedItem.DATE_FORMAT)}')");

            return Int32.Parse(masterID);
        }

        /// <summary>
        /// リストボックスのアイテムをクラスに変換して取得
        /// </summary>
        /// <returns></returns>
        private IEnumerable<RssSiteInfo> GetEditItems(Int32? removeNo = null)
        {
            if (removeNo == null)
            {
                foreach (var item in this.FavEditBox.Items)
                {
                    yield return item as RssSiteInfo;
                }
            }
            else
            {
                Int32 no = (Int32)removeNo;
                foreach (var item in this.FavEditBox.Items)
                {
                    if (item is RssSiteInfo info)
                    {
                        if(info.ID == no) { continue; }
                        yield return info;
                    }
                }
            }
        }

        /// <summary>
        /// 編集モードを変更する
        /// </summary>
        /// <param name="mode"></param>
        private void ChangeEditMode(Define.EditMode mode)
        {
            this.EditMode = mode;
            SwitchButton(mode);
        }

        /// <summary>
        /// 編集モードに応じたボタンの切り替え
        /// </summary>
        /// <param name="mode"></param>
        private void SwitchButton(Define.EditMode mode)
        {
            if (mode == Define.EditMode.None)
            {
                this.AddButton.IsEnabled = true;
                this.EditButton.IsEnabled = true;
                this.DelButton.IsEnabled = true;
                this.AppendButton.Visibility = Visibility.Hidden;
                this.CancelButton.Visibility = Visibility.Hidden;
            }
            else
            {
                this.AddButton.IsEnabled = false;
                this.EditButton.IsEnabled = false;
                this.DelButton.IsEnabled = false;
                this.AppendButton.Visibility = Visibility.Visible;
                this.CancelButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 編集適用ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppendButton_Click(Object sender, RoutedEventArgs e)
        {
            var items = new List<RssSiteInfo>(GetEditItems());
            Int32 id = this.EditingNo;

            foreach (var item in items)
            {
                if (item.ID == id)
                {                    
                    // DB更新
                    if (UpdateRSS(DBCommandType.Update, this.RssInputBox.Text, out id, out _))
                    {
                        // アイテム欄更新
                        item.Link = this.RssInputBox.Text;
                    }
                }
            }
            
            this.SiteName.Text = "";
            this.RssInputBox.Text = "";
            ChangeEditMode(Define.EditMode.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="masterID"></param>
        /// <param name="url"></param>
        private Boolean UpdateRSS(DBCommandType type, String url, out Int32 masterID, out String title)
        {
            masterID = -1;
            title = null;
            using (var db = new SQLite(Define.MASTER_PATH))
            {
                db.Open();

                // DB登録有無確認 
                if (SiteExists(db, url))
                {
                    MessageBox.Show("It is already registered.");
                    return false;
                }
                // RSSを一度取得する
                title = RSS.ReadFeedTitle(url);
                if (title == null)
                {
                    MessageBox.Show("Failed to get information.");
                    return false;
                }

                if (type == DBCommandType.Insert)
                {
                    masterID = SiteRegist(db, title, url);
                }
                else
                {
                    db.Update($"update rss_master set url='{url}' where id={masterID}");
                }
            }
            return true;
        }

        /// <summary>
        /// 編集キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(Object sender, RoutedEventArgs e)
        {
            this.SiteName.Text = "";
            this.RssInputBox.Text = "";
            ChangeEditMode(Define.EditMode.None);
        }
    }
}
