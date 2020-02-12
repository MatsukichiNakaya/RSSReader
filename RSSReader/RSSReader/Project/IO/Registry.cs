using System;
using Microsoft.Win32;

namespace Project.IO
{
    public static class Registry
    {
        public static Object GetValue(String path, String key)
        {
            Object result = null;
#if true
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
#else
            result = @"C:\Program Files (x86)\Google\Chrome Dev\Application\chrome.exe";
#endif
            return result;
        }
    }
}
