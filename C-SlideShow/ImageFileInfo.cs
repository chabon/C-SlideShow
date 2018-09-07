using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;

using C_SlideShow.Archiver;

namespace C_SlideShow
{
    public class ImageFileInfo
    {
        public string           FilePath;                  // ファイルパス
        public ArchiverBase     Archiver;                  // 対応するアーカイバ
        public DateTimeOffset?  LastWriteTime = null;      // 更新日時
        public DateTimeOffset?  CreationTime = null;       // 作成日時
        public long             Length = 0;                // ファイルサイズ(byte)
        public Size             PixelSize = Size.Empty;    // ピクセルサイズ
        public ExifInfo         ExifInfo = ExifInfo.Empty; // Exif情報
        public bool             IsDummy = false;           // 穴埋め用のダミー
        public string           TempFilePath = null;       // 一時展開ファイルフルパス
        public const string     TempDirName = "temp";      // 一時展開フォルダ名

        public ImageFileInfo()
        {

        }

        public ImageFileInfo(string _filePath)
        {
            this.FilePath = _filePath;
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

        /// <summary>
        /// スライド表示時に必要な、画像情報を取得(画像サイズとExif情報)
        /// </summary>
        public void ReadSlideViewInfo()
        {
            // すでに取得済み
            if( PixelSize != Size.Empty || ExifInfo != ExifInfo.Empty ) return;

            // ダミーの場合取得しない
            if( IsDummy ) return;

            using( var st = Archiver.OpenStream(FilePath) )
            {
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
                    PixelSize = new Size(bmf.PixelWidth, bmf.PixelHeight);
                }
                catch
                {
                    PixelSize = new Size();
                    Debug.WriteLine("ReadImagePixelSize() is failed");
                }

                // Exif情報取得
                st.Position = 0;
                try
                {
                    ExifInfo = ReadExifInfoFromBitmapMetadata(metaData);
                }
                catch
                {
                    ExifInfo = new ExifInfo();
                    Debug.WriteLine("GetExifInfo() is failed");
                }
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

        private Size ReadImagePixelSize(Stream st)
        {
            string ext = Path.GetExtension(this.FilePath).ToLower();
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


        public void ReadLastWriteTime()
        {
            // ダミー
            if( IsDummy ) return;

            // 取得済み
            if( LastWriteTime != null ) return;

            // zipの場合、エントリー取得時に更新日時も取得出来ているので必要なし
            if( Archiver is ZipArchiver ) return;

            // FolderArchiver, NullArchiver
            this.LastWriteTime = File.GetLastWriteTime(FilePath);
        }

    }
}
