﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

using C_SlideShow.Shortcut.Command;
using C_SlideShow.Shortcut.Drag;
using C_SlideShow.CommonControl;


namespace C_SlideShow.Shortcut
{
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
        MouseButtonStateSet mouseButtonStateSet = new MouseButtonStateSet();

        // 長押し用タイマー
        DispatcherTimer longClickTimer;

        // 長押しクリック開始したボタン
        MouseInputButton longClickButton;

        // 長押しクリック開始座標(スクリーン座標系)
        Point longClickStartPos;

        // 左クリックでのウインドウドラッグを管理
        WindowDragMove windowDragMove;

        // 拡大パネルのドラッグ移動を管理
        TileExpantionPanelDragMove tileExpantionPanelDragMove;

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
            MainWindow.Current.MouseMove     += this.MainWindow_MouseMove;
            MainWindow.Current.MouseDoubleClick   += this.MainWindow_MouseDoubleClick;

            // 左クリック後、ドラッグでウインドウドラッグ可能に
            windowDragMove = new WindowDragMove(MainWindow.Current);
            windowDragMove.CanDragStart = () => {
                if( MainWindow.Current.Setting.TempProfile.IsFullScreenMode.Value ) return false;
                if( MainWindow.Current.TileExpantionPanel.IsShowing && MainWindow.Current.TileExpantionPanel.ZoomFactor > 1.0 ) return false;
                return true;
            };
            windowDragMove.DragMoved += (s, e) => { mouseButtonStateSet.L.CommandExecuted = true; };

            // 左クリック後、拡大パネル拡大中ならパネルを移動可能に
            tileExpantionPanelDragMove = new TileExpantionPanelDragMove(MainWindow.Current);
            tileExpantionPanelDragMove.CanDragStart = () => {
                var panel = MainWindow.Current.TileExpantionPanel;
                if( panel.IsShowing && panel.IsAnimationCompleted && panel.ZoomFactor > 1.0 ) return true;
                return false;
            };
            tileExpantionPanelDragMove.DragMoved += (s, e) => { mouseButtonStateSet.L.CommandExecuted = true; };
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

            foreach( CommandID id in Enum.GetValues(typeof(CommandID)) )
            {
                commands.Add( CommandFactory.CreateById(id) );
            }
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

                if( command.CanExecute() )
                {
                    command.Execute();
                    if(command.Message != null )
                    {
                        MainWindow.Current.NotificationBlock.Show(command.Message, NotificationPriority.Normal, NotificationTime.Short, NotificationType.None);
                    }
                }
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

        // 長押しクリックの判定開始
        private void StartLongClick(MouseButton mouseButton)
        {
            if( this.longClickTimer == null )
            {
                longClickTimer = new DispatcherTimer();
                longClickTimer.Tick += (s, e) =>
                {
                    StopLongClick();
                    DispatchMouseInput( new MouseInput(longClickButton, Keyboard.Modifiers) );
                };
            }

            StopLongClick();
            longClickButton = MouseInput.MouseButtonToMouseInputLongClickButton(mouseButton);
            longClickStartPos = Win32.GetCursorPos();
            longClickTimer.Interval = TimeSpan.FromMilliseconds(MainWindow.Current.Setting.LongClickDecisionTime);
            longClickTimer.Start();
        }

        private void StopLongClick()
        {
            if(longClickTimer != null) longClickTimer.Stop();
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
                        mouseButtonStateSet.CommandExecuted();
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
            // メニューが開いているなら無効
            if( MainWindow.Current.IsAnyToolbarMenuOpened ) return;

            // キー情報取得
            ModifierKeys modKeys = Keyboard.Modifiers;
            Key inputKey;

            if (e.ImeProcessedKey != Key.None) // Ime有効な場合の対処
            {
                inputKey = e.ImeProcessedKey;
            }
            else if(e.Key == Key.System ) // システムキーが押された場合
            {
                inputKey = e.SystemKey;
            }
            else
            { 
                inputKey = e.Key;
            }

            KeyInput keyInput = new KeyInput(modKeys, inputKey);
            Debug.WriteLine("------------------------------------------------------------------");
            Debug.WriteLine("MainWindow KeyDown   original source:" + e.OriginalSource.ToString() );
            Debug.WriteLine( "key: " + keyInput.Key.ToString() + "\nmod: " + keyInput.Modifiers.ToString() );
            Debug.WriteLine("Keybord focus:" + Keyboard.FocusedElement.ToString() );
            Debug.WriteLine("------------------------------------------------------------------");

            // キーインプット送信
            if( DispatchKeyInput(keyInput) ) e.Handled = true;
        }

