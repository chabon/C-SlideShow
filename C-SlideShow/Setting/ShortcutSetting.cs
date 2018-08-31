using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Windows.Input;

using C_SlideShow.Shortcut;


namespace C_SlideShow
{
    /// <summary>
    /// ショートカットのマッピング設定クラス
    /// </summary>
    [DataContract(Name = "ShortcutSetting")]
    public class ShortcutSetting
    {
        // コマンドMap
        [DataMember]
        public List<CommandMap> CommandMap = new List<CommandMap>();

        public ShortcutSetting()
        {
            initCommandMap();
        }

        private void initCommandMap()
        {
            CommandMap.Clear();
            CommandMap = CreateDefaultCommandMap();
        }

        public static List<CommandMap> CreateDefaultCommandMap()
        {
            List<CommandMap> defaultCommandMap = new List<CommandMap>();

            // 全般
            defaultCommandMap.Add( new CommandMap(CommandID.OpenFolder, 0, null,           new KeyInput(ModifierKeys.Control, Key.F), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenAdditionalFolder, 0, null, new KeyInput(ModifierKeys.Control | ModifierKeys.Shift, Key.F), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenFile, 0, null,             null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenAdditionalFile, 0, null,   null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.WindowSizeUp, 10, null,        null, null, new MouseGestureInput(MouseButton.Right, "[WU]")) );
            defaultCommandMap.Add( new CommandMap(CommandID.WindowSizeDown, 10, null,      null, null, new MouseGestureInput(MouseButton.Right, "[WD]")) );

            // 通常時
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToForward, 0, null,             new KeyInput(ModifierKeys.None, Key.F), new MouseInput(MouseInputButton.WheelDown, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToBackward, 0, null,            new KeyInput(ModifierKeys.None, Key.B), new MouseInput(MouseInputButton.WheelUp, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToLeft, 0, null,                new KeyInput(ModifierKeys.None, Key.Left), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToTop, 0, null,                 new KeyInput(ModifierKeys.None, Key.Up), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToRight, 0, null,               new KeyInput(ModifierKeys.None, Key.Right), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToBottom, 0, null,              new KeyInput(ModifierKeys.None, Key.Down), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToForwardByOneImage, 0, null,   new KeyInput(ModifierKeys.Shift, Key.F), new MouseInput(MouseInputButton.WheelDown, ModifierKeys.Shift), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToBackwardByOneImage, 0, null,  new KeyInput(ModifierKeys.Shift, Key.B), new MouseInput(MouseInputButton.WheelUp, ModifierKeys.Shift), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ZoomImageUnderCursor, 0, null,       null, new MouseInput(MouseInputButton.R_Click, ModifierKeys.None), null) );

            // 画像拡大時
            defaultCommandMap.Add( new CommandMap(CommandID.ZoomInImage, 50, null,              new KeyInput(ModifierKeys.None, Key.Up), new MouseInput(MouseInputButton.WheelUp, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ZoomOutImage, 50, null,             new KeyInput(ModifierKeys.None, Key.Down), new MouseInput(MouseInputButton.WheelDown, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ExitZoom, 0, null,                  null, new MouseInput(MouseInputButton.R_Click, ModifierKeys.None), null) );


            return defaultCommandMap;
        }
    }
}
