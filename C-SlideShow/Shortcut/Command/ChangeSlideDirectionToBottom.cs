using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// スライド方向を下に変更
    /// </summary>
    public class ChangeSlideDirectionToBottom : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ChangeSlideDirectionToBottom()
        {
            ID    = CommandID.ChangeSlideDirectionToBottom;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow.Current.ChangeSlideDirection(SlideDirection.Bottom);
            Message = "スライド方向の変更完了: 下";

            return;
        }

        public string GetDetail()
        {
            return "スライド方向を下に変更";
        }
    }
}
