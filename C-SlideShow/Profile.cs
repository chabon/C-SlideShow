using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows;


namespace C_SlideShow
{
    public enum SlideDirection
    {
        Left, Top, Right, Bottom
    }

    public enum TileOrigin
    {
        TopLeft, TopRight, BottomLeft, BottomRight
    }

    public enum TileOrientation
    {
        Horizontal, Vertical
    }

    public enum SlidePlayMethod
    {
        Continuous, Interval
    }

    public enum FileSortMethod
    {
        FileName, FileNameRev, FileNameNatural, FileNameNaturalRev,
        LastWriteTime, LastWriteTimeRev, Random, None
    }

    public enum ProfileType
    {
        Temp, Default, Preset, User
    }

    public class ProfileMemberProp
    {
        public double Min = int.MinValue;
        public double Max = int.MaxValue;
    }

    [DataContract(Name = "Profile")]
    public class Profile
    {
        // ダイアログ未実装


        // 行列設定
        [DataMember]
        public int NumofRow { get; set; }

        [DataMember]
        public int NumofCol { get; set; }

        [DataMember]
        public TileOrigin TileOrigin { get; set; }

        [DataMember]
        public TileOrientation TileOrientation { get; set; }

        [DataMember]
        public bool UseDefaultTileOrigin { get; set; }


        // アスペクト比設定
        [DataMember]
        public int AspectRatioH { get; set; }

        [DataMember]
        public int AspectRatioV { get; set; }

        [DataMember]
        public bool NonFixAspectRatio { get; set; }


        // スライドの設定
        [DataMember]
        public SlidePlayMethod SlidePlayMethod { get; set; }

        [DataMember]
        public double SlideSpeed { get; set; }

        [DataMember]
        public int SlideInterval { get; set; }

        [DataMember]
        public SlideDirection SlideDirection { get; set; }

        [DataMember]
        public int SlideTimeInIntevalMethod { get; set; }

        [DataMember]
        public bool SlideByOneImage { get; set; }


        // その他の設定_全般
        [DataMember]
        public FileSortMethod FileSortMethod { get; set; }

        [DataMember]
        public bool TopMost { get; set; }

        [DataMember]
        public bool StartUp_OpenPrevFolder { get; set; }

        [DataMember]
        public bool ApplyRotateInfoFromExif { get; set; }

        [DataMember]
        public int BitmapDecodeTotalPixel { get; set; }


        // その他の設定_外観1
        [DataMember]
        public bool AllowTransparency { get; set; }

        [DataMember]
        public double OverallOpacity { get; set; }

        [DataMember]
        public double BackgroundOpacity { get; set; }

        [DataMember]
        public Color BaseGridBackgroundColor { get; set; }

        [DataMember]
        public bool UsePlaidBackground { get; set; }

        [DataMember]
        public Color PairColorOfPlaidBackground { get; set; }


        // その他の設定_外観2
        [DataMember]
        public double ResizeGripThickness { get; set; }

        [DataMember]
        public Color ResizeGripColor { get; set; }

        [DataMember]
        public int TilePadding { get; set; }

        [DataMember]
        public Color GridLineColor { get; set; }

        [DataMember]
        public Color SeekbarColor { get; set; }

        // ダイアログにはない設定
        [DataMember]
        public List<string> Path { get; set; }

        [DataMember]
        public int LastPageIndex { get; set; }

        [DataMember]
        public Rect WindowRect { get; set; }

        [DataMember]
        public bool IsFullScreenMode { get; set; }


        // 有効なメンバ
        [DataMember]
        public ProfileEnabledMember ProfileEnabledMember { get; set; }

        // タイプ
        [DataMember]
        public ProfileType ProfileType { get; set; }

        // プロファイル名
        [DataMember]
        public string Name { get; set; }


        public Profile()
        {
            // ダイアログ未実装


            // アスペクト比設定
            AspectRatioH = 4;
            AspectRatioV = 3;
            NonFixAspectRatio = false;

            // 行列設定
            NumofRow = 2;
            NumofCol = 2;
            TileOrigin = TileOrigin.TopLeft;
            TileOrientation = TileOrientation.Vertical;
            UseDefaultTileOrigin = true;

            // スライドの設定
            SlidePlayMethod = SlidePlayMethod.Continuous;
            SlideSpeed = 30;
            SlideInterval = 5;
            SlideTimeInIntevalMethod = 1000;
            SlideByOneImage = false;
            SlideDirection = SlideDirection.Left;

            // その他の設定_全般
            FileSortMethod = FileSortMethod.FileName;
            TopMost = false;
            StartUp_OpenPrevFolder = true;
            ApplyRotateInfoFromExif = false;
            BitmapDecodeTotalPixel = 1920;

            // その他の設定_外観1
            AllowTransparency = false;
            OverallOpacity = 0.5;
            BackgroundOpacity = 1.0;
            BaseGridBackgroundColor = Colors.White;
            UsePlaidBackground = false;
            PairColorOfPlaidBackground = Colors.LightGray;

            // その他の設定_外観2
            ResizeGripThickness = 5;
            ResizeGripColor = Colors.Gray;
            SeekbarColor = Colors.Black;
            TilePadding = 3;
            GridLineColor = Colors.LightGray;

            // ダイアログにはない設定
            Path = new List<string>();
            LastPageIndex = 0;
            WindowRect = new Rect(50, 50, 640, 480);
            IsFullScreenMode = false;

            // 有効なメンバ
            ProfileEnabledMember = new ProfileEnabledMember();

            // タイプ
            ProfileType = ProfileType.Temp;

            // プロファイル名
            Name = "NoName";
        }

