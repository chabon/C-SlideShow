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
    /// ファイルを追加読み込み
    /// </summary>
    public class OpenAdditionalFile : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public OpenAdditionalFile()
        {
            ID = CommandID.OpenAdditionalFile;
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
            ofd.Title = "追加するファイルを選択してください";
            ofd.Multiselect = true;
            if( mw.Setting.FileOpenDialogLastSelectedPath != null && File.Exists(mw.Setting.FileOpenDialogLastSelectedPath) )
                ofd.InitialDirectory = Directory.GetParent( mw.Setting.FileOpenDialogLastSelectedPath ).FullName;

            if (ofd.ShowDialog() == Forms.DialogResult.OK)
            {
                if(ofd.FileNames.Length > 0) mw.Setting.FileOpenDialogLastSelectedPath = ofd.FileNames[0];
                mw.ReadFilesAndInitMainContent(ofd.FileNames, true,  0);

                // 拡大中なら解除
                if( mw.TileExpantionPanel.IsShowing ) mw.TileExpantionPanel.Hide();
            }
        }

        public string GetDetail()
        {
            return "ファイルを追加読み込み";
        }
    }
}
