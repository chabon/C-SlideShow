using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 右方向にスライド
    /// </summary>
    public class SlideToRight : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public SlideToRight()
        {
            ID    = CommandID.SlideToRight;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            if( MainWindow.Current.Setting.TempProfile.IsHorizontalSlide )
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

            if(mw.Setting.TempProfile.SlideDirection.Value == SlideDirection.Left )
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
            return "右方向にスライド";
        }
    }
}
