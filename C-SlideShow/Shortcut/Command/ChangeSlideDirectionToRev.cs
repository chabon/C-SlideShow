using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// スライド方向を逆方向に変更
    /// </summary>
    public class ChangeSlideDirectionToRev : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ChangeSlideDirectionToRev()
        {
            ID    = CommandID.ChangeSlideDirectionToRev;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            string dir;
            switch( MainWindow.Current.Setting.TempProfile.SlideDirection.Value )
            {
                case SlideDirection.Left:
                    MainWindow.Current.ChangeSlideDirection(SlideDirection.Right);
                    dir = "右";
                    break;
                case SlideDirection.Top:
                    MainWindow.Current.ChangeSlideDirection(SlideDirection.Bottom);
                    dir = "下";
                    break;
                case SlideDirection.Right:
                    MainWindow.Current.ChangeSlideDirection(SlideDirection.Left);
                    dir = "左";
                    break;
                case SlideDirection.Bottom:
                    MainWindow.Current.ChangeSlideDirection(SlideDirection.Top);
                    dir = "上";
                    break;
                default:
                    dir = "";
                    break;
            }


            Message = "スライド方向の変更完了: " + dir;

            return;
        }

        public string GetDetail()
        {
            return "スライド方向を逆方向に変更";
        }
    }
}
