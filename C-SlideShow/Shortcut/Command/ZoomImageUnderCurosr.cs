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

            // MainWindow上の座標取得
            Point p = Mouse.GetPosition(mw);

            // カーソル下のオブジェクトを取得
            //VisualTreeHelper.HitTest(mw, null, new HitTestResultCallback(OnHitTestResultCallback), new PointHitTestParameters(p));
            IInputElement ie = mw.InputHitTest(p);
            DependencyObject source = ie as DependencyObject;
            if( source == null ) return;

            // クリックされたTileContainer
            TileContainer tc = WpfTreeUtil.FindAncestor<TileContainer>(source);
            if( tc == null ) return;

            // クリックされたBorder
            Border border;
            if( source is Border ) border = source as Border;
            else
            {
                border = WpfTreeUtil.FindAncestor<Border>(source);
            }
            if( border == null ) return;

            // 紐づけられているTileオブジェクトを特定
            Tile targetTile = tc.Tiles.FirstOrDefault(t => t.Border == border);
            if( targetTile == null ) return;

            Debug.WriteLine(targetTile.ToString());
            // 表示
            mw.TileExpantionPanel.Show(targetTile);
        }

        public string GetDetail()
        {
            return "カーソル下の画像を全体に拡大";
        }
    }
}
