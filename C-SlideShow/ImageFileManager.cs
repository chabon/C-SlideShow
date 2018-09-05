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

using System.Windows;
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
        public bool                IsSingleImageFileLoaded { get; private set; } = false; // 一度でも画像ファイル単体でロードされたか
        public bool                ApplyRotateInfoFromExif { get; set; } = false;
        public int                 NextIndex { get; set; }
        public int                 PrevIndex { get; set; }

        public bool IsSingleArchiver
        {
            get
            {
                if( Archivers.Count == 1 && !IsSingleImageFileLoaded ) return true;
                else return false;
            }
        }
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

        public ImageFileInfo CurrentImageFileInfo
        {
            get
            {
                if( ImgFileInfo.Count > 0 && CurrentIndex < ImgFileInfo.Count )
                    return ImgFileInfo[CurrentIndex];
                else
                    return null;
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
                if( ifi != null )
                {
                    IsSingleImageFileLoaded = true;
                    ImgFileInfo.Add(ifi);
                }

                // 圧縮ファイル / その他のファイル
                else
                {
                    string ext = Path.GetExtension(path);

                    switch( ext )
                    {
                        case ".zip":
                            Archivers.Add(archiver = new ZipArchiver(path));
                            break;
                        case ".rar":
                            Archivers.Add(archiver = new RarArchiver(path));
                            break;
                        case ".7z":
                            Archivers.Add(archiver = new SevenZipArchiver(path));
                            break;
                        case ".tar":
                            Archivers.Add(archiver = new TarArchiver(path));
                            break;
                        default:
                            return;
                    }

                    ImgFileInfo.AddRange(archiver.LoadImageFileInfoList());
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
        /// <param name="bitmapDecodePixel"></param>
        /// <returns>BitmapSource(失敗時はnullを返す)</returns>
        public BitmapSource LoadBitmap(ImageFileInfo imageFileInfo, Size bitmapDecodePixel)
        {
            string path = imageFileInfo.FilePath;
            if( imageFileInfo.IsDummy || path == "" ) return null;

            var source = new BitmapImage();

            using(Stream st = imageFileInfo.Archiver.OpenStream(path) )
            {
                try
                {
                    source.BeginInit();
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.CreateOptions = BitmapCreateOptions.None;

                    // 画像のアス比から、縦横どちらのDecodePixelを適用するのかを決める
                    // DecodePixelの値が、画像のサイズをオーバーする場合は、画像サイズをDecodePixelの値として指定する
                    if( bitmapDecodePixel != Size.Empty )
                    {
                        double imgRatio = imageFileInfo.PixelSize.Height / (double)imageFileInfo.PixelSize.Width;
                        if( imgRatio > 1.0 ) // 画像が縦長
                        {
                            if(bitmapDecodePixel.Height > imageFileInfo.PixelSize.Height)
                                source.DecodePixelHeight = (int)imageFileInfo.PixelSize.Height;
                            else
                                source.DecodePixelHeight = (int)bitmapDecodePixel.Height;
                        }
                        else // 画像が横長
                        {
                            if(bitmapDecodePixel.Width > imageFileInfo.PixelSize.Width)
                                source.DecodePixelWidth = (int)imageFileInfo.PixelSize.Width;
                            else
                                source.DecodePixelWidth = (int)bitmapDecodePixel.Width;
                        }
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

        public void ClearFileInfo()
        {
            foreach( ArchiverBase archicer in Archivers )
            {
                archicer.DisposeArchive();
            }
            Archivers.Clear();
            ImgFileInfo.Clear();
            IsSingleImageFileLoaded = false;
        }

        public void Sort(FileSortMethod order)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            if (ImgFileInfo.Count < 1) return;

            switch (order)
            {
                case FileSortMethod.FileName:
                    ImgFileInfo = ImgFileInfo.OrderBy(f => f.FilePath).ToList();
                    break;
                case FileSortMethod.FileNameRev:
                    ImgFileInfo = ImgFileInfo.OrderByDescending(f => f.FilePath).ToList();
                    break;
                case FileSortMethod.FileNameNatural:
                    ImgFileInfo = ImgFileInfo.OrderBy(f => f.FilePath, new NaturalStringComparer()).ToList();
                    break;
                case FileSortMethod.FileNameNaturalRev:
                    ImgFileInfo = ImgFileInfo.OrderByDescending(f => f.FilePath, new NaturalStringComparer()).ToList();
                    break;
                case FileSortMethod.LastWriteTime:
                    foreach( ImageFileInfo ifi in ImgFileInfo ) ifi.ReadLastWriteTime();
                    ImgFileInfo = ImgFileInfo.OrderBy(f => f.LastWriteTime).ToList();
                    break;
                case FileSortMethod.LastWriteTimeRev:
                    foreach( ImageFileInfo ifi in ImgFileInfo ) ifi.ReadLastWriteTime();
                    ImgFileInfo = ImgFileInfo.OrderByDescending(f => f.LastWriteTime).ToList();
                    break;
                case FileSortMethod.DateTaken:
                    foreach( ImageFileInfo ifi in ImgFileInfo ) ifi.ReadSlideViewInfo();
                    ImgFileInfo = ImgFileInfo.OrderBy((f) => {
                        if( f.ExifInfo == null || f.ExifInfo.DateTaken == null) return new DateTimeOffset();
                        return f.ExifInfo.DateTaken;
                    }).ToList();
                    break;
                case FileSortMethod.DateTakenRev:
                    foreach( ImageFileInfo ifi in ImgFileInfo ) ifi.ReadSlideViewInfo();
                    ImgFileInfo = ImgFileInfo.OrderByDescending((f) => {
                        if( f.ExifInfo == null || f.ExifInfo.DateTaken == null) return new DateTimeOffset();
                        return f.ExifInfo.DateTaken;
                    }).ToList();
                    break;
                case FileSortMethod.Random:
                    ImgFileInfo.Shuffle();
                    break;
                case FileSortMethod.None:
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
            ImgFileInfo.RemoveAll(fi => fi.IsDummy);

            while(ImgFileInfo.Count % grids != 0)
            {
                ImageFileInfo imgFileInfo = new ImageFileInfo("");
                imgFileInfo.IsDummy = true;
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
