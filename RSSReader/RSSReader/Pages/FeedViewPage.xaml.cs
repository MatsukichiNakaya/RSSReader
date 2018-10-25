﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Project.IO;
using Project.Windows;
using RSSReader.Model;

using static RSSReader.Define;

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

        /// <summary>動作設定</summary>
        private RssConfigure Config { get; set; }
        #endregion

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeedViewPage()
        {
            InitializeComponent();

            // DBファイルが無ければ作成する。
            if (!File.Exists(MASTER_PATH)) {
                CommFunc.CreateDB();
            }

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
            this.Config = CommFunc.ConfigLoad();

            // コンボボックスは最初の項目を選択する
            if (0 < this.SiteSelectBox.Items.Count) {
                // 前回のページ保持オプション
                if (this.Config.IsKeepPage) {
                    if (Int32.TryParse(TextFile.Read(PAGE_DAT), out Int32 id)) {
                        Int32 page = GetIndexFromMasterID(id);
                        if (0 <= page && page < this.SiteSelectBox.Items.Count) {
                            // 正常にデータが読めたら値を設定。
                            this.SiteSelectBox.SelectedIndex = page;
                            return;
                        }
                    }
                }
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
            if (sender is ComboBox cmb) {
                if (!(cmb.SelectedItem is RssSiteInfo item)) { return; }
                UpdateListBox(item);

                this.FeedList.SelectedIndex = 0;
                this.FeedList.ScrollIntoView(this.FeedList.SelectedItem);

                // メインウインドウにサイト変更メッセージを送信する
                var bgw = WindowInfo.FindWindowByName(null, TITLE);
                WinMessage.Send(bgw, CHANGE_MESSAGE, (IntPtr)item.ID, IntPtr.Zero);
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
            Int32 index = (this.SiteSelectBox.SelectedItem as RssSiteInfo)?.ID ?? ERROR_RESULT;
            if (index < 0) { return; }

            TextFile.Write(PAGE_DAT, $"{index}", TextFile.OVER_WRITE);
            this.NavigationService.Navigate(new ConfigurePage(this));
        }

        /// <summary>
        /// RSSフィード情報編集画面遷移ボタン
        /// </summary>
        private void FabButton_Click(Object sender, RoutedEventArgs e)
        {
            Int32 index = (this.SiteSelectBox.SelectedItem as RssSiteInfo)?.ID ?? ERROR_RESULT;
            if (index < 0) { return; }

            TextFile.Write(PAGE_DAT, $"{index}", TextFile.OVER_WRITE);
            this.NavigationService.Navigate(new RSSEditPage(this, GetSiteInfo()));
        }

        /// <summary>
        /// 記事ページ ブラウザ起動処理 ダブルクリック
        /// </summary>
        private void ListBoxItem_MouseDoubleClick(Object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item) {
                if (item.Content is FeedItem feed) {
                    StartBrowser(feed);
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
                    StartBrowser(feed);
                }
            }
        }
        #endregion
    }
}