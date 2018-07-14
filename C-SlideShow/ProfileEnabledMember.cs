using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows;

namespace C_SlideShow
{
    [DataContract(Name = "ProfileEnabledMember")]
    public class ProfileEnabledMember
    {
        // 行列設定
        [DataMember]
        public bool NumofMatrix { get; set; } = false;

        [DataMember]
        public bool TileOrigin { get; set; } = false;

        [DataMember]
        public bool TileOrientation { get; set; } = false;

        [DataMember]
        public bool UseDefaultTileOrigin { get; set; } = false;


        // アスペクト比設定
        [DataMember]
        public bool AspectRatio { get; set; } = false;

        [DataMember]
        public bool NonFixAspectRatio { get; set; } = false;


        // スライドの設定
        [DataMember]
        public bool SlidePlayMethod { get; set; } = false;

        [DataMember]
        public bool SlideSpeed { get; set; } = false;

        [DataMember]
        public bool SlideInterval { get; set; } = false;

        [DataMember]
        public bool SlideDirection { get; set; } = false;

        [DataMember]
        public bool SlideTimeInIntevalMethod { get; set; } = false;

        [DataMember]
        public bool SlideByOneImage { get; set; } = false;


        // その他の設定_全般
        [DataMember]
        public bool FileSortMethod { get; set; } = false;

        [DataMember]
        public bool TopMost { get; set; } = false;

        [DataMember]
        public bool StartUp_OpenPrevFolder { get; set; } = false;

        [DataMember]
        public bool ApplyRotateInfoFromExif { get; set; } = false;

        [DataMember]
        public bool BitmapDecodeTotalPixel { get; set; } = false;


        // その他の設定_外観1
        [DataMember]
        public bool AllowTransparency { get; set; } = false;

        [DataMember]
        public bool OverallOpacity { get; set; } = false;

        [DataMember]
        public bool BackgroundOpacity { get; set; } = false;

        [DataMember]
        public bool BaseGridBackgroundColor { get; set; } = false;

        [DataMember]
        public bool UsePlaidBackground { get; set; } = false;

        [DataMember]
        public bool PairColorOfPlaidBackground { get; set; } = false;


        // その他の設定_外観2
        [DataMember]
        public bool ResizeGripThickness { get; set; } = false;

        [DataMember]
        public bool ResizeGripColor { get; set; } = false;

        [DataMember]
        public bool TilePadding { get; set; } = false;

        [DataMember]
        public bool GridLineColor { get; set; } = false;

        [DataMember]
        public bool SeekbarColor { get; set; } = false;

        // ダイアログにはない設定
        [DataMember]
        public bool Path { get; set; } = false;

        [DataMember]
        public bool LastPageIndex { get; set; } = false;

        [DataMember]
        public bool WindowRect_Pos { get; set; } = false;

        [DataMember]
        public bool WindowRect_Size { get; set; } = false;

        [DataMember]
        public bool IsFullScreenMode { get; set; } = false;

    }
}
