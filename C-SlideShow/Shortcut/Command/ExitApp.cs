using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// アプリケーションを終了
    /// </summary>
    public class ExitApp : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ExitApp()
        {
            ID    = CommandID.ExitApp;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {

            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(10) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                MainWindow.Current.Close();
            };
            timer.Start();

            return;
        }

        public string GetDetail()
        {
            return "アプリケーションを終了";
        }
    }
}
