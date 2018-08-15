using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;

using C_SlideShow.Shortcut.Command;


namespace C_SlideShow.Shortcut
{
    /// <summary>
    /// ショートカット全般を管理
    /// </summary>
    class ShortcutManager
    {
        // コマンドリスト
        List<ICommand> commands;

        // ショートカット設定
        ShortcutSetting shortcutSetting;

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
        // イベントハンドラの初期化
        private void InitEventHandler()
        {
            MainWindow.Current.PreviewKeyDown += this.MainWindow_PreviewKeyDown;
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
            commands.Add( new IncreaseWindowSize() );
            commands.Add( new DecreaseWindow() );

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
                command.Execute();
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

        /* ---------------------------------------------------- */
        //       EventHandler
        /* ---------------------------------------------------- */
        // キー押下イベント
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // キー情報取得
            ModifierKeys modKeys = Keyboard.Modifiers;
            KeyInput keyInput = new KeyInput(modKeys, e.Key);
            System.Diagnostics.Debug.WriteLine( "key: " + keyInput.Key.ToString() + "\nmod: " + keyInput.Modifiers.ToString() );

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
                        return;
                    }
                }
            }

        }


        // End of Class
    }
}
