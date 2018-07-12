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
using System.Windows.Shapes;

namespace C_SlideShow
{
    /// <summary>
    /// ProfileEditDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ProfileEditDialog : Window
    {
        // フォルダ・ファイルのパス(このダイアログで編集中のリスト)
        private List<string> editingPath;

        public ProfileEditDialog()
        {
            InitializeComponent();
            SetMinAndMaxToAllControl();
        }

        private void SetMinAndMaxToAllControl()
        {
            // ウインドウ位置
            WinPos_X.MaxValue = int.MaxValue;
            WinPos_X.MinValue = int.MinValue;
            WinPos_Y.MaxValue = int.MaxValue;
            WinPos_Y.MinValue = int.MinValue;

            // ウインドウサイズ
            WinSize_Width.MinValue = 100;
            WinSize_Width.MaxValue = int.MaxValue;
            WinSize_Height.MinValue = 100;
            WinSize_Height.MaxValue = int.MaxValue;

            // ページ番号
            SetMinAndMaxToNumericUpDown( LastPageIndex, nameof(Profile.LastPageIndex) );

            // 列数・行数
            SetMinAndMaxToNumericUpDown( NumofCol, nameof(Profile.NumofCol) );
            SetMinAndMaxToNumericUpDown( NumofRow, nameof(Profile.NumofRow) );

            // アスペクト比(横：縦)
            SetMinAndMaxToNumericUpDown( AspectRatioH, nameof(Profile.AspectRatioH) );
            SetMinAndMaxToNumericUpDown( AspectRatioV, nameof(Profile.AspectRatioV) );

            // 速度(スライドショー)
            SetMinAndMaxToNumericUpDown( SlideSpeed, nameof(Profile.SlideSpeed) );

            // 待機時間
            SetMinAndMaxToNumericUpDown( SlideInterval, nameof(Profile.SlideInterval) );
            
            // スライド時間
            SetMinAndMaxToNumericUpDown( SlideTimeInIntevalMethod, nameof(Profile.SlideTimeInIntevalMethod) );

            // バックバッファのサイズ
            SetMinAndMaxToNumericUpDown( BitmapDecodeTotalPixel, nameof(Profile.BitmapDecodeTotalPixel) );

            // 不透明度(全体)
            OverallOpacity.MinValue = 0;
            OverallOpacity.MaxValue = 100;
            
            // 不透明度(背景)
            BackgroundOpacity.MinValue = 0;
            BackgroundOpacity.MaxValue = 100;

            // ウインドウ枠の太さ
            SetMinAndMaxToNumericUpDown( ResizeGripThickness, nameof(Profile.ResizeGripThickness) );

            // グリッド線の太さ
            SetMinAndMaxToNumericUpDown( TilePadding, nameof(Profile.TilePadding) );
        }

        private void SetMinAndMaxToNumericUpDown(CommonControl.NumericUpDown numericUpDown, string memberName)
        {
            ProfileMemberProp prop = Profile.GetProfileMemberProp( memberName );
            numericUpDown.MinValue = (int)prop.Min;
            numericUpDown.MaxValue = (int)prop.Max;
        }

        public void LoadProfile(Profile pf)
        {
            /* ---------------------------------------------------- */
            // ウインドウの状態
            /* ---------------------------------------------------- */
            // ウインドウの位置
            WinPos_X.Value = (int)pf.WindowRect.Left;
            WinPos_Y.Value = (int)pf.WindowRect.Top;

            // ウインドウサイズ
            WinSize_Width.Value = (int)pf.WindowRect.Width;
            WinSize_Height.Value = (int)pf.WindowRect.Height;

            // フルスクリーン
            IsFullScreenMode.SelectedIndex = pf.IsFullScreenMode ? 0 : 1;

            /* ---------------------------------------------------- */
            // 読み込み
            /* ---------------------------------------------------- */
            // ファイル・フォルダ
            editingPath = new List<string>(pf.Path);
            UpdatePathListView();

            // ページ番号
            LastPageIndex.Value = pf.LastPageIndex;

            /* ---------------------------------------------------- */
            // 列数・行数
            /* ---------------------------------------------------- */
            NumofCol.Value = pf.NumofCol;
            NumofRow.Value = pf.NumofRow;

            /* ---------------------------------------------------- */
            // グリッドのアスペクト比
            /* ---------------------------------------------------- */
            // アスペクト比を非固定
            NonFixAspectRatio.SelectedIndex = pf.NonFixAspectRatio ? 0 : 1;

            // アスペクト比
            AspectRatioH.Value = pf.AspectRatioH;
            AspectRatioV.Value = pf.AspectRatioV;

            /* ---------------------------------------------------- */
            // スライド
            /* ---------------------------------------------------- */
            // スライドショー設定
            if( pf.SlidePlayMethod == C_SlideShow.SlidePlayMethod.Continuous ) SlidePlayMethod.SelectedIndex = 0;
            else SlidePlayMethod.SelectedIndex = 1;

            // スライド方向
            switch( pf.SlideDirection )
            {
                case C_SlideShow.SlideDirection.Left:
                    SlideDirection.SelectedIndex = 0;
                    break;
                case C_SlideShow.SlideDirection.Top:
                    SlideDirection.SelectedIndex = 1;
                    break;
                case C_SlideShow.SlideDirection.Right:
                    SlideDirection.SelectedIndex = 2;
                    break;
                case C_SlideShow.SlideDirection.Bottom:
                    SlideDirection.SelectedIndex = 3;
                    break;
            }

            /* ---------------------------------------------------- */
            // スライドショー詳細
            /* ---------------------------------------------------- */
            // 速度
            SlideSpeed.Value = (int)pf.SlideSpeed;

            // 待機時間(sec)
            SlideInterval.Value = pf.SlideInterval;

            // スライド時間(ms)
            SlideTimeInIntevalMethod.Value = pf.SlideTimeInIntevalMethod;

            // 画像一枚ずつスライド
            SlideByOneImage.SelectedIndex = pf.SlideByOneImage ? 0 : 1;

            // [todo] 自動再生

            /* ---------------------------------------------------- */
            // その他/全般
            /* ---------------------------------------------------- */
            // 最前面表示
            TopMost.SelectedIndex = pf.TopMost ? 0 : 1;

            // 画像の並び順
            switch( pf.FileSortMethod )
            {
                case C_SlideShow.FileSortMethod.FileName:
                    FileSortMethod.SelectedIndex = 0;
                    break;
                case C_SlideShow.FileSortMethod.FileNameRev:
                    FileSortMethod.SelectedIndex = 1;
                    break;
                case C_SlideShow.FileSortMethod.FileNameNatural:
                    FileSortMethod.SelectedIndex = 2;
                    break;
                case C_SlideShow.FileSortMethod.FileNameNaturalRev:
                    FileSortMethod.SelectedIndex = 3;
                    break;
                case C_SlideShow.FileSortMethod.LastWriteTime:
                    FileSortMethod.SelectedIndex = 4;
                    break;
                case C_SlideShow.FileSortMethod.LastWriteTimeRev:
                    FileSortMethod.SelectedIndex = 5;
                    break;
                case C_SlideShow.FileSortMethod.Random:
                    FileSortMethod.SelectedIndex = 6;
                    break;
                case C_SlideShow.FileSortMethod.None:
                    FileSortMethod.SelectedIndex = 7;
                    break;
            }

            // Exifの回転・反転情報
            ApplyRotateInfoFromExif.SelectedIndex = pf.ApplyRotateInfoFromExif ? 0 : 1;

            // バックバッファのサイズ(ピクセル値)
            BitmapDecodeTotalPixel.Value = pf.BitmapDecodeTotalPixel;

            /* ---------------------------------------------------- */
            // その他/外観1
            /* ---------------------------------------------------- */
            // 透過
            AllowTransparency.SelectedIndex = pf.AllowTransparency ? 0 : 1;

            // 不透明度(全体)
            OverallOpacity.Value = (int)(pf.OverallOpacity * 100);

            // 不透明度(背景)
            BackgroundOpacity.Value = (int)(pf.BackgroundOpacity * 100);

            // 背景色
            BaseGridBackgroundColor.PickedColor = pf.BaseGridBackgroundColor;

            // チェック柄の背景
            UsePlaidBackground.SelectedIndex = pf.UsePlaidBackground ? 0 : 1;

            // ペアとなる背景色
            PairColorOfPlaidBackground.PickedColor = pf.PairColorOfPlaidBackground;

            /* ---------------------------------------------------- */
            // その他/外観2
            /* ---------------------------------------------------- */
            // ウインドウ枠の太さ
            ResizeGripThickness.Value = (int)pf.ResizeGripThickness;

            // ウインドウ枠の色
            ResizeGripColor.PickedColor = pf.ResizeGripColor;

            // グリッド線の太さ
            TilePadding.Value = pf.TilePadding;

            // グリッド線の色
            GridLineColor.PickedColor = pf.GridLineColor;

            // シークバーの色
            SeekbarColor.PickedColor = pf.SeekbarColor;
        }

        /// <summary>
        /// ダイアログ表示中の、ファイル・フォルダパスを更新
        /// </summary>
        private void UpdatePathListView()
        {
            string pathTooltip = "";

            for(int i=0; i<editingPath.Count; i++ )
            {
                if( i >= 30 )
                {
                    pathTooltip += string.Format("... 他{0}件の項目", editingPath.Count - i);
                    break;
                }
                if( i == 0 ) Path.Text = editingPath[i];
                pathTooltip += editingPath[i];
                if(i != editingPath.Count - 1) pathTooltip += "\n";
            }
            Path.ToolTip = pathTooltip;
            ToolTipService.SetShowDuration(Path, 1000000);
            Path_Label.Content = string.Format("{0}件の項目", editingPath.Count);
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
        }
    }
}
