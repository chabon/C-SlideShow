using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO;

using System.Windows;

using C_SlideShow.Archiver;
using C_SlideShow.CommonControl;

namespace C_SlideShow.Core
{
    public class ImageFileContext
    {
        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        public string        FilePath       { get; set; }
        public bool          IsDummy        { get; set; }
        public BitmapImage   BitmapImage    { get; set; }
        public ImageFileInfo Info           { get; set; } = new ImageFileInfo();
        public ArchiverBase  Archiver       { get; set; } 
        public int           RefCount       { get; set; } = 0;  // 参照カウンタ(コンテナからの)
        public string        TempFilePath   { get; set; }       // 一時展開ファイルフルパス
        public const string  TempDirName = "temp";              // 一時展開フォルダ名

        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */
        public ImageFileContext(string path)
        {
            this.FilePath = path;
        }


        public Stream GetStream()
        {
            return File.OpenRead(this.FilePath);
        }


        // @ref https://chitoku.jp/programming/wpf-lazy-image-behavior
        public Task<BitmapSource> GetImage(Size bitmapDecodePixel)
        {
            return Task.Run(() =>
            {
                if( IsDummy || FilePath == null || FilePath == "" ) return null;

                using( Stream st = Archiver.OpenStream(FilePath) )
                {
                    try
                    {
                        // 表示に必要な情報取得
                        ReadInfoForView(st);

                        // BitmapImage(BitmapSource)用意 
                        var source = new BitmapImage();
                        source.BeginInit();
                        source.CacheOption = BitmapCacheOption.OnLoad;
                        source.CreateOptions = BitmapCreateOptions.None;

                        // 画像のアス比から、縦横どちらのDecodePixelを適用するのかを決める
                        // DecodePixelの値が、画像のサイズをオーバーする場合は、画像サイズをDecodePixelの値として指定する
                        if( bitmapDecodePixel != Size.Empty )
                        {
                            if(Info.PixelSize.Height > (double)Info.PixelSize.Width) // 画像が縦長
                            {
                                if(bitmapDecodePixel.Height > Info.PixelSize.Height)
                                    source.DecodePixelHeight = (int)Info.PixelSize.Height;
                                else
                                    source.DecodePixelHeight = (int)bitmapDecodePixel.Height;
                            }
                            else // 画像が横長
                            {
                                if(bitmapDecodePixel.Width > Info.PixelSize.Width)
                                    source.DecodePixelWidth = (int)Info.PixelSize.Width;
                                else
                                    source.DecodePixelWidth = (int)bitmapDecodePixel.Width;
                            }
                        }

                        // 読み込み
                        source.StreamSource = st;

                        // 回転
                        if( MainWindow.Current.Setting.TempProfile.ApplyRotateInfoFromExif.Value )
                            source.Rotation = Info.ExifInfo.Rotation;

                        // 読み込み終了処理
                        source.EndInit();
                        source.Freeze();

                        Debug.WriteLine("bitmap load from stream: " + source.PixelWidth + "x" + source.PixelHeight + "  path: " + FilePath + " refCnt: " + RefCount);

                        // Exifに反転もあった場合は、BitmapImage.Rotationで対応出来ないのでTransform
                        if(  MainWindow.Current.Setting.TempProfile.ApplyRotateInfoFromExif.Value  && Info.ExifInfo.ScaleTransform != null )
                        {
                            return TransformBitmap(source, Info.ExifInfo.ScaleTransform);
                        }

                        return source;
                    }
                    catch
                    {
                        return null;
                    }
                }
            });
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


        /// <summary>
        /// スライド表示時に必要な、画像情報を取得(画像サイズとExif情報)
        /// </summary>
        public void ReadInfoForView(Stream st)
        {
            // すでに取得済み
            if( Info.PixelSize != Size.Empty || Info.ExifInfo != ExifInfo.Empty ) return;

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
                Info.PixelSize = new Size(bmf.PixelWidth, bmf.PixelHeight);
            }
            catch
            {
                Info.PixelSize = new Size();
                Debug.WriteLine("ReadImagePixelSize() is failed");
            }

            // Exif情報取得
            st.Position = 0;
            try
            {
                Info.ExifInfo = ReadExifInfoFromBitmapMetadata(metaData);
            }
            catch
            {
                Info.ExifInfo = new ExifInfo();
                Debug.WriteLine("GetExifInfo() is failed");
            }
        }


