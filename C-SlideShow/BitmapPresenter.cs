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
        public List<ImageFileInfo> FileInfo { get; set; }
        public static string DummyFilePath = ":dummy";
        public int NextIndex { get; set; }
        public int PrevIndex { get; set; }

        string[] allowedExt = { ".jpg", ".png", ".jpeg", ".gif" };
        ZipArchive zipArchive;

        #region property
        public BitmapReadType ReadType { get; set; }

        public int NumofImageFile
        {
            get
            {
                return FileInfo.Count - numofDummyFileInfo;
            }
        }

        public int ActualCurrentIndex
        {
            get
            {
                // ダミーであろうと関係なく返す
                int idx = PrevIndex + 1;
                if (idx > this.FileInfo.Count - 1) idx = 0;
                return idx;
            }
        }
        public int CurrentIndex
        {
            get
            {
                // ダミーだったら対象外(0を返す)
                int idx = PrevIndex + 1;
                if (idx > this.FileInfo.Count - 1 - numofDummyFileInfo) idx = 0;
                return idx;
            }
        }

        public bool IsCurrentIndexDummy
        {
            get
            {
                int idx = PrevIndex + 1;
                if (idx < this.FileInfo.Count - 1 - numofDummyFileInfo) return false;
                else if (idx == this.FileInfo.Count) return false;
                else return true;
            }
        }

        int numofDummyFileInfo; // コンテナ内の隙間グリッド埋め用のダミー情報の数

        #endregion

        public BitmapPresenter()
        {
            FileInfo = new List<ImageFileInfo>();
            NextIndex = 0;
            PrevIndex = 0;
            ReadType = BitmapReadType.File;
        }


        /* ---------------------------------------------------- */
        //     method
        /* ---------------------------------------------------- */
        public void InitPrevIndex(int firstIndex)
        {
            if (FileInfo.Count < 1) return;
            PrevIndex = firstIndex - 1;
            if (PrevIndex < 0) PrevIndex = FileInfo.Count - 1;
        }

        public ImageFileInfo PickImageFileInfo(bool isPlayback)
        {
            if (FileInfo.Count > 0)
            {
                if (isPlayback) return FileInfo[PrevIndex];
                else return FileInfo[NextIndex];
            }
            else return null;
        }


        public int GetLastNoDeviationIndex(int grids)
        {
            int idx = FileInfo.Count - grids;
            if (idx < 0) idx = 0;
            return idx;
        }

        public string CreateCurrentIndexInfoString()
        {
            int idx = CurrentIndex;
            int num = idx + 1;
            int numMax = FileInfo.Count - numofDummyFileInfo;
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

            FileInfo.Clear();
            foreach (string imgPath in filteredFiles)
            {
                //if (System.IO.File.Exists(imgPath))
                ImageFileInfo imageFileInfo = new ImageFileInfo();
                imageFileInfo.FilePath = imgPath;
                //imageFileInfo.LastWriteTime = File.GetLastWriteTime(imgPath);
                FileInfo.Add(imageFileInfo);
            }
            this.ReadType = BitmapReadType.File;

#if DEBUG
            sw.Stop();
            Debug.WriteLine( FileInfo.Count + " / " + filePathes.Length
                + " files info loaded"  + " time: " + sw.Elapsed);
#endif
        }

        public void LoadFileInfoFromZip(string filePath)
        {
            try
            {
                this.zipArchive = ZipFile.OpenRead(filePath);

                this.FileInfo.Clear();
                
                foreach(ZipArchiveEntry entory in this.zipArchive.Entries)
                {
                    // ファイル情報を拡張子でフィルタ
                    if(  allowedExt.Any( ext => entory.FullName.ToLower().EndsWith(ext) ))
                    {
                        ImageFileInfo fi = new ImageFileInfo(entory.FullName);
                        fi.LastWriteTime = entory.LastWriteTime;
                        FileInfo.Add(fi);
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
            if (FileInfo.Count < 1) return;

            DateTimeOffset dateDefault = new DateTimeOffset();
            if(FileInfo[0].LastWriteTime.CompareTo(dateDefault) == 0)
            {
                foreach(ImageFileInfo fi in FileInfo)
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
            if (FileInfo.Count < 1) return;

            switch (order)
            {
                case FileReadingOrder.FileName:
                    FileInfo = FileInfo.OrderBy(f => f.FilePath).ToList();
                    break;
                case FileReadingOrder.FileNameRev:
                    FileInfo = FileInfo.OrderByDescending(f => f.FilePath).ToList();
                    break;
                case FileReadingOrder.LastWriteTime:
                    GetLastWriteTimeFileInfo();
                    FileInfo = FileInfo.OrderByDescending(f => f.LastWriteTime).ToList();
                    break;
                case FileReadingOrder.LastWriteTimeRev:
                    GetLastWriteTimeFileInfo();
                    FileInfo = FileInfo.OrderBy(f => f.LastWriteTime).ToList();
                    break;
                case FileReadingOrder.Random:
                    FileInfo.Shuffle();
                    break;
            }


#if DEBUG
            sw.Stop();
            Debug.WriteLine("-----------------------------------------------------");
            Debug.WriteLine(" sort end    order: " + order + "  time: " + sw.Elapsed);
            Debug.WriteLine("-----------------------------------------------------");
#endif
        }


        public BitmapImage LoadBitmap(ImageFileInfo imageFileInfo)
        {
            string filePath = imageFileInfo.FilePath;
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
        /// ファイル総数がグリッド数の倍数にならないことで生じるFileInfoの空きをダミーで埋める
        /// </summary>
        /// <param name="grids"></param>
        public void FillVacantWithDummyFileInfo(int grids)
        {
            if (FileInfo.Count < 1) return;

            // 初期化
            numofDummyFileInfo = 0;
            FileInfo.RemoveAll(fi => fi.FilePath == DummyFilePath);

            while(FileInfo.Count % grids != 0)
            {
                FileInfo.Add(new ImageFileInfo(DummyFilePath));
                numofDummyFileInfo++;
            }
        }

        public void SlideIndex(bool isPlayback)
        {
            if (FileInfo.Count > 0)
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
            if (NextIndex > FileInfo.Count - 1) NextIndex = 0;
        }

        public void DecrementNextIndex()
        {
            NextIndex--;
            if (NextIndex < 0) NextIndex = FileInfo.Count - 1;
        }

        public void IncrementPrevIndex()
        {
            PrevIndex++;
            if (PrevIndex > FileInfo.Count - 1) PrevIndex = 0;
        }

        public void DecrementPrevIndex()
        {
            PrevIndex--;
            if (PrevIndex < 0) PrevIndex = FileInfo.Count - 1;
        }




    }


}
