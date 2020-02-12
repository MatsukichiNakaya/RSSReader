using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RSSReader.Model
{
    public class DriveFile
    {
        [JsonProperty("name")]
        public String Name { get; set; }
        // 以下のプロパティは今回使用しませんが、デバッグ時に値を見ることをお勧めします。
        [JsonProperty("webUrl")]
        public String WebUrl { get; set; }
        [JsonProperty("createdDateTime")]
        public String CreatedDateTime { get; set; }
        [JsonProperty("lastModifiedDateTime")]
        public String LastModifiedDateTime { get; set; }
    }

    public class DriveFiles
    {
        [JsonProperty("value")]
        public List<DriveFile> Value;
    }

    // ファイル移動時に使います。
    public class DriveParentFolder
    {
        [JsonProperty("path")]
        public String Path { get; set; }
    }

    public class DriveFileModify
    {
        [JsonProperty("name")]
        public String Name { get; set; }
        // ファイル移動時に使います。
        [JsonProperty("parentReference")]
        public DriveParentFolder ParentReference { get; set; }
    }
}
