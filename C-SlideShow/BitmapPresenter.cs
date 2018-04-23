using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Windows.Controls;
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
        public List<ImageFileInfo> ImgFileInfo { get; set; }
        public static string DummyFilePath = ":dummy";
        public int BitmapDecodePixelWidth { get; set; } = 640;
        public int NextIndex { get; set; }
        public int PrevIndex { get; set; }

        string[] allowedExt = { ".jpg", ".png", ".jpeg", ".bmp", ".gif" };
        ZipArchive zipArchive;

        #region property
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

        #endregion

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
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            // 拡張子でフィルタ
            var filteredFiles = filePathes.Where(file => allowedExt.Any(ext => 
                file.ToLower().EndsWith(ext)));

            ImgFileInfo.Clear();
            foreach (string imgPath in filteredFiles)
            {
                //if (System.IO.File.Exists(imgPath))
                ImageFileInfo imageFileInfo = new ImageFileInfo();
                imageFileInfo.FilePath = imgPath;
                //imageFileInfo.LastWriteTime = File.GetLastWriteTime(imgPath);
                ImgFileInfo.Add(imageFileInfo);
            }
            this.ReadType = BitmapReadType.File;

#if DEBUG
            sw.Stop();
            Debug.WriteLine( ImgFileInfo.Count + " / " + filePathes.Length
                + " files info loaded"  + " time: " + sw.Elapsed);
#endif
        }

        public void LoadFileInfoFromZip(string filePath)
        {
            try
            {
                this.zipArchive = ZipFile.OpenRead(filePath);

                this.ImgFileInfo.Clear();
                
                foreach(ZipArchiveEntry entory in this.zipArchive.Entries)
                {
                    // ファイル情報を拡張子でフィルタ
                    if(  allowedExt.Any( ext => entory.FullName.ToLower().EndsWith(ext) ))
                    {
                        ImageFileInfo fi = new ImageFileInfo(entory.FullName);
                        fi.LastWriteTime = entory.LastWriteTime;
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
            }


#if DEBUG
            sw.Stop();
            Debug.WriteLine("-----------------------------------------------------");
            Debug.WriteLine(" sort end    order: " + order + "  time: " + sw.Elapsed);
            Debug.WriteLine("-----------------------------------------------------");
#endif
        }


        public BitmapImage LoadBitmap(string filePath, bool bLoadOrginalPxcelSize)
        {
            if (filePath == DummyFilePath || filePath == "") return null;

            var source = new BitmapImage();

            // zip
            if(ReadType == BitmapReadType.Zip)
            {
                ZipArchiveEntry entory = zipArchive.GetEntry(filePath);

                try
                {
                    //var zipStream = entory.Open();
                    //var memoryStream = new MemoryStream();
                    using (var zipStream = entory.Open())
                    using (var memoryStream = new MemoryStream())
                    {
                        zipStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        source.BeginInit();
                        source.CacheOption = BitmapCacheOption.OnLoad;
                        source.CreateOptions = BitmapCreateOptions.None;
                        if( !bLoadOrginalPxcelSize)
                            source.DecodePixelWidth = BitmapDecodePixelWidth;
                        source.StreamSource = memoryStream;
                        source.EndInit();
                        source.Freeze();

                        Debug.WriteLine("bitmap from zip: " + filePath);
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
                    source.BeginInit();
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.CreateOptions = BitmapCreateOptions.None;
                    if( !bLoadOrginalPxcelSize)
                        source.DecodePixelWidth = BitmapDecodePixelWidth;
                    source.UriSource = new Uri(filePath);
                    source.EndInit();
                    source.Freeze();

                    Debug.WriteLine("bitmap from file: " + filePath);
                    return source;
                }
                catch
                {
                    return null;
                }
            }

            return null;
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
