using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 画像の拡大を終了
    /// </summary>
    public class ExitZoom : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public ExitZoom()
        {
            ID    = CommandID.ExitZoom;
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
            MainWindow.Current.TileExpantionPanel.Hide();

            return;
        }

        public string GetDetail()
        {
            return "画像の拡大を終了";
        }
    }
}
