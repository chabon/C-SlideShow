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
        StartUp     = 0,
        Shortcut    = 1,
        History     = 2,
        AspectRatio = 3,
        ExternalApp = 4,
        Detail      = 5
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
        public int CommandValue { get; set; }
        public string CommandStrValue { get; set; }
    }

    public class ExternalAppListViewItem :INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if(value != this.name)
                {
                    this.name = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                if(value != this.path)
                {
                    this.path = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string arg = "\"$FilePath$\"";
        public string Arg
        {
            get { return arg; }
            set
            {
                if(value != this.arg)
                {
                    this.arg = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string showContextMenu = "○";
        public string ShowContextMenu
        {
            get { return showContextMenu; }
            set
            {
                if(value != this.showContextMenu)
                {
                    this.showContextMenu = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public ExternalAppListViewItem Clone()
        {
            ExternalAppListViewItem newItem = new ExternalAppListViewItem();
            newItem.Name = this.Name;
            newItem.Path = this.Path;
            newItem.Arg  = this.Arg;
            newItem.showContextMenu = this.ShowContextMenu;

            return newItem;
        }
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
        bool isSettingToTextBox = false;
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
                MainWindow.Current.Setting.AppSettingDialog_ShortcutSettingTabIndex = ShortcutSettingTab.SelectedIndex;
                MainWindow.Current.Setting.AppSettingDialogSize = new Size(this.Width, this.Height);

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

            // 起動時
            StartUp_RestoreWindowSizeAndPos.IsChecked   = setting.StartUp_RestoreWindowSizeAndPos;
            StartUp_LoadLastFiles.IsChecked             = setting.StartUp_LoadLastFiles;
            StartUp_RestoreLastPageIndex.IsChecked      = setting.StartUp_RestoreLastPageIndex;
            StartUp_RestoreSlideShowPlaying.IsChecked   = setting.StartUp_RestoreSlideShowPlaying;


            // ショートカット
            foreach(CommandMap commandMap in setting.ShortcutSetting.CommandMap )
            {
                Shortcut.ICommand cmd = MainWindow.Current.ShortcutManager.GetCommand(commandMap.CommandID);
                if( cmd == null ) continue;

                ListView targetListView;
                switch( cmd.Scene )
                {
                    default:
                    case Scene.All:
                        targetListView = ShortcutListView_ALL;
                        break;
                    case Scene.Nomal:
                        targetListView = ShortcutListView_Normal;
                        break;
                    case Scene.Expand:
                        targetListView = ShortcutListView_Expand;
                        break;
                }

                if( cmd.EnableValue ) cmd.Value = commandMap.CommandValue;
                else if( cmd.EnableStrValue ) cmd.StrValue = commandMap.CommandStrValue;
                string commandStr = cmd.GetDetail();

                targetListView.Items.Add(  new ShortcutListViewItem {
                    CommandStr      = commandStr,                                  CommandID         = commandMap.CommandID,
                    CommandValue    = commandMap.CommandValue,                     CommandStrValue   = commandMap.CommandStrValue,
                    KeyStr          = commandMap.KeyInput?.ToString(),             KeyInput          = commandMap.KeyInput?.Clone(),
                    MouseInputStr   = commandMap.MouseInput?.ToString(),           MouseInput        = commandMap.MouseInput?.Clone(),
                    MouseGestureStr = commandMap.MouseGestureInput?.ToString(),    MouseGestureInput = commandMap.MouseGestureInput?.Clone()}  );
            }

            MouseGestureRange.Value = setting.MouseGestureRange;
            LongClickDecisionTime.Value = setting.LongClickDecisionTime;


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
            foreach(ExternalAppInfo exAppInfo in setting.ExternalAppInfoList )
            {
                ExternalAppListView.Items.Add(new ExternalAppListViewItem
                {
                    Name            = exAppInfo.Name,
                    Path            = exAppInfo.Path,
                    Arg             = exAppInfo.Arg,
                    ShowContextMenu = exAppInfo.ShowContextMenu ? "○" : "×"
                });
            };


            // 詳細
            ShowMenuItem_AdditionalRead.IsChecked = setting.ShowMenuItem_AdditionalRead;
            SerachAllDirectoriesInFolderReading.IsChecked = setting.SerachAllDirectoriesInFolderReading;

            SeekbarColor.PickedColor = setting.SeekbarColor;

            SeekBarIsMoveToPointEnabled.SelectedIndex = setting.SeekBarIsMoveToPointEnabled ? 1 : 0;

            MouseCursorAutoHide.IsChecked = setting.MouseCursorAutoHide;
            MouseCursorAutoHideInFullScreenModeOnly.IsChecked = setting.MouseCursorAutoHideInFullScreenModeOnly;
            if( setting.MouseCursorAutoHide ) MouseCursorAutoHideInFullScreenModeOnly.IsEnabled = true;
            else MouseCursorAutoHideInFullScreenModeOnly.IsEnabled = false;

            CorrectPageIndexInOperationSlideCrrosOverTheOrigin.IsChecked = setting.CorrectPageIndexInOperationSlideCrrosOverTheOrigin;

            OperationSlideDuration.Value = setting.OperationSlideDuration;

            EnableScreenSnap.IsChecked = setting.EnableScreenSnap;
            EnableWindowSnap.IsChecked = setting.EnableWindowSnap;
            ScreenSnapRange.Value = setting.ScreenSnapRange;
            WindowSnapRange.Value = setting.WindowSnapRange;
            if( setting.EnableScreenSnap ) ScreenSnapRange.IsEnabled = true;
            else ScreenSnapRange.IsEnabled = false;
            if( setting.EnableWindowSnap ) WindowSnapRange.IsEnabled = true;
            else WindowSnapRange.IsEnabled = false;

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

        // ショートカット 重複チェック
        private bool DoubleCheck<T>(T input, ShortcutListViewItem own) where T : class, IEquatable<T>
        {
            // 重複したアイテムのリスト
            List<ShortcutListViewItem> doubleItemList = new List<ShortcutListViewItem>();

            Func<ShortcutListViewItem, object> getInput = (si) =>
            {
                if( typeof(T) == typeof(KeyInput) ) return si.KeyInput;
                else if( typeof(T) == typeof(MouseInput) ) return si.MouseInput;
                else if( typeof(T) == typeof(MouseGestureInput) ) return si.MouseGestureInput;
                else return null;
            };

            // チェック
            Action<ListView> doubleCheck = (listView) =>
            {
                foreach( var li in listView.Items )
                {
                    ShortcutListViewItem si = li as ShortcutListViewItem;
                    T otherInput = getInput(si) as T;
                    if(si != null && otherInput != null && si != own )
                    {
                        if( otherInput.Equals(input) )
                        {
                            doubleItemList.Add(si);
                        }
                    }
                }
            };

            doubleCheck.Invoke(ShortcutListView_ALL);
            switch(ShortcutSettingTab.SelectedIndex)
            {
                case 0: // いつでも
                    doubleCheck.Invoke(ShortcutListView_Normal);
                    doubleCheck.Invoke(ShortcutListView_Expand);
                    break;
                case 1: // 通常時
                    doubleCheck.Invoke(ShortcutListView_Normal);
                    break;
                case 2: // 拡大時
                    doubleCheck.Invoke(ShortcutListView_Expand);
                    break;
            }

            // 確認ダイアログ
            if(doubleItemList.Count > 0 )
            {
                string message = input.ToString() +  "\n\nは以下のコマンドに割り当てられています。\n置き換えてもよろしいですか？";
                message += "\n";
                foreach(var item in doubleItemList )
                {
                    ShortcutListViewItem si = item as ShortcutListViewItem;
                    if(si != null )
                    {
                        message += "\n";
                        C_SlideShow.Shortcut.ICommand cmd = MainWindow.Current.ShortcutManager.GetCommand(si.CommandID);
                        switch( cmd.Scene )
                        {
                            case Scene.All:    message += "[いつでも] "; break;
                            case Scene.Nomal:  message += "[通常時] "; break;
                            case Scene.Expand: message += "[拡大時] "; break;
                        }
                        message += si.CommandStr;
                    }
                }

                MessageBoxResult result =  MessageBoxEx.Show(this, message, "重複確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if( result == MessageBoxResult.Yes )
                {
                    foreach(var item in doubleItemList )
                    {
                        ShortcutListViewItem si = item as ShortcutListViewItem;
                        if(si != null )
                        {
                            if( typeof(T) == typeof(KeyInput) )
                            {
                                si.KeyInput = null;
                                si.KeyStr = "";
                            }
                            else if( typeof(T) == typeof(MouseInput) )
                            {
                                si.MouseInput = null;
                                si.MouseInputStr = "";
                            }
                            else if( typeof(T) == typeof(MouseGestureInput) )
                            {
                                si.MouseGestureInput = null;
                                si.MouseGestureStr = "";
                            }
                        }
                    }

                    return true;
                }
                else
                {
                    return false;

                }

            }

            return true;
        }

        // キー
        private void SetKeyInputToControl(KeyInput keyInput)
        {
            if( keyInput == null ) { keyInput = new KeyInput(Key.None); }

            HotkeyControl.SetKey( keyInput.Modifiers, keyInput.Key );

            KeyInputModifire_Shift.IsChecked = (  ( (int)keyInput.Modifiers & (int)ModifierKeys.Shift    ) != 0  ) ? true : false;
            KeyInputModifire_Ctrl.IsChecked  = (  ( (int)keyInput.Modifiers & (int)ModifierKeys.Control  ) != 0  ) ? true : false;
            KeyInputModifire_Alt.IsChecked   = (  ( (int)keyInput.Modifiers & (int)ModifierKeys.Alt      ) != 0  ) ? true : false;
        }

        private void ClearKeyInputControl()
        {
            HotkeyControl.Clear();

            KeyInputModifire_Shift.IsChecked = false;
            KeyInputModifire_Ctrl.IsChecked  = false;
            KeyInputModifire_Alt.IsChecked   = false; 
        }

        // マウスインプット
        private void SetMouseInputToControl(MouseInput mouseInput)
        {
            isSettingToComboBox = true;

            if( mouseInput == null ) mouseInput = new MouseInput(Shortcut.MouseInputButton.None, ModifierKeys.None);
            MouseInputModifire_Shift.IsChecked = (  ( (int)mouseInput.ModifierKeys & (int)ModifierKeys.Shift    ) != 0  ) ? true : false;
            MouseInputModifire_Ctrl.IsChecked  = (  ( (int)mouseInput.ModifierKeys & (int)ModifierKeys.Control  ) != 0  ) ? true : false;
            MouseInputModifire_Alt.IsChecked   = (  ( (int)mouseInput.ModifierKeys & (int)ModifierKeys.Alt      ) != 0  ) ? true : false;

            MouseInputButton.SelectedIndex = (int)mouseInput.MouseInputButton;

            isSettingToComboBox = false;
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

        // マウスジェスチャ
        private void SetMouseGestureInputToControl(MouseGestureInput gestureInput)
        {
            if( gestureInput == null ) return;

            MouseGestureControl.SetValue(gestureInput.Stroke, gestureInput.StartingButton);
        }

        // 外部連携
        private ExternalAppListViewItem GetSelectedExternalAppListViewItem()
        {
            ExternalAppListViewItem item = ExternalAppListView.SelectedItem as ExternalAppListViewItem;
            return item ?? item;
        }


        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */

        // 起動時設定
        private void AllDefault_StartUp_Click(object sender, RoutedEventArgs e)
        {
            StartUp_RestoreWindowSizeAndPos.IsChecked = true;
            StartUp_LoadLastFiles.IsChecked           = true;
            StartUp_RestoreLastPageIndex.IsChecked    = true;
            StartUp_RestoreSlideShowPlaying.IsChecked = true;
        }

        // ショートカット設定 (共通)
        private void ShortcutListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;

            ListView currentListView = GetCurrentShortcutListView();
            ShortcutListViewItem item = currentListView.SelectedItem as ShortcutListViewItem;

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
                KeyInputModifire_Shift.IsEnabled = true;
                KeyInputModifire_Ctrl.IsEnabled = true;
                KeyInputModifire_Alt.IsEnabled = true;
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
                KeyInputModifire_Shift.IsEnabled = false;
                KeyInputModifire_Ctrl.IsEnabled  = false;
                KeyInputModifire_Alt.IsEnabled = false;
            }

            // 詳細ボタン
            if(item != null )
            {
                CopySelectedCommand.IsEnabled = true;

                int cnt = 0;
                foreach(object i in currentListView.Items )
                {
                    ShortcutListViewItem si = i as ShortcutListViewItem;
                    if(i != null )
                    {
                        if( si.CommandID == item.CommandID ) cnt++;
                    }
                }

                if( cnt >= 2 ) DeleteSelectedCommand.IsEnabled = true;
                else DeleteSelectedCommand.IsEnabled = false;
            }
            else
            {
                CopySelectedCommand.IsEnabled = false;
                DeleteSelectedCommand.IsEnabled = false;
            }

            // コマンドの値
            if(item != null )
            {
                Shortcut.ICommand cmd = MainWindow.Current.ShortcutManager.GetCommand(item.CommandID);
                if(cmd.EnableValue || cmd.EnableStrValue )
                {
                    CommandValue.Visibility = Visibility.Visible;
                    CommandValueLabel.Visibility = Visibility.Visible;
                    if( cmd.EnableValue ) CommandValue.Text = item.CommandValue.ToString();
                    else if( cmd.EnableStrValue ) CommandValue.Text = item.CommandStrValue;
                }
                else
                {
                    CommandValue.Visibility = Visibility.Collapsed;
                    CommandValueLabel.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                CommandValue.Visibility = Visibility.Collapsed;
                CommandValueLabel.Visibility = Visibility.Collapsed;
            }

            // キー
            if(item != null && item.KeyInput != null)
            {
                SetKeyInputToControl(item.KeyInput);
            }
            else
            {
                ClearKeyInputControl();
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
            List<CommandMap> defaultCommandMap = ShortcutSetting.CreateDefaultCommandMap();

            // シーン(現在のタブ)取得
            Shortcut.Scene scene;
            ListView currentListView = GetCurrentShortcutListView();
            if( currentListView == ShortcutListView_ALL ) scene = Shortcut.Scene.All;
            else if( currentListView == ShortcutListView_Normal ) scene = Shortcut.Scene.Nomal;
            else if( currentListView == ShortcutListView_Expand ) scene = Shortcut.Scene.Expand;
            else return;

            // リストクリア
            currentListView.Items.Clear();

            // リストに追加
            foreach(CommandMap commandMap in defaultCommandMap )
            {
                Shortcut.ICommand cmd = MainWindow.Current.ShortcutManager.GetCommand(commandMap.CommandID);
                if(cmd.Scene == scene )
                {
                    if( cmd.EnableValue ) cmd.Value = commandMap.CommandValue;
                    else if( cmd.EnableStrValue ) cmd.StrValue = commandMap.CommandStrValue;
                    string commandStr = cmd.GetDetail();

                    currentListView.Items.Add(  new ShortcutListViewItem {
                        CommandStr      = commandStr,                                  CommandID         = commandMap.CommandID,
                        CommandValue    = commandMap.CommandValue,                     CommandStrValue   = commandMap.CommandStrValue,
                        KeyStr          = commandMap.KeyInput?.ToString(),             KeyInput          = commandMap.KeyInput?.Clone(),
                        MouseInputStr   = commandMap.MouseInput?.ToString(),           MouseInput        = commandMap.MouseInput?.Clone(),
                        MouseGestureStr = commandMap.MouseGestureInput?.ToString(),    MouseGestureInput = commandMap.MouseGestureInput?.Clone()}  );
                }
            }

            ShortcutListView_SelectionChanged(this, null);
        }

        private void ShortcutSettingTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;
            ShortcutListView_SelectionChanged(this, null);
        }

       private void CopySelectedCommand_Click(object sender, RoutedEventArgs e)
        {
            ListView currentShortcutList = GetCurrentShortcutListView();
            ShortcutListViewItem item = currentShortcutList.SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            // 複製
            ShortcutListViewItem copyItem = new ShortcutListViewItem
            {
                CommandStr = item.CommandStr,
                CommandID = item.CommandID,
                CommandValue = item.CommandValue,
                CommandStrValue = item.CommandStrValue
            };

            // リストに挿入
            int index = currentShortcutList.SelectedIndex;
            currentShortcutList.Items.Insert(index + 1, copyItem);

            ShortcutListView_SelectionChanged(this, null);
        }

        private void DeleteSelectedCommand_Click(object sender, RoutedEventArgs e)
        {
            ListView currentShortcutList = GetCurrentShortcutListView();
            ShortcutListViewItem item = currentShortcutList.SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            int index = currentShortcutList.SelectedIndex;
            currentShortcutList.Items.Remove(item);
            if( index < currentShortcutList.Items.Count ) currentShortcutList.SelectedIndex = index;
        }

        private void Shortcut_DetailButton_Click(object sender, RoutedEventArgs e)
        {
            Shortcut_DetailButton.ContextMenu.IsOpen = true;
        }

        // ショートカット設定 コマンドの値
        private void CommandValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            Shortcut.ICommand cmd = MainWindow.Current.ShortcutManager.GetCommand(item.CommandID);
            if( cmd == null ) return;

            if( cmd.EnableValue )
            {
                int val;
                try { val = Int32.Parse(CommandValue.Text); }
                catch { val = 0; }
         
                item.CommandValue = val;
                cmd.Value = val;
                item.CommandStr = cmd.GetDetail();
            }
            else if( cmd.EnableStrValue )
            {
                item.CommandStrValue = CommandValue.Text;
                cmd.StrValue = CommandValue.Text;
                item.CommandStr = cmd.GetDetail();
            }
        }

        // ショートカット設定 (キー)
        private void HotkeyControl_KeyAssigned(object sender, EventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;
            KeyInput ki = new KeyInput(HotkeyControl.Modifiers, HotkeyControl.Key);

            // 重複キーチェック
            if( DoubleCheck<KeyInput>(ki, item) )
            {
                item.KeyInput = ki;
                item.KeyStr = ki.ToString();
            }
            SetKeyInputToControl(item.KeyInput);
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

        private void KeyInputModifier_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            if( item.KeyInput == null ) return;

            ModifierKeys modifierKeys = ModifierKeys.None;
            if( (bool)KeyInputModifire_Shift.IsChecked ) modifierKeys |= ModifierKeys.Shift;
            if( (bool)KeyInputModifire_Ctrl.IsChecked  ) modifierKeys |= ModifierKeys.Control;
            if( (bool)KeyInputModifire_Alt.IsChecked   ) modifierKeys |= ModifierKeys.Alt;

            KeyInput ki = new KeyInput(modifierKeys, item.KeyInput.Key);

            // 重複チェック
            if( DoubleCheck<KeyInput>(ki, item) )
            {
                item.KeyInput = ki;
                item.KeyStr = item.KeyInput.ToString();
            }
            SetKeyInputToControl(item.KeyInput);
        }

        // ショートカット設定 (マウス入力)
        private void MouseInputButton_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;
            if( isSettingToComboBox ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            MouseInput newMouseInput = new MouseInput((MouseInputButton)MouseInputButton.SelectedIndex, ModifierKeys.None);
            if(item.MouseInput != null )
            {
                newMouseInput.ModifierKeys = item.MouseInput.ModifierKeys;
            }

            // 重複チェック
            if( DoubleCheck<MouseInput>(newMouseInput, item) )
            {
                item.MouseInput = newMouseInput;
                item.MouseInputStr = item.MouseInput.ToString();
            }

            SetMouseInputToControl(item.MouseInput);
        }

        private void MouseInputModifier_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            ShortcutListViewItem item = GetCurrentShortcutListView().SelectedItem as ShortcutListViewItem;
            if( item == null ) return;

            ModifierKeys modifierKeys = ModifierKeys.None;
            if( (bool)MouseInputModifire_Shift.IsChecked ) modifierKeys |= ModifierKeys.Shift;
            if( (bool)MouseInputModifire_Ctrl.IsChecked  ) modifierKeys |= ModifierKeys.Control;
            if( (bool)MouseInputModifire_Alt.IsChecked   ) modifierKeys |= ModifierKeys.Alt;

            MouseInput newMouseInput = new MouseInput(Shortcut.MouseInputButton.None, modifierKeys);

            if( item.MouseInput != null )
            {
                newMouseInput.MouseInputButton = item.MouseInput.MouseInputButton;
            }

            // 重複チェック
            if( DoubleCheck<MouseInput>(newMouseInput, item) )
            {
                item.MouseInput = newMouseInput;
                item.MouseInputStr = item.MouseInput.ToString();
            }

            SetMouseInputToControl(item.MouseInput);
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
                // 重複チェック
                if( DoubleCheck<MouseGestureInput>(gestureInput, item) )
                {
                    item.MouseGestureInput = gestureInput;
                    item.MouseGestureStr = gestureInput.ToString();
                }
            }

            if(item.MouseGestureInput != null )
            {
                SetMouseGestureInputToControl(item.MouseGestureInput);
            }
            else
            {
                MouseGestureControl.StartAcceptingInput();
            }
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
            MessageBoxResult result =  MessageBoxEx.Show(this,"全てのフォルダ(書庫)の履歴を削除してもよろしいですか？\r\nこの操作はキャンセル出来ません", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
        private void ExternalAppListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( isInitializing ) return;

            ExternalAppListViewItem item = ExternalAppListView.SelectedItem as ExternalAppListViewItem;

            if(item != null )
            {
                ExternalAppName.IsEnabled = true;
                ExternalAppPath.IsEnabled = true;
                ExternalAppPathBrowse.IsEnabled = true;
                ExternalAppArg.IsEnabled = true;
                ExternalApp_ShowContextMenu.IsEnabled = true;
            }
            else
            {
                ExternalAppName.IsEnabled = false;
                ExternalAppPath.IsEnabled = false;
                ExternalAppPathBrowse.IsEnabled = false;
                ExternalAppArg.IsEnabled = false;
                ExternalApp_ShowContextMenu.IsEnabled = false;
            }

            if(item != null )
            {
                isSettingToTextBox = true;
                ExternalAppName.Text = item.Name;
                ExternalAppPath.Text = item.Path;
                ExternalAppArg.Text  = item.Arg;
                ExternalApp_ShowContextMenu.IsChecked = item.ShowContextMenu == "○" ? true : false;
                isSettingToTextBox = false;
            }
            else
            {
                ExternalAppName.Text = "";
                ExternalAppPath.Text = "";
                ExternalAppArg.Text  = "";
                ExternalApp_ShowContextMenu.IsChecked = false;
            }
        }

        private void ExternalApp_New_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            ExternalAppListViewItem item = new ExternalAppListViewItem();
            ExternalAppListView.Items.Add(item);
            ExternalAppListView.SelectedItem = item;
        }

        private void ExternalApp_Del_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            var item = GetSelectedExternalAppListViewItem();
            if( item != null )
            {
                int index = ExternalAppListView.SelectedIndex;
                ExternalAppListView.Items.Remove(item);
                if(ExternalAppListView.Items.Count > index )
                {
                    ExternalAppListView.SelectedIndex = index;
                }
                else if(ExternalAppListView.Items.Count > 0 )
                {
                    ExternalAppListView.SelectedIndex = index - 1;
                }
            }

        }

        private void ExternalApp_Copy_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            var item = GetSelectedExternalAppListViewItem();
            if(item != null )
            {
                ExternalAppListViewItem newItem = item.Clone();

                int index = ExternalAppListView.SelectedIndex;
                ExternalAppListView.Items.Insert(index, newItem);
            }
        }

        private void ExternalApp_Up_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            var item = GetSelectedExternalAppListViewItem();
            if( item != null )
            {
                int index = ExternalAppListView.SelectedIndex;
                if( index <= 0 ) return;

                ExternalAppListView.Items.Remove(item);
                ExternalAppListView.Items.Insert(index - 1, item);
                ExternalAppListView.SelectedIndex = index - 1;
            }
        }

        private void ExternalApp_Down_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            var item = GetSelectedExternalAppListViewItem();
            if( item != null )
            {
                int index = ExternalAppListView.SelectedIndex;
                if( index >= ExternalAppListView.Items.Count - 1) return;

                ExternalAppListView.Items.Remove(item);
                ExternalAppListView.Items.Insert(index + 1, item);
                ExternalAppListView.SelectedIndex = index + 1;
            }
        }


        private void ExternalAppName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if( isInitializing ) return;
            if( isSettingToTextBox ) return;

            var item = GetSelectedExternalAppListViewItem();
            if(item != null)  item.Name = ExternalAppName.Text;
        }

        private void ExternalAppPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if( isInitializing ) return;
            if( isSettingToTextBox ) return;

            var item = GetSelectedExternalAppListViewItem();
            if(item != null)  item.Path = ExternalAppPath.Text;
        }

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

        private void ExternalAppArg_TextChanged(object sender, TextChangedEventArgs e)
        {
            if( isInitializing ) return;
            if( isSettingToTextBox ) return;

            var item = GetSelectedExternalAppListViewItem();
            if(item != null)  item.Arg = ExternalAppArg.Text;
        }

        private void ExternalApp_ShowContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if( isInitializing ) return;

            var item = GetSelectedExternalAppListViewItem();
            if(item != null )
            {
                item.ShowContextMenu = (bool)ExternalApp_ShowContextMenu.IsChecked ? "○" : "×";
            }
        }

        // 詳細
        private void MouseCursorAutoHide_Click(object sender, RoutedEventArgs e)
        {
            if( (bool)MouseCursorAutoHide.IsChecked ) MouseCursorAutoHideInFullScreenModeOnly.IsEnabled = true;
            else MouseCursorAutoHideInFullScreenModeOnly.IsEnabled = false;
        }

        private void EnableScreenSnap_Click(object sender, RoutedEventArgs e)
        {
            if( (bool)EnableScreenSnap.IsChecked ) ScreenSnapRange.IsEnabled = true;
            else ScreenSnapRange.IsEnabled = false;
        }

        private void EnableWindowSnap_Click(object sender, RoutedEventArgs e)
        {
            if( (bool)EnableWindowSnap.IsChecked ) WindowSnapRange.IsEnabled = true;
            else WindowSnapRange.IsEnabled = false;
        }

        private void AllDefault_Detail_Click(object sender, RoutedEventArgs e)
        {
            ShowMenuItem_AdditionalRead.IsChecked = true;
            SerachAllDirectoriesInFolderReading.IsChecked = true;
            SeekbarColor.PickedColor = Colors.Black;
            SeekBarIsMoveToPointEnabled.SelectedIndex = 1;
            MouseCursorAutoHide.IsChecked = true;
            MouseCursorAutoHideInFullScreenModeOnly.IsChecked = false;
            MouseCursorAutoHideInFullScreenModeOnly.IsEnabled = true;
            CorrectPageIndexInOperationSlideCrrosOverTheOrigin.IsChecked = true;
            OperationSlideDuration.Value = 300;
            EnableScreenSnap.IsEnabled = true;
            EnableWindowSnap.IsChecked = true;
            ScreenSnapRange.Value = 10;
            WindowSnapRange.Value = 10;
            ScreenSnapRange.IsEnabled = true;
            WindowSnapRange.IsEnabled = true;
        }


        /* ---------------------------------------------------- */
        //     OK / キャンセル
        /* ---------------------------------------------------- */
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 起動時設定
            setting.StartUp_RestoreWindowSizeAndPos = (bool)StartUp_RestoreWindowSizeAndPos.IsChecked; 
            setting.StartUp_LoadLastFiles           = (bool)StartUp_LoadLastFiles.IsChecked; 
            setting.StartUp_RestoreLastPageIndex    = (bool)StartUp_RestoreLastPageIndex.IsChecked; 
            setting.StartUp_RestoreSlideShowPlaying = (bool)StartUp_RestoreSlideShowPlaying.IsChecked; 

            // ショートカット設定 コマンドマッピング
            List<CommandMap> commandMapList = new List<CommandMap>();
            Action<ListView> addToCommandMapList = (ListView listView) =>
            {
                foreach(var item in listView.Items )
                {
                    ShortcutListViewItem si = item as ShortcutListViewItem;
                    if(si != null )
                    {
                        Shortcut.ICommand cmd = MainWindow.Current.ShortcutManager.GetCommand(si.CommandID);

                        KeyInput            keyInput            = si.KeyInput           != null? si.KeyInput            : null;
                        MouseInput          mouseInput          = si.MouseInput         != null? si.MouseInput          : null;
                        MouseGestureInput   mouseGestureInput   = si.MouseGestureInput  != null? si.MouseGestureInput   : null;

                        CommandMap commandMap = new CommandMap(si.CommandID, si.CommandValue, si.CommandStrValue, keyInput, mouseInput, mouseGestureInput);
                        commandMapList.Add(commandMap);
                    }
                }
            };

            addToCommandMapList.Invoke(ShortcutListView_ALL);
            addToCommandMapList.Invoke(ShortcutListView_Normal);
            addToCommandMapList.Invoke(ShortcutListView_Expand);
            setting.ShortcutSetting.CommandMap = commandMapList;

            // ショートカット設定 長押し判定時間
            setting.LongClickDecisionTime = LongClickDecisionTime.Value;

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
            setting.ExternalAppInfoList.Clear();
            foreach(var item in ExternalAppListView.Items )
            {
                ExternalAppListViewItem i = item as ExternalAppListViewItem;
                if(i != null )
                {
                    ExternalAppInfo exAppInfo = new ExternalAppInfo();
                    exAppInfo.Name = i.Name;
                    exAppInfo.Path = i.Path;
                    exAppInfo.Arg  = i.Arg;
                    exAppInfo.ShowContextMenu = i.ShowContextMenu == "○" ? true : false;

                    setting.ExternalAppInfoList.Add(exAppInfo);
                }
            }

            // 詳細
            setting.ShowMenuItem_AdditionalRead = (bool)ShowMenuItem_AdditionalRead.IsChecked ;
            setting.SerachAllDirectoriesInFolderReading = (bool)SerachAllDirectoriesInFolderReading.IsChecked;
            setting.SeekbarColor = SeekbarColor.PickedColor;
            setting.SeekBarIsMoveToPointEnabled = SeekBarIsMoveToPointEnabled.SelectedIndex == 1 ? true : false;
            setting.MouseCursorAutoHide = (bool)MouseCursorAutoHide.IsChecked;
            setting.MouseCursorAutoHideInFullScreenModeOnly = (bool)MouseCursorAutoHideInFullScreenModeOnly.IsChecked;
            setting.CorrectPageIndexInOperationSlideCrrosOverTheOrigin = (bool)CorrectPageIndexInOperationSlideCrrosOverTheOrigin.IsChecked;
            setting.OperationSlideDuration = OperationSlideDuration.Value;
            setting.EnableScreenSnap = (bool)EnableScreenSnap.IsChecked;
            setting.EnableWindowSnap = (bool)EnableWindowSnap.IsChecked;
            setting.ScreenSnapRange = ScreenSnapRange.Value;
            setting.WindowSnapRange = WindowSnapRange.Value;

            // メインウインドウ更新
            MainWindow.Current.UpdateUI();
            MainWindow.Current.Seekbar.IsMoveToPointEnabled = setting.SeekBarIsMoveToPointEnabled;

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }













        // end of class
    }
}
