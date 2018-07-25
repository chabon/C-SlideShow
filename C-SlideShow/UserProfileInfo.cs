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
    // ユーザープロファイルの情報 Profileのラッパー
    [DataContract(Name = "UserProfileInfo")]
    public class UserProfileInfo
    {
        public UserProfileInfo(string relativePath)
        {
            IsEnabled = true;
            RelativePath = relativePath;
            ParentPath = "";
            HierarcheyDepth = 0;
            IsDirectory = false;
        }

        [NonSerialized]
        public Profile Profile; 

        [DataMember]
        public bool IsEnabled { get; set; } // 有効無効

        [DataMember]
        public string RelativePath { get; set; } // アプリのディレクトリ/Profile/ からxmlまでの相対パス

        [DataMember]
        public string ParentPath { get; set; }

        [DataMember]
        public int HierarcheyDepth { get; set; } // 階層の深さ

        [DataMember]
        public bool IsDirectory { get; set; } 


        /* ---------------------------------------------------- */
        //     IO
        /* ---------------------------------------------------- */
        public void SaveProfileToXmlFile()
        {
            // 出力ディレクトリ
            string outputDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + "\\Profile";
            if( !Directory.Exists(outputDir) ) Directory.CreateDirectory(outputDir);

            // 保存
            string outputFullPath = outputDir + "\\" + this.RelativePath;
            try
            {
                SettingSerializer.SaveSettings<Profile>(outputFullPath, this.Profile);
            }
            catch { }

        }

        public static Profile LoadProfileFromXmlFile(string path)
        {
            // 読み込み
            Profile profile;
            try
            {
                profile = SettingSerializer.LoadSettings<Profile>(path);
            }
            catch
            {
                profile = new Profile();
            }
            return profile;
        }
    }
}
