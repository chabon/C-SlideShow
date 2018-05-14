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
    }
}
