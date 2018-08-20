using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// ウインドウサイズを小さく
    /// </summary>
    public class WindowSizeDown : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public WindowSizeDown()
        {
            ID    = CommandID.WindowSizeDown;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;

            if( mw.Setting.TempProfile.IsFullScreenMode.Value ) return;

            mw.Width = mw.Width * 0.9;
            mw.Height = mw.Height * 0.9;
            mw.UpdateWindowSize();
        }

        public string GetDetail()
        {
            return "ウインドウサイズを小さく";
        }
    }
}
