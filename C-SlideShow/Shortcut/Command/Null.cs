using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// 空コマンド
    /// </summary>
    public class Null : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public Null()
        {
            ID    = CommandID.None;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {

        }

        public string GetDetail()
        {
            return "処理なし";
        }
    }
}
