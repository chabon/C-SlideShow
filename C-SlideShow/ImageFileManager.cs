using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.ObjectModel;

using C_SlideShow.Archiver;



namespace C_SlideShow
{
    public class ImageFileManager
    {
        // field
        int numofDummyFileInfo; // コンテナ内の隙間グリッド埋め用のダミー情報の数

        // property
        #region Properties
        public List<ImageFileInfo> ImgFileInfo { get; set; }
        public List<ArchiverBase>  Archivers { get; set; }
        public NullArchiver        NullArchiver { get; set; }
        public static string       DummyFilePath = ":dummy";
        public bool                ApplyRotateInfoFromExif { get; set; } = false;
        public double              TileAspectRatio { get; set; } = 0.75;
        public int                 NextIndex { get; set; }
        public int                 PrevIndex { get; set; }

        public int NumofImageFile
        {
            get
            {
                return ImgFileInfo.Count - numofDummyFileInfo;
            }
        }

        public int ActualCurrentIndex
        {
            get
            {
                // ダミーであろうと関係なく返す
                int idx = PrevIndex + 1;
                if (idx > this.ImgFileInfo.Count - 1) idx = 0;
                return idx;
            }
        }

        public int CurrentIndex
        {
            get
            {
                // ダミーだったら対象外(0を返す)
                int idx = PrevIndex + 1;
                if (idx > this.ImgFileInfo.Count - 1 - numofDummyFileInfo) idx = 0;
                return idx;
            }
        }

        public bool IsCurrentIndexDummy
        {
            get
            {
                int idx = PrevIndex + 1;
                if (idx < this.ImgFileInfo.Count - 1 - numofDummyFileInfo) return false;
                else if (idx == this.ImgFileInfo.Count) return false;
                else return true;
            }
        }
        #endregion


        // constractor
        public ImageFileManager()
        {
            ImgFileInfo = new List<ImageFileInfo>();
            Archivers = new List<ArchiverBase>();
            NullArchiver = new NullArchiver();
            NextIndex = 0;
            PrevIndex = 0;
        }


        /* ---------------------------------------------------- */
        //     method
        /* ---------------------------------------------------- */
        public void InitPrevIndex(int firstIndex)
        {
            if (ImgFileInfo.Count < 1) return;
            PrevIndex = firstIndex - 1;
            if (PrevIndex < 0) PrevIndex = ImgFileInfo.Count - 1;
        }

        public ImageFileInfo PickImageFileInfo(bool isPlayback)
        {
            if (ImgFileInfo.Count > 0)
            {
                if (isPlayback) return ImgFileInfo[PrevIndex];
                else return ImgFileInfo[NextIndex];
            }
            else return null;
        }


        public int GetLastNoDeviationIndex(int grids)
        {
            int idx = ImgFileInfo.Count - grids;
            if (idx < 0) idx = 0;
            return idx;
        }

        public string CreateCurrentIndexInfoString()
        {
            int idx = CurrentIndex;
            int num = idx + 1;
            int numMax = ImgFileInfo.Count - numofDummyFileInfo;
            if (numMax < 1) { num = 0; numMax = 0; }
            return String.Format("{0} / {1}", num, numMax);
        }

        public void LoadImageFileInfo(string path)
        {
            // 新規アーカイバ
            ArchiverBase archiver;

            // ファイル
            if( File.Exists(path) )
            {
                // 画像ファイル単体
                ImageFileInfo ifi = NullArchiver.LoadImageFileInfo(path);
                if( ifi != null ) ImgFileInfo.Add(ifi);

                // 圧縮ファイル / その他のファイル
                else
                {
                    string ext = Path.GetExtension(path);

                    switch( ext )
                    {
                        case ".zip":
                            Archivers.Add( archiver = new ZipArchiver(path) );
                            ImgFileInfo.AddRange( archiver.LoadImageFileInfoList() );
                            break;
                        default:
                            return;
                    }
                }
            }

            // フォルダ
            else if( Directory.Exists(path) )
            {
                Archivers.Add( archiver = new FolderArchiver(path) );
                ImgFileInfo.AddRange( archiver.LoadImageFileInfoList() );
            }
        }


