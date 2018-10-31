using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using C_SlideShow.Core;
using C_SlideShow.CommonControl;


namespace C_SlideShow.Shortcut.Command
{
    public class OpenPrevArchiver : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public OpenPrevArchiver()
        {
            ID    = CommandID.OpenPrevArchiver;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            Profile pf = MainWindow.Current.Setting.TempProfile;

            // 履歴保存
            MainWindow.Current.SaveHistoryItem();

            // 現在のコンテキスト
            ImageFileContext ifc = MainWindow.Current.ImgContainerManager.CurrentImageFileContext;

            // フォルダ(書庫)のパス
            string archiverPath;
            if(ifc != null )
            {
                if(ifc.Archiver is Archiver.NullArchiver )
                {
                    archiverPath = Directory.GetParent(ifc.FilePath).FullName;
                }
                else
                {
                    archiverPath = ifc.Archiver.ArchiverPath;
                }
            }
            else
            {
                // TempProfileから取得
                if( pf.Path.Value.Count == 0 ) return;
                var cand = pf.Path.Value[0];
                if( Archiver.ArchiverBase.IsReadablePath(cand) ) archiverPath = cand;
                else return;
            }

            // 親フォルダ取得
            string parentFolderPath;
            try { parentFolderPath = Directory.GetParent(archiverPath).FullName; }
            catch { return; }

            // 親フォルダ内のフォルダ・ファイル一覧
            var candidates = Directory.GetDirectories(parentFolderPath).ToList();
            candidates.AddRange( Directory.GetFiles(parentFolderPath).ToList() );

            // 順番取得
            var currentPath = candidates.FirstOrDefault(c => c == archiverPath);
            if( currentPath == null ) return;
            var currentIndex = candidates.IndexOf(currentPath);

            // 次のフォルダ(書庫)を取得
            int cnt = 0;
            string nextPath = "";
            int nextIndex = currentIndex;
            while(cnt < candidates.Count)
            {
                // 取得
                nextIndex--;
                if( nextIndex < 0 ) nextIndex = candidates.Count - 1;
                nextPath = candidates[nextIndex];

                // チェック
                if( nextPath == currentPath ) { nextPath = ""; break; } // 1周して見つからず
                else if( Archiver.ArchiverBase.IsReadablePath(nextPath) ) break;　// 対応可能な書庫orフォルダ

                cnt++;
            }

            // 通知
            string message;
            if(nextPath == "" || nextPath == currentPath )
            {
                message = "前のフォルダ(書庫)がありません";
            }
            else
            {
                message = "前のフォルダ(書庫)： " + Path.GetFileName( nextPath );
            }
            MainWindow.Current.NotificationBlock.Show(message, NotificationPriority.Normal, NotificationTime.Normal, NotificationType.None);
            MainWindow.Current.Refresh();

            // 読み込み
            if(nextPath != "" && nextPath != currentPath) MainWindow.Current.DropNewFiles( new string[]{ nextPath });
            return;
        }

        public string GetDetail()
        {
            return "前のフォルダ(書庫)を開く";
        }
    }
}
