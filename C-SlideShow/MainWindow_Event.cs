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
                Win32.SetWindowPosProperly(new WindowInteropHelper(this).Handle, 
                    (int)this.Left, (int)this.Top, true, 0, true);
            };

            this.SizeChanged += (s, e) =>
            {
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

                    // タイルサイズ(アス比)、コンテナサイズの決定
                    double w = (this.Width - MainContent.Margin.Left * 2) / pf.NumofMatrix.Col;
                    double h = (this.Height - MainContent.Margin.Left * 2) / pf.NumofMatrix.Row;
                    double gridRatio = h / w;

                    int gridWidth = TileContainer.StandardTileWidth + pf.TilePadding.Value * 2;
                    int gridHeight = (int)( gridWidth * gridRatio );
                    pf.AspectRatio.Value = new int[] { TileContainer.StandardTileWidth, gridHeight - pf.TilePadding.Value * 2 };

                    foreach( TileContainer tc in tileContainers )
                    {
                        tc.InitSize(pf.AspectRatio.H, pf.AspectRatio.V, pf.TilePadding.Value);
                        tc.InitWrapPoint();
                    }

                    // 1タイルのバックバッファサイズを更新
                    TileContainer.TileAspectRatio = pf.AspectRatio.V / (double)pf.AspectRatio.H;
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
                SavePageIndexToHistory();
                Setting.SettingDialogTabIndex = settingDialog.MainTabControl.SelectedIndex;
                Setting.TempProfile.LastPageIndex.Value = imageFileManager.CurrentIndex;
                SaveWindowRect();
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

                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if( IsCtrlOrShiftKeyPressed )
                {
                    // 追加読み込み
                    ReadFilesAndInitMainContent(files, true, 0);
                }
                else
                {
                    // 通常読み込み
                    ReadFilesAndInitMainContent(files, false,  LoadPageIndexFromHistory() );
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
                        TileContainer tc = VisualTreeUtil.FindAncestor<TileContainer>(source);
                        if( tc == null ) return;

                        // クリックされたBorder
                        Border border;
                        if( e.OriginalSource is Border ) border = e.OriginalSource as Border;
                        else
                        {
                            border = VisualTreeUtil.FindAncestor<Border>(source);
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
                    MenuItem continuation = (MenuItem)MenuItem_Load.Items[MenuItem_Load.Items.Count - 1];
                    MenuItem mi = new MenuItem();
                    mi.Header = "hoge";
                    mi.ToolTip = "aaaa";
                    continuation.Items.Add(mi);


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
            const int hIndex = 7;
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
            cmi1.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem;
                if( mi == null ) return;

                string[] path = { mi.ToolTip.ToString() };
                ReadFilesAndInitMainContent(path, true, 0);
            };
            cmi2.Click += (se, ev) =>
            {
                MenuItem mi = ((ev.Source as MenuItem).Parent as ContextMenu).PlacementTarget as MenuItem;
                if( mi == null ) return;

                string path = mi.ToolTip.ToString();
                Process.Start(path);
            };
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
            const int numofMainHistory = 10;
            for(int i=0; i<numofMainHistory; i++ )
            {
                if(i < Setting.History.Count )
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = System.IO.Path.GetFileName( Setting.History[i].Path );
                    mi.ToolTip = Setting.History[i].Path;
                    mi.Click += OnHistoryItemSelected;
                    mi.ContextMenu = contextMenu;
                    MenuItem_Load.Items.Add(mi);
                }
            }

            // サブヒストリー追加
            if(Setting.History.Count > numofMainHistory )
            {
                MenuItem continuation = new MenuItem();
                continuation.Header = "フォルダ履歴の続き...";
                MenuItem_Load.Items.Add(continuation);

                for( int i = numofMainHistory; i < Setting.History.Count; i++ )
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = System.IO.Path.GetFileName(Setting.History[i].Path);
                    mi.ToolTip = Setting.History[i].Path;
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

            string[] path = { miSrc.ToolTip.ToString() };
            ReadFilesAndInitMainContent(path, false,  LoadPageIndexFromHistory() );
        }
        

        // フォルダ読み込み
        private void Toolbar_Load_Folder_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Forms.FolderBrowserDialog();
            dlg.Description = "画像フォルダーを選択してください。";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] path = { dlg.SelectedPath };
                ReadFilesAndInitMainContent(path, false,  LoadPageIndexFromHistory() );
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
                ReadFilesAndInitMainContent(ofd.FileNames, false,  LoadPageIndexFromHistory() );
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
                    Setting.TempProfile.NonFixAspectRatio.Value = true;
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
            // イベントの発生元チェック(子要素からのイベントなら無視)
            MenuItem miSrc = e.OriginalSource as MenuItem;
            if( miSrc == null || miSrc.Name != MenuItem_Profile.Name) return;

            // プロファイル追加前に削除
            const int basicManuCnt = 2;
            while(MenuItem_Profile.Items.Count > basicManuCnt )
            {
                MenuItem_Profile.Items.RemoveAt(basicManuCnt);
            }

            // プロファイル追加
            foreach(Profile pf in Setting.ProfileList )
            {
                MenuItem mi = new MenuItem();
                mi.Header = pf.Name;
                //mi.ToolTip = pf.CreateProfileToolTip();
                mi.Click += (se, ev) => 
                {
                    Setting.TempProfile.Marge(pf);
                    LoadProfile(Setting.TempProfile);
                };
                //mi.ContextMenu = contextMenu;
                MenuItem_Profile.Items.Add(mi);
            }
        }

        // プロファイル 新規作成
        private void Toolbar_Profile_New_Click(object sender, RoutedEventArgs e)
        {
            if(profileEditDialog == null )
            {
                profileEditDialog = new ProfileEditDialog();
                profileEditDialog.Owner = this;
            }
            profileEditDialog.Mode = ProfileEditDialogMode.New;
            profileEditDialog.ProfileList = Setting.ProfileList;

            // 表示前にメインウインドウのウインドウ情報保存
            SaveWindowRect();

            // 初期位置は、メインウインドウの中心に
            Rect rcMw = new Rect(Left, Top, Width, Height);
            Point ptCenter = new Point( rcMw.Left + rcMw.Width / 2 , rcMw.Top + rcMw.Height / 2 );
            Rect rcDlg = new Rect(
                ptCenter.X - profileEditDialog.Width / 2,
                ptCenter.Y - profileEditDialog.Height / 2,
                profileEditDialog.Width,
                profileEditDialog.Height
            );

            // ワーキングエリアはみ出しの補正
            rcDlg = Util.GetCorrectedWindowRect(rcDlg);
            profileEditDialog.Left = rcDlg.Left;
            profileEditDialog.Top = rcDlg.Top;

            // プロファイルを読み込んで、表示
            profileEditDialog.LoadProfile(Setting.TempProfile);
            profileEditDialog.ShowDialog();
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
