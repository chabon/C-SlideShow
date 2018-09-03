using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// アプリの設定ダイアログを表示
    /// </summary>
    public class ShowAppSettingDialog : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ShowAppSettingDialog()
        {
            ID    = CommandID.ShowAppSettingDialog;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow.Current.ShowAppSettingDialog(MainWindow.Current.Setting.AppSettingDialogTabIndex);
            return;
        }

        public string GetDetail()
        {
            return "アプリの設定ダイアログを表示";
        }
    }
}
