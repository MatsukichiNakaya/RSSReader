using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Project.DataBase;
using Project.Serialization.Xml;
using RSSReader.Model;

namespace RSSReader.Pages
{
    /// <summary>
    /// ConfigurePage.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigurePage : Page
    {
        private FeedViewPage InnerViewPage { get; set; }

        public ConfigurePage(FeedViewPage inner)
        {
            InitializeComponent();

            var conf = XmlSerializer.Load<RssConfigure>(Define.XML_PATH);
            this.ConfGrid.DataContext = conf;


            this.InnerViewPage = inner;
        }

        private void ChashDelButton_Click(Object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Cancel 
                == MessageBox.Show("Delete unnecessary files?")) { return; }

        }

        private void ReturnButton_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(this.InnerViewPage);
        }

        private void AppendButton_Click(Object sender, RoutedEventArgs e)
        {
            if (!Int32.TryParse(this.UpdateBox.Text, out Int32 span)) { return; }
            if (span < 5) { return; }

            var conf = new RssConfigure() {
                BrowserOption = this.BrowsBox.Text,
                UpdateSpan = span,
                IsShowImage = false,
            };
            XmlSerializer.Save(conf, Define.XML_PATH);
        }

        /// <summary>
        /// キャッシュしている画像の削除
        /// </summary>
        private void DeleteButton_Click(Object sender, RoutedEventArgs e)
        {
            using (var db = new SQLite(Define.MASTER_PATH))
            {
                db.Open();
                var masterIDs = db.Select("select id from rss_master")["id"];

                foreach (var id in masterIDs)
                {
                    // URLからファイル名だけを抜き出したリストにする
                    var thumbs = new HashSet<String>(
                        db.Select($"select thumb_url from log where master_id={id}")["thumb_url"]
                        .Select(p => System.IO.Path.GetFileName(p)));
                    // 個別のディレクトリからファイル名を取得
                    var files = System.IO.Directory.GetFiles($@"{FeedItem.CHASH_DIR}\{id}", "*",
                        System.IO.SearchOption.TopDirectoryOnly);

                    foreach (var f in files)
                    {
                        // DBへの登録なし
                        if (!thumbs.Contains(System.IO.Path.GetFileName(f)))
                        {
                            System.IO.File.Delete(f);
                        }
                    }
                }
            }
        }
    }
}
