﻿using System;
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

    public enum FileReadingOrder
    {
        FileName, FileNameRev, LastWriteTime, LastWriteTimeRev, Random
    }


    [DataContract(Name = "Profile")]
    public class Profile
    {
        // ダイアログ未実装
        [DataMember]
        public int TilePadding { get; set; }

        [DataMember]
        public Color GridLineColor { get; set; }

        [DataMember]
        public int BitmapDecodeTotalPixelWidth { get; set; }

        [DataMember]
        public bool ApplyRotateInfoFromExif { get; set; }

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

        [DataMember]
        public FileReadingOrder FileReadingOrder { get; set; }

        [DataMember]
        public bool TopMost { get; set; }

        [DataMember]
        public bool StartUp_OpenPrevFolder { get; set; }

        // その他の設定_UI
        [DataMember]
        public double UI_ResizeGripThickness { get; set; }

        [DataMember]
        public Color UI_ResizeGripColor { get; set; }

        [DataMember]
        public Color UI_SeekbarColor { get; set; }

        // ダイアログにはない設定
        [DataMember]
        public string FolderPath { get; set; }

        [DataMember]
        public int LastPageIndex { get; set; }

        [DataMember]
        public Rect WindowRect { get; set; }

        [DataMember]
        public bool IsFullScreenMode { get; set; }



        public Profile()
        {
            // ダイアログ未実装
            TilePadding = 3;
            GridLineColor = Colors.LightGray;
            BitmapDecodeTotalPixelWidth = 1920;
            ApplyRotateInfoFromExif = false;

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
            AllowTransparency = false;
            OverallOpacity = 0.5;
            BackgroundOpacity = 1.0;
            BaseGridBackgroundColor = Colors.White;
            UsePlaidBackground = false;
            PairColorOfPlaidBackground = Colors.LightGray;
            TopMost = false;
            StartUp_OpenPrevFolder = true;

            // その他の設定_UI
            UI_ResizeGripThickness = 5;
            UI_ResizeGripColor = Colors.Gray;
            UI_SeekbarColor = Colors.Black;

            // ダイアログにはない設定
            FolderPath = "";
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
        }

    }
}
