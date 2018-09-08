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
    /// 画像を[]px下に移動
    /// </summary>
    public class MoveZoomImageToBottom : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; } = 1;
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public MoveZoomImageToBottom()
        {
            ID    = CommandID.MoveZoomImageToBottom;
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
            MainWindow.Current.TileExpantionPanel.Move(0, Value);

            return;
        }

        public string GetDetail()
        {
            return "画像を" + Value.ToString() + "px下に移動";
        }
    }
}
