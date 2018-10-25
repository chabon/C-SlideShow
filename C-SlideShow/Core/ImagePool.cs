using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.IO;

using C_SlideShow.Archiver;
using System.Diagnostics;

namespace C_SlideShow.Core
{
    public class ImagePool
    {
        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        public List<ImageFileContext>  ImageFileContextList  = new List<ImageFileContext>();
        public static ImageFileContext DummyImageContext = new ImageFileContext(null) { IsDummy = true };
        public int ForwardIndex  { get; private set; } = 0;
        public int BackwardIndex { get; private set; } = 0;
        public List<ArchiverBase> Archivers { get; set; } = new List<ArchiverBase>();
        public NullArchiver        NullArchiver { get; set; } = new NullArchiver();
        public bool IsSingleArchiver
        {
            get
            {
                if( Archivers.Count == 1 && ImageFileContextList.All( i => i.Archiver == Archivers[0]) ) return true;
                else return false;
            }
        }

        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */
        public void Initialize(string[] pathes)
        {
            ImageFileContextList.Clear();
            Archivers.Clear();

            foreach( string path in pathes )
            {
                LoadFileOrDirectory(path);
            }

            InitIndex(0);
        }

        private void LoadFileOrDirectory(string path)
        {
            // 新規アーカイバ
            ArchiverBase archiver;

            // ファイル
            if( File.Exists(path) )
            {
                // 画像ファイル単体
                ImageFileContext ifc = NullArchiver.LoadImageFileContext(path);
                if( ifc != null )
                {
                    ImageFileContextList.Add(ifc);
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

                    ImageFileContextList.AddRange(archiver.LoadImageFileContextList());
                }
            }

            // フォルダ
            else if( Directory.Exists(path) )
            {
                Archivers.Add( archiver = new FolderArchiver(path) );
                ImageFileContextList.AddRange( archiver.LoadImageFileContextList() );
            }
        }

        public void InitIndex(int index)
        {
            int maxIdx = ImageFileContextList.Count - 1;

            // 前方向
            ForwardIndex = index;
            if(ForwardIndex < 0 || maxIdx < ForwardIndex )
            {
                ForwardIndex = 0;
            }

            // 巻き戻し方向
            BackwardIndex = ForwardIndex - 1;
            if( BackwardIndex < 0 )
            {
                BackwardIndex = maxIdx;
            }
        }

        public void InitImageFileContextRefCount()
        {
            foreach( ImageFileContext context in ImageFileContextList )
            {
                context.RefCount = 0;
            }
        }

        public void ReleaseBitmapImageOutofRefarence()
        {
            foreach( ImageFileContext context in ImageFileContextList )
            {
                if( context.RefCount == 0 ) context.BitmapImage = null;
            }
        }

        public void ReleaseAllBitmapImage()
        {
            foreach( ImageFileContext context in ImageFileContextList )
            {
                context.BitmapImage = null;
            }
        }

        public void ShiftForwardIndex(int vari)
        {
            ForwardIndex += vari;
            int count = ImageFileContextList.Count;

            if( ForwardIndex >= count )
            {
                ForwardIndex = ForwardIndex % count;
            }
            else if( ForwardIndex < 0)
            {
                int p = ForwardIndex % count;
                if( p == 0 ) ForwardIndex = 0;
                else ForwardIndex = count + p;
            }
        }

        public void ShiftBackwardIndex(int vari)
        {
            BackwardIndex += vari;
            int count = ImageFileContextList.Count;

            if( BackwardIndex >= count )
            {
                BackwardIndex = BackwardIndex % count;
            }
            else if( BackwardIndex < 0)
            {
                int p = BackwardIndex % count;
                if( p == 0 ) BackwardIndex = 0;
                else BackwardIndex = count + p;
            }
        }

        public ImageFileContext PickForward()
        {
            ImageFileContext context = ImageFileContextList[ForwardIndex];
            ImageFileContextList[ForwardIndex].RefCount++;
            ForwardIndex++;
            if(ForwardIndex >= ImageFileContextList.Count )
            {
                ForwardIndex = 0;
            }

            return context;
        } 

