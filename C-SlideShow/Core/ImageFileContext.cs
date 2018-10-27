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
        public string        FilePath       { get; set; }       // 書庫の場合はファイルまでの相対パス。PDFの場合はページ番号
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
        public async Task<BitmapSource> LoadBitmap(Size bitmapDecodePixel)
        {
            if( IsDummy ) return null;

            try
            {
                var source = await Archiver.LoadBitmap(bitmapDecodePixel, this);
                return source;
            }
            catch
            {
                return null;
            }
        }


        public void ReadInfoForView()
        {
            Archiver.ReadInfoForView(this);
        }


        public void ReadLastWriteTime()
        {
            if( IsDummy ) return;

            // FolderArchiver, NullArchiver意外は読めない
            if( !Archiver.CanReadFile ) return;

            // 取得済み
            if( Info.LastWriteTime != null ) return;
            
            // 読み込み
            Info.LastWriteTime = File.GetLastWriteTime(FilePath);
        }


        /* ---------------------------------------------------- */
        //     コマンド
        /* ---------------------------------------------------- */
        // エクスプローラーで開く
        public void OpenByExplorer()
        {
            ExternalAppInfo exAppInfo = new ExternalAppInfo();
            exAppInfo.Path = "explorer.exe";
            exAppInfo.Arg  = "/select," + Format.FilePathFormat;
            OpenByExternalApp(exAppInfo);
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

                srcFilePath = TempFilePath;
                notificationFileName = TempDirName + "\\" + Path.GetFileName(TempFilePath);
            }

            // コピー
            System.Collections.Specialized.StringCollection files = new System.Collections.Specialized.StringCollection();
            files.Add(srcFilePath);
            Clipboard.SetFileDropList(files);

            // 通知
            MainWindow.Current.NotificationBlock.Show("ファイルをコピー: " + notificationFileName,
                NotificationPriority.Normal, NotificationTime.Normal, NotificationType.None);
        }


        // 画像データをコピー
        public async Task CopyImageData()
        {
            BitmapSource source = await LoadBitmap( new Size(0, 0) );
            Clipboard.SetImage(source);

            string fileName = System.IO.Path.GetFileName(FilePath);
            MainWindow.Current.NotificationBlock.Show("画像データをコピー: " + fileName, NotificationPriority.Normal, NotificationTime.Normal, NotificationType.None);
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
            if( Archiver is PdfArchiver ) ext = "png";
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



    }
}
