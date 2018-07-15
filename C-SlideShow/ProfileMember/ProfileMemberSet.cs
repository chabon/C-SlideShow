using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows;

using System.Runtime.Serialization;

namespace C_SlideShow.ProfileMember
{
    /* ---------------------------------------------------- */
    // 読み込み
    /* ---------------------------------------------------- */
    public class Path : ProfileMemberBase
    {
        public Path()
        {
            this.Value = new List<string>();
            IsEnabled  = false;
        }

        public new List<string> Value { get; set; }

        public override string TooltipStr
        {
            get { return "Path: " + base.Value; }
        }

    }

    public class LastPageIndex : ProfileMemberBase
    {
        public LastPageIndex()
        {
            base.Value = 0;
            IsEnabled  = false;
        }

        public override string TooltipStr
        {
            get { return "LastPageIndex: " + base.Value; }
        }

        public static readonly int Min = 0;
        public static readonly int Max = int.MaxValue;
    }
    /* ---------------------------------------------------- */
    // 行列設定
    /* ---------------------------------------------------- */
    public class NumofMatrix : ProfileMemberBase
    {
        public NumofMatrix()
        {
            this.Value = new int[2] { 2, 2 }; // col, row
            IsEnabled  = false;
        }

        public new int[] Value { get; set; }

        public override string TooltipStr
        {
            get { return "列数・行数: " + base.Value; }
        }

        public int Col { get { return Value[0]; } }
        public int Row { get { return Value[1]; } }
        public int Grid { get { return Col * Row; } }

        public static readonly int Min = 1;
        public static readonly int Max = 32;
    }

    public class TileOrigin : ProfileMemberBase
    {
        public TileOrigin()
        {
            this.Value = C_SlideShow.TileOrigin.TopLeft;
            IsEnabled  = false;
        }

        public new C_SlideShow.TileOrigin Value { get; set; }

        public override string TooltipStr
        {
            get { return "TileOrigin: " + base.Value; }
        }
    }

    public class TileOrientation : ProfileMemberBase
    {
        public TileOrientation()
        {
            this.Value = C_SlideShow.TileOrientation.Vertical;
            IsEnabled  = false;
        }

        public new C_SlideShow.TileOrientation Value { get; set; }

        public override string TooltipStr
        {
            get { return "TileOrientation: " + base.Value; }
        }
    }

    public class UseDefaultTileOrigin : ProfileMemberBase
    {
        public UseDefaultTileOrigin()
        {
            this.Value = true;
            IsEnabled  = false;
        }

        public new bool Value { get; set; }

        public override string TooltipStr
        {
            get { return "UseDefaultTileOrigin: " + base.Value; }
        }
    }

    /* ---------------------------------------------------- */
    // アスペクト比設定
    /* ---------------------------------------------------- */
    public class AspectRatio : ProfileMemberBase
    {
        public AspectRatio()
        {
            this.Value = new int[2] { 4, 3 }; // 横, 縦
            IsEnabled  = false;
        }

        public new int[] Value { get; set; }

        public override string TooltipStr
        {
            get { return "AspectRatio: " + base.Value; }
        }

        public int H { get { return this.Value[0]; } }
        public int V { get { return this.Value[1]; } }

        public static readonly int Min = 1;
        public static readonly int Max = 100000;
    }

    public class NonFixAspectRatio : ProfileMemberBase
    {
        public NonFixAspectRatio()
        {
            this.Value = false;
            IsEnabled  = false;
        }

        public new bool Value { get; set; }

        public override string TooltipStr
        {
            get { return "NonFixAspectRatio: " + base.Value; }
        }
    }

    /* ---------------------------------------------------- */
    // スライド設定
    /* ---------------------------------------------------- */
    public class SlidePlayMethod : ProfileMemberBase
    {
        public SlidePlayMethod()
        {
            this.Value = C_SlideShow.SlidePlayMethod.Continuous;
            IsEnabled  = false;
        }

        public new C_SlideShow.SlidePlayMethod Value { get; set; }

        public override string TooltipStr
        {
            get { return "SlidePlayMethod: " + base.Value; }
        }
    }

    public class SlideSpeed : ProfileMemberBase
    {
        public SlideSpeed()
        {
            this.Value = (double)30;
            IsEnabled  = false;
        }

        public new double Value { get; set; }

        public override string TooltipStr
        {
            get { return "速度: " + base.Value; }
        }