        public ImageFileContext PickBackward()
        {
            ImageFileContext context = ImageFileContextList[BackwardIndex];
            ImageFileContextList[BackwardIndex].RefCount++;
            BackwardIndex--;
            if(BackwardIndex < 0 )
            {
                BackwardIndex = ImageFileContextList.Count - 1;
            }

            return context;
        } 


        /// <summary>
        /// 次のピックが見開きかチェック
        /// </summary>
        /// <param name="isBackward">巻き戻し方向をチェック</param>
        /// <returns>見開きならtrue</returns>
        public bool IsNextPickImageSpreaded(bool isBackward)
        {
            ImageFileContext context;
            if( !isBackward ) context = ImageFileContextList[ForwardIndex];
            else context = ImageFileContextList[BackwardIndex];

            context.ReadInfoForView();
            Size pxSize = context.Info.PixelSize;
            if( pxSize == Size.Empty || pxSize == null ) return false;

            switch( MainWindow.Current.Setting.TempProfile.DetectionOfSpread.Value )
            {
                default:
                case DetectionOfSpread.None:
                    return false;
                case DetectionOfSpread.ByWideImage:
                    if( pxSize.Width > pxSize.Height ) return true;
                    else return false;
                case DetectionOfSpread.ByHighImage:
                    if( pxSize.Height > pxSize.Width ) return true;
                    else return false;
            }
        }


        /// <summary>
        /// ソート
        /// </summary>
        /// <param name="order"></param>
        public void Sort(FileSortMethod order)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            if (ImageFileContextList.Count < 1) return;

            switch (order)
            {
                case FileSortMethod.FileName:
                    ImageFileContextList = ImageFileContextList.OrderBy(i => i.FilePath).ToList();
                    break;
                case FileSortMethod.FileNameRev:
                    ImageFileContextList = ImageFileContextList.OrderByDescending(i => i.FilePath).ToList();
                    break;
                case FileSortMethod.FileNameNatural:
                    ImageFileContextList = ImageFileContextList.OrderBy(i => i.FilePath, new NaturalStringComparer()).ToList();
                    break;
                case FileSortMethod.FileNameNaturalRev:
                    ImageFileContextList = ImageFileContextList.OrderByDescending(i => i.FilePath, new NaturalStringComparer()).ToList();
                    break;
                case FileSortMethod.LastWriteTime:
                    foreach( ImageFileContext ifc in ImageFileContextList ) ifc.ReadLastWriteTime();
                    ImageFileContextList = ImageFileContextList.OrderBy(i => i.Info.LastWriteTime).ToList();
                    break;
                case FileSortMethod.LastWriteTimeRev:
                    foreach( ImageFileContext ifc in ImageFileContextList ) ifc.ReadLastWriteTime();
                    ImageFileContextList = ImageFileContextList.OrderByDescending(i => i.Info.LastWriteTime).ToList();
                    break;
                case FileSortMethod.DateTaken:
                    foreach( ImageFileContext ifc in ImageFileContextList ) ifc.ReadInfoForView();
                    ImageFileContextList = ImageFileContextList.OrderBy((i) => {
                        if( i.Info.ExifInfo == null || i.Info.ExifInfo.DateTaken == null) return new DateTimeOffset();
                        return i.Info.ExifInfo.DateTaken;
                    }).ToList();
                    break;
                case FileSortMethod.DateTakenRev:
                    foreach( ImageFileContext ifc in ImageFileContextList ) ifc.ReadInfoForView();
                    ImageFileContextList = ImageFileContextList.OrderByDescending((i) => {
                        if( i.Info.ExifInfo == null || i.Info.ExifInfo.DateTaken == null) return new DateTimeOffset();
                        return i.Info.ExifInfo.DateTaken;
                    }).ToList();
                    break;
                case FileSortMethod.Random:
                    ImageFileContextList.Shuffle();
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

    }
}
