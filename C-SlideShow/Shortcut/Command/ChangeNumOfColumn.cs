using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 列数を[]にする
    /// </summary>
    public class ChangeNumOfColumn : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; } = 2;
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public ChangeNumOfColumn()
        {
            ID    = CommandID.ChangeNumOfColumn;
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

            int num;
            if( Value < 1 ) num = 1;
            else if( Value > ProfileMember.NumofMatrix.Max ) num = ProfileMember.NumofMatrix.Max;
            else num = Value;

            MainWindow.Current.ChangeGridDifinition(num, current[1]);

            return;
        }

        public string GetDetail()
        {
            int num;
            if( Value < 1 ) num = 1;
            else if( Value > ProfileMember.NumofMatrix.Max ) num = ProfileMember.NumofMatrix.Max;
            else num = Value;

            return "列数を" + num.ToString() + "に変更";
        }
    }
}
