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
    public class WindowDragMove : Drag
    {
        // フィールド
        private Point ptWindowPrev;

        // プロパティ
        public WindowSnap WindowSnap { get; private set; }

        // コンストラクタ
        public WindowDragMove(Window window) : base(window)
        {
            DragStart   += WindowDragMove_DragStart;
            DragMoving  += WindowDragMove_DragMoving;
        }

        private void WindowDragMove_DragStart(object sender, EventArgs e)
        {
            ptWindowPrev = new Point(targetWindow.Left, targetWindow.Top);
            if( WindowSnap == null ) WindowSnap = new WindowSnap(targetWindow);

            // ウインドウスナップ有効or無効
            WindowSnap.EnableScreenSnap = MainWindow.Current.Setting.EnableScreenSnap;
            WindowSnap.EnableWindowSnap = MainWindow.Current.Setting.EnableWindowSnap;
            WindowSnap.Range_Screen  = MainWindow.Current.Setting.ScreenSnapRange;
            WindowSnap.Range_Window  = MainWindow.Current.Setting.WindowSnapRange;
        }

        private void WindowDragMove_DragMoving(object sender, EventArgs e)
        {
            Rect rcDest = new Rect() { X = ptWindowPrev.X + ptDragMovingDiff.X, Y = ptWindowPrev.Y + ptDragMovingDiff.Y, Width = targetWindow.Width, Height = targetWindow.Height };

            if( !WindowSnap.OnWindowMoving(rcDest) )
            {
                targetWindow.Left = rcDest.Left;
                targetWindow.Top = rcDest.Top;
            }
        }

    }
}
