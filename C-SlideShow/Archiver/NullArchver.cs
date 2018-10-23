using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using C_SlideShow.Core;


namespace C_SlideShow.Archiver
{
    /// <summary>
    /// アーカイバを持たないことを示すオブジェクト
    /// </summary>
    public class NullArchiver : ArchiverBase
    {
        public NullArchiver() : base()
        {
            CanReadFile  = true;
        }

        public override Stream OpenStream(string path)
        {
            if( File.Exists(path) )
                return File.OpenRead(path);
            else
                return Stream.Null;
        }

        public ImageFileContext LoadImageFileContext(string filePath)
        {
            // 拡張子でフィルタ
            if( !AllowedFileExt.Any(ext => filePath.ToLower().EndsWith(ext)) )
                return null;

            ImageFileContext imageFileContext = new ImageFileContext(filePath);
            imageFileContext.Archiver = this;

            return imageFileContext;
        }

    }
}
