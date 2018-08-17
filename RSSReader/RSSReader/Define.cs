using System;

namespace RSSReader
{
    internal class Define
    {
        public const String RSS_HEADER_10 = "rdf:RDF";
        public const String RSS_HEADER_20 = "rss";
        public const String RSS_HEADER_ATOM = "feed";
        public const String MASTER_PATH = @".\rss_log.db";

        public enum EditMode
        {
            None = 0,
            Editing,
        }
    }
}
