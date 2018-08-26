using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Threading;

using C_SlideShow.Shortcut.Command;
using C_SlideShow.CommonControl;


namespace C_SlideShow.Shortcut
{
    /// <summary>
    /// マウスボタンを押すときと離す時、どちらもウインドウ内でなければコマンドを発行しないこと
    /// マウスボタンを押している最中に他のインプット方法によってコマンドが実行された場合、離した時の単クリックによるコマンドはキャンセルすること
    /// の２つを管理するための値
    /// </summary>
    public class MouseButtonHoldState
    {
        public bool IsPressed;
        public bool CommandExecuted;
    }

    /// <summary>
    /// ショートカット全般を管理
    /// </summary>
    public class ShortcutManager
    {
        // コマンドリスト
        List<ICommand> commands;

        // ショートカット設定
        ShortcutSetting shortcutSetting;

        // マウスジェスチャー管理
        MouseGesture mouseGesture;

        // マウスボタンの状態
        MouseButtonHoldState mouseButtonHoldState_L  = new MouseButtonHoldState();
        MouseButtonHoldState mouseButtonHoldState_R  = new MouseButtonHoldState();
        MouseButtonHoldState mouseButtonHoldState_M  = new MouseButtonHoldState();
        MouseButtonHoldState mouseButtonHoldState_X1 = new MouseButtonHoldState();
        MouseButtonHoldState mouseButtonHoldState_X2 = new MouseButtonHoldState();

        //// ウインドウドラッグの準備
        //bool mainWindowDragMoveReady = false;

        // 左クリックでのウインドウドラッグを管理
        WindowDragMove windowDragMove;

        /* ---------------------------------------------------- */
        //       Constructor
        /* ---------------------------------------------------- */
        public ShortcutManager()
        {
            // 初期化処理
            InitEventHandler();

            // 設定のロード
            shortcutSetting = MainWindow.Current.Setting.ShortcutSetting;
        }

        /* ---------------------------------------------------- */
        //       Initialize
        /* ---------------------------------------------------- */
        private void InitEventHandler()
        {
            // キー
            MainWindow.Current.KeyDown += this.MainWindow_KeyDown;

            // マウスインプット マウスジェスチャー
            MainWindow.Current.MouseWheel    += this.MainWindow_MouseWheel;
            MainWindow.Current.MouseDown     += this.MainWindow_MouseDown;
            MainWindow.Current.MouseMove     += this.MainWindow_MouseMove;
            MainWindow.Current.MouseUp       += this.MainWindow_MouseUp;
            MainWindow.Current.MouseDoubleClick   += this.MainWindow_MouseDoubleClick;

            // 左クリック後、ドラッグでウインドウドラッグ可能に
            windowDragMove = new WindowDragMove(MainWindow.Current);
            windowDragMove.DragMoved += (s, e) => { mouseButtonHoldState_L.CommandExecuted = true; };
        }

        private void InitMouseGesture()
        {
            mouseGesture = new MouseGesture();
            mouseGesture.StrokeChanged += MouseGestureStrokeChanged;
            mouseGesture.GestureFinished += MouseGestureFinished;
            mouseGesture.Range = MainWindow.Current.Setting.MouseGestureRange;
        }

        /* ---------------------------------------------------- */
        //       Method
        /* ---------------------------------------------------- */
        // コマンド作成
        private void CreateCommands()
        {
            commands = new List<ICommand>();

            // 全般
            commands.Add( new OpenFolder() );
            commands.Add( new OpenAdditionalFolder() );
            commands.Add( new OpenFile() );
            commands.Add( new OpenAdditionalFile() );
            commands.Add( new WindowSizeUp() );
            commands.Add( new WindowSizeDown() );

            // 通常時
            commands.Add( new SlideToForward() );
            commands.Add( new SlideToBackward() );
            commands.Add( new SlideToForwardByOneImage() );
            commands.Add( new SlideToBackwardByOneImage() );
            commands.Add( new SlideToLeft() );
            commands.Add( new SlideToRight() );
            commands.Add( new SlideToTop() );
            commands.Add( new SlideToBottom() );
            commands.Add( new ZoomImageUnderCursor() );

            // 画像拡大時
            commands.Add( new ZoomInImage() );
            commands.Add( new ZoomOutImage() );
            commands.Add( new ExitZoom() );
        }
        
