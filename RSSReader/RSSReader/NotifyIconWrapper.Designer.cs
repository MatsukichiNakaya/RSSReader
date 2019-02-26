namespace RSSReader
{
    partial class NotifyIconWrapper
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotifyIconWrapper));
            this.TaskTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.TaskTrayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuClose = new System.Windows.Forms.ToolStripMenuItem();
            this.TaskTrayMenu.SuspendLayout();
            // 
            // TaskTrayIcon
            // 
            this.TaskTrayIcon.ContextMenuStrip = this.TaskTrayMenu;
            this.TaskTrayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("TaskTrayIcon.Icon")));
            this.TaskTrayIcon.Text = "RSS Reader";
            this.TaskTrayIcon.Visible = true;
            // 
            // TaskTrayMenu
            // 
            this.TaskTrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuOpen,
            this.ToolStripMenuClose});
            this.TaskTrayMenu.Name = "TaskTrayMenu";
            this.TaskTrayMenu.Size = new System.Drawing.Size(104, 48);
            // 
            // ToolStripMenuOpen
            // 
            this.ToolStripMenuOpen.Name = "ToolStripMenuOpen";
            this.ToolStripMenuOpen.Size = new System.Drawing.Size(103, 22);
            this.ToolStripMenuOpen.Text = "Open";
            // 
            // ToolStripMenuClose
            // 
            this.ToolStripMenuClose.Name = "ToolStripMenuClose";
            this.ToolStripMenuClose.Size = new System.Drawing.Size(103, 22);
            this.ToolStripMenuClose.Text = "Close";
            this.TaskTrayMenu.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon TaskTrayIcon;
        private System.Windows.Forms.ContextMenuStrip TaskTrayMenu;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuOpen;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuClose;
    }
}
