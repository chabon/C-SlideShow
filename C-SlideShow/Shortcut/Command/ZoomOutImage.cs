using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 拡大率をダウン
    /// </summary>
    public class ZoomOutImage : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public ZoomOutImage()
        {
            ID    = CommandID.ZoomOutImage;
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
            MainWindow.Current.TileExpantionPanel.ZoomOut(param);

            return;
        }

        public string GetDetail()
        {
            return "画像の拡大率を" + Value.ToString() + "%ダウン";
        }
    }
}
