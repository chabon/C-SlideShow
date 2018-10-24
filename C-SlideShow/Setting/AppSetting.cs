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
    // 履歴情報
    [DataContract(Name = "HistoryItem")]
    public class HistoryItem
    {
        public HistoryItem(string archverPath)
        {
            ArchiverPath = archverPath;
        }

        [DataMember]
        public string ArchiverPath { get; set; }

        [DataMember]
        public string ImagePath { get; set; } = null;

        [DataMember]
        public int[] AspectRatio { get; set; } = null;

        [DataMember]
        public int[] Matrix { get; set; } = null;

        [DataMember]
        public SlideDirection SlideDirection { get; set; } = SlideDirection.None;
    }

    // 履歴に残す項目
    [DataContract(Name = "EnabledItemsInHistory")]
    public class EnabledItemsInHistory
    {
        [DataMember]
        public bool ArchiverPath { get; set; } = true;

        [DataMember]
        public bool ImagePath { get; set; } = true;

        [DataMember]
        public bool AspectRatio { get; set; } = true;

        [DataMember]
        public bool Matrix { get; set; } = true;

        [DataMember]
        public bool SlideDirection { get; set; } = true;
    }


    // 外部プログラム
    [DataContract(Name = "ExternalAppInfo")]
    public class ExternalAppInfo
    {
        [DataMember]
        public string Name { get; set; } = "";

        [DataMember]
        public string Path { get; set; } = "";

        [DataMember]
        public string Arg { get; set; } = "\"" + Format.FilePathFormat + "\"";

        [DataMember]
        public bool ShowContextMenu { get; set; } = true;

        public string GetAppName()
        {
            if( Name != null && Name != "" )
            {
                return Name;
            }
            else if(Path != null && Path != "")
            {
                return System.IO.Path.GetFileName(Path);
            }
            else
            {
                return null;
            }
        }
    }


    [DataContract(Name = "AppSetting")]
    public class AppSetting
    {
        // ダイアログ未実装



        // 一時的プロファイル
        [DataMember]
        public Profile TempProfile { get; set; }


        // ダイアログには無い設定
        [DataMember]
        public bool ShowFileInfoInTileExpantionPanel { get; set; }

        [DataMember]
        public int SettingDialogTabIndex { get; set; }

        [DataMember]
        public int AppSettingDialogTabIndex { get; set; }

        [DataMember]
        public int AppSettingDialog_ShortcutSettingTabIndex { get; set; }

        [DataMember]
        public Size AppSettingDialogSize { get; set; }

        [DataMember]
        public int MatrixSelecterMaxSize { get; set; }

        [DataMember]
        public string FolderOpenDialogLastSelectedPath { get; set; }

        [DataMember]
        public string FileOpenDialogLastSelectedPath { get; set; }


        // プロファイル
        [DataMember]
        public List<UserProfileInfo> UserProfileList { get; set; }

        [DataMember]
        public bool UsePresetProfile { get; set; }


        // 起動時
        [DataMember]
        public bool StartUp_RestoreWindowSizeAndPos { get; set; }

        [DataMember]
        public bool StartUp_LoadLastFiles { get; set; }

        [DataMember]
        public bool StartUp_RestoreSlideShowPlaying { get; set; }

        [DataMember]
        public bool StartUp_RestoreLastPageIndex { get; set; }

        // ショートカット
        [DataMember]
        public ShortcutSetting ShortcutSetting { get; set; }

        [DataMember]
        public int MouseGestureRange { get; set; }

        [DataMember]
        public int LongClickDecisionTime { get; set; }

        // アスペクト比のリスト
        [DataMember]
        public List<Point> AspectRatioList { get; set; }


        // 履歴設定
        [DataMember]
        public List<HistoryItem> History { get; set; }

        [DataMember]
        public int NumofHistory { get; set; }

        [DataMember]
        public int NumofHistoryInMenu { get; set; }

        [DataMember]
        public int NumofHistoryInMainMenu { get; set; }

        [DataMember]
        public EnabledItemsInHistory EnabledItemsInHistory { get; set; }

        [DataMember]
        public bool ApplyHistoryInfoInNewArchiverReading { get; set; }


        // 外部連携
        [DataMember]
        public List<ExternalAppInfo> ExternalAppInfoList { get; set; }


        // 詳細
        [DataMember]
        public bool ShowMenuItem_AdditionalRead { get; set; }

        [DataMember]
        public bool SerachAllDirectoriesInFolderReading { get; set; }

        [DataMember]
        public bool ReadSingleImageAsParentFolder { get; set; }

        [DataMember]
        public Color SeekbarColor { get; set; }

        [DataMember]
        public bool SeekBarIsMoveToPointEnabled { get; set; }

        [DataMember]
        public bool MouseCursorAutoHide { get; set; }

        [DataMember]
        public bool MouseCursorAutoHideInFullScreenModeOnly { get; set; }

        [DataMember]
        public bool CorrectPageIndexInOperationSlideCrrosOverTheOrigin { get; set; }

        [DataMember]
        public int OperationSlideDuration { get; set; }

        [DataMember]
        public bool EnableScreenSnap { get; set; }

        [DataMember]
        public bool EnableWindowSnap { get; set; }

        [DataMember]
        public int ScreenSnapRange { get; set; }

        [DataMember]
        public int WindowSnapRange { get; set; }


        public AppSetting()
        {
            // 初期化

            // 一時的プロファイル
            TempProfile = new Profile();
            TempProfile.ProfileType = ProfileType.Temp;

            // ダイアログにはない設定
            ShowFileInfoInTileExpantionPanel = true;
            SettingDialogTabIndex = 0;
            AppSettingDialogTabIndex = 0;
            AppSettingDialog_ShortcutSettingTabIndex = 0;
            AppSettingDialogSize = Size.Empty;
            MatrixSelecterMaxSize = 4;
            FolderOpenDialogLastSelectedPath = null;
            FileOpenDialogLastSelectedPath = null;

            // プロファイル
            UserProfileList = new List<UserProfileInfo>();
            UsePresetProfile = true;

            // 起動時
            StartUp_RestoreWindowSizeAndPos = true;
            StartUp_LoadLastFiles = true;
            StartUp_RestoreLastPageIndex = true;
            StartUp_RestoreSlideShowPlaying = true;


            // ショートカット
            ShortcutSetting = new ShortcutSetting();
            MouseGestureRange = 15;
            LongClickDecisionTime = 400;

            // 履歴設定
            History = new List<HistoryItem>();
            NumofHistory = 100;
            NumofHistoryInMenu = 30;
            NumofHistoryInMainMenu = 10;
            EnabledItemsInHistory = new EnabledItemsInHistory();
            ApplyHistoryInfoInNewArchiverReading = true;

            // アスペクト比のリスト
            AspectRatioList = new List<Point> { new Point(4, 3), new Point(3, 4), new Point(16, 9), new Point(9, 16), new Point(3, 2), new Point(2, 3), new Point(1, 1)};

            // 外部連携
            ExternalAppInfoList = new List<ExternalAppInfo>();

            var exAppInfo1 = new ExternalAppInfo();
            exAppInfo1.Name = "規定のプログラム"; 
            exAppInfo1.Arg = "\""+ Format.FilePathFormat + "\"";
            ExternalAppInfoList.Add(exAppInfo1);

            var exAppInfo2 = new ExternalAppInfo();
            exAppInfo2.Name = "エクスプローラー";
            exAppInfo2.Path = "explorer.exe";
            exAppInfo2.Arg = "/select,\""+ Format.FilePathFormat + "\"";
            ExternalAppInfoList.Add(exAppInfo2);

            // 詳細
            ShowMenuItem_AdditionalRead = true;
            SerachAllDirectoriesInFolderReading = true;
            ReadSingleImageAsParentFolder = true;
            SeekbarColor = Colors.Black;
            SeekBarIsMoveToPointEnabled = true;
            MouseCursorAutoHide = true;
            MouseCursorAutoHideInFullScreenModeOnly = false;
            CorrectPageIndexInOperationSlideCrrosOverTheOrigin = true;
            OperationSlideDuration = 300;
            EnableScreenSnap = true;
            EnableWindowSnap = true;
            ScreenSnapRange = 10;
            WindowSnapRange = 10;
        }


        // 既定値(xmlファイルデシリアライズ時に呼ばれる)
        [OnDeserializing]
        public void DefaultDeserializing(StreamingContext sc)
        {

        }




        /* ---------------------------------------------------- */
        //     IO
        /* ---------------------------------------------------- */

        /// <summary>
        /// Xmlから読み込み
        /// </summary>
        /// <returns></returns>
        public AppSetting loadFromXmlFile()
        {
            // exeのディレクトリ
            string exeDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

            // 入力パス
            string inputFullPath = exeDir + "\\AppSetting.xml";

            // 読み込み
            AppSetting appSetting;
            try
            {
                appSetting = SettingSerializer.LoadSettings<AppSetting>(inputFullPath);
            }
            catch
            {
                appSetting = new AppSetting();
            }
            return appSetting;
        }

        /// <summary>
        /// Xmlに保存
        /// </summary>
        public void saveToXmlFile()
        {
            // 出力ディレクトリ
            string outputDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

            // 保存
            string outputFullPath = outputDir + "\\AppSetting.xml";
            try
            {
                SettingSerializer.SaveSettings<AppSetting>(outputFullPath, this);
            }
            catch { }
        }
    }
}
