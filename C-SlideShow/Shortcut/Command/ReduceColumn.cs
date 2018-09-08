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
            var current = MainWindow.Current.Setting.TempProfile.NumofMatrix.Value;
            if( current == null || current.Length < 2 ) return;

            if( 0 < current[0] - Value && current[0] - Value <= ProfileMember.NumofMatrix.Max )
            {
                MainWindow.Current.ChangeGridDifinition(current[0] - Value, current[1]);
            }

            return;
        }

        public string GetDetail()
        {
            return "列数を" + Value.ToString() + "減らす";
        }
    }
}
