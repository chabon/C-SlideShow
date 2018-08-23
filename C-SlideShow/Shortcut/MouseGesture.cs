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
		private bool enable;
        public bool Enable { get { return enable; } }
		
		/// <summary>
		/// 判定距離
		/// </summary>
		private int range;
		public int Range
		{
			get { return range; }
			set { range = value; }
		}

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
        /// フックチェーンにインストールするフックプロシージャのイベント
        /// </summary>
        private event HOOKPROC hookCallback;

        /// <summary>
        /// フックハンドル
        /// </summary>
        private IntPtr hHook;

        /// <summary>
        /// ジェスチャストローク更新時イベント
        /// </summary>
        public event EventHandler StrokeChanged;

        /// <summary>
        /// ジェスチャ完了イベント
        /// </summary>
        public event EventHandler GestureFinished;

        /// <summary>
        /// コンストラクタ
        /// </summary>
		public MouseGesture()
		{
			enable = false;
            stroke = "";
			directionInfo = new DirectionInfo[4];
			for(int i=0; i<4; i++)
			{
				directionInfo[i] =new DirectionInfo();
			}
			range = 15;
            SetHook();
		}

		/// <summary>
		/// マウスジェスチャの開始
		/// </summary>
		public void Start()
		{
			enable = true;
            stroke = "";
			ResetDirection();
			oldPos = GetCursorPos();
        }


        /// <summary>
		/// マウスジェスチャの終わり
        /// </summary>
        /// <returns>終了時のジェスチャのストローク</returns>
		public string End()
		{
			if(enable)
			{
				enable = false;
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
			if(enable)
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
					if(directionInfo[(int)arrow].Enable && directionInfo[(int)arrow].Length > range)
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
        /// スクリーン上でのMouseMoveイベント、MouseRButtonUpイベント取得のためのグローバルフック
        /// </summary>
        /// <returns></returns>
        private int SetHook()
        {
            IntPtr hmodule = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);

            hookCallback += HookProc;
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

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if(enable)
            {
                if ( (int)wParam == 0x0205 ) // WM_RBUTTONUP
                {
                    Debug.WriteLine(string.Format("Mouse R Button up"));
                    End();
                }

                else if( (int)wParam == 0x0200 ) // WM_MOUSEMOVE
                {
                    //MSLLHOOKSTRUCT MouseHookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    //Debug.WriteLine(string.Format("Mouse Position : {0:d}, {1:d}", MouseHookStruct.pt.X, MouseHookStruct.pt.Y));
                    this.Test();
                }
            }

            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        public void UnHook()
        {
            UnhookWindowsHookEx(hHook);
        }

        /* ---------------------------------------------------- */
        //     Win32 API
        /* ---------------------------------------------------- */
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
