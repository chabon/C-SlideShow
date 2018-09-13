﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace C_SlideShow.Shortcut.Command
{
    /// <summary>
    /// メニューの表示： 読み込み
    /// </summary>
    public class OepnSubMenu_Load : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        public OepnSubMenu_Load()
        {
            ID    = CommandID.OepnSubMenu_Load;
            Scene = Scene.Nomal;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(30) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();

                MainWindow.Current.ToolbarWrapper.Visibility = Visibility.Visible;
                MainWindow.Current.MenuItem_Load.IsSubmenuOpen = true;
                Keyboard.Focus(MainWindow.Current.MenuItem_Load);
            };
            timer.Start();

            return;
        }

        public string GetDetail()
        {
            return "メニューの表示： 読み込み";
        }
    }
}
