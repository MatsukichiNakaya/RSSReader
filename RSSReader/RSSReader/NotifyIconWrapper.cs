using System;
using System.ComponentModel;
using System.Windows;

namespace RSSReader
{
    public partial class NotifyIconWrapper : Component
    {
        /// <summary>
        /// 
        /// </summary>
        public NotifyIconWrapper()
        {
            InitializeComponent();

            this.ToolStripMenuClose.Click += ToolStripMenuClose_Click;
            this.ToolStripMenuOpen.Click += ToolStripMenuOpen_Click;
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
            // 現在のアプリケーションを終了
            Application.Current.Shutdown();
        }
    }
}
