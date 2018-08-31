using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Forms = System.Windows.Forms;


namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// フォルダを開く
    /// </summary>
    public class OpenFolder : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public OpenFolder()
        {
            ID = CommandID.OpenFolder;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;

            var dlg = new Forms.FolderBrowserDialog();
            dlg.Description = "画像フォルダーを選択してください。";
            if( mw.Setting.FolderOpenDialogLastSelectedPath != null && Directory.Exists(mw.Setting.FolderOpenDialogLastSelectedPath) )
                dlg.SelectedPath = mw.Setting.FolderOpenDialogLastSelectedPath;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mw.Setting.FolderOpenDialogLastSelectedPath = dlg.SelectedPath;
                mw.SaveHistoryItem();

                mw.DropNewFiles( new string[] { dlg.SelectedPath });

                // 拡大中なら解除
                if( mw.TileExpantionPanel.IsShowing ) mw.TileExpantionPanel.Hide();
            }
        }

        public string GetDetail()
        {
            return "フォルダを読み込み";
        }
    }
}
