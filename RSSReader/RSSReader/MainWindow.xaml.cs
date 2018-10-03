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
        private readonly UInt32 CHANGE_MESSAGE = 32770;
        private Int32 Page { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>メッセージ受信</summary>
        private IntPtr WndProc(IntPtr hwnd, Int32 msg,
                                IntPtr wParam, IntPtr lParam, ref Boolean handled)
        {
            if (msg == CHANGE_MESSAGE)
            {
                // 現在のページを保持
                this.Page = wParam.ToInt32();   
            }
            return IntPtr.Zero;
        }

        private void NavigationWindow_Loaded(Object sender, System.Windows.RoutedEventArgs e)
        {
            // メッセージ受信イベントを自身に追加する
            var src = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            src.AddHook(new HwndSourceHook(this.WndProc));
        }

        private void NavigationWindow_Closing(Object sender, CancelEventArgs e)
        {
            TextFile.Write(@".\page.dat", $"{this.Page}", TextFile.OVER_WRITE);
        }


    }
}
