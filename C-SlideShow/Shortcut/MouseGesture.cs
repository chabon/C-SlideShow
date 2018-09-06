using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Interop;
using System.Diagnostics;
using System.Windows.Input;


namespace C_SlideShow.Shortcut
{
    /// <summary>
    /// マウスジェスチャ
    /// @ref http://www.atelier-blue.com/memo/memo2006-5-4-2.htm
    /// </summary>
    public class MouseGesture
    {
        #region インラインクラス
        /// 方向
        public enum Arrow
        {
            None    = -1,     // 移動無し
            Up      = 0,      // 上への移動
            Right   = 1,      // 右への移動
            Down    = 2,      // 下への移動
            Left    = 3,      // 左への移動
        }

        /// <summary>
        /// 一定の方向に関する情報を持つ
        /// </summary>
        public class DirectionInfo
        {
            /// <summary>
            /// 有効無効
            /// </summary>
            public bool Enable;
            
            /// <summary>
            /// 累計移動距離
            /// </summary>
            public double Length;

            /// <summary>
            /// リセットします。
            /// </summary>
            public void Reset()
            {
                Enable = true;
                Length = 0;
            }

            public DirectionInfo()
            {
                Reset();
            }
        }    
        #endregion

        /// <summary>
        /// マウスジェスチャの有効無効
        /// </summary>
        private bool isActive;
        public bool IsActive { get { return isActive; } }
        
        /// <summary>
        /// 判定距離
        /// </summary>
        public int Range { get; set; }

        /// <summary>
        /// ドラッグによるジェスチャを有効
        /// </summary>
        public bool EnableDragGesture { get; set; }

        /// <summary>
        /// ジェスチャのストローク
        /// </summary>
        private string stroke;
        public string Stroke { get { return stroke; } }

        /// <summary>
        /// 四方向それぞれについての情報(要素の大きさは４)
        /// </summary>
        private DirectionInfo[] directionInfo;
        
        /// <summary>
        /// 移動距離計算用の古い位置(スクリーン座標系)
        /// </summary>
        private Point oldPos;

        /// <summary>
        /// 始動ボタン
        /// </summary>
        private MouseButton startingButton;
        public MouseButton StartingButton { get { return startingButton; } }

        /// <summary>
        /// 始動ボタンリリース時のウインドウズメッセージ
        /// </summary>
        private int WMessageStartingButtonUp;

        /// <summary>
        /// ジェスチャ中のクリックをジェスチャとみなすかどうか
        /// </summary>
        public bool AllowHoldClick { get; set; }

        /// <summary>
        /// フックチェーンにインストールするフックプロシージャのイベント
        /// </summary>
        private event HOOKPROC hookCallback;

        /// <summary>
        /// フックハンドル
        /// </summary>
        private IntPtr hHook = IntPtr.Zero;

        /// <summary>
        /// ジェスチャストローク更新時イベント
        /// </summary>
        public event EventHandler StrokeChanged;

        /// <summary>
        /// ホールドクリック(ジェスチャー開始後のクリック)イベント
        /// </summary>
        public event EventHandler HoldClick;

        /// <summary>
        /// ジェスチャ完了イベント
        /// </summary>
        public event EventHandler GestureFinished;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MouseGesture()
        {
            isActive = false;
            EnableDragGesture = true;
            AllowHoldClick = true;
            stroke = "";
            directionInfo = new DirectionInfo[4];
            for(int i=0; i<4; i++)
            {
                directionInfo[i] =new DirectionInfo();
            }
            Range = 15;

            // フックプロシージャ
            hookCallback += HookProc;
        }

        /// <summary>
        /// マウスジェスチャの開始
        /// </summary>
        public void Start(MouseButton startingButton)
        {
            if(hHook != IntPtr.Zero)
            {
                UnHook();
                return;
            }

            isActive = true;
            stroke = "";
            ResetDirection();
            oldPos = GetCursorPos();
            this.startingButton = startingButton;

            switch( startingButton )
            {
                case MouseButton.Left:
                    WMessageStartingButtonUp = WM_LBUTTONUP;
                    break;
                case MouseButton.Right:
                    WMessageStartingButtonUp = WM_RBUTTONUP;
                    break;
                case MouseButton.Middle:
                    WMessageStartingButtonUp = WM_MBUTTONUP;
                    break;
                case MouseButton.XButton1:
                case MouseButton.XButton2:
                    WMessageStartingButtonUp = WM_XBUTTONUP;
                    break;
                default:
                    isActive = false;
                    return;
            }

            SetHook();
            Debug.WriteLine("mouse gesture start");
        }


        /// <summary>
        /// マウスジェスチャの終わり
        /// </summary>
        /// <returns>終了時のジェスチャのストローク</returns>
        public string End()
        {
            UnHook();

            if(isActive)
            {
                isActive = false;
                Debug.WriteLine("mouse gesture end  stroke:" + this.stroke);
                GestureFinished?.Invoke( this, new EventArgs() );
                return stroke;
            }

            return "";
        }

