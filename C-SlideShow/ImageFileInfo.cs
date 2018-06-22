using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.IO;

namespace C_SlideShow
{
    public class ImageFileInfo
    {
        public string FilePath;                 // ファイルパス
        public DateTimeOffset LastWriteTime;    // 更新日時
        public DateTimeOffset CreationTime;     // 作成日時
        public DateTimeOffset ShootingTime;     // 撮影日時
        public long Length = 0;                 // ファイルサイズ(byte)
        public Size PixelSize = Size.Empty;     // ピクセルサイズ

        public ImageFileInfo()
        {

        }

        public ImageFileInfo(string _filePath)
        {
            this.FilePath = _filePath;
        }


        /// <summary>
        /// スライド表示時に必要な、画像情報を取得(画像サイズと回転情報(予定))
        /// </summary>
        public void ReadDetailInfo()
        {
            if(File.Exists(FilePath))
            {
                using( var fs = new FileStream(FilePath, FileMode.Open) )
                {
                    // ピクセルサイズ取得
#if DEBUG
                    if(PixelSize == Size.Empty) PixelSize = ReadImagePixelSize(fs);
#else
                    try
                    {
                        if(PixelSize == Size.Empty) PixelSize = ReadImagePixelSize(fs);
                    }
                    catch 
                    {
                        PixelSize = new Size();
                    }
#endif
                }
            }
        }

        public Size ReadImagePixelSize(FileStream fs)
        {
            string ext = Path.GetExtension(fs.Name).ToLower();
            switch( ext )
            {
                case ".jpg":
                case ".jpeg":
                    return ReadJpegPixelSize(fs);
                case ".png":
                    return ReadPngPixelSize(fs);
                case ".bmp":
                    return ReadBmpPixelSize(fs);
                case ".gif":
                    return ReadGifPixelSize(fs);
                default:
                    return new Size();
            }
        }

        public Size ReadJpegPixelSize(FileStream fs)
        {
            var buf = new byte[8];
            while (fs.Read(buf, 0, 2) == 2 && buf[0] == 0xff)
            {
                if (buf[1] == 0xc0 && fs.Read(buf, 0, 7) == 7)
                    return new Size(buf[5] * 256 + buf[6], buf[3] * 256 + buf[4]);
                else if (buf[1] != 0xd8)
                {
                    if (fs.Read(buf, 0, 2) == 2)
                        fs.Position += buf[0] * 256 + buf[1] - 2;
                    else
                        break;
                }
            }
            return new Size();
        }

        public Size ReadPngPixelSize(FileStream fs)
        {
            fs.Seek(16, SeekOrigin.Begin);
            byte[] buf = new byte[8];
            fs.Read(buf, 0, 8);

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
        public Size ReadBmpPixelSize(FileStream fs)
        {
            fs.Seek(18, SeekOrigin.Begin);
            byte[] buf = new byte[8];
            fs.Read(buf, 0, 8);

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
        public Size ReadGifPixelSize(FileStream fs)
        {
            fs.Seek(6, SeekOrigin.Begin);
            byte[] buf = new byte[4];
            fs.Read(buf, 0, 4);

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
