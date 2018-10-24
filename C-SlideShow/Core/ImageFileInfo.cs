using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;


namespace C_SlideShow.Core
{
    public class ImageFileInfo
    {
        public DateTimeOffset?  LastWriteTime   = null;             // 更新日時
        public DateTimeOffset?  CreationTime    = null;             // 作成日時
        public long             Length          = 0;                // ファイルサイズ(byte)
        public Size             PixelSize       = Size.Empty;       // ピクセルサイズ
        public ExifInfo         ExifInfo        = ExifInfo.Empty;   // Exif情報
    }



}
