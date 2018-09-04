using System;
using System.Windows;

using System.Runtime.InteropServices;

namespace C_SlideShow
{
    public static class Win32
    {
        /* ---------------------------------------------------- */
        //     Common Strunture
        /* ---------------------------------------------------- */
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        /* ---------------------------------------------------- */
        //     Native method
        /* ---------------------------------------------------- */
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref POINT pt);


        public const int VK_LBUTTON  = 0x01;
        public const int VK_RBUTTON  = 0x02;
        public const int VK_MBUTTON  = 0x04;
        public const int VK_XBUTTON1 = 0x05;
        public const int VK_XBUTTON2 = 0x06;
        [DllImport("user32")] 
        public static extern short GetKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private const int GW_HWNDNEXT = 2;

        [DllImport("user32")]
        private extern static int GetParent(int hwnd);

        [DllImport("user32")]
        private extern static int GetWindow(int hwnd, int wCmd);

        [DllImport("user32")]
        private extern static int FindWindow( String lpClassName, String lpWindowName);

        [DllImport("user32")]
        private extern static int GetWindowThreadProcessId( int hwnd, out int lpdwprocessid);

        [DllImport("user32")]
        private extern static int IsWindowVisible(int hwnd);

        [DllImport("User32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, out MonitorInfoEx info);
        [StructLayout(LayoutKind.Sequential)]
        public struct MonitorInfoEx
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        [DllImport("User32.dll")]
        public static extern IntPtr MonitorFromRect(out RECT lprc, uint dwFlags);

        [DllImport("User32.dll")]
        public static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);
        public const uint MONITOR_DEFAULTTONEAREST  = 0x00000002;
        public const uint MONITOR_DEFAULTTONULL     = 0x00000000;
        public const uint MONITOR_DEFAULTTOPRIMARY  = 0x00000001;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        public const uint SWP_NOSIZE        = 0x0001;
        public const uint SWP_NOMOVE        = 0x0002;
        public const uint SWP_NOZORDER      = 0x0004;
        public const uint SWP_NOACTIVATE    = 0x0010;

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);


        public const int WM_MOUSEMOVE  = 0x0200;
        public const int HC_ACTION = 0;

