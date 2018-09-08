using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// フルスクリーンモード ON/OFF
    /// </summary>
    public class ToggleFullScreen : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ToggleFullScreen()
        {
            ID    = CommandID.ToggleFullScreen;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow.Current.ToggleFullScreen();

            return;
        }

        public string GetDetail()
        {
            return "フルスクリーンモード ON/OFF";
        }
    }
}
