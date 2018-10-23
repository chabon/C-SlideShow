using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 行数を[]減らす
    /// </summary>
    public class ReduceRow : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public ReduceRow()
        {
            ID    = CommandID.ReduceRow;
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

            if( 0 < current[1] - Value && current[1] - Value <= ProfileMember.NumofMatrix.Max )
            {
                pf.NumofMatrix.Value = new int[] { current[0], current[1] - Value };
                MainWindow.Current.ImgContainerManager.ApplyGridDifinition();
            }

            return;
        }

        public string GetDetail()
        {
            return "行数を" + Value.ToString() + "減らす";
        }
    }
}
