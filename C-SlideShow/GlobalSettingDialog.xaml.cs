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

namespace C_SlideShow
{
    /// <summary>
    /// GlobalSettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class GlobalSettingDialog : Window
    {

        bool isInitializing = false;
        AppSetting setting;

        public GlobalSettingDialog()
        {
            InitializeComponent();

            this.Closing += (s, e) =>
            {
                MainWindow.Current.Setting.GlobalSettingDialogTabIndex = MainTabControl.SelectedIndex;

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
            if( setting.EnabledItemsInHistory.ArchiverPath ) EnabledItemsInHistory_ArchiverPath.IsChecked = true;
            else EnabledItemsInHistory_ArchiverPath.IsChecked = false;

            if( setting.EnabledItemsInHistory.ImagePath ) EnabledItemsInHistory_ImagePath.IsChecked = true;
            else EnabledItemsInHistory_ImagePath.IsChecked = false;

            if( setting.EnabledItemsInHistory.AspectRatio ) EnabledItemsInHistory_AspectRatio.IsChecked = true;
            else EnabledItemsInHistory_AspectRatio.IsChecked = false;

            if( setting.EnabledItemsInHistory.Matrix ) EnabledItemsInHistory_Matrix.IsChecked = true;
            else EnabledItemsInHistory_Matrix.IsChecked = false;

            if( setting.EnabledItemsInHistory.SlideDirection ) EnabledItemsInHistory_SlideDirection.IsChecked = true;
            else EnabledItemsInHistory_SlideDirection.IsChecked = false;

            NumofHistory.Value = setting.NumofHistory;

            if( setting.ApplyHistoryInfoInNewArchiverReading ) ApplyHistoryInfoInNewArchiverReading.IsChecked = true;
            else ApplyHistoryInfoInNewArchiverReading.IsChecked = false;
            
            NumofHistoryInMenu.Value = setting.NumofHistoryInMenu;
            NumofHistoryInMainMenu.Value = setting.NumofHistoryInMainMenu;


            isInitializing = false;
        }

        public void CorrectNumofHistory()
        {
            if( setting.NumofHistory < setting.NumofHistoryInMenu ) setting.NumofHistoryInMenu = setting.NumofHistory;
            if( setting.NumofHistoryInMenu < setting.NumofHistoryInMainMenu ) setting.NumofHistoryInMainMenu = setting.NumofHistoryInMenu;

            NumofHistory.Value = setting.NumofHistory;
            NumofHistoryInMenu.Value = setting.NumofHistoryInMenu;
            NumofHistoryInMainMenu.Value = setting.NumofHistoryInMainMenu;
        }

        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        // 履歴設定
        private void EnabledItemsInHistory_ArchiverPath_Click(object sender, RoutedEventArgs e)
        {
            if( (bool)EnabledItemsInHistory_ArchiverPath.IsChecked ) setting.EnabledItemsInHistory.ArchiverPath = true;
            else setting.EnabledItemsInHistory.ArchiverPath = false;
        }

        private void EnabledItemsInHistory_ImagePath_Click(object sender, RoutedEventArgs e)
        {
            if( (bool)EnabledItemsInHistory_ImagePath.IsChecked ) setting.EnabledItemsInHistory.ImagePath = true;
            else setting.EnabledItemsInHistory.ImagePath = false;
        }

        private void EnabledItemsInHistory_AspectRatio_Click(object sender, RoutedEventArgs e)
        {
            if( (bool)EnabledItemsInHistory_AspectRatio.IsChecked ) setting.EnabledItemsInHistory.AspectRatio = true;
            else setting.EnabledItemsInHistory.AspectRatio = false;
        }

        private void EnabledItemsInHistory_Matrix_Click(object sender, RoutedEventArgs e)
        {
            if( (bool)EnabledItemsInHistory_Matrix.IsChecked ) setting.EnabledItemsInHistory.Matrix = true;
            else setting.EnabledItemsInHistory.Matrix = false;
        }

        private void EnabledItemsInHistory_SlideDirection_Click(object sender, RoutedEventArgs e)
        {
            if( (bool)EnabledItemsInHistory_SlideDirection.IsChecked ) setting.EnabledItemsInHistory.SlideDirection = true;
            else setting.EnabledItemsInHistory.SlideDirection = false;
        }

        private void NumofHistory_ValueChanged(object sender, EventArgs e)
        {
            if( isInitializing ) return;

            setting.NumofHistory = NumofHistory.Value;
            CorrectNumofHistory();
        }

        private void ApplyHistoryInfoInNewArchiverReading_Click(object sender, RoutedEventArgs e)
        {
            if( (bool)ApplyHistoryInfoInNewArchiverReading.IsChecked ) setting.ApplyHistoryInfoInNewArchiverReading = true;
            else setting.ApplyHistoryInfoInNewArchiverReading = false;
        }

        private void NumofHistoryInMenu_ValueChanged(object sender, EventArgs e)
        {
            if( isInitializing ) return;

            setting.NumofHistoryInMenu = NumofHistoryInMenu.Value;
            CorrectNumofHistory();
        }

        private void NumofHistoryInMainMenu_ValueChanged(object sender, EventArgs e)
        {
            if( isInitializing ) return;

            setting.NumofHistoryInMainMenu = NumofHistoryInMainMenu.Value;
            CorrectNumofHistory();
        }

        private void DeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result =  MessageBox.Show("全てのフォルダ(書庫)の履歴を削除してもよろしいですか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if( result == MessageBoxResult.Yes ) setting.History.Clear();
        }

        private void AllDefault_History_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result =  MessageBox.Show("履歴設定をすべてデフォルトに戻します。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if( result == MessageBoxResult.No ) return;

            setting.EnabledItemsInHistory = new EnabledItemsInHistory();
            setting.NumofHistory = 100;
            setting.NumofHistoryInMenu = 30;
            setting.NumofHistoryInMainMenu = 10;
            setting.ApplyHistoryInfoInNewArchiverReading = true;

            Initialize();
        }


        // 閉じる
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
