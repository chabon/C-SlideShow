﻿using System;
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

            // 全体の不透明度
            OverallOpacity.Value = (int)( pf.OverallOpacity * 100 );
            Text_OverallOpacity.Text = ( (int)OverallOpacity.Value ).ToString();

            // 背景の不透明度
            BackgroundOpacity.Value = (int)( pf.BackgroundOpacity * 100 );
            Text_BackgroundOpacity.Text = ( (int)BackgroundOpacity.Value ).ToString();

            // 背景色
            BaseGridBackgroundColor.PickedColor = pf.BaseGridBackgroundColor;

            // チェック柄の背景にする
            if(pf.UsePlaidBackground) UsePlaidBackground.IsChecked = true;
            else UsePlaidBackground.IsChecked = false;

            // チェック柄の背景のペアとなる色
            PairColorOfPlaidBackground.PickedColor = pf.PairColorOfPlaidBackground;


            // ウインドウ枠の太さ
            ResizeGripThickness.Text = pf.ResizeGripThickness.ToString();

            // ウインドウ枠の色
            ResizeGripColor.PickedColor = pf.ResizeGripColor;
            
            // シークバーの色
            SeekbarColor.PickedColor = pf.SeekbarColor;

            // グリッド線の幅
            TilePadding.Text = pf.TilePadding.ToString();

            // グリッド線の色
            GridLineColor.PickedColor = pf.GridLineColor;


            // 最前面表示
            if(pf.TopMost) TopMost.IsChecked = true;
            else TopMost.IsChecked = false;

            // ファイル読み込み順
            FileReadingOrder.SelectedIndex = (int)pf.FileSortMethod;

            // 起動時、前回のフォルダを開く(未実装)
            if(pf.StartUp_OpenPrevFolder)
                StartUp_OpenPrevFolder.IsChecked = true;
            else
                StartUp_OpenPrevFolder.IsChecked = false;

            // Exifの回転・反転情報を反映させる
            if( pf.ApplyRotateInfoFromExif )
                ApplyRotateInfoFromExif.IsChecked = true;
            else
                ApplyRotateInfoFromExif.IsChecked = false;

            // バックバッファの幅(ピクセル値)
            BitmapDecodeTotalPixel.Text = pf.BitmapDecodeTotalPixel.ToString();


            UpdateDlgShowing();

            isInitializing = false;
        }


        private void UpdateDlgShowing()
        {
            if (Setting.TempProfile.AllowTransparency)
            {
                OverallOpacity.IsEnabled = true;
                BackgroundOpacity.IsEnabled = true;
            }
            else
            {
                OverallOpacity.IsEnabled = false;
                BackgroundOpacity.IsEnabled = false;
            }
        }


        // 透過を有効
        private void AllowTransparency_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)AllowTransparency.IsChecked)
                Setting.TempProfile.AllowTransparency = true;
            else
                Setting.TempProfile.AllowTransparency = false;

            mainWindow.ApplyAllowTransparency();
        }

        // 画像の不透明度
        private void OverallOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInitializing) return;

            Text_OverallOpacity.Text = ( (int)OverallOpacity.Value ).ToString();
            double param = OverallOpacity.Value / 100;
            if (param < 0.005) param = 0.005;
            Setting.TempProfile.OverallOpacity = param;

            mainWindow.ApplyColorAndOpacitySetting();
        }


        // 背景の不透明度
        private void BackgroundOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInitializing) return;

            Text_BackgroundOpacity.Text = ( (int)BackgroundOpacity.Value ).ToString();
            double param = BackgroundOpacity.Value / 100;
            if (param < 0.005) param = 0.005;
            Setting.TempProfile.BackgroundOpacity = param;

            mainWindow.ApplyColorAndOpacitySetting();
        }

        // 背景色
        private void BaseGridBackgroundColor_ColorPicked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            Setting.TempProfile.BaseGridBackgroundColor = BaseGridBackgroundColor.PickedColor;
            mainWindow.ApplyColorAndOpacitySetting();
        }

        // チェック柄の背景にする
        private void UsePlaidBackground_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)UsePlaidBackground.IsChecked)
                Setting.TempProfile.UsePlaidBackground = true;
            else
                Setting.TempProfile.UsePlaidBackground = false;

            mainWindow.ApplyColorAndOpacitySetting();
        }

        // チェック柄の背景のペアとなる色
        private void PairColorOfPlaidBackground_ColorPicked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            Setting.TempProfile.PairColorOfPlaidBackground = PairColorOfPlaidBackground.PickedColor;
            mainWindow.ApplyColorAndOpacitySetting();
        }

        // ウインドウ枠の太さ
        private void ResizeGripThickness_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox editTextBox = TilePadding.Template.FindName("PART_EditableTextBox", ResizeGripThickness) as TextBox;
            if (editTextBox != null)
            {
                editTextBox.TextChanged -= ResizeGripThickness_EditTextBox_TextChanged;
                editTextBox.TextChanged += ResizeGripThickness_EditTextBox_TextChanged;
            }
        }
        private void ResizeGripThickness_EditTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if( isInitializing ) return;

            try
            {
                int val = Int32.Parse(ResizeGripThickness.Text);
                ProfileMemberProp prop = Profile.GetProfileMemberProp( nameof(Profile.ResizeGripThickness) );
                if( val > prop.Max ) val = (int)prop.Max;
                if( val < prop.Min ) val = (int)prop.Min;
                Setting.TempProfile.ResizeGripThickness = val;
                mainWindow.UpdateUI();
            }
            catch { }
        }

        // ウインドウ枠の色
        private void ResizeGripColor_ColorPicked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            Setting.TempProfile.ResizeGripColor = ResizeGripColor.PickedColor;
            mainWindow.UpdateUI();
        }

        // グリッド線の幅
        private void TilePadding_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox editTextBox = TilePadding.Template.FindName("PART_EditableTextBox", TilePadding) as TextBox;
            if (editTextBox != null)
            {
                editTextBox.TextChanged -= TilePadding_EditTextBox_TextChanged;
                editTextBox.TextChanged += TilePadding_EditTextBox_TextChanged;
            }
        }
        private void TilePadding_EditTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInitializing) return;

            try
            {
                int val = Int32.Parse(TilePadding.Text);
                ProfileMemberProp prop = Profile.GetProfileMemberProp( nameof(Profile.TilePadding) );
                if( val > prop.Max ) val = (int)prop.Max;
                if( val < prop.Min ) val = (int)prop.Min;
                Setting.TempProfile.TilePadding = val;
                mainWindow.UpdateGridLine();
            }
            catch { }
        }

        // グリッド線の色
        private void GridLineColor_ColorPicked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            Setting.TempProfile.GridLineColor = GridLineColor.PickedColor;
            mainWindow.UpdateGridLine();
        }

        // シークバーの色
        private void SeekbarColor_ColorPicked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            Setting.TempProfile.SeekbarColor = SeekbarColor.PickedColor;
            mainWindow.UpdateUI();
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

        // ファイル読み込み順序
        private void FileReadingOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) return;

            int idx = FileReadingOrder.SelectedIndex;
            Setting.TempProfile.FileSortMethod = (FileSortMethod)idx;
            mainWindow.SortAllImage(Setting.TempProfile.FileSortMethod);
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

        // Exifの回転・反転情報を反映させる
        private void ApplyRotateInfoFromExif_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)ApplyRotateInfoFromExif.IsChecked)
                Setting.TempProfile.ApplyRotateInfoFromExif = true;
            else
                Setting.TempProfile.ApplyRotateInfoFromExif = false;

            mainWindow.Reload(true);
        }

        // バックバッファの幅(ピクセル値)
        private void BitmapDecodeTotalPixel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) return;

                int val = Int32.Parse(BitmapDecodeTotalPixel.SelectedValue.ToString());

                ProfileMemberProp prop = Profile.GetProfileMemberProp( nameof(Profile.BitmapDecodeTotalPixel) );
                if( val > prop.Max ) val = (int)prop.Max;
                if( val < prop.Min ) val = (int)prop.Min;

                Setting.TempProfile.BitmapDecodeTotalPixel = val;
                mainWindow.Reload(true);
        }
    }
}
