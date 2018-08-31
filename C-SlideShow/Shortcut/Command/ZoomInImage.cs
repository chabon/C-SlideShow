using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 拡大率をアップ
    /// </summary>
    public class ZoomInImage : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public ZoomInImage()
        {
            ID    = CommandID.ZoomInImage;
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
            double param = Value / 100.0;
            if( param < 0 ) param = 0;
            else if( param > 10 ) param = 10;

            MainWindow.Current.TileExpantionPanel.ZoomIn(param);

            return;
        }

        public string GetDetail()
        {
            return "画像の拡大率を" + Value.ToString() + "%アップ";
        }
    }
}
