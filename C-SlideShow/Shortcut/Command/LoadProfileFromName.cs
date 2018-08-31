using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace C_SlideShow.Shortcut.Command
{
    public class LoadProfileFromName : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = true;

        public LoadProfileFromName()
        {
            ID    = CommandID.LoadProfileFromName;
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
            var userProfileInfo = list.FirstOrDefault(l => l.RelativePath == StrValue + ".xml");
            if(userProfileInfo != null )
            {
                string xmlDir = Directory.GetParent( System.Reflection.Assembly.GetExecutingAssembly().Location ).FullName + "\\Profile";
                string xmlPath = xmlDir + "\\" + userProfileInfo.RelativePath;
                pf = UserProfileInfo.LoadProfileFromXmlFile(xmlPath);
            }

            // プロファイルのロード
            if(pf != null )
            {
                MainWindow.Current.LoadUserProfile(pf);
            }
        }

        public string GetDetail()
        {
            return "プロファイル名「" +  StrValue + "」をロード";
        }
    }
}
