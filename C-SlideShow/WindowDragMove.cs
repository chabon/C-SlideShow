using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;

namespace C_SlideShow
{
    public class WindowDragMove
    {
        Window targetWindow;
        bool   bDragStart = false;
        Point  ptDragStart;
        Point  ptWindowPrev;

        Point  ptMaxDiff; // ドラッグ開始時からの最大移動量
        const double thresholdOfMaxDiff = 0.5; // DragMovedイベントを発生させるしきい値

        public event EventHandler DragMoved;

        public WindowDragMove(Window window)
        {
            targetWindow = window;

            targetWindow.MouseLeftButtonDown += TargetWindow_MouseLeftButtonDown;
            targetWindow.MouseMove           += TargetWindow_MouseMove;
            targetWindow.MouseLeftButtonUp   += TargetWindow_MouseLeftButtonUp;
            targetWindow.MouseLeave          += TargetWindow_MouseLeave;
        }

        private void TargetWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ptDragStart  = Win32.GetCursorPos();
            ptWindowPrev = new Point(targetWindow.Left, targetWindow.Top);
            ptMaxDiff    = new Point(0, 0);
            bDragStart   = true;
        }

        private void TargetWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if( bDragStart )
            {
                Point ptCurrent = Win32.GetCursorPos();
                Point ptDiff    = new Point(ptCurrent.X - ptDragStart.X, ptCurrent.Y - ptDragStart.Y);

                if( ptMaxDiff.X < Math.Abs(ptDiff.X) ) ptMaxDiff.X = Math.Abs(ptDiff.X);
                if( ptMaxDiff.Y < Math.Abs(ptDiff.Y) ) ptMaxDiff.Y = Math.Abs(ptDiff.Y);

                targetWindow.Left = ptWindowPrev.X + ptDiff.X;
                targetWindow.Top  = ptWindowPrev.Y + ptDiff.Y;
            }
        }

        private void TargetWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if( ptMaxDiff.X > thresholdOfMaxDiff || ptMaxDiff.Y > thresholdOfMaxDiff )
            {
                DragMoved?.Invoke( this, new EventArgs() );
            }
            bDragStart = false;
        }

        private void TargetWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            bDragStart = false;
        }

    }
}
