using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;


namespace C_SlideShow.Archiver
{
    /// <summary>
    /// アーカイバを持たないことを示すオブジェクト
    /// </summary>
    public class NullArchiver : ArchiverBase
    {
        public override Stream OpenStream(string path)
        {
            if( File.Exists(path) )
                return File.OpenRead(path);
            else
                return Stream.Null;
        }

        public ImageFileInfo LoadImageFileInfo(string filePath)
        {
            // 拡張子でフィルタ
            if( !AllowedFileExt.Any(ext => filePath.ToLower().EndsWith(ext)) )
                return null;

            ImageFileInfo imageFileInfo = new ImageFileInfo();
            imageFileInfo.FilePath = filePath;
            imageFileInfo.Archiver = this;

            return imageFileInfo;
        }

    }
}
