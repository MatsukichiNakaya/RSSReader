using System;
using System.Collections.Generic;

namespace RSSReader.Model
{
    /// <summary>
    /// Xmlを構成するタグ情報のクラス
    /// </summary>
    [Serializable]
    public class MarkupElement
    {
        /// <summary>タグ名</summary>
        public String Name { get; set; }
        /// <summary>値</summary>
        public String Value { get; set; }
        /// <summary>属性値</summary>
        public Dictionary<String, String> Attributes { get; set; }

        /// <summary>コンストラクタ</summary>
        public MarkupElement() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">タグ名</param>
        /// <param name="val">値</param>
        /// <param name="att">属性値</param>
        public MarkupElement(String name, String val, Dictionary<String, String> att)
        {
            this.Name = name;
            this.Value = val;
            this.Attributes = att;
        }
    }
}
