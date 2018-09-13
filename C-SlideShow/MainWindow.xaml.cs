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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;

using C_SlideShow.Archiver;
using C_SlideShow.Shortcut;
using C_SlideShow.CommonControl;

using Forms = System.Windows.Forms;


namespace C_SlideShow
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // field
        ImageFileManager imageFileManager;
        DispatcherTimer intervalSlideTimer = new DispatcherTimer(DispatcherPriority.Normal) ;
        int intervalSlideTimerCount = 0;
        List<TileContainer> tileContainers = new List<TileContainer>();
        bool ignoreSliderValueChangeEvent = false; // SliderのValue変更時にイベントを飛ばさないフラグ
        bool isSeekbarDragStarted = false;
        //Point aspectRatioInNonFixMode = new Point( 4, 3 );

        UIHelper uiHelper;
        Rect windowRectBeforeFullScreen = new Rect(50, 50, 400, 300);

        // property
        public static MainWindow Current { get; private set; }
        public AppSetting Setting { get; set; }
        public ShortcutManager ShortcutManager { get; set; }
        public ImageFileManager ImageFileManager { get { return imageFileManager; } }

        public MatrixSelecter       MatrixSelecter { get; private set; }
        public SlideSettingDialog   SlideSettingDialog { get; private set; }
        public SettingDialog        SettingDialog { get; private set; }
        public ProfileEditDialog    ProfileEditDialog { get; private set; }

        public bool IsHorizontalSlide
        {
            get
            {
                return tileContainers[0].IsHorizontalSlide;
            }
        }
        private bool IsCtrlOrShiftKeyPressed
        {
            get
            {
                return (Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) == KeyStates.Down ||
                     (Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) == KeyStates.Down ||
                     (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) == KeyStates.Down ||
                     (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) == KeyStates.Down;
            }
        }
        public double MainContentAspectRatio
        {
            get
            {
                return (double)(tileContainers[0].Width)
                    / ( tileContainers[0].Height );
            }
        }
        public bool IsPlaying
        {
            get
            {
                if (intervalSlideTimer.IsEnabled || tileContainers[0].IsContinuousSliding)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IsAnyToolbarMenuOpened
        {
            get
            {
                return MenuItem_Load.IsSubmenuOpen
                    || MenuItem_Matrix.IsSubmenuOpen
                    || MenuItem_AspectRatio.IsSubmenuOpen
                    || MenuItem_SlideSetting.IsSubmenuOpen
                    || MenuItem_Profile.IsSubmenuOpen
                    || MenuItem_Setting.IsSubmenuOpen;
            }
        }
        public bool IgnoreResizeEvent { get; set; } = false;

        // 起動時
        public MainWindow(AppSetting setting)
        {
            Current = this;
            InitMainWindow(setting);
        }

        // 起動時(引数有り)
        public MainWindow(AppSetting setting, string[] args)  
        {
            Current = this;

            setting.TempProfile.Path.Value.Clear();
            InitMainWindow(setting);
            DropNewFiles(args);
        }


        private void InitMainWindow(AppSetting setting)
        {
            Setting = setting;
            this.AllowsTransparency = (bool)Setting.TempProfile.AllowTransparency.Value;

            IgnoreResizeEvent = true;

            InitializeComponent();

            // debug
#if DEBUG
            //Setting.ShortcutSetting = new ShortcutSetting();
#endif

            // init
            InitControls();
            InitControlsEvent();
            InitHelper();
            InitEvent();

            // ショートカットマネージャー
            ShortcutManager = new ShortcutManager();

            // 前回の状態を復元
            Profile pf = Setting.TempProfile;

            // ウインドウ位置・フルスクリーン復元
            this.Left   = pf.WindowPos.X;
            this.Top    = pf.WindowPos.Y;
            this.Width  = pf.WindowSize.Width;
            this.Height = pf.WindowSize.Height;

            if( pf.IsFullScreenMode.Value )
            {
                pf.IsFullScreenMode.Value = false;
                ToggleFullScreen();
                IgnoreResizeEvent = true;
            }

            // 画像情報の読み込みとソート
            String[] files = pf.Path.Value.ToArray();
            ReadFilesAndInitMainContent(files, false, pf.LastPageIndex.Value);

            // 背景色と不透明度
            ApplyColorAndOpacitySetting();

            // UI設定( UpdateMainWindowView()も実行される )
            UpdateUI();

            // ツールバーの見た目
            UpdateToolbarViewing();

            // 最前面
            this.Topmost = Setting.TempProfile.TopMost.Value;

            // ウインドウ描画完了後、リサイズイベントの許可
            this.ContentRendered += (s, e) =>
            {
                IgnoreResizeEvent = false;
            };

            // 自動再生
            if( pf.SlideShowAutoStart.Value ) StartSlideShow(true);
        }

        private void InitControls()
        {
            tileContainers.Add(this.TileContainer1);
            tileContainers.Add(this.TileContainer2);
            tileContainers.Add(this.TileContainer3);
            TileContainer1.ForwardContainer = TileContainer3;
            TileContainer2.ForwardContainer = TileContainer1;
            TileContainer3.ForwardContainer = TileContainer2;
            foreach(TileContainer tc in tileContainers)
            {
                tc.MainWindow = this;
            }

            MenuItem_MatrixSelecter.ApplyTemplate();
            MatrixSelecter = MenuItem_MatrixSelecter.Template.FindName("MatrixSelecter", MenuItem_MatrixSelecter) as MatrixSelecter;
            MatrixSelecter.MaxSize = Setting.MatrixSelecterMaxSize;
            MatrixSelecter.Initialize();

            // タイル拡大パネル
            TileExpantionPanel.MainWindow = this;

            // スライドの設定ダイアログ
            MenuItem_SlideSettingDialog.ApplyTemplate();
            SlideSettingDialog = MenuItem_SlideSettingDialog.Template.FindName("SlideSettingDialog", MenuItem_SlideSettingDialog) as SlideSettingDialog;
            SlideSettingDialog.mainWindow = this;
            SlideSettingDialog.Setting = this.Setting;

            // その他の設定ダイアログ
            MenuItem_SettingDialog.ApplyTemplate();
            SettingDialog = MenuItem_SettingDialog.Template.FindName("SettingDialog", MenuItem_SettingDialog) as SettingDialog;
            SettingDialog.mainWindow = this;
            SettingDialog.Setting = this.Setting;
            SettingDialog.MainTabControl.SelectedIndex = Setting.SettingDialogTabIndex;

            // タイマー
            intervalSlideTimer.Tick += intervalSlideTimer_Tick;
            intervalSlideTimer.Interval = new TimeSpan(0, 0, 0, 1);

        }

        private void InitHelper()
        {
            // helper
            uiHelper = new UIHelper(this);
            imageFileManager = new ImageFileManager();
            foreach(TileContainer tc in tileContainers)
            {
                tc.ImageFileManager = imageFileManager;
            }
            this.TileExpantionPanel.ImageFileManager = imageFileManager;
        }

        public void LoadUserProfile(Profile userProfile)
        {
            // 統合前のTempProfileの設定値
            bool before_IsFullScreenMode = Setting.TempProfile.IsFullScreenMode.Value;

            // 読み込む前のファイルの履歴情報を保存
            SaveHistoryItem();

            // TempProfileに統合
            UpdateTempProfile();
            Setting.TempProfile.Marge(userProfile);

            Profile tp = Setting.TempProfile;

            // ファイルを読み込む場合は、ページ番号は初期化(有効時は除く)
            if( userProfile.Path.IsEnabled && !userProfile.LastPageIndex.IsEnabled )
            {
                tp.LastPageIndex.Value = 0; 
            }

            // 透過有効
            if( tp.AllowTransparency.Value != this.AllowsTransparency)
            {
                MainWindow mw = new MainWindow(this.Setting);
                mw.Show(); // InitMainWindow()でプロファイル適用
                this.Close();
                return; 
            }

            // ウインドウ位置
            if( !before_IsFullScreenMode )
            {
                this.Left   = tp.WindowPos.X;
                this.Top    = tp.WindowPos.Y;
                this.Width  = tp.WindowSize.Width;
                this.Height = tp.WindowSize.Height;
            }
            else
            {
                // 既にフルスクリーン中だった場合
                windowRectBeforeFullScreen = new Rect(tp.WindowPos.X, tp.WindowPos.Y, tp.WindowSize.Width, tp.WindowSize.Height);
            }

            // 画像情報の読み込み、ソート、コンテンツ初期化
            if( userProfile.Path.IsEnabled )
            {
                String[] files = tp.Path.Value.ToArray();
                ReadFilesAndInitMainContent(files, false, tp.LastPageIndex.Value);
            }
            else
            {
                if( userProfile.FileSortMethod.IsEnabled ) SortOnFileLoaded(); // ファイルは読み込まずにソート
                InitMainContent(tp.LastPageIndex.Value);
            }
            
            // 外観更新
            ApplyColorAndOpacitySetting(); // 背景色と不透明度
            UpdateUI(); // UI設定
            UpdateToolbarViewing(); // ツールバーの見た目
            this.Topmost = Setting.TempProfile.TopMost.Value; // 最前面

            // フルスクリーン設定
            if( userProfile.IsFullScreenMode.IsEnabled )
            {
                // UserProfile読み込み前と状態が変わらないなら、ToggleFullScreen()を呼ばない
                if( tp.IsFullScreenMode.Value && !before_IsFullScreenMode)
                {
                    tp.IsFullScreenMode.Value = false;
                    ToggleFullScreen();
                }
                else if( !tp.IsFullScreenMode.Value && before_IsFullScreenMode )
                {
                    tp.IsFullScreenMode.Value = true;
                    ToggleFullScreen();
                }
            }

            // 拡大中だったら、閉じる
            if( TileExpantionPanel.IsShowing ) TileExpantionPanel.Hide();

            // 自動再生
            if( tp.SlideShowAutoStart.Value ) StartSlideShow(false);

            // 読み込み完了メッセージ
            NotificationBlock.Show("プロファイルのロード完了: " + userProfile.Name, NotificationPriority.High, NotificationTime.Short, NotificationType.None);
        }


        /* ---------------------------------------------------- */
        //     
        /* ---------------------------------------------------- */
        public void ReadFilesAndInitMainContent(string[] pathes, bool isAddition, int firstIndex)
        {
            ReadFiles(pathes, isAddition);
            InitMainContent(firstIndex);
        }

        public void InitMainContent(int firstIndex)
        {
            // 「読み込み中」メッセージ
            this.WaitingMessageBase.Visibility = Visibility.Visible; 
            this.WaitingMessageBase.Refresh();

            if( IsPlaying || tileContainers.Any(tc => tc.IsActiveSliding) )
                StopSlideShow(true);

            // profile
            Profile pf = Setting.TempProfile;
            
            // 行列
            ProfileMember.NumofMatrix mtx = Setting.TempProfile.NumofMatrix;

            // アス比
            //ProfileMember.AspectRatio ar = Setting.TempProfile.AspectRatio;
            Point ar = new Point(pf.AspectRatio.H, pf.AspectRatio.V);

            // set up imageFileManager
            int grids = mtx.Col * mtx.Row;
            imageFileManager.FillFileInfoVacancyWithDummy(grids);
            if (firstIndex > imageFileManager.NumofImageFile - 1) firstIndex = 0;
            imageFileManager.NextIndex = firstIndex;
            imageFileManager.ApplyRotateInfoFromExif = pf.ApplyRotateInfoFromExif.Value;

            // init container
            TileContainer.TileAspectRatio = ar.Y / ar.X;
            TileContainer.SetBitmapDecodePixelOfTile(pf.BitmapDecodeTotalPixel.Value, mtx.Col, mtx.Row);
            foreach(TileContainer tc in tileContainers)
            {
                tc.InitSlideDerection(pf.SlideDirection.Value);
                tc.InitGrid(mtx.Col, mtx.Row, pf.TileImageStretch.Value);
                tc.InitGridLineColor(pf.GridLineColor.Value);
                tc.InitSizeAndPos((int)ar.X, (int)ar.Y, pf.TilePadding.Value);
                tc.InitWrapPoint();
                tc.InitTileOrigin(pf.TileOrigin.Value, pf.TileOrientation.Value, pf.UseDefaultTileOrigin.Value);
            }

            // load image
            TileContainer.ReleaseBitmapLoadThread();
            if(imageFileManager.ImgFileInfo.Count > 0)
            {
                foreach(TileContainer tc in tileContainers)
                {
                    tc.LoadImageToGrid(false, false);
                }
            }


            // 巻き戻し用のインデックス初期値は必ずファイルを全てのコンテナに割り当てた後決める
            // (割り当て中にスライドしてしまうので)
            imageFileManager.InitPrevIndex(firstIndex);

            // init ui
            UpdatePageInfo();
            InitSeekbar();

            // 「読み込み中」メッセージ解除
            this.WaitingMessageBase.Visibility = Visibility.Collapsed;
        }

        private void ReadFiles(string[] pathes, bool isAddition)
        {
            // 「読み込み中」メッセージ
            this.WaitingMessageBase.Visibility = Visibility.Visible; 
            this.WaitingMessageBase.Refresh();

            Profile pf = Setting.TempProfile;

            if( isAddition )
            {
                // 追加の場合、ダミーファイル情報を消す
                imageFileManager.ImgFileInfo.RemoveAll( fi => fi.IsDummy );
            }
            else
            {
                // 追加じゃない場合、色々クリア
                pf.Path.Value.Clear();
                imageFileManager.ClearFileInfo();
                tileContainers.ForEach( tc => tc.ClearAllTileImage() );
            }

            // 読み込み
            if (pathes.Length > 0)
            {
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif
                foreach(string path in pathes )
                {
                    imageFileManager.LoadImageFileInfo(path);
                    pf.Path.Value.Add(path);
                }
#if DEBUG
                sw.Stop();
                Debug.WriteLine( pathes.Length + " filePathes loaded"  + " time: " + sw.Elapsed);
#endif
            }

            // ソート
            if( !isAddition ) SortOnFileLoaded();

            // ヒストリーに追加
            if( Setting.EnabledItemsInHistory.ArchiverPath )
            {
                imageFileManager.Archivers.Where(a1 => a1.LeaveHistory).ToList().ForEach( a2 => 
                {
                    HistoryItem hiOld = Setting.History.FirstOrDefault( h => h.ArchiverPath == a2.ArchiverPath );
                    Setting.History.RemoveAll(h => h.ArchiverPath == a2.ArchiverPath);
                    if(hiOld != null)
                        Setting.History.Insert( 0, hiOld );
                    else
                        Setting.History.Insert( 0, new HistoryItem(a2.ArchiverPath) );
                });
            }

            // ヒストリー上限超えを削除
            if(Setting.History.Count > Setting.NumofHistory )
            {
                Setting.History.RemoveRange( Setting.NumofHistory, Setting.History.Count - Setting.NumofHistory);
            }

            // 「読み込み中」メッセージ解除
            this.WaitingMessageBase.Visibility = Visibility.Collapsed;
        }

        public void DropNewFiles(string[] pathes)
        {
            // 拡大中なら解除
            if( TileExpantionPanel.IsShowing ) TileExpantionPanel.Hide();

            // 画像1枚だけ読み込みの時は、親フォルダを読み込む
            if(  Setting.ReadSingleImageAsParentFolder && pathes.Length == 1 && File.Exists(pathes[0]) &&  
                ArchiverBase.AllowedFileExt.Any( ext => pathes[0].ToLower().EndsWith(ext) )  )
            {
                DropNewFileAsFolder(pathes[0]);
                return;
            }

            // 読み込み
            if( Setting.EnabledItemsInHistory.ArchiverPath && pathes.Length == 1 &&
                Setting.History.Any( hi => hi.ArchiverPath == pathes[0]) && Setting.ApplyHistoryInfoInNewArchiverReading)
            {
                // 履歴に存在する場合
                LoadHistory(pathes[0]);
            }
            else
            {
                ReadFilesAndInitMainContent(pathes, false, 0);
            }
        }

        public void DropNewFileAsFolder(string path)
        {
            // 単体画像をフォルダとして読み込み
            string dirPath = Directory.GetParent(path).FullName;

            // 履歴にあるかチェック
            HistoryItem historyItem = Setting.History.FirstOrDefault(hi => hi.ArchiverPath == dirPath);

            // 履歴にあるなら、最後に開いた画像の情報を置き換え
            if( Setting.EnabledItemsInHistory.ArchiverPath && historyItem != null && Setting.ApplyHistoryInfoInNewArchiverReading)
            {
                historyItem.ImagePath = path;
                LoadHistory(dirPath);
            }

            // 履歴にない場合
            else
            {
                ReadFiles(new string[] { dirPath }, false);
                ImageFileInfo dropedFileInfo = imageFileManager.ImgFileInfo.FirstOrDefault(i => i.FilePath == path);
                int firstIndex;
                if(dropedFileInfo != null )
                {
                    firstIndex = imageFileManager.ImgFileInfo.IndexOf(dropedFileInfo);
                }
                else
                {
                    firstIndex = 0;
                }
                InitMainContent(firstIndex);
            }
        }

        private void SortOnFileLoaded()
        {
            if( imageFileManager.Archivers.Count == 1 && Directory.Exists(imageFileManager.Archivers[0].ArchiverPath) && Setting.TempProfile.FileSortMethod.Value == FileSortMethod.FileName)
            {
                // 追加ではなく、フォルダ１つだけを読み込んだ場合ファイル名順になっているので、
                // 並び順設定が「ファイル名(昇順)」ならばソートの必要なし
            }
            else
            {
                imageFileManager.Sort( Setting.TempProfile.FileSortMethod.Value );
            }
        }

        private void InitSeekbar()
        {
            // クリックした時の挙動
            //Seekbar.IsMoveToPointEnabled = true;

            int grids = Setting.TempProfile.NumofMatrix.Grid;
            Seekbar.Maximum = imageFileManager.GetLastNoDeviationIndex(grids) + 1; // 末尾がずれないように
            Seekbar.Minimum = 1;

            // クリック時(Large)、方向キー(Small)押下時の移動量
            Seekbar.LargeChange = 1;
            Seekbar.SmallChange = 1;

            if (tileContainers[0].IsHorizontalSlide)
            {
                SeekbarWrapper.VerticalAlignment = VerticalAlignment.Bottom;
                SeekbarWrapper.HorizontalAlignment = HorizontalAlignment.Stretch;
                SeekbarWrapper.Margin = new Thickness(10, 10, 10, 10);

                Seekbar.HorizontalAlignment = HorizontalAlignment.Stretch;
                Seekbar.VerticalAlignment = VerticalAlignment.Bottom;
            }
            else
            {
                SeekbarWrapper.HorizontalAlignment = HorizontalAlignment.Right;
                SeekbarWrapper.VerticalAlignment = VerticalAlignment.Stretch;
                SeekbarWrapper.Margin = new Thickness(10, 40, 10, 10);

                Seekbar.HorizontalAlignment = HorizontalAlignment.Right;
                Seekbar.VerticalAlignment = VerticalAlignment.Stretch;
            }

            switch ( (SlideDirection)Setting.TempProfile.SlideDirection.Value )
            {
                case SlideDirection.Left:
                default:
                    Seekbar.Orientation = Orientation.Horizontal;
                    Seekbar.IsDirectionReversed = false;
                    break;
                case SlideDirection.Top:
                    Seekbar.Orientation = Orientation.Vertical;
                    Seekbar.IsDirectionReversed = true;
                    break;
                case SlideDirection.Right:
                    Seekbar.Orientation = Orientation.Horizontal;
                    Seekbar.IsDirectionReversed = true;
                    //Seekbar.Dispatcher.BeginInvoke(new Action( () => Seekbar.IsDirectionReversed = true), null);
                    break;
                case SlideDirection.Bottom:
                    Seekbar.Orientation = Orientation.Vertical;
                    Seekbar.IsDirectionReversed = false;
                    break;
            }

            // 表示更新(Seekbar.IsDirectionReversed 変更後のつまみの位置反映の為)
            ignoreSliderValueChangeEvent = true;
            double tmp = Seekbar.Value;
            Seekbar.Value = Seekbar.Maximum;
            Seekbar.Value = Seekbar.Minimum;
            Seekbar.Value = tmp;
            ignoreSliderValueChangeEvent = false;
        }

        public void StartSlideShow(bool allowNotification)
        {
            // ファイル無し
            if (imageFileManager.ImgFileInfo.Count < 1) return;

            // 今再生中
            if (IsPlaying) return;

            if( (SlidePlayMethod)Setting.TempProfile.SlidePlayMethod.Value == SlidePlayMethod.Continuous )
            {
                // 連続的スライド

                // 速度 → 移動にかける時間パラメータ 3000(ms) - 300000(ms)
                int param = (int)( 300000 / (double)Setting.TempProfile.SlideSpeed.Value );

                foreach(TileContainer tc in tileContainers)
                {
                    Point ptFrom = new Point(tc.Margin.Left, tc.Margin.Top);
                    tc.BeginContinuousSlideAnimation(ptFrom, param);
                }
            }
            else
            {
                // インターバルスライド
                intervalSlideTimer.Start();
                intervalSlideTimerCount = 0;
                if( allowNotification )
                {
                    NotificationBlock.Show("スライドショー開始 (待機時間" + Setting.TempProfile.SlideInterval.Value + "秒)", NotificationPriority.Normal, NotificationTime.Short, NotificationType.None);
                }
            }

            // 再生ボタン表示変更
            UpdateToolbarViewing();
        }

        public void StartOperationSlide(bool isPlayback, bool slideByOneImage)
        {
            // ファイル無し
            if (imageFileManager.ImgFileInfo.Count < 1) return;

            // インターバルスライド中なら、停止してスライド処理続行
            if (intervalSlideTimer.IsEnabled) StopSlideShow(true);

            // まだ操作によるアクティブスライド中なら、短いので待つ
            else if (tileContainers.Any(c => c.IsActiveSliding)) return;

            // まだ自動再生中なら、止める
            if (tileContainers.Any(c => c.IsContinuousSliding)) StopSlideShow(false);

            // コンテナがずれているかどうか
            bool isNoDeviation = tileContainers.All(c =>
            {
                if (c.IsHorizontalSlide && c.Margin.Left % c.Width == 0) return true;
                else if (c.IsVerticalSlide && c.Margin.Top % c.Height == 0) return true;
                else return false;
            });

            // 最初の画像(原点)をまたぐかどうか
            bool isCrossOverOrigin = false;
            int grids = Setting.TempProfile.NumofMatrix.Grid;
            int idx = imageFileManager.ActualCurrentIndex;
            if(Setting.CorrectPageIndexInOperationSlideCrrosOverTheOrigin)
            {
                if (isPlayback)
                {
                    if (!slideByOneImage && 0 < idx && idx < grids && isNoDeviation)
                    {
                        isCrossOverOrigin = true;
                    }
                }
                else
                {
                    if(!slideByOneImage && imageFileManager.GetLastNoDeviationIndex(grids) < idx)
                    {
                        isCrossOverOrigin = true;
                    }
                }
            }

            foreach(TileContainer tc in tileContainers)
            {
                int param = Setting.OperationSlideDuration;
                Debug.WriteLine("active slide start: " + tc.Margin);
                Debug.WriteLine("isNodeviation: " + isNoDeviation);
                tc.BeginActiveSlideAnimation(isNoDeviation, isPlayback, slideByOneImage, param, isCrossOverOrigin);
            }


        }

        private void StartIntervalSlide(bool slideByOneImage, int moveTime)
        {
            // ファイル無し
            if (imageFileManager.ImgFileInfo.Count < 1) return;

            // コンテナがずれているかどうか
            bool isNoDeviation = tileContainers.All(c =>
            {
                if (c.IsHorizontalSlide && c.Margin.Left % c.Width == 0) return true;
                else if (c.IsVerticalSlide && c.Margin.Top % c.Height == 0) return true;
                else return false;
            });

            foreach(TileContainer tc in tileContainers)
            {
                int param = moveTime;
                Debug.WriteLine("interval slide start: " + tc.Margin);
                tc.BeginActiveSlideAnimation(isNoDeviation, false, slideByOneImage, param, false);
            }

        }

        public void StopSlideShow(bool allowNotification)
        {
            if(allowNotification && Setting.TempProfile.SlidePlayMethod.Value == SlidePlayMethod.Interval && intervalSlideTimer.IsEnabled)
            {
                NotificationBlock.Show("スライドショー停止", NotificationPriority.Normal, NotificationTime.Short, NotificationType.None);
            }
            intervalSlideTimer.Stop();
            intervalSlideTimerCount = 0;

            foreach(TileContainer tc in tileContainers)
            {
                //if (tc.IsContinuousSliding) tc.StopSlideAnimation();
                tc.StopSlideAnimation();
            }
            //DoEvents();
            //System.Threading.Thread.Sleep(100);
            UpdateToolbarViewing();
        }

        public void UpdateSlideSpeed()
        {
            if (tileContainers[0].IsContinuousSliding)
            {
                StopSlideShow(false);
                StartSlideShow(false);
            }
        }

        public void ChangeSlideDirection(SlideDirection direction)
        {
            if ( direction == (SlideDirection)Setting.TempProfile.SlideDirection.Value ) return;

            Setting.TempProfile.SlideDirection.Value = direction;
            InitMainContent(imageFileManager.CurrentIndex);

            UpdateToolbarViewing();
        }

        public void ChangeGridDifinition(int numofCol, int numofRow)
        {
            if ( Setting.TempProfile.NumofMatrix.Col == numofCol && Setting.TempProfile.NumofMatrix.Row == numofRow) return;

            Setting.TempProfile.NumofMatrix.Value = new int[] { numofCol, numofRow };

            InitMainContent(imageFileManager.CurrentIndex);

            UpdateMainWindowView();
        }

        private void ChangeAspectRatio(int h, int v)
        {
            Setting.TempProfile.AspectRatio.Value = new int[] { h, v };
            InitMainContent(imageFileManager.CurrentIndex);
            UpdateToolbarViewing();

            UpdateMainWindowView();
        }

        public void ChangeCurrentImageIndex(int index)
        {
            //if( ImageFileManager.ActualCurrentIndex == index ) return;
            imageFileManager.NextIndex = index;
            StopSlideShow(true);

            TileContainer.ReleaseBitmapLoadThread();

            foreach(TileContainer tc in tileContainers)
            {
                //tc.InitSizeAndPos(Setting.TempProfile.AspectRatio.H, Setting.TempProfile.AspectRatio.V, Setting.TempProfile.TilePadding.Value);
                tc.InitPos();
                tc.LoadImageToGrid(false, true);
            }

            imageFileManager.PrevIndex = index;
            imageFileManager.DecrementPrevIndex();
            UpdatePageInfo();

        }

        public void FitMainContentToWindow()
        {
            double zoomFactor = (this.Width - MainContent.Margin.Left * 2 ) / this.tileContainers[0].Width;
            this.MainContent.LayoutTransform = new ScaleTransform(zoomFactor, zoomFactor);
        }

        public void UpdateMainWindowView()
        {
            if ( Setting.TempProfile.IsFullScreenMode.Value )
            {
                UpdateFullScreenView();
            }
            else
            {
                UpdateWindowSize();
                FitMainContentToWindow();
            }
        }

        public void UpdateWindowSize()
        {
            //// サイズ変更後の枠内の比率
            double w = tileContainers[0].Width;
            double h = tileContainers[0].Height;
            double nextRate = h / w;

            // サイズ変更前の枠内面積
            double margin = this.MainContent.Margin.Left;
            double prevContentWidth = Width - margin * 2;
            double prevContentHeight = Height - margin * 2;
            double prevArea = prevContentWidth * prevContentHeight;

            // 面積と比率から、枠内矩形の幅と高さを算出
            double nextContentWidth = Math.Sqrt(prevArea / nextRate);
            double nextContentHeight = nextContentWidth * nextRate;

            // 枠を足して適用
            IgnoreResizeEvent = true;
            Width = nextContentWidth + margin * 2;
            Height = nextContentHeight + margin * 2;
            IgnoreResizeEvent = false;
        }

        public void UpdatePageInfo()
        {
            this.PageInfoText.Text = imageFileManager.CreateCurrentIndexInfoString();
            int c = imageFileManager.CurrentIndex + 1;

            this.ignoreSliderValueChangeEvent = true;
            this.Seekbar.Value = c;
            this.ignoreSliderValueChangeEvent = false;
        }

        public void UpdateIntervalSlideTimer()
        {
            if (intervalSlideTimer.IsEnabled)
            {
                intervalSlideTimerCount = 0;
            }
        }
            
        public void UpdateGridLine()
        {
            Profile pf = Setting.TempProfile;

            foreach(TileContainer tc in tileContainers)
            {
                tc.InitGridLineColor( (Color)pf.GridLineColor.Value );
                tc.InitSize(pf.AspectRatio.H, pf.AspectRatio.V, pf.TilePadding.Value);
                //tc.InitSizeAndPos(pf.AspectRatio.H, pf.AspectRatio.V, pf.TilePadding.Value);
            }

            // 現在表示中の画像をそのままに、コンテナの位置を初期化する
            ChangeCurrentImageIndex(imageFileManager.CurrentIndex);

            foreach(TileContainer tc in tileContainers)
            {
                tc.InitWrapPoint();
            }

            UpdateMainWindowView();
        }

        public void UpdateTileArrange()
        {
            // タイルの配置を更新
            InitMainContent(imageFileManager.CurrentIndex);
        }

        public void UpdateUI()
        {
            Profile pf = Setting.TempProfile;
            this.MainContent.Margin = new Thickness(pf.ResizeGripThickness.Value);
            this.ResizeGrip.BorderThickness = new Thickness(pf.ResizeGripThickness.Value);
            this.ResizeGrip.BorderBrush = new SolidColorBrush(pf.ResizeGripColor.Value);
            this.Seekbar.Foreground = new SolidColorBrush(Setting.SeekbarColor);
            this.Seekbar.IsMoveToPointEnabled = Setting.SeekBarIsMoveToPointEnabled;

            UpdateMainWindowView();
        }

        public void UpdateToolbarViewing()
        {
            Profile pf = Setting.TempProfile;

            // アス比 ボタン
            Toolbar_AspectRate_Text.FontSize = 12;
            if( pf.NonFixAspectRatio.Value )
            {
                Toolbar_AspectRate_Text.Text = "Free";
            }
            else
            {
                string h = pf.AspectRatio.H.ToString();
                string v = pf.AspectRatio.V.ToString();
                string aRateTxt;
                if( h.Length + v.Length > 4 )
                {
                    aRateTxt = "Fixed";
                }
                else if( h.Length + v.Length > 3 )
                {
                    Toolbar_AspectRate_Text.FontSize = 11;
                    aRateTxt = h + " : " + v;
                }
                else
                {
                    aRateTxt = h + " : " + v;
                }
                Toolbar_AspectRate_Text.Text = aRateTxt;
            }

            // アス比 チェックマーク、ツールチップ更新
            foreach( var child in LogicalTreeHelper.GetChildren( MenuItem_AspectRatio ) )
            {
                MenuItem i = child as MenuItem;
                if(i != null)
                {
                    i.IsChecked = false;

                    if( i.Tag.ToString() == "FREE" )
                    {
                        if( pf.NonFixAspectRatio.Value ) i.IsChecked = true;

                        // ツールチップ
                        //string tooltip = string.Format("現在のアスペクト比 {0}:{1}", pf.AspectRatio.H, pf.AspectRatio.V);
                        //i.ToolTip = tooltip;
                    }
                    else
                    {
                        string[] str = i.Tag.ToString().Split('_');
                        if(str.Length == 2 )
                        {
                            int w = int.Parse(str[0]);
                            int h = int.Parse(str[1]);
                            if(w == pf.AspectRatio.H && h == pf.AspectRatio.V && !pf.NonFixAspectRatio.Value) i.IsChecked = true;
                        }
                    }
                }
            }


            // 再生 / 停止
            if (IsPlaying)
            {
                Toolbar_Play_Image.Source = new BitmapImage(new Uri("Resources/stop.png", UriKind.Relative));

            }
            else
            {
                Toolbar_Play_Image.Source = new BitmapImage(new Uri("Resources/play.png", UriKind.Relative));
            }

            // スライド
            string iconName = "";
            switch (pf.SlideDirection.Value)
            {
                case SlideDirection.Left:
                    iconName = "slide_left";
                    break;
                case SlideDirection.Top:
                    iconName = "slide_top";
                    break;
                case SlideDirection.Right:
                    iconName = "slide_right";
                    break;
                case SlideDirection.Bottom:
                    iconName = "slide_bottom";
                    break;
            }

            //if (pf.SlidePlayMethod == SlidePlayMethod.Continuous)
            //    iconName += "_orange";
            //else
            //    iconName +=  "_blue";

            iconName += ".png";
            string uri = "Resources/" + iconName;
            MenuItem_SlideSetting_Image.Source = new BitmapImage(new Uri(uri, UriKind.Relative));


        }

        public void ToggleFullScreen()
        {
            if (Setting.TempProfile.IsFullScreenMode.Value)
            {
                // 解除
                this.MainContent.Margin = new Thickness(Setting.TempProfile.ResizeGripThickness.Value);
                this.ResizeGrip.Visibility = Visibility.Visible;
                this.IgnoreResizeEvent = true;
                this.Left = windowRectBeforeFullScreen.Left;
                this.Top = windowRectBeforeFullScreen.Top;
                this.Width = windowRectBeforeFullScreen.Width;
                this.Height = windowRectBeforeFullScreen.Height;
                this.IgnoreResizeEvent = false;
                Setting.TempProfile.IsFullScreenMode.Value = false;
                FullScreenBase_TopLeft.Visibility = Visibility.Hidden;
                FullScreenBase_BottomRight.Visibility = Visibility.Hidden;
                UpdateMainWindowView();

                // システムアイコン変更
                SystemButton_Maximize_Image.Source = 
                    new BitmapImage(new Uri("Resources/maximize.png", UriKind.Relative));

                // 拡大パネル
                if( TileExpantionPanel.IsShowing ) TileExpantionPanel.FitToMainWindow();
            }
            else
            {
                // フルスクリーン開始
                windowRectBeforeFullScreen = new Rect(Left, Top, Width, Height);

                // このウインドウと一番重なりが大きいモニターのサイズを取得
                Rect rcMonitor = Win32.GetScreenRectFromRect(new Rect(Left, Top, Width, Height));

                // サイズ変更
                this.IgnoreResizeEvent = true;
                this.Left = rcMonitor.Left;
                this.Top = rcMonitor.Top;
                this.Width = rcMonitor.Width;
                this.Height = rcMonitor.Height;
                this.IgnoreResizeEvent = false;

                // 適切なコンテナの位置と、拡大率を指定する
                UpdateFullScreenView();

                Setting.TempProfile.IsFullScreenMode.Value = true;
                this.ResizeGrip.Visibility = Visibility.Hidden;

                // システムアイコン変更
                SystemButton_Maximize_Image.Source = 
                    new BitmapImage(new Uri("Resources/normalize.png", UriKind.Relative));

                // 拡大パネル
                if( TileExpantionPanel.IsShowing ) TileExpantionPanel.FitToFullScreenWindow();
            }

        }

        public void UpdateFullScreenView()
        {
            // 拡大率、コンテンツ座標計算
            double monitorRatio = Width / Height;
            double zoomFactor;
            Point origin;
            double w = tileContainers[0].Width;
            double h = tileContainers[0].Height;
            if(MainContentAspectRatio > monitorRatio)
            {
                // モニターよりも横長 → 拡大率はモニターの幅と比べて決まる
                zoomFactor = Width / w;
                double d = h * zoomFactor / 2;
                origin = new Point(0, Height / 2 - d);
                FullScreenBase_TopLeft.Height = origin.Y;
                FullScreenBase_TopLeft.Width = Width;
                FullScreenBase_BottomRight.Height = origin.Y;
                FullScreenBase_BottomRight.Width = Width;
                FullScreenBase_TopLeft.Visibility = Visibility.Visible;
                FullScreenBase_BottomRight.Visibility = Visibility.Visible;
            }
            else if(MainContentAspectRatio < monitorRatio)
            {
                // モニターよりも縦長
                zoomFactor = Height / h;
                double d = w * zoomFactor / 2;
                origin = new Point(Width / 2 - d, 0);
                FullScreenBase_TopLeft.Height = Height;
                FullScreenBase_TopLeft.Width = origin.X;
                FullScreenBase_BottomRight.Height = Height;
                FullScreenBase_BottomRight.Width = origin.X;
                FullScreenBase_TopLeft.Visibility = Visibility.Visible;
                FullScreenBase_BottomRight.Visibility = Visibility.Visible;
            }
            else
            {
                zoomFactor = Width / w;
                origin = new Point(0, 0);
                FullScreenBase_TopLeft.Visibility = Visibility.Hidden;
                FullScreenBase_BottomRight.Visibility = Visibility.Hidden;
            }

            // コンテナ位置
            this.MainContent.Margin = new Thickness(origin.X, origin.Y, 0, 0);

            // 拡大率
            this.MainContent.LayoutTransform = new ScaleTransform(zoomFactor, zoomFactor);
        }

        public void ApplyAllowTransparency()
        {
            if (this.AllowsTransparency == Setting.TempProfile.AllowTransparency.Value) return;

            UpdateTempProfile();
            MainWindow mw = new MainWindow(this.Setting);

            mw.Show();
            this.Close();
        }

        public void ApplyColorAndOpacitySetting()
        {
            Profile pf = Setting.TempProfile;

            if (this.AllowsTransparency)
            {

                // 透過有効時、透過設定時用背景Gridを有効に
                this.Bg_ForTransparencySetting.Visibility = Visibility.Visible;
                this.BaseGrid.Background = new SolidColorBrush(Colors.Transparent);

                // 背景ブラシ
                if( Setting.TempProfile.UsePlaidBackground.Value )
                {
                    this.Bg_ForTransparencySetting.Background = Util.CreatePlaidBrush(
                        pf.BaseGridBackgroundColor.Value, pf.PairColorOfPlaidBackground.Value);
                }
                else
                    this.Bg_ForTransparencySetting.Background = new SolidColorBrush(pf.BaseGridBackgroundColor.Value);

                // 不透明度
                this.BaseGrid.Opacity = Setting.TempProfile.OverallOpacity.Value;
                this.Bg_ForTransparencySetting.Opacity = Setting.TempProfile.BackgroundOpacity.Value;
            }
            else
            {
                this.Bg_ForTransparencySetting.Visibility = Visibility.Hidden;
                if( pf.UsePlaidBackground.Value )
                {
                    this.BaseGrid.Background = Util.CreatePlaidBrush(
                        pf.BaseGridBackgroundColor.Value, pf.PairColorOfPlaidBackground.Value);
                }
                else
                    this.BaseGrid.Background = new SolidColorBrush(pf.BaseGridBackgroundColor.Value);
                this.BaseGrid.Opacity = 1.0;
            }
        }

        private void SaveWindowRect()
        {
            Rect rc;
            if( Setting.TempProfile.IsFullScreenMode.Value )
            {
                rc = windowRectBeforeFullScreen;
            }
            else
            {
                rc = new Rect(Left, Top, Width, Height);
            }

            Setting.TempProfile.WindowPos.Value = new Point( rc.Left, rc.Top );
            Setting.TempProfile.WindowSize.Value = new Size( rc.Width, rc.Height );
        }

        public void UpdateTempProfile()
        {
            SaveWindowRect();
            Setting.TempProfile.LastPageIndex.Value = imageFileManager.CurrentIndex;
            Setting.SettingDialogTabIndex = SettingDialog.MainTabControl.SelectedIndex;
            if( IsPlaying ) Setting.TempProfile.SlideShowAutoStart.Value = true;
            else Setting.TempProfile.SlideShowAutoStart.Value = false;
        }

        public void SortAllImage(FileSortMethod order)
        {
            // 読み込み中のメッセージ
            this.WaitingMessageBase.Visibility = Visibility.Visible;
            this.WaitingMessageBase.Refresh();

            if( order == FileSortMethod.None )
                ReadFiles(Setting.TempProfile.Path.Value.ToArray(), false);
            int grids = Setting.TempProfile.NumofMatrix.Grid;
            imageFileManager.Sort(order);
            imageFileManager.FillFileInfoVacancyWithDummy(grids);
            ChangeCurrentImageIndex(0);

            this.WaitingMessageBase.Visibility = Visibility.Collapsed;
        }

        public List<TileContainer> GetTileContainersInCurrentOrder()
        {
            List<TileContainer> containersInCurrentOrder;
            switch (Setting.TempProfile.SlideDirection.Value)
            {
                case SlideDirection.Left:
                default:
                    containersInCurrentOrder = tileContainers.OrderBy(c => c.Margin.Left).ToList();
                    break;
                case SlideDirection.Top:
                    containersInCurrentOrder = tileContainers.OrderBy(c => c.Margin.Top).ToList();
                    break;
                case SlideDirection.Right:
                    containersInCurrentOrder = tileContainers.OrderByDescending(c => c.Margin.Left).ToList();
                    break;
                case SlideDirection.Bottom:
                    containersInCurrentOrder = tileContainers.OrderByDescending(c => c.Margin.Top).ToList();
                    break;
            }

            return containersInCurrentOrder;
        }

        public Tile GetTileUnderCursor()
        {
            // MainWindow上の座標取得
            Point p = Mouse.GetPosition(this);

            // カーソル下のオブジェクトを取得
            //VisualTreeHelper.HitTest(mw, null, new HitTestResultCallback(OnHitTestResultCallback), new PointHitTestParameters(p));
            IInputElement ie = this.InputHitTest(p);
            DependencyObject source = ie as DependencyObject;
            if( source == null ) return null;

            // 拡大時は1つしか候補が無い
            if( TileExpantionPanel.IsShowing ) return TileExpantionPanel.TargetTile;

            // クリックされたTileContainer
            TileContainer tc = WpfTreeUtil.FindAncestor<TileContainer>(source);
            if( tc == null ) return null;

            // クリックされたBorder
            Border border;
            if( source is Border ) border = source as Border;
            else
            {
                border = WpfTreeUtil.FindAncestor<Border>(source);
            }
            if( border == null ) return null;

            // 紐づけられているTileオブジェクトを特定
            Tile targetTile = tc.Tiles.FirstOrDefault(t => t.Border == border);
            if( targetTile == null ) return null;

            return targetTile;
        }

        public void Reload(bool keepCurrentIdx)
        {
            // 拡大パネル表示中なら閉じる
            if( TileExpantionPanel.IsShowing ) TileExpantionPanel.Hide();

            // 画像情報の読み込みとソート
            String[] files = Setting.TempProfile.Path.Value.ToArray();
            ReadFiles(files, false);

            // コンテンツ初期化
            if( keepCurrentIdx )
            {
                InitMainContent(imageFileManager.CurrentIndex);
            }
            else
            {
                InitMainContent(0);
            }

            // 画面更新
            UpdateMainWindowView();
        }

        public void SaveHistoryItem()
        {
            EnabledItemsInHistory ei = Setting.EnabledItemsInHistory;
            if( !ei.ImagePath && !ei.Matrix && !ei.SlideDirection ) return;

            // アーカイバが１つの時だけ保存
            if( imageFileManager.IsSingleArchiver && imageFileManager.Archivers[0].LeaveHistory )
            {
                HistoryItem hi = Setting.History.FirstOrDefault( h => h.ArchiverPath == imageFileManager.Archivers[0].ArchiverPath );
                if(hi != null )
                {
                    // 最後に読み込んだ画像のパス(並び順に依らないページ番号復元用)
                    if( ei.ImagePath )
                    {
                        ImageFileInfo ifi = imageFileManager.CurrentImageFileInfo;
                        if( ifi != null ) hi.ImagePath = imageFileManager.CurrentImageFileInfo.FilePath;
                        else hi.ImagePath = null;
                    }
                    // アス比
                    if(ei.AspectRatio)
                        hi.AspectRatio = new int[] { Setting.TempProfile.AspectRatio.H, Setting.TempProfile.AspectRatio.V };
                    // 行列
                    if( ei.Matrix )
                        hi.Matrix = new int[] { Setting.TempProfile.NumofMatrix.Col, Setting.TempProfile.NumofMatrix.Row };
                    // スライド方向
                    if( ei.SlideDirection )
                        hi.SlideDirection = Setting.TempProfile.SlideDirection.Value;
                }
            }
        }

        public void LoadHistory(string path)
        {
            EnabledItemsInHistory ei = Setting.EnabledItemsInHistory;
            HistoryItem hi = Setting.History.FirstOrDefault( h => h.ArchiverPath == path );
            Profile pf = Setting.TempProfile;

            if(  hi != null && ( Directory.Exists(path) || File.Exists(path) )  )
            {
                // ファイル情報読み込み
                ReadFiles(new string[]{ hi.ArchiverPath }, false);

                // 最後に読み込んだ画像(ページ番号)
                pf.LastPageIndex.Value = 0;
                if( ei.ImagePath && hi.ImagePath != null)
                {
                    ImageFileInfo lastImageInfo = imageFileManager.ImgFileInfo.FirstOrDefault( ifi => ifi.FilePath == hi.ImagePath );
                    if( lastImageInfo != null )
                    {
                        pf.LastPageIndex.Value = imageFileManager.ImgFileInfo.IndexOf(lastImageInfo);
                    }
                }
                // アス比
                if( ei.AspectRatio && hi.AspectRatio != null )
                {
                    Array.Copy(hi.AspectRatio, pf.AspectRatio.Value, pf.AspectRatio.Value.Length);
                }

                // 行列設定
                if( ei.Matrix && hi.Matrix != null )
                {
                    Array.Copy(hi.Matrix, pf.NumofMatrix.Value, pf.NumofMatrix.Value.Length);
                }

                // スライド方向
                if( ei.SlideDirection && !(hi.SlideDirection == SlideDirection.None) )
                {
                    pf.SlideDirection.Value = hi.SlideDirection;
                }

                InitMainContent(pf.LastPageIndex.Value);

                // 更新
                UpdateMainWindowView();
                UpdateToolbarViewing();
            }
        }

        public void ShowProfileEditDialog(ProfileEditDialogMode mode, UserProfileInfo targetUseProfileInfo)
        {
            if(ProfileEditDialog == null )
            {
                ProfileEditDialog = new ProfileEditDialog();
                ProfileEditDialog.Owner = this;
            }
            ProfileEditDialog.Mode = mode;
            ProfileEditDialog.UserProfileList = Setting.UserProfileList;

            // 表示前にメインウインドウのウインドウ情報、ページ情報保存
            UpdateTempProfile();

            // モードに依る挙動
            string message = "";
            if(mode == ProfileEditDialogMode.New || targetUseProfileInfo == null)
            {
                ProfileEditDialog.Title = "プロファイルの作成";
                message = "現在の状態・設定をプロファイルとして保存することが出来ます。\r\nプロファイルに含める項目にチェックを入れて下さい。";
                ProfileEditDialog.SetControlValue(Setting.TempProfile);
            }
            else if( mode == ProfileEditDialogMode.Edit )
            {
                ProfileEditDialog.Title = "プロファイルの編集 (" + targetUseProfileInfo.Profile.Name + ")";
                message = null;
                ProfileEditDialog.EditingUserProfileInfo = targetUseProfileInfo;

                // コントロールの値は、TempProfileに統合したものを表示
                Profile pf = Setting.TempProfile.Clone().Marge(targetUseProfileInfo.Profile);
                pf.Name = targetUseProfileInfo.Profile.Name;
                ProfileEditDialog.SetControlValue(pf); 
            }
            ProfileEditDialog.Label_Message.Content = message;

            // 初期位置は、メインウインドウの中心に
            Util.SetWindowCenterOnWindow(this, ProfileEditDialog);

            // プロファイル編集ダイアログ表示
            ProfileEditDialog.ShowDialog();
            ProfileEditDialog.MainScrollViewer.ScrollToTop();
        }

        public void ShowAppSettingDialog(int tabIndex)
        {
            AppSettingDialog appSettingDialog = new AppSettingDialog();
            appSettingDialog.Owner = this;

            // サイズ復元
            if( !(Setting.AppSettingDialogSize == Size.Empty) )
            {
                appSettingDialog.Width  = Setting.AppSettingDialogSize.Width;
                appSettingDialog.Height = Setting.AppSettingDialogSize.Height;
            }

            // 表示位置はメインウインドウ中心に
            Util.SetWindowCenterOnWindow(this, appSettingDialog);

            // 値の代入
            appSettingDialog.Initialize();
            appSettingDialog.MainTabControl.SelectedIndex = tabIndex;
            appSettingDialog.ShortcutSettingTab.SelectedIndex = Setting.AppSettingDialog_ShortcutSettingTabIndex;

            // 表示
            appSettingDialog.ShowDialog();
        }

        public void ShowProfileListEditDialog()
        {
            ProfileListEditDialog profileListEditDialog = new ProfileListEditDialog();
            profileListEditDialog.Owner = this;
            Util.SetWindowCenterOnWindow(this, profileListEditDialog);
            profileListEditDialog.Initialize();
            profileListEditDialog.ShowDialog();
        }

        public void RemoveUserProfileInfo(UserProfileInfo upi)
        {
            Setting.UserProfileList.Remove(upi);
            string xmlDir = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName + "\\Profile";
            string xmlPath = xmlDir + "\\" + upi.RelativePath;
            if(File.Exists(xmlPath) ) File.Delete(xmlPath);
        }

        public UserProfileInfo CopyUserProfileInfo(UserProfileInfo upi)
        {
            string xmlDir = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName + "\\Profile";

            // コピー先の相対パスを決める
            string srcRelativePathWithoutExt = System.IO.Path.GetFileNameWithoutExtension(upi.RelativePath);
            string newRelativePath = srcRelativePathWithoutExt + "_コピー.xml";
            int cnt = 2;
            while( File.Exists(xmlDir + "\\" + newRelativePath) )
            {
                newRelativePath = srcRelativePathWithoutExt + "_コピー(" + cnt + ").xml";
                cnt++;
                if( cnt >= int.MaxValue ) return null;
            }

            // Profileのコピー
            Profile newProfile = upi.Profile.Clone();
            string newProfileName = System.IO.Path.GetFileName(newRelativePath);
            newProfileName = System.IO.Path.GetFileNameWithoutExtension(newProfileName);
            newProfile.Name = newProfileName;

            // UserProfileInfoのコピー
            UserProfileInfo newUpi = new UserProfileInfo(newRelativePath);
            newUpi.Profile = newProfile;

            // Profileのコピーをxmlに保存
            newUpi.SaveProfileToXmlFile();


            return newUpi;
        }

        public void OpenCurrentFolderByExplorer()
        {
            ImageFileInfo fi = imageFileManager.CurrentImageFileInfo;
            string archiverPath;

            if(fi.Archiver is NullArchiver )
            {
                if( fi.IsDummy ) return;
                archiverPath = Directory.GetParent(fi.FilePath).FullName;
            }
            else
            {
                archiverPath = fi.Archiver.ArchiverPath;
            }

            Process.Start("explorer.exe", "/select,\"" + archiverPath + "\"");

        }

    }
}
