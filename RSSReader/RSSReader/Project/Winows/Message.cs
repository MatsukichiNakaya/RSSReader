using System;
using Project.API.User;

namespace Project.Windows
{
    /// <summary>
    /// Windows message class
    /// </summary>
    public class WinMessage : WinMsgAPI
    {
        /// <summary> Send Message </summary>
        /// <param name="hWnd">Target Handle</param>
        /// <param name="uiMsg">Send Message</param>
        /// <param name="wParam">wParam</param>
        /// <param name="lParam">lParam</param>
        /// <returns></returns>
        public static Int64 Send(IntPtr hWnd, UInt32 uiMsg, IntPtr wParam, IntPtr lParam)
        {
            return NativeMethod.SendMessage(hWnd, uiMsg, wParam, lParam);
        }

        /// <summary> Post Message </summary>
        /// <param name="hWnd">Target Handle</param>
        /// <param name="uiMsg">Send Message</param>
        /// <param name="wParam">wParam</param>
        /// <param name="lParam">lParam</param>
        /// <returns></returns>
        public static Int64 Post(IntPtr hWnd, UInt32 uiMsg, UInt32 wParam, UInt32 lParam)
        {
            return NativeMethod.PostMessage(hWnd, uiMsg, wParam, lParam);
        }
    }
}
