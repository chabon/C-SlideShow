using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace C_SlideShow.Shortcut.Drag
{
    public class Drag
    {
        // フィールド
        protected Window targetWindow;
        protected bool   bDragStart = false;
        protected Point  ptDragStart;           // ドラッグ開始時のカーソル位置(スクリーン座標系。以下2つも同様)
        protected Point  ptDragMoving;          // ドラッグ中のカーソル位置
        protected Point  ptDragMovingDiff;      // ドラッグ開始時の位置との差分

        private IntPtr hHook = IntPtr.Zero;
        private event Win32.HOOKPROC hookCallback;

        protected Point  ptMaxDiff; // ドラッグ開始時からの最大移動量
        protected const double thresholdOfMaxDiff = 0.5; // DragMovedイベントを発生させるしきい値

        // プロパティ
        public Func<bool> CanDragStart { private get; set; }

        // イベント
        public event EventHandler DragStart;
        public event EventHandler DragMoving;
        public event EventHandler DragMoved;

        // コンストラクタ
        public Drag(Window window)
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
                ptMaxDiff    = new Point(0, 0);
                bDragStart   = true;
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
                if( targetWindow.IsActive)
                {
                    if( (int)wParam == Win32.WM_MOUSEMOVE )
                    {
                        ptDragMoving        = Win32.GetCursorPos();
                        ptDragMovingDiff    = new Point(ptDragMoving.X - ptDragStart.X, ptDragMoving.Y - ptDragStart.Y);

                        if( ptMaxDiff.X < Math.Abs(ptDragMovingDiff.X) ) ptMaxDiff.X = Math.Abs(ptDragMovingDiff.X);
                        if( ptMaxDiff.Y < Math.Abs(ptDragMovingDiff.Y) ) ptMaxDiff.Y = Math.Abs(ptDragMovingDiff.Y);

                        DragMoving?.Invoke( this, new EventArgs() );

                        if( Win32.GetKeyState(Win32.VK_LBUTTON) >= 0 ) DragFinish();
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
