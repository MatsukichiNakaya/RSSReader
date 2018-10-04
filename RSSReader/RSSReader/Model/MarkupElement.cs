using System;
using System.Collections.Generic;

namespace RSSReader.Model
{
    [Serializable]
    public class MarkupElement
    {
        public String Name { get; set; }
        public String Value { get; set; }
        public Dictionary<String, String> Attributes { get; set; }

        public MarkupElement() { }

        public MarkupElement(String val, Dictionary<String, String> att)
        {
            this.Value = val;
            this.Attributes = att;
        }
    }
}
