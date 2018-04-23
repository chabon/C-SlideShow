using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow
{
    public class ImageFileInfo
    {
        public string FilePath;
        public DateTimeOffset LastWriteTime;
        public DateTimeOffset CreationTime;
        public long Length = 0; // ファイルサイズ(byte)

        public ImageFileInfo()
        {

        }

        public ImageFileInfo(string _filePath)
        {
            this.FilePath = _filePath;
        }
    }
}
