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
        public Rotation Rotation;
        public ScaleTransform ScaleTransform;

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
