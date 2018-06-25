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
            try
            {
                archive = ZipFile.OpenRead(archiverPath);
            }
            catch
            {
                DisposeArchive();
            }
        }

        public override BitmapSource LoadBitmap(ImageFileInfo imageFileInfo, int bitmapDecodePixelWidth, bool applyRotateInfoFromExif)
        {
            string filePath = imageFileInfo.FilePath;
            if (filePath == ImageFileManager.DummyFilePath || filePath == "") return null;

            var source = new BitmapImage();
            
            ZipArchiveEntry entory = GetEntry(filePath);

            try
            {
                using (var zipStream = entory.Open())
                using (var ms_bitmap = new MemoryStream())
                {
                    zipStream.CopyTo(ms_bitmap);
                    ms_bitmap.Position = 0;

                    source.BeginInit();
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.CreateOptions = BitmapCreateOptions.None;
                    if( bitmapDecodePixelWidth != 0)
                        source.DecodePixelWidth = bitmapDecodePixelWidth;
                    source.StreamSource = ms_bitmap;
                    if(applyRotateInfoFromExif)
                        source.Rotation = imageFileInfo.ExifInfo.Rotation;
                    source.EndInit();
                    source.Freeze();

                    Debug.WriteLine("bitmap from zip: " + filePath);

                    // Exifに反転もあった場合は、BitmapImage.Rotationで対応出来ないのでTransform
                    if( applyRotateInfoFromExif && imageFileInfo.ExifInfo.ScaleTransform != null )
                    {
                        return TransformBitmap(source, imageFileInfo.ExifInfo.ScaleTransform);
                    }

                    return source;
                }
            }
            catch { return null; }
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
