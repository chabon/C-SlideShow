using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace C_SlideShow.Shortcut.Command
{
    public class LoadProfileFromNum : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; } = 1;
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = true;
        public bool      EnableStrValue  { get; } = false;

        public LoadProfileFromNum()
        {
            ID    = CommandID.LoadProfileFromNum;
            Scene = Scene.All;
        }

        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            var list = MainWindow.Current.Setting.UserProfileList;

            // プロファイル取得
            Profile pf = null;

            if( 1 <= Value && list.Count >= Value )
            {
                var userProfileInfo = list[Value - 1];
                if(userProfileInfo.Profile == null )
                {
                    string xmlDir = Directory.GetParent( System.Reflection.Assembly.GetExecutingAssembly().Location ).FullName + "\\Profile";
                    string xmlPath = xmlDir + "\\" + userProfileInfo.RelativePath;
                    pf = UserProfileInfo.LoadProfileFromXmlFile(xmlPath);
                }
                else
                {
                    pf = userProfileInfo.Profile;
                }
            }

            // プロファイルのロード
            if(pf != null )
            {
                MainWindow.Current.LoadUserProfile(pf);
            }
        }

        public string GetDetail()
        {
            string profileName;
            var list = MainWindow.Current.Setting.UserProfileList;

            if( 1 <= Value && list.Count >= Value )
            {
                var userProfileInfo = list[Value - 1];
                profileName = System.IO.Path.GetFileNameWithoutExtension(userProfileInfo.RelativePath);
            }
            else
            {
                profileName = "未作成";
            }

            return "プロファイル番号" + string.Format("{0:00}", Value) + "(" + profileName + ")をロード";
        }
    }
}
