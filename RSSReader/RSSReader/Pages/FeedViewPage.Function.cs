using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Project.DataBase;
using Project.Windows;
using RSSReader.Model;

using static RSSReader.Define;

namespace RSSReader.Pages
{
    public partial class FeedViewPage : Page
    {
        #region ComboBox
        /// <summary>
        /// コンボボックスの中身を設定する
        /// </summary>
        private void ReLoadSiteItems()
        {
            this.SiteSelectBox.ItemsSource = CommFunc.GetSiteInfo();
        }

        /// <summary>
        /// マスターIDからコンボボックス上でのインデックスを取得する
        /// </summary>
        /// <param name="masterID">DB上のマスターID</param>
        /// <returns>コンボボックス上のインデックス</returns>
        private Int32 GetIndexFromMasterID(Int32 masterID)
        {
            for (Int32 index = 0; index < this.SiteSelectBox.Items.Count; index++) {
                if (this.SiteSelectBox.Items[index] is RssSiteInfo site) {
                    if (site.ID == masterID) {
                        return index;
                    }
                }
            }
            return ERROR_RESULT;
        }
        #endregion

        #region ListBox
        /// <summary>
        /// RSS フィードリストの更新を行う
        /// </summary>
        /// <param name="item">サイト情報</param>
        /// <param name="isListUpdate">ListBoxの表示を更新するか</param>
        private void UpdateListBox(RssSiteInfo item, Boolean isListUpdate)
        {
            String url = item?.Link;
            if (url == null) { return; }

            Int32 masterID = item.ID;
            IEnumerable<FeedItem> feedItems = null;

            if(!IsOnline()) {
                // インターネット接続が無いため、強制的にオフラインモードに設定
                App.Configure.IsOffLine = true;
            }

            using (var db = new SQLite(MASTER_PATH)) {

                db.Open();

                // 更新間隔の確認とOffLineモードオプションを確認する
                if (CanRSSRead(db, masterID) && !(App.Configure?.IsOffLine ?? false)) {
                    // フィードデータダウンロード
                    feedItems = RSS.ReadFeedItems(url);
                    // ダウンロード時刻アップデート
                    UpdateLastSync(db, masterID);
                }

                // リスト・DBの更新
                var items = GetFeedItems(db, feedItems, masterID, isListUpdate);
                if (isListUpdate) {
                    this.FeedList.ItemsSource = items;
                }
            }
        }

        /// <summary>
        /// 更新間隔チェック
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <returns>取得間隔経過判定</returns>
        private Boolean CanRSSRead(SQLite db, Int32 masterID)
        {
            Boolean result = false;

            // 最終更新日時を取得
            var ret = db.Select($"select last_update from sync where master_id = {masterID}");
            var last = DateTime.Parse(ret["last_update"][0]);

            // 更新間隔設定値を超えているか？
            result = INTERVAL_TIME <= (DateTime.Now - last).Minutes;

            return result;
        }

        /// <summary>
        /// 読み込み日時更新
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="masterID">DB上のマスターID</param>
        private void UpdateLastSync(SQLite db, Int32 masterID)
        {
            db.Update(
                $"update sync set last_update='{DateTime.Now.ToString(FeedItem.DATE_FORMAT)}'" +
                $" where master_id={masterID}");
        }

