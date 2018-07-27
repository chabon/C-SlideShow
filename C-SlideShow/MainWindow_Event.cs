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



namespace C_SlideShow
{
    public partial class MainWindow
    {


        private void InitEvent()
        {
            /* ---------------------------------------------------- */
            //     init events
            /* ---------------------------------------------------- */

            // ウインドウ全体でドラッグ可能に
            this.MouseLeftButtonDown += (sender, e) =>
            {
                if( Setting.TempProfile.IsFullScreenMode.Value ) return;
                this.DragMove();
            };
            

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

                // 画像拡大パネルのサイズ更新
                if( TileExpantionPanel.IsShowing ) TileExpantionPanel.FitToMainWindow();

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
                    if( Setting.EnabledItemsInHistory.ArchiverPath && files.Length == 1 && Setting.History.Any( hi => hi.ArchiverPath ==  files[0]))
                    {
                        // 履歴に存在する場合
                        LoadHistory(files[0]);
                    }
                    else
                    {
                        ReadFilesAndInitMainContent(files, false, 0);
                    }
                }
            };

            this.MouseWheel += (s, e) =>
            {
                // 右クリック押しながらで、拡大縮小
                // --------------------------------
                if (!Setting.TempProfile.IsFullScreenMode.Value)
                {
                    short stateR = Win32.GetAsyncKeyState(Win32.VK_RBUTTON);
                    if ( (stateR & 0x8000) != 0)
                    {
                        if(e.Delta > 0)
                        {
                            this.Width = Width * 1.1;
                            this.Height = Height * 1.1;
                            UpdateWindowSize();
                        }
                        else
                        {
                            this.Width = Width * 0.9;
                            this.Height = Height * 0.9;
                            UpdateWindowSize();
                        }
                        prevMouseRButtonDownEventContext.Handled = true;
                        return;
                    }
                }

                // スライド操作
                // --------------------------------
                if( TileExpantionPanel.IsShowing ) return;

                bool isPlayback; // 巻き戻しかどうか
                if (e.Delta > 0) isPlayback = true;  // wheel up
                else isPlayback = false;

                bool slideByOneImage = false; // 画像１枚毎のスライド
                if (IsCtrlOrShiftKeyPressed) slideByOneImage = true;

                StartOperationSlide(isPlayback, slideByOneImage, 300);
            };

            // 右クリックの制御
            this.PreviewMouseRightButtonDown += (s, e) =>
            {
                prevMouseRButtonDownEventContext.IsPressed = true;
                prevMouseRButtonDownEventContext.Handled = false;
            };

