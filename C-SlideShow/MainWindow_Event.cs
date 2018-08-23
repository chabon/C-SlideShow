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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;

using Forms = System.Windows.Forms;
using C_SlideShow.Shortcut;



namespace C_SlideShow
{
    public partial class MainWindow
    {


        private void InitEvent()
        {
            /* ---------------------------------------------------- */
            //     init events
            /* ---------------------------------------------------- */

            MenuItem_Matrix.SubmenuOpened += (s, e) =>
            {
                matrixSelecter.SetMatrix(Setting.TempProfile.NumofMatrix.Col, Setting.TempProfile.NumofMatrix.Row);
            };

            MenuItem_SlideSetting.SubmenuOpened += (s, e) =>
            {
                slideSettingDialog.ApplySettingToDlg();
            };

            MenuItem_Setting.SubmenuOpened += (s, e) =>
            {
                settingDialog.ApplySettingToDlg();
            };


            this.SourceInitialized += (s, e) =>
            {
                // ウインドウ位置復元が正常に行われたかチェック
                Win32.SetWindowPosProperly(new WindowInteropHelper(this).Handle, (int)this.Left, (int)this.Top, true, 0, true);
            };

            this.SizeChanged += (s, e) =>
            {
                //return;
                if (ignoreResizeEvent) return;

                // 画像拡大パネルのサイズ更新、拡大中ならリセット
                if( TileExpantionPanel.IsShowing )
                {
                    TileExpantionPanel.ZoomReset();
                    TileExpantionPanel.FitToMainWindow();
                }

                // アス比非固定時
                if( Setting.TempProfile.NonFixAspectRatio.Value )
                {
                    // 現在のプロファイル
                    Profile pf = Setting.TempProfile;

                    // 現在の順番でコンテナを取得
                    List<TileContainer> containersInCurrentOrder = GetTileContainersInCurrentOrder();
                    //Debug.WriteLine("Current top container: " + currentContainers[0].Order);

                    // タイルサイズ(アス比)の決定
                    double w = (this.Width - MainContent.Margin.Left * 2) / pf.NumofMatrix.Col;
                    double h = (this.Height - MainContent.Margin.Left * 2) / pf.NumofMatrix.Row;
                    double gridRatio = h / w;

                    int gridWidth = TileContainer.StandardTileWidth + pf.TilePadding.Value * 2;
                    int gridHeight = (int)( gridWidth * gridRatio );
                    pf.AspectRatio.Value = new int[] { TileContainer.StandardTileWidth, gridHeight - pf.TilePadding.Value * 2 };

                    // コンテナサイズの決定
                    foreach( TileContainer tc in tileContainers )
                    {
                        tc.InitSize(pf.AspectRatio.H, pf.AspectRatio.V, pf.TilePadding.Value);
                        tc.InitWrapPoint();
                    }

                    // 1タイルのバックバッファサイズを更新
                    TileContainer.TileAspectRatio = (double)pf.AspectRatio.V / pf.AspectRatio.H;
                    TileContainer.SetBitmapDecodePixelOfTile(pf.BitmapDecodeTotalPixel.Value, pf.NumofMatrix.Col, pf.NumofMatrix.Row);

                    // 位置を正規化
                    containersInCurrentOrder[0].Margin = new Thickness(0);
                    double containerWidth = tileContainers[0].Width;
                    double containerHeight = tileContainers[0].Height;
                    switch( pf.SlideDirection.Value )
                    {
                        case SlideDirection.Left:
                        default:
                            containersInCurrentOrder[1].Margin = new Thickness(containerWidth, 0, 0, 0);
                            containersInCurrentOrder[2].Margin = new Thickness(2 * containerWidth, 0, 0, 0);
                            break;
                        case SlideDirection.Top:
                            containersInCurrentOrder[1].Margin = new Thickness(0, containerHeight, 0, 0);
                            containersInCurrentOrder[2].Margin = new Thickness(0, 2 * containerHeight, 0, 0);
                            break;
                        case SlideDirection.Right:
                            containersInCurrentOrder[1].Margin = new Thickness(-containerWidth, 0, 0, 0);
                            containersInCurrentOrder[2].Margin = new Thickness(-2 * containerWidth, 0, 0, 0);
                            break;
                        case SlideDirection.Bottom:
                            containersInCurrentOrder[1].Margin = new Thickness(0, -containerHeight, 0, 0);
                            containersInCurrentOrder[2].Margin = new Thickness(0, -2 * containerHeight, 0, 0);
                            break;
                    }

                }

                // アス比固定・非固定に関わらず拡大縮小
                FitMainContentToWindow();
            };

            this.Closing += (s, e) =>
            {
                ShortcutManager.UnhookWindowsHook();
                if( ignoreClosingEvent ) return;
                SaveHistoryItem();
                Setting.SettingDialogTabIndex = settingDialog.MainTabControl.SelectedIndex;
                UpdateTempProfile();
                Setting.saveToXmlFile();
            };

            this.PreviewDragOver += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                    e.Effects = DragDropEffects.All;
                else
                    e.Effects = DragDropEffects.None;
                e.Handled = true;
            };

