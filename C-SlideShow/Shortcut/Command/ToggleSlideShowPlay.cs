using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using C_SlideShow.CommonControl;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// スライドショー 再生/停止
    /// </summary>
    public class ToggleSlideShowPlay : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; private set; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public ToggleSlideShowPlay()
        {
            ID    = CommandID.ToggleSlideShowPlay;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;
            Profile pf = mw.Setting.TempProfile;


            if( mw.IsPlaying )
            {
                mw.StopSlideShow();

                // 一定時間ごとのスライドだったら、停止がわかりにくいのでメッセージ
                if(pf.SlidePlayMethod.Value == SlidePlayMethod.Interval)
                    mw.NotificationBlock.Show("スライドショー停止", NotificationPriority.Normal, NotificationTime.Short);
            }
            else
            {
                mw.StartSlideShow();

                if(pf.SlidePlayMethod.Value == SlidePlayMethod.Interval)
                mw.NotificationBlock.Show("スライドショー開始 (待機時間" + pf.SlideInterval.Value + "秒)",
                    NotificationPriority.Normal, NotificationTime.Short);
            }


            return;
        }

        public string GetDetail()
        {
            return "スライドショー 再生/停止";
        }
    }
}
