using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace C_SlideShow
{
    public class WindowSnap
    {
        /* ---------------------------------------------------- */
        //     フィールド
        /* ---------------------------------------------------- */
        private IntPtr  hWnd;
        private RECT    rcMonitor = new RECT();
        private RECT    rcWork    = new RECT();

        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        public bool EnableWindowSnap { get; set; } = true;
        public bool EnableScreenSnap { get; set; } = true;
        public int  Range_Window { get; set; } = 10;
        public int  Range_Screen { get; set; } = 10;

        /* ---------------------------------------------------- */
        //     コンストラクタ
        /* ---------------------------------------------------- */
        public WindowSnap(Window targetWindow)
        {
            hWnd =  new System.Windows.Interop.WindowInteropHelper(targetWindow).Handle;
        }

        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */

        /// <summary>
        /// ウインドウ移動中に呼ぶことで、ウインドウスナップを適用させる
        /// </summary>
        /// <param name="_rcDest">移動先(予定)の矩形</param>
        /// <returns>スナップしたかどうか</returns>
        public bool OnWindowMoving(Rect _rcDest)
        {
            if( !EnableScreenSnap && !EnableWindowSnap ) return false;
            if( Range_Window < 1 && Range_Screen < 1 ) return false;

            RECT rcDest = new RECT() { left = (int)_rcDest.Left, top = (int)_rcDest.Top, right = (int)_rcDest.Right, bottom = (int)_rcDest.Bottom };

            bool bSnaped = false;

            if(EnableScreenSnap && !EnableWindowSnap )
            {
                UpdateScreenRectFromRect(rcDest);
                bSnaped |= ScreenSnapH(ref rcDest);
                bSnaped |= ScreenSnapV(ref rcDest);
            }
            else if(!EnableScreenSnap && EnableWindowSnap )
            {
                bSnaped |= WindowSnapH(ref rcDest);
                bSnaped |= WindowSnapV(ref rcDest);
            }
            else
            {
                UpdateScreenRectFromRect(rcDest);
                if( !ScreenSnapH(ref rcDest) ) { bSnaped |= WindowSnapH(ref rcDest); }
                else bSnaped = true;
                if( !ScreenSnapV(ref rcDest) ) { bSnaped |= WindowSnapV(ref rcDest); }
                else bSnaped = true;
            }

            SetWindowPos(hWnd, IntPtr.Zero, rcDest.left, rcDest.top, 0, 0, SWP_NOSIZE);

            return bSnaped;
        }

        private bool WindowSnapH(ref RECT rcDest)
        {
            RECT rcOther = new RECT();
            int ptY = rcDest.top + (rcDest.bottom - rcDest.top) / 2;
            POINT ptLeft  = new POINT() { X = rcDest.left  - Range_Window, Y = ptY };
            POINT ptRight = new POINT() { X = rcDest.right + Range_Window, Y = ptY };

            // left
            if( GetTopLevelWindowRectFromPoint(ptLeft, ref rcOther) && SnapTest_Window(rcDest.left, rcOther.right) )
            {
                rcDest.left -= (rcDest.left - rcOther.right); 
                return true;
            }

            // right
            else if( GetTopLevelWindowRectFromPoint(ptRight, ref rcOther) && SnapTest_Window(rcDest.right, rcOther.left) )
            {
                rcDest.left -= (rcDest.right - rcOther.left); 
                return true;
            }

            return false;
        }

        private bool WindowSnapV(ref RECT rcDest)
        {
            RECT rcOther = new RECT();
            int  ptX     = rcDest.left + (rcDest.right - rcDest.left) / 2;
            POINT ptTop    = new POINT() { X = ptX, Y = rcDest.top    - Range_Window };
            POINT ptBottom = new POINT() { X = ptX, Y = rcDest.bottom + Range_Window };

            // top
            if( GetTopLevelWindowRectFromPoint(ptTop, ref rcOther) && SnapTest_Window(rcDest.top, rcOther.bottom) )
            {
                rcDest.top -= (rcDest.top - rcOther.bottom);
                return true;
            }

            // bottom
            else if( GetTopLevelWindowRectFromPoint(ptBottom, ref rcOther) && SnapTest_Window(rcDest.bottom, rcOther.top) )
            {
                rcDest.top -= (rcDest.bottom - rcOther.top);
                return true;
            }

            return false;
        }

        private bool ScreenSnapH(ref RECT rcDest)
        {
            int windowWidth = rcDest.right - rcDest.left;

            if (SnapTest_Screen(rcWork.left, rcDest.left))
            {
                rcDest.left = rcWork.left;
                return true;
            }
            else if (SnapTest_Screen(rcWork.right, rcDest.right))
            {
                rcDest.left = rcWork.right - windowWidth;
                return true;
            }
            else if (SnapTest_Screen(rcMonitor.left, rcDest.left))
            {
                rcDest.left = rcMonitor.left;
                return true;
            }
            else if (SnapTest_Screen(rcMonitor.right, rcDest.right))
            {
                rcDest.left = rcMonitor.right - windowWidth;
                return true;
            }
            return false;
        }

        private bool ScreenSnapV(ref RECT rcDest)
        {
            int windowHeight = rcDest.bottom - rcDest.top;

            if (SnapTest_Screen(rcWork.top, rcDest.top))
            {
                rcDest.top = rcWork.top;
                return true;
            }
            else if (SnapTest_Screen(rcWork.bottom, rcDest.bottom))
            {
                rcDest.top = rcWork.bottom - windowHeight;
                return true;
            }
            else if (SnapTest_Screen(rcMonitor.top, rcDest.top))
            {
                rcDest.top = rcMonitor.top;
                return true;
            }
            else if (SnapTest_Screen(rcMonitor.bottom, rcDest.bottom))
            {
                rcDest.top = rcMonitor.bottom - windowHeight;
                return true;
            }
            return false;
        }

        private bool GetTopLevelWindowRectFromPoint(POINT pt, ref RECT rc)
        {
            IntPtr hCtrl = WindowFromPoint(pt);
            if( hCtrl == IntPtr.Zero ) return false;

            IntPtr hTop = GetAncestor(hCtrl, GA_ROOT);
            if ( hTop == IntPtr.Zero ) return false;

            System.OperatingSystem os = System.Environment.OSVersion;
            if(os.Version.Major >= 6 ) // over windows vista
            {
                HRESULT hr = DwmGetWindowAttribute(hTop, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out rc, Marshal.SizeOf(rc));
                if( hr == HRESULT.S_OK )
                {
                    return true;
                }
            }

            return GetWindowRect(hTop, out rc);
        }

        private bool SnapTest_Window(int src, int dest)
        {
            return ( Math.Abs(src - dest) < Range_Window);
        }

        private bool SnapTest_Screen(int src, int dest)
        {
            return ( Math.Abs(src - dest) < Range_Screen);
        }

        private void UpdateScreenRectFromRect(RECT rcFrom)
        {
            IntPtr hMonitor;
            MonitorInfoEx mi;
            mi.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(MonitorInfoEx));

            hMonitor = MonitorFromRect(out rcFrom, MONITOR_DEFAULTTONEAREST);
            GetMonitorInfo(hMonitor, out mi);

            this.rcMonitor  = mi.rcMonitor;
            this.rcWork     = mi.rcWork;
        }

        /* ---------------------------------------------------- */
        //     Struct
        /* ---------------------------------------------------- */
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInfoEx
        {
            public int  cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int  dwFlags;
        }

        /* ---------------------------------------------------- */
        //     flags
        /* ---------------------------------------------------- */
        private const int GA_PARENT     = 1;
        private const int GA_ROOT       = 2;
        private const int GA_ROOTOWNER  = 3;

        private const uint SWP_NOSIZE        = 0x0001;
        private const uint SWP_NOMOVE        = 0x0002;
        private const uint SWP_NOZORDER      = 0x0004;
        private const uint SWP_NOACTIVATE    = 0x0010;

        private const uint MONITOR_DEFAULTTONEAREST  = 0x00000002;
        private const uint MONITOR_DEFAULTTONULL     = 0x00000000;
        private const uint MONITOR_DEFAULTTOPRIMARY  = 0x00000001;

        /* ---------------------------------------------------- */
        //     define
        /* ---------------------------------------------------- */
        enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }

        enum HRESULT : uint
        {
            S_FALSE = 0x0001,
            S_OK = 0x0000,
            E_INVALIDARG = 0x80070057,
            E_OUTOFMEMORY = 0x8007000E
        }
        /* ---------------------------------------------------- */
        //     Native Method
        /* ---------------------------------------------------- */
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT pt);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern IntPtr GetAncestor(IntPtr hwnd, int gaFlag);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("User32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, out MonitorInfoEx info);

        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromRect(out RECT lprc, uint dwFlags);

        [DllImport("dwmapi.dll")]
        static extern HRESULT DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT pvAttribute, int cbAttribute);

        // end of class
    }

}
