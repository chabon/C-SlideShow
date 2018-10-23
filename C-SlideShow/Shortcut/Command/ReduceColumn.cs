using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 列数を[]減らす
    /// </summary>
    public class ReduceColumn : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public ReduceColumn()
        {
            ID    = CommandID.ReduceColumn;
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

            if( 0 < current[0] - Value && current[0] - Value <= ProfileMember.NumofMatrix.Max )
            {
                pf.NumofMatrix.Value = new int[] { current[0] - Value, current[1] };
                MainWindow.Current.ImgContainerManager.ApplyGridDifinition();
            }

            return;
        }

        public string GetDetail()
        {
            return "列数を" + Value.ToString() + "減らす";
        }
    }
}
