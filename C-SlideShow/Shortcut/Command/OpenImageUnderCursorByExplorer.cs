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
    /// カーソル下の画像をエクスプローラーで開く
    /// </summary>
    public class OpenImageUnderCursorByExplorer : ICommand
    {
        private MainWindow mw;

        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public OpenImageUnderCursorByExplorer()
        {
            ID    = CommandID.OpenImageUnderCursorByExplorer;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            mw = MainWindow.Current;
            Tile targetTile;
            if( mw.TileExpantionPanel.IsShowing )
            {
                targetTile = mw.TileExpantionPanel.TargetTile;
            }
            else
            {
                targetTile = mw.GetTileUnderCursor();
            }

            if(targetTile != null)
            {
                targetTile.OpenExplorer();
            }
        }

        public string GetDetail()
        {
            return "カーソル下の画像をエクスプローラーで開く";
        }
    }
}
