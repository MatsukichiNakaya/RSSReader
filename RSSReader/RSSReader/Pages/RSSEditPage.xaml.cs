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
        private const String MASTER_PATH = @".\rss_log.db";

        private FeedViewPage InnerViewPage { get; set; }

        public RSSEditPage(FeedViewPage page, IEnumerable<RssSiteInfo> sites)
        {
            InitializeComponent();
            this.InnerViewPage = page;
            this.FavEditBox.ItemsSource = sites;
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

            using (var db = new SQLite(MASTER_PATH))
            {
                db.Open();

                // DB登録有無確認 
                if (SiteExists(db, url))
                {
                    MessageBox.Show("It is already registered.");
                    return;
                }
                // RSSを一度取得する
                String title = RSS.ReadFeedTitle(url);
                if (title == null)
                {
                    MessageBox.Show("Failed to get information.");
                    return;
                }
                // DBに登録
                SiteRegist(db, title, url);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void EditButton_Click(Object sender, RoutedEventArgs e)
        {

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
        private void SiteRegist(SQLite db, String title, String url)
        {
            db.Update($"insert into rss_master(site, url) values('{title}', '{url}')");
            
            // 登録したデータから自動設定のID取得する
            var ret = db.Select($"select * from rss_master where url = '{url}'");
            String masterID = ret["id"][0];

            // すぐに更新できるように指定の時刻分の余裕をもってDBに登録する
            db.Update($"insert into sync(master_id, last_update) values({masterID}, "
                        + $"'{(DateTime.Now - (new TimeSpan(0, 5, 0))).ToString(FeedItem.DATE_FORMAT)}')");
        }
    }
}
