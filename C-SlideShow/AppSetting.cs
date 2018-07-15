﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Media;


namespace C_SlideShow
{
    // 履歴情報
    [DataContract(Name = "HistoryItem")]
    public class HistoryItem
    {
        public HistoryItem(string path, int index)
        {
            Path = path;
            Index = index;
        }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public int Index { get; set; }
    }

    [DataContract(Name = "AppSetting")]
    public class AppSetting
    {
        // 一時的プロファイル
        [DataMember]
        public Profile TempProfile { get; set; }

        // プロファイルリスト
        [DataMember]
        public List<Profile> ProfileList { get; set; }

        // ダイアログには無い設定
        [DataMember]
        public bool ShowFileInfoInTileExpantionPanel { get; set; }

        // ダイアログ未実装
        [DataMember]
        public List<HistoryItem> History { get; set; }

        [DataMember]
        public int NumofHistory { get; set; }

        [DataMember]
        public bool SaveLastPageIndexToHistory { get; set; }

        [DataMember]
        public int SettingDialogTabIndex { get; set; }

        public AppSetting()
        {
            // 初期化
            TempProfile = new Profile();
            TempProfile.ProfileType = ProfileType.Temp;
            ProfileList = new List<Profile>();
            ShowFileInfoInTileExpantionPanel = false;
            History = new List<HistoryItem>();
            NumofHistory = 30;
            SaveLastPageIndexToHistory = true;
            SettingDialogTabIndex = 0;
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
