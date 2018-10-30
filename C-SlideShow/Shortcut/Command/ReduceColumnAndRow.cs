using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    public class ReduceColumnAndRow : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; } = 1;
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public ReduceColumnAndRow()
        {
            ID    = CommandID.ReduceColumnAndRow;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            Profile pf = MainWindow.Current.Setting.TempProfile;

            var current = pf.NumofMatrix.Value;
            if( current == null || current.Length < 2 ) return;

            if( 0 < current[0] - Value && current[0] - Value <= ProfileMember.NumofMatrix.Max &&
                0 < current[1] - Value && current[1] - Value <= ProfileMember.NumofMatrix.Max)
            {
                pf.NumofMatrix.Value = new int[] { current[0] - Value, current[1] - Value };
                MainWindow.Current.ImgContainerManager.ApplyGridDifinition();
            }

            return;
        }

        public string GetDetail()
        {
            return "列数と行数を" + Value.ToString() + "ずつ減らす";
        }
    }
}
