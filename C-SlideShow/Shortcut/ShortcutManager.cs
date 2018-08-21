using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;

using C_SlideShow.Shortcut.Command;


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

        // マウスボタンの状態
        private MouseButtonHoldState MouseLButtonHoldState = new MouseButtonHoldState();
        private MouseButtonHoldState MouseRButtonHoldState = new MouseButtonHoldState();
        private MouseButtonHoldState MouseMButtonHoldState = new MouseButtonHoldState();


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

            // マウスインプット
            MainWindow.Current.MouseWheel       += this.MainWindow_MouseWheel;
            MainWindow.Current.PreviewMouseDown += this.MainWindow_PreviewMouseDown;
            MainWindow.Current.PreviewMouseUp   += this.MainWindow_PreviewMouseUp;
            MainWindow.Current.PreviewMouseDoubleClick   += this.MainWindow_PreviewMouseDoubleClick;

            // ウインドウ全体でドラッグ可能に
            MainWindow.Current.MouseLeftButtonDown += (sender, e) =>
            {
                if( MainWindow.Current.Setting.TempProfile.IsFullScreenMode.Value ) return;
                MainWindow.Current.DragMove();
            };


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
                        MouseLButtonHoldState.CommandExecuted = true;
                        MouseRButtonHoldState.CommandExecuted = true;
                        MouseMButtonHoldState.CommandExecuted = true;
                        return true;
                    }
                }
            }
            return false;
        }

        private MouseInputHold GetMouseInputHold()
        {
            MouseInputHold hold = MouseInputHold.None;

            if( (Win32.GetAsyncKeyState(Win32.VK_RBUTTON) & 0x8000) != 0 ) // マウス右ボタン
            {
                hold |= MouseInputHold.R_Button;
            }

            if( (Win32.GetAsyncKeyState(Win32.VK_LBUTTON) & 0x8000) != 0 ) // マウス左ボタン
            {
                hold |= MouseInputHold.L_Button;
            }

            if( (Win32.GetAsyncKeyState(Win32.VK_MBUTTON) & 0x8000) != 0 ) // マウス中央ボタン
            {
                hold |= MouseInputHold.M_Button;
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

            System.Diagnostics.Debug.WriteLine( "MouseInputHold: " + hold.ToString() );
            return hold;
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
            System.Diagnostics.Debug.WriteLine( "key: " + keyInput.Key.ToString() + "\nmod: " + keyInput.Modifiers.ToString() );

            // インプット送信
            if( DispatchKeyInput(keyInput) ) e.Handled = true;
        }

        /* ---------------------------------------------------- */
        //       EventHandler (マウスインプット)
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
                MouseLButtonHoldState.IsPressed = true;
                MouseLButtonHoldState.CommandExecuted = false;
            }
            else if(e.ChangedButton == MouseButton.Right )
            {
                MouseRButtonHoldState.IsPressed = true;
                MouseRButtonHoldState.CommandExecuted = false;
            }
            else if(e.ChangedButton == MouseButton.Middle )
            {
                MouseMButtonHoldState.IsPressed = true;
                MouseMButtonHoldState.CommandExecuted = false;
            }
        }

        // マウスボタン離した時
        private void MainWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseButtonHoldState mouseButtonHoldState = new MouseButtonHoldState();
            MouseInputButton mouseInputButton = MouseInputButton.None;

            if(e.ChangedButton == MouseButton.Left )
            {
                mouseButtonHoldState = MouseLButtonHoldState;
                mouseInputButton = MouseInputButton.L_Click;
            }
            else if(e.ChangedButton == MouseButton.Right )
            {
                mouseButtonHoldState = MouseRButtonHoldState;
                mouseInputButton = MouseInputButton.R_Click;
            }
            else if(e.ChangedButton == MouseButton.Middle )
            {
                mouseButtonHoldState = MouseMButtonHoldState;
                mouseInputButton = MouseInputButton.M_Click;
            }

            if( mouseButtonHoldState.CommandExecuted )
            {
                e.Handled = true;
                return;
            }
            else if(mouseButtonHoldState.IsPressed)
            {
                mouseButtonHoldState.IsPressed = false;
                MouseInput mouseInput = new MouseInput(GetMouseInputHold(), mouseInputButton);
                if( DispatchMouseInput(mouseInput) ) e.Handled = true;
            }

        }

        // ダブルクリック
        private void MainWindow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MouseInputButton mouseInputButton = MouseInputButton.None;
            MouseInputHold mouseInputHold     = GetMouseInputHold();

            if(e.ChangedButton == MouseButton.Left )
            {
                mouseInputButton = MouseInputButton.L_DoubleClick;
                mouseInputHold = (mouseInputHold & ~MouseInputHold.L_Button);
            }
            else if(e.ChangedButton == MouseButton.Right )
            {
                mouseInputButton = MouseInputButton.R_DoubleClick;
                mouseInputHold = (mouseInputHold & ~MouseInputHold.R_Button);
            }

            MouseInput mouseInput = new MouseInput(mouseInputHold, mouseInputButton);
            if( DispatchMouseInput(mouseInput) ) e.Handled = true;
        }


        // End of Class
    }
}