        public const int WM_LBUTTONUP  = 0x0202;    // 514
        public const int WM_RBUTTONUP  = 0x0205;    // 517
        public const int WM_MBUTTONUP  = 0x0208;    // 520
        public const int WM_XBUTTONUP  = 0x020C;    // 524

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HOOKPROC lpfn, IntPtr hMod, IntPtr dwThreadId);
        public delegate IntPtr HOOKPROC(int nCode, IntPtr wParam, IntPtr lParam);

        public enum HookType : int
        {
             WH_MSGFILTER = -1,
             WH_JOURNALRECORD = 0,
             WH_JOURNALPLAYBACK = 1,
             WH_KEYBOARD = 2,
             WH_GETMESSAGE = 3,
             WH_CALLWNDPROC = 4,
             WH_CBT = 5,
             WH_SYSMSGFILTER = 6,
             WH_MOUSE = 7,
             WH_HARDWARE = 8,
             WH_DEBUG = 9,
             WH_SHELL = 10,
             WH_FOREGROUNDIDLE = 11,
             WH_CALLWNDPROCRET = 12,
             WH_KEYBOARD_LL = 13,
             WH_MOUSE_LL = 14,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
             public POINT pt;
             public int mouseData;
             public int flags;
             public int time;
             public UIntPtr dwExtraInfo;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string moduleName);

        /* ---------------------------------------------------- */
        //     Wrapper
        /* ---------------------------------------------------- */
        // ウィンドウハンドル(hwnd)をプロセスID(pid)に変換する
        public static int GetPidFromHwnd(int hwnd)
        {
            int pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return pid;
        }

        public static Point GetCursorPos()
        {
            POINT w32Mouse = new POINT();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public static Rect GetWindowRect(Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;;
            RECT rect;
            GetWindowRect(hwnd, out rect);

            return new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        public static Point GetWindowPos(IntPtr hwnd)
        {
            RECT rect;
            GetWindowRect(hwnd, out rect);
            return new Point(rect.left, rect.top);
        }

        public static Rect GetScreenRectFromRect(Rect rcWnd)
        {
            RECT rcFrom;
            rcFrom.left     = (int)rcWnd.Left;
            rcFrom.top      = (int)rcWnd.Top;
            rcFrom.right    = (int)rcWnd.Right;
            rcFrom.bottom   = (int)rcWnd.Right;

            //移動先モニターの取得(なければ近くのを取得)
            IntPtr hMonitor;
            MonitorInfoEx mi;
            mi.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MonitorInfoEx));

            hMonitor = MonitorFromRect(out rcFrom, MONITOR_DEFAULTTONEAREST);
            GetMonitorInfo(hMonitor, out mi);

            return new Rect(
                new Point(mi.rcMonitor.left, mi.rcMonitor.top),
                new Point(mi.rcMonitor.right, mi.rcMonitor.bottom)
                );
        }


        /// <summary>
        /// ウインドウの位置を正しく調整
        //  ワーキングエリアから出ないようにする
        //  モニター設定の変更のせいで、見えない位置に移動されるのも防ぐ 
        //  最大化・最小化の復元は考慮しない
        /// </summary>
        /// <param name="hWnd">ウインドウハンドル</param>
        /// <param name="PosX">移動先のX座標</param>
        /// <param name="PosY">移動先のY座標</param>
        /// <param name="allowProtrusion">はみ出し許可</param>
        /// <param name="padding"></param>
        /// <param name="bMonitorFromRect">どのモニターでチェックするかは、矩形で決める。falseの場合原点で決まる</param>
        /// <returns>ずらした値</returns>
        public static POINT SetWindowPosProperly(IntPtr hWnd, int PosX, int PosY, 
            bool allowProtrusion, int padding, bool bMonitorFromRect)
        {
            //対象のウインドウの大きさを取得
            RECT rcWnd;
            GetWindowRect(hWnd, out rcWnd);
            int width   = rcWnd.right - rcWnd.left;
            int height  = rcWnd.bottom - rcWnd.top;

            //移動先にする予定の矩形
            RECT rcDest;
            rcDest.left     = PosX;
            rcDest.top      = PosY;
            rcDest.right    = PosX + width;
            rcDest.bottom   = PosY + height;

            //移動先モニターの取得(なければ近くのを取得)
            IntPtr hMonitor;
            MonitorInfoEx mi;
            mi.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MonitorInfoEx));

            POINT pt;
            pt.X = PosX ;
            pt.Y = PosY ;
            if (bMonitorFromRect){
                hMonitor = MonitorFromRect(out rcDest, MONITOR_DEFAULTTONEAREST);
            }
            else{
                hMonitor = MonitorFromPoint(pt, MONITOR_DEFAULTTONEAREST);
            }
            GetMonitorInfo(hMonitor, out mi);

            //補正値
            POINT ptCorrected;
            ptCorrected.X = ptCorrected.Y = 0;

            // 位置補正(はみ出し許可。ただし、ウインドウ全体がはみ出したら補正される。padding値は現在は無視)
            if (allowProtrusion)
            {
                if (rcDest.left > mi.rcWork.right)
                {
                    ptCorrected.X = mi.rcWork.right - rcDest.right;
                    //rcDest.left -= rcDest.right - mi.rcWork.right;
                }
                if (rcDest.right < mi.rcWork.left)
                {
                    ptCorrected.X = mi.rcWork.right - rcDest.right;
                    //rcDest.left = mi.rcWork.left;
                }
                if (rcDest.top > mi.rcWork.bottom)
                {
                    ptCorrected.Y = mi.rcWork.bottom - rcDest.bottom;
                    //rcDest.top -= rcDest.bottom - mi.rcWork.bottom;
                }
                if (rcDest.bottom < mi.rcWork.top)
                {
                    ptCorrected.Y = mi.rcWork.bottom - rcDest.bottom;
                    //rcDest.top = mi.rcWork.top;
                }
            }

            // 位置補正(少しでもはみ出すことを許可しない)
            else
            {
                if (rcDest.right - padding > mi.rcWork.right) //右
                {
                    ptCorrected.X = mi.rcWork.right - rcDest.right + padding;
                    //rcDest.left -= rcDest.right - mi.rcWork.right - padding;
                }
                if (rcDest.left + padding < mi.rcWork.left) //左
                {
                    ptCorrected.X = mi.rcWork.left - rcDest.left - padding;
                    //rcDest.left = mi.rcWork.left - padding;
                }
                if (rcDest.bottom - padding > mi.rcWork.bottom) //下
                {
                    ptCorrected.Y = mi.rcWork.bottom - rcDest.bottom + padding;
                    //rcDest.top -= rcDest.bottom - mi.rcWork.bottom - padding;
                }
                if (rcDest.top  + padding < mi.rcWork.top) //上
                {
                    ptCorrected.Y = mi.rcWork.top - rcDest.top - padding;
                    //rcDest.top = mi.rcWork.top - padding;
                }
            }
            // ウィンドウ位置復元
            SetWindowPos( hWnd, System.IntPtr.Zero, 
                rcDest.left + ptCorrected.X, 
                rcDest.top + ptCorrected.Y,
                width, height, SWP_NOZORDER);

            return ptCorrected;
        }

    }
}
