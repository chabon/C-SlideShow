using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;
using System.Diagnostics;

namespace C_SlideShow.Shortcut.Drag
{
    public class TileExpantionPanelDragMove : Drag
    {
        // フィールド
        private TileExpantionPanel panel;
        private Point panelPosInDragStart;

        // コンストラクタ
        public TileExpantionPanelDragMove(Window window) : base(window)
        {
            panel = MainWindow.Current.TileExpantionPanel;

            DragStart       += WindowDragMove_DragStart;
            DragMoving      += WindowDragMove_DragMoving;
            DragMoved       += TileExpantionPanelDragMove_DragMoved;
            DragCanceled    += TileExpantionPanelDragMove_DragCanceled;
        }


        private void WindowDragMove_DragStart(object sender, EventArgs e)
        {
            panelPosInDragStart = new Point(panel.ExpandedBorder.Margin.Left, panel.ExpandedBorder.Margin.Top);
            MainWindow.Current.Cursor = Cursors.SizeAll;
        }


        private void WindowDragMove_DragMoving(object sender, EventArgs e)
        {
            panel.MoveTo(panelPosInDragStart.X + ptDragMovingDiff.X, panelPosInDragStart.Y + ptDragMovingDiff.Y);
        }


        private void TileExpantionPanelDragMove_DragMoved(object sender, EventArgs e)
        {
            MainWindow.Current.Cursor = Cursors.Arrow;
        }


        private void TileExpantionPanelDragMove_DragCanceled(object sender, EventArgs e)
        {
            MainWindow.Current.Cursor = Cursors.Arrow;
        }
    }
}
