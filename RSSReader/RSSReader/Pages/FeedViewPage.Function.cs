using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using RSSReader.Model;
using Project.DataBase;
using System.IO;
using System.Windows.Media;

namespace RSSReader.Pages
{
    public partial class FeedViewPage : Page
    {
        private const String MASTER_PATH = @".\rss_log.db";

        #region ComboBox
        /// <summary>
        /// コンボボックスの中身を設定する
        /// </summary>
        public void ReLoadSiteItems()
        {
            this.SiteSelectBox.ItemsSource = GetSiteInfo();
        }

        /// <summary>
        /// コンボボックスに設定するデータを取得する
        /// </summary>
        /// <returns></returns>
        private IEnumerable<RssSiteInfo> GetSiteInfo()
        {
            var cmbItems = new List<RssSiteInfo>();
            using (var db = new SQLite(MASTER_PATH))
            {
                db.Open();

                var ret = db.Select("select * from rss_master");

                Int32 count = ret["id"].Count;
                for (Int32 i = 0; i < count; i++)
                {
                    cmbItems.Add(new RssSiteInfo() {
                        ID = Int32.Parse(ret["id"][i]),
                        SiteName = ret["site"][i],
                        Link = ret["url"][i],
                    });
                }
            }
            return cmbItems;
        }
        #endregion

        #region ListBox
        /// <summary>
        /// RSS フィードリストの更新を行う
        /// </summary>
        /// <param name="item"></param>
        private void UpdateListBox(RssSiteInfo item)
        {
            String url = item?.Link;
            if (url == null)
            { return; }

            Int32 masterID = item.ID;
            IEnumerable<FeedItem> feedItems = null;

            using (var db = new SQLite(MASTER_PATH))
            {
                db.Open();
                if (CanRSSRead(db, masterID))
                {
                    feedItems = RSS.ReadFeedItems(url);
                    UpdateLastSync(db, masterID);
                }

                // リスト更新
                SetFeedItems(db, feedItems, masterID);
            }
        }

        /// <summary>
        /// 更新間隔チェック
        /// </summary>
        /// <param name="masterID"></param>
        /// <returns></returns>
        private Boolean CanRSSRead(SQLite db, Int32 masterID)
        {
            Boolean result = false;

            // 最終更新日時を取得
            var ret = db.Select($"select last_update from sync where master_id = {masterID}");
            var last = DateTime.Parse(ret["last_update"][0]);

            // Todo :
            // 更新間隔設定値を超えているか？
            result = 5 < (DateTime.Now - last).Minutes;

            return result;
        }

        /// <summary>
        /// 読み込み日時更新
        /// </summary>
        /// <param name="masterID"></param>
        private void UpdateLastSync(SQLite db, Int32 masterID)
        {
            // 一件だけなのでこれだけ
            db.Update($"update sync set last_update='{DateTime.Now.ToString(FeedItem.DATE_FORMAT)}' where master_id={masterID}");
        }

        /// <summary>
        /// リストボックスに割り当てる項目をDBからも取得して設定する
        /// </summary>
        /// <param name="feedItems"></param>
        /// <param name="masterID"></param>
        private void SetFeedItems(SQLite db, IEnumerable<FeedItem> feedItems, Int32 masterID)
        {
            // キャッシュ用のディレクトリ確認
            FeedItem.ExistsChashDirectory(masterID.ToString());

            // 更新日時の最新で並べ替える
            var items = GetFeedItemsToDB(db, feedItems, masterID).OrderByDescending(fd => fd.PublishDate);

            foreach (var item in items)
            {
                item.Thumbnail = GetImage(item.ThumbUri, masterID);
                if (item.ThumbUri != null)
                {
                    item.ThumbWidth = 160;
                }
            }
            this.FeedList.ItemsSource = items;

        }

