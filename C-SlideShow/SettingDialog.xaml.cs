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
            if (pf.AllowTransparency.Value)
                AllowTransparency.IsChecked = true;
            else
                AllowTransparency.IsChecked = false;

            // 全体の不透明度
            OverallOpacity.Value = (int)( pf.OverallOpacity.Value * 100 );
            Text_OverallOpacity.Text = ( (int)OverallOpacity.Value ).ToString();

            // 背景の不透明度
            BackgroundOpacity.Value = (int)( pf.BackgroundOpacity.Value * 100 );
            Text_BackgroundOpacity.Text = ( (int)BackgroundOpacity.Value ).ToString();

            // 背景色
            BaseGridBackgroundColor.PickedColor = pf.BaseGridBackgroundColor.Value;

            // チェック柄の背景にする
            if(pf.UsePlaidBackground.Value) UsePlaidBackground.IsChecked = true;
            else UsePlaidBackground.IsChecked = false;

            // チェック柄の背景のペアとなる色
            PairColorOfPlaidBackground.PickedColor = pf.PairColorOfPlaidBackground.Value;


            // ウインドウ枠の太さ
            ResizeGripThickness.Text = pf.ResizeGripThickness.Value.ToString();

            // ウインドウ枠の色
            ResizeGripColor.PickedColor = pf.ResizeGripColor.Value;
            
            // シークバーの色
            SeekbarColor.PickedColor = pf.SeekbarColor.Value;

            // グリッド線の幅
            TilePadding.Text = pf.TilePadding.Value.ToString();

            // グリッド線の色
            GridLineColor.PickedColor = pf.GridLineColor.Value;


            // 最前面表示
            if(pf.TopMost.Value) TopMost.IsChecked = true;
            else TopMost.IsChecked = false;

            // ファイル読み込み順
            FileReadingOrder.SelectedIndex = (int)pf.FileSortMethod.Value;

            // 起動時、前回のフォルダを開く(未実装)
            if(pf.OpenPrevFolderOnStartUp.Value)
                StartUp_OpenPrevFolder.IsChecked = true;
            else
                StartUp_OpenPrevFolder.IsChecked = false;

            // Exifの回転・反転情報を反映させる
            if( pf.ApplyRotateInfoFromExif.Value )
                ApplyRotateInfoFromExif.IsChecked = true;
            else
                ApplyRotateInfoFromExif.IsChecked = false;

            // バックバッファの幅(ピクセル値)
            BitmapDecodeTotalPixel.Text = pf.BitmapDecodeTotalPixel.Value.ToString();

            // グリッドへの画像の配置方法
            if( pf.UseDefaultTileOrigin.Value )
            {
                UseDefaultTileOrigin.SelectedIndex = 0;
                TileOrigin.IsEnabled = false;
                TileOrientation.IsEnabled = false;

                SetTileArrangeSettingBySlideDirection();
            }
            else
            {
                UseDefaultTileOrigin.SelectedIndex = 1;
                TileOrigin.IsEnabled = true;
                TileOrientation.IsEnabled = true;
            }

            // 配置する起点
            TileOrigin.SelectedIndex = (int)pf.TileOrigin.Value;

            // 配置方向
            TileOrientation.SelectedIndex = (int)pf.TileOrientation.Value;

            // 配置プレビュー
            UpdateTileArrangePreview();


            UpdateDlgShowing();

            isInitializing = false;
        }


        private void UpdateDlgShowing()
        {
            if (Setting.TempProfile.AllowTransparency.Value)
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
                Setting.TempProfile.AllowTransparency.Value = true;
            else
                Setting.TempProfile.AllowTransparency.Value = false;

            mainWindow.ApplyAllowTransparency();
        }

        // 画像の不透明度
        private void OverallOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInitializing) return;

            Text_OverallOpacity.Text = ( (int)OverallOpacity.Value ).ToString();
            double param = OverallOpacity.Value / 100;
            if (param < ProfileMember.OverallOpacity.Min) param = ProfileMember.OverallOpacity.Min;
            Setting.TempProfile.OverallOpacity.Value = param;

            mainWindow.ApplyColorAndOpacitySetting();
        }


        // 背景の不透明度
        private void BackgroundOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInitializing) return;

            Text_BackgroundOpacity.Text = ( (int)BackgroundOpacity.Value ).ToString();
            double param = BackgroundOpacity.Value / 100;
            if (param < ProfileMember.BackgroundOpacity.Min) param = ProfileMember.BackgroundOpacity.Min;
            Setting.TempProfile.BackgroundOpacity.Value = param;

            mainWindow.ApplyColorAndOpacitySetting();
        }

        // 背景色
        private void BaseGridBackgroundColor_ColorPicked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            Setting.TempProfile.BaseGridBackgroundColor.Value = BaseGridBackgroundColor.PickedColor;
            mainWindow.ApplyColorAndOpacitySetting();
        }

        // チェック柄の背景にする
        private void UsePlaidBackground_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)UsePlaidBackground.IsChecked)
                Setting.TempProfile.UsePlaidBackground.Value = true;
            else
                Setting.TempProfile.UsePlaidBackground.Value = false;

            mainWindow.ApplyColorAndOpacitySetting();
        }

        // チェック柄の背景のペアとなる色
        private void PairColorOfPlaidBackground_ColorPicked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            Setting.TempProfile.PairColorOfPlaidBackground.Value = PairColorOfPlaidBackground.PickedColor;
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
                if( val > ProfileMember.ResizeGripThickness.Max ) val = (int)ProfileMember.ResizeGripThickness.Max;
                if( val < ProfileMember.ResizeGripThickness.Min ) val = (int)ProfileMember.ResizeGripThickness.Min;
                Setting.TempProfile.ResizeGripThickness.Value = val;
                mainWindow.UpdateUI();
            }
            catch { }
        }

        // ウインドウ枠の色
        private void ResizeGripColor_ColorPicked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            Setting.TempProfile.ResizeGripColor.Value = ResizeGripColor.PickedColor;
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
                if( val > ProfileMember.TilePadding.Max ) val = (int)ProfileMember.TilePadding.Max;
                if( val < ProfileMember.TilePadding.Min ) val = (int)ProfileMember.TilePadding.Min;
                Setting.TempProfile.TilePadding.Value = val;
                mainWindow.UpdateGridLine();
            }
            catch { }
        }

        // グリッド線の色
        private void GridLineColor_ColorPicked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            Setting.TempProfile.GridLineColor.Value = GridLineColor.PickedColor;
            mainWindow.UpdateGridLine();
        }

        // シークバーの色
        private void SeekbarColor_ColorPicked(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            Setting.TempProfile.SeekbarColor.Value = SeekbarColor.PickedColor;
            mainWindow.UpdateUI();
        }

        // 最前面表示
        private void TopMost_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)TopMost.IsChecked)
                Setting.TempProfile.TopMost.Value = true;
            else
                Setting.TempProfile.TopMost.Value = false;

            mainWindow.Topmost = Setting.TempProfile.TopMost.Value;
        }

        // ファイル読み込み順序
        private void FileReadingOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) return;

            int idx = FileReadingOrder.SelectedIndex;
            Setting.TempProfile.FileSortMethod.Value = (FileSortMethod)idx;
            mainWindow.SortAllImage(Setting.TempProfile.FileSortMethod.Value);
        }

        // 起動時、前回のフォルダを開く(未実装)
        private void StartUp_OpenPrevFolder_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)StartUp_OpenPrevFolder.IsChecked)
                Setting.TempProfile.OpenPrevFolderOnStartUp.Value = true;
            else
                Setting.TempProfile.OpenPrevFolderOnStartUp.Value = false;
        }

        // Exifの回転・反転情報を反映させる
        private void ApplyRotateInfoFromExif_Click(object sender, RoutedEventArgs e)
        {
            if (isInitializing) return;

            if ((bool)ApplyRotateInfoFromExif.IsChecked)
                Setting.TempProfile.ApplyRotateInfoFromExif.Value = true;
            else
                Setting.TempProfile.ApplyRotateInfoFromExif.Value = false;

            mainWindow.Reload(true);
        }

        // バックバッファの幅(ピクセル値)
        private void BitmapDecodeTotalPixel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializing) return;

                int val = Int32.Parse(BitmapDecodeTotalPixel.SelectedValue.ToString());

                if( val > ProfileMember.BitmapDecodeTotalPixel.Max ) val = (int)ProfileMember.BitmapDecodeTotalPixel.Max;
                if( val < ProfileMember.BitmapDecodeTotalPixel.Min ) val = (int)ProfileMember.BitmapDecodeTotalPixel.Min;

                Setting.TempProfile.BitmapDecodeTotalPixel.Value = val;
                mainWindow.Reload(true);
        }

        // アプリの設定
        private void AppSettingButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.ShowAppSettingDialog(Setting.AppSettingDialogTabIndex);
        }


        // 配置設定 グリッドへの画像の配置方法
        private void UseDefaultTileOrigin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;

            Profile pf = Setting.TempProfile;

            if(UseDefaultTileOrigin.SelectedIndex == 0 )
            {
                pf.UseDefaultTileOrigin.Value = true;
                TileOrigin.IsEnabled = false;
                TileOrientation.IsEnabled = false;

                SetTileArrangeSettingBySlideDirection();
                mainWindow.UpdateTileArrange();
            }
            else
            {
                pf.UseDefaultTileOrigin.Value = false;
                TileOrigin.IsEnabled = true;
                TileOrientation.IsEnabled = true;
            }

            UpdateTileArrangePreview();
        }

        // 配置設定 配置する起点
        private void TileOrigin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;

            Setting.TempProfile.TileOrigin.Value = (TileOrigin)TileOrigin.SelectedIndex;
            UpdateTileArrangePreview();
            mainWindow.UpdateTileArrange();
        }

        // 配置設定 配置方向
        private void TileOrientation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;

            Setting.TempProfile.TileOrientation.Value = (TileOrientation)TileOrientation.SelectedIndex;
            UpdateTileArrangePreview();
            mainWindow.UpdateTileArrange();
        }

        // スライド方向により、配置設定を決める
        private void SetTileArrangeSettingBySlideDirection()
        {
            isInitializing = true;

            Profile pf = Setting.TempProfile;

            switch( pf.SlideDirection.Value )
            {
                case SlideDirection.Left:
                    TileOrigin.SelectedIndex = 0;
                    TileOrientation.SelectedIndex = 1;
                    pf.TileOrigin.Value = C_SlideShow.TileOrigin.TopLeft;
                    pf.TileOrientation.Value = C_SlideShow.TileOrientation.Vertical;
                    break;
                case SlideDirection.Top:
                    TileOrigin.SelectedIndex = 0;
                    TileOrientation.SelectedIndex = 0;
                    pf.TileOrigin.Value = C_SlideShow.TileOrigin.TopLeft;
                    pf.TileOrientation.Value = C_SlideShow.TileOrientation.Horizontal;
                    break;
                case SlideDirection.Right:
                    TileOrigin.SelectedIndex = 1;
                    TileOrientation.SelectedIndex = 1;
                    pf.TileOrigin.Value = C_SlideShow.TileOrigin.TopRight;
                    pf.TileOrientation.Value = C_SlideShow.TileOrientation.Vertical;
                    break;
                case SlideDirection.Bottom:
                    TileOrigin.SelectedIndex = 2;
                    TileOrientation.SelectedIndex = 0;
                    pf.TileOrigin.Value = C_SlideShow.TileOrigin.BottomRight;
                    pf.TileOrientation.Value = C_SlideShow.TileOrientation.Horizontal;
                    break;
            }
            isInitializing = false;
        }

        // 配置設定プレビュー
        private void UpdateTileArrangePreview()
        {
            Profile pf = Setting.TempProfile;

            if( pf.UseDefaultTileOrigin.Value )
            {
                switch( pf.SlideDirection.Value )
                {
                    case SlideDirection.Left:
                        TileArrangePreview_TopLeft.Content     = "1";
                        TileArrangePreview_TopRight.Content    = "3";
                        TileArrangePreview_BottomRight.Content = "4";
                        TileArrangePreview_BottomLeft.Content  = "2";
                        break;
                    case SlideDirection.Top:
                        TileArrangePreview_TopLeft.Content     = "1";
                        TileArrangePreview_TopRight.Content    = "2";
                        TileArrangePreview_BottomRight.Content = "4";
                        TileArrangePreview_BottomLeft.Content  = "3";
                        break;
                    case SlideDirection.Right:
                        TileArrangePreview_TopLeft.Content     = "3";
                        TileArrangePreview_TopRight.Content    = "1";
                        TileArrangePreview_BottomRight.Content = "2";
                        TileArrangePreview_BottomLeft.Content  = "4";
                        break;
                    case SlideDirection.Bottom:
                        TileArrangePreview_TopLeft.Content     = "4";
                        TileArrangePreview_TopRight.Content    = "3";
                        TileArrangePreview_BottomRight.Content = "1";
                        TileArrangePreview_BottomLeft.Content  = "2";
                        break;
                }
            }
            else
            {
                switch( pf.TileOrigin.Value )
                {
                    case C_SlideShow.TileOrigin.TopLeft:
                        TileArrangePreview_TopLeft.Content     = "1";
                        if(pf.TileOrientation.Value == C_SlideShow.TileOrientation.Horizontal )
                        {
                            TileArrangePreview_TopRight.Content    = "2";
                            TileArrangePreview_BottomRight.Content = "4";
                            TileArrangePreview_BottomLeft.Content  = "3";
                        }
                        else
                        {
                            TileArrangePreview_TopRight.Content    = "3";
                            TileArrangePreview_BottomRight.Content = "4";
                            TileArrangePreview_BottomLeft.Content  = "2";
                        }
                        break;
                    case C_SlideShow.TileOrigin.TopRight:
                        TileArrangePreview_TopRight.Content    = "1";
                        if(pf.TileOrientation.Value == C_SlideShow.TileOrientation.Horizontal )
                        {
                            TileArrangePreview_TopLeft.Content     = "2";
                            TileArrangePreview_BottomRight.Content = "3";
                            TileArrangePreview_BottomLeft.Content  = "4";
                        }
                        else
                        {
                            TileArrangePreview_TopLeft.Content     = "3";
                            TileArrangePreview_BottomRight.Content = "2";
                            TileArrangePreview_BottomLeft.Content  = "4";
                        }
                        break;
                    case C_SlideShow.TileOrigin.BottomRight:
                        TileArrangePreview_BottomRight.Content = "1";
                        if(pf.TileOrientation.Value == C_SlideShow.TileOrientation.Horizontal )
                        {
                            TileArrangePreview_TopLeft.Content     = "4";
                            TileArrangePreview_TopRight.Content    = "3";
                            TileArrangePreview_BottomLeft.Content  = "2";
                        }
                        else
                        {
                            TileArrangePreview_TopLeft.Content     = "4";
                            TileArrangePreview_TopRight.Content    = "2";
                            TileArrangePreview_BottomLeft.Content  = "3";
                        }
                        break;
                    case C_SlideShow.TileOrigin.BottomLeft:
                        TileArrangePreview_BottomLeft.Content  = "1";
                        if(pf.TileOrientation.Value == C_SlideShow.TileOrientation.Horizontal )
                        {
                            TileArrangePreview_TopLeft.Content     = "3";
                            TileArrangePreview_TopRight.Content    = "4";
                            TileArrangePreview_BottomRight.Content = "2";
                        }
                        else
                        {
                            TileArrangePreview_TopLeft.Content     = "2";
                            TileArrangePreview_TopRight.Content    = "4";
                            TileArrangePreview_BottomRight.Content = "3";
                        }
                        break;
                }
            }
        }

    }
}
