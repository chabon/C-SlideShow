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

namespace C_SlideShow
{
    /// <summary>
    /// SlideSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class SlideSettingDialog : UserControl
    {
        private bool isInitializing = true;

        public AppSetting Setting;
        public MainWindow mainWindow;

        public SlideSettingDialog()
        {
            InitializeComponent();
        }


        public void ApplySettingToDlg()
        {
            isInitializing = true;

            Profile pf = Setting.TempProfile;

            // スライド方法
            if(pf.SlidePlayMethod.Value == SlidePlayMethod.Continuous)
            {
                SlidePlayMethod_Continuous.IsChecked = true;
                SlidePlayMethod_Interval.IsChecked = false;
            }
            else
            {
                SlidePlayMethod_Continuous.IsChecked = false;
                SlidePlayMethod_Interval.IsChecked = true;
            }

            // 常にスライド
            SlideSpeed.Value = pf.SlideSpeed.Value;
            Text_SlideSpeed.Text = pf.SlideSpeed.Value.ToString();

            // 一定時間待機してスライド
            SlideInterval.Text = pf.SlideInterval.Value.ToString();
            SlideTimeInIntevalMethod.Text = pf.SlideTimeInIntevalMethod.Value.ToString();
            if (pf.SlideByOneImage.Value) SlideByOneImage.IsChecked = true;
            else SlideByOneImage.IsChecked = false;

            // スライド方向
            SlideDirection_Left.IsChecked = false;
            SlideDirection_Top.IsChecked = false;
            SlideDirection_Right.IsChecked = false;
            SlideDirection_Bottom.IsChecked = false;
            switch (pf.SlideDirection.Value)
            {
                case SlideDirection.Left:
                    SlideDirection_Left.IsChecked = true;
                    break;
                case SlideDirection.Top:
                    SlideDirection_Top.IsChecked = true;
                    break;
                case SlideDirection.Right:
                    SlideDirection_Right.IsChecked = true;
                    break;
                case SlideDirection.Bottom:
                    SlideDirection_Bottom.IsChecked = true;
                    break;
            }

            // 表示する項目を更新
            UpdateDlgShowing();

            isInitializing = false;
        }

        private void UpdateDlgShowing()
        {
            if (Setting.TempProfile.SlidePlayMethod.Value == SlidePlayMethod.Continuous)
            {
                ContinuousSlideSettingWrapper.Visibility = Visibility.Visible;
                ContinuousSlideSettingWrapper.Height = Double.NaN;
                IntervalSlideSettingWrapper.Visibility = Visibility.Hidden;
                IntervalSlideSettingWrapper.Height = 0;
            }
            else
            {
                ContinuousSlideSettingWrapper.Visibility = Visibility.Hidden;
                ContinuousSlideSettingWrapper.Height = 0;
                IntervalSlideSettingWrapper.Visibility = Visibility.Visible;
                IntervalSlideSettingWrapper.Height = Double.NaN;
            }
        }


        // スライド方法
        private void SlidePlayMethod_Click(object sender, RoutedEventArgs e)
        {
            bool b = mainWindow.IsPlaying;

            if ((bool)SlidePlayMethod_Continuous.IsChecked)
            {
                Setting.TempProfile.SlidePlayMethod.Value = SlidePlayMethod.Continuous;
            }
            else
            {
                Setting.TempProfile.SlidePlayMethod.Value = SlidePlayMethod.Interval;
            }

            mainWindow.StopSlideShow();
            mainWindow.UpdateToolbarViewing();
            UpdateDlgShowing();

            if( b ) mainWindow.StartSlideShow(); // 方法を変えても、スライドしてたら継続
        }

        // 速度
        private void SlideSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInitializing) return;

            Text_SlideSpeed.Text = ( (int)SlideSpeed.Value ).ToString();
            Setting.TempProfile.SlideSpeed.Value = (int)SlideSpeed.Value;

            mainWindow.UpdateSlideSpeed();
        }

        // 待機時間
        private void SlideInterval_TextChanged(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            int val;
            int.TryParse( SlideInterval.Text, out val);
            if (val < ProfileMember.SlideInterval.Min || val > ProfileMember.SlideInterval.Max) val = 5;
            Setting.TempProfile.SlideInterval.Value = val;
            mainWindow.UpdateIntervalSlideTimer();
        }

        // スライド時間
        private void SlideTimeInIntevalMethod_TextChanged(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            int val;
            int.TryParse( SlideTimeInIntevalMethod.Text, out val);
            if (val < ProfileMember.SlideTimeInIntevalMethod.Min) val = ProfileMember.SlideTimeInIntevalMethod.Min;
            else if (val > ProfileMember.SlideTimeInIntevalMethod.Max) val = ProfileMember.SlideTimeInIntevalMethod.Max;
            Setting.TempProfile.SlideTimeInIntevalMethod.Value = val;
        }

        // 画像一枚ずつ移動させる
        private void SlideByOneImage_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)SlideByOneImage.IsChecked)
                Setting.TempProfile.SlideByOneImage.Value = true;
            else
                Setting.TempProfile.SlideByOneImage.Value = false;
        }

        // スライド方向
        private void SlideDirection_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)SlideDirection_Left.IsChecked)
                mainWindow.ChangeSlideDirection(SlideDirection.Left);
            else if((bool)SlideDirection_Top.IsChecked)
                mainWindow.ChangeSlideDirection(SlideDirection.Top);
            else if((bool)SlideDirection_Right.IsChecked)
                mainWindow.ChangeSlideDirection(SlideDirection.Right);
            else if((bool)SlideDirection_Bottom.IsChecked)
                mainWindow.ChangeSlideDirection(SlideDirection.Bottom);
        }
    }
}
