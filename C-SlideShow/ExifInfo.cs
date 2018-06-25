using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace C_SlideShow
{
    public class ExifInfo
    {
        // 静的で空の C_SlideShow.ExifInfo を表す値を取得します。
        public static ExifInfo Empty { get; }

        public Rotation Rotation;               // 回転情報
        public ScaleTransform ScaleTransform;   // 反転情報

        public ExifInfo()
        {
            Rotation = Rotation.Rotate0;
            ScaleTransform = null;
        }

        public ExifInfo(Rotation _rotation, ScaleTransform st)
        {
            Rotation = _rotation;
            ScaleTransform = st;
        }
    }
}
