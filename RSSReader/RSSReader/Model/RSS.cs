using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Xml;

namespace RSSReader.Model
{
    public static class RSS
    {
        /// <summary>
        /// RSSに設定してあるサイトのタイトルを取得する
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static String ReadFeedTitle(String url)
        {
            try
            {
                var xml = new XmlDocument();
                xml.Load(url);
                XmlElement elem = xml.DocumentElement;

                // atom 形式
                if (elem.Name == "feed")
                {
                    return elem["title"].InnerText;
                }
                // RSS 1.0, 2.0 形式
                return elem["channel"]["title"].InnerText;
            }
            catch (Exception)
            {
                // いずれでもない
                return null;
            }
        }

        /// <summary>
        /// RSSのデータからFeedItemを読み込む
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IEnumerable<FeedItem> ReadFeedItems(String url)
        {
            var xml = new XmlDocument();
            xml.Load(url);
            XmlElement elem = xml.DocumentElement;
            String rssRootNodeTag = elem.Name;

            switch (rssRootNodeTag)
            {   // RSS1.0の場合、ルートノードはrdf:RDFタグ
                case "rdf:RDF":
                    return ReadRSS10(url);
                // RSS2.0の場合、ルートノードはrssタグである。
                case "rss":
                // atomの場合、ルートノードはfeedタグである。
                case "feed":
                    // RSS 2.0 と atomは同ロジックで読み込める
                    return ReadRSS20(url);
            }
            return null;
        }

        /// <summary>
        /// RSS 1.0 読み込み
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IEnumerable<FeedItem> ReadRSS10(String url)
        {
            var xml = new XmlDocument();

            xml.Load(url);
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            nsmgr.AddNamespace("rss", "http://purl.org/rss/1.0/");
            nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

            var items = new List<FeedItem>();

            foreach (XmlElement node in xml.SelectNodes("/rdf:RDF/rss:item", nsmgr))
            {
                yield return FeedItem.ReadRSS10(node, nsmgr);
            }
        }

        /// <summary>
        /// RSS 2.0 読み込み
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IEnumerable<FeedItem> ReadRSS20(String url)
        {
            using (var rdr = XmlReader.Create(url))
            {
                var feed = SyndicationFeed.Load(rdr);
                foreach (var item in feed.Items)
                {
                    yield return FeedItem.ReadRSS20(item);
                }
            }
        }
    }
}
