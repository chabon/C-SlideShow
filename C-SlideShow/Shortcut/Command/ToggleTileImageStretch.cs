using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// グリッド枠内への画像の収め方を切り替え
    /// </summary>
    public class ToggleTileImageStretch : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ToggleTileImageStretch()
        {
            ID    = CommandID.ToggleTileImageStretch;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            Profile pf = MainWindow.Current.Setting.TempProfile;

            Message = "グリッド枠内への画像の収め方を変更: ";

            switch( pf.TileImageStretch.Value )
            {
                case TileImageStretch.Uniform:
                    pf.TileImageStretch.Value = TileImageStretch.UniformToFill;
                    Message += "枠内全体が埋まるように配置";
                    break;
                case TileImageStretch.UniformToFill:
                    pf.TileImageStretch.Value = TileImageStretch.Fill;
                    Message += "枠内全体に引き伸ばす";
                    break;
                case TileImageStretch.Fill:
                    pf.TileImageStretch.Value = TileImageStretch.Uniform;
                    Message += "枠内に収める";
                    break;
            }

            MainWindow mw = MainWindow.Current;
            var t = mw.ImgContainerManager.InitAllContainer(mw.ImgContainerManager.CurrentImageIndex);

            return;
        }

        public string GetDetail()
        {
            return "グリッド枠内への画像の収め方を切り替え";
        }
    }
}
