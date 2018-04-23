using System;
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
    [DataContract(Name = "AppSetting")]
    public class AppSetting
    {
        // 一時的プロファイル
        [DataMember]
        public Profile TempProfile { get; set; }

        // ダイアログには無い設定
        [DataMember]
        public bool ShowFileInfoInTileExpantionPanel { get; set; }

        public AppSetting()
        {
            TempProfile = new Profile();
            ShowFileInfoInTileExpantionPanel = false;
        }

        // 既定値(xmlファイルデシリアライズ時に呼ばれる)
        [OnDeserializing]
        public void DefaultDeserializing(StreamingContext sc)
        {
            //this.editModeBackgroundColor = Colors.Gray;
            TempProfile = new Profile();
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
