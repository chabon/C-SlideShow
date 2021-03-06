﻿using System;
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

            // 再生中ならストップ
            if( mw.ImgContainerManager.SlideShowState != Core.SlideShowState.Stop )
            {
                mw.ImgContainerManager.StopSlideShow(true);
            }

            // 停止中なら再生
            else
            {
                mw.ImgContainerManager.StartSlideShow(true);
            }

            return;
        }

        public string GetDetail()
        {
            return "スライドショー 再生/停止";
        }
    }
}