        public void ReadInfoForView()
        {
            using( Stream st = Archiver.OpenStream(FilePath) )
            {
                try
                {
                    ReadInfoForView(st);
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


        public void ReadLastWriteTime()
        {
            if( IsDummy ) return;

            // 取得済み
            if( Info.LastWriteTime != null ) return;

            // FolderArchiver, NullArchiver
            if(Archiver is FolderArchiver || Archiver is NullArchiver )
            {
                Info.LastWriteTime = File.GetLastWriteTime(FilePath);
            }
        }


        /* ---------------------------------------------------- */
        //     コマンド
        /* ---------------------------------------------------- */
        // エクスプローラーでタイルを開く
        public void OpenExplorer()
        {
            string dirPath;
            string filePath;
            if( Archiver.CanReadFile )
            {
                dirPath = Directory.GetParent(FilePath).FullName;
                filePath = FilePath;
            }
            else
            {
                dirPath = Directory.GetParent(Archiver.ArchiverPath).FullName;
                filePath = Archiver.ArchiverPath;
            }
            Process.Start("explorer.exe", "/select,\"" + filePath + "\"");
        }


        // ファイルをコピー
        public void CopyFile()
        {
            string notificationFileName;
            string srcFilePath;

            if( Archiver.CanReadFile )
            {
                srcFilePath = FilePath;
                notificationFileName = Path.GetFileName(FilePath);
            }
            else
            {
                // 書庫内ファイルの場合
                if(TempFilePath == null) WriteToTempFolder();

                notificationFileName = TempDirName + "\\" + Path.GetFileName(TempFilePath);
            }

            // コピー
            System.Collections.Specialized.StringCollection files = new System.Collections.Specialized.StringCollection();
            files.Add(TempFilePath);
            Clipboard.SetFileDropList(files);

            // 通知
            MainWindow.Current.NotificationBlock.Show("ファイルをコピー: " + notificationFileName,
                NotificationPriority.Normal, NotificationTime.Normal, NotificationType.None);
        }


        // 画像データをコピー
        public void CopyImageData()
        {
            //BitmapSource source = MainWindow.Current.ImageFileManager.LoadBitmap( ImageFileInfo, new Size(0, 0) );
            //Clipboard.SetImage(source);

            string fileName = System.IO.Path.GetFileName(FilePath);
            MainWindow.Current.NotificationBlock.Show("画像データをコピー: " + fileName,
                NotificationPriority.Normal, NotificationTime.Normal, NotificationType.None);
        }


        // ファイルパスをコピー
        public void CopyFilePath()
        {
            string filePath;
            if( Archiver.CanReadFile )
            {
                filePath = FilePath;
            }
            else
            {
                filePath = Archiver.ArchiverPath;
            }
            Clipboard.SetText(filePath);

            MainWindow.Current.NotificationBlock.Show("ファイルパスをコピー: " + filePath,
                NotificationPriority.Normal, NotificationTime.Normal, NotificationType.None);
        }


        // ファイル名をコピー
        public void CopyFileName()
        {
            string fileName = Path.GetFileName(FilePath);
            Clipboard.SetText(fileName);

            MainWindow.Current.NotificationBlock.Show("ファイル名をコピー: " + fileName,
                NotificationPriority.Normal, NotificationTime.Normal, NotificationType.None);
        }


        // 外部プログラムで画像を開く
        public void OpenByExternalApp(ExternalAppInfo exAppInfo)
        {
            if( exAppInfo == null ) return;

            // ファイルパスの決定
            string filePath = "";
            if( exAppInfo.Arg.Contains(Format.FilePathFormat) )
            {
                if( Archiver.CanReadFile )
                {
                    filePath = FilePath;
                }
                else
                {
                    // 書庫内ファイルなら一時展開
                    if( TempFilePath == null ) WriteToTempFolder();
                    filePath = TempFilePath;
                }
            }

            // フォルダ(書庫)パスの決定
            string folderPath;
            if(Archiver is Archiver.NullArchiver )
            {
                try { folderPath = Directory.GetParent(FilePath).FullName; }
                catch { folderPath = ""; }
            }
            else
            {
                folderPath = Archiver.ArchiverPath;
            }

            // 親フォルダパスの決定
            string parentFolderPath;
            try { parentFolderPath = Directory.GetParent(folderPath).FullName; }
            catch { parentFolderPath = ""; }


            // 外部プログラム呼び出し
            string arg = exAppInfo.Arg;
            arg = arg.Replace(Format.FilePathFormat, filePath);
            arg = arg.Replace(Format.FolderPathFormat, folderPath);
            arg = arg.Replace(Format.ParentFolderPathFormat, parentFolderPath);

            if(exAppInfo.Path != null && exAppInfo.Path != "" )
            {
                // プログラムの指定あり
                try { Process.Start( exAppInfo.Path, arg ); }
                catch { }
            }
            else
            {
                // プログラムの指定がなければ、拡張子で関連付けられているプログラムで開く(引数そのままStart()に)
                try { Process.Start( arg ); }
                catch { }
            }
        }


        /// <summary>
        /// アーカイブ中のファイルを一時フォルダに展開する
        /// </summary>
        public void WriteToTempFolder()
        {
            if( TempFilePath != null || Archiver == null) return;

            // 一時ファイル名
            string ext = System.IO.Path.GetExtension(FilePath);
            string tempFileName = System.IO.Path.GetRandomFileName();
            if(ext != null && ext != string.Empty )
            {
                tempFileName = System.IO.Path.ChangeExtension(tempFileName, ext);
            }

            // 一時ファイルのディレクトリパス(なければ作成)
            string tempDir = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName + "\\" + TempDirName;
            if( !Directory.Exists(tempDir) ) Directory.CreateDirectory(tempDir);

            // 一時ファイルフルパス
            TempFilePath = tempDir + "\\" + tempFileName;

            // 出力
            Archiver.WriteAsFile(FilePath, TempFilePath);

            // リストに追加(アプリケーション終了時に削除)
            if( App.TempFilePathList == null ) App.TempFilePathList = new List<string>();
            App.TempFilePathList.Add(TempFilePath);
        }


        /* ---------------------------------------------------- */
        //     Obsolete Method
        /* ---------------------------------------------------- */
        private Size ReadImagePixelSize(Stream st)
        {
            string ext = System.IO.Path.GetExtension(FilePath).ToLower();
            switch( ext )
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
