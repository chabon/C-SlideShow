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
using System.Windows;

using PdfiumViewer;

using C_SlideShow.Core;


namespace C_SlideShow.Archiver
{
    public class PdfArchiver : ArchiverBase
    {
        private PdfDocument pdfDoc;

        public PdfArchiver(string archiverPath) : base(archiverPath)
        {
            LeaveHistory = true;

            try
            {
                pdfDoc = PdfDocument.Load(archiverPath);
            }
            catch
            {
                DisposeArchive();
            }
        }

        private int PathToPageIndex(string path)
        {
            return int.Parse(path) - 1;

        }

        // レンダリング後、pngファイルとして保存したものをストリームに渡す、サイズはBitmapDecodePixelに準拠
        public override Stream OpenStream(string path)
        {
            try
            {
                // レンダリング
                var s = MainWindow.Current.Setting.TempProfile.BitmapDecodeTotalPixel.Value;
                var maxSize = new Size(s, s);
                var sizef = pdfDoc.PageSizes[ PathToPageIndex(path) ];
                Size size = new Size(sizef.Width, sizef.Height);
                size = size.StreachAsUniform(maxSize);
                var bitmap = pdfDoc.Render( PathToPageIndex(path), (int)Math.Round(size.Width), (int)Math.Round(size.Height), 96, 96, false );

                // ストリームへ
                var ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            catch
            {
                return Stream.Null;
            }
        }

        public override List<ImageFileContext> LoadImageFileContextList()
        {
            List<ImageFileContext> newList = new List<ImageFileContext>();

            try
            {
                var pdfInfo = pdfDoc.GetInformation();
                for (int i=0; i < pdfDoc.PageCount; i++)
                {
                    ImageFileContext ifc = new ImageFileContext( $"{i+1:000}" );
                    ImageFileInfo fi = new ImageFileInfo();
                    ifc.Info = fi;
                    ifc.Archiver = this;
                    fi.LastWriteTime = pdfInfo.ModificationDate;
                    fi.Length = 0;

                    newList.Add(ifc);
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
            if( pdfDoc != null ) pdfDoc.Dispose();
        }

        public override Task<BitmapSource> LoadBitmap(Size bitmapDecodePixel, ImageFileContext context)
        {
            return Task.Run(() =>
            {
                if( context.IsDummy || context.FilePath == null || context.FilePath == "" ) return null;

                // 表示に必要な情報取得
                ReadInfoForView(context);

                //Size size = context.Info.PixelSize.StreachAsUniform(bitmapDecodePixel).Round();


                // 画像のアス比から、縦横どちらのDecodePixelを適用するのかを決める
                // DecodePixelの値が、画像のサイズをオーバーする場合は、画像サイズをDecodePixelの値として指定する
                ImageFileInfo info = context.Info;
                if( bitmapDecodePixel == Size.Empty || bitmapDecodePixel.Width == 0 || bitmapDecodePixel.Height == 0 ) bitmapDecodePixel = new Size(info.PixelSize.Width, info.PixelSize.Height);
                Size decodePixel = new Size(0, 0);
                double ratio = info.PixelSize.Height / info.PixelSize.Width;
                if( bitmapDecodePixel != Size.Empty )
                {
                    if(context.Info.PixelSize.Height > (double)context.Info.PixelSize.Width) // 画像が縦長
                    {
                        if(bitmapDecodePixel.Height > context.Info.PixelSize.Height)
                            decodePixel.Height = context.Info.PixelSize.Height;
                        else
                            decodePixel.Height = bitmapDecodePixel.Height;

                        decodePixel.Width = decodePixel.Height / ratio;
                    }
                    else // 画像が横長
                    {
                        if(bitmapDecodePixel.Width > context.Info.PixelSize.Width)
                            decodePixel.Width = context.Info.PixelSize.Width;
                        else
                            decodePixel.Width = bitmapDecodePixel.Width;

                        decodePixel.Height = decodePixel.Width * ratio;
                    }
                }

                decodePixel = decodePixel.Round();

                // Bitmap読み込み(System.Drawing.Image)
                var bitmap = pdfDoc.Render( PathToPageIndex(context.FilePath), (int)decodePixel.Width, (int)decodePixel.Height, 96, 96, false );

                // BitmapSourceに変換
                using(MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);
                    BitmapImage source = new BitmapImage();
                    source.BeginInit();
                    source.StreamSource = ms;
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.CreateOptions = BitmapCreateOptions.None;
                    source.EndInit();
                    source.Freeze();
                    Debug.WriteLine("bitmap load from pdf archiver: " + source.PixelWidth + "x" + source.PixelHeight + "  path: " + context.FilePath + " refCnt: " + context.RefCount);
                    return (BitmapSource)source;
                }
            });

        }

        public override void ReadInfoForView(ImageFileContext context)
        {
            // すでに取得済み
            if( context.Info.PixelSize != Size.Empty ) return;

            // サイズ
            var size = pdfDoc.PageSizes[ PathToPageIndex(context.FilePath) ];
            var side = MainWindow.Current.Setting.TempProfile.BitmapDecodeTotalPixel.Value;
            var maxSize = new Size(side, side);
            context.Info.PixelSize = new Size(size.Width, size.Height).StreachAsUniform(maxSize);
        }
    }
}
