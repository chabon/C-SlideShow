using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;


namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 画像を[]px左に移動
    /// </summary>
    public class MoveZoomImageToLeft : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; } = 1;
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public MoveZoomImageToLeft()
        {
            ID    = CommandID.MoveZoomImageToLeft;
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
            MainWindow.Current.TileExpantionPanel.Move(-Value, 0);

            return;
        }

        public string GetDetail()
        {
            return "画像を" + Value.ToString() + "px左に移動";
        }
    }
}