        /* ---------------------------------------------------- */
        //       EventHandler (マウスインプット、マウスジェスチャー)
        /* ---------------------------------------------------- */

        // マウスホイール
        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // メニューが開いているなら無効
            if( MainWindow.Current.IsAnyToolbarMenuOpened ) return;

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
            // メニューが開いているなら無効
            if( MainWindow.Current.IsAnyToolbarMenuOpened ) return;

            // マウスジェスチャ入力中
            if( mouseGesture != null && mouseGesture.IsActive ) return;

            Debug.WriteLine("mouse down : " + e.ChangedButton.ToString());

            // ボタン押下状態をセット
            mouseButtonStateSet.SetPress(e.ChangedButton);

            // マウスボタン長押し判定スタート
            StartLongClick(e.ChangedButton);

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
            // メニューが開いているなら無効
            if( MainWindow.Current.IsAnyToolbarMenuOpened ) return;

            // マウスジェスチャ入力中
            if( mouseGesture != null && mouseGesture.IsActive ) return;

            Debug.WriteLine("mouse up : " + e.ChangedButton.ToString());

            // 長押しクリック判定解除
            StopLongClick();

            // マウスクリックの状態取得
            MouseButtonState mouseButtonState = mouseButtonStateSet.GetState(e.ChangedButton);

            // マウスインプット生成
            MouseInputButton mouseInputButton = MouseInput.MouseButtonToMouseInputButton(e.ChangedButton);

            // 取得失敗
            if( mouseButtonState == null) return;
            if( mouseInputButton == MouseInputButton.None ) return;

            // 既に他の入力でコマンド実行済み
            if( mouseButtonState.CommandExecuted )
            {
                //e.Handled = true;
            }

            // マウスクリックコマンド実行
            else if(mouseButtonState.IsPressed)
            {
                MouseInput mouseInput = new MouseInput(mouseInputButton, Keyboard.Modifiers);
                if( DispatchMouseInput(mouseInput) ) e.Handled = true;
            }

            // 押下状態更新
            mouseButtonState.IsPressed = false;
        }

        // ダブルクリック
        private void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("------------------------------------------------------------------");
            Debug.WriteLine("MainWindow_MouseDoubleClick   original source:" + e.OriginalSource.ToString() );
            //Debug.WriteLine("Keybord focus:" + Keyboard.FocusedElement.ToString() );
            Debug.WriteLine("------------------------------------------------------------------");

            // Menu上、Slider上、Button上では無効
            DependencyObject source = e.OriginalSource as DependencyObject;
            if( source == null ) return;
            Menu menu = source.FindAncestor<Menu>();
            if( menu != null) return;
            Slider slider = source.FindAncestor<Slider>();
            if( slider != null) return;
            Button button = source.FindAncestor<Button>();
            if( button != null ) return;

            // メニューが開いているなら無効
            if( MainWindow.Current.IsAnyToolbarMenuOpened ) return;

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


        /* ---------------------------------------------------- */
        //       EventHandler (マウスジェスチャ)
        /* ---------------------------------------------------- */
        private void MouseGestureStrokeChanged(object sender, EventArgs e)
        {
            // １つでもジェスチャがあれば、開始ボタンによるMouseUp時のコマンドは無効化
            mouseButtonStateSet.CommandExecuted(mouseGesture.StartingButton);

            // 長押しクリック判定解除
            StopLongClick();

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
            MainWindow.Current.NotificationBlock.Show(notification, NotificationPriority.Normal, NotificationTime.Eternally, NotificationType.GesturePreview);
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

                    MainWindow.Current.NotificationBlock.Hide(NotificationType.GesturePreview);

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
            MainWindow.Current.NotificationBlock.Hide(NotificationType.GesturePreview);

            if( gestureInput.Stroke.Length > 0 && gestureInput.Stroke != strokeOfLastExecutedCommand)
            {
                DispatchMouseGestureInput(gestureInput);
                return;
            }
        }


        /* ---------------------------------------------------- */
        //       EventHandler (その他)
        /* ---------------------------------------------------- */
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPos = Win32.GetCursorPos();
            if(currentPos.X != longClickStartPos.X || currentPos.Y != longClickStartPos.Y )
            {
                StopLongClick();
            }
        }

        // End of Class
    }
}
