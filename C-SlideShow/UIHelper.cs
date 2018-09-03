using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace C_SlideShow
{
    public class UIHelper
    {
        // field
        MainWindow mainWindow;
        DispatcherTimer uIVisibleTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) ;
        int processId = 0;
        bool isCursorPaused = false;
        Point ptCursorPause = new Point(0, 0);
        int cursorPauseTime = 0;
        //int cnt = 0;


        public UIHelper(MainWindow mw)
        {
            mainWindow = mw;


            uIVisibleTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            uIVisibleTimer.Tick += UIVisibleTimer_Ticked;

        }

        private void UIVisibleTimer_Ticked(object sender, EventArgs e)
        {
#if DEBUG
            //cnt += 1;
            //Debug.WriteLine("UIVisible Timer Ticked " + cnt.ToString() );
#endif
            // 設定プロファイル
            Profile pf = mainWindow.Setting.TempProfile;

            // ウインドウ幅が狭い時は、ツールバー位置を調整
            int p1 = 490;
            if(mainWindow.Width < p1)
            {
                mainWindow.ToolbarWrapper.HorizontalAlignment = HorizontalAlignment.Left;
                mainWindow.ToolbarWrapper.Margin = new Thickness(10, 10, 80, 0);

                // ツールバー縮小
                //int p2 = 290;
                //if(mainWindow.Width < p2 )
                //{
                //    double scale = mainWindow.Width / ( p2 + 50 );
                //    this.mainWindow.ToolbarWrapper.LayoutTransform = new ScaleTransform(scale, scale);
                //}
                //else
                //{
                //    this.mainWindow.ToolbarWrapper.LayoutTransform = new ScaleTransform(1.0, 1.0);
                //}
            }
            else
            {
                mainWindow.ToolbarWrapper.HorizontalAlignment = HorizontalAlignment.Center;
                mainWindow.ToolbarWrapper.Margin = new Thickness(10, 10, 10, 0);

                // ツールバー等倍
                this.mainWindow.ToolbarWrapper.LayoutTransform = new ScaleTransform(1.0, 1.0);
            }

            // マウスボタンが押されているか(リサイズ中かどうか)チェック
            short stateL = Win32.GetKeyState(Win32.VK_LBUTTON);
            short stateR = Win32.GetKeyState(Win32.VK_RBUTTON);
            if ((stateL & 0x8000) != 0 || (stateR & 0x8000) != 0)
            {
                return;
            }

            // 現在座標(スクリーン上の)
            Win32.POINT pt = new Win32.POINT();
            Win32.GetCursorPos(ref pt);
            
            // スクリーン上のカーソル下のプロセスが、このアプリのものであるかチェック
            IntPtr hwnd = Win32.WindowFromPoint(pt);
            int pid = Win32.GetPidFromHwnd((int)hwnd);
            if (this.processId == 0)
            {
                this.processId = Win32.GetPidFromHwnd((int)new WindowInteropHelper(mainWindow).Handle);
            }

            if (pid != this.processId)
            {
                HideAllUI();
                uIVisibleTimer.Stop();
                return;
            }

            // カーソル下がMenuItem等の場合はreturn
            IntPtr hwndThis = new WindowInteropHelper(mainWindow).Handle;
            if (hwnd != hwndThis) return;


            // カーソル停止をチェックして、カーソルを隠す
            Point d = new Point(Math.Abs(pt.X - ptCursorPause.X), Math.Abs(pt.Y - ptCursorPause.Y));
            if ( !isCursorPaused && d.X == 0 && d.Y == 0)
            {
                cursorPauseTime += 1;
                if (cursorPauseTime > 15) // 1.5 sec
                {
                    mainWindow.Cursor = Cursors.None;
                    isCursorPaused = true;
                    this.uIVisibleTimer.Stop();
                    return;
                }
            }
            else
            {
                mainWindow.Cursor = null;
                ptCursorPause.X = pt.X;
                ptCursorPause.Y = pt.Y;
                cursorPauseTime = 0;
                isCursorPaused = false;
            }


            // カーソル位置によってUIの表示を変える
            Point ptWnd = Win32.GetWindowPos(hwndThis);

            var h3 = mainWindow.Height / 3;
            var w3 = mainWindow.Width / 3;

            // 拡大パネル内ファイル情報表示時
            TileExpantionPanel panel = mainWindow.TileExpantionPanel;
            if( panel.IsShowing )
            {
                // ツールバー、シークバーは常に隠す
                mainWindow.ToolbarWrapper.Visibility = Visibility.Collapsed;
                HideSeekbar();

                // 上部 1/3
                if(pt.Y < ptWnd.Y + h3 )
                {
                    mainWindow.SystemButtonWrapper.Visibility = Visibility.Visible;
                }
                else
                {
                    mainWindow.SystemButtonWrapper.Visibility = Visibility.Collapsed;
                }

                // 下部 1/3
                if( pt.Y > ptWnd.Y + (2 * h3) )
                {
                    panel.ToolbarWrapper.Visibility = Visibility.Visible;
                }
                else
                {
                    panel.ToolbarWrapper.Visibility = Visibility.Collapsed;
                }

                return;
            }


            // ツールバー、システムボタン
            double borderHeight = mainWindow.ToolbarWrapper.ActualHeight + mainWindow.ToolbarWrapper.Margin.Top;
            if (pt.Y < ptWnd.Y + h3 || pt.Y < ptWnd.Y + borderHeight)
            {
                mainWindow.ToolbarWrapper.Visibility = Visibility.Visible;
                mainWindow.SystemButtonWrapper.Visibility = Visibility.Visible;
            }
            else
            {
                if(!mainWindow.IsToolbarMenuOpened)
                    mainWindow.ToolbarWrapper.Visibility = Visibility.Hidden;
                mainWindow.SystemButtonWrapper.Visibility = Visibility.Hidden;
            }

            // シークバー
            if (mainWindow.IsHorizontalSlide)
            {
                // 水平
                if (pt.Y > ptWnd.Y + h3 * 2) ShowSeekbar();
                else HideSeekbar();
            }
            else
            {
                // 垂直
                if (pt.X > ptWnd.X + w3 * 2) ShowSeekbar();
                else HideSeekbar();
            }

        }
        

        public void HideAllUI()
        {
            if(!mainWindow.IsToolbarMenuOpened)
                mainWindow.ToolbarWrapper.Visibility = Visibility.Hidden;
            mainWindow.SystemButtonWrapper.Visibility = Visibility.Hidden;
            //mainWindow.ResizeGrip.Visibility = Visibility.Hidden;
            //mainWindow.BaseGrid.Opacity = mainWindow.Setting.TempProfile.BaseGridOpacity;
            mainWindow.PageInfo.Visibility = Visibility.Hidden;
            mainWindow.SeekbarWrapper.Visibility = Visibility.Hidden;
            //mainWindow.TileExpantionPanel.FileInfoGrid.Visibility = Visibility.Collapsed;
            mainWindow.TileExpantionPanel.ToolbarWrapper.Visibility = Visibility.Collapsed;
        }

        public void ShowSeekbar()
        {
            mainWindow.PageInfo.Visibility = Visibility.Visible;
            mainWindow.SeekbarWrapper.Visibility = Visibility.Visible;
        }

        public void HideSeekbar()
        {
            mainWindow.PageInfo.Visibility = Visibility.Hidden;
            mainWindow.SeekbarWrapper.Visibility = Visibility.Hidden;
        }

        /* ---------------------------------------------------- */
        //   リサイズ機能
        //   @ref http://hogetan.blog24.fc2.com/blog-entry-7.html
        //   @note WindowのResizeModeをCanResizeにしていないと、機能しない
        //         枠はWM_NCCALCSIZEをフックして消す
        /* ---------------------------------------------------- */
        private const int WM_NCHITTEST = 0x0084;
         
        // WM_NCHITTEST and MOUSEHOOKSTRUCT Mouse Position Codes
        public const int HTERROR = (-2);
        public const int HTTRANSPARENT = (-1);
        public const int HTNOWHERE = 0;
        public const int HTCLIENT = 1;
        public const int HTCAPTION = 2;
        public const int HTSYSMENU = 3;
        public const int HTGROWBOX = 4;
        public const int HTSIZE = HTGROWBOX;
        public const int HTMENU = 5;
        public const int HTHSCROLL = 6;
        public const int HTVSCROLL = 7;
        public const int HTMINBUTTON = 8;
        public const int HTMAXBUTTON = 9;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;
        public const int HTBORDER = 18;
        public const int HTREDUCE = HTMINBUTTON;
        public const int HTZOOM = HTMAXBUTTON;
        public const int HTSIZEFIRST = HTLEFT;
        public const int HTSIZELAST = HTBOTTOMRIGHT;
        public const int HTOBJECT = 19;
        public const int HTCLOSE = 20;
        public const int HTHELP = 21;

        const int WM_SIZING = 0x214;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int WM_SYSCOMMAND = 0x0112;
        const int WM_SETCURSOR = 0x20;
        const int WM_ENTERSIZEMOVE = 0x0231;
        const int WM_EXITSIZEMOVE = 0x0232;
        const int WM_NCCALCSIZE = 0x0083;
        const int WM_NCACTIVATE = 0x0086;
        const int WM_MOUSEMOVE = 0x0200;

        const int SC_MOVE = 0xF010;
        const int SC_SIZE = 0xF000;

        const int WMSZ_LEFT = 1;
        const int WMSZ_RIGHT =2;
        const int WMSZ_TOP = 3;
        const int WMSZ_TOPLEFT =4;
        const int WMSZ_TOPRIGHT =5;
        const int WMSZ_BOTTOM =6;
        const int WMSZ_BOTTOMLEFT =7;
        const int WMSZ_BOTTOMRIGHT = 8;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }




        //NCCALCSIZE_PARAMS Structure
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct NCCALCSIZE_PARAMS
        {
            public RECT rgrc0, rgrc1, rgrc2;
            public WINDOWPOS lppos;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndinsertafter;
            public int x, y, cx, cy;
            public int flags;
    }

        public IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if(msg == WM_MOUSEMOVE )
            {
                //Debug.WriteLine("WM_MOUSEMOVE");

                // カーソル停止からの復帰
                if( isCursorPaused )
                {
                    Win32.POINT pt = new Win32.POINT();
                    Win32.GetCursorPos(ref pt);
                    if(pt.X != ptCursorPause.X && pt.Y != ptCursorPause.Y )
                    {
                        mainWindow.Cursor = null;
                        isCursorPaused = false;
                        cursorPauseTime = 0;
                        this.uIVisibleTimer.Start();
                    }
                }
            }

            if(msg == WM_NCACTIVATE)
            {
                // wParam:アクティブになった時1 非アクティブになった時0
                // lParam: 非アクティブにさせたウインドウハンドル
                Debug.WriteLine("WM_NCACTIVATE  " + "wParam: " + wParam + "  lParam: " + lParam);

                // ウインドウ切り替えにより、非アクティブにした時に、非アクティブ用の枠を表示させない
                if (lParam == new IntPtr(0))
                {
                    handled = true;
                    return new IntPtr(0);
                }
                else
                {
                    if(wParam != new IntPtr(0))
                    {
                        // フォルダ指定ダイアログ等を表示した為、非アクティブになり、戻った時
                        // 再描画し、非アクティブ用のウインドウ枠を消す
                        // handled をtrueにしない(するとフリーズしてしまう)
                        Win32.InvalidateRect(hwnd, IntPtr.Zero, false);
                        return new IntPtr(0);
                    }
                }

                //this.mainWindow.Dispatcher.Invoke(DispatcherPriority.Render, 
                //    new Action( () => { }));

                //mainWindow.UpdateWindowSize();
                //Win32.PostMessage(hwnd, WM_SIZING, new IntPtr(0), new IntPtr(0));
            }

            if(msg == WM_NCCALCSIZE)
            {
                // クライアント領域を、ウインドウ枠を覆うまで広げる
                handled = true;
                return new IntPtr(0);
            }


            if (msg == WM_NCHITTEST)
            {
                // カーソル停止中は処理しない
                if (isCursorPaused)
                {
                    handled = false;
                    return new IntPtr(HTCLIENT);
                }

                // handled により、MouseEnterが呼ばれない為、ここでUI表示
                if (!uIVisibleTimer.IsEnabled)
                {
                    //ShowExceptToolbar();
                    uIVisibleTimer.Start();
                    this.UIVisibleTimer_Ticked(this, null);
                }

                // これ以上処理させない（完全に処理を横取りする）
                handled = true;

                // フルスクリーンモード中はリサイズの必要がないので、リターン
                if (mainWindow.Setting.TempProfile.IsFullScreenMode.Value)
                {
                    handled = false;
                    return new IntPtr(HTCLIENT);
                }
         
                // クライアント座標のマウス位置を取得
                Point ptScreen = new Point((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF);
                Point ptClient = mainWindow.PointFromScreen(ptScreen);

         
                // リサイズ可能と判断するサイズ
                //double bh = SystemParameters.ResizeFrameHorizontalBorderHeight;
                //double bw = SystemParameters.ResizeFrameVerticalBorderWidth;
                //double captionH = SystemParameters.CaptionHeight;
                double bh = 10;
                double bw = 10;

                // 四隅の斜め方向リサイズが優先
                int hit = -1;
                if (new Rect(0, 0, bw, bh).Contains(ptClient)) hit = HTTOPLEFT;
                else if (new Rect(mainWindow.Width - bw, 0, bw, bh).Contains(ptClient)) hit = HTTOPRIGHT;
                else if (new Rect(0, mainWindow.Height - bh, bw, bh).Contains(ptClient)) hit = HTBOTTOMLEFT;
                else if (new Rect(mainWindow.Width - bw, mainWindow.Height - bh, bw, bh).Contains(ptClient)) hit = HTBOTTOMRIGHT;

                // 四辺の直交方向リサイズ
                else if (new Rect(0, 0, mainWindow.Width, bw).Contains(ptClient)) hit = HTTOP;
                else if (new Rect(0, 0, bw, mainWindow.Height).Contains(ptClient)) hit = HTLEFT;
                else if (new Rect(mainWindow.Width - bw, 0, bw, mainWindow.Height).Contains(ptClient)) hit = HTRIGHT;
                else if (new Rect(0, mainWindow.Height - bh, mainWindow.Width, bh).Contains(ptClient)) hit = HTBOTTOM;

                if (hit != -1)
                {
                    //return new IntPtr(HTCLIENT);
                    return new IntPtr(hit);
                }


                // ドラッグ移動可能な領域を指定
                //if (new Rect(0, 0, Width, captionH).Contains(ptClient)) return new IntPtr(HTCAPTION);


                // 上記以外はクライアント領域と判断
                handled = false;
                return new IntPtr(HTCLIENT);

            }

            /* ---------------------------------------------------- */
            //    メインウインドウ アス比固定処理 
            /* ---------------------------------------------------- */
            if (msg == WM_SIZING && !mainWindow.Setting.TempProfile.NonFixAspectRatio.Value)
            {
                RECT r = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                int w = r.right - r.left;
                int h = r.bottom - r.top;
                w -= (int)mainWindow.MainContent.Margin.Left * 2;
                h -= (int)mainWindow.MainContent.Margin.Top * 2;
                int dw = (int)(h * mainWindow.MainContentAspectRatio + 0.5) - w;
                int dh = (int)(w / mainWindow.MainContentAspectRatio + 0.5) - h;
                switch( wParam.ToInt32() )
                {
                    case WMSZ_TOP:
                    case WMSZ_BOTTOM:
                        r.right += dw;
                        break;
                    case WMSZ_LEFT:
                    case WMSZ_RIGHT:
                        r.bottom += dh;
                        break;
                    case WMSZ_TOPLEFT:
                        if( dw > 0 ) r.left -= dw;
                        else r.top -= dh;
                        break;
                    case WMSZ_TOPRIGHT:
                        if( dw > 0 ) r.right += dw;
                        else r.top -= dh;
                        break;
                    case WMSZ_BOTTOMLEFT:
                        if( dw > 0 ) r.left -= dw;
                        else r.bottom += dh;
                        break;
                    case WMSZ_BOTTOMRIGHT:
                        if( dw > 0 ) r.right += dw;
                        else r.bottom += dh;
                        break;
                }
                Marshal.StructureToPtr(r, lParam, false);
            }
         
            return IntPtr.Zero;
        }




    }
}
