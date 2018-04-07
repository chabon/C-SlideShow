using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;


namespace C_SlideShow
{
    public class ImageFileInfo
    {
        public string FilePath;
        public BitmapImage bitmapBuffer;
        public DateTimeOffset LastWriteTime;
        public DateTimeOffset CreationTime;

        public ImageFileInfo()
        {
            bitmapBuffer = null;
        }

        public ImageFileInfo(string _filePath)
        {
            this.FilePath = _filePath;
        }
    }
}
