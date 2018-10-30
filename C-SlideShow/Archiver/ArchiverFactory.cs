using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Archiver
{
    public static class ArchiverFactory
    {
        public static ArchiverBase Create(string path, string ext)
        {
            // 対象外の拡張子
            if( ArchiverBase.AllowedArchiverExt.All(e => e != ext) ) return null;

            switch( ext )
            {
                case ".zip":
                    return new ZipArchiver(path);
                case ".rar":
                    return new RarArchiver(path);
                case ".7z":
                    return new SevenZipArchiver(path);
                case ".tar":
                    return new TarArchiver(path);
                case ".pdf":
                    return new PdfArchiver(path);
                default:
                    return null;
            }

        }

        public static ArchiverBase Create(string path)
        {
            string ext = System.IO.Path.GetExtension(path);
            return Create(path, ext);
        }
    }
}
