using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 拡大率をリセット
    /// </summary>
    public class ZoomReset : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ZoomReset()
        {
            ID    = CommandID.ZoomReset;
            Scene = Scene.Expand;
        }
        
        public bool CanExecute()
        {
            if( MainWindow.Current.TileExpantionPanel.IsShowing && MainWindow.Current.TileExpantionPanel.IsAnimationCompleted )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Execute()
        {
            MainWindow.Current.TileExpantionPanel.ResetZoomAndMove();

            return;
        }

        public string GetDetail()
        {
            return "拡大率をリセット";
        }
    }
}
