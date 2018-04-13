using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

using System.Windows.Controls;

namespace C_SlideShow
{
    public class Tile
    {
        public Image Image { get; private set; }
        public Border Border { get; private set; }
        public int Row { set; get; }
        public int Col { set; get; }
        public bool ByPlayback { get; set; } // タイルの設置が巻き戻しによるものか
        public string filePath { get; set; }
        public bool IsDummy { get; set; }

        public Tile()
        {
            Image = new Image();
            Border = new Border();
            IsDummy = false;
        }


        public void SetGridPos(int row, int col, bool byPlayback)
        {
            Row = row;
            Col = col;
            ByPlayback = byPlayback;
        }
        
    }
}
