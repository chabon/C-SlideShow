using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;

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

        // @ref https://chitoku.jp/programming/wpf-lazy-image-behavior
        public virtual Task<BitmapSource> LoadBitmap(Size bitmapDecodePixel, ImageFileContext context)
        {
            return Task.Run(() =>
            {
                if( context.IsDummy || context.FilePath == null || context.FilePath == "" ) return null;

                using( Stream st = OpenStream(context.FilePath) )
                {
                    try
                    {
                        // 表示に必要な情報取得
                        ReadInfoForView(st, context);

                        // BitmapImage(BitmapSource)用意 
                        var source = new BitmapImage();
                        source.BeginInit();
                        source.CacheOption = BitmapCacheOption.OnLoad;
                        source.CreateOptions = BitmapCreateOptions.None;

                        // 画像のアス比から、縦横どちらのDecodePixelを適用するのかを決める
                        // DecodePixelの値が、画像のサイズをオーバーする場合は、画像サイズをDecodePixelの値として指定する
                        if( bitmapDecodePixel != Size.Empty )
                        {
                            if(context.Info.PixelSize.Height > (double)context.Info.PixelSize.Width) // 画像が縦長
                            {
                                if(bitmapDecodePixel.Height > context.Info.PixelSize.Height)
                                    source.DecodePixelHeight = (int)context.Info.PixelSize.Height;
                                else
                                    source.DecodePixelHeight = (int)bitmapDecodePixel.Height;
                            }
                            else // 画像が横長
                            {
                                if(bitmapDecodePixel.Width > context.Info.PixelSize.Width)
                                    source.DecodePixelWidth = (int)context.Info.PixelSize.Width;
                                else
                                    source.DecodePixelWidth = (int)bitmapDecodePixel.Width;
                            }
                        }

                        // 読み込み
                        source.StreamSource = st;

                        // 回転
                        if( MainWindow.Current.Setting.TempProfile.ApplyRotateInfoFromExif.Value )
                            source.Rotation = context.Info.ExifInfo.Rotation;

                        // 読み込み終了処理
                        source.EndInit();
                        source.Freeze();

                        // Exifに反転もあった場合は、BitmapImage.Rotationで対応出来ないのでTransform
                        if( source != null && MainWindow.Current.Setting.TempProfile.ApplyRotateInfoFromExif.Value  && context.Info.ExifInfo.ScaleTransform != null )
                        {
                            return TransformBitmap(source, context.Info.ExifInfo.ScaleTransform);
                        }

                        Debug.WriteLine("bitmap load from archiver: " + source.PixelWidth + "x" + source.PixelHeight + "  path: " + context.FilePath + " refCnt: " + context.RefCount);

                        return (BitmapSource)source;
                    }
                    catch
                    {
                        return null;
                    }
                }
            });

        }


        /// <summary>
        /// スライド表示時に必要な、画像情報を取得(画像サイズとExif情報)
        /// </summary>
        protected virtual void ReadInfoForView(Stream st, ImageFileContext context)
        {
            // すでに取得済み
            if( context.Info.PixelSize != Size.Empty || context.Info.ExifInfo != ExifInfo.Empty ) return;

            // ストリーム取得エラー
            if( st == Stream.Null ) return;

            // メタデータ(Exif含む)取得
            BitmapFrame bmf = BitmapFrame.Create(st);
            var metaData = (bmf.Metadata) as BitmapMetadata;
            //bmf.Freeze();

            // ピクセルサイズ取得
            st.Position = 0;
            try
            {
                context.Info.PixelSize = new Size(bmf.PixelWidth, bmf.PixelHeight);
            }
            catch
            {
                context.Info.PixelSize = new Size();
                Debug.WriteLine("ReadImagePixelSize() is failed");
            }

            // Exif情報取得
            st.Position = 0;
            try
            {
                context.Info.ExifInfo = ReadExifInfoFromBitmapMetadata(metaData);
            }
            catch
            {
                context.Info.ExifInfo = new ExifInfo();
                Debug.WriteLine("GetExifInfo() is failed");
            }
        }


        public virtual void ReadInfoForView(ImageFileContext context)
        {
            using( Stream st = OpenStream(context.FilePath) )
            {
                try
                {
                    ReadInfoForView(st, context);
                }
                catch { }
            }

        }


        private ExifInfo ReadExifInfoFromBitmapMetadata(BitmapMetadata metaData)
        {
            // Exif情報作成
            ExifInfo exifInfo = new ExifInfo();

            // 撮影日時(原画像データの生成日時)
            try { exifInfo.DateTaken = DateTime.Parse(metaData.DateTaken); }
            catch { return exifInfo; }

            // カメラメーカー
            try { exifInfo.CameraMaker = metaData.CameraManufacturer;
            }catch { }

            // カメラ
            try { exifInfo.CameraModel = metaData.CameraModel; }
            catch { }

            // ソフトウェア
            try { exifInfo.Software = metaData.ApplicationName; }
            catch { }

            // 回転情報
            string query_orientation = "/app1/ifd/exif:{uint=274}";
            if ( metaData.ContainsQuery(query_orientation) )
            {
                switch (  Convert.ToUInt32( metaData.GetQuery(query_orientation) )  ) {
                    case 1:
                        // 回転・反転なし
                        break;
                    case 3:
                        // 180度回転
                        exifInfo.Rotation = Rotation.Rotate180;
                        break;
                    case 6:
                        // 時計回りに90度回転
                        exifInfo.Rotation = Rotation.Rotate90;
                        break;
                    case 8:
                        // 時計回りに270度回転
                        exifInfo.Rotation = Rotation.Rotate270;
                        break;
                    case 2:
                        // 水平方向に反転
                        exifInfo.ScaleTransform = new ScaleTransform(-1, 1, 0, 0);
                        break;
                    case 4:
                        // 垂直方向に反転
                        exifInfo.ScaleTransform = new ScaleTransform(1, -1, 0, 0);
                        break;
                    case 5:
                        // 時計回りに90度回転 + 水平方向に反転
                        exifInfo.Rotation = Rotation.Rotate90;
                        exifInfo.ScaleTransform = new ScaleTransform(-1, 1, 0, 0);
                        break;
                    case 7:
                        // 時計回りに270度回転 + 水平方向に反転
                        exifInfo.Rotation = Rotation.Rotate270;
                        exifInfo.ScaleTransform = new ScaleTransform(-1, 1, 0, 0);
                        break;
                }
            }

            return exifInfo;
        }


        private BitmapSource TransformBitmap(BitmapSource source, Transform transform)
        {
            var result = new TransformedBitmap();
            result.BeginInit();
            result.Source = source;
            result.Transform = transform;
            result.EndInit();
            result.Freeze();
            return result;
        }


        /* ---------------------------------------------------- */
        //     Obsolete Method
        /* ---------------------------------------------------- */
        private Size ReadImagePixelSize(Stream st, string ext)
        {
            switch( ext.ToLower() )
            {
                case ".jpg":
                case ".jpeg":
                    return ReadJpegPixelSize(st);
                case ".png":
                    return ReadPngPixelSize(st);
                case ".bmp":
                    return ReadBmpPixelSize(st);
                case ".gif":
                    return ReadGifPixelSize(st);
                default:
                    return new Size();
            }
        }

        // 参考：
        // http://d.hatena.ne.jp/n7shi/20110204/1296891184
        // http://blog.mirakui.com/entry/2012/09/17/121109
        // https://hp.vector.co.jp/authors/VA032610/JPEGFormat/markers.htm
        private Size ReadJpegPixelSize(Stream st)
        {
            const int SOI  = 0xd8;  // スタートマーカー
            const int SOF0 = 0xc0;  // ベースラインフレームヘッダー (ハフマン符号化基本DCT方式)
            const int SOF1 = 0xc1;  // フレームヘッダー (ハフマン符号化拡張シーケンシャルDCT方式)
            const int SOF2 = 0xc2;  // プログレッシブフレームヘッダー (ハフマン符号化プログレッシブDCT方式)
            const int SOF3 = 0xc3;  // フレームヘッダー (ハフマン符号化ロスレス方式)

            var buf = new byte[8];
            while( st.Read(buf, 0, 2) == 2 && buf[0] == 0xff ) // 2byte = 16進数で4桁
            {
                switch( buf[1] )
                {
                    case SOF0:
                    case SOF1:
                    case SOF2:
                    case SOF3:
                        if( st.Read(buf, 0, 7) == 7 ) // 7byte = 16進数で14桁
                            return new Size(buf[5] * 256 + buf[6], buf[3] * 256 + buf[4]);
                        break;
                    default:
                        if( buf[1] != SOI ){
                            if(st.Read(buf, 0, 2) == 2)
                                st.Position += buf[0] * 256 + buf[1] - 2; // 1セグメント進めて、フレームヘッダセグメントを目指す
                            else
                                return new Size();
                        }
                        break;
                }
            }
            return new Size();
        }

        // 参考: https://dixq.net/forum/viewtopic.php?t=17600
        private Size ReadPngPixelSize(Stream st)
        {
            st.Seek(16, SeekOrigin.Begin);
            byte[] buf = new byte[8];
            st.Read(buf, 0, 8);

            int width, height;
            if (BitConverter.IsLittleEndian)   // pngのヘッダのバイトオーダーはビッグエンディアン
            {
                Array.Reverse(buf);
                width = BitConverter.ToInt32(buf, 4);
                height = BitConverter.ToInt32(buf, 0);
            }
            else
            {
                width = BitConverter.ToInt32(buf, 0);
                height = BitConverter.ToInt32(buf, 4);
            }

            return new Size(width, height);
        }


        // http://www.umekkii.jp/data/computer/file_format/bitmap.cgi
        private Size ReadBmpPixelSize(Stream st)
        {
            st.Seek(18, SeekOrigin.Begin);
            byte[] buf = new byte[8];
            st.Read(buf, 0, 8);

            int width, height;
            if (BitConverter.IsLittleEndian) // bmpのヘッダのバイトオーダーはリトルエンディアン
            {
                width = BitConverter.ToInt32(buf, 0);
                height = BitConverter.ToInt32(buf, 4);
            }
            else
            {
                Array.Reverse(buf);
                width = BitConverter.ToInt32(buf, 4);
                height = BitConverter.ToInt32(buf, 0);
            }

            return new Size(width, height);
        }


        // https://www.setsuki.com/hsp/ext/gif.htm
        private Size ReadGifPixelSize(Stream st)
        {
            st.Seek(6, SeekOrigin.Begin);
            byte[] buf = new byte[4];
            st.Read(buf, 0, 4);

            int width, height;
            if (BitConverter.IsLittleEndian) // gifのヘッダのバイトオーダーはリトルエンディアン
            {
                width = BitConverter.ToInt16(buf, 0);
                height = BitConverter.ToInt16(buf, 2);
            }
            else
            {
                Array.Reverse(buf);
                width = BitConverter.ToInt16(buf, 2);
                height = BitConverter.ToInt16(buf, 0);
            }

            return new Size(width, height);
        }

    }
}
