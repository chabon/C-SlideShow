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
    /// フォルダを追加読み込み
    /// </summary>
    public class OpenAdditionalFolder : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public OpenAdditionalFolder()
        {
            ID = CommandID.OpenAdditionalFolder;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;

            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(10) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();

                var dlg = new Forms.FolderBrowserDialog();
                dlg.Description = "追加する画像フォルダーを選択してください。";
                if( mw.Setting.FolderOpenDialogLastSelectedPath != null && Directory.Exists(mw.Setting.FolderOpenDialogLastSelectedPath) )
                    dlg.SelectedPath = mw.Setting.FolderOpenDialogLastSelectedPath;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    mw.Setting.FolderOpenDialogLastSelectedPath = dlg.SelectedPath;
                    string[] path = { dlg.SelectedPath };
                    mw.ReadFilesAndInitMainContent(path, true,  0);

                    // 拡大中なら解除
                    if( mw.TileExpantionPanel.IsShowing ) mw.TileExpantionPanel.Hide();
                }
            };
            timer.Start();
        }

        public string GetDetail()
        {
            return "フォルダを追加読み込み";
        }
    }
}
