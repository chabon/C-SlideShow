using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 左方向にスライド
    /// </summary>
    public class SlideToLeft : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public SlideToLeft()
        {
            ID    = CommandID.SlideToLeft;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            if( MainWindow.Current.IsHorizontalSlide )
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
                mw.StartOperationSlide(false, false);
            }
            else
            {
                mw.StartOperationSlide(true, false);
            }
        }

        public string GetDetail()
        {
            return "左方向にスライド";
        }
    }
}
