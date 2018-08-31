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

        public int       Value           { get; set; } = 10;
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;


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

            double param = Value / 100.0;
            if( param < 0 ) param = 0;
            else if( param > 10 ) param = 10;

            mw.Width = mw.Width * (1.0 + param);
            mw.Height = mw.Height * (1.0 + param);
            mw.UpdateWindowSize();
        }

        public string GetDetail()
        {
            return "ウインドウサイズを" + Value.ToString() + "%大きく";
        }
    }
}