        // 既定値(xmlファイルデシリアライズ時に呼ばれる)
        [OnDeserializing]
        public void DefaultDeserializing(StreamingContext sc)
        {
            //WindowRect = new Rect(50, 50, 400, 300);
            //PairColorOfPlaidBackground = Colors.Gray;
            AspectRatioH = 4;
            AspectRatioV = 3;
            Path = new List<string>();
        }

        // 最大値・最小値を取得
        public static ProfileMemberProp GetProfileMemberProp(string memberName)
        {
            ProfileMemberProp prop = new ProfileMemberProp();
            switch( memberName )
            {
                // 行列設定
                case nameof(NumofRow):
                case nameof(NumofCol):
                    prop.Min = 1;
                    prop.Max = 32;
                    break;
                // アスペクト比設定
                case nameof(AspectRatioH):
                    prop.Min = 1;
                    prop.Max = 100000;
                    break;
                // スライド設定
                case nameof(SlideSpeed):
                    prop.Min = 1;
                    prop.Max = 100;
                    break;
                case nameof(SlideInterval):
                    prop.Min = 1;
                    prop.Max = 100000;
                    break;
                case nameof(SlideTimeInIntevalMethod):
                    prop.Min = 100;
                    prop.Max = 100000;
                    break;
                // その他の設定 全般
                case nameof(BitmapDecodeTotalPixel):
                    prop.Min = 320;
                    prop.Max = 10000;
                    break;
                // その他の設定 外観1
                case nameof(OverallOpacity):
                case nameof(BackgroundOpacity):
                    prop.Min = 0.005;
                    prop.Max = 1.0;
                    break;
                // その他の設定 外観2
                case nameof(ResizeGripThickness):
                    prop.Min = 0;
                    prop.Max = 100;
                    break;
                case nameof(TilePadding):
                    prop.Min = 0;
                    prop.Max = 10000;
                    break;
                // ダイアログには無い設定
                case nameof(LastPageIndex):
                    prop.Min = 0;
                    break;
            }

            return prop;
        }

