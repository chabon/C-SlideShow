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
            get
            {
                string str = "パス: " + this.Value[0];
                if( this.Value.Count != 1 ) str += " 他" + (this.Value.Count - 1) + "件";
                return str;
            }
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
            get { return "ページ番号: " + (base.Value + 1); }
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
            get { return "列数・行数: " + this.Value[0] + "x" + this.Value[1]; }
        }

        public int Col { get { return Value[0]; } }
        public int Row { get { return Value[1]; } }
        public int Grid { get { return Col * Row; } }

        public static readonly int Min = 1;
        public static readonly int Max = 32;
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
            get { return "アスペクト比(横:縦): " + this.Value[0] + ":" + this.Value[1]; }
        }

        public int H { get { return this.Value[0]; } }
        public int V { get { return this.Value[1]; } }

        public static readonly int Min = 1;
        public static readonly int MaxH = 1000;
        public static readonly int MaxV = 99999;
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
            get { return "アスペクト比: " + (this.Value ? "非固定" : "固定"); }
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
            get { return "スライドショー設定: " + (this.Value == C_SlideShow.SlidePlayMethod.Continuous ? "常にスライド" : "一定時間待機してスライド"); }
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
            get { return "スライド速度: " + this.Value; }
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
            get { return "待機時間(sec): " + base.Value; }
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
            get
            {
                string dire = "";
                switch( this.Value )
                {
                    case C_SlideShow.SlideDirection.Left:
                        dire = "左";
                        break;
                    case C_SlideShow.SlideDirection.Top:
                        dire = "上";
                        break;
                    case C_SlideShow.SlideDirection.Right:
                        dire = "右";
                        break;
                    case C_SlideShow.SlideDirection.Bottom:
                        dire = "下";
                        break;
                }
                return "スライド方向: " + dire;
            }
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
            get { return "スライド時間(ms): " + base.Value; }
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
            get { return "画像一枚ずつスライド: " + (this.Value ? "させる" : "させない"); }
        }
    }

    public class SlideShowAutoStart : ProfileMemberBase
    {
        public SlideShowAutoStart()
        {
            this.Value = false;
            IsEnabled  = false;
        }

        public new bool Value { get; set; }

        public override string TooltipStr
        {
            get { return "自動でスライドショーを開始: " + (this.Value ? "する" : "しない"); }
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
            get
            {
                string method = "";
                switch( this.Value )
                {
                    case C_SlideShow.FileSortMethod.FileName:
                        method = "ファイル名(昇順)";
                        break;
                    case C_SlideShow.FileSortMethod.FileNameRev:
                        method = "ファイル名(降順)";
                        break;
                    case C_SlideShow.FileSortMethod.FileNameNatural:
                        method = "ファイル名 自然順(昇順)";
                        break;
                    case C_SlideShow.FileSortMethod.FileNameNaturalRev:
                        method = "ファイル名 自然順(降順)";
                        break;
                    case C_SlideShow.FileSortMethod.LastWriteTime:
                        method = "更新日時(昇順)";
                        break;
                    case C_SlideShow.FileSortMethod.LastWriteTimeRev:
                        method = "更新日時(降順)";
                        break;
                    case C_SlideShow.FileSortMethod.Random:
                        method = "ランダム";
                        break;
                    case C_SlideShow.FileSortMethod.None:
                        method = "指定しない";
                        break;
                }
                return "画像の並び順: " + method ;
            }
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
            get { return "最前面表示: " + (this.Value ? "する" : "しない"); }
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
            get { return "Exifの回転・反転情報: " + (this.Value ? "反映させる" : "反映させない"); }
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
            get { return "バックバッファのサイズ(ピクセル値): " + base.Value; }
        }

        public static readonly int Min = 320;
        public static readonly int Max = 10000;
    }


    /* ---------------------------------------------------- */
    //    その他の設定_配置
    /* ---------------------------------------------------- */
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
            get { return "画像の配置方法: " + (this.Value ? "スライド方法から自動で決定する" : "指定する"); }
        }
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
            get
            {
                string orig = "";
                switch( this.Value )
                {
                    case C_SlideShow.TileOrigin.TopLeft:
                        orig = "左上";
                        break;
                    case C_SlideShow.TileOrigin.TopRight:
                        orig = "右上";
                        break;
                    case C_SlideShow.TileOrigin.BottomRight:
                        orig = "右下";
                        break;
                    case C_SlideShow.TileOrigin.BottomLeft:
                        orig = "左下";
                        break;
                }
                return "配置する起点: " + orig;
            }
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
            get { return "配置方向: " + (this.Value == C_SlideShow.TileOrientation.Vertical ? "縦" : "横"); }
        }
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
            get { return "透過: " + (this.Value ? "有効" : "無効"); }
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
            get { return "不透明度(全体): " + (int)(this.Value * 100); }
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
            get { return "不透明度(背景): " + (int)(this.Value * 100); }
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
            get { return "背景色: " + this.Value.ToString().Remove(1,2); }
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
            get { return "チェック柄の背景に: " + (this.Value ? "する" : "しない"); }
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
            get { return "ペアとなる色: " + this.Value.ToString().Remove(1,2); }
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
            get { return "ウインドウ枠の太さ: " + this.Value; }
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
            get { return "ウインドウ枠の色: " + this.Value.ToString().Remove(1,2); }
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
            get { return "グリッド線の太さ: " + this.Value; }
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
            get { return "グリッド線の色: " + this.Value.ToString().Remove(1,2); }
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
            get { return "シークバーの色: " + this.Value.ToString().Remove(1,2); }
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
            get { return "ウインドウの位置: " + "X=" + this.X + " Y=" + this.Y; }
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
            get { return "ウインドウサイズ: " + "幅=" + this.Width + " 高さ=" + this.Height; }
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
            get { return "フルスクリーンに: " + (this.Value? "する" : "しない"); }
        }
    }
}
