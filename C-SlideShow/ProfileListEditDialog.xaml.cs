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
    /// ProfileListEditDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ProfileListEditDialog : Window
    {
        AppSetting setting;

        public ProfileListEditDialog()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            setting = MainWindow.Current.Setting;

            InitListBox();
            UsePresetProfile.IsChecked = setting.UsePresetProfile;
        }

        private void InitListBox()
        {
            ProfileListBox.Items.Clear();

            foreach(UserProfileInfo upi in setting.UserProfileList )
            {
                ListBoxItem item = new ListBoxItem();
                item.ToolTip = upi.Profile.CreateProfileToolTip();
                ToolTipService.SetShowDuration(item, 1000000);
                item.Content = upi.Profile.Name;
                ProfileListBox.Items.Add(item);
            }

            UpdateNumberingItemTextAll();
        }

        private void UpdateListBoxItem(int index)
        {
            if( index < 0 || index > setting.UserProfileList.Count - 1 ) return;

            UserProfileInfo upi = setting.UserProfileList[index];
            ListBoxItem newItem = new ListBoxItem();

            newItem.ToolTip = upi.Profile.CreateProfileToolTip();
            ToolTipService.SetShowDuration(newItem, 1000000);
            newItem.Content = CreateNumberingItemText(index + 1, upi.Profile.Name);
            ProfileListBox.Items[index] = newItem;
        }

        private void InsertUserProfileInfoToListBox(UserProfileInfo newUpi, int index)
        {

            ListBoxItem newItem = new ListBoxItem();
            newItem.ToolTip = newUpi.Profile.CreateProfileToolTip();
            ToolTipService.SetShowDuration(newItem, 1000000);
            newItem.Content = newUpi.Profile.Name;
            ProfileListBox.Items.Insert(index, newItem);

            UpdateNumberingItemTextAll();
        }

        private void UpdateNumberingItemTextAll()
        {
            for(int i=0; i < setting.UserProfileList.Count; i++ )
            {
                if( i < ProfileListBox.Items.Count )
                {
                    ListBoxItem item = (ListBoxItem)ProfileListBox.Items[i];
                    item.Content = CreateNumberingItemText(i+1, setting.UserProfileList[i].Profile.Name);
                }
            } 
        }

        private string CreateNumberingItemText(int number, string profileName)
        {
                return string.Format("{0:00}", number) + ": " + profileName;
        }

        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        private void UsePresetProfile_Click(object sender, RoutedEventArgs e)
        {
            setting.UsePresetProfile =  (bool)UsePresetProfile.IsChecked ;
        }

        private void ProfileList_New_Click(object sender, RoutedEventArgs e)
        {
            int cntPrev = setting.UserProfileList.Count;
            MainWindow.Current.ShowProfileEditDialog(ProfileEditDialogMode.New, null);
            if(setting.UserProfileList.Count > cntPrev )
            {
                UserProfileInfo newUpi = setting.UserProfileList[setting.UserProfileList.Count - 1];
                InsertUserProfileInfoToListBox(newUpi, ProfileListBox.Items.Count);
                ProfileListBox.SelectedIndex = ProfileListBox.Items.Count - 1;
                ProfileListBox.ScrollIntoView( ProfileListBox.Items[ProfileListBox.Items.Count - 1] );
            }
        }

        private void ProfileList_Edit_Click(object sender, RoutedEventArgs e)
        {
            int index = ProfileListBox.SelectedIndex;
            if( index < 0 || index > ProfileListBox.Items.Count - 1) return;
            
            MainWindow.Current.ShowProfileEditDialog(ProfileEditDialogMode.Edit, setting.UserProfileList[ProfileListBox.SelectedIndex]);
            UpdateListBoxItem(index);
        }

        private void ProfileList_Copy_Click(object sender, RoutedEventArgs e)
        {
            int index = ProfileListBox.SelectedIndex;
            if( index < 0 || index > ProfileListBox.Items.Count - 1) return;

            UserProfileInfo newUpi = MainWindow.Current.CopyUserProfileInfo( setting.UserProfileList[index] );
            setting.UserProfileList.Insert(index + 1, newUpi);
            InsertUserProfileInfoToListBox(newUpi, index + 1);
        }

        private void ProfileList_Delete_Click(object sender, RoutedEventArgs e)
        {
            int index = ProfileListBox.SelectedIndex;
            if( index < 0 || index > ProfileListBox.Items.Count - 1) return;

            UserProfileInfo upi = setting.UserProfileList[index];
            MessageBoxResult result =  MessageBoxEx.Show(this,"プロファイル「" + upi.Profile.Name + "」を削除してもよろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if( result == MessageBoxResult.Yes )
            {
                MainWindow.Current.RemoveUserProfileInfo(upi);
                ProfileListBox.Items.RemoveAt(index);
                if(index < ProfileListBox.Items.Count - 1 )
                    ProfileListBox.SelectedIndex = index;
                else if (ProfileListBox.Items.Count > 0)
                    ProfileListBox.SelectedIndex = ProfileListBox.Items.Count - 1;

                UpdateNumberingItemTextAll();
            }
        }

        private void ProfileList_Up_Click(object sender, RoutedEventArgs e)
        {
            int index = ProfileListBox.SelectedIndex;
            if( index <= 0 || index > ProfileListBox.Items.Count - 1) return;

            UserProfileInfo upi = setting.UserProfileList[index];
            setting.UserProfileList.RemoveAt(index);
            setting.UserProfileList.Insert(index - 1, upi);

            ListBoxItem lbi = ProfileListBox.Items[index] as ListBoxItem;
            ProfileListBox.Items.RemoveAt(index);
            ProfileListBox.Items.Insert(index - 1, lbi);

            ProfileListBox.SelectedIndex = index - 1;

            UpdateNumberingItemTextAll();
        }

        private void ProfileList_Down_Click(object sender, RoutedEventArgs e)
        {
            int index = ProfileListBox.SelectedIndex;
            if( index >= ProfileListBox.Items.Count - 1 || index < 0) return;

            UserProfileInfo upi = setting.UserProfileList[index];
            setting.UserProfileList.RemoveAt(index);
            setting.UserProfileList.Insert(index + 1, upi);

            ListBoxItem lbi = ProfileListBox.Items[index] as ListBoxItem;
            ProfileListBox.Items.RemoveAt(index);
            ProfileListBox.Items.Insert(index + 1, lbi);

            ProfileListBox.SelectedIndex = index + 1;

            UpdateNumberingItemTextAll();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
