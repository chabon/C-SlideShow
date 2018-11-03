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

        [OnDeserialized]
        public void DefaultDeserialized(StreamingContext sc)
        {
            // デシリアライズ後、1つもないコマンドは補填
            bool compensated = false;
            foreach(  CommandID id in Enum.GetValues( typeof(CommandID) )  )
            {
                if( CommandMap.Any(m => m.CommandID == id) ) continue;
                else
                {
                    int     defaultValue = 0;
                    string  defaultStrValue = null;

                    C_SlideShow.Shortcut.ICommand cmd = CommandFactory.CreateById(id);
                    if( cmd.EnableValue ) defaultValue = cmd.Value;
                    if( cmd.EnableStrValue ) defaultStrValue = cmd.StrValue;

                    CommandMap.Add( new CommandMap(id, defaultValue, defaultStrValue, null, null, null) );
                    compensated = true;
                }
            }

            // 補填されたならID順でソート
            if( compensated )
            {
                CommandMap = CommandMap.OrderBy(m => m.CommandID).ToList();
            }
        }

        public static List<CommandMap> CreateDefaultCommandMap()
        {
            List<CommandMap> defaultCommandMap = new List<CommandMap>();

            // いつでも
            defaultCommandMap.Add( new CommandMap(CommandID.OpenFolder,                         0,  null,       new KeyInput(ModifierKeys.Control, Key.F), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenAdditionalFolder,               0,  null,       new KeyInput(ModifierKeys.Control | ModifierKeys.Shift, Key.F), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenFile,                           0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenAdditionalFile,                 0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.Reload,                             0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ToggleTopMost,                      0,  null,       new KeyInput(ModifierKeys.None, Key.T), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ToggleFullScreen,                   0,  null,       new KeyInput(ModifierKeys.Alt, Key.Return), new MouseInput(MouseInputButton.L_DoubleClick, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.WindowSizeUp,                       10, null,       null, null, new MouseGestureInput(MouseButton.Right, "[WU]")) );
            defaultCommandMap.Add( new CommandMap(CommandID.WindowSizeDown,                     10, null,       null, null, new MouseGestureInput(MouseButton.Right, "[WD]")) );
            defaultCommandMap.Add( new CommandMap(CommandID.ShowContextMenu,                    0,  null,       null, new MouseInput(MouseInputButton.L_LongClick, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenFolderByExplorer,               0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenNextArchiver,                   0,  null,       new KeyInput(ModifierKeys.Control, Key.N), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenPrevArchiver,                   0,  null,       new KeyInput(ModifierKeys.Control, Key.P), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ShowAppSettingDialog,               0,  null,       new KeyInput(ModifierKeys.None, Key.O), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.CloseFile,                          0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.MinimizeWindow,                     0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ExitApp,                            0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenImageUnderCursorByExplorer,     0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenImageUnderCursorByDefaultApp,   0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OpenImageUnderCursorByExternalApp,  0,  "未指定",   null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.CopyImageFileUnderCursor,           0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.CopyImageDataUnderCursor,           0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.CopyImageFilePathUnderCursor,       0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.CopyImageFileNameUnderCursor,       0,  null,       null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.LoadProfileFromNum,                 1,  null,       new KeyInput(ModifierKeys.Control, Key.D1), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.LoadProfileFromNum,                 2,  null,       new KeyInput(ModifierKeys.Control, Key.D2), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.LoadProfileFromNum,                 3,  null,       new KeyInput(ModifierKeys.Control, Key.D3), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.LoadProfileFromNum,                 4,  null,       new KeyInput(ModifierKeys.Control, Key.D4), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.LoadProfileFromName,                0,  "未指定",   null, null, null) );

            // 通常時
            defaultCommandMap.Add( new CommandMap(CommandID.ToggleSlideShowPlay,                0, null,  new KeyInput(ModifierKeys.None, Key.Space), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ZoomImageUnderCursor,               0, null,  null, new MouseInput(MouseInputButton.R_Click, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToForward,                     0, null,  new KeyInput(ModifierKeys.None, Key.F), new MouseInput(MouseInputButton.WheelDown, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToBackward,                    0, null,  new KeyInput(ModifierKeys.None, Key.B), new MouseInput(MouseInputButton.WheelUp, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToLeft,                        0, null,  new KeyInput(ModifierKeys.None, Key.Left), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToRight,                       0, null,  new KeyInput(ModifierKeys.None, Key.Right), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToTop,                         0, null,  new KeyInput(ModifierKeys.None, Key.Up), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToBottom,                      0, null,  new KeyInput(ModifierKeys.None, Key.Down), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToCursorDirection,             0, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToCursorDirectionRev,          0, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToForwardByOneImage,           0, null,  new KeyInput(ModifierKeys.Shift, Key.F), new MouseInput(MouseInputButton.WheelDown, ModifierKeys.Shift), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.SlideToBackwardByOneImage,          0, null,  new KeyInput(ModifierKeys.Shift, Key.B), new MouseInput(MouseInputButton.WheelUp, ModifierKeys.Shift), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ChangeSlideDirectionToLeft,         0, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ChangeSlideDirectionToRight,        0, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ChangeSlideDirectionToTop,          0, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ChangeSlideDirectionToBottom,       0, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ChangeSlideDirectionToRev,          0, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ShiftForward,                       1, null,  new KeyInput(ModifierKeys.None, Key.Enter), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ShiftBackward,                      1, null,  new KeyInput(ModifierKeys.None, Key.Back), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.AddColumn,                          1, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.AddRow,                             1, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ReduceColumn,                       1, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ReduceRow,                          1, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.AddColumnAndRow,                    1, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ReduceColumnAndRow,                 1, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ChangeNumOfColumn,                  2, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ChangeNumOfRow,                     2, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ToggleTileImageStretch,             0, null,  null, null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OepnSubMenu_Load,                   0, null,  new KeyInput(ModifierKeys.Alt, Key.L), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OepnSubMenu_AspectRatio,            0, null,  new KeyInput(ModifierKeys.Alt, Key.A), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.OepnSubMenu_Profile,                0, null,  new KeyInput(ModifierKeys.Alt, Key.P), null, null) );

            // 画像拡大時
            defaultCommandMap.Add( new CommandMap(CommandID.ExitZoom,                   0,  null,   new KeyInput(ModifierKeys.None, Key.Escape), new MouseInput(MouseInputButton.R_Click, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ZoomInImage,                50, null,   null, new MouseInput(MouseInputButton.WheelUp, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ZoomOutImage,               50, null,   null, new MouseInput(MouseInputButton.WheelDown, ModifierKeys.None), null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ZoomReset,                   0, null,   new KeyInput(ModifierKeys.None, Key.R), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.GoToForwardImage,            1, null,   new KeyInput(ModifierKeys.None, Key.Enter), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.GoToBackwardImage,           1, null,   new KeyInput(ModifierKeys.None, Key.Back), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.MoveZoomImageToLeft,        50, null,   new KeyInput(ModifierKeys.None, Key.Right), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.MoveZoomImageToRight,       50, null,   new KeyInput(ModifierKeys.None, Key.Left), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.MoveZoomImageToTop,         50, null,   new KeyInput(ModifierKeys.None, Key.Down), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.MoveZoomImageToBottom,      50, null,   new KeyInput(ModifierKeys.None, Key.Up), null, null) );
            defaultCommandMap.Add( new CommandMap(CommandID.ToggleDisplayOfFileInfo,    0,  null,   new KeyInput(ModifierKeys.None, Key.Space), null, null) );

            return defaultCommandMap;
        }
    }
}
