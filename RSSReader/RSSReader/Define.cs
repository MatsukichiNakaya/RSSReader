using System;

namespace RSSReader
{
    /// <summary>
    /// 共通定義
    /// </summary>
    internal class Define
    {
        /// <summary>
        /// アプリケーションタイトル
        /// </summary>
        public const String TITLE = "RssReader";

        /// <summary>
        /// RSS 1.0
        /// </summary>
        public const String RSS_HEADER_10 = "rdf:RDF";
        /// <summary>
        /// RSS 2.0
        /// </summary>
        public const String RSS_HEADER_20 = "rss";
        /// <summary>
        /// atom
        /// </summary>
        public const String RSS_HEADER_ATOM = "feed";

        /// <summary>
        /// 設定ファイルなどのディレクトリ
        /// </summary>
        public const String DAT_DIR = @".\Dat";

        /// <summary>
        /// RSSフィードの履歴保存DB
        /// </summary>
        public const String MASTER_PATH = @".\Dat\rss_log.db";

        /// <summary>
        /// ソフト設定値ファイル
        /// </summary>
        public const String XML_PATH = @".\Dat\Configure.xml";

        /// <summary>
        /// 表示状態保持用ファイル
        /// </summary>
        public const String PAGE_DAT = @".\Dat\page.dat";

        /// <summary>
        /// エディットモード
        /// </summary>
        public enum EditMode
        {
            None = 0,
            Editing,
        }

        /// <summary>
        /// 既読・未読ステータス
        /// </summary>
        public enum ReadState
        {
            None = 0,
            Read = 1,
            Unread = 2
        }

        /// <summary>
        /// ページ保持用のメッセージ
        /// </summary>
        public const Int32 CHANGE_MESSAGE = 32770;

        /// <summary>
        /// ウインドウ最小化メッセージ
        /// </summary>
        public const Int32 Window_MIN_MESSAGE = 32771;

        /// <summary>
        /// 背景画像読み込みメッセージ
        /// </summary>
        public const Int32 BACKGROUND_READ_MESSAGE = 32772;

        /// <summary>
        /// エラー時の返り値
        /// </summary>
        public const Int32 ERROR_RESULT = -1;

        /// <summary>
        /// 同じ場所にアクセスする場合の時間間隔
        /// </summary>
        public const Int32 INTERVAL_TIME = 5;

        /// <summary>
        /// RSSの履歴を保持する最大数(1サイト当たり)
        /// </summary>
        public const Int32 LOG_SAVE_MAX_COUNT = 100;

        /// <summary>
        /// サムネイル画像のデフォルト設定値
        /// </summary>
        public const Int32 DEFAULT_PIC_WIDTH = 160;

        /// <summary>
        /// RSS読み込み時にListBoxを更新するか？
        /// </summary>
        public const Boolean LISTBOX_UPDATE = true;
    }
}
