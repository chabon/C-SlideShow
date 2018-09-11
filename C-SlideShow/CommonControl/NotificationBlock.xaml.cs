using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Threading;

namespace C_SlideShow.CommonControl
{
    public enum NotificationType
    {
        None,
        GesturePreview,
    }

    public enum NotificationPriority
    {
        Lowest,
        Low,
        Normal,
        High,
        Highest
    }

    public enum NotificationTime
    {
        VeryShort     = 1,
        Short         = 2,
        Normal        = 3,
        Long          = 5,
        Eternally     = -1,
    }

    /// <summary>
    /// NotificationBlock.xaml の相互作用ロジック
    /// </summary>
    public partial class NotificationBlock : UserControl
    {
        /* ---------------------------------------------------- */
        //     フィールド
        /* ---------------------------------------------------- */
        private NotificationPriority currentPriority = NotificationPriority.Lowest;
        private DispatcherTimer hideTimer = new DispatcherTimer();
        private int hideCount = 0;
        private NotificationType type;

        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        public new double FontSize
        {
            get
            {
                return this.MessageLabel.FontSize;
            }
            set
            {
                this.MessageLabel.FontSize = value;
            }
        }

        public event EventHandler PreviewShowNotification;



        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */
        public NotificationBlock()
        {
            InitializeComponent();

            hideTimer.Interval = new TimeSpan(0, 0, 1);
            hideTimer.Tick += HideTimer_Tick;
        }


        public void Show(string message, NotificationPriority priority, NotificationTime time, NotificationType type)
        {
            if( currentPriority > priority ) return;
            currentPriority = priority;

            this.MessageLabel.Content = message;
            PreviewShowNotification?.Invoke( this, new EventArgs() );
            this.Visibility = Visibility.Visible;
            this.type = type;

            hideTimer.Stop();
            if( !(time == NotificationTime.Eternally) )
            {
                hideCount = (int)time;
                hideTimer.Start();
            }
        }

        public void Hide()
        {
            currentPriority = NotificationPriority.Lowest;
            this.Visibility = Visibility.Collapsed;
        }

        public void Hide(NotificationType type)
        {
            if(type == this.type ) Hide();
        }

        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        private void HideTimer_Tick(object sender, EventArgs e)
        {
            hideCount -= 1;

            if(hideCount <= 0 )
            {
                hideTimer.Stop();
                Hide();
            }
        }



        // end of class
    }
}
