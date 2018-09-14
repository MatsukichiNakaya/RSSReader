using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Project.Serialization.Xml
{
    /// <summary>Xmlの書き出しクラス</summary>
    public class XmlSerializer
    {
        private const Boolean FILE_APPEND = true;
        private const Boolean FILE_OVERWRITE = false;

        /// <summary>
        /// XMLファイル書出し
        /// </summary>
        /// <typeparam name="T">クラス</typeparam>
        /// <param name="src">シリアライズ化するクラス</param>
        /// <param name="savePath">保存パス</param>
        /// <returns>成功:True/失敗:False</returns>
        public static Boolean Save<T>(T src, String savePath) where T : class
        {
            try
            {   // 読み込み用オブジェ作成
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                // 書き込み
                using (var sw = new StreamWriter(savePath,
                                        FILE_OVERWRITE, new UTF8Encoding(false)))
                {
                    serializer.Serialize(sw, src);
                }
            }
            catch (Exception) { return false; }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static T ConvertToClass<T>(String xmlData) where T : class
        {
            var result = default(T);
            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                using (var memorystream = new MemoryStream(Encoding.UTF8.GetBytes(xmlData)))
                {
                    result = serializer.Deserialize(memorystream) as T;
                }
            }
            catch { throw; }

            return result;
        }

        /// <summary> XMLファイル読込み </summary>
        /// <typeparam name="T">クラス</typeparam>
        /// <param name="readPath">読込パス</param>
        /// <returns>デシリアライズされたクラス</returns>
        public static T Load<T>(String readPath) where T : class
        {
            var result = default(T);
            // 読み込み用オブジェ作成
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            // 読込み
            using (var sr = new StreamReader(readPath, new UTF8Encoding(false)))
            {
                result = serializer.Deserialize(sr) as T;
            }
            return result;
        }

        /// <summary>Xml読込み文字列改行対応版 [L]ine[F]eed</summary>
        public static T LoadLF<T>(String readPath) where T : class
        {
            var result = default(T);
            var xmlDoc = new XmlDocument() { PreserveWhitespace = true };
            xmlDoc.Load(readPath);
            using (var reader = new XmlNodeReader(xmlDoc.DocumentElement))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                result = serializer.Deserialize(reader) as T;
            }
            return result;
        }

        /// <summary>
        /// XMLファイル読込み
        /// </summary>
        /// <typeparam name="T">クラス</typeparam>
        /// <param name="readPath">読込パス</param>
        /// <param name="readData">デシリアライズされたクラス</param>
        /// <returns>読込み正否</returns>
        public static Boolean TryLoad<T>(String readPath, out T readData) where T : class
        {
            readData = default(T);
            try
            {
                // 読み込み用オブジェ作成
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                // 読込み
                using (var sr = new StreamReader(readPath, new UTF8Encoding(false)))
                {   // デシリアライズできるか判定を設ける
                    if (serializer.CanDeserialize(new XmlTextReader(sr)))
                    {
                        readData = serializer.Deserialize(sr) as T;
                        return true;    // 読込み成功時にはTrue
                    }
                }
            }
            catch (Exception) { return false; }
            return false;// 何も無くきた場合は、読込できなかったとしてfalse
        }
    }
}
