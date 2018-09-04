using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using System.Windows;

using System.Windows.Controls;

namespace C_SlideShow
{
    public class Tile
    {
        public Image Image { get; private set; }
        public Border Border { get; private set; }
        public int Row { set; get; }
        public int Col { set; get; }
        public bool ByPlayback { get; set; } // タイルの設置が巻き戻しによるものか
        public ImageFileInfo ImageFileInfo { get; set; }
        public bool IsDummy { get; set; }
        public TileContainer ParentConteiner { get; set; }

        public Tile()
        {
            Image = new Image();
            Border = new Border();
            Border.Background = new SolidColorBrush(Colors.Transparent);
            IsDummy = false;
        }


        public void SetGridPos(int row, int col, bool byPlayback)
        {
            Row = row;
            Col = col;
            ByPlayback = byPlayback;
        }
        
        // エクスプローラーでタイルを開く
        public void OpenExplorer()
        {
            string folderDirPath;
            string filePath;
            if( ImageFileInfo.Archiver.CanReadFile )
            {
                folderDirPath = Directory.GetParent(ImageFileInfo.FilePath).FullName;
                filePath = ImageFileInfo.FilePath;
            }
            else
            {
                folderDirPath = Directory.GetParent(ImageFileInfo.Archiver.ArchiverPath).FullName;
                filePath = ImageFileInfo.Archiver.ArchiverPath;
            }
            Process.Start("explorer.exe", "/select,\"" + filePath + "\"");
        }

        // ファイルをコピー
        public void CopyFile()
        {
            string notificationFileName;
            string srcFilePath;

            if( ImageFileInfo.Archiver.CanReadFile )
            {
                srcFilePath = ImageFileInfo.FilePath;
                notificationFileName = System.IO.Path.GetFileName(ImageFileInfo.FilePath);
            }
            else
            {
                // 書庫内ファイルの場合
                if(ImageFileInfo.TempFilePath == null) ImageFileInfo.WriteToTempFolder();

                srcFilePath = ImageFileInfo.TempFilePath;
                notificationFileName = ImageFileInfo.TempDirName + "\\" + System.IO.Path.GetFileName(ImageFileInfo.TempFilePath);
            }

            // コピー
            System.Collections.Specialized.StringCollection files = new System.Collections.Specialized.StringCollection();
            files.Add(srcFilePath);
            Clipboard.SetFileDropList(files);

            // 通知
            MainWindow.Current.NotificationBlock.Show("クリップボードにファイルをコピーしました: " + notificationFileName,
                CommonControl.NotificationPriority.Normal, CommonControl.NotificationTime.Normal);
        }

        // 画像データをコピー
        public void CopyImageData()
        {
            BitmapSource source = MainWindow.Current.ImageFileManager.LoadBitmap( ImageFileInfo, new Size(0, 0) );
            Clipboard.SetImage(source);

            string fileName = System.IO.Path.GetFileName(ImageFileInfo.FilePath);
            MainWindow.Current.NotificationBlock.Show("クリップボードに画像データをコピーしました: " + fileName,
                CommonControl.NotificationPriority.Normal, CommonControl.NotificationTime.Normal);
        }

        // ファイルパスをコピー
        public void CopyFilePath()
        {
            string filePath;
            if( ImageFileInfo.Archiver.CanReadFile )
            {
                filePath = ImageFileInfo.FilePath;
            }
            else
            {
                filePath = ImageFileInfo.Archiver.ArchiverPath;
            }
            Clipboard.SetText(filePath);

            MainWindow.Current.NotificationBlock.Show("クリップボードにファイルパスをコピーしました: " + filePath,
                CommonControl.NotificationPriority.Normal, CommonControl.NotificationTime.Normal);
        }

        // ファイル名をコピー
        public void CopyFileName()
        {
            string fileName = System.IO.Path.GetFileName(ImageFileInfo.FilePath);
            Clipboard.SetText(fileName);

            MainWindow.Current.NotificationBlock.Show("クリップボードにファイル名をコピーしました: " + fileName,
                CommonControl.NotificationPriority.Normal, CommonControl.NotificationTime.Normal);
        }

        // 外部プログラムで画像を開く
        public void OpenByExternalApp(ExternalAppInfo exAppInfo)
        {
            // ファイルパスの決定
            string filePath;
            if( ImageFileInfo.Archiver.CanReadFile )
            {
                filePath = ImageFileInfo.FilePath;
            }
            else
            {
                // 書庫内ファイルなら一時展開
                if( ImageFileInfo.TempFilePath == null ) ImageFileInfo.WriteToTempFolder();
                filePath = ImageFileInfo.TempFilePath;
            }

            // 外部プログラム呼び出し
            if( exAppInfo != null)
            {
                string filePathFormat = "$FilePath$";

                string arg = exAppInfo.Arg;
                if( arg == "" ) arg = "\"" + filePathFormat + "\"";

                if(exAppInfo.Path != null && exAppInfo.Path != "" )
                {
                    // プログラムの指定あり
                    try { Process.Start( exAppInfo.Path, arg.Replace(filePathFormat, filePath) ); }
                    catch { }
                }
                else
                {
                    // プログラムの指定がなければ、拡張子で関連付けられているプログラムで開く
                    try { Process.Start( "\"" +  filePath +"\"" ); }
                    catch { }
                }
            }

        }

    }
}
