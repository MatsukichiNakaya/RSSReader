using Project.IO;
using RSSReader.Pages;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using static RSSReader.Define;

namespace RSSReader
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // 現在表示しているRSSのページ番号
        private Int32 Page { get; set; }

        private const Double PROP_OPACITY = 0.2;

        

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(DAT_DIR)) {
                Directory.CreateDirectory(DAT_DIR);
            }

            // DBファイルが無ければ作成する。
            if (!File.Exists(MASTER_PATH)) {
                CommFunc.CreateDB();
            }

            // 初期設定
            this.Page = -1;
            App.Configure = CommFunc.ConfigLoad();
            ButtonDeactivate(this.ListButton);

            //var fontfamily = new FontFamily("游ゴシック");
            //var style = new Style(typeof(Window));
            //style.Setters.Add(new Setter(Window.FontFamilyProperty, fontfamily));
            //FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window),
            //                                new FrameworkPropertyMetadata(style));

           
        }

        /// <summary>
        /// メッセージ受信
        /// </summary>
        /// <param name="hwnd">ウインドウハンドル</param>
        /// <param name="msg">メッセージ</param>
        /// <param name="wParam">パラメータ</param>
        /// <param name="lParam">パラメータ</param>
        /// <param name="handled"><ハンドル/param>
        /// <returns>可否</returns>
        private IntPtr WndProc(IntPtr hwnd, Int32 msg,
                                IntPtr wParam, IntPtr lParam, ref Boolean handled)
        {
            switch (msg) {
                case Define.CHANGE_MESSAGE:
                    // 現在のページを保持
                    this.Page = wParam.ToInt32();
                    break;
                case Define.Window_MIN_MESSAGE:
                    // 最小化
                    this.WindowState = System.Windows.WindowState.Minimized;
                    break;
                case Define.BROWSING_URL_MESSAGE:
                    NavigateBrowserPage();
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 読み込み完了イベント
        /// </summary>
        private void NavigationWindow_Loaded(Object sender, System.Windows.RoutedEventArgs e)
        {
            // メッセージ受信イベントを自身に追加する
            var src = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            src.AddHook(new HwndSourceHook(this.WndProc));
        }

        /// <summary>
        /// 終了中イベント
        /// </summary>
        private void NavigationWindow_Closing(Object sender, CancelEventArgs e)
        {
            if(this.Page < 0) { return; }
            // 表示するページを保持するために終了前に状態を保存する
            TextFile.Write(Define.PAGE_DAT, $"{this.Page}", TextFile.OVER_WRITE);
        }

        /// <summary>
        /// ドラッグ＆ドロップによるウインドウの移動
        /// </summary>
        private void Window_MouseLeftButtonDown(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState != MouseButtonState.Pressed) { return; }
            this.DragMove();
        }

        /// <summary>
        /// ウインドウ最小化
        /// </summary>
        private void MinimumButton_Click(Object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            //this.Close();
        }

        /// <summary>
        /// ウインドウ最大化
        /// </summary>
        private void MaximumButton_Click(Object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// ウインドウを閉じる
        /// </summary>
        private void CloseButton_Click(Object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 設定画面へ
        /// </summary>
        private void SettingButton_Click(Object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(new ConfigurePage());
            ButtonStateClear();
            ButtonDeactivate(this.SettingButton);
        }

        /// <summary>
        /// RSSフィード編集画面へ
        /// </summary>
        private void FabButton_Click(Object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(new RSSEditPage(CommFunc.GetSiteInfo()));
            ButtonStateClear();
            ButtonDeactivate(this.FabButton);
        }

        /// <summary>
        /// リスト画面へ
        /// </summary>
        private void ListButton_Click(Object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(new FeedViewPage(this.Page));
            ButtonStateClear();
            ButtonDeactivate(this.ListButton);
        }

        /// <summary>
        /// ピックアップ画面へ
        /// </summary>
        private void PickupButton_Click(Object sender, RoutedEventArgs e)
        {
            this.MainFrame.Navigate(new PickupPage());
            ButtonStateClear();
            ButtonDeactivate(this.PickupButton);
        }

        /// <summary>
        /// クラウド画面へ
        /// </summary>
        private void CloudButton_Click(Object sender, RoutedEventArgs e)
        {
            //this.MainFrame.Navigate(new DriveManagePage());
            //ButtonStateClear();
            //ButtonDeactivate(this.CloudButton);
        }

        private void NavigateBrowserPage()
        {
            this.MainFrame.Navigate(new BrowsingPage());
            ButtonStateClear();
        }

        /// <summary>
        /// ボタンの非活性化
        /// </summary>
        /// <param name="btn">対象のボタン</param>
        private void ButtonDeactivate(Button btn)
        {
            btn.IsEnabled = false;
            btn.Opacity = PROP_OPACITY;
        }

        /// <summary>
        /// 無効化解除
        /// </summary>
        private void ButtonStateClear()
        {
            this.ListButton.IsEnabled = true;
            this.ListButton.Opacity = 1;
            this.FabButton.IsEnabled = true;
            this.FabButton.Opacity = 1;
            this.SettingButton.IsEnabled = true;
            this.SettingButton.Opacity = 1;
            this.PickupButton.IsEnabled = true;
            this.PickupButton.Opacity = 1;
            this.CloudButton.IsEnabled = true;
            this.CloudButton.Opacity = 1;
        }
    }
}
