using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// ウインドウサイズを大きく
    /// </summary>
    public class WindowSizeUp : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public WindowSizeUp()
        {
            ID    = CommandID.WindowSizeUp;
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

            mw.Width = mw.Width * 1.1;
            mw.Height = mw.Height * 1.1;
            mw.UpdateWindowSize();
        }

        public string GetDetail()
        {
            return "ウインドウサイズを大きく";
        }
    }
}
