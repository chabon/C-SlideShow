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
    /// ファイルを開く
    /// </summary>
    public class OpenFile : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public OpenFile()
        {
            ID = CommandID.OpenFile;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;

            Forms.OpenFileDialog ofd = new Forms.OpenFileDialog();
            ofd.Title = "ファイルを選択してください";
            ofd.Multiselect = true;
            if( mw.Setting.FileOpenDialogLastSelectedPath != null && File.Exists(mw.Setting.FileOpenDialogLastSelectedPath) )
                ofd.InitialDirectory = Directory.GetParent( mw.Setting.FileOpenDialogLastSelectedPath ).FullName;

            if (ofd.ShowDialog() == Forms.DialogResult.OK)
            {
                if(ofd.FileNames.Length > 0) mw.Setting.FileOpenDialogLastSelectedPath = ofd.FileNames[0];
                mw.SaveHistoryItem();

                mw.DropNewFiles(ofd.FileNames);

                // 拡大中なら解除
                if( mw.TileExpantionPanel.IsShowing ) mw.TileExpantionPanel.Hide();
            }
        }

        public string GetDetail()
        {
            return "ファイルを読み込み";
        }
    }
}