            this.PreviewMouseRightButtonUp += (s, e) =>
            {
                prevMouseRButtonDownEventContext.IsPressed = false;

                if( prevMouseRButtonDownEventContext.Handled )
                {
                    e.Handled = true;
                    return;
                }
                else
                {
                    // 画像拡大パネルの表示
                    if( TileExpantionPanel.IsShowing ) return;
                    try
                    {
                        DependencyObject source = e.OriginalSource as DependencyObject;
                        if( source == null ) return;

                        // クリックされたTileContainer
                        TileContainer tc = WpfTreeUtil.FindAncestor<TileContainer>(source);
                        if( tc == null ) return;

                        // クリックされたBorder
                        Border border;
                        if( e.OriginalSource is Border ) border = e.OriginalSource as Border;
                        else
                        {
                            border = WpfTreeUtil.FindAncestor<Border>(source);
                        }
                        if( border == null ) return;

                        // 紐づけられているTileオブジェクトを特定
                        Tile targetTile = tc.Tiles.First(t => t.Border == border);
                        if( targetTile == null ) return;

                        // 表示
                        TileExpantionPanel.Show(targetTile);
                    }
                    catch { }
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

            //this.Toolbar_Test.Click += (s, e) =>
            //{
            //    this.tileContainers.ForEach( tc => tc.StopSlideAnimation() );
            //};


            // キーイベント（暫定的に）
            this.PreviewKeyDown += (s, e) =>
            {
#if DEBUG
                // debug
                Profile pf = Setting.TempProfile;

                if(e.Key == Key.T )
                {
                    Setting.TempProfile.UsePlaidBackground.Value = !Setting.TempProfile.UsePlaidBackground.Value;
                    Setting.TempProfile.PairColorOfPlaidBackground.Value = Colors.LightGray;
                    ApplyColorAndOpacitySetting();
                }
                if(e.Key == Key.A )
                {
                    //MenuItem continuation = (MenuItem)MenuItem_Load.Items[MenuItem_Load.Items.Count - 1];
                    //MenuItem mi = new MenuItem();
                    //mi.Header = "hoge";
                    //mi.ToolTip = "aaaa";
                    //continuation.Items.Add(mi);


                    //pf.ApplyRotateInfoFromExif = !pf.ApplyRotateInfoFromExif;
                    //InitMainContent(imageFileManager.CurrentIndex);
                }

                if(e.Key == Key.D1 )
                {
                    pf.TilePadding.Value = Setting.TempProfile.TilePadding.Value - 1;
                    if( pf.TilePadding.Value < 0 ) pf.TilePadding.Value = 0;
                    foreach(TileContainer tc in tileContainers )
                    {
                        tc.InitSize(pf.AspectRatio.H, pf.AspectRatio.V, pf.TilePadding.Value);
                    }
                    UpdateWindowSize();
                }
                if(e.Key == Key.D2 )
                {
                    pf.TilePadding.Value = Setting.TempProfile.TilePadding.Value + 1;
                    foreach(TileContainer tc in tileContainers )
                    {
                        tc.InitSize(pf.AspectRatio.H, pf.AspectRatio.V, pf.TilePadding.Value);
                    }
                    UpdateWindowSize();
                }
#endif

                // alt + enter でフルスクリーン切り替え
                //if (e.SystemKey == Key.LeftAlt && e.Key == Key.Enter)
                //{
                //    ToggleFullScreen();
                //    return;
                //}

                // フルスクリーン解除
                if(e.Key == Key.Escape && Setting.TempProfile.IsFullScreenMode.Value)
                {
                    ToggleFullScreen();
                }
            };



            //this.PreviewMouseDoubleClick += (s, e) => ToggleFullScreen();

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

        private void MatrixSelecter_MatrixSelected(object sender, EventArgs e)
        {
            MatrixSelecter ms = sender as MatrixSelecter;
            if(ms != null)
            {
                this.MenuItem_Matrix.IsSubmenuOpen = false;
                ChangeGridDifinition(ms.ColValue, ms.RowValue);
                this.Focus();
            }

        }

        // ツールバーボタン(読み込み)
        private void MenuItem_Load_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            // イベントの発生元チェック(子要素からのイベントなら無視)
            MenuItem miSrc = e.OriginalSource as MenuItem;
            if( miSrc == null || miSrc.Name != MenuItem_Load.Name) return;

            // ヒストリー追加前に削除
            const int hIndex = 9;
            while(MenuItem_Load.Items.Count - 1 >= hIndex )
            {
                MenuItem_Load.Items.RemoveAt(MenuItem_Load.Items.Count - 1);
            }

            // ヒストリーなし
            if( Setting.History.Count < 1 ) return;

            // セパレータ
            MenuItem_Load.Items.Add( new Separator() );

            // ヒストリーの右クリックメニュー
            ContextMenu contextMenu = new ContextMenu();
            MenuItem cmi1 = new MenuItem();
            MenuItem cmi2 = new MenuItem();
            MenuItem cmi3 = new MenuItem();
            cmi1.Header = "追加読み込み";
            cmi2.Header = "外部プログラムで開く";
            cmi3.Header = "外部プログラムで親フォルダを開く";

            // 追加読み込み
            cmi1.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem;
                if( mi == null ) return;

                string[] path = { mi.ToolTip.ToString() };
                ReadFilesAndInitMainContent(path, true, 0);
            };

            // 外部プログラムで開く
            cmi2.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem;
                if( mi == null ) return;

                string path = mi.ToolTip.ToString();
                Process.Start(path);
            };

            // 外部プログラムで親フォルダを開く
            cmi3.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem;
                if( mi == null ) return;

