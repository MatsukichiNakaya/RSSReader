using System;
using System.Xml.Serialization;

namespace RSSReader.Model
{
    /// <summary>
    /// RSS Readerの動作設定
    /// </summary>
    [Serializable]
    public class RssConfigure
    {
        /// <summary>
        /// ブラウザ起動時のオプション
        /// </summary>
        [XmlElement("BrowserOption", DataType = "string", IsNullable = true)]
        public String BrowserOption { get; set; }

        /// <summary>
        /// 自動更新間隔
        /// </summary>
        [XmlElement("UpdateSpan", DataType = "int", IsNullable =false)]
        public Int32 UpdateSpan { get; set; }

        /// <summary>
        /// サムネイル画像表示有無
        /// </summary>
        [XmlElement("ShowImage", DataType = "boolean", IsNullable = false)]
        public Boolean IsShowImage { get; set; }

        /// <summary>
        /// ソフトを閉じたときのインデックス保持有無
        /// </summary>
        [XmlElement("KeepPage", DataType = "boolean", IsNullable = false)]
        public Boolean IsKeepPage { get; set; }

        /// <summary>
        /// ブラウザ起動時に自動的に最小化する
        /// </summary>
        [XmlElement("AutoMinimize", DataType = "boolean", IsNullable = false)]
        public Boolean IsAutoMinimize { get; set; }
    }
}