        /// <summary>
        /// マウスジェスチャの判定
        /// </summary>
        public void Test()
        {    
            //有効なときだけ判定する。
            if(isActive)
            {
                double ox = oldPos.X, oy = oldPos.Y;
                Arrow arrow = Arrow.None;

                // 現在のカーソル位置(スクリーン座標系)を取得
                Point pos = GetCursorPos();

                // 情報を入れ替えておく
                oldPos = pos;
        
                //移動量を判定して縦横どっちに動くかを判定
                if(Math.Abs(ox - pos.X) > Math.Abs(oy - pos.Y))
                {
                    if(ox > pos.X)
                    {
                        directionInfo[(int)Arrow.Left].Length += ox - pos.X;
                        directionInfo[(int)Arrow.Right].Length = 0;
                        arrow = Arrow.Left;
                    }
                    else if(pos.X >ox)
                    {
                        directionInfo[(int)Arrow.Right].Length += pos.X - ox;
                        directionInfo[(int)Arrow.Left].Length = 0;
                        arrow = Arrow.Right;
                    }
                }
                else
                {
                    if(oy > pos.Y)
                    {
                        directionInfo[(int)Arrow.Up].Length += oy - pos.Y;
                        directionInfo[(int)Arrow.Down].Length = 0;
                        arrow = Arrow.Up;
                    }
                    else if(pos.Y >oy)
                    {
                        directionInfo[(int)Arrow.Down].Length += pos.Y - oy;
                        directionInfo[(int)Arrow.Up].Length = 0;
                        arrow = Arrow.Down;
                    }
                }
    
                //移動を検知したとき
                if(arrow != Arrow.None)
                {
                    if(directionInfo[(int)arrow].Enable && directionInfo[(int)arrow].Length > Range)
                    {
                        ResetDirection();
            
                        //同じ向きが２度入力されないようにする。
                        directionInfo[(int)arrow].Enable = false;
                    
                        stroke += ArrowToString(arrow);
                        Debug.WriteLine("MouseGesture Stroke: " + stroke);
                        StrokeChanged?.Invoke( this, new EventArgs() );
                    }
                }
            }
        }

        /// <summary>
        /// Arrow列挙体をストローク用のstringに変換
        /// </summary>
        /// <returns></returns>
        private string ArrowToString(Arrow arrow)
        {
            switch( arrow )
            {
                default:
                case Arrow.None:
                    return "";
                case Arrow.Left:
                    return "←";
                case Arrow.Up:
                    return "↑";
                case Arrow.Right:
                    return "→";
                case Arrow.Down:
                    return "↓";
            }
        }

        /// <summary>
        /// ４方向の情報をリセットする
        /// </summary>
        private void ResetDirection()
        {
            for(int i=0; i<4; i++)
            {
                directionInfo[i].Reset();
            }
        }

