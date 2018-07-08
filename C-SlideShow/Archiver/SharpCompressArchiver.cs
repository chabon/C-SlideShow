using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

using SharpCompress.Archives;


namespace C_SlideShow.Archiver
{
    public class SharpCompressArchiver : ArchiverBase
    {
        private IArchive archive;

        public SharpCompressArchiver(string archiverPath) : base(archiverPath)
        {
            LeaveHistory = true;

            try
            {
                archive = ArchiveFactory.Open(archiverPath);
            }
            catch
            {
                DisposeArchive();
            }
        }

        public override Stream OpenStream(string path)
        {
            IArchiveEntry entory = archive.Entries.First(e => e.Key == path);

            using (var rarStream = entory.OpenEntryStream())
            {
                try
                {
                    var ms = new MemoryStream();
                    rarStream.CopyTo(ms);
                    ms.Position = 0;

                    return ms;
                }
                catch
                {
                    return Stream.Null;
                }
            }
        }

        public override List<ImageFileInfo> LoadImageFileInfoList()
        {
            List<ImageFileInfo> newList = new List<ImageFileInfo>();
            try
            {
                foreach(IArchiveEntry entory in archive.Entries)
                {
                    // ファイル拡張子でフィルタ
                    if(  AllowedFileExt.Any( ext => entory.Key.ToLower().EndsWith(ext) ) )
                    {
                        // ロード
                        ImageFileInfo fi = new ImageFileInfo(entory.Key);
                        fi.LastWriteTime = entory.LastModifiedTime;
                        fi.Length = entory.Size;
                        fi.Archiver = this;
                        newList.Add(fi);
                    }
                }
            }
            catch
            {
                DisposeArchive();
                Debug.WriteLine("LoadImageFileInfoList() failed  path = " + ArchiverPath);
            }

            return newList;
        }

        public override void DisposeArchive()
        {
            if( archive != null ) archive.Dispose();
        }
    }
}
