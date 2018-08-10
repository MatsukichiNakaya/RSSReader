using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSSReader.Model
{
    /// <summary>
    /// RSS の取得先情報
    /// </summary>
    public class RssSiteInfo
    {
        /// <summary>
        /// DB登録番号
        /// </summary>
        public Int32 ID { get; set; }

        /// <summary>
        /// RSS配信サイト名
        /// </summary>
        public String SiteName { get; set; }

        /// <summary>
        /// RSSへのリンク
        /// </summary>
        public String Link { get; set; }

        /// <summary>
        /// RSS配信サイト情報
        /// </summary>
        public String Summary { get; set; }
    }
}
