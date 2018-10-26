using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Windows.Media;

using C_SlideShow.Core;


namespace C_SlideShow.Archiver
{
    public class ArchiverBase
    {
        public string ArchiverPath { get; set; }
        public static string[] AllowedFileExt = { ".jpg", ".png", ".jpeg", ".bmp", ".gif", ".ico" };
        public bool LeaveHistory { get; protected set; } = false;  // 履歴に残すかどうか
        public bool CanReadFile  { get; protected set; } = false;  // 圧縮書庫でないならtrue

        public ArchiverBase()
        {
        }

        public ArchiverBase(string archiverPath)
        {
            ArchiverPath = archiverPath;
        }

        public virtual Stream OpenStream(string path)
        {
            return Stream.Null;
        }

        public virtual void WriteAsFile(string path, string outputPath)
        {
            Stream st = OpenStream(path);
            FileStream fs = new FileStream(outputPath, FileMode.Create);

            int b;
            while( ( b = st.ReadByte() ) != -1 )
            {
                fs.WriteByte( (Byte)b );
            }

            fs.Close();
        }

        public virtual List<ImageFileContext> LoadImageFileContextList()
        {
            return new List<ImageFileContext>();
        }

        public virtual void DisposeArchive()
        {

        }
    }
}
