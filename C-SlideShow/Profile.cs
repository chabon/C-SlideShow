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


    [DataContract(Name = "Profile")]
    public class Profile
    {
        /* ---------------------------------------------------- */
        //     Profile data
        /* ---------------------------------------------------- */
        // タイプ
        [DataMember]
        public ProfileType ProfileType { get; set; }

        // プロファイル名
        [DataMember]
        public string Name { get; set; }

        /* ---------------------------------------------------- */
        //     Member
        /* ---------------------------------------------------- */
        // ダイアログ未実装


        // 読み込み
        [DataMember]
        public ProfileMember.Path Path { get; set; } = new ProfileMember.Path();

        [DataMember]
        public ProfileMember.LastPageIndex LastPageIndex { get; set; } = new ProfileMember.LastPageIndex();

        // 行列設定
        [DataMember]
        public ProfileMember.NumofMatrix NumofMatrix { get; set; } = new ProfileMember.NumofMatrix();

        [DataMember]
        public ProfileMember.TileOrigin TileOrigin { get; set; } = new ProfileMember.TileOrigin();

        [DataMember]
        public ProfileMember.TileOrientation TileOrientation { get; set; } = new ProfileMember.TileOrientation();

        [DataMember]
        public ProfileMember.UseDefaultTileOrigin UseDefaultTileOrigin { get; set; } = new ProfileMember.UseDefaultTileOrigin();


        // アスペクト比設定
        [DataMember]
        public ProfileMember.AspectRatio AspectRatio { get; set; } = new ProfileMember.AspectRatio();

        [DataMember]
        public ProfileMember.NonFixAspectRatio NonFixAspectRatio { get; set; } = new ProfileMember.NonFixAspectRatio();


        // スライドの設定
        [DataMember]
        public ProfileMember.SlidePlayMethod SlidePlayMethod { get; set; } = new ProfileMember.SlidePlayMethod();

        [DataMember]
        public ProfileMember.SlideSpeed SlideSpeed { get; set; } = new ProfileMember.SlideSpeed();

        [DataMember]
        public ProfileMember.SlideInterval SlideInterval { get; set; } = new ProfileMember.SlideInterval();

        [DataMember]
        public ProfileMember.SlideDirection SlideDirection { get; set; } = new ProfileMember.SlideDirection();

        [DataMember]
        public ProfileMember.SlideTimeInIntevalMethod SlideTimeInIntevalMethod { get; set; } = new ProfileMember.SlideTimeInIntevalMethod();

        [DataMember]
        public ProfileMember.SlideByOneImage SlideByOneImage { get; set; } = new ProfileMember.SlideByOneImage();


        // その他の設定_全般
        [DataMember]
        public ProfileMember.FileSortMethod FileSortMethod { get; set; } = new ProfileMember.FileSortMethod();

        [DataMember]
        public ProfileMember.TopMost TopMost { get; set; } = new ProfileMember.TopMost();

        [DataMember]
        public ProfileMember.OpenPrevFolderOnStartUp OpenPrevFolderOnStartUp { get; set; } = new ProfileMember.OpenPrevFolderOnStartUp();

        [DataMember]
        public ProfileMember.ApplyRotateInfoFromExif ApplyRotateInfoFromExif { get; set; } = new ProfileMember.ApplyRotateInfoFromExif();

        [DataMember]
        public ProfileMember.BitmapDecodeTotalPixel BitmapDecodeTotalPixel { get; set; } = new ProfileMember.BitmapDecodeTotalPixel();


        // その他の設定_外観1
        [DataMember]
        public ProfileMember.AllowTransparency AllowTransparency { get; set; } = new ProfileMember.AllowTransparency();

        [DataMember]
        public ProfileMember.OverallOpacity OverallOpacity { get; set; } = new ProfileMember.OverallOpacity();

        [DataMember]
        public ProfileMember.BackgroundOpacity BackgroundOpacity { get; set; } = new ProfileMember.BackgroundOpacity();

        [DataMember]
        public ProfileMember.BaseGridBackgroundColor BaseGridBackgroundColor { get; set; } = new ProfileMember.BaseGridBackgroundColor();

        [DataMember]
        public ProfileMember.UsePlaidBackground UsePlaidBackground { get; set; } = new ProfileMember.UsePlaidBackground();

        [DataMember]
        public ProfileMember.PairColorOfPlaidBackground PairColorOfPlaidBackground { get; set; } = new ProfileMember.PairColorOfPlaidBackground();


        // その他の設定_外観2
        [DataMember]
        public ProfileMember.ResizeGripThickness ResizeGripThickness { get; set; } = new ProfileMember.ResizeGripThickness();

        [DataMember]
        public ProfileMember.ResizeGripColor ResizeGripColor { get; set; } = new ProfileMember.ResizeGripColor();

        [DataMember]
        public ProfileMember.TilePadding TilePadding { get; set; } = new ProfileMember.TilePadding();

        [DataMember]
        public ProfileMember.GridLineColor GridLineColor { get; set; } = new ProfileMember.GridLineColor();

        [DataMember]
        public ProfileMember.SeekbarColor SeekbarColor { get; set; } = new ProfileMember.SeekbarColor();


        // ダイアログにはない設定
        [DataMember]
        public ProfileMember.WindowPos WindowPos { get; set; } = new ProfileMember.WindowPos();

        [DataMember]
        public ProfileMember.WindowSize WindowSize { get; set; } = new ProfileMember.WindowSize();

        [DataMember]
        public ProfileMember.IsFullScreenMode IsFullScreenMode { get; set; } = new ProfileMember.IsFullScreenMode();

        public Profile()
        {
            // タイプ
            ProfileType = ProfileType.Temp;

            // プロファイル名
            Name = "NoName";
        }

        // 既定値(xmlファイルデシリアライズ時に呼ばれる)
        [OnDeserializing]
        public void DefaultDeserializing(StreamingContext sc)
        {

        }


        /// <summary>
        /// 他のプロファイルを統合(有効なメンバが衝突したら、他のプロファイルを優先)
        /// </summary>
        /// <param name="pf">統合するプロファイル</param>
        public void Marge(Profile pf)
        {
            // 読み込み
            if( pf.Path.IsEnabled ) Path.Value = new List<string>(pf.Path.Value);
            if( pf.LastPageIndex.IsEnabled ) LastPageIndex.Value = pf.LastPageIndex.Value;

            // 行列設定
            if( pf.NumofMatrix.IsEnabled ) Array.Copy(pf.NumofMatrix.Value, NumofMatrix.Value, 2);
            if( pf.TileOrigin.IsEnabled ) TileOrigin.Value = pf.TileOrigin.Value;
            if( pf.TileOrientation.IsEnabled ) TileOrientation.Value = pf.TileOrientation.Value;
            if( pf.UseDefaultTileOrigin.IsEnabled ) UseDefaultTileOrigin.Value = pf.UseDefaultTileOrigin.Value;

            // アスペクト比設定
            if( pf.AspectRatio.IsEnabled ) Array.Copy(pf.AspectRatio.Value, AspectRatio.Value, 2);
            if( pf.NonFixAspectRatio.IsEnabled ) NonFixAspectRatio.Value = pf.NonFixAspectRatio.Value;

            // スライドの設定
            if( pf.SlidePlayMethod.IsEnabled ) SlidePlayMethod.Value = pf.SlidePlayMethod.Value;
            if( pf.SlideSpeed.IsEnabled ) SlideSpeed.Value = pf.SlideSpeed.Value;
            if( pf.SlideInterval.IsEnabled ) SlideInterval.Value = pf.SlideInterval.Value;
            if( pf.SlideDirection.IsEnabled ) SlideDirection.Value = pf.SlideDirection.Value;
            if( pf.SlideTimeInIntevalMethod.IsEnabled ) SlideTimeInIntevalMethod.Value = pf.SlideTimeInIntevalMethod.Value;
            if( pf.SlideByOneImage.IsEnabled ) SlideByOneImage.Value = pf.SlideByOneImage.Value;

            // その他の設定_全般
            if( pf.FileSortMethod.IsEnabled ) FileSortMethod.Value = pf.FileSortMethod.Value;
            if( pf.TopMost.IsEnabled ) TopMost.Value = pf.TopMost.Value;
            if( pf.OpenPrevFolderOnStartUp.IsEnabled ) OpenPrevFolderOnStartUp.Value = pf.OpenPrevFolderOnStartUp.Value;
            if( pf.ApplyRotateInfoFromExif.IsEnabled ) ApplyRotateInfoFromExif.Value = pf.ApplyRotateInfoFromExif.Value;
            if( pf.BitmapDecodeTotalPixel.IsEnabled ) BitmapDecodeTotalPixel.Value = pf.BitmapDecodeTotalPixel.Value;

            // その他の設定_外観1
            if( pf.AllowTransparency.IsEnabled ) AllowTransparency.Value = pf.AllowTransparency.Value;
            if( pf.OverallOpacity.IsEnabled ) OverallOpacity.Value = pf.OverallOpacity.Value;
            if( pf.BackgroundOpacity.IsEnabled ) BackgroundOpacity.Value = pf.BackgroundOpacity.Value;
            if( pf.BaseGridBackgroundColor.IsEnabled ) BaseGridBackgroundColor.Value = pf.BaseGridBackgroundColor.Value;
            if( pf.UsePlaidBackground.IsEnabled ) UsePlaidBackground.Value = pf.UsePlaidBackground.Value;
            if( pf.PairColorOfPlaidBackground.IsEnabled ) PairColorOfPlaidBackground.Value = pf.PairColorOfPlaidBackground.Value;

            // その他の設定_外観2
            if( pf.ResizeGripThickness.IsEnabled ) ResizeGripThickness.Value = pf.ResizeGripThickness.Value;
            if( pf.ResizeGripColor.IsEnabled ) ResizeGripColor.Value = pf.ResizeGripColor.Value;
            if( pf.TilePadding.IsEnabled ) TilePadding.Value = pf.TilePadding.Value;
            if( pf.GridLineColor.IsEnabled ) GridLineColor.Value = pf.GridLineColor.Value;
            if( pf.SeekbarColor.IsEnabled ) SeekbarColor.Value = pf.SeekbarColor.Value;

            // ダイアログにはない設定
            if( pf.WindowPos.IsEnabled ) WindowPos.Value = new Point(pf.WindowPos.Value.X, pf.WindowPos.Y);
            if( pf.WindowSize.IsEnabled ) WindowSize.Value = new Size(pf.WindowSize.Value.Width, pf.WindowSize.Height);
            if( pf.IsFullScreenMode.IsEnabled ) IsFullScreenMode.Value = pf.IsFullScreenMode.Value;
        }


        public string CreateProfileToolTip()
        {
            return "tool tip";
        }
    }
}
