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
    /// SettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingDialog : UserControl
    {
        private bool isInitializing = true;

        public AppSetting Setting;
        public MainWindow mainWindow;


        public SettingDialog()
        {
            InitializeComponent();
        }


        public void ApplySettingToDlg()
        {
            isInitializing = true;

            Profile pf = Setting.TempProfile;

            // 背景の透過を有効
            if (pf.AllowTransparency)
                AllowTransparency.IsChecked = true;
            else
                AllowTransparency.IsChecked = false;

            // 背景の不透明度
            BaseGridOpacity.Value = (int)( pf.BaseGridOpacity * 100 );
            Text_BaseGridOpacity.Text = ( (int)BaseGridOpacity.Value ).ToString();

            // アルファチャンネルのみに適用
            if (pf.ApplyOpacityToAlphaChannelOnly)
                ApplyOpacityToAlphaChannelOnly.IsChecked = true;
            else
                ApplyOpacityToAlphaChannelOnly.IsChecked = false;

            // 背景色
            Border_BaseGridBackgroundColor.Background =
                new SolidColorBrush(pf.BaseGridBackgroundColor);

            // ファイル読み込み順
            FileReadingOrder.SelectedIndex = (int)pf.FileReadingOrder;

            // 最前面表示
            if(pf.TopMost)
                TopMost.IsChecked = true;
            else
                TopMost.IsChecked = false;

            // 起動時、前回のフォルダを開く(未実装)
            if(pf.StartUp_OpenPrevFolder)
                StartUp_OpenPrevFolder.IsChecked = true;
            else
                StartUp_OpenPrevFolder.IsChecked = false;
            

            // ウインドウ枠の太さ
            UI_ResizeGripThickness.SelectedIndex = (int)pf.UI_ResizeGripThickness;

            // ウインドウ枠の色
            Border_UI_ResizeGripColor.Background = new SolidColorBrush(pf.UI_ResizeGripColor);
            
            // シークバーの色
            Border_UI_SeekbarColor.Background = new SolidColorBrush(pf.UI_SeekbarColor);


            UpdateDlgShowing();

            isInitializing = false;
        }


        private void UpdateDlgShowing()
        {
            if (Setting.TempProfile.AllowTransparency)
            {
                BaseGridOpacity.IsEnabled = true;
                ApplyOpacityToAlphaChannelOnly.IsEnabled = true;
            }
            else
            {
                BaseGridOpacity.IsEnabled = false;
                ApplyOpacityToAlphaChannelOnly.IsEnabled = false;
            }
        }


        // 背景の透過を有効
        private void AllowTransparency_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)AllowTransparency.IsChecked)
                Setting.TempProfile.AllowTransparency = true;
            else
                Setting.TempProfile.AllowTransparency = false;

            mainWindow.ApplyAllowTransparency();
        }

        // 背景の不透明度
        private void BaseGridOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInitializing) return;

            Text_BaseGridOpacity.Text = ( (int)BaseGridOpacity.Value ).ToString();
            double param = BaseGridOpacity.Value / 100;
            if (param < 0.005) param = 0.005;
            Setting.TempProfile.BaseGridOpacity = param;

            mainWindow.ApplyColorAndOpacitySetting();
        }

        // アルファチャンネルのみに適用
        private void ApplyOpacityToAlphaChannelOnly_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)ApplyOpacityToAlphaChannelOnly.IsChecked)
                Setting.TempProfile.ApplyOpacityToAlphaChannelOnly = true;
            else
                Setting.TempProfile.ApplyOpacityToAlphaChannelOnly = false;

            mainWindow.ApplyColorAndOpacitySetting();
        }

        // 背景色
        private void BaseGridBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color color = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
                Border_BaseGridBackgroundColor.Background = new SolidColorBrush(color);

                Setting.TempProfile.BaseGridBackgroundColor = color;
                mainWindow.ApplyColorAndOpacitySetting();
            }

        }

        // ファイル読み込み順序
        private void FileReadingOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) return;

            int idx = FileReadingOrder.SelectedIndex;
            Setting.TempProfile.FileReadingOrder = (FileReadingOrder)idx;
            mainWindow.SortAllImage(Setting.TempProfile.FileReadingOrder);
        }

        // 最前面表示
        private void TopMost_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)TopMost.IsChecked)
                Setting.TempProfile.TopMost = true;
            else
                Setting.TempProfile.TopMost = false;

            mainWindow.Topmost = Setting.TempProfile.TopMost;
        }

        // 起動時、前回のフォルダを開く(未実装)
        private void StartUp_OpenPrevFolder_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)StartUp_OpenPrevFolder.IsChecked)
                Setting.TempProfile.StartUp_OpenPrevFolder = true;
            else
                Setting.TempProfile.StartUp_OpenPrevFolder = false;
        }



        // ウインドウ枠の太さ
        private void UI_ResizeGripThickness_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) return;

            int idx = UI_ResizeGripThickness.SelectedIndex;
            Setting.TempProfile.UI_ResizeGripThickness = idx;
            mainWindow.UpdateUI();
        }

        // ウインドウ枠の色
        private void UI_ResizeGripColor_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color color = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
                Border_UI_ResizeGripColor.Background = new SolidColorBrush(color);

                Setting.TempProfile.UI_ResizeGripColor = color;
                mainWindow.UpdateUI();
            }
        }

        // シークバーの色
        private void UI_SeekbarColor_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color color = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
                Border_UI_SeekbarColor.Background = new SolidColorBrush(color);

                Setting.TempProfile.UI_SeekbarColor = color;
                mainWindow.UpdateUI();
            }
        }



    }
}
