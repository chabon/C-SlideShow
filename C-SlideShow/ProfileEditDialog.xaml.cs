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
        /// 編集中のプロファイル(モードがEditの時のみ扱う)
        /// </summary>
        public Profile EditingProfile { get; set; }

        /// <summary>
        /// モード
        /// </summary>
        public ProfileEditDialogMode Mode { get; set; }

        /// <summary>
        /// 現在のプロファイルリスト
        /// </summary>
        public List<Profile> ProfileList { get; set; }

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

        /// <summary>
        /// プロファイルを読み込み、コントロールに値を代入
        /// </summary>
        /// <param name="pf"></param>
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
            // アスペクト比を固定
            FixAspectRatio.SelectedIndex = pf.NonFixAspectRatio ? 1 : 0;

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

            /* ---------------------------------------------------- */
            // プロファイル名
            /* ---------------------------------------------------- */
            switch( Mode )
            {
                case ProfileEditDialogMode.New:
                    Footer_ProfileName.Text = SuggestNewProfileName();
                    Footer_ProfileName.Items.Clear();
                    ProfileList.ForEach( p => Footer_ProfileName.Items.Add(p.Name) );
                    break;
                case ProfileEditDialogMode.Edit:
                    Footer_ProfileName.Text = EditingProfile.Name;
                    break;
            }

        }

        private string SuggestNewProfileName()
        {
            int cnt = 1;
            while(cnt != int.MaxValue )
            {
                string newName = "新規プロファイル" + cnt.ToString();
                if( !ProfileList.Any(p => p.Name == newName) ) return newName;
                cnt++;
            }
            return Guid.NewGuid ().ToString ("N").Substring(0, 12); // 適当な文字列
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

        private Profile CreateProfile()
        {
            Profile pf = new Profile();

            /* ---------------------------------------------------- */
            // ウインドウの状態
            /* ---------------------------------------------------- */
            // ウインドウの位置
            if( (bool)PfCheckBox_WinPos.IsChecked )
            {
                pf.ProfileEnabledMember.WindowRect_Pos = true;
                pf.WindowRect = new Rect(WinPos_X.Value, WinPos_Y.Value, pf.WindowRect.Width, pf.WindowRect.Height);
            }

            // ウインドウサイズ
            if( (bool)PfCheckBox_WinSize.IsChecked )
            {
                pf.ProfileEnabledMember.WindowRect_Size = true;
                pf.WindowRect = new Rect(pf.WindowRect.Left, pf.WindowRect.Top, WinSize_Width.Value, WinSize_Height.Value);
            }

            // フルスクリーン
            if( (bool)PfCheckBox_IsFullScreenMode.IsChecked )
            {
                pf.ProfileEnabledMember.IsFullScreenMode = true;
                pf.IsFullScreenMode = IsFullScreenMode.SelectedIndex == 0 ? true: false;
            }

            /* ---------------------------------------------------- */
            // 読み込み
            /* ---------------------------------------------------- */
            // ファイル・フォルダ
            if( (bool)PfCheckBox_Path.IsChecked )
            {
                pf.ProfileEnabledMember.Path = true;
                pf.Path = editingPath;
            }

            // ページ番号
            if( (bool)PfCheckBoxd_LastPageIndex.IsChecked )
            {
                pf.ProfileEnabledMember.LastPageIndex = true;
                pf.LastPageIndex = LastPageIndex.Value;
            }

            /* ---------------------------------------------------- */
            // 列数・行数
            /* ---------------------------------------------------- */
            if( (bool)PfCheckBox_NumofMatrix.IsChecked )
            {
                pf.ProfileEnabledMember.NumofMatrix = true;
                pf.NumofCol = NumofCol.Value;
                pf.NumofRow = NumofRow.Value;
            }

            /* ---------------------------------------------------- */
            // グリッドのアスペクト比
            /* ---------------------------------------------------- */
            // アスペクト比を固定
            if( (bool)PfCheckBox_FixAspectRatio.IsChecked )
            {
                pf.ProfileEnabledMember.NonFixAspectRatio = true; // あえて逆(Non)にしてるので注意(ダイアログのわかりやすさ優先)
                pf.NonFixAspectRatio = FixAspectRatio.SelectedIndex == 0 ? false : true;
            }

            // アスペクト比
            if( (bool)PfCheckBox_AspectRatio.IsChecked )
            {
                pf.ProfileEnabledMember.AspectRatio = true;
                pf.AspectRatioH = AspectRatioH.Value;
                pf.AspectRatioV = AspectRatioV.Value;
            }

            /* ---------------------------------------------------- */
            // スライド
            /* ---------------------------------------------------- */
            // スライドショー設定
            if( (bool)PfCheckBox_SlidePlayMethod.IsChecked )
            {
                pf.ProfileEnabledMember.SlidePlayMethod = true;
                pf.SlidePlayMethod = SlidePlayMethod.SelectedIndex == 0? C_SlideShow.SlidePlayMethod.Continuous: C_SlideShow.SlidePlayMethod.Interval;
            }

            // スライド方向
            if( (bool)PfCheckBox_SlideDirection.IsChecked )
            {
                pf.ProfileEnabledMember.SlideDirection = true;
                switch( SlideDirection.SelectedIndex )
                {
                    case 0:
                        pf.SlideDirection = C_SlideShow.SlideDirection.Left;
                        break;
                    case 1:
                        pf.SlideDirection = C_SlideShow.SlideDirection.Top;
                        break;
                    case 2:
                        pf.SlideDirection = C_SlideShow.SlideDirection.Right;
                        break;
                    case 3:
                        pf.SlideDirection = C_SlideShow.SlideDirection.Bottom;
                        break;
                }
            }

            /* ---------------------------------------------------- */
            // スライドショー詳細
            /* ---------------------------------------------------- */
            // 速度
            if( (bool)PfCheckBox_SlideSpeed.IsChecked )
            {
                pf.ProfileEnabledMember.SlideSpeed = true;
                pf.SlideSpeed = SlideSpeed.Value;
            }

            // 待機時間(sec)
            if( (bool)PfCheckBox_SlideInterval.IsChecked )
            {
                pf.ProfileEnabledMember.SlideInterval = true;
                pf.SlideInterval = SlideInterval.Value;
            }

            // スライド時間(ms)
            if( (bool)PfCheckBox_SlideTimeInIntevalMethod.IsChecked )
            {
                pf.ProfileEnabledMember.SlideTimeInIntevalMethod = true;
                pf.SlideTimeInIntevalMethod = SlideTimeInIntevalMethod.Value;
            }

            // 画像一枚ずつスライド
            if( (bool)PfCheckBox_SlideByOneImage.IsChecked )
            {
                pf.ProfileEnabledMember.SlideByOneImage = true;
                pf.SlideByOneImage = SlideByOneImage.SelectedIndex == 0 ? true : false;
            }

            // [todo] 自動再生

            /* ---------------------------------------------------- */
            // その他/全般
            /* ---------------------------------------------------- */
            // 最前面表示
            if( (bool)PfCheckBox_TopMost.IsChecked )
            {
                pf.ProfileEnabledMember.TopMost = true;
                pf.TopMost = TopMost.SelectedIndex == 0 ? true : false;
            }

            // 画像の並び順
            if( (bool)PfCheckBox_FileSortMethod.IsChecked )
            {
                pf.ProfileEnabledMember.FileSortMethod = true;

                switch( FileSortMethod.SelectedIndex )
                {
                    case 0:
                        pf.FileSortMethod = C_SlideShow.FileSortMethod.FileName;
                        break;
                    case 1:
                        pf.FileSortMethod = C_SlideShow.FileSortMethod.FileNameRev;
                        break;
                    case 2:
                        pf.FileSortMethod = C_SlideShow.FileSortMethod.FileNameNatural;
                        break;
                    case 3:
                        pf.FileSortMethod = C_SlideShow.FileSortMethod.FileNameNaturalRev;
                        break;
                    case 4:
                        pf.FileSortMethod = C_SlideShow.FileSortMethod.LastWriteTime;
                        break;
                    case 5:
                        pf.FileSortMethod = C_SlideShow.FileSortMethod.LastWriteTimeRev;
                        break;
                    case 6:
                        pf.FileSortMethod = C_SlideShow.FileSortMethod.Random;
                        break;
                    case 7:
                        pf.FileSortMethod = C_SlideShow.FileSortMethod.None;
                        break;
                }
            }

            // Exifの回転・反転情報
            if( (bool)PfCheckBox_ApplyRotateInfoFromExif.IsChecked )
            {
                pf.ProfileEnabledMember.ApplyRotateInfoFromExif = true;
                pf.ApplyRotateInfoFromExif = ApplyRotateInfoFromExif.SelectedIndex == 0 ? true : false;
            }

            // バックバッファのサイズ(ピクセル値)
            if( (bool)PfCheckBox_BitmapDecodeTotalPixel.IsChecked )
            {
                pf.ProfileEnabledMember.BitmapDecodeTotalPixel = true;
                pf.BitmapDecodeTotalPixel = BitmapDecodeTotalPixel.Value;
            }

            /* ---------------------------------------------------- */
            // その他/外観1
            /* ---------------------------------------------------- */
            // 透過
            if( (bool)PfCheckBox_AllowTransparency.IsChecked )
            {
                pf.ProfileEnabledMember.AllowTransparency = true;
                pf.AllowTransparency = AllowTransparency.SelectedIndex == 0 ? true : false;
            }

            // 不透明度(全体)
            if( (bool)PfCheckBox_OverallOpacity.IsChecked )
            {
                pf.ProfileEnabledMember.OverallOpacity = true;
                double val = (double)OverallOpacity.Value / 100;
                if( val <= 0 ) val = 0.005;
                pf.OverallOpacity = val;
            }

            // 不透明度(背景)
            if( (bool)PfCheckBox_BackgroundOpacity.IsChecked )
            {
                pf.ProfileEnabledMember.BackgroundOpacity = true;
                double val = (double)BackgroundOpacity.Value / 100;
                if( val <= 0 ) val = 0.005;
                pf.BackgroundOpacity = val;
            }

            // 背景色
            if( (bool)PfCheckBox_BaseGridBackgroundColor.IsChecked )
            {
                pf.ProfileEnabledMember.BaseGridBackgroundColor = true;
                pf.BaseGridBackgroundColor = BaseGridBackgroundColor.PickedColor;
            }

            // チェック柄の背景
            if( (bool)PfCheckBox_UsePlaidBackground.IsChecked )
            {
                pf.ProfileEnabledMember.UsePlaidBackground = true;
                pf.UsePlaidBackground = UsePlaidBackground.SelectedIndex == 0 ? true : false;
            }

            // ペアとなる背景色
            if( (bool)PfCheckBox_PairColorOfPlaidBackground.IsChecked )
            {
                pf.ProfileEnabledMember.PairColorOfPlaidBackground = true;
                pf.PairColorOfPlaidBackground = PairColorOfPlaidBackground.PickedColor;
            }


            /* ---------------------------------------------------- */
            // その他/外観2
            /* ---------------------------------------------------- */
            // ウインドウ枠の太さ
            if( (bool)PfCheckBox_ResizeGripThickness.IsChecked )
            {
                pf.ProfileEnabledMember.ResizeGripThickness = true;
                pf.ResizeGripThickness = ResizeGripThickness.Value;
            }

            // ウインドウ枠の色
            if( (bool)PfCheckBox_ResizeGripColor.IsChecked )
            {
                pf.ProfileEnabledMember.ResizeGripColor = true;
                pf.ResizeGripColor = ResizeGripColor.PickedColor;
            }

            // グリッド線の太さ
            if( (bool)PfCheckBox_TilePadding.IsChecked )
            {
                pf.ProfileEnabledMember.TilePadding = true;
                pf.TilePadding = TilePadding.Value;
            }

            // グリッド線の色
            if( (bool)PfCheckBox_GridLineColor.IsChecked )
            {
                pf.ProfileEnabledMember.GridLineColor = true;
                pf.GridLineColor = GridLineColor.PickedColor;
            }

            // シークバーの色
            if( (bool)PfCheckBox_SeekbarColor.IsChecked )
            {
                pf.ProfileEnabledMember.SeekbarColor = true;
                pf.SeekbarColor = SeekbarColor.PickedColor;
            }

            /* ---------------------------------------------------- */
            // プロファイル名、タイプ
            /* ---------------------------------------------------- */
            pf.Name = Footer_ProfileName.Text;
            pf.ProfileType = ProfileType.User;

            return pf;
            // End of method
        }

        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
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
            Profile newProfile = CreateProfile();

            // プロファイル名重複確認
            bool conflict = false;
            if( ProfileList.Any( p => p.Name == newProfile.Name) )
            {
                switch( Mode )
                {
                    case ProfileEditDialogMode.New:
                        conflict = true;
                        break;
                    case ProfileEditDialogMode.Edit:
                        if( EditingProfile.Name == newProfile.Name ) conflict = false;
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
            if( ProfileList.Any( p => p.Name == newProfile.Name) )
            {
                //　上書き
                Profile oldProfile = ProfileList.FirstOrDefault( p => p.Name == newProfile.Name);
                if(oldProfile != null )
                {
                    int idx = ProfileList.IndexOf(oldProfile);
                    ProfileList.RemoveAt(idx);
                    ProfileList.Insert(idx, newProfile);
                }
            }
            else
            {
                // 追加
                this.ProfileList.Add(newProfile);
            }

            this.Close();
        }
    }
}