        /// <summary>
        /// リストボックスに割り当てる項目をDBからも取得して設定する
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="feedItems">feed項目一覧</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <param name="isListUpdate">ListBoxの表示を更新するか</param>
        private IEnumerable<FeedItem> GetFeedItems(SQLite db, IEnumerable<FeedItem> feedItems,
                                    Int32 masterID, Boolean isListUpdate)
        {
            FeedItem.ExistsChashDirectory(masterID.ToString());

            // 更新日時の最新で並べ替える
            var items = GetFeedItemsToDB(db, feedItems, masterID)
                            .OrderByDescending(fd => fd.PublishDate);
            // リストを更新しないのでサムネイル画像は読み込まない。
            if(!isListUpdate) { return items; }

            if (App.Configure?.IsShowImage ?? false) {
                // サムネの読み込み
                foreach (var item in items) {
                    item.Thumbnail = CommFunc.GetImage(item.ThumbUri, masterID, item.Host);
                    if (item.ThumbUri != null) {
                        item.ThumbWidth = DEFAULT_PIC_WIDTH;
                    }
                }
            }
            else {
                // サムネ表示無効なので幅を調整する
                foreach (var item in items) {
                    item.ThumbWidth = 0;
                }
            }
            //this.FeedList.ItemsSource = items;
            return items;
        }

#if false
        /// <summary>
        /// サムネ画像をダウンロード、または、キャッシュから読み込む
        /// </summary>
        /// <param name="url">サムネイルのUrl</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <param name="host">webサイトのホスト名</param>
        /// <returns>画像データ</returns>
        private ImageSource GetImage(Uri url, Int32 masterID, String host)
        {
            String localPath = FeedItem.GetChashPath(url?.AbsoluteUri, masterID, host);

            if (File.Exists(localPath)) {   
                // chashから読み込み
                return FeedItem.ReadChashThumb(localPath);
            }
            else {   
                // ダウンロード
                return FeedItem.DownloadThumb(url?.AbsoluteUri, masterID, host);
            }
        }
#endif

        /// <summary>
        /// webからのデータとDBのログ情報をマージして返す。
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="feedItems">feed項目一覧</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <returns>feed項目一覧</returns>
        private IEnumerable<FeedItem> GetFeedItemsToDB(SQLite db,
                                                IEnumerable<FeedItem> feedItems, Int32 masterID)
        {
            String sql = $"select * from log where master_id = {masterID}";

            // RSS記事のページURLをもとに新規項目を取得する
            var registeredItem = CommFunc.GetLogItems(db, sql).ToList();
            var urlHash = new HashSet<String>(registeredItem.Select(l => l.Link.AbsoluteUri));
            var newItems = GetNewcomer(feedItems, urlHash);
            Boolean isCommit = false;

            db.BeginTransaction();
            try {
                // 新規項目をDBに登録
                LogUpdate(db, newItems, masterID);
                isCommit = true;
            }
            catch (Exception) {
                isCommit = false;
            }
            finally {
                db.EndTransaction(isCommit);
            }
            // DBに登録したので改めて取得する。
            // ※ToArray()が無いと遅延評価の影響でサムネ読み込みに影響があるので注意
            //return GetLogItems(db, masterID).ToArray();
            return CommFunc.GetLogItems(db, sql).ToArray();
        }

#if false
        /// <summary>
        /// DBからFeedItemを取得する
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <returns>feed項目一覧</returns>
        private IEnumerable<FeedItem> GetLogItems(SQLite db, Int32 masterID)
        {
            var ret = db.Select($"select * from log where master_id = {masterID}");

            if (0 < ret.Count) {
                Int32 count = ret["log_id"].Count;
                for (Int32 i = 0; i < count; i++) {
                    yield return new FeedItem() {
                        ID          = ret["log_id"][i],
                        Title       = ret["title"][i],
                        PublishDate = ret["reg_date"][i],
                        Summary     = ret["summary"][i]?.Replace("'", ""),
                        Link        = new Uri(ret["page_url"][i]),
                        IsRead      = Int32.Parse(ret["is_read"][i]) == 1,
                        ThumbUri    = String.IsNullOrEmpty(ret["thumb_url"][i])
                                        ? null : new Uri(ret["thumb_url"][i]),
                    };
                }
            }
        }
#endif

        /// <summary>
        /// ログから取得したURLとかぶらないfeed項目を選別する
        /// </summary>
        /// <param name="feedItems">feed項目一覧</param>
        /// <param name="hash">DB登録済みのURLの一覧</param>
        /// <returns>新規取得のfeed項目</returns>
        private IEnumerable<FeedItem> GetNewcomer(IEnumerable<FeedItem> feedItems,
                                                    HashSet<String> hash)
        {
            if (null != feedItems) {
                foreach (var feed in feedItems) {
                    // 重複するものは除外
                    if (hash.Contains(feed.Link.AbsoluteUri)) { continue; }

                    yield return feed;
                }
            }
        }

