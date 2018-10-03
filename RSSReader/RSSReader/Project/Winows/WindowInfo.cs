using System;
using System.Collections.Generic;
using System.Text;
using Project.API.User;
using Project.Extention;

namespace Project.Windows
{
    /// <summary>
    /// Window関連のAPIをまとめたクラス
    /// </summary>
    public class WindowInfo : WindowAPI
    {
        /// <summary>
        /// ウインドウのタイトルからハンドルを取得します
        /// </summary>
        /// <param name="className"></param>
        /// <param name="windowName"></param>
        /// <returns></returns>
        public static IntPtr FindWindowByName(String className, String windowName)
        {
            return NativeMethod.FindWindow(className, windowName);
        }

        /// <summary>  </summary>
        /// <param name="hWndParent"></param>
        /// <param name="hWndChild"></param>
        /// <param name="className"></param>
        /// <param name="windowName"></param>
        /// <returns></returns>
        public static IntPtr FindWindowChild(IntPtr hWndParent, IntPtr hWndChild,
                                        String className, String windowName)
        {
            return NativeMethod.FindWindowEx(hWndParent, hWndChild, className, windowName);
        }

        /// <summary>
        /// アクティブウインドウのハンドル取得
        /// </summary>
        /// <returns></returns>
        public static IntPtr GetActiveWindow()
        {
            return NativeMethod.GetForegroundWindow();
        }

        /// <summary>
        /// プロセスIDの取得
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="processID"></param>
        /// <returns></returns>
        public static Int32 GetProcessID(IntPtr hWnd, out Int32 processID)
        {
            return NativeMethod.GetWindowThreadProcessId(hWnd, out processID);
        }

        /// <summary>
        /// ウインドウ名の列挙
        /// </summary>
        public static String[] EnumWinName()
        {
            var windowNames = new List<String>();
            //ウィンドウを列挙する
            NativeMethod.EnumWindows((hWnd, lParam) =>
            {
                String name = GetWindowText(hWnd);
                if (name.Length > 0) { windowNames.Add(name); }
                return true;
            }, IntPtr.Zero);

            return windowNames.ToArray();
        }

        /// <summary>ウインドウハンドルの列挙</summary>
        public static IntPtr[] EnumWinHandle()
        {
            var handles = new List<IntPtr>();
            //ウィンドウを列挙する
            NativeMethod.EnumWindows((hWnd, lParam) => 
            {
                String name = GetWindowText(hWnd);
                if (name.Length > 0) { handles.Add(hWnd); }
                return true;
            }, IntPtr.Zero);

            return handles.ToArray();
        }

        /// <summary>
        /// 指定ウインドウのコントロール列挙
        /// </summary>
        /// <param name="hWnd"></param>
        public static String[] EnumControl(IntPtr hWnd)
        {
            var names = new List<String>();
            // コントロールを列挙する 
            NativeMethod.EnumChildWindows(hWnd,(hCWnd, lParam) =>
            {
                String name = GetWindowText(hCWnd);  // テキストの取得
                if (name.Length > 0) { names.Add(name); }
                return true;
            }, IntPtr.Zero);
            return names.ToArray();
        }

        /// <summary>
        /// ウインドウのテキスト取得
        /// </summary>
        /// <param name="hWnd"></param>
        public static String GetWindowText(IntPtr hWnd)
        {
            String result = String.Empty;
            Int32 txtLength = NativeMethod.GetWindowTextLength(hWnd);

            if (0 < txtLength)
            {
                var title = new StringBuilder(txtLength + 1);
                NativeMethod.GetWindowText(hWnd, title, title.Capacity);
                result = title.ToString();
            }
            return result;
        }

        /// <summary>
        /// 座標下のウインドウ取得(座標を指定して取得 int型)
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns>ウインドウハンドル</returns>
        public static IntPtr GetHandleWindowFromPoint(Int32 X, Int32 Y)
        {
            return NativeMethod.WindowFromPoint(new System.Drawing.Point(X, Y));
        }

        /// <summary>
        /// 座標下のウインドウ取得(座標を指定して取得 Point型)
        /// </summary>
        /// <param name="pntPoint"></param>
        /// <returns>ウインドウハンドル</returns>
        public static IntPtr GetHandleWindowFromPoint(System.Drawing.Point pntPoint)
        {
            return NativeMethod.WindowFromPoint(pntPoint);
        }

        /// <summary>
        /// クラス名の取得
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static String GetClassName(IntPtr hWnd, Int32 buffer = 256)
        {
            Int32 textLen = NativeMethod.GetWindowTextLength(hWnd);
            var csb = new StringBuilder(buffer);
            if (0 < textLen)
            {
                NativeMethod.GetClassName(hWnd, csb, csb.Capacity);
            }
            return csb.ToString();
        }

        /// <summary>ウインドウを最背面に設置</summary>
        /// <param name="hWnd">設定を行うウインドウのハンドル</param>
        public static void SetBackMost(IntPtr hWnd)
        {
            // 現在あるすべてのウインドウのハンドルを取得 [Progman]か[Program Manager]を探す
            var Handles = EnumWinHandle().Filter(h => {
                var name = GetClassName(h);
                return name == "Progman" || name == "WorkerW";//"Program Manager";
            });

            foreach (var handle in Handles)
            {
                // [SHELLDLL_DefView]がぶら下がっているか？
                IntPtr parent = NativeMethod.FindWindowEx(handle, IntPtr.Zero,
                                                          "SHELLDLL_DefView", null);
                if (parent != IntPtr.Zero)
                {
                    NativeMethod.SetParent(hWnd, parent);
                    break;
                }
            }
        }

        public static IntPtr GetBackMostHanndle()
        {
            return NativeMethod.FindWindowEx(
                            NativeMethod.FindWindowEx(NativeMethod.FindWindow("Progman", 
                                                                              "Program Manager"),
                                                      IntPtr.Zero, "SHELLDLL_DefView", ""),
                            IntPtr.Zero, "SysListView32", "FolderView");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="hdl"></param>
        public static IntPtr SetParent(IntPtr hWnd, IntPtr hdl)
        {
            return NativeMethod.SetParent(hWnd, hdl);
        }
    }
}
