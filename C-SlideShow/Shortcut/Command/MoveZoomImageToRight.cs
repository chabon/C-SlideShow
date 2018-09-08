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
    /// 画像を[]px右に移動
    /// </summary>
    public class MoveZoomImageToRight : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; } = 1;
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public MoveZoomImageToRight()
        {
            ID    = CommandID.MoveZoomImageToRight;
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
            MainWindow.Current.TileExpantionPanel.Move(Value, 0);

            return;
        }

        public string GetDetail()
        {
            return "画像を" + Value.ToString() + "px右に移動";
        }
    }
}