        public static readonly int Min = 1;
        public static readonly int Max = 100;
    }

    public class SlideInterval : ProfileMemberBase
    {
        public SlideInterval()
        {
            base.Value = 5;
            IsEnabled  = false;
        }

        public override string TooltipStr
        {
            get { return "SlideInterval: " + base.Value; }
        }

        public static readonly int Min = 1;
        public static readonly int Max = 100000;
    }

    public class SlideDirection : ProfileMemberBase
    {
        public SlideDirection()
        {
            this.Value = C_SlideShow.SlideDirection.Left;
            IsEnabled  = false;
        }

        public new C_SlideShow.SlideDirection Value { get; set; }

        public override string TooltipStr
        {
            get { return "SlideDirection: " + base.Value; }
        }
    }


    public class SlideTimeInIntevalMethod : ProfileMemberBase
    {
        public SlideTimeInIntevalMethod()
        {
            base.Value = 1000;
            IsEnabled  = false;
        }

        public override string TooltipStr
        {
            get { return "SlideTimeInIntevalMethod: " + base.Value; }
        }

        public static readonly int Min = 100;
        public static readonly int Max = 100000;
    }

    public class SlideByOneImage : ProfileMemberBase
    {
        public SlideByOneImage()
        {
            this.Value = false;
            IsEnabled  = false;
        }

        public new bool Value { get; set; }

        public override string TooltipStr
        {
            get { return "SlideByOneImage: " + base.Value; }
        }
    }

    /* ---------------------------------------------------- */
    //     その他の設定_全般
    /* ---------------------------------------------------- */
    public class FileSortMethod : ProfileMemberBase
    {
        public FileSortMethod()
        {
            this.Value = C_SlideShow.FileSortMethod.FileName;
            IsEnabled  = false;
        }

        public new C_SlideShow.FileSortMethod Value { get; set; }

        public override string TooltipStr
        {
            get { return "FileSortMethod: " + base.Value; }
        }
    }

    public class TopMost : ProfileMemberBase
    {
        public TopMost()
        {
            this.Value = false;
            IsEnabled  = false;
        }

        public new bool Value { get; set; }

        public override string TooltipStr
        {
            get { return "TopMost: " + base.Value; }
        }
    }

    public class OpenPrevFolderOnStartUp : ProfileMemberBase
    {
        public OpenPrevFolderOnStartUp()
        {
            this.Value = true;
            IsEnabled  = false;
        }

        public new bool Value { get; set; }

        public override string TooltipStr
        {
            get { return "OpenPrevFolderOnStartUp: " + base.Value; }
        }
    }

    public class ApplyRotateInfoFromExif : ProfileMemberBase
    {
        public ApplyRotateInfoFromExif()
        {
            this.Value = false;
            IsEnabled  = false;
        }

        public new bool Value { get; set; }

        public override string TooltipStr
        {
            get { return "ApplyRotateInfoFromExif: " + base.Value; }
        }
    }

    public class BitmapDecodeTotalPixel : ProfileMemberBase
    {
        public BitmapDecodeTotalPixel()
        {
            base.Value = 1920;
            IsEnabled  = false;
        }

        public override string TooltipStr
        {
            get { return "BitmapDecodeTotalPixel: " + base.Value; }
        }

        public static readonly int Min = 320;
        public static readonly int Max = 10000;
    }

    /* ---------------------------------------------------- */
    //    その他の設定_外観1
    /* ---------------------------------------------------- */
    public class AllowTransparency : ProfileMemberBase
    {
        public AllowTransparency()
        {
            this.Value = false;
            IsEnabled  = false;
        }

        public new bool Value { get; set; }

        public override string TooltipStr
        {
            get { return "AllowTransparency: " + base.Value; }
        }
    }

    public class OverallOpacity : ProfileMemberBase
    {
        public OverallOpacity()
        {
            this.Value = 0.5;
            IsEnabled  = false;
        }

        public new double Value { get; set; }

        public override string TooltipStr
        {
            get { return "OverallOpacity: " + base.Value; }
        }

        public static readonly double Min = 0.005;
        public static readonly double Max = 1.0;
    }

    public class BackgroundOpacity : ProfileMemberBase
    {
        public BackgroundOpacity()
        {
            this.Value = 1.0;
            IsEnabled  = false;
        }

        public new double Value { get; set; }

