using System;
using System.Runtime.InteropServices;

namespace Project.API.User
{
    /// <summary></summary>
    public class WinMsgAPI
    {
        /// <summary></summary>
        protected class NativeMethod
        {
            public const Int32 USER_MESSAGE_FIRST_NO = 0x8000;

            /// <summary> SendMessage </summary>
            [DllImport("user32.dll", SetLastError = true)]
            public extern static Int64 SendMessage(IntPtr hWnd, UInt32 uiMsg, UInt32 wParam, String lParam);
            /// <summary> SendMessage </summary>
            [DllImport("user32.dll")]
            public extern static Int64 SendMessage(IntPtr hWnd, UInt32 wMsg, IntPtr wParam, IntPtr lParam);

            /// <summary> PostMessage </summary>
            [DllImport("user32.dll", SetLastError = true)]
            public static extern Int32 PostMessage(IntPtr hWnd, UInt32 uiMsg, UInt32 wParam, UInt32 lParam);
        }
    }

}