using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Project.DataBase;
using Project.Serialization.Xml;
using RSSReader.Model;
using static RSSReader.Define;

namespace RSSReader.Pages
{
    /// <summary>
    /// ConfigurePage.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigurePage : Page
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="inner"></param>
        public ConfigurePage()
        {
            InitializeComponent();

            // インデックスにバインドするため先に項目を設定する
            this.HAnchorBox.ItemsSource = Enum.GetNames(typeof(HorizontalAlignment));
            this.VAnchorBox.ItemsSource = Enum.GetNames(typeof(VerticalAlignment));
            this.StretchBox.ItemsSource = Enum.GetNames(typeof(Stretch));

            try {
                this.ConfGrid.DataContext = App.Configure;

                var list = (from brush in typeof(Brushes).GetProperties() select (Brush)brush.GetValue(null, null)).ToList();
                list.Sort(delegate (Brush source, Brush target) { return source.ToString().CompareTo(target.ToString()); });

                this.MainPicker.DataContext = list;
            }
            catch (Exception) {
            }
        }

        /// <summary>
        /// 位置設定の情報に適切な値が設定されているか
        /// </summary>
        /// <returns>true:適切, false:不適切</returns>
        public Boolean IsPositionProper()
        {
            foreach (var elem in this.PosGrid.Children) {
                if (elem is ComboBox cmb) {
                    if(cmb.SelectedIndex < 0) {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 設定の保存ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppendButton_Click(Object sender, RoutedEventArgs e)
        {
            try {
                var conf = this.ConfGrid.DataContext as RssConfigure;

                // 内部で定義している間隔以下は設定できない。
                if (conf.UpdateSpan < INTERVAL_TIME) {
                    conf.UpdateSpan = INTERVAL_TIME;
                }
                // 設定が適切でないと保存しない
                if(!IsPositionProper()) {
                    MessageBox.Show("Position setting is not proper.",
                        "Error", MessageBoxButton.OK);
                    return;
                }
                
                XmlSerializer.Save(conf, XML_PATH);
                App.Configure = conf;
            }
            catch (Exception) {
                MessageBox.Show("setting error!", "error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// キャッシュしている画像の削除
        /// </summary>
        private void DeleteButton_Click(Object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Cancel
                 == MessageBox.Show("Delete unnecessary files?", "message",
                                    MessageBoxButton.OKCancel)) {
                return;
            }

            if (!File.Exists(MASTER_PATH)) {
                MessageBox.Show("DB file not found", "error", MessageBoxButton.OK);
                return;
            }

            // DBの最適化
            DBMaintenance();
            
            // 画像ファイル削除
            CashFileDelete();
        }

        /// <summary>
        /// 不要になった画像ファイルを削除する。
        /// </summary>
        private void CashFileDelete()
        {
            try {
                using (var db = new SQLite(MASTER_PATH)) {

                    db.Open();
                    var masterIDs = db.Select("select id from rss_master")["id"];

                    foreach (var id in masterIDs) {
                        // URLからファイル名だけを抜き出したリストにする
                        var thumbs = new HashSet<String>(
                            db.Select($"select thumb_url from log where master_id={id}")["thumb_url"]
                            .Select(p => System.IO.Path.GetFileName(p)));
                        // 個別のディレクトリからファイル名を取得
                        var files = System.IO.Directory.GetFiles($@"{FeedItem.CHASH_DIR}\{id}", "*",
                            System.IO.SearchOption.TopDirectoryOnly);

                        foreach (var f in files) {
                            // DBへの登録なしのサムネを削除する
                            if (!thumbs.Contains(System.IO.Path.GetFileName(f))) {
                                System.IO.File.Delete(f);
                            }
                        }
                    }
                }
            }
            catch (Exception) {
            }
        }

        /// <summary>
        /// DBファイルのテンポラリ領域の解放
        /// </summary>
        private void DBMaintenance()
        {
            try {
                using (var db = new SQLite(MASTER_PATH)) {
                    db.Open();
                    db.Update("VACUUM");
                }
            }
            catch (Exception) {
            }
        }
    }
}
