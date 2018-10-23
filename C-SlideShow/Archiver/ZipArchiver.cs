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

using C_SlideShow.Core;


namespace C_SlideShow.Archiver
{
    public class ZipArchiver : ArchiverBase
    {
        private ZipArchive archive;

        public ZipArchiver(string archiverPath) : base(archiverPath)
        {
            LeaveHistory = true;

            try
            {
                archive = ZipFile.OpenRead(archiverPath);
            }
            catch
            {
                DisposeArchive();
            }
        }

        public override Stream OpenStream(string path)
        {
            ZipArchiveEntry entory = GetEntry(path);

            using (var zipStream = entory.Open())
            {
                try
                {
                    var ms = new MemoryStream();
                    zipStream.CopyTo(ms);
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
                // エントリ
                var entries = GetEntries();

                foreach(ZipArchiveEntry entory in entries)
                {
                    // ファイル拡張子でフィルタ
                    if(  AllowedFileExt.Any( ext => entory.FullName.ToLower().EndsWith(ext) ) )
                    {
                        // ロード
                        ImageFileContext ifc = new ImageFileContext(entory.FullName);
                        ifc.Archiver = this;
                        ImageFileInfo fi = new ImageFileInfo();
                        fi.LastWriteTime = entory.LastWriteTime;
                        fi.Length = entory.Length;

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

        public ZipArchiveEntry GetEntry(string filePath)
        {
            return archive.GetEntry(filePath);
        }

        public ReadOnlyCollection<ZipArchiveEntry> GetEntries()
        {
            return archive.Entries;
        }

        public override void DisposeArchive()
        {
            if( archive != null ) archive.Dispose();
        }
    }
}
