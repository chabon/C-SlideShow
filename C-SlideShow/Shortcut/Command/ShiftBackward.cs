﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut.Command
{
    public class ShiftBackward : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; } = 1;
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public ShiftBackward()
        {
            ID    = CommandID.ShiftBackward;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;
            int current = mw.ImgContainerManager.CurrentImageIndex;
            int destIndex = current -= Value;
            int maxIndex = mw.ImgContainerManager.ImagePool.ImageFileContextList.Count - 1;
            if( destIndex < 0 )
            {
                int revParam = Math.Abs(destIndex + 1);
                if( revParam > maxIndex ) revParam = revParam % maxIndex;
                destIndex = maxIndex - revParam;
            }

            var t = mw.ImgContainerManager.ChangeCurrentIndex(destIndex);

            return;
        }

        public string GetDetail()
        {
            return "画像" + Value.ToString() + "枚分ずらし戻す";
        }
    }
}
