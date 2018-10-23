using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 前方向に画像1枚分だけスライド
    /// </summary>
    public class SlideToForwardByOneImage : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public SlideToForwardByOneImage()
        {
            ID    = CommandID.SlideToForwardByOneImage;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;
            mw.ImgContainerManager.ActiveSlideToForward(true, mw.Setting.OperationSlideDuration, false);
        }

        public string GetDetail()
        {
            return "前方向に画像1枚分だけスライド";
        }
    }
}
