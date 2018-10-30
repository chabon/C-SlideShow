using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    public class CloseFile : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public CloseFile()
        {
            ID    = CommandID.CloseFile;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            Profile pf = MainWindow.Current.Setting.TempProfile;

            pf.Path.Value.Clear();
            MainWindow.Current.ImgContainerManager.ImagePool.Initialize(new string[] { }, false);
            MainWindow.Current.Reload(false);

            return;
        }

        public string GetDetail()
        {
            return "ファイルを閉じる";
        }
    }
}
