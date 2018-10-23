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

using C_SlideShow.Core;


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

        public override List<ImageFileContext> LoadImageFileContextList()
        {
            List<ImageFileContext> newList = new List<ImageFileContext>();
            try
            {
                foreach(IArchiveEntry entry in archive.Entries)
                {
                    // ファイル拡張子でフィルタ
                    if(  AllowedFileExt.Any( ext => entry.Key.ToLower().EndsWith(ext) ) )
                    {
                        // ロード
                        ImageFileContext ifc = new ImageFileContext(entry.Key);
                        ifc.Archiver = this;
                        ImageFileInfo fi = new ImageFileInfo();
                        fi.LastWriteTime = entry.LastModifiedTime;
                        fi.Length = entry.Size;
                        ifc.Info = fi;

                        newList.Add(ifc);
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
