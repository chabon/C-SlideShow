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
    /// カーソル下の画像を規定のプログラムで開く
    /// </summary>
    public class OpenImageUnderCursorByDefaultApp : ICommand
    {
        private MainWindow mw;

        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public OpenImageUnderCursorByDefaultApp()
        {
            ID    = CommandID.OpenImageUnderCursorByDefaultApp;
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
                targetTile.OpenByExternalApp( new ExternalAppInfo() );
            }
        }

        public string GetDetail()
        {
            return "カーソル下の画像を規定のプログラムで開く";
        }
    }
}