        /// <summary>
        /// 新しい項目をDBに登録する
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="feedItems">feed項目一覧</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <returns>DB登録成功可否</returns>
        private Boolean LogUpdate(SQLite db, IEnumerable<FeedItem> feedItems, Int32 masterID)
        {
            Int32 regCount = TableRegistRowCount(db, masterID);
            Int32 delCount = (regCount + feedItems.Count()) - LOG_SAVE_MAX_COUNT;

            // 上限に合わせてログを削除
            DeleteLogItems(db, masterID, delCount);

            try {
                // 新規分を登録
                foreach (var item in feedItems) {
                    db.Update($"insert into log(" +
                              $"master_id, title, page_url, summary, is_read, reg_date, thumb_url)" +
                              $" values({masterID}, '{item.Title}', '{item.Link.AbsoluteUri}'," +
                              $" '{item.Summary?.Replace("'", "") ?? ""}', " +
                              $"{(item.IsRead ? 1 : 0)}, '{item.PublishDate}', '{item.ThumbUri}')");
                }
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 登録してあるmaster_idの件数を取得する
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <returns>指定IDのDB登録件数</returns>
        private Int32 TableRegistRowCount(SQLite db, Int32 masterID)
        {
            var ret = db.Select($"select count(log_id) as cnt from log where master_id = {masterID}");
            return Int32.Parse(ret["cnt"][0]);
        }

        /// <summary>
        /// 古いログを削除する
        /// </summary>
        /// <param name="db">DBインスタンス</param>
        /// <param name="masterID">DB上のマスターID</param>
        /// <param name="delCount">削除件数</param>
        private void DeleteLogItems(SQLite db, Int32 masterID, Int32 delCount)
        {
            if (0 < delCount) {
                // 古い順にログIDを取得
                var ret = db.Select(
                    $"select log_id from log where master_id = {masterID} order by reg_date asc");
                // 指定のカウント分　DBから削除する
                for (Int32 i = 0; i < delCount; i++) {
                    db.Update($"delete from log where log_id = {ret["log_id"][i]}");
                }
            }
        }
#if false
        /// <summary>
        /// Uriをもとにブラウザを起動する。
        /// </summary>
        /// <param name="item"></param>
        private void StartBrowser(FeedItem item)
        {
            item.IsRead = true;

            // 既読履歴を更新
            UpdateReadHistory(item);

            // ブラウザを起動
            Process.Start(this.ChromePath, $"{App.Configure?.BrowserOption ?? ""} {item.Link}");

            // 自動で最小化するオプション
            if (App.Configure?.IsAutoMinimize ?? false) {
                var bgw = WindowInfo.FindWindowByName(null, TITLE);
                WinMessage.Send(bgw, Window_MIN_MESSAGE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        /// <summary>
        /// 既読済みの設定を行う
        /// </summary>
        /// <param name="item">feed項目</param>
        private void UpdateReadHistory(FeedItem item)
        {
            Int32 masterID = (this.SiteSelectBox.SelectedItem as RssSiteInfo)?.ID ?? ERROR_RESULT;
            if (masterID < 0) { return; }

            using (var db = new SQLite(MASTER_PATH)) {

                db.Open();

                var isCommit = false;
                try {
                    db.BeginTransaction();
                    // 既読は [ 1 ] を設定する。
                    db.Update($"update log set is_read = 1 where log_id = {item.ID}");
                    isCommit = true;
                }
                catch (Exception) {
                    isCommit = false;
                }
                finally {
                    db.EndTransaction(isCommit);
                }
                db.Close();
            }
        }
#endif
#endregion

#region Network
        /// <summary>
        /// インターネットに接続しているか
        /// </summary>
        /// <returns></returns>
        private Boolean IsOnline()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        /// <summary>
        /// オフラインモードの表示
        /// </summary>
        /// <param name="conf">動作設定値</param>
        private void DispOfflineMode(RssConfigure conf)
        {
            if (IsOnline()) {
                // インターネット接続があっても設定がOfflineモードであれば表示する
                if (conf?.IsOffLine ?? false) {
                    this.IsOfflineBox.Visibility = Visibility.Visible;
                }
            }
            else {
                this.IsOfflineBox.Visibility = Visibility.Visible;
            }
        }
#endregion

#region Filter
        /// <summary>
        /// フィルタの解除
        /// </summary>
        private void FilterClear()
        {
            //this.FilterState = EditMode.None;

            this.DatePick.SelectedDate = null;
            this.KeywordBox.Text = String.Empty;
            this.IsReadComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// 条件による絞り込み処理を実行する
        /// </summary>
        /// <param name="item">サイト情報</param>
        /// <param name="key">フィルタリングするワード</param>
        /// <param name="date">フィルタリングする日付</param>
        /// <param name="isRead">フィルタリングする既読・未読状態</param>
        private void FilteringItems(RssSiteInfo item, 
                                    String key, DateTime? date, String isRead)
        {
            var source = GetMasterData(item);
            Int32 srcCount = source.Count();

            if (source == null) { return; }

            try {
                // キーワードフィルタ
                if (!String.IsNullOrEmpty(key)) {
                    source = source.Where(x =>
                                    0 <= x.Title.IndexOf(key));
                }
                // 日付フィルタ
                if (date != null) {
                    source = source.Where(x =>
                                    date?.Date == DateTime.Parse(x.PublishDate).Date);
                }
                // 既読・未読フィルタ
                if (Enum.TryParse(isRead, out ReadState state)) {
                    switch (state) {
                        case ReadState.Read:
                            source = source.Where(x => x.IsRead);
                            break;
                        case ReadState.Unread:
                            source = source.Where(x => !x.IsRead);
                            break;
                    }
                }
                // 無駄に更新しないように
                if (srcCount != source.Count()) {
                    this.FeedList.ItemsSource = source;
                }
            }
            catch (Exception) {
            }
        }

        /// <summary>
        /// フィルタ用にDBからベースとなるデータを取得する。
        /// </summary>
        /// <param name="item">サイト情報</param>
        /// <remarks>
        /// DBから毎回読み込むのがいいか
        /// Feedの読み込み枚にベースとなるデータをローカルに保持していた方がいいのか
        /// </remarks>
        private IEnumerable<FeedItem> GetMasterData(RssSiteInfo item)
        {
            Int32 masterID = item.ID;
            IEnumerable<FeedItem> feedItems = null;

            using (var db = new SQLite(MASTER_PATH)) {

                db.Open();

                // リスト・DBの更新
                feedItems = GetFeedItems(db, feedItems, masterID, LISTBOX_UPDATE);
            }
            return feedItems;
        }
#endregion

#region Ather
        /// <summary>
        /// 背景画像の設定がある場合にその画像を読み込んで設定する
        /// </summary>
        /// <param name="path">画像ファイルのパス</param>
        /// <param name="pos">画像ファイルを設定する位置設定</param>
        private void ReadBackground(String path, ImagePositionSetting pos)
        {
            if (String.IsNullOrWhiteSpace(path)) { return; }
            if (!File.Exists(path)) { return; }

            this.BackgroundImage.Source = CommFunc.ReadImage(path);

            try {
                this.BackgroundImage.Stretch = (Stretch)pos.Stretch;
                this.BackgroundImage.HorizontalAlignment = (HorizontalAlignment)pos.XAnchor;
                this.BackgroundImage.VerticalAlignment = (VerticalAlignment)pos.YAnchor;
            }
            catch (Exception) {
                // 強制的に位置を設定する
                this.BackgroundImage.Stretch = Stretch.None;
                this.BackgroundImage.HorizontalAlignment = HorizontalAlignment.Left;
                this.BackgroundImage.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        /// <summary>
        /// ピックアップにデータ登録を行う
        /// </summary>
        /// <param name="logID">対象のログID</param>
        private void RegistPickup(String logID)
        {
            if (String.IsNullOrWhiteSpace(logID)) { return; }

            using (var db = new SQLite(MASTER_PATH)) {

                db.Open();

                var isCommit = false;
                try {
                    db.BeginTransaction();

                    var ret = db.Select($"select * from pickup where log_id = {logID}");

                    if (ret.Count == 0) {
                        db.Update($"insert into pickup(log_id) values({logID})");
                    }
                    isCommit = true;
                }
                catch (Exception) {
                    isCommit = false;
                }
                finally {
                    db.EndTransaction(isCommit);
                }
                db.Close();
            }
        }
#endregion
    }
}