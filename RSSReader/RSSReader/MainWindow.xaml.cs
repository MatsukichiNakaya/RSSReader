using System.Windows.Navigation;
using System.ComponentModel;
using System;
using System.Windows.Interop;
using Project.IO;

namespace RSSReader
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        // 現在表示しているRSSのページ番号
        private Int32 Page { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
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
            // 表示するページを保持するために終了前に状態を保存する
            TextFile.Write(Define.PAGE_DAT, $"{this.Page}", TextFile.OVER_WRITE);
        }


    }
}
