using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 上方向にスライド
    /// </summary>
    public class SlideToTop : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public SlideToTop()
        {
            ID    = CommandID.SlideToTop;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            if( !MainWindow.Current.IsHorizontalSlide )
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
                mw.StartOperationSlide(false, false);
            }
            else
            {
                mw.StartOperationSlide(true, false);
            }
        }

        public string GetDetail()
        {
            return "上方向にスライド";
        }
    }
}
