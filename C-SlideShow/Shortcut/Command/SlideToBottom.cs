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

        public SlideToBottom()
        {
            ID    = CommandID.SlideToBottom;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;
            if( !mw.IsHorizontalSlide )
            {
                if(mw.Setting.TempProfile.SlideDirection.Value == SlideDirection.Top )
                {
                    mw.StartOperationSlide(true, false);
                }
                else
                {
                    mw.StartOperationSlide(false, false);
                }

            }
        }

        public string GetDetail()
        {
            return "下方向にスライド";
        }
    }
}
