//#define HOTKEY

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;


namespace RSSReader
{
    public partial class NotifyIconWrapper : Component
    {
#if HOTKEY
        private HotKeyHelper _hotkey;
#endif
        /// <summary>
        /// 
        /// </summary>
        public NotifyIconWrapper()
        {
            InitializeComponent();

            this.ToolStripMenuClose.Click += ToolStripMenuClose_Click;
            this.ToolStripMenuOpen.Click += ToolStripMenuOpen_Click;
#if HOTKEY
            // HotKeyの登録
            this._hotkey = new HotKeyHelper(this);
            this._hotkey.Register(ModifierKeys.Control | ModifierKeys.Shift,
                                  Key.X,
                                  (_, __) => { MessageBox.Show("HotKey"); });
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        public NotifyIconWrapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuOpen_Click(Object sender, EventArgs e)
        {
            // MainWindow を生成、表示
            var wnd = new MainWindow();
            wnd.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuClose_Click(Object sender, EventArgs e)
        {
#if HOTKEY
            if (this._hotkey != null) {
                // HotKeyの登録解除
                this._hotkey.Dispose();
            }
#endif
            // 現在のアプリケーションを終了
            Application.Current.Shutdown();
        }
    }
}
