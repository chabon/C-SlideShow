using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 常に最前面表示 ON/OFF
    /// </summary>
    public class ToggleTopMost : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ToggleTopMost()
        {
            ID    = CommandID.ToggleTopMost;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            bool current = MainWindow.Current.Topmost;
            MainWindow.Current.Topmost = !current;

            Message = "常に最前面表示: " + (!current ? "ON" : "OFF");
            return;
        }

        public string GetDetail()
        {
            return "常に最前面表示 ON/OFF";
        }
    }
}
