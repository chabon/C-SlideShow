using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;


namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// カーソル下の画像を全体に拡大
    /// </summary>
    public class ZoomImageUnderCursor : ICommand
    {
        private MainWindow mw;

        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ZoomImageUnderCursor()
        {
            ID    = CommandID.ZoomImageUnderCursor;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            mw = MainWindow.Current;
            if( mw.TileExpantionPanel.IsShowing ) return;

            Tile targetTile = mw.GetTileUnderCursor();

            if(targetTile != null )
            {
                // 表示
                Debug.WriteLine(targetTile.ToString());
                mw.TileExpantionPanel.Show(targetTile);
            }
        }

        public string GetDetail()
        {
            return "カーソル下の画像を全体に拡大";
        }
    }
}
