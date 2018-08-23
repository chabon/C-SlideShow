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
        MouseButtonHoldState mouseLButtonHoldState = new MouseButtonHoldState();
        MouseButtonHoldState mouseRButtonHoldState = new MouseButtonHoldState();
        MouseButtonHoldState mouseMButtonHoldState = new MouseButtonHoldState();


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
            MainWindow.Current.PreviewKeyDown += this.MainWindow_PreviewKeyDown;

            // マウスインプット マウスジェスチャー
            MainWindow.Current.MouseWheel       += this.MainWindow_MouseWheel;
            MainWindow.Current.PreviewMouseDown += this.MainWindow_PreviewMouseDown;
            MainWindow.Current.PreviewMouseMove += this.MainWindow_PreviewMouseMove;
            MainWindow.Current.PreviewMouseUp   += this.MainWindow_PreviewMouseUp;
            MainWindow.Current.PreviewMouseDoubleClick   += this.MainWindow_PreviewMouseDoubleClick;

            // ウインドウ全体でドラッグ可能に
            MainWindow.Current.MouseLeftButtonDown += (sender, e) =>
            {
                if( MainWindow.Current.Setting.TempProfile.IsFullScreenMode.Value ) return;
                MainWindow.Current.DragMove();
            };
        }

        private void InitMouseGesture()
        {
            mouseGesture = new MouseGesture();
            mouseGesture.StrokeChanged += MouseGestureStrokeChanged;
            mouseGesture.GestureFinished += MouseGestureFinished;
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
            commands.Add( new SlideToTop() );
            commands.Add( new SlideToRight() );
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
                        cmd.Execute();
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
            Scene currentScene = GetCurrentScene();

            foreach(MouseInputMap mouseInputMap in shortcutSetting.MouseInputMap )
            {
                if(mouseInputMap.MouseInput.Equals( mouseInput ) )
                {
                    ICommand cmd = GetCommand(mouseInputMap.CommandID);
                    if( cmd == null ) continue;
                    if( cmd.Scene == Scene.All || cmd.Scene == currentScene )
                    {
                        cmd.Execute();
                        mouseLButtonHoldState.CommandExecuted = true;
                        mouseRButtonHoldState.CommandExecuted = true;
                        mouseMButtonHoldState.CommandExecuted = true;
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
        private ICommand GetCommandFromMouseGestureStroke(string stroke)
        {
            Scene currentScene = GetCurrentScene();

            foreach(MouseGestureMap mouseGestureMap in shortcutSetting.MouseGestureMap )
            {
                if(mouseGestureMap.Gesture == stroke )
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
        private bool DispatchMouseGestureStroke(string stroke)
        {
            ICommand cmd = GetCommandFromMouseGestureStroke(stroke);
            if( cmd != null )
            {
                cmd.Execute();
                return true;
            }

            return false;
        }

        private MouseInputHold GetMouseInputHold()
        {
            MouseInputHold hold = MouseInputHold.None;

            if( (Win32.GetAsyncKeyState(Win32.VK_RBUTTON) & 0x8000) != 0 ) // マウス右ボタン
            {
                hold |= MouseInputHold.R_Click;
            }

            if( (Win32.GetAsyncKeyState(Win32.VK_LBUTTON) & 0x8000) != 0 ) // マウス左ボタン
            {
                hold |= MouseInputHold.L_Click;
            }

            if( (Win32.GetAsyncKeyState(Win32.VK_MBUTTON) & 0x8000) != 0 ) // マウス中央ボタン
            {
                hold |= MouseInputHold.M_Click;
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

            Debug.WriteLine( "MouseInputHold: " + hold.ToString() );
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
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
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
                mouseInput = new MouseInput(GetMouseInputHold(), MouseInputButton.WheelUp);
            }
            else // wheel down
            {
                mouseInput = new MouseInput(GetMouseInputHold(), MouseInputButton.WheelDown);
            }

            // 送信
            if( DispatchMouseInput(mouseInput) ) e.Handled = true;
        }

        // マウスボタン押下
        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left )
            {
                mouseLButtonHoldState.IsPressed = true;
                mouseLButtonHoldState.CommandExecuted = false;
            }
            else if(e.ChangedButton == MouseButton.Right )
            {
                mouseRButtonHoldState.IsPressed = true;
                mouseRButtonHoldState.CommandExecuted = false;

                // マウスジェスチャ スタート
                if(shortcutSetting.MouseGestureMap.Count > 0 )
                {
                    if( mouseGesture == null ) InitMouseGesture();
                    mouseGesture.Start();
                }
            }
            else if(e.ChangedButton == MouseButton.Middle )
            {
                mouseMButtonHoldState.IsPressed = true;
                mouseMButtonHoldState.CommandExecuted = false;
            }
        }

        // マウスを動かしている時
        private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }

        // マウスボタン離した時
        private void MainWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // マウスボタン 押下状態を更新
            MouseButtonHoldState mouseButtonHoldState = new MouseButtonHoldState();;
            MouseInputButton mouseInputButton = MouseInputButton.None;

            if(e.ChangedButton == MouseButton.Left )
            {
                mouseButtonHoldState = mouseLButtonHoldState;
                mouseInputButton = MouseInputButton.L_Click;
            }
            else if(e.ChangedButton == MouseButton.Right )
            {
                mouseButtonHoldState = mouseRButtonHoldState;
                mouseInputButton = MouseInputButton.R_Click;
            }
            else if(e.ChangedButton == MouseButton.Middle )
            {
                mouseButtonHoldState = mouseMButtonHoldState;
                mouseInputButton = MouseInputButton.M_Click;
            }

            // マウスインプット 入力終了
            if( mouseButtonHoldState.CommandExecuted )
            {
                e.Handled = true;
                return;
            }
            else if(mouseButtonHoldState.IsPressed)
            {
                MouseInput mouseInput = new MouseInput(GetMouseInputHold(), mouseInputButton);
                if( DispatchMouseInput(mouseInput) ) e.Handled = true;
            }

            // 押下状態更新
            mouseButtonHoldState.IsPressed = false;

        }

        // ダブルクリック
        private void MainWindow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MouseInputButton mouseInputButton = MouseInputButton.None;
            MouseInputHold mouseInputHold     = GetMouseInputHold();

            if(e.ChangedButton == MouseButton.Left )
            {
                mouseInputButton = MouseInputButton.L_DoubleClick;
                mouseInputHold = (mouseInputHold & ~MouseInputHold.L_Click);
            }
            else if(e.ChangedButton == MouseButton.Right )
            {
                mouseInputButton = MouseInputButton.R_DoubleClick;
                mouseInputHold = (mouseInputHold & ~MouseInputHold.R_Click);
            }

            MouseInput mouseInput = new MouseInput(mouseInputHold, mouseInputButton);
            if( DispatchMouseInput(mouseInput) ) e.Handled = true;
        }


        // マウスジェスチャ ストローク更新時
        private void MouseGestureStrokeChanged(object sender, EventArgs e)
        {
            mouseRButtonHoldState.CommandExecuted = true;

            // 現在のストロークと、一致するコマンドを通知ブロックに表示
            string notification = mouseGesture.Stroke;
            ICommand cmd = GetCommandFromMouseGestureStroke(mouseGesture.Stroke);
            if(cmd != null )
            {
                notification += "(" + cmd.GetDetail() + ")";
            }
            MainWindow.Current.NotificationBlock.Show(notification, NotificationPriority.Normal, NotificationTime.Eternally);
        }

        // マウスジェスチャ 完了時
        private void MouseGestureFinished(object sender, EventArgs e)
        {
            string stroke = mouseGesture.Stroke;

            // ストロークがあった場合、コマンド発送
            if( stroke.Length > 0 )
            {
                MainWindow.Current.NotificationBlock.Hide();
                DispatchMouseGestureStroke(stroke);
                return;
            }
        }

        // End of Class
    }
}