            this.Drop += (s, e) =>
            {
                // 「読み込み中」メッセージの表示
                this.WaitingMessageBase.Visibility = Visibility.Visible;
                this.WaitingMessageBase.Refresh();

                SaveHistoryItem();

                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];

                if( IsCtrlOrShiftKeyPressed )
                {
                    // 追加読み込み
                    ReadFilesAndInitMainContent(files, true, 0);
                }
                else
                {
                    // 通常読み込み
                    DropNewFiles(files);
                }
            };

            this.Seekbar.ValueChanged += (s, e) =>
            {
                if (this.ignoreSliderValueChangeEvent) return;

                int value = (int)Seekbar.Value;
                //if (isHSeekbarDragStarted && BitmapPresenter.FileInfo.Count > 99)
                if (isSeekbarDragStarted)
                {
                    PageInfoText.Text = String.Format("{0} / {1}",
                        value, imageFileManager.NumofImageFile);
                    return;
                }
                Debug.WriteLine("seek bar value: " + Seekbar.Value);
                OnSeekbarValueChanged(value);
            };

            // キーイベント（暫定的に）
            this.PreviewKeyDown += (s, e) =>
            {
#if DEBUG
                // debug
                Profile pf = Setting.TempProfile;

                if(e.Key == Key.T )
                {
                    ShortcutManager.ExecuteCommand(CommandID.ZoomImageUnderCursor);
                }
                if(e.Key == Key.A )
                {
                }
#endif
            };


