using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// ファイル情報の表示 ON/OFF
    /// </summary>
    public class ToggleDisplayOfFileInfo : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ToggleDisplayOfFileInfo()
        {
            ID    = CommandID.ToggleDisplayOfFileInfo;
            Scene = Scene.Expand;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow.Current.Setting.ShowFileInfoInTileExpantionPanel = !MainWindow.Current.Setting.ShowFileInfoInTileExpantionPanel;
            MainWindow.Current.TileExpantionPanel.UpdateFileInfoAreaVisiblity();

            return;
        }

        public string GetDetail()
        {
            return "ファイル情報の表示 ON/OFF";
        }
    }
}
