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

        public ZoomOutImage()
        {
            ID    = CommandID.ZoomOutImage;
            Scene = Scene.Expand;
        }
        
        public bool CanExecute()
        {
            if( MainWindow.Current.TileExpantionPanel.IsShowing )
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
            MainWindow.Current.TileExpantionPanel.ZoomOut();

            return;
        }

        public string GetDetail()
        {
            return "画像の拡大率をダウン";
        }
    }
}