            // end of method
        }



        private void intervalSlideTimer_Tick(object sender, EventArgs e)
        {
            intervalSlideTimerCount += 1;

            if(Setting.TempProfile.SlidePlayMethod.Value == SlidePlayMethod.Interval)
            {
                if(intervalSlideTimerCount >= Setting.TempProfile.SlideInterval.Value)
                {
                    Profile pf = Setting.TempProfile;
                    StartIntervalSlide(pf.SlideByOneImage.Value, pf.SlideTimeInIntevalMethod.Value);

                    int slideTime = (int)( pf.SlideTimeInIntevalMethod.Value / 1000 );
                    intervalSlideTimerCount = 0 - slideTime;
                }
            }
            else
            {
                intervalSlideTimer.Stop();
            }
        }


        private void Seekbar_DragStarted(object sender, RoutedEventArgs e)
        {
            isSeekbarDragStarted = true;
        }

        private void Seekbar_DragCompleted(object sender, RoutedEventArgs e)
        {
            isSeekbarDragStarted = false;
            OnSeekbarValueChanged((int)this.Seekbar.Value);
        }

        private void OnSeekbarValueChanged(int value)
        {
            int index = value - 1;
            ChangeCurrentImageIndex(index);
        }


        // ツールバーボタン(読み込み)
        private void MenuItem_Load_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            // イベントの発生元チェック(子要素からのイベントなら無視)
            MenuItem miSrc = e.OriginalSource as MenuItem;
            if( miSrc == null || miSrc.Name != MenuItem_Load.Name) return;

            // 項目追加前に、基本メニュー以外を削除
            for(int i=MenuItem_Load.Items.Count - 1; i>=0; i-- )
            {
                MenuItem mi = MenuItem_Load.Items[i] as MenuItem;
                if(mi != null && (string)mi.Tag == "BasicItemLast" ) break;
                MenuItem_Load.Items.RemoveAt(i);
            }

            // 追加読み込み
            if( Setting.ShowMenuItem_AdditionalRead )
            {
                MenuItem_Load.Items.Add( new Separator() );
                MenuItem mi_addFolder = new MenuItem();
                mi_addFolder.Header = "フォルダを追加読み込み";
                mi_addFolder.Click += Toolbar_Add_Folder_Click;
                MenuItem_Load.Items.Add(mi_addFolder);

                MenuItem mi_addFile = new MenuItem();
                mi_addFile.Header = "ファイルを追加読み込み";
                mi_addFile.Click += Toolbar_Add_File_Click;
                MenuItem_Load.Items.Add(mi_addFile);
            }

            // 再読込み(必ず表示)
            MenuItem_Load.Items.Add( new Separator() );
            MenuItem mi_reload = new MenuItem();
            mi_reload.Header = "再読み込み";
            mi_reload.Click += Toolbar_Load_Reload_Click;
            MenuItem_Load.Items.Add(mi_reload);

            // エクスプローラーでフォルダを開く(必ず表示)
            MenuItem mi_openFolder = new MenuItem();
            mi_openFolder.Header = "エクスプローラーでフォルダを開く";
            mi_openFolder.Click += (s, ev) => { OpenCurrentFolderByExplorer(); };
            MenuItem_Load.Items.Add(mi_openFolder);


            // ヒストリーなし
            Action addHistorySettingMenu = () =>
            {
                MenuItem_Load.Items.Add( new Separator() );
                MenuItem m0 = new MenuItem();
                m0.Header = "履歴設定...";
                m0.Click += (s, ev) => ShowAppSettingDialog(0);
                MenuItem_Load.Items.Add(m0);
            };
            if( Setting.History.Count < 1 )
            {
                addHistorySettingMenu.Invoke();
                return;
            }

            // セパレータ
            MenuItem_Load.Items.Add( new Separator() );

            // ヒストリーの右クリックメニュー
            ContextMenu contextMenu = new ContextMenu();
            MenuItem cmi1 = new MenuItem();
            MenuItem cmi2 = new MenuItem();
            MenuItem cmi3 = new MenuItem();
            cmi1.Header = "追加読み込み";
            cmi2.Header = "エクスプローラーで開く";
            cmi3.Header = "削除";

            // 追加読み込み
            cmi1.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem;
                if( mi == null ) return;

                string[] path = { mi.ToolTip.ToString() };
                ReadFilesAndInitMainContent(path, true, 0);
            };

            // エクスプローラーで開く
            cmi2.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem;
                if( mi == null ) return;

                string path = mi.ToolTip.ToString();
                //Process.Start(path);
                Process.Start("explorer.exe", "/select,\"" + path + "\"");
            };

            // 削除
            cmi3.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem;
                if( mi == null ) return;

                string path = mi.ToolTip.ToString();
                Setting.History.RemoveAll( h => h.ArchiverPath == path );
                MenuItem_Load_SubmenuOpened(this, new RoutedEventArgs(null, MenuItem_Load));
            };
            contextMenu.Items.Add(cmi1);
            contextMenu.Items.Add(cmi2);
            contextMenu.Items.Add(cmi3);

            // メインヒストリー追加
            for(int i=0; i<Setting.NumofHistoryInMainMenu; i++ )
            {
                if(i < Setting.History.Count )
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = System.IO.Path.GetFileName( Setting.History[i].ArchiverPath ).Replace("_", "__");
                    mi.ToolTip = Setting.History[i].ArchiverPath;
                    mi.Click += OnHistoryItemSelected;
                    mi.ContextMenu = contextMenu;
                    MenuItem_Load.Items.Add(mi);
                }
            }

            // サブヒストリー追加
            if(Setting.History.Count > Setting.NumofHistoryInMainMenu && Setting.NumofHistoryInMenu > Setting.NumofHistoryInMainMenu)
            {
                MenuItem continuation = new MenuItem();
                continuation.Header = "フォルダ履歴の続き";
                MenuItem_Load.Items.Add(continuation);

                for( int i = Setting.NumofHistoryInMainMenu; i < Setting.NumofHistoryInMenu; i++ )
                {
                    if( i > Setting.History.Count - 1) break;
                    MenuItem mi = new MenuItem();
                    mi.Header = System.IO.Path.GetFileName(Setting.History[i].ArchiverPath).Replace("_", "__");
                    mi.ToolTip = Setting.History[i].ArchiverPath;
                    mi.Click += OnHistoryItemSelected;
                    mi.ContextMenu = contextMenu;
                    continuation.Items.Add(mi);
                }
            }

            // 履歴設定
            addHistorySettingMenu.Invoke();
        }

        // ヒストリー選択時
        private void OnHistoryItemSelected(object sender, RoutedEventArgs e)
        {
            MenuItem miSrc = e.OriginalSource as MenuItem;
            if( miSrc == null ) return;

            SaveHistoryItem();
            LoadHistory( miSrc.ToolTip.ToString() );
        }
        

        // フォルダ読み込み
        private void Toolbar_Load_Folder_Click(object sender, RoutedEventArgs e)
        {
            ShortcutManager.ExecuteCommand(CommandID.OpenFolder);
        }

        // ファイル読み込み
        private void Toolbar_Load_File_Click(object sender, RoutedEventArgs e)
        {
            ShortcutManager.ExecuteCommand(CommandID.OpenFile);
        }

        // フォルダ追加読み込み
        private void Toolbar_Add_Folder_Click(object sender, RoutedEventArgs e)
        {
            ShortcutManager.ExecuteCommand(CommandID.OpenAdditionalFolder);
        }

        // ファイル追加読み込み
        private void Toolbar_Add_File_Click(object sender, RoutedEventArgs e)
        {
            ShortcutManager.ExecuteCommand(CommandID.OpenAdditionalFile);
        }


        // 再読込み
        private void Toolbar_Load_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload(false);
        }

        // 履歴設定
        private void Toolbar_Load_HistorySetting_Click(object sender, RoutedEventArgs e)
        {
            ShowAppSettingDialog( (int)AppSettingDialogTabIndex.History );
        }


        // グリッドのアスペクト比 開いた時
        private void MenuItem_AspectRatio_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem_AspectRatio.Items.Clear();

            // 非固定
            MenuItem m1 = new MenuItem();
            m1.Header = "非固定";
            m1.Tag = "FREE";
            m1.IsCheckable = true;
            m1.Click += Toolbar_AspectRate_Click;
            MenuItem_AspectRatio.Items.Add(m1);
            MenuItem_AspectRatio.Items.Add( new Separator() );

            // 固定
            foreach(Point pt in Setting.AspectRatioList )
            {
                MenuItem mi = new MenuItem();
                mi.Header = string.Format("{0} : {1}", pt.X, pt.Y);
                mi.Tag    = string.Format("{0}_{1}", pt.X, pt.Y);
                mi.IsCheckable = true;
                mi.Click += Toolbar_AspectRate_Click;
                MenuItem_AspectRatio.Items.Add(mi);
            }

            // リストを編集...
            MenuItem_AspectRatio.Items.Add( new Separator() );
            MenuItem m2 = new MenuItem();
            m2.Header = "リストを編集...";
            m2.Tag = "EDIT";
            m2.IsCheckable = true;
            m2.Click += (s, ev) => { ShowAppSettingDialog( (int)AppSettingDialogTabIndex.AspectRatio ); };
            MenuItem_AspectRatio.Items.Add(m2);

            UpdateToolbarViewing();
        }

        // グリッドのアスペクト比 選択時
        private void Toolbar_AspectRate_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if(item != null)
            {
                // 非固定を選択
                if(item.Tag.ToString() == "FREE" )
                {
                    if( Setting.TempProfile.NonFixAspectRatio.Value )
                    {
                        // 非固定解除
                        Setting.TempProfile.NonFixAspectRatio.Value = false;
                    }
                    else
                    {
                        // 非固定にする
                        Setting.TempProfile.NonFixAspectRatio.Value = true;
                    }
                    UpdateToolbarViewing();
                    return;
                }

                // 固定値を選択
                else
                {
                    Setting.TempProfile.NonFixAspectRatio.Value = false;
                    string[] str = item.Tag.ToString().Split('_');
                    int w = int.Parse(str[0]);
                    int h = int.Parse(str[1]);

                    ChangeAspectRatio(w, h);
                }
            }
        }


        // 行列設定ダイアログ
        private void MatrixSelecter_MatrixSelected(object sender, EventArgs e)
        {
            MatrixSelecter ms = sender as MatrixSelecter;
            if(ms != null)
            {
                this.MenuItem_Matrix.IsSubmenuOpen = false;
                this.WaitingMessageBase.Visibility = Visibility.Visible;
                this.WaitingMessageBase.Refresh();
                ChangeGridDifinition(ms.ColValue, ms.RowValue);
                this.Focus();
            }

        }

        private void MatrixSelecter_MaxSizeChanged(object sender, EventArgs e)
        {
            Setting.MatrixSelecterMaxSize = matrixSelecter.MaxSize;
        }


        // 再生
        private void Toolbar_Play_Click(object sender, RoutedEventArgs e)
        {
            // 再生中だったら停止
            if(IsPlaying) StopSlideShow();

            // 再生
            else StartSlideShow();
        }


        // プロファイルメニュー開いた時
        private void MenuItem_Profile_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            // Xmlからプロファイルをロード
            string xmlDir = Directory.GetParent( System.Reflection.Assembly.GetExecutingAssembly().Location ).FullName + "\\Profile";
            if( Setting.UserProfileList.All( pl => pl.Profile == null) && Directory.Exists(xmlDir) )
            {
                string[] xmls = Directory.GetFiles(xmlDir);
                foreach(string xml in xmls )
                {
                    if( System.IO.Path.GetExtension(xml) == ".xml" )
                    {
                        // ロード
                        Profile pf = UserProfileInfo.LoadProfileFromXmlFile(xml);

                        // UserProfileListにあるなら、メンバとしてプロファイルを追加、ないなら新たにUserProfileInfoを作成して追加
                        string relativePath = xml.Replace(xmlDir + "\\", "");
                        UserProfileInfo upi = Setting.UserProfileList.FirstOrDefault(pl => pl.RelativePath == relativePath);
                        if( upi != null ) upi.Profile = pf;
                        else
                        {
                            UserProfileInfo newUpi = new UserProfileInfo(relativePath);
                            newUpi.Profile = pf;
                            Setting.UserProfileList.Add(newUpi);
                        }
                    }
                }

                // xmlファイルからロード出来なかった、UserProfileInfoを全て削除
                Setting.UserProfileList.RemoveAll( pl => pl.Profile == null );
            }


            // イベントの発生元チェック(子要素からのイベントなら無視)
            MenuItem miSrc = e.OriginalSource as MenuItem;
            if( miSrc == null || miSrc.Name != MenuItem_Profile.Name) return;

            // プロファイル追加前に削除
            const int basicManuCnt = 1;
            while(MenuItem_Profile.Items.Count > basicManuCnt )
            {
                MenuItem_Profile.Items.RemoveAt(basicManuCnt);
            }

            // プリセットプロファイル追加
            if( Setting.UsePresetProfile )
            {
                MenuItem_Profile.Items.Add( new Separator() );
                foreach(Profile ppf in PresetProfile.Items)
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = ppf.Name;
                    if( ppf.Name == "デフォルト" ) mi.ToolTip = "デフォルト";
                    else mi.ToolTip = ppf.CreateProfileToolTip();
                    ToolTipService.SetShowDuration(mi, 1000000);
                    mi.Click += (se, ev) => { LoadUserProfile(ppf); };
                    MenuItem_Profile.Items.Add(mi);
                }
            }

            // セパレータ追加
            if(Setting.UserProfileList.Count > 0) MenuItem_Profile.Items.Add( new Separator() );

            // プロファイル追加
            foreach(UserProfileInfo upi in Setting.UserProfileList )
            {
                MenuItem mi = new MenuItem();
                mi.Header = upi.Profile.Name.Replace("_", "__");
                mi.ToolTip = upi.Profile.CreateProfileToolTip();
                ToolTipService.SetShowDuration(mi, 1000000);
                mi.Click += (se, ev) => { LoadUserProfile(upi.Profile); };
                mi.ContextMenu = CreateProfileContextMenu(upi);
                MenuItem_Profile.Items.Add(mi);
            }

            // リストを編集... を追加
            MenuItem_Profile.Items.Add( new Separator() );
            MenuItem ms = new MenuItem();
            ms.Header = "リストを編集...";
            ms.Click += (se, ev) => { ShowProfileListEditDialog(); };
            MenuItem_Profile.Items.Add(ms);
        }

        // プロファイルメニュー内、コンテキストメニュー作成
        private ContextMenu CreateProfileContextMenu(UserProfileInfo upi)
        {
            ContextMenu contextMenu = new ContextMenu();
            MenuItem cmi1 = new MenuItem();
            MenuItem cmi2 = new MenuItem();
            MenuItem cmi3 = new MenuItem();
            MenuItem cmi4 = new MenuItem();
            MenuItem cmi5 = new MenuItem();
            Separator sep1 = new Separator();

            cmi1.Header = "編集";
            cmi2.Header = "コピー";
            cmi3.Header = "削除";
            cmi4.Header = "上へ";
            cmi5.Header = "下へ";

            Action updateMenu = () => { MenuItem_Profile_SubmenuOpened(this, new RoutedEventArgs(null, MenuItem_Profile)); };

            // 編集
            cmi1.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem; if( mi == null ) return;

                ShowProfileEditDialog(ProfileEditDialogMode.Edit, upi);
            };

            // コピー
            cmi2.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem; if( mi == null ) return;

                int idx = Setting.UserProfileList.IndexOf(upi);
                UserProfileInfo newUpi = CopyUserProfileInfo(upi);
                Setting.UserProfileList.Insert(idx + 1, newUpi);

                MenuItem_Profile.IsSubmenuOpen = false;
                MenuItem_Profile.IsSubmenuOpen = true;
            };

            // 削除
            cmi3.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem; if( mi == null ) return;

                MessageBoxResult result =  MessageBox.Show("プロファイル「" + upi.Profile.Name + "」を削除してもよろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if( result == MessageBoxResult.Yes ) RemoveUserProfileInfo(upi);

                MenuItem_Profile.IsSubmenuOpen = true;
            };

            // 上へ
            cmi4.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem; if( mi == null ) return;

                int index = Setting.UserProfileList.IndexOf(upi);
                if( index <= 0 ) return;

                Setting.UserProfileList.RemoveAt(index);
                Setting.UserProfileList.Insert(index - 1, upi);

                updateMenu.Invoke();
            };

            // 下へ
            cmi5.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem; if( mi == null ) return;

                int index = Setting.UserProfileList.IndexOf(upi);
                if( index == Setting.UserProfileList.Count - 1 ) return;

                Setting.UserProfileList.RemoveAt(index);
                Setting.UserProfileList.Insert(index + 1, upi);

                updateMenu.Invoke();
            };

            contextMenu.Items.Add(cmi1);
            contextMenu.Items.Add(cmi2);
            contextMenu.Items.Add(cmi3);

            contextMenu.Items.Add(sep1);

            contextMenu.Items.Add(cmi4);
            contextMenu.Items.Add(cmi5);

            return contextMenu;
        }

        // プロファイル 新規作成
        private void Toolbar_Profile_New_Click(object sender, RoutedEventArgs e)
        {
            ShowProfileEditDialog(ProfileEditDialogMode.New, null);
        }

        // システムボタン
        private void SystemButton_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SystemButton_Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.ToggleFullScreen();
        }

        private void SystemButton_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        // ウインドウプロシージャのフック
        protected override void OnSourceInitialized(EventArgs e)
        {
            HwndSource hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            hwndSource.AddHook(uiHelper.HwndSourceHook);

            base.OnSourceInitialized(e);
        }



    }
}
