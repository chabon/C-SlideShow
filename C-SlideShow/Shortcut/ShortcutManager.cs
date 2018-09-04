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
    public class MouseButtonClickState
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
        string strokeOfLastExecutedCommand;

        // マウスボタンの状態
        MouseButtonClickState mouseButtonClickState_L  = new MouseButtonClickState();
        MouseButtonClickState mouseButtonClickState_R  = new MouseButtonClickState();
        MouseButtonClickState mouseButtonClickState_M  = new MouseButtonClickState();
        MouseButtonClickState mouseButtonClickState_X1 = new MouseButtonClickState();
        MouseButtonClickState mouseButtonClickState_X2 = new MouseButtonClickState();

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
            MainWindow.Current.MouseUp       += this.MainWindow_MouseUp;
            MainWindow.Current.MouseDoubleClick   += this.MainWindow_MouseDoubleClick;

            // 左クリック後、ドラッグでウインドウドラッグ可能に
            windowDragMove = new WindowDragMove(MainWindow.Current);
            windowDragMove.CanDragStart = () => { return !MainWindow.Current.Setting.TempProfile.IsFullScreenMode.Value; };
        }

        private void InitMouseGesture()
        {
            mouseGesture = new MouseGesture();
            mouseGesture.StrokeChanged   += MouseGestureStrokeChanged;
            mouseGesture.GestureFinished += MouseGestureFinished;
            mouseGesture.HoldClick     += MouseGestureHoldClick;
            mouseGesture.Range = MainWindow.Current.Setting.MouseGestureRange;
            strokeOfLastExecutedCommand = "";
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
            commands.Add( new ShowContextMenu() );
            commands.Add( new LoadProfileFromNum() );
            commands.Add( new LoadProfileFromName() );
            commands.Add( new ShowAppSettingDialog() );
            commands.Add( new OpenImageUnderCursorByDefaultApp() );
            commands.Add( new OpenImageUnderCursorByExternalApp() );

            // 通常時
            commands.Add( new SlideToForward() );
            commands.Add( new SlideToBackward() );
            commands.Add( new SlideToLeft() );
            commands.Add( new SlideToRight() );
            commands.Add( new SlideToTop() );
            commands.Add( new SlideToBottom() );
            commands.Add( new SlideToForwardByOneImage() );
            commands.Add( new SlideToBackwardByOneImage() );
            commands.Add( new ZoomImageUnderCursor() );
            commands.Add( new ShiftForward() );
            commands.Add( new ShiftBackward() );

            // 画像拡大時
            commands.Add( new ZoomInImage() );
            commands.Add( new ZoomOutImage() );
            commands.Add( new ExitZoom() );
        }
        
        // IDからコマンド取得
        public ICommand GetCommand(CommandID id)
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

        // コマンド実行
        public void ExecuteCommand(ICommand command, int value, string strValue)
        {
            if( command != null )
            {
                if( command.EnableValue ) command.Value = value;
                if( command.EnableStrValue ) command.StrValue = strValue;

                if( command.CanExecute() ) command.Execute();
            }
        }

        // コマンドIDから実行
        public void ExecuteCommand(CommandID id, int value, string strValue)
        {
            if( commands == null ) CreateCommands();

            ICommand command = GetCommand(id);
            if( command != null )
            {
                ExecuteCommand(command, value, strValue);
            }
        }

        // コマンドIDから実行(値は0と空文字で固定)
        public void ExecuteCommand(CommandID id)
        {
            if( commands == null ) CreateCommands();

            ICommand command = GetCommand(id);
            if( command != null )
            {
                ExecuteCommand(command, 0, "");
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

            foreach(CommandMap commandMap in shortcutSetting.CommandMap )
            {
                if( commandMap.KeyInput == null ) continue;
                
                if( commandMap.KeyInput.Equals( keyInput ) )
                {
                    ICommand cmd = GetCommand(commandMap.CommandID);
                    if( cmd == null ) continue;
                    if( cmd.Scene == Scene.All || cmd.Scene == currentScene )
                    {
                        ExecuteCommand(cmd, commandMap.CommandValue, commandMap.CommandStrValue);
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

            foreach(CommandMap commandMap in shortcutSetting.CommandMap )
            {
                if( commandMap.MouseInput == null ) continue;

                if( commandMap.MouseInput.Equals( mouseInput ) )
                {
                    ICommand cmd = GetCommand(commandMap.CommandID);
                    if( cmd == null ) continue;
                    if( cmd.Scene == Scene.All || cmd.Scene == currentScene )
                    {
                        ExecuteCommand(cmd, commandMap.CommandValue, commandMap.CommandStrValue);
                        mouseButtonClickState_L.CommandExecuted = true;
                        mouseButtonClickState_R.CommandExecuted = true;
                        mouseButtonClickState_M.CommandExecuted = true;
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
        private ICommand GetCommandFromMouseGestureInput(MouseGestureInput gestureInput, out CommandMap _commandMap)
        {
            Scene currentScene = GetCurrentScene();

            foreach(CommandMap commandMap in shortcutSetting.CommandMap )
            {
                if( commandMap.MouseGestureInput == null ) continue;

                if( commandMap.MouseGestureInput.Equals(gestureInput) )
                {
                    ICommand cmd = GetCommand(commandMap.CommandID);
                    if( cmd == null ) continue;
                    if( cmd.Scene == Scene.All || cmd.Scene == currentScene )
                    {
                        _commandMap = commandMap;
                        return cmd;
                    }
                }
            }

            _commandMap = null;
            return null;
        }

        /// <summary>
        /// マウスジェスチャのストロークから、コマンド実行
        /// </summary>
        /// <param name="stroke"></param>
        /// <returns>コマンドを実行したかどうか</returns>
        private bool DispatchMouseGestureInput(MouseGestureInput gestureInput)
        {
            CommandMap commandMap;
            ICommand cmd = GetCommandFromMouseGestureInput(gestureInput, out commandMap);
            if( cmd != null )
            {
                ExecuteCommand(cmd, commandMap.CommandValue, commandMap.CommandStrValue);
                return true;
            }

            return false;
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
            // マウスジェスチャ入力中
            if( mouseGesture != null && mouseGesture.IsActive ) return;

            // 
            MouseInput mouseInput;
            if(e.Delta > 0 ) // wheel up
            {
                mouseInput = new MouseInput(MouseInputButton.WheelUp, Keyboard.Modifiers);
            }
            else // wheel down
            {
                mouseInput = new MouseInput(MouseInputButton.WheelDown, Keyboard.Modifiers);
            }

            // 送信
            if( DispatchMouseInput(mouseInput) ) e.Handled = true;
        }

        // マウスボタン押下
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // マウスジェスチャ入力中
            if( mouseGesture != null && mouseGesture.IsActive ) return;

            Debug.WriteLine("mouse down : " + e.ChangedButton.ToString());
            MouseButtonClickState mouseButtonHoldState = null;

            if(e.ChangedButton == MouseButton.Left )          mouseButtonHoldState = mouseButtonClickState_L;
            else if(e.ChangedButton == MouseButton.Right )    mouseButtonHoldState = mouseButtonClickState_R;
            else if(e.ChangedButton == MouseButton.Middle )   mouseButtonHoldState = mouseButtonClickState_M;
            else if(e.ChangedButton == MouseButton.XButton1 ) mouseButtonHoldState = mouseButtonClickState_X1;
            else if(e.ChangedButton == MouseButton.XButton2 ) mouseButtonHoldState = mouseButtonClickState_X2;

            if(mouseButtonHoldState != null )
            {
                mouseButtonHoldState.IsPressed = true;
                mouseButtonHoldState.CommandExecuted = false;
            }

            // マウスジェスチャ スタート
            if( shortcutSetting.CommandMap.Any( c => c.MouseGestureInput != null) )
            {
                CommandMap map = shortcutSetting.CommandMap.FirstOrDefault( c => c.MouseGestureInput?.StartingButton == e.ChangedButton );
                if(map != null )
                {
                    if( mouseGesture == null ) InitMouseGesture();
                    if( !mouseGesture.IsActive )
                    {
                        mouseGesture.Range = MainWindow.Current.Setting.MouseGestureRange;
                        if( e.ChangedButton == MouseButton.Left ) mouseGesture.EnableDragGesture = false; // 左ボタンのドラッグは無効に(ウインドウ移動に使われているので)
                        else mouseGesture.EnableDragGesture = true;
                        strokeOfLastExecutedCommand = "";
                        mouseGesture.Start(e.ChangedButton);
                    }
                }
            }
        }

        // マウスボタン離した時
        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // マウスジェスチャ入力中
            if( mouseGesture != null && mouseGesture.IsActive ) return;

            // マウスクリックの取得
            MouseButtonClickState mouseButtonClickState = null;
            MouseInputButton mouseInputButton = MouseInputButton.None;

            Debug.WriteLine("mouse up : " + e.ChangedButton.ToString());

            if(e.ChangedButton == MouseButton.Left )
            {
                mouseButtonClickState = mouseButtonClickState_L;
                mouseInputButton = MouseInputButton.L_Click;
            }
            else if(e.ChangedButton == MouseButton.Right )
            {
                mouseButtonClickState = mouseButtonClickState_R;
                mouseInputButton = MouseInputButton.R_Click;
            }
            else if(e.ChangedButton == MouseButton.Middle )
            {
                mouseButtonClickState = mouseButtonClickState_M;
                mouseInputButton = MouseInputButton.M_Click;
            }
            else if(e.ChangedButton == MouseButton.XButton1 )
            {
                mouseButtonClickState = mouseButtonClickState_X1;
                mouseInputButton = MouseInputButton.X1_Click;
            }
            else if(e.ChangedButton == MouseButton.XButton2 )
            {
                mouseButtonClickState = mouseButtonClickState_X2;
                mouseInputButton = MouseInputButton.X2_Click;
            }

            // 取得失敗
            if( mouseButtonClickState == null) return;
            if( mouseInputButton == MouseInputButton.None ) return;

            // 既に他の入力でコマンド実行済み
            if( mouseButtonClickState.CommandExecuted )
            {
                //e.Handled = true;
            }

            // マウスクリックコマンド実行
            else if(mouseButtonClickState.IsPressed)
            {
                MouseInput mouseInput = new MouseInput(mouseInputButton, Keyboard.Modifiers);
                if( DispatchMouseInput(mouseInput) ) e.Handled = true;
            }

            // 押下状態更新
            mouseButtonClickState.IsPressed = false;
        }

        // ダブルクリック
        private void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // マウスジェスチャ入力中
            if( mouseGesture != null && mouseGesture.IsActive ) return;

            //
            MouseInputButton mouseInputButton = MouseInputButton.None;

            if(e.ChangedButton == MouseButton.Left )
            {
                mouseInputButton = MouseInputButton.L_DoubleClick;
            }
            else if(e.ChangedButton == MouseButton.Right )
            {
                mouseInputButton = MouseInputButton.R_DoubleClick;
            }

            MouseInput mouseInput = new MouseInput(mouseInputButton, Keyboard.Modifiers);
            if( DispatchMouseInput(mouseInput) ) e.Handled = true;
        }


        // マウスジェスチャ ストローク更新時
        private void MouseGestureStrokeChanged(object sender, EventArgs e)
        {
            switch( mouseGesture.StartingButton )
            {
                case MouseButton.Left:
                    mouseButtonClickState_L.CommandExecuted = true;
                    break;
                case MouseButton.Right:
                    mouseButtonClickState_R.CommandExecuted = true;
                    break;
                case MouseButton.Middle:
                    mouseButtonClickState_M.CommandExecuted = true;
                    break;
                case MouseButton.XButton1:
                    mouseButtonClickState_X1.CommandExecuted = true;
                    break;
                case MouseButton.XButton2:
                    mouseButtonClickState_X2.CommandExecuted = true;
                    break;
            }

            // 現在のストロークと、一致するコマンドを通知ブロックに表示
            MouseGestureInput gestureInput = new MouseGestureInput(mouseGesture.StartingButton, mouseGesture.Stroke);
            string notification = gestureInput.ToString();
            CommandMap commandMap;
            ICommand cmd = GetCommandFromMouseGestureInput(gestureInput, out commandMap);
            if(cmd != null )
            {
                // マウスホイールの場合は、即HoldClickイベントでコマンドが実行されるので、通知表示しない
                if( gestureInput.Stroke.EndsWith("[WU]") || gestureInput.Stroke.EndsWith("[WD]") ) return;

                if( cmd.EnableValue ) cmd.Value = commandMap.CommandValue;
                if( cmd.EnableStrValue ) cmd.StrValue = commandMap.CommandStrValue;

                notification += "  [" + cmd.GetDetail() + "]";
            }
            MainWindow.Current.NotificationBlock.Show(notification, NotificationPriority.Normal, NotificationTime.Eternally);
        }

        // マウスジェスチャ ホールドクリック時
        private void MouseGestureHoldClick(object sender, EventArgs e)
        {
            MouseGestureInput gestureInput = new MouseGestureInput(mouseGesture.StartingButton, mouseGesture.Stroke);

            if( gestureInput.Stroke.Length > 0 )
            {
                CommandMap commandMap;
                ICommand cmd = GetCommandFromMouseGestureInput(gestureInput, out commandMap);
                if( cmd != null && commandMap != null)
                {
                    //      コマンドがマッチした場合

                    MainWindow.Current.NotificationBlock.Hide();

                    // 現在のストロークから、最後のクリックストロークを削除(ボタンをホールドしたまま、別のボタンのコマンドも押せるように)
                    mouseGesture.RemoveLastClickStroke();

                    // 削除後のストロークを保持。マウスジェスチャ完了時に被るならば、コマンド発行させない
                    strokeOfLastExecutedCommand = mouseGesture.Stroke;
                    Debug.WriteLine("strokeOfLastExecutedCommand: " + mouseGesture.Stroke );

                    // コマンド実行
                    ExecuteCommand(cmd, commandMap.CommandValue, commandMap.CommandStrValue);
                }
                return;
            }
        }

        // マウスジェスチャ 完了時
        private void MouseGestureFinished(object sender, EventArgs e)
        {
            MouseGestureInput gestureInput = new MouseGestureInput(mouseGesture.StartingButton, mouseGesture.Stroke);
            MainWindow.Current.NotificationBlock.Hide();

            if( gestureInput.Stroke.Length > 0 && gestureInput.Stroke != strokeOfLastExecutedCommand)
            {
                DispatchMouseGestureInput(gestureInput);
                return;
            }
        }

        // End of Class
    }
}
