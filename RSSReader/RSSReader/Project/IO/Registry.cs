using System;
using Microsoft.Win32;

namespace Project.IO
{
    public static class Registry
    {
        public static Object GetValue(String path, String key)
        {
            Object result = null;
            try
            {
                using (RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path))
                {
                    // レジストリの値を取得
                    result = regKey.GetValue(key);
                    // 読むだけなのでCloseは無し
                }
            }
            catch (Exception) { result = null; }

            return result;
        }
    }
}
