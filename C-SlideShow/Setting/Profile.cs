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
        Left, Top, Right, Bottom, None
    }

    public enum TileOrigin
    {
        TopLeft, TopRight, BottomRight, BottomLeft
    }

    public enum TileOrientation
    {
        Horizontal, Vertical
    }

    public enum TileImageStretch
    {
        Uniform, UniformToFill, Fill
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

        // ダイアログにはない設定
        [DataMember]
        public ProfileMember.WindowPos WindowPos { get; set; } = new ProfileMember.WindowPos();

        [DataMember]
        public ProfileMember.WindowSize WindowSize { get; set; } = new ProfileMember.WindowSize();

        [DataMember]
        public ProfileMember.IsFullScreenMode IsFullScreenMode { get; set; } = new ProfileMember.IsFullScreenMode();


        // 読み込み
        [DataMember]
        public ProfileMember.Path Path { get; set; } = new ProfileMember.Path();

        [DataMember]
        public ProfileMember.LastPageIndex LastPageIndex { get; set; } = new ProfileMember.LastPageIndex();

        // 行列設定
        [DataMember]
        public ProfileMember.NumofMatrix NumofMatrix { get; set; } = new ProfileMember.NumofMatrix();


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

        [DataMember]
        public ProfileMember.SlideShowAutoStart SlideShowAutoStart { get; set; } = new ProfileMember.SlideShowAutoStart();

        // その他の設定_全般
        [DataMember]
        public ProfileMember.FileSortMethod FileSortMethod { get; set; } = new ProfileMember.FileSortMethod();

        [DataMember]
        public ProfileMember.TopMost TopMost { get; set; } = new ProfileMember.TopMost();

        [DataMember]
        public ProfileMember.ApplyRotateInfoFromExif ApplyRotateInfoFromExif { get; set; } = new ProfileMember.ApplyRotateInfoFromExif();

        [DataMember]
        public ProfileMember.BitmapDecodeTotalPixel BitmapDecodeTotalPixel { get; set; } = new ProfileMember.BitmapDecodeTotalPixel();


        // その他の設定_配置
        [DataMember]
        public ProfileMember.UseDefaultTileOrigin UseDefaultTileOrigin { get; set; } = new ProfileMember.UseDefaultTileOrigin();

        [DataMember]
        public ProfileMember.TileOrigin TileOrigin { get; set; } = new ProfileMember.TileOrigin();

        [DataMember]
        public ProfileMember.TileOrientation TileOrientation { get; set; } = new ProfileMember.TileOrientation();

        [DataMember]
        public ProfileMember.TileImageStretch TileImageStretch { get; set; } = new ProfileMember.TileImageStretch();

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



        public Profile()
        {
            ProfileType = ProfileType.Temp; // タイプ
            Name = "NoName";                // プロファイル名
        }

        // 既定値(xmlファイルデシリアライズ時に呼ばれる)
        [OnDeserializing]
        public void DefaultDeserializing(StreamingContext sc)
        {
            this.SlideShowAutoStart = new ProfileMember.SlideShowAutoStart();
        }


        /// <summary>
        /// 他のプロファイルの設定値を統合(有効なメンバが衝突したら、他のプロファイルを優先)
        /// </summary>
        /// <param name="pf">統合するプロファイル</param>
        public Profile Marge(Profile pf)
        {
            // 読み込み
            if( pf.Path.IsEnabled )          Path.Value = new List<string>(pf.Path.Value);
            if( pf.LastPageIndex.IsEnabled ) LastPageIndex.Value = pf.LastPageIndex.Value;

            // 行列設定
            if( pf.NumofMatrix.IsEnabled )          Array.Copy(pf.NumofMatrix.Value, NumofMatrix.Value, 2);

            // アスペクト比設定
            if( pf.AspectRatio.IsEnabled )       Array.Copy(pf.AspectRatio.Value, AspectRatio.Value, 2);
            if( pf.NonFixAspectRatio.IsEnabled ) NonFixAspectRatio.Value = pf.NonFixAspectRatio.Value;

            // スライドの設定
            if( pf.SlidePlayMethod.IsEnabled )          SlidePlayMethod.Value = pf.SlidePlayMethod.Value;
            if( pf.SlideSpeed.IsEnabled )               SlideSpeed.Value = pf.SlideSpeed.Value;
            if( pf.SlideInterval.IsEnabled )            SlideInterval.Value = pf.SlideInterval.Value;
            if( pf.SlideDirection.IsEnabled )           SlideDirection.Value = pf.SlideDirection.Value;
            if( pf.SlideTimeInIntevalMethod.IsEnabled ) SlideTimeInIntevalMethod.Value = pf.SlideTimeInIntevalMethod.Value;
            if( pf.SlideByOneImage.IsEnabled )          SlideByOneImage.Value = pf.SlideByOneImage.Value;
            if( pf.SlideShowAutoStart.IsEnabled )       SlideShowAutoStart.Value = pf.SlideShowAutoStart.Value;

            // その他の設定_全般
            if( pf.FileSortMethod.IsEnabled )          FileSortMethod.Value = pf.FileSortMethod.Value;
            if( pf.TopMost.IsEnabled )                 TopMost.Value = pf.TopMost.Value;
            if( pf.ApplyRotateInfoFromExif.IsEnabled ) ApplyRotateInfoFromExif.Value = pf.ApplyRotateInfoFromExif.Value;
            if( pf.BitmapDecodeTotalPixel.IsEnabled )  BitmapDecodeTotalPixel.Value = pf.BitmapDecodeTotalPixel.Value;

            // その他の設定_配置
            if( pf.UseDefaultTileOrigin.IsEnabled ) UseDefaultTileOrigin.Value = pf.UseDefaultTileOrigin.Value;
            if( pf.TileOrigin.IsEnabled )           TileOrigin.Value = pf.TileOrigin.Value;
            if( pf.TileOrientation.IsEnabled )      TileOrientation.Value = pf.TileOrientation.Value;
            if( pf.TileImageStretch.IsEnabled )     TileImageStretch.Value = pf.TileImageStretch.Value;

            // その他の設定_外観1
            if( pf.AllowTransparency.IsEnabled )          AllowTransparency.Value = pf.AllowTransparency.Value;
            if( pf.OverallOpacity.IsEnabled )             OverallOpacity.Value = pf.OverallOpacity.Value;
            if( pf.BackgroundOpacity.IsEnabled )          BackgroundOpacity.Value = pf.BackgroundOpacity.Value;
            if( pf.BaseGridBackgroundColor.IsEnabled )    BaseGridBackgroundColor.Value = pf.BaseGridBackgroundColor.Value;
            if( pf.UsePlaidBackground.IsEnabled )         UsePlaidBackground.Value = pf.UsePlaidBackground.Value;
            if( pf.PairColorOfPlaidBackground.IsEnabled ) PairColorOfPlaidBackground.Value = pf.PairColorOfPlaidBackground.Value;

            // その他の設定_外観2
            if( pf.ResizeGripThickness.IsEnabled )  ResizeGripThickness.Value = pf.ResizeGripThickness.Value;
            if( pf.ResizeGripColor.IsEnabled )      ResizeGripColor.Value = pf.ResizeGripColor.Value;
            if( pf.TilePadding.IsEnabled )          TilePadding.Value = pf.TilePadding.Value;
            if( pf.GridLineColor.IsEnabled )        GridLineColor.Value = pf.GridLineColor.Value;

            // ダイアログにはない設定
            if( pf.WindowPos.IsEnabled )        WindowPos.Value = new Point(pf.WindowPos.Value.X, pf.WindowPos.Y);
            if( pf.WindowSize.IsEnabled )       WindowSize.Value = new Size(pf.WindowSize.Value.Width, pf.WindowSize.Height);
            if( pf.IsFullScreenMode.IsEnabled ) IsFullScreenMode.Value = pf.IsFullScreenMode.Value;

            return this;
        }


        /// <summary>
        /// クローン
        /// </summary>
        /// <returns></returns>
        public Profile Clone()
        {
            Profile newProfile = new Profile();

            PropertyInfo[] infoArraySrc  = this.GetType().GetProperties();
            PropertyInfo[] infoArrayDest = newProfile.GetType().GetProperties();

            for(int i=0; i < infoArraySrc.Count(); i++ )
            {
                ProfileMember.IProfileMember memberSrc = infoArraySrc[i].GetValue(this, null) as ProfileMember.IProfileMember;
                ProfileMember.IProfileMember memberDest = infoArrayDest[i].GetValue(newProfile, null) as ProfileMember.IProfileMember;

                if(memberSrc != null && memberDest != null)
                {
                    memberDest.IsEnabled = memberSrc.IsEnabled;
                }
            }

            // Profile data
            newProfile.Name = this.Name;
            newProfile.ProfileType = this.ProfileType;

            // 読み込み
            newProfile.Path.Value = new List<string>(this.Path.Value);
            newProfile.LastPageIndex.Value = this.LastPageIndex.Value;

            // 行列設定
            Array.Copy(this.NumofMatrix.Value, newProfile.NumofMatrix.Value, 2);

            // アスペクト比設定
            Array.Copy(this.AspectRatio.Value, newProfile.AspectRatio.Value, 2);
            newProfile.NonFixAspectRatio.Value = this.NonFixAspectRatio.Value;

            // スライドの設定
            newProfile.SlidePlayMethod.Value = this.SlidePlayMethod.Value;
            newProfile.SlideSpeed.Value = this.SlideSpeed.Value;
            newProfile.SlideInterval.Value = this.SlideInterval.Value;
            newProfile.SlideDirection.Value = this.SlideDirection.Value;
            newProfile.SlideTimeInIntevalMethod.Value = this.SlideTimeInIntevalMethod.Value;
            newProfile.SlideByOneImage.Value = this.SlideByOneImage.Value;
            newProfile.SlideShowAutoStart.Value = this.SlideShowAutoStart.Value;

            // その他の設定_全般
            newProfile.FileSortMethod.Value = this.FileSortMethod.Value;
            newProfile.TopMost.Value = this.TopMost.Value;
            newProfile.ApplyRotateInfoFromExif.Value = this.ApplyRotateInfoFromExif.Value;
            newProfile.BitmapDecodeTotalPixel.Value = this.BitmapDecodeTotalPixel.Value;

            // その他の設定_配置
            newProfile.UseDefaultTileOrigin.Value = this.UseDefaultTileOrigin.Value;
            newProfile.TileOrigin.Value = this.TileOrigin.Value;
            newProfile.TileOrientation.Value = this.TileOrientation.Value;
            newProfile.TileImageStretch.Value = this.TileImageStretch.Value;

            // その他の設定_外観1
            newProfile.AllowTransparency.Value = this.AllowTransparency.Value;
            newProfile.OverallOpacity.Value = this.OverallOpacity.Value;
            newProfile.BackgroundOpacity.Value = this.BackgroundOpacity.Value;
            newProfile.BaseGridBackgroundColor.Value = this.BaseGridBackgroundColor.Value;
            newProfile.UsePlaidBackground.Value = this.UsePlaidBackground.Value;
            newProfile.PairColorOfPlaidBackground.Value = this.PairColorOfPlaidBackground.Value;

            // その他の設定_外観2
            newProfile.ResizeGripThickness.Value = this.ResizeGripThickness.Value;
            newProfile.ResizeGripColor.Value = this.ResizeGripColor.Value;
            newProfile.TilePadding.Value = this.TilePadding.Value;
            newProfile.GridLineColor.Value = this.GridLineColor.Value;

            // ダイアログにはない設定
            newProfile.WindowPos.Value = new Point(this.WindowPos.Value.X, this.WindowPos.Y);
            newProfile.WindowSize.Value = new Size(this.WindowSize.Value.Width, this.WindowSize.Height);
            newProfile.IsFullScreenMode.Value = this.IsFullScreenMode.Value;


            return newProfile;
        }


        public string CreateProfileToolTip()
        {
            string tooltip = "";

            PropertyInfo[] infoArray = this.GetType().GetProperties();

            foreach( PropertyInfo info in infoArray )
            {
                ProfileMember.IProfileMember member = info.GetValue(this, null) as ProfileMember.IProfileMember;
                if(member != null && member.IsEnabled)
                {
                    tooltip += member.TooltipStr;
                    tooltip += "\r\n";
                }
            }

            // 最後の改行削除
            tooltip = tooltip.TrimEnd('\r', '\n');

            return tooltip;
        }


    }
}
