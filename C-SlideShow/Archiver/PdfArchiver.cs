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

        public override Task<BitmapSource> LoadBitmap(Size bitmapDecodePixelMax, ImageFileContext context)
        {
            return Task.Run(() =>
            {
                if( context.IsDummy || context.FilePath == null || context.FilePath == "" ) return null;

                // 表示に必要な情報取得
                ReadInfoForView(context);

                // BitmapDecodePixel値の決定
                Size bitmapDecodePixel;
                if( bitmapDecodePixelMax != Size.Empty)
                {
                    bitmapDecodePixel = context.Info.PixelSize.StreachAsUniform(bitmapDecodePixelMax).Round();
                    if( bitmapDecodePixel.Width > context.Info.PixelSize.Width ) {   // 画像のサイズをオーバーする場合は、画像サイズをDecodePixelの値として指定する
                        bitmapDecodePixel = context.Info.PixelSize;
                    }
                }
                else
                {
                    bitmapDecodePixel = context.Info.PixelSize;
                }

                // Bitmap読み込み(System.Drawing.Image)
                var bitmap = pdfDoc.Render( PathToPageIndex(context.FilePath), (int)bitmapDecodePixel.Width, (int)bitmapDecodePixel.Height, 96, 96, false );

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