        /// <summary>
        /// 他のプロファイルを統合(有効なメンバが衝突したら、他のプロファイルを優先)
        /// </summary>
        /// <param name="pf">統合するプロファイル</param>
        public void Marge(Profile pf)
        {
            /* ---------------------------------------------------- */
            // ウインドウの状態
            /* ---------------------------------------------------- */
            // ウインドウの位置
            if( pf.ProfileEnabledMember.WindowRect_Pos )
            {
                this.WindowRect = new Rect(pf.WindowRect.Left, pf.WindowRect.Top, this.WindowRect.Width, this.WindowRect.Height);
            }

            // ウインドウサイズ
            if( pf.ProfileEnabledMember.WindowRect_Size )
            {
                this.WindowRect = new Rect(this.WindowRect.Left, this.WindowRect.Top, pf.WindowRect.Width, pf.WindowRect.Height);
            }

            // フルスクリーン
            if( pf.ProfileEnabledMember.IsFullScreenMode )
            {
                this.IsFullScreenMode = pf.IsFullScreenMode;
            }

            /* ---------------------------------------------------- */
            // 読み込み
            /* ---------------------------------------------------- */
            // ファイル・フォルダ
            if( pf.ProfileEnabledMember.Path )
            {
                this.Path = pf.Path;
            }

            // ページ番号
            if( pf.ProfileEnabledMember.LastPageIndex )
            {
                this.LastPageIndex = pf.LastPageIndex;
            }

            /* ---------------------------------------------------- */
            // 列数・行数
            /* ---------------------------------------------------- */
            if( pf.ProfileEnabledMember.NumofMatrix )
            {
                this.NumofCol = pf.NumofCol;
                this.NumofRow = pf.NumofRow;
            }

            /* ---------------------------------------------------- */
            // グリッドのアスペクト比
            /* ---------------------------------------------------- */
            // アスペクト比を固定
            if( pf.ProfileEnabledMember.NonFixAspectRatio )
            {
                this.NonFixAspectRatio = pf.NonFixAspectRatio;
            }

            // アスペクト比
            if( pf.ProfileEnabledMember.AspectRatio )
            {
                this.AspectRatioH = pf.AspectRatioH;
                this.AspectRatioV = pf.AspectRatioV;
            }

            /* ---------------------------------------------------- */
            // スライド
            /* ---------------------------------------------------- */
            // スライドショー設定
            if( pf.ProfileEnabledMember.SlidePlayMethod )
            {
                this.SlidePlayMethod = pf.SlidePlayMethod;
            }

            // スライド方向
            if( pf.ProfileEnabledMember.SlideDirection )
            {
                this.SlideDirection = pf.SlideDirection;
            }

            /* ---------------------------------------------------- */
            // スライドショー詳細
            /* ---------------------------------------------------- */
            // 速度
            if( pf.ProfileEnabledMember.SlideSpeed )
            {
                this.SlideSpeed = pf.SlideSpeed;
            }

            // 待機時間(sec)
            if( pf.ProfileEnabledMember.SlideInterval )
            {
                this.SlideInterval = pf.SlideInterval;
            }

            // スライド時間(ms)
            if( pf.ProfileEnabledMember.SlideTimeInIntevalMethod )
            {
                this.SlideTimeInIntevalMethod = pf.SlideTimeInIntevalMethod;
            }

            // 画像一枚ずつスライド
            if( pf.ProfileEnabledMember.SlideByOneImage )
            {
                this.SlideByOneImage = pf.SlideByOneImage;
            }

            // [todo] 自動再生

            /* ---------------------------------------------------- */
            // その他/全般
            /* ---------------------------------------------------- */
            // 最前面表示
            if( pf.ProfileEnabledMember.TopMost )
            {
                this.TopMost = pf.TopMost;
            }

            // 画像の並び順
            if( pf.ProfileEnabledMember.FileSortMethod )
            {
                this.FileSortMethod = pf.FileSortMethod;
            }

            // Exifの回転・反転情報
            if( pf.ProfileEnabledMember.ApplyRotateInfoFromExif )
            {
                this.ApplyRotateInfoFromExif = pf.ApplyRotateInfoFromExif;
            }

            // バックバッファのサイズ(ピクセル値)
            if( pf.ProfileEnabledMember.BitmapDecodeTotalPixel )
            {
                this.BitmapDecodeTotalPixel = pf.BitmapDecodeTotalPixel;
            }

            /* ---------------------------------------------------- */
            // その他/外観1
            /* ---------------------------------------------------- */
            // 透過
            if( pf.ProfileEnabledMember.AllowTransparency )
            {
                this.AllowTransparency = pf.AllowTransparency;
            }

            // 不透明度(全体)
            if( pf.ProfileEnabledMember.OverallOpacity )
            {
                this.OverallOpacity = pf.OverallOpacity;
            }

            // 不透明度(背景)
            if( pf.ProfileEnabledMember.BackgroundOpacity )
            {
                this.BackgroundOpacity = pf.BackgroundOpacity;
            }

            // 背景色
            if( pf.ProfileEnabledMember.BaseGridBackgroundColor )
            {
                this.BaseGridBackgroundColor = pf.BaseGridBackgroundColor;
            }

            // チェック柄の背景
            if( pf.ProfileEnabledMember.UsePlaidBackground )
            {
                this.UsePlaidBackground = pf.UsePlaidBackground;
            }

            // ペアとなる背景色
            if( pf.ProfileEnabledMember.PairColorOfPlaidBackground )
            {
                this.PairColorOfPlaidBackground = pf.PairColorOfPlaidBackground;
            }

            /* ---------------------------------------------------- */
            // その他/外観2
            /* ---------------------------------------------------- */
            // ウインドウ枠の太さ
            if( pf.ProfileEnabledMember.ResizeGripThickness )
            {
                this.ResizeGripThickness = pf.ResizeGripThickness;
            }

            // ウインドウ枠の色
            if( pf.ProfileEnabledMember.ResizeGripColor )
            {
                this.ResizeGripColor = pf.ResizeGripColor;
            }

            // グリッド線の太さ
            if( pf.ProfileEnabledMember.TilePadding )
            {
                this.TilePadding = pf.TilePadding;
            }

            // グリッド線の色
            if( pf.ProfileEnabledMember.GridLineColor )
            {
                this.GridLineColor = pf.GridLineColor;
            }

            // シークバーの色
            if( pf.ProfileEnabledMember.SeekbarColor )
            {
                this.SeekbarColor = pf.SeekbarColor;
            }
        }


        public string CreateProfileToolTip()
        {
            return "tool tip";
        }
    }
}
