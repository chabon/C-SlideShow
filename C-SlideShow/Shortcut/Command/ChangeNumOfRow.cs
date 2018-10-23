using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 行数を[]にする
    /// </summary>
    public class ChangeNumOfRow : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; } = 2;
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public ChangeNumOfRow()
        {
            ID    = CommandID.ChangeNumOfRow;
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

            int num;
            if( Value < 1 ) num = 1;
            else if( Value > ProfileMember.NumofMatrix.Max ) num = ProfileMember.NumofMatrix.Max;
            else num = Value;

            pf.NumofMatrix.Value = new int[] { current[0], num };
            MainWindow.Current.ImgContainerManager.ApplyGridDifinition();

            return;
        }

        public string GetDetail()
        {
            int num;
            if( Value < 1 ) num = 1;
            else if( Value > ProfileMember.NumofMatrix.Max ) num = ProfileMember.NumofMatrix.Max;
            else num = Value;

            return "行数を" + num.ToString() + "に変更";
        }
    }
}
