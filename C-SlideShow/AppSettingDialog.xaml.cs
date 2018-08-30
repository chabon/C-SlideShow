﻿using System;
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
        public MouseGestureInput MouseGestureInput { get; set; }
    }

    /// <summary>
    /// AppSettingDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AppSettingDialog : Window
    {
        /* ---------------------------------------------------- */
        //     フィールド
        /* ---------------------------------------------------- */
        bool isInitializing = false;
        bool isSettingToComboBox = false;  // コード中でComboBoxのSelectedItemが変更された時のイベントを防止するため
        AppSetting setting;

        /* ---------------------------------------------------- */
        //     コンストラクタ
        /* ---------------------------------------------------- */
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

        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */
        // 共通
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
                        MouseGestureStr = gm?.GestureInput.ToString(), MouseGestureInput = gm?.GestureInput.Clone()}  );
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

        private void SelectComboBoxItemByTag(ComboBox comboBox, string tag)
        {
            isSettingToComboBox = true;
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
            isSettingToComboBox = false;
        }

        private ComboBoxItem GetComboBoxItemByTag(ComboBox comboBox, string tag)
        {
            foreach(ComboBoxItem item in comboBox.Items )
            {
                if( item.Tag.ToString() == tag )
                {
                    return item;
                }
            }
            return null;
        }

        // 履歴設定
        private void CorrectNumofHistory()
        {
            if( NumofHistory.Value < NumofHistoryInMenu.Value ) NumofHistoryInMenu.Value = NumofHistory.Value;
            if( NumofHistoryInMenu.Value < NumofHistoryInMainMenu.Value ) NumofHistoryInMainMenu.Value = NumofHistoryInMenu.Value;
        }

        // ショートカット 共通
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

        // マウスインプット
        private void SetMouseInputToControl(MouseInput mouseInput)
        {
            MouseInputModifire_Shift.IsChecked = (  ( (int)mouseInput.ModifierKeys & (int)ModifierKeys.Shift    ) != 0  ) ? true : false;
            MouseInputModifire_Ctrl.IsChecked  = (  ( (int)mouseInput.ModifierKeys & (int)ModifierKeys.Control  ) != 0  ) ? true : false;
            MouseInputModifire_Alt.IsChecked   = (  ( (int)mouseInput.ModifierKeys & (int)ModifierKeys.Alt      ) != 0  ) ? true : false;

            MouseInputButton.SelectedIndex = (int)mouseInput.MouseInputButton;
        }

        private void ClearMouseInputControl()
        {
            isSettingToComboBox = true;

            MouseInputButton.SelectedIndex = 0;
            MouseInputModifire_Shift.IsChecked = false;
            MouseInputModifire_Ctrl.IsChecked  = false;
            MouseInputModifire_Alt.IsChecked   = false; 

            isSettingToComboBox = false;
        }

        private void DoubleCheck_MouseInput(MouseInput mouseInput, ShortcutListViewItem own)
        {
            foreach( var li in GetCurrentShortcutListView().Items )
            {
                ShortcutListViewItem si = li as ShortcutListViewItem;
                if(si != null && si.MouseInput != null && si != own )
                {
                    if( si.MouseInput.Equals(mouseInput) )
                    {
                        si.MouseInput = null;
                        si.MouseInputStr = "";
                    }
                }
            }
        }

        // マウスジェスチャ
        private void SetMouseGestureInputToControl(MouseGestureInput gestureInput)
        {
            MouseGestureControl.SetValue(gestureInput.Stroke, gestureInput.StartingButton);
        }

        private void DoubleCheck_MouseGestureInput(MouseGestureInput gestureInput, ShortcutListViewItem own)
        {
            foreach( var li in GetCurrentShortcutListView().Items )
            {
                ShortcutListViewItem si = li as ShortcutListViewItem;
                if(si != null && si.MouseGestureInput != null && si != own )
                {
                    if( si.MouseGestureInput.Equals(gestureInput) )
                    {
                        si.MouseGestureInput = null;
                        si.MouseGestureStr = null;
                    }
                }
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
                MouseInputButton.IsEnabled = true;
                MouseInputMapClearButton.IsEnabled = true;
                MouseGestureControl.IsEnabled = true;
                MouseGestureClearButton.IsEnabled = true;
                MouseInputModifire_Shift.IsEnabled = true;
                MouseInputModifire_Ctrl.IsEnabled = true;
                MouseInputModifire_Alt.IsEnabled = true;
            }
            else
            {
                HotkeyControl.IsEnabled = false;
                KeymapClearButton.IsEnabled = false;
                MouseInputButton.IsEnabled = false;
                MouseInputMapClearButton.IsEnabled = false;
                MouseGestureControl.IsEnabled = false;
                MouseGestureClearButton.IsEnabled = false;
                MouseInputModifire_Shift.IsEnabled = false;
                MouseInputModifire_Ctrl.IsEnabled  = false;
                MouseInputModifire_Alt.IsEnabled = false;
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
            if(item != null && item.MouseGestureInput != null)
            {
                SetMouseGestureInputToControl(item.MouseGestureInput);
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
                        si.MouseGestureInput = null;
                        si.MouseGestureStr = null;
                        var mg = defaultMouseGesturemap.FirstOrDefault(k => k.CommandID == si.CommandID);
                        if(mg != null )
                        {
                            si.MouseGestureInput = mg.GestureInput;
                            si.MouseGestureStr = mg.GestureInput.ToString();
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
        private void MouseInputButton_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;
            if( isSettingToComboBox ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            if( item.MouseInput == null ) item.MouseInput = new MouseInput();

            item.MouseInput.MouseInputButton = (Shortcut.MouseInputButton)MouseInputButton.SelectedIndex;
            item.MouseInputStr = item.MouseInput.ToString();

            // 重複の削除
            DoubleCheck_MouseInput(item.MouseInput, item);
        }

        private void MouseInputModifire_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            if( item.MouseInput == null ) item.MouseInput = new MouseInput();

            ModifierKeys modifierKeys = ModifierKeys.None;
            if( (bool)MouseInputModifire_Shift.IsChecked ) modifierKeys |= ModifierKeys.Shift;
            if( (bool)MouseInputModifire_Ctrl.IsChecked  ) modifierKeys |= ModifierKeys.Control;
            if( (bool)MouseInputModifire_Alt.IsChecked   ) modifierKeys |= ModifierKeys.Alt;

            item.MouseInput.ModifierKeys = modifierKeys;
            item.MouseInputStr = item.MouseInput.ToString();

            // 重複の削除
            DoubleCheck_MouseInput(item.MouseInput, item);
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

            MouseGestureInput gestureInput = new MouseGestureInput(MouseGestureControl.StartingButton, MouseGestureControl.Stroke);
            if( gestureInput.Stroke.Length > 0 )
            {
                SetMouseGestureInputToControl(gestureInput);
                item.MouseGestureInput = gestureInput;
                item.MouseGestureStr = gestureInput.ToString();
            }
            else { return; }

            // 重複の削除
            DoubleCheck_MouseGestureInput(gestureInput, item);
        }

        private void MouseGestureControl_MainBorderLostFocus(object sender, EventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;
            if( item.MouseGestureInput == null ) return;

            MouseGestureControl.SetValue(item.MouseGestureInput.Stroke, item.MouseGestureInput.StartingButton);
        }

        private void MouseGestureClearButton_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            item.MouseGestureInput = null;
            item.MouseGestureStr = null;
            MouseGestureControl.Clear();
        }

        private void MouseGestureRange_ValueChanged(object sender, EventArgs e)
        {
            if( isInitializing ) return;

            MouseGestureControl.Range = MouseGestureRange.Value;
        }

        private void MouseGestureHelpButton_Click(object sender, RoutedEventArgs e)
        {
            MouseGestureHelpButton.ContextMenu.IsOpen = true;
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
                    if(si != null && si.MouseInput != null && si.MouseInput.MouseInputButton != Shortcut.MouseInputButton.None)
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
                    if(si != null && si.MouseGestureInput != null)
                    {
                        mouseGestureMapList.Add( new MouseGestureMap(si.MouseGestureInput, si.CommandID) );
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
