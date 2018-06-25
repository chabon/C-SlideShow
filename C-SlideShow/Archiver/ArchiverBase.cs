using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Windows.Media;


namespace C_SlideShow.Archiver
{
    public class ArchiverBase
    {
        public string ArchiverPath { get; set; }

        public ArchiverBase()
        {
        }

        public ArchiverBase(string archiverPath)
        {
            ArchiverPath = archiverPath;
        }

        public void GetStream(string path)
        {
        }

        public virtual BitmapSource LoadBitmap(
            ImageFileInfo imageFileInfo, int bitmapDecodePixelWidth, bool applyRotateInfoFromExif)
        {
            string filePath = imageFileInfo.FilePath;
            if (filePath == ImageFileManager.DummyFilePath || filePath == "") return null;

            var source = new BitmapImage();

            try
            {
                // ストリーム
                FileStream fs = File.OpenRead(filePath);

                // ストリーム開放
                fs.Close();

                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.CreateOptions = BitmapCreateOptions.None;
                if( bitmapDecodePixelWidth != 0)
                    source.DecodePixelWidth = bitmapDecodePixelWidth;
                source.UriSource = new Uri(filePath);
                if(applyRotateInfoFromExif)
                    source.Rotation = imageFileInfo.ExifInfo.Rotation;
                source.EndInit();
                source.Freeze();

                Debug.WriteLine("bitmap from file: " + filePath);

                // Exifに反転もあった場合は、BitmapImage.Rotationで対応出来ないのでTransform
                if( applyRotateInfoFromExif && imageFileInfo.ExifInfo.ScaleTransform != null )
                {
                    return TransformBitmap(source, imageFileInfo.ExifInfo.ScaleTransform);
                }

                return source;
            }
            catch
            {
                return null;
            }
        }



        protected BitmapSource TransformBitmap(BitmapSource source, Transform transform) {
            var result = new TransformedBitmap();
            result.BeginInit();
            result.Source = source;
            result.Transform = transform;
            result.EndInit();
            result.Freeze();
            return result;
        }

        public virtual void DisposeArchive()
        {

        }
    }
}
