using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    public class ShiftForward : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public ShiftForward()
        {
            ID    = CommandID.ShiftForward;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;
            int current = mw.ImageFileManager.ActualCurrentIndex;
            current += Value;
            int maxIndex = mw.ImageFileManager.ImgFileInfo.Count - 1;
            if( current > maxIndex ) current = (current - 1) % maxIndex;

            mw.ChangeCurrentImageIndex(current);

            return;
        }

        public string GetDetail()
        {
            return "画像" + Value.ToString() + "枚分ずらし進める";
        }
    }
}
