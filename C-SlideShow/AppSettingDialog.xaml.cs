using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Forms = System.Windows.Forms;


namespace C_SlideShow
{
    public enum AppSettingDialogTabIndex
    {
        History     = 0,
        AspectRatio = 1,
        ExternalApp = 2,
        Detail      = 3
    }

    /// <summary>
    /// AppSettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AppSettingDialog : Window
    {

        bool isInitializing = false;
        AppSetting setting;

        public AppSettingDialog()
        {
            InitializeComponent();

            this.Closing += (s, e) =>
            {
                MainWindow.Current.Setting.AppSettingDialogTabIndex = MainTabControl.SelectedIndex;

                // 履歴上限数を超えてたら削除
                if(setting.History.Count > setting.NumofHistory )
                {
                    setting.History.RemoveRange( setting.NumofHistory, setting.History.Count - setting.NumofHistory);
                }
            };
        }

        public void Initialize()
        {
            isInitializing = true;
            setting = MainWindow.Current.Setting;

            // 履歴設定
            EnabledItemsInHistory_ArchiverPath.IsChecked = setting.EnabledItemsInHistory.ArchiverPath;
            EnabledItemsInHistory_ImagePath.IsChecked =  setting.EnabledItemsInHistory.ImagePath ;
            EnabledItemsInHistory_AspectRatio.IsChecked =  setting.EnabledItemsInHistory.AspectRatio ;
            EnabledItemsInHistory_Matrix.IsChecked =  setting.EnabledItemsInHistory.Matrix ;
            EnabledItemsInHistory_SlideDirection.IsChecked =  setting.EnabledItemsInHistory.SlideDirection ;

            NumofHistory.Value = setting.NumofHistory;
            ApplyHistoryInfoInNewArchiverReading.IsChecked =  setting.ApplyHistoryInfoInNewArchiverReading ;
            
            NumofHistoryInMenu.Value = setting.NumofHistoryInMenu;
            NumofHistoryInMainMenu.Value = setting.NumofHistoryInMainMenu;


            // アスペクト比
            AspectRatio_H.MinValue = ProfileMember.AspectRatio.Min;
            AspectRatio_H.MaxValue = ProfileMember.AspectRatio.MaxH;
            AspectRatio_V.MinValue = ProfileMember.AspectRatio.Min;
            AspectRatio_V.MaxValue = ProfileMember.AspectRatio.MaxV;

            foreach(Point pt in setting.AspectRatioList )
            {
                AspectRatioList.Items.Add( string.Format("{0} : {1}", pt.X, pt.Y) );
            }
            if( setting.AspectRatioList.Count > 0 )
            {
                AspectRatioList.SelectedIndex = 0;
                AspectRatio_H.Value = (int)setting.AspectRatioList[0].X;
                AspectRatio_V.Value = (int)setting.AspectRatioList[0].Y;
            }


            // 外部連携
            ExternalAppPath.Text = setting.ExternalAppInfoList[0].Path;
            ExternalAppArg.Text  = setting.ExternalAppInfoList[0].Arg;


            // 詳細
            ShowMenuItem_AdditionalRead.IsChecked = setting.ShowMenuItem_AdditionalRead;
            SerachAllDirectoriesInFolderReading.IsChecked = setting.SerachAllDirectoriesInFolderReading;


            isInitializing = false;
        }

        private void CorrectNumofHistory()
        {
            if( NumofHistory.Value < NumofHistoryInMenu.Value ) NumofHistoryInMenu.Value = NumofHistory.Value;
            if( NumofHistoryInMenu.Value < NumofHistoryInMainMenu.Value ) NumofHistoryInMainMenu.Value = NumofHistoryInMenu.Value;
        }


        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        // 履歴設定
        private void NumofHistory_ValueChanged(object sender, EventArgs e)
        {
            if( isInitializing ) return;
            CorrectNumofHistory();
        }

        private void NumofHistoryInMenu_ValueChanged(object sender, EventArgs e)
        {
            if( isInitializing ) return;
            CorrectNumofHistory();
        }

        private void NumofHistoryInMainMenu_ValueChanged(object sender, EventArgs e)
        {
            if( isInitializing ) return;
            CorrectNumofHistory();
        }

        private void DeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result =  MessageBox.Show("全てのフォルダ(書庫)の履歴を削除してもよろしいですか？\r\nこの操作はキャンセル出来ません", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if( result == MessageBoxResult.Yes )
            {
                setting.History.Clear();
                setting.FolderOpenDialogLastSelectedPath = "";
                setting.FileOpenDialogLastSelectedPath   = "";
            }
        }

        private void AllDefault_History_Click(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult result =  MessageBox.Show("履歴設定をすべてデフォルトに戻します。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            //if( result == MessageBoxResult.No ) return;

            EnabledItemsInHistory_ArchiverPath.IsChecked = true;
            EnabledItemsInHistory_ImagePath.IsChecked = true;
            EnabledItemsInHistory_AspectRatio.IsChecked = true;
            EnabledItemsInHistory_Matrix.IsChecked = true;
            EnabledItemsInHistory_SlideDirection.IsChecked = true;
            NumofHistory.Value = 100;
            ApplyHistoryInfoInNewArchiverReading.IsChecked = true;
            NumofHistoryInMenu.Value = 30;
            NumofHistoryInMainMenu.Value = 10;

        }

        // アスペクト比
        private void AspectRatioList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string str = AspectRatioList.SelectedItem as string;
            int sIdx = AspectRatioList.SelectedIndex;
            if(str != null )
            {
                str = str.ToString().Replace(" ", "");
                string[] ar = str.Split(':');
                int w = int.Parse(ar[0]);
                int h = int.Parse(ar[1]);
                AspectRatio_H.Value = w;
                AspectRatio_V.Value = h;
                AspectRatioList.SelectedIndex = sIdx;
            }
        }

        private void AspectRatio_H_ValueChanged(object sender, EventArgs e)
        {
            if( isInitializing ) return;
            int sIdx = AspectRatioList.SelectedIndex;
            if(sIdx >= 0 )
            {
                AspectRatioList.Items[sIdx] = string.Format( "{0} : {1}", AspectRatio_H.Value, AspectRatio_V.Value );
                AspectRatioList.SelectedIndex = sIdx;
            }
        }

        private void AspectRatio_V_ValueChanged(object sender, EventArgs e)
        {
            if( isInitializing ) return;
            int sIdx = AspectRatioList.SelectedIndex;
            if(sIdx >= 0 )
            {
                AspectRatioList.Items[sIdx] = string.Format( "{0} : {1}", AspectRatio_H.Value, AspectRatio_V.Value );
                AspectRatioList.SelectedIndex = sIdx;
            }
        }

        private void AspectRatioList_Up_Click(object sender, RoutedEventArgs e)
        {
            if( AspectRatioList.Items.Count < 2 ) return;

            int sIdx = AspectRatioList.SelectedIndex;
            if(sIdx > 0 && sIdx < AspectRatioList.Items.Count)
            {
                string str = AspectRatioList.SelectedItem as string;
                AspectRatioList.Items.RemoveAt(sIdx);
                AspectRatioList.Items.Insert(sIdx - 1, str);
                AspectRatioList.SelectedIndex = sIdx - 1;
            }
        }

        private void AspectRatioList_Down_Click(object sender, RoutedEventArgs e)
        {
            if( AspectRatioList.Items.Count < 2 ) return;

            int sIdx = AspectRatioList.SelectedIndex;
            if(sIdx < AspectRatioList.Items.Count - 1 && sIdx >= 0)
            {
                string str = AspectRatioList.SelectedItem as string;
                AspectRatioList.Items.RemoveAt(sIdx);
                AspectRatioList.Items.Insert(sIdx + 1, str);
                AspectRatioList.SelectedIndex = sIdx + 1;
            }
        }

        private void AspectRatioList_New_Click(object sender, RoutedEventArgs e)
        {
            string str = AspectRatioList.SelectedItem as string;
            if( str != null )
            {
                AspectRatioList.Items.Add( string.Copy(str) );
                AspectRatioList.SelectedIndex = AspectRatioList.Items.Count - 1;
            }
            else
                AspectRatioList.Items.Add("4 : 3");
        }

        private void AspectRatioList_Delete_Click(object sender, RoutedEventArgs e)
        {
            int sIdx = AspectRatioList.SelectedIndex;
            if( sIdx >= 0 && sIdx < AspectRatioList.Items.Count)
            {
                AspectRatioList.Items.RemoveAt(sIdx);
                if(sIdx < AspectRatioList.Items.Count )
                    AspectRatioList.SelectedIndex = sIdx;
                else
                    AspectRatioList.SelectedIndex = AspectRatioList.Items.Count - 1;
            }
        }

        private void AllDefault_AspectRatio_Click(object sender, RoutedEventArgs e)
        {
            AspectRatioList.Items.Clear();
            AspectRatioList.Items.Add("4 : 3");
            AspectRatioList.Items.Add("3 : 4");
            AspectRatioList.Items.Add("16 : 9");
            AspectRatioList.Items.Add("9 : 16");
            AspectRatioList.Items.Add("3 : 2");
            AspectRatioList.Items.Add("2 : 3");
            AspectRatioList.Items.Add("1 : 1");
        }


        // 外部連携
        private void ExternalAppPathBrowse_Click(object sender, RoutedEventArgs e)
        {
            Forms.OpenFileDialog ofd = new Forms.OpenFileDialog();
            ofd.Title = "プログラムを選択してください";
            ofd.Filter = "EXEファイル(*.exe)|*.exe|すべてのファイル(*.*)|*.*";
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == Forms.DialogResult.OK)
            {
                ExternalAppPath.Text = ofd.FileNames[0];
            }
        }