        // IDからコマンド取得
        private ICommand GetCommand(CommandID id)
        {
            if( commands == null ) CreateCommands();

            foreach (ICommand command in commands)
            {
                if (command.ID == id) {
                    return command;
                }
            }

            return null;
        }

        // コマンドIDから実行
        public void ExecuteCommand(CommandID id)
        {
            if( commands == null ) CreateCommands();

            ICommand command = GetCommand(id);
            if( command != null )
            {
                if( command.CanExecute() ) command.Execute();
            }
        }

        // 現在のシーン取得
        private Scene GetCurrentScene()
        {
            if( MainWindow.Current.TileExpantionPanel.IsShowing )
                return Scene.Expand;
            else
                return Scene.Nomal;
        }

        // コマンドリスト取得
        public List<ICommand> GetCommandList()
        {
            if( commands == null ) CreateCommands();

            return commands;
        }

        /// <summary>
        /// キー入力から、コマンド実行
        /// </summary>
        /// <param name="keyInput"></param>
        /// <returns>コマンドを実行したかどうか</returns>
        private bool DispatchKeyInput(KeyInput keyInput)
        {
            // ディクショナリから検索して、キーマップ、シーンが共に引っかかればコマンド実行
            Scene currentScene = GetCurrentScene();

            foreach(KeyMap keyMap in shortcutSetting.KeyMap )
            {
                if(keyMap.KeyInput.Equals( keyInput ) )
                {
                    ICommand cmd = GetCommand(keyMap.CommandID);
                    if( cmd == null ) continue;
                    if( cmd.Scene == Scene.All || cmd.Scene == currentScene )
                    {
                        if( cmd.CanExecute() ) cmd.Execute();
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// マウスインプットから、コマンド実行
        /// </summary>
        /// <param name="mouseInput"></param>
        /// <returns>コマンドを実行したかどうか</returns>
        private bool DispatchMouseInput(MouseInput mouseInput)
        {
            Debug.WriteLine(mouseInput.ToString());
            Scene currentScene = GetCurrentScene();

            foreach(MouseInputMap mouseInputMap in shortcutSetting.MouseInputMap )
            {
                if(mouseInputMap.MouseInput.Equals( mouseInput ) )
                {
                    ICommand cmd = GetCommand(mouseInputMap.CommandID);
                    if( cmd == null ) continue;
                    if( cmd.Scene == Scene.All || cmd.Scene == currentScene )
                    {
                        if( cmd.CanExecute() ) cmd.Execute();
                        mouseButtonHoldState_L.CommandExecuted = true;
                        mouseButtonHoldState_R.CommandExecuted = true;
                        mouseButtonHoldState_M.CommandExecuted = true;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// マウスジェスチャから、コマンド取得
        /// </summary>
        /// <param name="stroke"></param>
        /// <returns>コマンド</returns>
        private ICommand GetCommandFromMouseGestureInput(MouseGestureInput gestureInput)
        {
            Scene currentScene = GetCurrentScene();

            foreach(MouseGestureMap mouseGestureMap in shortcutSetting.MouseGestureMap )
            {
                if(mouseGestureMap.GestureInput.Equals(gestureInput) )
                {
                    ICommand cmd = GetCommand(mouseGestureMap.CommandID);
                    if( cmd == null ) continue;
                    if( cmd.Scene == Scene.All || cmd.Scene == currentScene )
                    {
                        return cmd;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// マウスジェスチャのストロークから、コマンド実行
        /// </summary>
        /// <param name="stroke"></param>
        /// <returns>コマンドを実行したかどうか</returns>
        private bool DispatchMouseGestureInput(MouseGestureInput gestureInput)
        {
            ICommand cmd = GetCommandFromMouseGestureInput(gestureInput);
            if( cmd != null )
            {
                if( cmd.CanExecute() ) cmd.Execute();
                return true;
            }

            return false;
        }

        private MouseInputHold GetMouseInputHold()
        {
            MouseInputHold hold = MouseInputHold.None;

            if( (Win32.GetKeyState(Win32.VK_RBUTTON) & 0x8000) != 0 ) // マウス右ボタン
            {
                hold |= MouseInputHold.R_Button;
            }

            if( (Win32.GetKeyState(Win32.VK_LBUTTON) & 0x8000) != 0 ) // マウス左ボタン
            {
                hold |= MouseInputHold.L_Button;
            }

            if( (Win32.GetKeyState(Win32.VK_MBUTTON) & 0x8000) != 0 ) // マウス中央ボタン
            {
                hold |= MouseInputHold.M_Button;
            }

            if( (Win32.GetKeyState(Win32.VK_XBUTTON1) & 0x8000) != 0 ) // マウス戻るボタン
            {
                hold |= MouseInputHold.XButton1;
            }

            if( (Win32.GetKeyState(Win32.VK_XBUTTON2) & 0x8000) != 0 ) // マウス進むボタン
            {
                hold |= MouseInputHold.XButton2;
            }

            if( (Keyboard.Modifiers & ModifierKeys.Control) != 0) // Ctrlキー
            {
                hold |= MouseInputHold.Ctrl;
            }

            if( (Keyboard.Modifiers & ModifierKeys.Shift) != 0) // Shiftキー
            {
                hold |= MouseInputHold.Shift;
            }
            if( (Keyboard.Modifiers & ModifierKeys.Alt) != 0) // Altキー
            {
                hold |= MouseInputHold.Alt;
            }

            //Debug.WriteLine( "MouseInputHold: " + hold.ToString() );
            return hold;
        }

        // マウスジェスチャー用のフックを解除
        public void UnhookWindowsHook()
        {
            if( mouseGesture != null ) mouseGesture.UnHook();
        }

        /* ---------------------------------------------------- */
        //       EventHandler (キー)
        /* ---------------------------------------------------- */
        // キー押下イベント
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // キー情報取得
            ModifierKeys modKeys = Keyboard.Modifiers;
            KeyInput keyInput;
            if(e.Key == Key.System ) { keyInput = new KeyInput(modKeys, e.SystemKey); }
            else { keyInput = new KeyInput(modKeys, e.Key); }
            Debug.WriteLine( "key: " + keyInput.Key.ToString() + "\nmod: " + keyInput.Modifiers.ToString() );

            // キーインプット送信
            if( DispatchKeyInput(keyInput) ) e.Handled = true;
        }

        /* ---------------------------------------------------- */
        //       EventHandler (マウスインプット、マウスジェスチャー)
        /* ---------------------------------------------------- */

        // マウスホイール
        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MouseInput mouseInput;
            if(e.Delta > 0 ) // wheel up
            {
                mouseInput = new MouseInput(GetMouseInputHold(), MouseInputClick.WheelUp);
            }
            else // wheel down
            {
                mouseInput = new MouseInput(GetMouseInputHold(), MouseInputClick.WheelDown);
            }

            // 送信
            if( DispatchMouseInput(mouseInput) ) e.Handled = true;
        }

        // マウスボタン押下
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("mouse down : " + e.ChangedButton.ToString());
            MouseButtonHoldState mouseButtonHoldState = null;

            if(e.ChangedButton == MouseButton.Left )          mouseButtonHoldState = mouseButtonHoldState_L;
            else if(e.ChangedButton == MouseButton.Right )    mouseButtonHoldState = mouseButtonHoldState_R;
            else if(e.ChangedButton == MouseButton.Middle )   mouseButtonHoldState = mouseButtonHoldState_M;
            else if(e.ChangedButton == MouseButton.XButton1 ) mouseButtonHoldState = mouseButtonHoldState_X1;
            else if(e.ChangedButton == MouseButton.XButton2 ) mouseButtonHoldState = mouseButtonHoldState_X2;

            if(mouseButtonHoldState != null )
            {
                mouseButtonHoldState.IsPressed = true;
                mouseButtonHoldState.CommandExecuted = false;
            }

            // マウスジェスチャ スタート
            if(shortcutSetting.MouseGestureMap.Count > 0 )
            {
                MouseGestureMap map = shortcutSetting.MouseGestureMap.FirstOrDefault( m => m.GestureInput?.StartingButton == e.ChangedButton );
                if(map != null )
                {
                    if( mouseGesture == null ) InitMouseGesture();
                    mouseGesture.Range = MainWindow.Current.Setting.MouseGestureRange;
                    mouseGesture.Start(e.ChangedButton);
                }
            }
        }

        // マウスを動かしている時
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
        }

        // マウスボタン離した時
        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // マウスインプット
            MouseButtonHoldState mouseButtonHoldState = new MouseButtonHoldState();;
            MouseInputClick mouseInputClick = MouseInputClick.None;

            Debug.WriteLine("mouse up : " + e.ChangedButton.ToString());

            if(e.ChangedButton == MouseButton.Left )
            {
                mouseButtonHoldState = mouseButtonHoldState_L;
                mouseInputClick = MouseInputClick.L_Click;
            }
            else if(e.ChangedButton == MouseButton.Right )
            {
                mouseButtonHoldState = mouseButtonHoldState_R;
                mouseInputClick = MouseInputClick.R_Click;
            }
            else if(e.ChangedButton == MouseButton.Middle )
            {
                mouseButtonHoldState = mouseButtonHoldState_M;
                mouseInputClick = MouseInputClick.M_Click;
            }
            else if(e.ChangedButton == MouseButton.XButton1 )
            {
                mouseButtonHoldState = mouseButtonHoldState_X1;
                mouseInputClick = MouseInputClick.X1_Click;
            }
            else if(e.ChangedButton == MouseButton.XButton2 )
            {
                mouseButtonHoldState = mouseButtonHoldState_X2;
                mouseInputClick = MouseInputClick.X2_Click;
            }


            if( mouseButtonHoldState.CommandExecuted )
            {
                e.Handled = true;
            }
            else if(mouseButtonHoldState.IsPressed)
            {
                MouseInput mouseInput = new MouseInput(GetMouseInputHold(), mouseInputClick);
                if( DispatchMouseInput(mouseInput) ) e.Handled = true;
            }

            // 押下状態更新
            mouseButtonHoldState.IsPressed = false;

        }

        // ダブルクリック
        private void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MouseInputClick mouseInputClick = MouseInputClick.None;
            MouseInputHold mouseInputHold     = GetMouseInputHold();

            if(e.ChangedButton == MouseButton.Left )
            {
                mouseInputClick = MouseInputClick.L_DoubleClick;
                mouseInputHold = (mouseInputHold & ~MouseInputHold.L_Button);
            }
            else if(e.ChangedButton == MouseButton.Right )
            {
                mouseInputClick = MouseInputClick.R_DoubleClick;
                mouseInputHold = (mouseInputHold & ~MouseInputHold.R_Button);
            }

            MouseInput mouseInput = new MouseInput(mouseInputHold, mouseInputClick);
            if( DispatchMouseInput(mouseInput) ) e.Handled = true;
        }


        // マウスジェスチャ ストローク更新時
        private void MouseGestureStrokeChanged(object sender, EventArgs e)
        {
            mouseButtonHoldState_R.CommandExecuted = true;

            // 現在のストロークと、一致するコマンドを通知ブロックに表示
            MouseGestureInput gestureInput = new MouseGestureInput(mouseGesture.StartingButton, mouseGesture.Stroke);
            string notification = gestureInput.ToString();
            ICommand cmd = GetCommandFromMouseGestureInput(gestureInput);
            if(cmd != null )
            {
                notification += " [" + cmd.GetDetail() + "]";
            }
            MainWindow.Current.NotificationBlock.Show(notification, NotificationPriority.Normal, NotificationTime.Eternally);
        }

        // マウスジェスチャ 完了時
        private void MouseGestureFinished(object sender, EventArgs e)
        {
            MouseGestureInput gestureInput = new MouseGestureInput(mouseGesture.StartingButton, mouseGesture.Stroke);

            // ストロークがあった場合、コマンド発送
            if( gestureInput.Stroke.Length > 0 )
            {
                MainWindow.Current.NotificationBlock.Hide();
                DispatchMouseGestureInput(gestureInput);
                return;
            }
        }

        // End of Class
    }
}
