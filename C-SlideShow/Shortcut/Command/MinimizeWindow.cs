using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    public class MinimizeWindow : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public MinimizeWindow()
        {
            ID    = CommandID.MinimizeWindow;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow.Current.WindowState = System.Windows.WindowState.Minimized;
            return;
        }

        public string GetDetail()
        {
            return "ウインドウを最小化";
        }
    }
}