        /// <summary>
        /// ストロークの最後がクリックならば削除する
        /// </summary>
        public void RemoveLastClickStroke()
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"\[.{1,2}\]$");
            stroke = reg.Replace(stroke, "");
        }
        　 
        /// <summary>
        /// スクリーン上でのMouseMoveイベント、MouseRButtonUpイベント取得のためのグローバルフック
        /// </summary>
        /// <returns></returns>
        private int SetHook()
        {
            IntPtr hmodule = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);

            hHook = SetWindowsHookEx((int)HookType.WH_MOUSE_LL, hookCallback, hmodule, IntPtr.Zero);

            if (hHook == null)
            {
                //MessageBox.Show("SetWindowsHookEx 失敗", "Error");
                return -1;
            }
            else
            {
                //MessageBox.Show("SetWindowsHookEx 成功", "OK");
                return 0;
            }
        }

        public void UnHook()
        {
            if(hHook != IntPtr.Zero) UnhookWindowsHookEx(hHook);
            hHook = IntPtr.Zero;
        }

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if(isActive && nCode >= HC_ACTION)
            {
                Debug.WriteLine( "WMessage in HookProc  wParam:" + ( (int)wParam ).ToString() + "  lParam:" + ( (int)lParam ).ToString()  );

                // マウス移動時は、軌道のチェック
                if( (int)wParam == WM_MOUSEMOVE )
                {
                    if(EnableDragGesture) this.Test();
                }

                // 始動ボタンが離されたらジェスチャ終了
                else if ( IsWMParamMatchesStartingButtonUp(wParam, lParam) )
                {
                    End();
                }

                else if( AllowHoldClick )
                {
                    // クリックストローク取得 (Mouse Button Down Message)
                    string cs = GetClickStroke(wParam, lParam);
                    if( cs.Length > 0 && !stroke.EndsWith(cs) ) // 2重押下の防止
                    {
                        stroke = stroke + cs;
                        ResetDirection();
                        StrokeChanged?.Invoke( this, new EventArgs() );
                    }

                    // ホールドクリック実行 (Mouse Button Up Message, Wheel Up Down Message)
                    if(cs.Length == 0 && (int)wParam != WMessageStartingButtonUp)
                    {
                        TryHoldClick(wParam, lParam);
                    }
                }
            }

            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        private bool IsWMParamMatchesStartingButtonUp(IntPtr wParam, IntPtr lParam)
        {
            if( (int)wParam == WMessageStartingButtonUp )
            {
                if( (int)wParam == WM_XBUTTONUP )
                {
                    MSLLHOOKSTRUCT mouseHookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    if(mouseHookStruct.mouseData >> 16 == 1 ) // X1(戻るボタン)
                    {
                        Debug.WriteLine("Starting Button Up: Mouse XButton1 (戻るボタン)");
                        if( startingButton == MouseButton.XButton1 ) return true;
                    }
                    else  // X2(進むボタン)
                    {
                        Debug.WriteLine("Starting Button Up: Mouse XButton2 (進むボタン) Up");
                        if( startingButton == MouseButton.XButton2 ) return true;
                    }
                }
                else
                {
                    Debug.WriteLine( "Starting Button Up: " + WMessageStartingButtonUp.ToString() );
                    return true;
                }
            }
            return false;
        }

        private string GetClickStroke(IntPtr wParam, IntPtr lParam)
        {
            string clickStroke = "";
            MSLLHOOKSTRUCT mouseHookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

            switch( (int)wParam )
            {
                case WM_LBUTTONDOWN:
                    clickStroke =  "[L]";
                    break;
                case WM_RBUTTONDOWN:
                    clickStroke = "[R]";
                    break;
                case WM_MBUTTONDOWN:
                    clickStroke = "[M]";
                    break;
                case WM_XBUTTONDOWN:
                    if(mouseHookStruct.mouseData >> 16 == 1 ) // X1(戻るボタン)
                    {
                        clickStroke = "[X1]";
                    }
                    else  // X2(進むボタン)
                    {
                        clickStroke = "[X2]";
                    }
                    break;
            }

            return clickStroke;
        }

        private void TryHoldClick(IntPtr wParam, IntPtr lParam)
        {
            MSLLHOOKSTRUCT mouseHookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            string strEnd;

            switch( (int)wParam )
            {
                case WM_LBUTTONUP:
                    strEnd = "[L]";
                    break;
                case WM_RBUTTONUP:
                    strEnd = "[R]";
                    break;
                case WM_MBUTTONUP:
                    strEnd = "[M]";
                    break;
                case WM_XBUTTONUP:
                    if(mouseHookStruct.mouseData >> 16 == 1 ) // X1(戻るボタン)
                    {
                        strEnd = "[X1]";
                    }
                    else  // X2(進むボタン)
                    {
                        strEnd = "[X2]";
                    }
                    break;
                case WM_MOUSEWHEEL:
                    if( (short)((mouseHookStruct.mouseData >> 16) & 0xffff) > 0 ) // Wheel Up
                    {
                        if( !stroke.EndsWith("[WU]") )
                        {
                            stroke = stroke + "[WU]";
                            StrokeChanged?.Invoke( this, new EventArgs() );
                        }
                        strEnd = "[WU]";
                    }
                    else  // Wheel Down
                    {
                        if( !stroke.EndsWith("[WD]") )
                        {
                            stroke = stroke + "[WD]";
                            StrokeChanged?.Invoke( this, new EventArgs() );
                        }
                        strEnd = "[WD]";
                    }
                    break;
                default:
                    return;
            }

            if( stroke.EndsWith(strEnd) ) HoldClick?.Invoke( this, new EventArgs() );
        }



        /* ---------------------------------------------------- */
        //     Win32 API
        /* ---------------------------------------------------- */
        // Message
        public const int WM_LBUTTONDOWN  = 0x0201;  // 513
        public const int WM_RBUTTONDOWN  = 0x0204;  // 516
        public const int WM_MBUTTONDOWN  = 0x0207;  // 519
        public const int WM_XBUTTONDOWN  = 0x020B;  // 523

        public const int WM_LBUTTONUP  = 0x0202;    // 514
        public const int WM_RBUTTONUP  = 0x0205;    // 517
        public const int WM_MBUTTONUP  = 0x0208;    // 520
        public const int WM_XBUTTONUP  = 0x020C;    // 524

        public const int WM_MOUSEWHEEL = 0x020A;    // 522
        

        public const int WM_MOUSEMOVE  = 0x0200;

        private static readonly IntPtr LRESULTCancel = new IntPtr(1);


        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref POINT pt);

        private static Point GetCursorPos()
        {
            POINT w32Mouse = new POINT();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HOOKPROC lpfn, IntPtr hMod, IntPtr dwThreadId);
        private const int HC_ACTION = 0;
        private delegate IntPtr HOOKPROC(int nCode, IntPtr wParam, IntPtr lParam);

        private enum HookType : int
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
        private struct MSLLHOOKSTRUCT
        {
             public POINT pt;
             public int mouseData;
             public int flags;
             public int time;
             public UIntPtr dwExtraInfo;
        }


        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook);


        // end of class
    }

}
