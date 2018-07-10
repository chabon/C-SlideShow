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
        public int TilePadding { get; set; }

        [DataMember]
        public Color GridLineColor { get; set; }
        [DataMember]
        public double ResizeGripThickness { get; set; }

        [DataMember]
        public Color ResizeGripColor { get; set; }

        [DataMember]
        public Color SeekbarColor { get; set; }


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

        // ダイアログにはない設定
        [DataMember]
        public List<string> Path { get; set; }

        [DataMember]
        public int LastPageIndex { get; set; }

        [DataMember]
        public Rect WindowRect { get; set; }

        [DataMember]
        public bool IsFullScreenMode { get; set; }



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

            // その他の設定_全般
            FileSortMethod = FileSortMethod.FileName;
            TopMost = false;
            StartUp_OpenPrevFolder = true;
            ApplyRotateInfoFromExif = false;
            BitmapDecodeTotalPixel = 1920;

            // ダイアログにはない設定
            Path = new List<string>();
            LastPageIndex = 0;
            WindowRect = new Rect(50, 50, 640, 480);
            IsFullScreenMode = false;
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

    }
}
