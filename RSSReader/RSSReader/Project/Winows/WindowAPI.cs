using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Project.API.User
{
    /// <summary></summary>
    public class WindowAPI
    {
        public delegate Boolean EnumWindowsDelegate(IntPtr hWnd, IntPtr lParam);
        public delegate Boolean EnumWindowsChildDelegate(IntPtr hWnd, IntPtr lParam);

        /// <summary></summary>
        protected class NativeMethod
        {
            #region Win32API
            /// <summary>
            /// Find Window
            /// </summary>
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr FindWindow(String className, String windowName);

            /// <summary>
            /// Find Window Ex
            /// </summary>
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChild,
                                                        String className, String windowName);

            /// <summary>
            /// Get Foreground window
            /// </summary>
            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            /// <summary>
            /// GetWindowThreadProcessId
            /// </summary>
            [DllImport("user32.dll")]
            public static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out Int32 processID);

            /// <summary>
            /// Enum Window
            /// </summary>
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public extern static Boolean EnumWindows(EnumWindowsDelegate pEnumFunc, IntPtr lParam);

            /// <summary>
            /// EnumChildWindows
            /// </summary>
            [DllImport("user32.Dll", CharSet = CharSet.Auto,
                                                    CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern Boolean EnumChildWindows(IntPtr hWndParent,
                                                EnumWindowsChildDelegate pCallback, IntPtr lParam);

            /// <summary>
            /// GetWindowText
            /// </summary>
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern Int32 GetWindowText(IntPtr hWnd, StringBuilder builder, Int32 maxCount);

            /// <summary>
            /// GetWindowTextLength
            /// </summary>
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern Int32 GetWindowTextLength(IntPtr hWnd);

            /// <summary>
            /// WindowFromPoint
            /// </summary>
            [DllImport("user32.dll", CharSet = CharSet.Auto,
                                                    CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr WindowFromPoint(System.Drawing.Point p);

            /// <summary>
            /// GetClassName
            /// </summary>
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern Int32 GetClassName(IntPtr hWnd, StringBuilder className, Int32 maxCount);

            /// <summary>
            /// SetParent
            /// </summary>
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
            #endregion
        }
    }
}