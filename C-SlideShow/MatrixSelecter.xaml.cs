using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace C_SlideShow
{
    /// <summary>
    /// MatrixSelecter.xaml の相互作用ロジック
    /// </summary>
    public partial class MatrixSelecter : UserControl
    {
        const int maxSize = 6;
        Rectangle[,] rects = new Rectangle[maxSize, maxSize];
        Rectangle rect_mouseLbuttonDown;

        public event EventHandler MatrixSelected;
        public int RowValue { get; set; } 
        public int ColValue { get; set; }
        public Color SelectColor { get; set; }
        public Color BaseColor { get; set; }



        public MatrixSelecter()
        {
            InitializeComponent();

            //MainGrid.Height = Width * 3 / 4;
            //Height = Width * 9 / 16;

            this.SelectColor = Colors.DarkGray;
            this.BaseColor = Colors.LightGray;

            for (int i=0; i< maxSize; i++)
            {
                ColumnDefinition c = new ColumnDefinition();
                MainGrid.ColumnDefinitions.Add(c);
            }

            for(int i=0; i< maxSize; i++)
            {
                RowDefinition r = new RowDefinition();
                MainGrid.RowDefinitions.Add(r);
            }

            for(int i=0; i<maxSize; i++)
            {
                for(int j=0; j<maxSize; j++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Fill = new SolidColorBrush(BaseColor);
                    rect.Margin = new Thickness(1);
                    rect.PreviewMouseMove += Rectangle_MouseMoved;
                    rect.PreviewMouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
                    rect.PreviewMouseLeftButtonUp += Rectangle_MouseLeftButtonUp;
                    rects[i, j] = rect;
                    Grid.SetColumn(rect, j);
                    Grid.SetRow(rect, i);
                    MainGrid.Children.Add(rect);
                }
            }
        }


        private void Rectangle_MouseMoved(object sender, MouseEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            if (rect == null) return;

            int numofRow = Grid.GetRow(rect) + 1;
            int numofCol = Grid.GetColumn(rect) + 1;
            SetMatrix(numofCol, numofRow);
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            if (rect == null) return;

            rect_mouseLbuttonDown = rect;

            e.Handled = true;
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            Rectangle rect = sender as Rectangle;
            if (rect == null) return;

            if (! ( rect_mouseLbuttonDown == rect) ) return;

            for(int i=0; i<maxSize; i++)
            {
                for(int j=0; j<maxSize; j++)
                {
                    if(rects[i,j] == rect)
                    {
                        this.RowValue = i + 1;
                        this.ColValue = j + 1;
                    }
                }
            }

            e.Handled = true;
            MatrixSelected?.Invoke(this, EventArgs.Empty);
        }

        public void SetMatrix(int numofCol, int numofRow)
        {
            for(int i=0; i<maxSize; i++)
            {
                for(int j=0; j<maxSize; j++)
                {
                    rects[i, j].Fill = new SolidColorBrush(BaseColor);
                }
            }

            for(int i=0; i<numofRow; i++)
            {
                for(int j=0; j<numofCol; j++)
                {
                    rects[i, j].Fill = new SolidColorBrush(SelectColor);
                }
            }

            this.MatrixInfoLabel.Content = String.Format("{0} × {1}", numofCol, numofRow);
        }
    }
}
