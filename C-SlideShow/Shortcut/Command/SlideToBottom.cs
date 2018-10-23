using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 下方向にスライド
    /// </summary>
    public class SlideToBottom : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public SlideToBottom()
        {
            ID    = CommandID.SlideToBottom;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            if( !MainWindow.Current.Setting.TempProfile.IsHorizontalSlide )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;

            if(mw.Setting.TempProfile.SlideDirection.Value == SlideDirection.Top )
            {
                mw.ImgContainerManager.ActiveSlideToBackward(false, mw.Setting.OperationSlideDuration);
            }
            else
            {
                mw.ImgContainerManager.ActiveSlideToForward(false, mw.Setting.OperationSlideDuration, false);
            }
        }

        public string GetDetail()
        {
            return "下方向にスライド";
        }
    }
}
