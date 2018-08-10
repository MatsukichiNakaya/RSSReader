using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using Project.IO;
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
                if (!(cmb.SelectedItem is RssSiteInfo item)) { return; }
                UpdateListBox(item);
            }
        }

        /// <summary>
        /// RSSフィードの再読み込みボタン
        /// </summary>
        private void SiteReloadButton_Click(Object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// アプリケーション設定画面遷移ボタン
        /// </summary>
        private void SettingButton_Click(Object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// RSSフィード情報編集画面遷移ボタン
        /// </summary>
        private void FabButton_Click(Object sender, RoutedEventArgs e)
        {

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
                    // Todo: 個人的な設定 設定画面で変更可能にする
                    String option = "--profile-directory=\"Profile 1\" --incognito";
                    Process.Start(this.ChromePath, $"{option} {feed.Link}");
                }
            }
        }
        #endregion

    }
}
