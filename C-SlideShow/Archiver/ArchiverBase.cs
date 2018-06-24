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

                // Exif情報取得
                ExifInfo ei;
                if( applyRotateInfoFromExif )
                {
                    ei = GetExifInfo(fs);
                }
                else ei = new ExifInfo();

                // 画像解像度を取得


                // ストリーム開放
                fs.Close();

                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.CreateOptions = BitmapCreateOptions.None;
                if( bitmapDecodePixelWidth != 0)
                    source.DecodePixelWidth = bitmapDecodePixelWidth;
                source.UriSource = new Uri(filePath);
                source.Rotation = ei.Rotation;
                source.EndInit();
                source.Freeze();

                Debug.WriteLine("bitmap from file: " + filePath);

                // Exifに反転もあった場合は、BitmapImage.Rotationで対応出来ないのでTransform
                if( ei.ScaleTransform != null )
                {
                    return TransformBitmap(source, ei.ScaleTransform);
                }

                return source;
            }
            catch
            {
                return null;
            }
        }


        protected ExifInfo GetExifInfo(Stream st)
        {
            // Exif(メタデータ)取得
            BitmapFrame bmf = BitmapFrame.Create(st);
            var metaData = (bmf.Metadata) as BitmapMetadata;
            //bmf.Freeze();
            st.Position = 0;

            //Debug.WriteLine("Metadata: " + source.Metadata);

            string query = "/app1/ifd/exif:{uint=274}";
            if (!metaData.ContainsQuery(query)) {
                return new ExifInfo();
            }

            switch (Convert.ToUInt32(metaData.GetQuery(query))) {
                case 1:
                    // 回転・反転なし
                    return new ExifInfo();
                case 3:
                    // 180度回転
                    return new ExifInfo(Rotation.Rotate180, null);
                case 6:
                    // 時計回りに90度回転
                    return new ExifInfo(Rotation.Rotate90, null);
                case 8:
                    // 時計回りに270度回転
                    return new ExifInfo(Rotation.Rotate270, null);
                case 2:
                    // 水平方向に反転
                    return new ExifInfo(Rotation.Rotate0, new ScaleTransform(-1, 1, 0, 0));
                case 4:
                    // 垂直方向に反転
                    return new ExifInfo(Rotation.Rotate0, new ScaleTransform(1, -1, 0, 0));
                case 5:
                    // 時計回りに90度回転 + 水平方向に反転
                    return new ExifInfo(Rotation.Rotate90, new ScaleTransform(-1, 1, 0, 0));
                case 7:
                    // 時計回りに270度回転 + 水平方向に反転
                    return new ExifInfo(Rotation.Rotate270, new ScaleTransform(-1, 1, 0, 0));
            }
            return new ExifInfo();
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
