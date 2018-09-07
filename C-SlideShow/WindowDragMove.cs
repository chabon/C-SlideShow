using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace C_SlideShow
{
    public class WindowDragMove
    {
        // フィールド
        private Window targetWindow;
        private bool   bDragStart = false;
        private Point  ptDragStart;
        private Point  ptWindowPrev;
        private IntPtr hHook = IntPtr.Zero;
        private event Win32.HOOKPROC hookCallback;

        private Point  ptMaxDiff; // ドラッグ開始時からの最大移動量
        private const double thresholdOfMaxDiff = 0.5; // DragMovedイベントを発生させるしきい値

        // プロパティ
        public WindowSnap WindowSnap { get; private set; }
        public Func<bool> CanDragStart { private get; set; }

        // イベント
        public event EventHandler DragMoved;
        public event EventHandler DragStart;

        // コンストラクタ
        public WindowDragMove(Window window)
        {
            targetWindow = window;
            targetWindow.MouseLeftButtonDown += TargetWindow_MouseLeftButtonDown;
            targetWindow.Closing += (s, e) => { UnHook(); };
            hookCallback += HookProc;
        }

        private void TargetWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if( CanDragStart != null && !CanDragStart.Invoke() ) return;

            if(hHook == IntPtr.Zero )
            {
                ptDragStart  = Win32.GetCursorPos();
                ptWindowPrev = new Point(targetWindow.Left, targetWindow.Top);
                ptMaxDiff    = new Point(0, 0);
                bDragStart   = true;
                if(WindowSnap == null) WindowSnap = new WindowSnap(targetWindow);
                SetHook();
                DragStart?.Invoke( this, new EventArgs() );
            }
            else
            {
                UnHook();
            }
        }

        private void DragFinish()
        {
            bDragStart = false;
            UnHook();
            if( ptMaxDiff.X > thresholdOfMaxDiff || ptMaxDiff.Y > thresholdOfMaxDiff )
            {
                DragMoved?.Invoke( this, new EventArgs() );
            }
        }

        private int SetHook()
        {
            IntPtr hmodule = Win32.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);

            hHook = Win32.SetWindowsHookEx((int)Win32.HookType.WH_MOUSE_LL, hookCallback, hmodule, IntPtr.Zero);

            if (hHook == null) { return -1; }
            else { return 0; }
        }

        public void UnHook()
        {
            if(hHook != IntPtr.Zero) Win32.UnhookWindowsHookEx(hHook);
            hHook = IntPtr.Zero;
        }

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if( bDragStart )
            {
                if( targetWindow.IsActive )
                {
                    if( (int)wParam == Win32.WM_MOUSEMOVE )
                    {
                        Point ptCurrent = Win32.GetCursorPos();
                        Point ptDiff    = new Point(ptCurrent.X - ptDragStart.X, ptCurrent.Y - ptDragStart.Y);

                        if( ptMaxDiff.X < Math.Abs(ptDiff.X) ) ptMaxDiff.X = Math.Abs(ptDiff.X);
                        if( ptMaxDiff.Y < Math.Abs(ptDiff.Y) ) ptMaxDiff.Y = Math.Abs(ptDiff.Y);

                        Rect rcDest = new Rect() { X = ptWindowPrev.X + ptDiff.X, Y = ptWindowPrev.Y + ptDiff.Y, Width  = targetWindow.Width, Height = targetWindow.Height }; 

                        if( !WindowSnap.OnWindowMoving(rcDest) )
                        {
                            targetWindow.Left = ptWindowPrev.X + ptDiff.X;
                            targetWindow.Top  = ptWindowPrev.Y + ptDiff.Y;
                        }
                    }
                }
                else
                {
                    DragFinish();
                }

                if( (int)wParam == Win32.WM_LBUTTONUP )
                {
                    DragFinish();
                }
            }

            return Win32.CallNextHookEx(hHook, nCode, wParam, lParam);
        }
    }
}