        private void ExternalAppDefault_Click(object sender, RoutedEventArgs e)
        {
            ExternalAppPath.Text = "";
            ExternalAppArg.Text  = "\"$FilePath$\"";
        }

        /* ---------------------------------------------------- */
        //     OK / キャンセル
        /* ---------------------------------------------------- */
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 履歴設定
            setting.EnabledItemsInHistory.ArchiverPath =  (bool)EnabledItemsInHistory_ArchiverPath.IsChecked ;
            setting.EnabledItemsInHistory.ImagePath =  (bool)EnabledItemsInHistory_ImagePath.IsChecked ;
            setting.EnabledItemsInHistory.AspectRatio =  (bool)EnabledItemsInHistory_AspectRatio.IsChecked ;
            setting.EnabledItemsInHistory.Matrix =  (bool)EnabledItemsInHistory_Matrix.IsChecked ;
            setting.EnabledItemsInHistory.SlideDirection =  (bool)EnabledItemsInHistory_SlideDirection.IsChecked ;
            setting.NumofHistory = NumofHistory.Value;
            setting.ApplyHistoryInfoInNewArchiverReading =  (bool)ApplyHistoryInfoInNewArchiverReading.IsChecked ;
            setting.NumofHistoryInMenu = NumofHistoryInMenu.Value;
            setting.NumofHistoryInMainMenu = NumofHistoryInMainMenu.Value;

            // アスペクト比
            setting.AspectRatioList.Clear();
            foreach(object item in AspectRatioList.Items )
            {
                string str = item as string;
                if(str != null )
                {
                    str = str.ToString().Replace(" ", "");
                    string[] ar = str.Split(':');
                    int w = int.Parse(ar[0]);
                    int h = int.Parse(ar[1]);
                    setting.AspectRatioList.Add( new Point(w, h) );
                }
            }

            // 外部連携
            setting.ExternalAppInfoList[0].Path = ExternalAppPath.Text;
            setting.ExternalAppInfoList[0].Arg  = ExternalAppArg.Text;

            // 詳細
            setting.ShowMenuItem_AdditionalRead = (bool)ShowMenuItem_AdditionalRead.IsChecked ;
            setting.SerachAllDirectoriesInFolderReading = (bool)SerachAllDirectoriesInFolderReading.IsChecked;


            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
