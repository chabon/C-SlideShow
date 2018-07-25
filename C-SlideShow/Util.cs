using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Media;

namespace C_SlideShow
{
    public static class Util
    {
        public static DrawingBrush CreatePlaidBrush(Color color1, Color color2)
        {
            // Create a DrawingBrush and use it to
            // paint the rectangle.
            DrawingBrush drBrush = new DrawingBrush();

            double sqSize = 1.0;
            GeometryDrawing backgroundSquare =
                new GeometryDrawing(
                    new SolidColorBrush(color1),
                    null,
                    new RectangleGeometry(new Rect(0, 0, sqSize * 2, sqSize * 2)));

            GeometryGroup aGeometryGroup = new GeometryGroup();
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, sqSize, sqSize)));
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(sqSize, sqSize, sqSize, sqSize)));

            SolidColorBrush checkerBrush = new SolidColorBrush(color2);
            GeometryDrawing checkers = new GeometryDrawing(checkerBrush, null, aGeometryGroup);

            DrawingGroup checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(backgroundSquare);
            checkersDrawingGroup.Children.Add(checkers);

            drBrush.Drawing = checkersDrawingGroup;
            //drBrush.Viewport = new Rect(0, 0, viewport, viewport);
            drBrush.Viewport = new Rect(0, 0, 30, 30);
            drBrush.ViewportUnits = BrushMappingMode.Absolute;
            drBrush.TileMode = TileMode.Tile;

            return drBrush;
        }

        /// <summary>
        /// ワーキングエリアをはみ出さないように補正されたウインドウ矩形を取得
        /// </summary>
        /// <param name="input">移動する予定のウインドウ座標(矩形)</param>
        /// <returns>補正されたRect</returns>
        public static Rect GetCorrectedWindowRect(Rect input)
        {
            // Rect → RECT構造体
            Win32.RECT rcInput;
            rcInput.left = (int)input.Left;
            rcInput.top = (int)input.Top;
            rcInput.right = (int)input.Right;
            rcInput.bottom = (int)input.Bottom;

            //移動先モニターの取得(なければ近くのを取得)
            IntPtr hMonitor;
            Win32.MonitorInfoEx mi;
            mi.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32.MonitorInfoEx));
            hMonitor = Win32.MonitorFromRect(out rcInput, Win32.MONITOR_DEFAULTTONEAREST);
            Win32.GetMonitorInfo(hMonitor, out mi);

            // 補正する値(ワーキングエリアに収めるために、ずらす座標の値)を算出
            Win32.POINT ptCorrected;
            ptCorrected.X = ptCorrected.Y = 0;

            if (rcInput.right > mi.rcWork.right) //右
            {
                ptCorrected.X = mi.rcWork.right - rcInput.right;
            }
            if (rcInput.left < mi.rcWork.left) //左
            {
                ptCorrected.X = mi.rcWork.left - rcInput.left;
            }
            if (rcInput.bottom > mi.rcWork.bottom) //下
            {
                ptCorrected.Y = mi.rcWork.bottom - rcInput.bottom;
            }
            if (rcInput.top < mi.rcWork.top) //上
            {
                ptCorrected.Y = mi.rcWork.top - rcInput.top;
            }

            return new Rect( input.Left + ptCorrected.X, input.Top + ptCorrected.Y, input.Width, input.Height );
        }

        /// <summary>
        /// ファイル名として無効な文字を「_」に置き換える
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ValidFileName (string s)
        {
            string valid = s;
            char[] invalidch = System.IO.Path.GetInvalidFileNameChars();

            foreach (char c in invalidch)
            {
                valid = valid.Replace(c, '_');
            }
            return valid;
        }

    }
}