        /// <summary>
        /// WPFで利用可能なBitmapをロード
        /// </summary>
        /// <param name="imageFileInfo"></param>
        /// <param name="bitmapDecodePixelWidth"></param>
        /// <returns>BitmapSource(失敗時はnullを返す)</returns>
        public BitmapSource LoadBitmap(ImageFileInfo imageFileInfo, int bitmapDecodePixelWidth)
        {
            string path = imageFileInfo.FilePath;
            if( path == DummyFilePath || path == "" ) return null;

            var source = new BitmapImage();

            using(Stream st = imageFileInfo.Archiver.OpenStream(path) )
            {
                try
                {
                    source.BeginInit();
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.CreateOptions = BitmapCreateOptions.None;

                    // 画像とタイルのアス比から、縦横どちらをDecodePixelの基準とするのかを決める
                    bool bSwap = false;
                    double imgRatio = imageFileInfo.PixelSize.Height / imageFileInfo.PixelSize.Width;
                    if( imgRatio > TileAspectRatio ) bSwap = true;

                    if( bitmapDecodePixelWidth != 0 )
                    {
                        if( bSwap )
                            source.DecodePixelHeight = (int)(bitmapDecodePixelWidth * TileAspectRatio);
                        else
                            source.DecodePixelWidth = bitmapDecodePixelWidth;
                    }

                    // 読み込み
                    //source.UriSource = new Uri(filePath);
                    source.StreamSource = st;

                    // 回転
                    if( ApplyRotateInfoFromExif )
                        source.Rotation = imageFileInfo.ExifInfo.Rotation;

                    source.EndInit();
                    source.Freeze();

                    Debug.WriteLine("bitmap load from stream: "
                        + source.PixelWidth + "x" + source.PixelHeight + "  path: " + path);

                    // Exifに反転もあった場合は、BitmapImage.Rotationで対応出来ないのでTransform
                    if( ApplyRotateInfoFromExif && imageFileInfo.ExifInfo.ScaleTransform != null )
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

        public void GetLastWriteTimeFileInfo()
        {
            if (ImgFileInfo.Count < 1) return;

            DateTimeOffset dateDefault = new DateTimeOffset();
            if(ImgFileInfo[0].LastWriteTime.CompareTo(dateDefault) == 0)
            {
                foreach(ImageFileInfo fi in ImgFileInfo)
                {
                    if(fi.FilePath != DummyFilePath)
                    {
                        fi.LastWriteTime = File.GetLastWriteTime(fi.FilePath);
                    }
                }
            }

        }


        public void Sort(FileReadingOrder order)
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            if (ImgFileInfo.Count < 1) return;

            switch (order)
            {
                case FileReadingOrder.FileName:
                    ImgFileInfo = ImgFileInfo.OrderBy(f => f.FilePath).ToList();
                    break;
                case FileReadingOrder.FileNameRev:
                    ImgFileInfo = ImgFileInfo.OrderByDescending(f => f.FilePath).ToList();
                    break;
                case FileReadingOrder.LastWriteTime:
                    GetLastWriteTimeFileInfo();
                    ImgFileInfo = ImgFileInfo.OrderByDescending(f => f.LastWriteTime).ToList();
                    break;
                case FileReadingOrder.LastWriteTimeRev:
                    GetLastWriteTimeFileInfo();
                    ImgFileInfo = ImgFileInfo.OrderBy(f => f.LastWriteTime).ToList();
                    break;
                case FileReadingOrder.Random:
                    ImgFileInfo.Shuffle();
                    break;
                case FileReadingOrder.None:
                    break;
            }


#if DEBUG
            sw.Stop();
            Debug.WriteLine("-----------------------------------------------------");
            Debug.WriteLine(" sort end    order: " + order + "  time: " + sw.Elapsed);
            Debug.WriteLine("-----------------------------------------------------");
#endif
        }


        /// <summary>
        /// ファイル総数が、グリッドの整数倍でないことによる空きを、ダミーファイル情報で埋める
        /// 埋めることにより、全体をスクロールし終わった時にズレがなくなる
        /// </summary>
        /// <param name="grids"></param>
        public void FillFileInfoVacancyWithDummy(int grids)
        {
            if (ImgFileInfo.Count < 1) return;

            // 初期化
            numofDummyFileInfo = 0;
            ImgFileInfo.RemoveAll(fi => fi.FilePath == DummyFilePath);

            while(ImgFileInfo.Count % grids != 0)
            {
                ImageFileInfo imgFileInfo = new ImageFileInfo(DummyFilePath);
                imgFileInfo.Archiver = this.NullArchiver;
                ImgFileInfo.Add(imgFileInfo);
                numofDummyFileInfo++;
            }
        }

        public void SlideIndex(bool isPlayback)
        {
            if (ImgFileInfo.Count > 0)
            {
                if (isPlayback)
                {
                    DecrementNextIndex();
                    DecrementPrevIndex();
                }
                else
                {
                    IncrementNextIndex();
                    IncrementPrevIndex();
                }
            }
        }

        public void IncrementNextIndex()
        {
            NextIndex++;
            if (NextIndex > ImgFileInfo.Count - 1) NextIndex = 0;
        }

        public void DecrementNextIndex()
        {
            NextIndex--;
            if (NextIndex < 0) NextIndex = ImgFileInfo.Count - 1;
        }

        public void IncrementPrevIndex()
        {
            PrevIndex++;
            if (PrevIndex > ImgFileInfo.Count - 1) PrevIndex = 0;
        }

        public void DecrementPrevIndex()
        {
            PrevIndex--;
            if (PrevIndex < 0) PrevIndex = ImgFileInfo.Count - 1;
        }




    }


}
