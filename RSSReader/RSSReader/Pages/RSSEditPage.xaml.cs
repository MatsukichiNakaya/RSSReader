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
using RSSReader.Model;
using Project.DataBase;

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

        public RSSEditPage(FeedViewPage page, IEnumerable<RssSiteInfo> sites)
        {
            InitializeComponent();
            this.InnerViewPage = page;
            this.FavEditBox.ItemsSource = sites;
            ChangeEditMode(Define.EditMode.None);
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddButton_Click(Object sender, RoutedEventArgs e)
        {
            String url = this.RssInputBox.Text;

            if (String.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Please, Input the RSS feed location.");
                return;
            }

            Int32 id;
            String title;

            using (var db = new SQLite(Define.MASTER_PATH))
            {
                db.Open();

                // DB登録有無確認 
                if (SiteExists(db, url))
                {
                    MessageBox.Show("It is already registered.");
                    return;
                }
                // RSSを一度取得する
                title = RSS.ReadFeedTitle(url);
                if (title == null)
                {
                    MessageBox.Show("Failed to get information.");
                    return;
                }
                // DBに登録
                id = SiteRegist(db, title, url);
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
        /// 
        /// </summary>
        private void EditButton_Click(Object sender, RoutedEventArgs e)
        {
            if (!(this.FavEditBox.SelectedItem is RssSiteInfo item)) { return; }

            this.RssInputBox.Text = item.Link;
            this.EditingNo = item.ID;

            ChangeEditMode(Define.EditMode.Editing);
        }

        /// <summary>
        /// 
        /// </summary>
        private void DelButton_Click(Object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        private void ReturnButton_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(this.InnerViewPage);
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
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerable<RssSiteInfo> GetEditItems()
        {
            foreach (var item in this.FavEditBox.Items)
            {
                yield return item as RssSiteInfo;
            } 
        }

        private void ChangeEditMode(Define.EditMode mode)
        {
            this.EditMode = mode;
            SwitchButton(mode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        private void SwitchButton(Define.EditMode mode)
        {
            if (mode == Define.EditMode.None)
            {
                this.AddButton.IsEnabled = true;
                this.EditButton.IsEnabled = true;
                this.DelButton.IsEnabled = true;
            }
            else
            {
                this.AddButton.IsEnabled = false;
                this.EditButton.IsEnabled = false;
                this.DelButton.IsEnabled = false;
            }
        }
    }
}