        /// <summary>
        /// サムネ画像をダウンロード、または、キャッシュから読み込む
        /// </summary>
        /// <param name="url"></param>
        /// <param name="masterID"></param>
        /// <returns></returns>
        private ImageSource GetImage(Uri url, Int32 masterID)
        {
            String localPath = FeedItem.GetChashPath(url?.AbsoluteUri, masterID);

            if (File.Exists(localPath))
            {   // chashから読み込み
                return FeedItem.ReadChashThumb(localPath);
            }
            else
            {   // ダウンロード
                return FeedItem.DownloadThumb(url?.AbsoluteUri, masterID);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="feedItems"></param>
        /// <param name="masterID"></param>
        /// <returns></returns>
        private IEnumerable<FeedItem> GetFeedItemsToDB(SQLite db, IEnumerable<FeedItem> feedItems, Int32 masterID)
        {
            var result = GetLogItems(db, masterID).ToList();
            // RSS記事のページURLからかぶりがないか確認する
            var urlHash = new HashSet<String>(result.Select(l => l.Link.AbsoluteUri));
            var newItems = GetNewcomer(feedItems, urlHash);

            db.BeginTransaction();
            Boolean isCommit = false;

            try
            {
                LogUpdate(db, newItems, masterID);

                isCommit = true;
            }
            catch (Exception)
            {
                isCommit = false;
            }
            finally
            {
                db.EndTransaction(isCommit);
            }

            result.AddRange(newItems);

            return result;
        }

        /// <summary>
        /// DBからFeedItemを取得する
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FeedItem> GetLogItems(SQLite db, Int32 masterID)
        {
            var ret = db.Select($"select * from log where master_id = {masterID}");

            Int32 count = ret["log_id"].Count;
            for (Int32 i = 0; i < count; i++)
            {
                yield return new FeedItem() {
                    Title = ret["title"][i],
                    PublishDate = ret["reg_date"][i],
                    Summary = ret["summary"][i]?.Replace("'", ""),
                    Link = new Uri(ret["page_url"][i]),
                    IsRead = Int32.Parse(ret["is_read"][i]) == 1,
                    ThumbUri = String.IsNullOrEmpty(ret["thumb_url"][i])
                                ? null : new Uri(ret["thumb_url"][i]),
                };
            }
        }

        /// <summary>
        /// ログから取得したURLとかぶらない
        /// </summary>
        /// <param name="feedItems"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        private IEnumerable<FeedItem> GetNewcomer(IEnumerable<FeedItem> feedItems, HashSet<String> hash)
        {
            if (null != feedItems)
            {
                foreach (var feed in feedItems)
                {
                    // 重複するものは除外
                    if (hash.Contains(feed.Link.AbsoluteUri))
                    { continue; }

                    yield return feed;
                }
            }
        }

        /// <summary>
        /// 新しい項目をDBに登録する
        /// </summary>
        /// <returns></returns>
        private Boolean LogUpdate(SQLite db, IEnumerable<FeedItem> feedItems, Int32 masterID)
        {
            Int32 regCount = TableRegistRowCount(db, masterID);
            Int32 delCount = (regCount + feedItems.Count()) - 100;

            // 上限に合わせてログを削除
            DeleteLogItems(db, masterID, delCount);

            // 新規分を登録
            foreach (var item in feedItems)
            {
                db.Update($"insert into log(master_id, title, page_url, summary, is_read, reg_date, thumb_url) values(" +
                          $"{masterID}, '{item.Title}', '{item.Link.AbsoluteUri}', '{item.Summary.Replace("'", "")}', " +
                          $"{(item.IsRead ? 1 : 0)}, '{item.PublishDate}', '{item.ThumbUri}')");
            }
            return true;
        }

        /// <summary>
        /// 登録してあるmaster_idの件数を取得する
        /// </summary>
        /// <param name="db"></param>
        /// <param name="masterID"></param>
        /// <returns></returns>
        private Int32 TableRegistRowCount(SQLite db, Int32 masterID)
        {
            var ret = db.Select($"select count(log_id) as cnt from log where master_id = {masterID}");
            return Int32.Parse(ret["cnt"][0]);
        }

        /// <summary>
        /// 古いログを削除する
        /// </summary>
        /// <param name="db"></param>
        /// <param name="masterID"></param>
        /// <param name="delCount"></param>
        private void DeleteLogItems(SQLite db, Int32 masterID, Int32 delCount)
        {
            if (0 < delCount)
            {
                // 古い順にログIDを取得
                var ret = db.Select($"select log_id from log where master_id = {masterID} order by reg_date asc");
                // 指定のカウント分　DBから削除する
                for (Int32 i = 0; i < delCount; i++)
                {
                    db.Update($"delete from log where log_id = {ret["log_id"][i]}");
                }
            }
        }
        #endregion

    }
}
