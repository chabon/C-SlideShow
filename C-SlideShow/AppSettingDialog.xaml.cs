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

using System.ComponentModel;
using System.Runtime.CompilerServices;

using Forms = System.Windows.Forms;
using C_SlideShow.Shortcut;


namespace C_SlideShow
{
    public enum AppSettingDialogTabIndex
    {
        Shortcut    = 0,
        History     = 1,
        AspectRatio = 2,
        ExternalApp = 3,
        Detail      = 4
    }

    public class ShortcutListViewItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string commandStr;
        public string CommandStr
        {
            get { return commandStr; }
            set
            {
                if(value != this.commandStr)
                {
                    this.commandStr = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string keyStr;
        public string KeyStr
        {
            get { return keyStr; }
            set
            {
                if(value != this.keyStr)
                {
                    this.keyStr = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string mouseInputStr;
        public string MouseInputStr
        {
            get { return mouseInputStr; }
            set
            {
                if(value != this.mouseInputStr)
                {
                    this.mouseInputStr = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string mouseGestureStr;
        public string MouseGestureStr
        {
            get { return mouseGestureStr; }
            set
            {
                if(value != this.mouseGestureStr)
                {
                    this.mouseGestureStr = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Shortcut.CommandID CommandID { get; set; }
        public KeyInput  KeyInput { get; set; }
        public MouseInput MouseInput { get; set; }
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

            // ショートカット
            foreach( Shortcut.ICommand command in MainWindow.Current.ShortcutManager.GetCommandList() )
            {
                KeyMap km = setting.ShortcutSetting.KeyMap.FirstOrDefault(k => k.CommandID == command.ID);
                MouseInputMap mm = setting.ShortcutSetting.MouseInputMap.FirstOrDefault(k => k.CommandID == command.ID);
                MouseGestureMap gm = setting.ShortcutSetting.MouseGestureMap.FirstOrDefault(k => k.CommandID == command.ID);

                Action<ListView> AddShortcutItemToListView = new Action<ListView>(listView => 
                {
                    listView.Items.Add(  new ShortcutListViewItem {
                        CommandStr = command.GetDetail(), CommandID = command.ID,
                        KeyStr = km?.KeyInput.ToString(), KeyInput = km?.KeyInput.Clone(),
                        MouseInputStr = mm?.MouseInput.ToString(), MouseInput = mm?.MouseInput.Clone(),
                        MouseGestureStr = gm?.Gesture}  );
                });

                switch( command.Scene )
                {
                    case Shortcut.Scene.All:
                        // 全般
                        AddShortcutItemToListView(ShortcutListView_ALL);
                        break;
                    case Shortcut.Scene.Nomal:
                        // 通常時
                        AddShortcutItemToListView(ShortcutListView_Normal);
                        break;
                    case Shortcut.Scene.Expand:
                        // 拡大時
                        AddShortcutItemToListView(ShortcutListView_Expand);
                        break;
                }
            }
            MouseGestureRange.Value = setting.MouseGestureRange;


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

        private ListView GetCurrentShortcutListView()
        {
            switch(ShortcutSettingTab.SelectedIndex)
            {
                default:
                case 0:
                    return ShortcutListView_ALL;
                case 1:
                    return ShortcutListView_Normal;
                case 2:
                    return ShortcutListView_Expand;
            }
        }

        private void SetMouseInputToControl(MouseInput mouseInput)
        {
            switch( mouseInput.MouseInputHold )
            {
                case Shortcut.MouseInputHold.None:
                    SelectComboBoxItemByTag(MouseInputHold, "None");
                    break;
                case Shortcut.MouseInputHold.L_Button:
                    SelectComboBoxItemByTag(MouseInputHold, "L_Button");
                    break;
                case Shortcut.MouseInputHold.R_Button:
                    SelectComboBoxItemByTag(MouseInputHold, "R_Button");
                    break;
                case Shortcut.MouseInputHold.M_Button:
                    SelectComboBoxItemByTag(MouseInputHold, "M_Button");
                    break;
                case Shortcut.MouseInputHold.Shift:
                    SelectComboBoxItemByTag(MouseInputHold, "Shift");
                    break;
                case Shortcut.MouseInputHold.Ctrl:
                    SelectComboBoxItemByTag(MouseInputHold, "Ctrl");
                    break;
                case Shortcut.MouseInputHold.Alt:
                    SelectComboBoxItemByTag(MouseInputHold, "Alt");
                    break;
            }

            UpdateMouseInputButtonItems();
            MouseInputButton.SelectedIndex = (int)mouseInput.MouseInputButton;
        }

        private void SelectComboBoxItemByTag(ComboBox comboBox, string tag)
        {
            ComboBoxItem tagMatchedItem = null;
            foreach(ComboBoxItem item in comboBox.Items )
            {
                if( item.Tag.ToString() == tag )
                {
                    tagMatchedItem = item;
                    break;
                }
            }

            if(tagMatchedItem != null )
            {
                comboBox.SelectedItem = tagMatchedItem;
            }
        }

        private void ClearMouseInputControl()
        {
            MouseInputHold.SelectedIndex = 0;
            UpdateMouseInputButtonItems();
            MouseInputButton.SelectedIndex = 0;
        }

        private void UpdateMouseInputButtonItems()
        {
            foreach(ComboBoxItem item in MouseInputButton.Items )
            {
                item.IsEnabled = true;
            }

            string tag = ( (ComboBoxItem)MouseInputHold.SelectedItem ).Tag.ToString();

            if( tag == "L_Button" )
            {
                ( (ComboBoxItem)MouseInputButton.Items[(int)Shortcut.MouseInputClick.L_Click] ).IsEnabled = false;
                ( (ComboBoxItem)MouseInputButton.Items[(int)Shortcut.MouseInputClick.L_DoubleClick] ).IsEnabled = false;
            }

            if( tag == "R_Button" )
            {
                ( (ComboBoxItem)MouseInputButton.Items[(int)Shortcut.MouseInputClick.R_Click] ).IsEnabled = false;
                ( (ComboBoxItem)MouseInputButton.Items[(int)Shortcut.MouseInputClick.R_DoubleClick] ).IsEnabled = false;
            }

            if( tag == "M_Button" )
            {
                ( (ComboBoxItem)MouseInputButton.Items[(int)Shortcut.MouseInputClick.M_Click] ).IsEnabled = false;
                ( (ComboBoxItem)MouseInputButton.Items[(int)Shortcut.MouseInputClick.WheelUp] ).IsEnabled = false;
                ( (ComboBoxItem)MouseInputButton.Items[(int)Shortcut.MouseInputClick.WheelDown] ).IsEnabled = false;
            }
        }

        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        // ショートカット設定 (共通)
        private void ShortcutListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;

            if( item != null )
            {
                HotkeyControl.IsEnabled = true;
                KeymapClearButton.IsEnabled = true;
                MouseInputHold.IsEnabled = true;
                MouseInputButton.IsEnabled = true;
                MouseInputMapClearButton.IsEnabled = true;
                MouseGestureControl.IsEnabled = true;
                MouseGestureClearButton.IsEnabled = true;
            }
            else
            {
                HotkeyControl.IsEnabled = false;
                KeymapClearButton.IsEnabled = false;
                MouseInputHold.IsEnabled = false;
                MouseInputButton.IsEnabled = false;
                MouseInputMapClearButton.IsEnabled = false;
                MouseGestureControl.IsEnabled = false;
                MouseGestureClearButton.IsEnabled = false;
            }

            // キー
            if(item != null && item.KeyInput != null)
            {
                HotkeyControl.SetKey( item.KeyInput.Modifiers, item.KeyInput.Key );
            }
            else
            {
                HotkeyControl.Clear();
            }

            // マウス入力
            if(item != null && item.MouseInput != null)
            {
                SetMouseInputToControl(item.MouseInput);
            }
            else
            {
                ClearMouseInputControl();
            }

            // マウスジェスチャ
            if(item != null && item.MouseGestureStr != null)
            {
                MouseGestureControl.SetStroke(item.MouseGestureStr);
            }
            else
            {
                MouseGestureControl.Clear();
            }
        }

        private void AllDefault_Shortcut_Click(object sender, RoutedEventArgs e)
        {
            var defaultKeymap = ShortcutSetting.CreateDefaultKeyMap();
            var defaultMouseInputmap = ShortcutSetting.CreateDefaultMouseInputMap();
            var defaultMouseGesturemap = ShortcutSetting.CreateDefaultMouseGestureMap();

            Action<ListView> makeAllItemsInListViewDefault = (ListView listView) =>
            {
                foreach( var li in listView.Items )
                {
                    ShortcutListViewItem si = li as ShortcutListViewItem;

                    if(si != null)
                    {
                        // キー
                        si.KeyInput = null;
                        si.KeyStr = "";
                        var km = defaultKeymap.FirstOrDefault(k => k.CommandID == si.CommandID);
                        if(km != null )
                        {
                            si.KeyInput = km.KeyInput;
                            si.KeyStr = km.KeyInput.ToString();
                        }

                        // マウス入力
                        si.MouseInput = null;
                        si.MouseInputStr = "";
                        var mm = defaultMouseInputmap.FirstOrDefault(k => k.CommandID == si.CommandID);
                        if(mm != null )
                        {
                            si.MouseInput = mm.MouseInput;
                            si.MouseInputStr = mm.MouseInput.ToString();
                        }

                        // マウスジェスチャ
                        si.MouseGestureStr = null;
                        var mg = defaultMouseGesturemap.FirstOrDefault(k => k.CommandID == si.CommandID);
                        if(mg != null )
                        {
                            si.MouseGestureStr = mg.Gesture;
                        }
                    }
                }
            };

            makeAllItemsInListViewDefault( GetCurrentShortcutListView() );
            ShortcutListView_SelectionChanged(this, null);
        }

        private void ShortcutSettingTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;
            ShortcutListView_SelectionChanged(this, null);
        }


        // ショートカット設定 (キー)
        private void HotkeyControl_KeyAssigned(object sender, EventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;
            KeyInput ki = new KeyInput(HotkeyControl.Modifiers, HotkeyControl.Key);

            item.KeyInput = ki;
            item.KeyStr = ki.ToString();

            // 重複キーの削除
            foreach( var li in GetCurrentShortcutListView().Items )
            {
                ShortcutListViewItem si = li as ShortcutListViewItem;
                if(si != null && si.KeyInput != null && si.CommandID != item.CommandID )
                {
                    if( si.KeyInput.Equals(ki) )
                    {
                        si.KeyInput = null;
                        si.KeyStr = "";
                    }
                }
            }
        }

        private void KeymapClearButton_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            item.KeyInput = null;
            item.KeyStr = "";
            HotkeyControl.Clear();
        }


        // ショートカット設定 (マウス入力)
        private void MouseInputHold_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            if( item.MouseInput == null ) item.MouseInput = new MouseInput();

            string tag = ( (ComboBoxItem)MouseInputHold.SelectedItem ).Tag.ToString();
            switch( tag )
            {
                case "None":
                    item.MouseInput.MouseInputHold = Shortcut.MouseInputHold.None;
                    break;
                case "L_Button":
                    item.MouseInput.MouseInputHold = Shortcut.MouseInputHold.L_Button;
                    break;
                case "R_Button":
                    item.MouseInput.MouseInputHold = Shortcut.MouseInputHold.R_Button;
                    break;
                case "M_Button":
                    item.MouseInput.MouseInputHold = Shortcut.MouseInputHold.M_Button;
                    break;
                case "Shift":
                    item.MouseInput.MouseInputHold = Shortcut.MouseInputHold.Shift;
                    break;
                case "Ctrl":
                    item.MouseInput.MouseInputHold = Shortcut.MouseInputHold.Ctrl;
                    break;
                case "Alt":
                    item.MouseInput.MouseInputHold = Shortcut.MouseInputHold.Alt;
                    break;
            }

            UpdateMouseInputButtonItems();

            // ホールドとボタンが同時指定になることを防止
            if( ( (ComboBoxItem)MouseInputButton.SelectedItem ).IsEnabled == false)
            {
                MouseInputButton.SelectedIndex = 0;
            }

            item.MouseInputStr = item.MouseInput.ToString();
        }

        private void MouseInputButton_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            if( item.MouseInput == null ) item.MouseInput = new MouseInput();

            item.MouseInput.MouseInputButton = (Shortcut.MouseInputClick)MouseInputButton.SelectedIndex;
            item.MouseInputStr = item.MouseInput.ToString();

            // 重複の削除
            foreach( var li in GetCurrentShortcutListView().Items )
            {
                ShortcutListViewItem si = li as ShortcutListViewItem;
                if(si != null && si.MouseInput != null && si.CommandID != item.CommandID )
                {
                    if( si.MouseInput.Equals(item.MouseInput) )
                    {
                        si.MouseInput = null;
                        si.MouseInputStr = "";
                    }
                }
            }
        }

        private void MouseInputMapClearButton_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            item.MouseInput = null;
            item.MouseInputStr = "";
            ClearMouseInputControl();
        }

        // ショートカット設定(マウスジェスチャ)
        private void MouseGestureControl_GestureAssigned(object sender, EventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;
            string stroke = MouseGestureControl.Stroke;

            item.MouseGestureStr = stroke;

            // 重複キーの削除
            foreach( var li in GetCurrentShortcutListView().Items )
            {
                ShortcutListViewItem si = li as ShortcutListViewItem;
                if(si != null && si.MouseGestureStr != null && si.CommandID != item.CommandID )
                {
                    if( si.MouseGestureStr == stroke )
                    {
                        si.MouseGestureStr = null;
                    }
                }
            }

        }

        private void MouseGestureClearButton_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            item.MouseGestureStr = null;
            MouseGestureControl.Clear();
        }

        private void MouseGestureRange_ValueChanged(object sender, EventArgs e)
        {
            if( isInitializing ) return;

            MouseGestureControl.Range = MouseGestureRange.Value;
        }


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
            // ショートカット設定 キー
            List<KeyMap> keymapList = new List<KeyMap>();
            Action<ListView> addToKeymapList = (ListView listView) =>
            {
                foreach(var item in listView.Items )
                {
                    ShortcutListViewItem si = item as ShortcutListViewItem;
                    if(si != null && si.KeyInput != null)
                    {
                        keymapList.Add( new KeyMap(si.KeyInput, si.CommandID) );
                    }
                }
            };
            addToKeymapList.Invoke(ShortcutListView_ALL);
            addToKeymapList.Invoke(ShortcutListView_Normal);
            addToKeymapList.Invoke(ShortcutListView_Expand);
            setting.ShortcutSetting.KeyMap = keymapList;

            // ショートカット設定 マウス入力
            List<MouseInputMap> mouseInputMapList = new List<MouseInputMap>();
            Action<ListView> addToMouseInputMapList = (ListView listView) =>
            {
                foreach(var item in listView.Items )
                {
                    ShortcutListViewItem si = item as ShortcutListViewItem;
                    if(si != null && si.MouseInput != null && si.MouseInput.MouseInputButton != Shortcut.MouseInputClick.None)
                    {
                        mouseInputMapList.Add( new MouseInputMap(si.MouseInput, si.CommandID) );
                    }
                }
            };
            addToMouseInputMapList.Invoke(ShortcutListView_ALL);
            addToMouseInputMapList.Invoke(ShortcutListView_Normal);
            addToMouseInputMapList.Invoke(ShortcutListView_Expand);
            setting.ShortcutSetting.MouseInputMap = mouseInputMapList;

            // ショートカット設定 マウスジェスチャ
            List<MouseGestureMap> mouseGestureMapList = new List<MouseGestureMap>();
            Action<ListView> addToMouseGestureMapList = (ListView listView) =>
            {
                foreach(var item in listView.Items )
                {
                    ShortcutListViewItem si = item as ShortcutListViewItem;
                    if(si != null && si.MouseGestureStr != null && si.MouseGestureStr != "")
                    {
                        mouseGestureMapList.Add( new MouseGestureMap(string.Copy(si.MouseGestureStr), si.CommandID) );
                    }
                }
            };
            addToMouseGestureMapList.Invoke(ShortcutListView_ALL);
            addToMouseGestureMapList.Invoke(ShortcutListView_Normal);
            addToMouseGestureMapList.Invoke(ShortcutListView_Expand);
            setting.ShortcutSetting.MouseGestureMap = mouseGestureMapList;

            // ショートカット設定 マウスジェスチャ判定距離
            setting.MouseGestureRange = MouseGestureRange.Value;

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



        // end of class
    }
}
