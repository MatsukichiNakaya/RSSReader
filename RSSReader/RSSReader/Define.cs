using System;

namespace RSSReader
{
    internal class Define
    {
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
        /// RSSフィードの履歴保存DB
        /// </summary>
        public const String MASTER_PATH = @".\rss_log.db";

        /// <summary>
        /// ソフト設定値ファイル
        /// </summary>
        public const String XML_PATH = @"Configure.xml";

        /// <summary>
        /// 表示状態保持用ファイル
        /// </summary>
        public const String PAGE_DAT = @".\page.dat";

        public enum EditMode
        {
            None = 0,
            Editing,
        }

        /// <summary>
        /// ページ保持用のメッセージ
        /// </summary>
        public const Int32 CHANGE_MESSAGE = 32770;

        /// <summary>
        /// ウインドウ最小化メッセージ
        /// </summary>
        public const Int32 Window_MIN_MESSAGE = 32771;
    }
}
