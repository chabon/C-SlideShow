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

        public override List<ImageFileInfo> LoadImageFileInfoList()
        {
            List<ImageFileInfo> newList = new List<ImageFileInfo>();
            try
            {
                // エントリ
                var entries = GetEntries();

                // ロード
                foreach(ZipArchiveEntry entory in entries)
                {
                    // ファイル拡張子でフィルタ
                    if(  AllowedFileExt.Any( ext => entory.FullName.ToLower().EndsWith(ext) ) )
                    {
                        ImageFileInfo fi = new ImageFileInfo(entory.FullName);
                        fi.LastWriteTime = entory.LastWriteTime;
                        fi.Length = entory.Length;
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
