using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace C_SlideShow
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        // 一時ファイルのリスト
        public static List<string> TempFilePathList;

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            MainWindow mainWindow;

            // xmlから設定をロード
            AppSetting setting = new AppSetting().loadFromXmlFile();

            // 起動時設定の適用
            Profile pf = setting.TempProfile;
            if( !setting.StartUp_RestoreWindowSizeAndPos )
            {
                pf.WindowPos  = new ProfileMember.WindowPos();
                pf.WindowSize = new ProfileMember.WindowSize();
            }
            if( !setting.StartUp_LoadLastFiles ) { pf.Path.Value.Clear(); }
            if( !setting.StartUp_RestoreLastPageIndex ) { pf.LastPageIndex.Value = 0; }
            if( !setting.StartUp_RestoreSlideShowPlaying ) { pf.SlideShowAutoStart.Value = false; }

            // 引数チェック
            if(e.Args.Length > 0 )
            {
                // 引数あり
                mainWindow = new MainWindow(setting, e.Args);
            }
            else
            {
                // 引数なし
                mainWindow = new MainWindow(setting);
            }

            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // 設定保存
            C_SlideShow.MainWindow.Current.UpdateTempProfile();
            C_SlideShow.MainWindow.Current.SaveHistoryItem();
            C_SlideShow.MainWindow.Current.Setting.saveToXmlFile();

            // 一時ファイルの削除
            if(TempFilePathList != null )
            {
                foreach(string tempFilePath in TempFilePathList )
                {
                    try { System.IO.File.Delete(tempFilePath); }
                    catch { }
                }
            }

        }
    }
}
