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



namespace C_SlideShow
{

    public enum BitmapReadType { File, Zip, Rar, Pdf }

    public class BitmapPresenter
    {
        public class ExifInfo
        {
            public Rotation Rotation;
            public ScaleTransform ScaleTransform;

            public ExifInfo()
            {
                Rotation = Rotation.Rotate0;
                ScaleTransform = null;
            }

            public ExifInfo(Rotation _rotation, ScaleTransform st)
            {
                Rotation = _rotation;
                ScaleTransform = st;
            }
        }

        public List<ImageFileInfo> ImgFileInfo { get; set; }
        public static string DummyFilePath = ":dummy";
        public bool ApplyRotateInfoFromExif { get; set; } = false;
        public int NextIndex { get; set; }
        public int PrevIndex { get; set; }

        string[] allowedExt = { ".jpg", ".png", ".jpeg", ".bmp", ".gif" };
        ZipArchive zipArchive;

        public BitmapReadType ReadType { get; set; }

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

        int numofDummyFileInfo; // コンテナ内の隙間グリッド埋め用のダミー情報の数

        public BitmapPresenter()
        {
            ImgFileInfo = new List<ImageFileInfo>();
            NextIndex = 0;
            PrevIndex = 0;
            ReadType = BitmapReadType.File;
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

        public void LoadFileInfoFromFile(string[] filePathes)
        {
            // 拡張子でフィルタ
            var filteredFiles = filePathes.Where(file => allowedExt.Any(ext => 
                file.ToLower().EndsWith(ext)));

            foreach (string imgPath in filteredFiles)
            {
                //if (System.IO.File.Exists(imgPath))
                ImageFileInfo imageFileInfo = new ImageFileInfo();
                imageFileInfo.FilePath = imgPath;
                //imageFileInfo.LastWriteTime = File.GetLastWriteTime(imgPath);
                ImgFileInfo.Add(imageFileInfo);
            }
        }

        public void LoadFileInfoFromFile(string filePath)
        {
            // 拡張子でフィルタ
            if( !allowedExt.Any(ext => filePath.ToLower().EndsWith(ext)) ) return;

            ImageFileInfo imageFileInfo = new ImageFileInfo();
            imageFileInfo.FilePath = filePath;
            this.ImgFileInfo.Add(imageFileInfo);
        }


        public void LoadFileInfoFromDir(string dirPath)
        {
            if (!System.IO.Directory.Exists(dirPath))
            {
                return;
            }

            var imgPathes = System.IO.Directory.GetFiles(
                dirPath, "*.*", System.IO.SearchOption.AllDirectories);

            this.LoadFileInfoFromFile(imgPathes);
        }

        public void LoadFileInfoFromZip(string filePath)
        {
            try
            {
                this.zipArchive = ZipFile.OpenRead(filePath);
                
                foreach(ZipArchiveEntry entory in this.zipArchive.Entries)
                {
                    // ファイル情報を拡張子でフィルタ
                    if(  allowedExt.Any( ext => entory.FullName.ToLower().EndsWith(ext) ))
                    {
                        ImageFileInfo fi = new ImageFileInfo(entory.FullName);
                        fi.LastWriteTime = entory.LastWriteTime;
                        fi.Length = entory.Length;
                        ImgFileInfo.Add(fi);
                    }
                }
            }
            catch
            {
                if(this.zipArchive != null) this.zipArchive.Dispose();
            }
            this.ReadType = BitmapReadType.Zip;
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


        public BitmapSource LoadBitmap(string filePath, int bitmapDecodePixelWidth)
        {
            if (filePath == DummyFilePath || filePath == "") return null;

            var source = new BitmapImage();

            // zip
            if(ReadType == BitmapReadType.Zip)
            {
                ZipArchiveEntry entory = zipArchive.GetEntry(filePath);

                try
                {
                    using (var zipStream = entory.Open())
                    using (var ms_bitmap = new MemoryStream())
                    {
                        zipStream.CopyTo(ms_bitmap);
                        ms_bitmap.Position = 0;

                        // Exif情報取得
                        ExifInfo ei;
                        if( ApplyRotateInfoFromExif )
                        {
                            using (var ms_exif = new MemoryStream() )
                            using (var zs_exif = entory.Open())
                            {
                                zs_exif.CopyTo(ms_exif);
                                ms_exif.Position = 0;
                                ei = GetExifInfo(ms_exif);
                            }
                        }
                        else ei = new ExifInfo();

                        source.BeginInit();
                        source.CacheOption = BitmapCacheOption.OnLoad;
                        source.CreateOptions = BitmapCreateOptions.None;
                        if( bitmapDecodePixelWidth != 0)
                            source.DecodePixelWidth = bitmapDecodePixelWidth;
                        source.StreamSource = ms_bitmap;
                        source.Rotation = ei.Rotation;
                        source.EndInit();
                        source.Freeze();

                        Debug.WriteLine("bitmap from zip: " + filePath);

                        // Exifに反転もあった場合は、BitmapImage.Rotationで対応出来ないのでTransform
                        if( ei.ScaleTransform != null )
                        {
                            return TransformBitmap(source, ei.ScaleTransform);
                        }

                        return source;
                    }
                }
                catch { return null; }
            }

            // file
            else if(ReadType == BitmapReadType.File)
            {
                try
                {
                    // Exif情報取得
                    ExifInfo ei;
                    if( ApplyRotateInfoFromExif )
                    {
                        FileStream fs = File.OpenRead(filePath);
                        ei = GetExifInfo(fs);
                    }
                    else ei = new ExifInfo();

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

            return null;
        }

        public ExifInfo GetExifInfo(Stream st)
        {
            // Exif(メタデータ)取得
            BitmapFrame bmf = BitmapFrame.Create(st);
            var metaData = (bmf.Metadata) as BitmapMetadata;
            //bmf.Freeze();
            st.Position = 0;
            st.Close();

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

        BitmapSource TransformBitmap(BitmapSource source, Transform transform) {
            var result = new TransformedBitmap();
            result.BeginInit();
            result.Source = source;
            result.Transform = transform;
            result.EndInit();
            result.Freeze();
            return result;
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
                ImgFileInfo.Add(new ImageFileInfo(DummyFilePath));
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
