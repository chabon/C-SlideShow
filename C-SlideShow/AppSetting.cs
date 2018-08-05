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
        public string Arg { get; set; } = "\"$FilePath$\"";
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


        // アスペクト比のリスト
        [DataMember]
        public List<Point> AspectRatioList { get; set; }


        // 外部連携
        [DataMember]
        public List<ExternalAppInfo> ExternalAppInfoList { get; set; }


        // 詳細
        [DataMember]
        public bool ShowMenuItem_AdditionalRead { get; set; }

        [DataMember]
        public bool SerachAllDirectoriesInFolderReading { get; set; }


        public AppSetting()
        {
            // 初期化

            // 一時的プロファイル
            TempProfile = new Profile();
            TempProfile.ProfileType = ProfileType.Temp;

            // ダイアログにはない設定
            ShowFileInfoInTileExpantionPanel = false;
            SettingDialogTabIndex = 0;
            AppSettingDialogTabIndex = 0;
            MatrixSelecterMaxSize = 6;
            FolderOpenDialogLastSelectedPath = null;
            FileOpenDialogLastSelectedPath = null;

            // プロファイル
            UserProfileList = new List<UserProfileInfo>();
            UsePresetProfile = true;

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
            ExternalAppInfoList.Add( new ExternalAppInfo() );

            // 詳細
            ShowMenuItem_AdditionalRead = true;
            SerachAllDirectoriesInFolderReading = true;
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
