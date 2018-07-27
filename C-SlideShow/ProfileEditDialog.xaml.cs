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
    public enum ProfileEditDialogMode
    {
        New, Edit
    }

    /// <summary>
    /// ProfileEditDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ProfileEditDialog : Window
    {
        /* ---------------------------------------------------- */
        //     フィールド
        /* ---------------------------------------------------- */
        // フォルダ・ファイルのパス(このダイアログで編集中のリスト)
        private List<string> editingPath;

        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        /// <summary>
        /// 編集中のプロファイル情報(モードがEditの時のみ扱う)
        /// </summary>
        public UserProfileInfo EditingUserProfileInfo { get; set; }

        /// <summary>
        /// モード
        /// </summary>
        public ProfileEditDialogMode Mode { get; set; }

        /// <summary>
        /// 現在のプロファイルリスト
        /// </summary>
        public List<UserProfileInfo> UserProfileList { get; set; }

        /* ---------------------------------------------------- */
        //     コンストラクタ
        /* ---------------------------------------------------- */
        public ProfileEditDialog()
        {
            InitializeComponent();
            SetMinAndMaxToAllControl();
        }

        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */
        private void SetMinAndMaxToAllControl()
        {
            // ウインドウ位置
            SetMinAndMaxToNumericUpDown( WinPos_X, ProfileMember.WindowPos.Min, ProfileMember.WindowPos.Max);
            SetMinAndMaxToNumericUpDown( WinPos_X, ProfileMember.WindowPos.Min, ProfileMember.WindowPos.Max);

            // ウインドウサイズ
            SetMinAndMaxToNumericUpDown( WinSize_Width, ProfileMember.WindowSize.Min, ProfileMember.WindowSize.Max);
            SetMinAndMaxToNumericUpDown( WinSize_Height, ProfileMember.WindowSize.Min, ProfileMember.WindowSize.Max);

            // ページ番号
            SetMinAndMaxToNumericUpDown( LastPageIndex, ProfileMember.LastPageIndex.Min, ProfileMember.LastPageIndex.Max);

            // 列数・行数
            SetMinAndMaxToNumericUpDown( NumofCol, ProfileMember.NumofMatrix.Min, ProfileMember.NumofMatrix.Max);
            SetMinAndMaxToNumericUpDown( NumofRow, ProfileMember.NumofMatrix.Min, ProfileMember.NumofMatrix.Max);

            // アスペクト比(横：縦)
            SetMinAndMaxToNumericUpDown( AspectRatioH, ProfileMember.AspectRatio.Min, ProfileMember.AspectRatio.MaxH);
            SetMinAndMaxToNumericUpDown( AspectRatioV, ProfileMember.AspectRatio.Min, ProfileMember.AspectRatio.MaxV);

            // 速度(スライドショー)
            SetMinAndMaxToNumericUpDown( SlideSpeed, ProfileMember.SlideSpeed.Min, ProfileMember.SlideSpeed.Max);

            // 待機時間
            SetMinAndMaxToNumericUpDown( SlideInterval, ProfileMember.SlideInterval.Min, ProfileMember.SlideInterval.Max);
            
            // スライド時間
            SetMinAndMaxToNumericUpDown( SlideTimeInIntevalMethod, ProfileMember.SlideTimeInIntevalMethod.Min, ProfileMember.SlideTimeInIntevalMethod.Max);

            // バックバッファのサイズ
            SetMinAndMaxToNumericUpDown( BitmapDecodeTotalPixel, ProfileMember.BitmapDecodeTotalPixel.Min, ProfileMember.BitmapDecodeTotalPixel.Max);

            // 不透明度(全体)
            OverallOpacity.MinValue = 0;
            OverallOpacity.MaxValue = 100;
            
            // 不透明度(背景)
            BackgroundOpacity.MinValue = 0;
            BackgroundOpacity.MaxValue = 100;

            // ウインドウ枠の太さ
            SetMinAndMaxToNumericUpDown( ResizeGripThickness, ProfileMember.ResizeGripThickness.Min, ProfileMember.ResizeGripThickness.Max);

            // グリッド線の太さ
            SetMinAndMaxToNumericUpDown( TilePadding, ProfileMember.TilePadding.Min, ProfileMember.TilePadding.Max);
        }

        private void SetMinAndMaxToNumericUpDown(CommonControl.NumericUpDown numericUpDown, int min, int max)
        {
            numericUpDown.MinValue = min;
            numericUpDown.MaxValue = max;
        }

        /// <summary>
        /// プロファイルの「有効・無効」をチェックボックスへ入力
        /// </summary>
        private void SetProfileMembaersWhetherEnabled()
        {
            if(Mode == ProfileEditDialogMode.New )
            {
                // 新規作成の場合
                //bool b = false;
                WpfTreeUtil.OperateLogicalChildren(MainStackPanel, obj =>
                {
                    CheckBox cb = obj as CheckBox;
                    if(cb != null )
                    {
                        //if( cb == this.PfCheckBox_AllowTransparency ) b = true;
                        //if( b ) cb.IsChecked = false; // 「外観」以降にはチェックを入れない
                        //else cb.IsChecked = true;

                        cb.IsChecked = false;
                    }
                });
            }
            else if(Mode == ProfileEditDialogMode.Edit )
            {
                // 編集の場合
                WpfTreeUtil.OperateLogicalChildren(MainStackPanel, obj =>
                {
                    CheckBox cb = obj as CheckBox;
                    if(cb != null ) { cb.IsChecked = false; }
                });

                Profile pf = EditingUserProfileInfo.Profile;

                // ウインドウの状態
                if( pf.WindowPos.IsEnabled ) PfCheckBox_WinPos.IsChecked = true;
                if( pf.WindowSize.IsEnabled ) PfCheckBox_WinSize.IsChecked = true;
                if( pf.IsFullScreenMode.IsEnabled ) PfCheckBox_IsFullScreenMode.IsChecked = true;
                // 読み込み
                if( pf.Path.IsEnabled ) PfCheckBox_Path.IsChecked = true;
                if( pf.LastPageIndex.IsEnabled ) PfCheckBox_LastPageIndex.IsChecked = true;
                // 列数・行数
                if( pf.NumofMatrix.IsEnabled ) PfCheckBox_NumofMatrix.IsChecked = true;
                // グリッドのアスペクト比
                if( pf.NonFixAspectRatio.IsEnabled ) PfCheckBox_FixAspectRatio.IsChecked = true;
                if( pf.AspectRatio.IsEnabled ) PfCheckBox_AspectRatio.IsChecked = true;
                // スライド
                if( pf.SlidePlayMethod.IsEnabled ) PfCheckBox_SlidePlayMethod.IsChecked = true;
                if( pf.SlideDirection.IsEnabled ) PfCheckBox_SlideDirection.IsChecked = true;
                // スライドショー詳細
                if( pf.SlideSpeed.IsEnabled ) PfCheckBox_SlideSpeed.IsChecked = true;
                if( pf.SlideInterval.IsEnabled ) PfCheckBox_SlideInterval.IsChecked = true;
                if( pf.SlideTimeInIntevalMethod.IsEnabled ) PfCheckBox_SlideTimeInIntevalMethod.IsChecked = true;
                if( pf.SlideByOneImage.IsEnabled ) PfCheckBox_SlideByOneImage.IsChecked = true;
                if( pf.SlideShowAutoStart.IsEnabled ) PfCheckBox_SlideShowAutoStart.IsChecked = true;
                // その他/全般
                if( pf.TopMost.IsEnabled ) PfCheckBox_TopMost.IsChecked = true;
                if( pf.FileSortMethod.IsEnabled ) PfCheckBox_FileSortMethod.IsChecked = true;
                if( pf.ApplyRotateInfoFromExif.IsEnabled ) PfCheckBox_ApplyRotateInfoFromExif.IsChecked = true;
                if( pf.BitmapDecodeTotalPixel.IsEnabled ) PfCheckBox_BitmapDecodeTotalPixel.IsChecked = true;
                // その他/外観1
                if( pf.AllowTransparency.IsEnabled ) PfCheckBox_AllowTransparency.IsChecked = true;
                if( pf.OverallOpacity.IsEnabled ) PfCheckBox_OverallOpacity.IsChecked = true;
                if( pf.BackgroundOpacity.IsEnabled ) PfCheckBox_BackgroundOpacity.IsChecked = true;
                if( pf.BaseGridBackgroundColor.IsEnabled ) PfCheckBox_BaseGridBackgroundColor.IsChecked = true;
                if( pf.UsePlaidBackground.IsEnabled ) PfCheckBox_UsePlaidBackground.IsChecked = true;
                if( pf.PairColorOfPlaidBackground.IsEnabled ) PfCheckBox_PairColorOfPlaidBackground.IsChecked = true;
                // その他/外観2
                if( pf.ResizeGripThickness.IsEnabled ) PfCheckBox_ResizeGripThickness.IsChecked = true;
                if( pf.ResizeGripColor.IsEnabled ) PfCheckBox_ResizeGripColor.IsChecked = true;
                if( pf.TilePadding.IsEnabled ) PfCheckBox_TilePadding.IsChecked = true;
                if( pf.GridLineColor.IsEnabled ) PfCheckBox_GridLineColor.IsChecked = true;
                if( pf.SeekbarColor.IsEnabled ) PfCheckBox_SeekbarColor.IsChecked = true;
            }
        }

        /// <summary>
        /// プロファイルを読み込み、コントロールに値を代入
        /// </summary>
        /// <param name="pf"></param>
        public void SetControlValue(Profile pf)
        {
            // 有効・無効の反映
            SetProfileMembaersWhetherEnabled();

            /* ---------------------------------------------------- */
            // ウインドウの状態
            /* ---------------------------------------------------- */
            // ウインドウの位置
            WinPos_X.Value = (int)pf.WindowPos.X;
            WinPos_Y.Value = (int)pf.WindowPos.Y;

            // ウインドウサイズ
            WinSize_Width.Value = (int)pf.WindowSize.Width;
            WinSize_Height.Value = (int)pf.WindowSize.Height;

            // フルスクリーン
            IsFullScreenMode.SelectedIndex = pf.IsFullScreenMode.Value ? 0 : 1;

            /* ---------------------------------------------------- */
            // 読み込み
            /* ---------------------------------------------------- */
            // ファイル・フォルダ
            editingPath = new List<string>(pf.Path.Value);
            UpdatePathListView();

            // ページ番号
            LastPageIndex.Value = pf.LastPageIndex.Value + 1;

            /* ---------------------------------------------------- */
            // 列数・行数
            /* ---------------------------------------------------- */
            NumofCol.Value = pf.NumofMatrix.Col;
            NumofRow.Value = pf.NumofMatrix.Row;

            /* ---------------------------------------------------- */
            // グリッドのアスペクト比
            /* ---------------------------------------------------- */
            // アスペクト比を固定
            FixAspectRatio.SelectedIndex = pf.NonFixAspectRatio.Value ? 1 : 0;

            // アスペクト比
            AspectRatioH.Value = pf.AspectRatio.H;
            AspectRatioV.Value = pf.AspectRatio.V;

            /* ---------------------------------------------------- */
            // スライド
            /* ---------------------------------------------------- */
            // スライドショー設定
            if( pf.SlidePlayMethod.Value == C_SlideShow.SlidePlayMethod.Continuous ) SlidePlayMethod.SelectedIndex = 0;
            else SlidePlayMethod.SelectedIndex = 1;

            // スライド方向
            switch( pf.SlideDirection.Value )
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
            SlideSpeed.Value = (int)pf.SlideSpeed.Value;

            // 待機時間(sec)
            SlideInterval.Value = pf.SlideInterval.Value;

            // スライド時間(ms)
            SlideTimeInIntevalMethod.Value = pf.SlideTimeInIntevalMethod.Value;

            // 画像一枚ずつスライド
            SlideByOneImage.SelectedIndex = pf.SlideByOneImage.Value ? 0 : 1;

            // 自動再生
            SlideShowAutoStart.SelectedIndex = pf.SlideShowAutoStart.Value ? 0 : 1;

            /* ---------------------------------------------------- */
            // その他/全般
            /* ---------------------------------------------------- */
            // 最前面表示
            TopMost.SelectedIndex = pf.TopMost.Value ? 0 : 1;

            // 画像の並び順
            switch( pf.FileSortMethod.Value )
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
            ApplyRotateInfoFromExif.SelectedIndex = pf.ApplyRotateInfoFromExif.Value ? 0 : 1;

            // バックバッファのサイズ(ピクセル値)
            BitmapDecodeTotalPixel.Value = pf.BitmapDecodeTotalPixel.Value;

            /* ---------------------------------------------------- */
            // その他/外観1
            /* ---------------------------------------------------- */
            // 透過
            AllowTransparency.SelectedIndex = pf.AllowTransparency.Value ? 0 : 1;

            // 不透明度(全体)
            OverallOpacity.Value = (int)(pf.OverallOpacity.Value * 100);

            // 不透明度(背景)
            BackgroundOpacity.Value = (int)(pf.BackgroundOpacity.Value * 100);

            // 背景色
            BaseGridBackgroundColor.PickedColor = pf.BaseGridBackgroundColor.Value;

            // チェック柄の背景
            UsePlaidBackground.SelectedIndex = pf.UsePlaidBackground.Value ? 0 : 1;

            // ペアとなる背景色
            PairColorOfPlaidBackground.PickedColor = pf.PairColorOfPlaidBackground.Value;

            /* ---------------------------------------------------- */
            // その他/外観2
            /* ---------------------------------------------------- */
            // ウインドウ枠の太さ
            ResizeGripThickness.Value = (int)pf.ResizeGripThickness.Value;

            // ウインドウ枠の色
            ResizeGripColor.PickedColor = pf.ResizeGripColor.Value;

            // グリッド線の太さ
            TilePadding.Value = pf.TilePadding.Value;

            // グリッド線の色
            GridLineColor.PickedColor = pf.GridLineColor.Value;

            // シークバーの色
            SeekbarColor.PickedColor = pf.SeekbarColor.Value;

            /* ---------------------------------------------------- */
            // プロファイル名
            /* ---------------------------------------------------- */
            Footer_ProfileName.Items.Clear();
            switch( Mode )
            {
                case ProfileEditDialogMode.New:
                    Footer_ProfileName.Text = SuggestNewProfileName();
                    break;
                case ProfileEditDialogMode.Edit:
                    Footer_ProfileName.Text = pf.Name;
                    break;
            }
            UserProfileList.ForEach( pl => Footer_ProfileName.Items.Add(pl.Profile.Name) );

        }

        private string SuggestNewProfileName()
        {
            int cnt = 1;
            while(cnt != int.MaxValue )
            {
                string newName = "新規プロファイル" + cnt.ToString();
                if( !UserProfileList.Any(pl => pl.Profile.Name == newName) ) return newName;
                cnt++;
            }
            return Guid.NewGuid ().ToString ("N").Substring(0, 12); // 適当な文字列
        }

        /// <summary>
        /// ダイアログ表示中の、ファイル・フォルダパスを更新
        /// </summary>
        private void UpdatePathListView()
        {
            // テキストボックス
            if( editingPath.Count > 0 ) Path.Text = editingPath[0];
            else Path.Text = "";

            // ツールチップ
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

            // ラベル
            Path_Label.Content = string.Format("{0}件の項目", editingPath.Count);
        }

        private Profile CreateProfile()
        {
            Profile pf = new Profile();

            /* ---------------------------------------------------- */
            // プロファイル名、タイプ、有効無効
            /* ---------------------------------------------------- */
            pf.Name = Footer_ProfileName.Text;
            pf.ProfileType = ProfileType.User;


            /* ---------------------------------------------------- */
            // ウインドウの状態
            /* ---------------------------------------------------- */
            // ウインドウの位置
            if( (bool)PfCheckBox_WinPos.IsChecked )
            {
                pf.WindowPos.IsEnabled = true;
                pf.WindowPos.Value = new Point( WinPos_X.Value, WinPos_Y.Value );
            }

            // ウインドウサイズ
            if( (bool)PfCheckBox_WinSize.IsChecked )
            {
                pf.WindowSize.IsEnabled = true;
                pf.WindowSize.Value = new Size( WinSize_Width.Value, WinSize_Height.Value );
            }

            // フルスクリーン
            if( (bool)PfCheckBox_IsFullScreenMode.IsChecked )
            {
                pf.IsFullScreenMode.IsEnabled = true;
                pf.IsFullScreenMode.Value = IsFullScreenMode.SelectedIndex == 0 ? true: false;
            }

            /* ---------------------------------------------------- */
            // 読み込み
            /* ---------------------------------------------------- */
            // ファイル・フォルダ
            if( (bool)PfCheckBox_Path.IsChecked )
            {
                pf.Path.IsEnabled = true;
                pf.Path.Value = editingPath;
            }

            // ページ番号
            if( (bool)PfCheckBox_LastPageIndex.IsChecked )
            {
                pf.LastPageIndex.IsEnabled = true;
                pf.LastPageIndex.Value = LastPageIndex.Value - 1;
            }

            /* ---------------------------------------------------- */
            // 列数・行数
            /* ---------------------------------------------------- */
            if( (bool)PfCheckBox_NumofMatrix.IsChecked )
            {
                pf.NumofMatrix.IsEnabled = true;
                pf.NumofMatrix.Value = new int[] { NumofCol.Value, NumofRow.Value };
            }

            /* ---------------------------------------------------- */
            // グリッドのアスペクト比
            /* ---------------------------------------------------- */
            // アスペクト比を固定
            if( (bool)PfCheckBox_FixAspectRatio.IsChecked )
            {
                pf.NonFixAspectRatio.IsEnabled = true;
                pf.NonFixAspectRatio.Value = FixAspectRatio.SelectedIndex == 0 ? false : true;
            }

            // アスペクト比
            if( (bool)PfCheckBox_AspectRatio.IsChecked )
            {
                pf.AspectRatio.IsEnabled = true;
                pf.AspectRatio.Value = new int[] { AspectRatioH.Value, AspectRatioV.Value };
            }

            /* ---------------------------------------------------- */
            // スライド
            /* ---------------------------------------------------- */
            // スライドショー設定
            if( (bool)PfCheckBox_SlidePlayMethod.IsChecked )
            {
                pf.SlidePlayMethod.IsEnabled = true;
                pf.SlidePlayMethod.Value = SlidePlayMethod.SelectedIndex == 0? C_SlideShow.SlidePlayMethod.Continuous: C_SlideShow.SlidePlayMethod.Interval;
            }

            // スライド方向
            if( (bool)PfCheckBox_SlideDirection.IsChecked )
            {
                pf.SlideDirection.IsEnabled = true;
                switch( SlideDirection.SelectedIndex )
                {
                    case 0:
                        pf.SlideDirection.Value = C_SlideShow.SlideDirection.Left;
                        break;
                    case 1:
                        pf.SlideDirection.Value = C_SlideShow.SlideDirection.Top;
                        break;
                    case 2:
                        pf.SlideDirection.Value = C_SlideShow.SlideDirection.Right;
                        break;
                    case 3:
                        pf.SlideDirection.Value = C_SlideShow.SlideDirection.Bottom;
                        break;
                }
            }

            /* ---------------------------------------------------- */
            // スライドショー詳細
            /* ---------------------------------------------------- */
            // 速度
            if( (bool)PfCheckBox_SlideSpeed.IsChecked )
            {
                pf.SlideSpeed.IsEnabled = true;
                pf.SlideSpeed.Value = SlideSpeed.Value;
            }

            // 待機時間(sec)
            if( (bool)PfCheckBox_SlideInterval.IsChecked )
            {
                pf.SlideInterval.IsEnabled = true;
                pf.SlideInterval.Value = SlideInterval.Value;
            }

            // スライド時間(ms)
            if( (bool)PfCheckBox_SlideTimeInIntevalMethod.IsChecked )
            {
                pf.SlideTimeInIntevalMethod.IsEnabled = true;
                pf.SlideTimeInIntevalMethod.Value = SlideTimeInIntevalMethod.Value;
            }

            // 画像一枚ずつスライド
            if( (bool)PfCheckBox_SlideByOneImage.IsChecked )
            {
                pf.SlideByOneImage.IsEnabled = true;
                pf.SlideByOneImage.Value = SlideByOneImage.SelectedIndex == 0 ? true : false;
            }

            // 自動再生
            if( (bool)PfCheckBox_SlideShowAutoStart.IsChecked )
            {
                pf.SlideShowAutoStart.IsEnabled = true;
                pf.SlideShowAutoStart.Value = SlideShowAutoStart.SelectedIndex == 0 ? true : false;
            }

            /* ---------------------------------------------------- */
            // その他/全般
            /* ---------------------------------------------------- */
            // 最前面表示
            if( (bool)PfCheckBox_TopMost.IsChecked )
            {
                pf.TopMost.IsEnabled = true;
                pf.TopMost.Value = TopMost.SelectedIndex == 0 ? true : false;
            }

            // 画像の並び順
            if( (bool)PfCheckBox_FileSortMethod.IsChecked )
            {
                pf.FileSortMethod.IsEnabled = true;

                switch( FileSortMethod.SelectedIndex )
                {
                    case 0:
                        pf.FileSortMethod.Value = C_SlideShow.FileSortMethod.FileName;
                        break;
                    case 1:
                        pf.FileSortMethod.Value = C_SlideShow.FileSortMethod.FileNameRev;
                        break;
                    case 2:
                        pf.FileSortMethod.Value = C_SlideShow.FileSortMethod.FileNameNatural;
                        break;
                    case 3:
                        pf.FileSortMethod.Value = C_SlideShow.FileSortMethod.FileNameNaturalRev;
                        break;
                    case 4:
                        pf.FileSortMethod.Value = C_SlideShow.FileSortMethod.LastWriteTime;
                        break;
                    case 5:
                        pf.FileSortMethod.Value = C_SlideShow.FileSortMethod.LastWriteTimeRev;
                        break;
                    case 6:
                        pf.FileSortMethod.Value = C_SlideShow.FileSortMethod.Random;
                        break;
                    case 7:
                        pf.FileSortMethod.Value = C_SlideShow.FileSortMethod.None;
                        break;
                }
            }

            // Exifの回転・反転情報
            if( (bool)PfCheckBox_ApplyRotateInfoFromExif.IsChecked )
            {
                pf.ApplyRotateInfoFromExif.IsEnabled = true;
                pf.ApplyRotateInfoFromExif.Value = ApplyRotateInfoFromExif.SelectedIndex == 0 ? true : false;
            }

            // バックバッファのサイズ(ピクセル値)
            if( (bool)PfCheckBox_BitmapDecodeTotalPixel.IsChecked )
            {
                pf.BitmapDecodeTotalPixel.IsEnabled = true;
                pf.BitmapDecodeTotalPixel.Value = BitmapDecodeTotalPixel.Value;
            }

            /* ---------------------------------------------------- */
            // その他/外観1
            /* ---------------------------------------------------- */
            // 透過
            if( (bool)PfCheckBox_AllowTransparency.IsChecked )
            {
                pf.AllowTransparency.IsEnabled = true;
                pf.AllowTransparency.Value = AllowTransparency.SelectedIndex == 0 ? true : false;
            }

            // 不透明度(全体)
            if( (bool)PfCheckBox_OverallOpacity.IsChecked )
            {
                pf.OverallOpacity.IsEnabled = true;
                double val = (double)OverallOpacity.Value / 100;
                if( val <= 0 ) val = ProfileMember.OverallOpacity.Min;
                pf.OverallOpacity.Value = val;
            }

            // 不透明度(背景)
            if( (bool)PfCheckBox_BackgroundOpacity.IsChecked )
            {
                pf.BackgroundOpacity.IsEnabled = true;
                double val = (double)BackgroundOpacity.Value / 100;
                if( val <= 0 ) val = ProfileMember.BackgroundOpacity.Min;
                pf.BackgroundOpacity.Value = val;
            }

            // 背景色
            if( (bool)PfCheckBox_BaseGridBackgroundColor.IsChecked )
            {
                pf.BaseGridBackgroundColor.IsEnabled = true;
                pf.BaseGridBackgroundColor.Value = BaseGridBackgroundColor.PickedColor;
            }

            // チェック柄の背景
            if( (bool)PfCheckBox_UsePlaidBackground.IsChecked )
            {
                pf.UsePlaidBackground.IsEnabled = true;
                pf.UsePlaidBackground.Value = UsePlaidBackground.SelectedIndex == 0 ? true : false;
            }

            // ペアとなる背景色
            if( (bool)PfCheckBox_PairColorOfPlaidBackground.IsChecked )
            {
                pf.PairColorOfPlaidBackground.IsEnabled = true;
                pf.PairColorOfPlaidBackground.Value = PairColorOfPlaidBackground.PickedColor;
            }


            /* ---------------------------------------------------- */
            // その他/外観2
            /* ---------------------------------------------------- */
            // ウインドウ枠の太さ
            if( (bool)PfCheckBox_ResizeGripThickness.IsChecked )
            {
                pf.ResizeGripThickness.IsEnabled = true;
                pf.ResizeGripThickness.Value = ResizeGripThickness.Value;
            }

            // ウインドウ枠の色
            if( (bool)PfCheckBox_ResizeGripColor.IsChecked )
            {
                pf.ResizeGripColor.IsEnabled = true;
                pf.ResizeGripColor.Value = ResizeGripColor.PickedColor;
            }

            // グリッド線の太さ
            if( (bool)PfCheckBox_TilePadding.IsChecked )
            {
                pf.TilePadding.IsEnabled = true;
                pf.TilePadding.Value = TilePadding.Value;
            }

            // グリッド線の色
            if( (bool)PfCheckBox_GridLineColor.IsChecked )
            {
                pf.GridLineColor.IsEnabled = true;
                pf.GridLineColor.Value = GridLineColor.PickedColor;
            }

            // シークバーの色
            if( (bool)PfCheckBox_SeekbarColor.IsChecked )
            {
                pf.SeekbarColor.IsEnabled = true;
                pf.SeekbarColor.Value = SeekbarColor.PickedColor;
            }


            return pf;
            // End of method
        }

        /// <summary>
        /// プロファイルリストに追加
        /// </summary>
        /// <param name="pf"></param>
        /// <param name="index">上書きではない場合、リストのどの位置に追加するか(-1指定で末尾に追加)</param>
        private void AddToUserProfileList(UserProfileInfo newUserProfileInfo, int index)
        {
            if( UserProfileList.Any( pl => pl.RelativePath == newUserProfileInfo.RelativePath) )
            {
                //　上書き
                UserProfileInfo oldProfileInfo = UserProfileList.FirstOrDefault( pl => pl.RelativePath == newUserProfileInfo.RelativePath);
                if(oldProfileInfo != null )
                {
                    index = UserProfileList.IndexOf(oldProfileInfo);
                    UserProfileList.RemoveAt(index);
                    UserProfileList.Insert(index, newUserProfileInfo);
                    newUserProfileInfo.SaveProfileToXmlFile();
                }
            }
            else
            {
                // 追加
                if(index > -1) UserProfileList.Insert(index, newUserProfileInfo);
                else UserProfileList.Add(newUserProfileInfo);
                newUserProfileInfo.SaveProfileToXmlFile();
            }
        }

        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
        }


        // ファイルパス テキストボックス
        private void Path_Drop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            this.editingPath.AddRange( files.ToList() );
            UpdatePathListView();
        }

        private void Path_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        // ファイルパス クリアボタン
        private void Path_ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.editingPath.RemoveRange(0, editingPath.Count);
            UpdatePathListView();
        }

        // フッター
        private void Footer_CheckAll_Click(object sender, RoutedEventArgs e)
        {
            this.Descendants<CheckBox>()
                .ToList()
                .ConvertAll(x => x.IsChecked = true);
                //.Select(x => x.IsChecked = true);
        }

        private void Footer_UnCheckAll_Click(object sender, RoutedEventArgs e)
        {
            this.Descendants<CheckBox>()
                .ToList()
                .ConvertAll(x => x.IsChecked = false);
                //.Select(x => x.IsChecked = false);
        }

        private void Footer_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Footer_OK_Click(object sender, RoutedEventArgs e)
        {
            // ファイル名に使用できない文字をアンダーバーに書き換え
            Footer_ProfileName.Text = Util.ValidFileName(Footer_ProfileName.Text);

            Profile newProfile = CreateProfile();
            UserProfileInfo newUserProfileInfo = new UserProfileInfo( newProfile.Name + ".xml" );
            newUserProfileInfo.Profile = newProfile;

            // プロファイル名重複確認
            bool conflict = false;
            if( UserProfileList.Any( pl => pl.RelativePath == newUserProfileInfo.RelativePath) )
            {
                switch( Mode )
                {
                    case ProfileEditDialogMode.New:
                        conflict = true;
                        break;
                    case ProfileEditDialogMode.Edit:
                        if( EditingUserProfileInfo.RelativePath == newUserProfileInfo.RelativePath ) conflict = false;
                        else conflict = true;
                        break;
                }
            }
            if( conflict )
            {
                MessageBoxResult result =  MessageBox.Show("プロファイル名「" + newProfile.Name + "」は既に存在しています。上書きしてよろしいですか？", "上書き確認", 
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if( result == MessageBoxResult.No ) return;
            }

            // プロファイル追加or上書き
            if(Mode == ProfileEditDialogMode.New )
            {
                AddToUserProfileList(newUserProfileInfo, -1);
            }
            else if(Mode == ProfileEditDialogMode.Edit )
            {
                // 編集前のプロファイルを削除
                int editingProfileIndex = UserProfileList.IndexOf(EditingUserProfileInfo);
                MainWindow.Current.RemoveUserProfileInfo(EditingUserProfileInfo);

                AddToUserProfileList(newUserProfileInfo, editingProfileIndex);
            }

            this.Close();
        }

    }
}