                string parentDir = Directory.GetParent(mi.ToolTip.ToString()).FullName;
                //Process.Start( "EXPLORER.EXE", parentDir);
                Process.Start(parentDir);
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
                    mi.Header = System.IO.Path.GetFileName( Setting.History[i].ArchiverPath );
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
                    mi.Header = System.IO.Path.GetFileName(Setting.History[i].ArchiverPath);
                    mi.ToolTip = Setting.History[i].ArchiverPath;
                    mi.Click += OnHistoryItemSelected;
                    mi.ContextMenu = contextMenu;
                    continuation.Items.Add(mi);
                }
            }
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
            var dlg = new Forms.FolderBrowserDialog();
            dlg.Description = "画像フォルダーを選択してください。";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveHistoryItem();

                if( Setting.EnabledItemsInHistory.ArchiverPath && Setting.History.Any( hi => hi.ArchiverPath ==  dlg.SelectedPath) )
                {
                    // 履歴に存在する場合
                    LoadHistory(dlg.SelectedPath);
                }
                else
                {
                    string[] path = { dlg.SelectedPath };
                    ReadFilesAndInitMainContent(path, false, 0);
                }
            }
        }

        // ファイル読み込み
        private void Toolbar_Load_File_Click(object sender, RoutedEventArgs e)
        {
            Forms.OpenFileDialog ofd = new Forms.OpenFileDialog();
            ofd.Title = "ファイルを選択してください";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == Forms.DialogResult.OK)
            {
                SaveHistoryItem();

                if( Setting.EnabledItemsInHistory.ArchiverPath && ofd.FileNames.Length == 1 && Setting.History.Any( hi => hi.ArchiverPath == ofd.FileNames[0]) )
                {
                    // 履歴に存在する場合
                    LoadHistory(ofd.FileNames[0]);
                }
                else
                {
                    ReadFilesAndInitMainContent(ofd.FileNames, false, 0);
                }
            }
        }

        // フォルダ追加読み込み
        private void Toolbar_Add_Folder_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Forms.FolderBrowserDialog();
            dlg.Description = "追加する画像フォルダーを選択してください。";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] path = { dlg.SelectedPath };
                ReadFilesAndInitMainContent(path, true,  0);
            }
        }

        // ファイル追加読み込み
        private void Toolbar_Add_File_Click(object sender, RoutedEventArgs e)
        {
            Forms.OpenFileDialog ofd = new Forms.OpenFileDialog();
            ofd.Title = "追加するファイルを選択してください";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == Forms.DialogResult.OK)
            {
                ReadFilesAndInitMainContent(ofd.FileNames, true,  0);
            }
        }


        // 再読込み
        private void Toolbar_Load_Reload_Click(object sender, RoutedEventArgs e)
        {
            Reload(false);
        }

        // グリッドのアスペクト比
        private void Toolbar_AspectRate_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if(item != null)
            {
                // 非固定を選択
                if(item.Tag.ToString() == "FREE" )
                {
                    if( !Setting.TempProfile.NonFixAspectRatio.Value )
                    {
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

        // 履歴設定
        private void Toolbar_Load_HistorySetting_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettingDialog globalSettingDialog = new GlobalSettingDialog();

            // 表示位置はメインウインドウ中心に
            Util.SetWindowCenterOnWindow(this, globalSettingDialog);

            // 値の代入
            globalSettingDialog.Initialize();
            globalSettingDialog.MainTabControl.SelectedIndex = 0;

            // 表示
            globalSettingDialog.ShowDialog();
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

            // セパレータ追加
            if(Setting.UserProfileList.Count > 0) MenuItem_Profile.Items.Add( new Separator() );

            // プロファイル追加
            foreach(UserProfileInfo upi in Setting.UserProfileList )
            {
                MenuItem mi = new MenuItem();
                mi.Header = upi.Profile.Name;
                mi.ToolTip = upi.Profile.CreateProfileToolTip();
                ToolTipService.SetShowDuration(mi, 1000000);
                mi.Click += (se, ev) => 
                {
                    LoadUserProfile(upi.Profile);
                };
                mi.ContextMenu = CreateProfileContextMenu(upi);
                MenuItem_Profile.Items.Add(mi);
            }
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
                if( index == 0 ) return;

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
