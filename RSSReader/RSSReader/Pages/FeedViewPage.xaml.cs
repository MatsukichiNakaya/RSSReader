using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Project.IO;
using Project.Serialization.Xml;
using Project.Windows;
using RSSReader.Model;

namespace RSSReader.Pages
{
    /// <summary>
    /// FeedViewPage.xaml の相互作用ロジック
    /// </summary>
    public partial class FeedViewPage : Page
    {
        #region プロパティ・定数定義
        /// <summary>
        /// google chrome ブラウザパス
        /// </summary>
        /// <remarks>
        /// Todo : デフォルトブラウザとの切替
        /// </remarks>
        private String ChromePath { get; set; }
        /// <summary>
        /// Chromeのインストール先のパスが格納されているレジストリパス
        /// </summary>
        private const String ChromeRegKey =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe";

        /// <summary>データ自動更新用のタイマ</summary>
        private DispatcherTimer AutoUpdateTimer;

        /// <summary>アイテムの項目数</summary>
        public Int32 SiteItemCount { get { return this.SiteSelectBox.Items.Count; } }

        private RssConfigure Config { get; set; }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeedViewPage()
        {
            InitializeComponent();

            // RSSフィード登録サイトの読み込み
            ReLoadSiteItems();

            // Chromeのパス読み込み
            this.ChromePath = Registry.GetValue(ChromeRegKey, null) as String;

            // データ自動更新タイマの初期化
            this.AutoUpdateTimer = new DispatcherTimer(DispatcherPriority.Normal) {
                Interval = new TimeSpan(1, 0, 0),
            };
            this.AutoUpdateTimer.Tick += AutoUpdateTimer_Tick;
            this.AutoUpdateTimer.Start();

            this.Config = XmlSerializer.Load<RssConfigure>(Define.XML_PATH);
        }
        #endregion

        #region イベント
        /// <summary>
        /// Loadedイベント
        /// </summary>
        private void Page_Loaded(Object sender, RoutedEventArgs e)
        {
            // コンボボックスは最初の項目を選択する
            if (0 < this.SiteSelectBox.Items.Count)
            {
                this.SiteSelectBox.SelectedIndex = 0;
            }
        }

        private void Page_UnLoadeded(Object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// データ自動更新タイマ
        /// </summary>
        private void AutoUpdateTimer_Tick(Object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// RSSフィードのサイト選択変更
        /// </summary>
        private void SiteSelectBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cmb)
            {
                if (!(cmb.SelectedItem is RssSiteInfo item))
                { return; }
                UpdateListBox(item);

                this.FeedList.SelectedIndex = 0;
                this.FeedList.ScrollIntoView(this.FeedList.SelectedItem);

                // ソフトへ終了メッセージを送信する
                var bgw = (IntPtr)WindowInfo.FindWindowByName(null, "RssReader");
                WinMessage.Send(bgw, 32770, (IntPtr)item.ID, IntPtr.Zero);
            }
        }

        /// <summary>
        /// RSSフィードの再読み込みボタン
        /// </summary>
        private void SiteReloadButton_Click(Object sender, RoutedEventArgs e)
        {
            if (!(this.SiteSelectBox.SelectedItem is RssSiteInfo item)) { return; }

            UpdateListBox(item);
        }

        /// <summary>
        /// アプリケーション設定画面遷移ボタン
        /// </summary>
        private void SettingButton_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ConfigurePage(this));
        }

        /// <summary>
        /// RSSフィード情報編集画面遷移ボタン
        /// </summary>
        private void FabButton_Click(Object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new RSSEditPage(this, GetSiteInfo()));
        }

        /// <summary>
        /// 記事ページ選択ブラウザ起動処理
        /// </summary>
        private void ListBoxItem_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                if (item.Content is FeedItem feed)
                {
                    Process.Start(this.ChromePath, $"{this.Config?.BrowserOption ?? ""} {feed.Link}");
                }
            }
        }

        /// <summary>
        /// 選択項目をキーで起動
        /// </summary>
        private void FeedList_PreviewKeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) { return; }

            if (sender is ListBox box)
            {
                if (box.SelectedValue is FeedItem item)
                {
                    Process.Start(this.ChromePath, $"{this.Config?.BrowserOption ?? ""} {item.Link}");
                }
            }
        }
        #endregion
    }
}
