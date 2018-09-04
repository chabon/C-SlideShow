using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;


namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// カーソル下の画像を外部プログラム名「[ ]」で開く
    /// </summary>
    public class OpenImageUnderCursorByExternalApp : ICommand
    {
        private MainWindow mw;

        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; } = "";
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = true;

        public OpenImageUnderCursorByExternalApp()
        {
            ID    = CommandID.OpenImageUnderCursorByExternalApp;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            mw = MainWindow.Current;
            Tile targetTile;
            if( mw.TileExpantionPanel.IsShowing )
            {
                targetTile = mw.TileExpantionPanel.TargetTile;
            }
            else
            {
                targetTile = mw.GetTileUnderCursor();
            }

            if(targetTile != null)
            {
                ExternalAppInfo exAppInfo = mw.Setting.ExternalAppInfoList.FirstOrDefault(i => i.Name == this.StrValue);
                if(exAppInfo == null )
                {
                    // 実行ファイル名を対象に検索
                    exAppInfo = mw.Setting.ExternalAppInfoList.FirstOrDefault(i => System.IO.Path.GetFileName(i.Path) == this.StrValue);
                }
                if(exAppInfo == null )
                {
                    // 実行ファイル名(拡張子抜き)を対象に検索
                    exAppInfo = mw.Setting.ExternalAppInfoList.FirstOrDefault(i => System.IO.Path.GetFileNameWithoutExtension(i.Path) == this.StrValue);
                }

                if(exAppInfo != null )
                {
                    targetTile.OpenByExternalApp(exAppInfo);
                }
            }
        }

        public string GetDetail()
        {
            string appName = this.StrValue;
            if(appName == "")
            {
                appName = "未指定";
            }

            return "カーソル下の画像を外部プログラム名「" + appName + "」で開く";
        }
    }
}