        public override string TooltipStr
        {
            get { return "BackgroundOpacity: " + base.Value; }
        }

        public static readonly double Min = 0.005;
        public static readonly double Max = 1.0;
    }

    public class BaseGridBackgroundColor : ProfileMemberBase
    {
        public BaseGridBackgroundColor()
        {
            this.Value = Colors.White;
            IsEnabled  = false;
        }

        public new Color Value { get; set; }

        public override string TooltipStr
        {
            get { return "BaseGridBackgroundColor: " + base.Value; }
        }

    }

    public class UsePlaidBackground : ProfileMemberBase
    {
        public UsePlaidBackground()
        {
            this.Value = false;
            IsEnabled  = false;
        }

        public new bool Value { get; set; }

        public override string TooltipStr
        {
            get { return "UsePlaidBackground: " + base.Value; }
        }
    }

    public class PairColorOfPlaidBackground : ProfileMemberBase
    {
        public PairColorOfPlaidBackground()
        {
            this.Value = Colors.LightGray;
            IsEnabled  = false;
        }

        public new Color Value { get; set; }

        public override string TooltipStr
        {
            get { return "PairColorOfPlaidBackground: " + base.Value; }
        }
    }

    /* ---------------------------------------------------- */
    //     その他の設定_外観2
    /* ---------------------------------------------------- */
    public class ResizeGripThickness : ProfileMemberBase
    {
        public ResizeGripThickness()
        {
            this.Value = (double)5;
            IsEnabled  = false;
        }

        public new double Value { get; set; }

        public override string TooltipStr
        {
            get { return "ResizeGripThickness: " + base.Value; }
        }

        public static readonly int Min = 0;
        public static readonly int Max = 100;
    }

    public class ResizeGripColor : ProfileMemberBase
    {
        public ResizeGripColor()
        {
            this.Value = Colors.Gray;
            IsEnabled  = false;
        }

        public new Color Value { get; set; }

        public override string TooltipStr
        {
            get { return "ResizeGripColor: " + base.Value; }
        }
    }

    public class TilePadding : ProfileMemberBase
    {
        public TilePadding()
        {
            base.Value = 3;
            IsEnabled  = false;
        }

        public override string TooltipStr
        {
            get { return "TilePadding: " + base.Value; }
        }

        public static readonly int Min = 0;
        public static readonly int Max = 10000;
    }

    public class GridLineColor : ProfileMemberBase
    {
        public GridLineColor()
        {
            this.Value = Colors.LightGray;
            IsEnabled  = false;
        }

        public new Color Value { get; set; }

        public override string TooltipStr
        {
            get { return "GridLineColor: " + base.Value; }
        }
    }

    public class SeekbarColor : ProfileMemberBase
    {
        public SeekbarColor()
        {
            this.Value = Colors.Black;
            IsEnabled  = false;
        }

        public new Color Value { get; set; }

        public override string TooltipStr
        {
            get { return "SeekbarColor: " + base.Value; }
        }
    }

    /* ---------------------------------------------------- */
    //     ダイアログにはない設定
    /* ---------------------------------------------------- */
    public class WindowPos : ProfileMemberBase
    {
        public WindowPos()
        {
            this.Value = new Point( 50, 50 ); // X, Y
            IsEnabled  = false;
        }

        public new Point Value { get; set; }

        public override string TooltipStr
        {
            get { return "WindowPos: " + base.Value; }
        }

        public double X { get { return Value.X; } }
        public double Y { get { return Value.Y; } }

        public static readonly int Min = int.MinValue;
        public static readonly int Max = int.MaxValue;
    }

    public class WindowSize : ProfileMemberBase
    {
        public WindowSize()
        {
            this.Value = new Size(640, 480);
            IsEnabled  = false;
        }

        public new Size Value { get; set; }

        public override string TooltipStr
        {
            get { return "WindowSize: " + base.Value; }
        }

        public double Width { get { return Value.Width; } }
        public double Height { get { return Value.Height; } }

        public static readonly int Min = 100;
        public static readonly int Max = int.MaxValue;
    }

    public class IsFullScreenMode : ProfileMemberBase
    {
        public IsFullScreenMode()
        {
            this.Value = false;
            IsEnabled  = false;
        }

        public new bool Value { get; set; }

        public override string TooltipStr
        {
            get { return "IsFullScreenMode: " + base.Value; }
        }
    }
}
