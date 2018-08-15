using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 後方向に画像1枚分だけスライド
    /// </summary>
    public class SlideToBackwardByOneImage : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public SlideToBackwardByOneImage()
        {
            ID    = CommandID.SlideToBackwardByOneImage;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;
            mw.StartOperationSlide(true, true);
        }

        public string GetDetail()
        {
            return "後方向に画像1枚分だけスライド";
        }
    }
}
