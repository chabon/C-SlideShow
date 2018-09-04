using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// カーソルのある方向の逆方向へスライド
    /// </summary>
    public class SlideToCursorDirectionRev : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public SlideToCursorDirectionRev()
        {
            ID    = CommandID.SlideToCursorDirectionRev;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;

            // ウインドウのスクリーン座標取得(Rect)
            Rect rcWindow = Win32.GetWindowRect(mw);

            // ウインドウの中心座標取得
            Point ptCenter = new Point( rcWindow.Left + (rcWindow.Width / 2), rcWindow.Top + (rcWindow.Height / 2) );

            // カーソルのスクリーン座標取得
            Point ptCursor = Win32.GetCursorPos();

            if( mw.IsHorizontalSlide )
            {
                if( ptCursor.X < ptCenter.X )
                {
                    mw.ShortcutManager.ExecuteCommand(CommandID.SlideToRight, 0, null);
                }
                else
                {
                    mw.ShortcutManager.ExecuteCommand(CommandID.SlideToLeft, 0, null);
                }
            }
            else
            {
                if( ptCursor.Y < ptCenter.Y )
                {
                    mw.ShortcutManager.ExecuteCommand(CommandID.SlideToBottom, 0, null);
                }
                else
                {
                    mw.ShortcutManager.ExecuteCommand(CommandID.SlideToTop, 0, null);
                }
            }

            mw.StartOperationSlide(true, false);
        }

        public string GetDetail()
        {
            return "カーソルのある方向の逆方向へスライド";
        }
    }
}
