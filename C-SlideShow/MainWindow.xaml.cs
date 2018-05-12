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
    /// <summary>
    /// PreviewMouseRightButtonDownイベントからUpまでの付随情報
    /// 右クリック押しながらのマウス操作(ホイール等)と右クリック操作の切り分けに利用
    /// </summary>
    public class PrevMouseRButtonDownEventContext
    {
        public bool IsPressed { get; set; } = false;
        public bool Handled { get; set; } = false;
        //public Point Position { get; set; } = new Point(0, 0);
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // field
        BitmapPresenter bitmapPresenter;
        DispatcherTimer intervalSlideTimer = new DispatcherTimer(DispatcherPriority.Normal) ;
        int intervalSlideTimerCount = 0;
        List<TileContainer> tileContainers = new List<TileContainer>();
        bool ignoreSliderValueChangeEvent = false; // SliderのValue変更時にイベントを飛ばさないフラグ
        bool ignoreResizeEvent = false;
        bool isSeekbarDragStarted = false;
        PrevMouseRButtonDownEventContext prevMouseRButtonDownEventContext = new PrevMouseRButtonDownEventContext();

        UIHelper uiHelper;
        Rect windowRectBeforeFullScreen = new Rect(50, 50, 400, 300);
        bool isTopmostBeforeFullScreen = false;

        MatrixSelecter matrixSelecter;
        SlideSettingDialog slideSettingDialog;
        SettingDialog settingDialog;


        // property
        public AppSetting Setting { get; set; }
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



        public MainWindow()
        {
            // load setting from xml
            AppSetting setting = new AppSetting().loadFromXmlFile();

            InitMainWindow(setting);
        }

        public MainWindow(AppSetting setting)  // AllowTransparency の切替時に利用
        {
            InitMainWindow(setting);
        }



        private void InitMainWindow(AppSetting setting)
        {
            Setting = setting;
            this.AllowsTransparency = Setting.TempProfile.AllowTransparency;

            InitializeComponent();

            // debug
            //Setting.TempProfile.TilePadding = 3;
            //this.TileContainer1.PreviewMouseDoubleClick += (s, e) =>
            //{
            //    MessageBox.Show("hoge");
            //};

            // init
            InitControls();
            InitHelper();
            InitEvent();
            LoadProfile(Setting.TempProfile);

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
            matrixSelecter = MenuItem_MatrixSelecter.Template.FindName("MatrixSelecter", MenuItem_MatrixSelecter) as MatrixSelecter;

            // タイル拡大パネル
            TileExpantionPanel.MainWindow = this;

            // スライドの設定ダイアログ
            MenuItem_SlideSettingDialog.ApplyTemplate();
            slideSettingDialog = MenuItem_SlideSettingDialog.Template.FindName("SlideSettingDialog", MenuItem_SlideSettingDialog) as SlideSettingDialog;
            slideSettingDialog.mainWindow = this;
            slideSettingDialog.Setting = this.Setting;

            // その他の設定ダイアログ
            MenuItem_SettingDialog.ApplyTemplate();
            settingDialog = MenuItem_SettingDialog.Template.FindName("SettingDialog", MenuItem_SettingDialog) as SettingDialog;
            settingDialog.mainWindow = this;
            settingDialog.Setting = this.Setting;

            // タイマー
            intervalSlideTimer.Tick += intervalSlideTimer_Tick;
            intervalSlideTimer.Interval = new TimeSpan(0, 0, 0, 1);

            // setting to dialog components
            matrixSelecter.SetMatrix(Setting.TempProfile.NumofRow, Setting.TempProfile.NumofCol);
            slideSettingDialog.ApplySettingToDlg();
            settingDialog.ApplySettingToDlg();
        }

        private void InitHelper()
        {
            // helper
            uiHelper = new UIHelper(this);
            bitmapPresenter = new BitmapPresenter();
            foreach(TileContainer tc in tileContainers)
            {
                tc.BitmapPresenter = bitmapPresenter;
            }
            this.TileExpantionPanel.BitmapPresenter = bitmapPresenter;
        }

        private void LoadProfile(Profile profile)
        {
            // ウインドウ位置
            this.Left = profile.WindowRect.Left;
            this.Top = profile.WindowRect.Top;
            this.Width = profile.WindowRect.Width;
            this.Height = profile.WindowRect.Height;
            profile.IsFullScreenMode = false; // todo (設定保存無視で、初期値固定)

            // 画像情報の読み込みとソート
            String[] files = { profile.FolderPath };
            ReadFiles(files);

            // メインコンテンツ作成
            InitMainContent(profile.LastPageIndex);

            // 背景色と不透明度
            ApplyColorAndOpacitySetting();

            // UI設定
            UpdateUI();

            // ツールバーの見た目
            UpdateToolbarViewing();

            // 最前面
            this.Topmost = Setting.TempProfile.TopMost;
        }


        /* ---------------------------------------------------- */
        //     
        /* ---------------------------------------------------- */
        private void InitMainContent(int firstIndex)
        {
            if(tileContainers.Any( tc => tc.IsActiveSliding || tc.IsContinuousSliding))
                StopSlideShow();

            // profile
            Profile pf = Setting.TempProfile;

            // set up bitmap presenter
            int grids = pf.NumofRow * pf.NumofCol;
            bitmapPresenter.FillFileInfoVacancyWithDummy(grids);
            if (firstIndex > bitmapPresenter.NumofImageFile - 1) firstIndex = 0;
            bitmapPresenter.NextIndex = firstIndex;
            bitmapPresenter.BitmapDecodePixelWidth = pf.BitmapDecodeTotalPixelWidth / pf.NumofCol;

            // init container
            foreach(TileContainer tc in tileContainers)
            {
                tc.InitSlideDerection(pf.SlideDirection);
                tc.InitGrid(pf.NumofRow, pf.NumofCol);
                tc.InitGridLineColor(pf.GridLineColor);
                tc.InitSizeAndPos(pf.AspectRatioH, pf.AspectRatioV, pf.TilePadding);
                tc.InitWrapPoint();
                tc.InitTileOrigin(pf.TileOrigin, pf.TileOrientation, true);
            }

            // load image
            TileContainer.ReleaseBitmapLoadThread();
            if(bitmapPresenter.ImgFileInfo.Count > 0)
            {
                foreach(TileContainer tc in tileContainers)
                {
                    tc.LoadImageToGrid(false, true);
                }
            }

            // 巻き戻し用のインデックス初期値は必ずファイルを全てのコンテナに割り当てた後決める
            // (割り当て中にスライドしてしまうので)
            bitmapPresenter.InitPrevIndex(firstIndex);

            // init ui
            UpdatePageInfo();
            InitSeekbar();
        }

        private void ReadFiles(string[] files)
        {
            if (files != null) {
                if(System.IO.Directory.Exists(files[0]))
                {
                    bitmapPresenter.LoadFileInfoFromDir(files[0]);
                    Setting.TempProfile.FolderPath = files[0];
                }
                else if (System.IO.Path.GetExtension(files[0]) == ".zip")
                {
                    bitmapPresenter.LoadFileInfoFromZip(files[0]);
                    Setting.TempProfile.FolderPath = files[0];
                }
                else
                {
                    bitmapPresenter.LoadFileInfoFromFile(files);
                }
            }

            // ソート
            Profile profile = Setting.TempProfile;
            if(bitmapPresenter.ReadType == BitmapReadType.File )
            {
                if (profile.FileReadingOrder != FileReadingOrder.FileName)
                    bitmapPresenter.Sort(profile.FileReadingOrder);
            }
            else
            {
                bitmapPresenter.Sort(profile.FileReadingOrder);
            }
        }

        private void InitSeekbar()
        {
            int grids = Setting.TempProfile.NumofRow * Setting.TempProfile.NumofCol;
            Seekbar.Maximum = bitmapPresenter.GetLastNoDeviationIndex(grids) + 1; // 末尾がずれないように
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

            switch (Setting.TempProfile.SlideDirection)
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

        public void StartSlideShow()
        {
            // ファイル無し
            if (bitmapPresenter.ImgFileInfo.Count < 1) return;

            // 今再生中
            if (IsPlaying) return;

            if(Setting.TempProfile.SlidePlayMethod == SlidePlayMethod.Continuous)
            {
                // 連続スライド

                // 速度 → 移動にかける時間パラメータ 3000(ms) - 300000(ms)
                int param = (int)( 300000 / Setting.TempProfile.SlideSpeed );

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
            }

            // 再生ボタン表示変更
            UpdateToolbarViewing();
        }

        public void StartOperationSlide(bool isPlayback, bool slideByOneImage, int moveTime)
        {
            // ファイル無し
            if (bitmapPresenter.ImgFileInfo.Count < 1) return;

            // インターバルスライド中なら、停止してスライド処理続行
            if (intervalSlideTimer.IsEnabled) StopSlideShow();

            // まだ操作によるアクティブスライド中なら、短いので待つ
            else if (tileContainers.Any(c => c.IsActiveSliding)) return;

            // まだ自動再生中なら、止める
            if (tileContainers.Any(c => c.IsContinuousSliding)) StopSlideShow();

            // 最初と最後の切り替えは、index 0 を通すように(画像１枚毎のスライドの時はしない)
            int grids = Setting.TempProfile.NumofCol * Setting.TempProfile.NumofRow;
            int idx = bitmapPresenter.ActualCurrentIndex;
            if (isPlayback)
            {
                if (!slideByOneImage && 0 < idx && idx < grids)
                {
                    ChangeCurrentImageIndex(0);
                    return;
                }
            }
            else
            {
                if(!slideByOneImage && bitmapPresenter.GetLastNoDeviationIndex(grids) < idx)
                {
                    ChangeCurrentImageIndex(0);
                    return;
                }
            }

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
                Debug.WriteLine("active slide start: " + tc.Margin);
                Debug.WriteLine("isNodeviation: " + isNoDeviation);
                tc.BeginActiveSlideAnimation(isNoDeviation, isPlayback, slideByOneImage, param);
            }
        }

        private void StartIntervalSlide(bool slideByOneImage, int moveTime)
        {
            // ファイル無し
            if (bitmapPresenter.ImgFileInfo.Count < 1) return;

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
                tc.BeginActiveSlideAnimation(isNoDeviation, false, slideByOneImage, param);
            }

        }

        public void StopSlideShow()
        {
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
                StopSlideShow();
                StartSlideShow();
            }
        }

        public void ChangeSlideDirection(SlideDirection direction)
        {
            if (direction == Setting.TempProfile.SlideDirection) return;

            Setting.TempProfile.SlideDirection = direction;
            InitMainContent(bitmapPresenter.CurrentIndex);

            UpdateToolbarViewing();
        }

        private void ChangeGridDifinition(int numofRow, int numofCol)
        {
            if (Setting.TempProfile.NumofRow == numofRow && Setting.TempProfile.NumofCol == numofCol) return;

            Setting.TempProfile.NumofRow = numofRow;
            Setting.TempProfile.NumofCol = numofCol;

            InitMainContent(bitmapPresenter.CurrentIndex);

            if (Setting.TempProfile.IsFullScreenMode)
            {
                UpdateFullScreenView();
            }
            else
            {
                UpdateWindowSize();
                FitMainContentToWindow();
            }
        }

        private void ChangeAspectRatio(int h, int v)
        {
            Setting.TempProfile.AspectRatioH = h;
            Setting.TempProfile.AspectRatioV = v;
            InitMainContent(bitmapPresenter.CurrentIndex);
            UpdateToolbarViewing();

            if (Setting.TempProfile.IsFullScreenMode)
            {
                UpdateFullScreenView();
            }
            else
            {
                UpdateWindowSize();
                FitMainContentToWindow();
            }
        }

        public void ChangeCurrentImageIndex(int index)
        {
            bitmapPresenter.NextIndex = index;
            StopSlideShow();

            TileContainer.ReleaseBitmapLoadThread();

            foreach(TileContainer tc in tileContainers)
            {
                tc.InitSizeAndPos(Setting.TempProfile.AspectRatioH, Setting.TempProfile.AspectRatioV, Setting.TempProfile.TilePadding);
                tc.LoadImageToGrid(false, true);
            }

            bitmapPresenter.PrevIndex = index;
            bitmapPresenter.DecrementPrevIndex();
            UpdatePageInfo();

        }

        public void FitMainContentToWindow()
        {
            double zoomFactor = (this.Width - MainContent.Margin.Left * 2 ) / this.tileContainers[0].Width;
            this.MainContent.LayoutTransform = new ScaleTransform(zoomFactor, zoomFactor);
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
            Width = nextContentWidth + margin * 2;
            Height = nextContentHeight + margin * 2;
        }

        public void UpdatePageInfo()
        {
            this.PageInfoText.Text = bitmapPresenter.CreateCurrentIndexInfoString();
            int c = bitmapPresenter.CurrentIndex + 1;

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
            
        public void UpdateUI()
        {
            Profile pf = Setting.TempProfile;
            this.MainContent.Margin = new Thickness(pf.UI_ResizeGripThickness);
            this.ResizeGrip.BorderThickness = new Thickness(pf.UI_ResizeGripThickness);
            this.ResizeGrip.BorderBrush = new SolidColorBrush(pf.UI_ResizeGripColor);
            this.Seekbar.Foreground = new SolidColorBrush(pf.UI_SeekbarColor);

            UpdateWindowSize();
            FitMainContentToWindow();
        }

        public void UpdateToolbarViewing()
        {
            Profile pf = Setting.TempProfile;

            // アス比 ボタン
            if( pf.NonFixAspectRatio )
            {
                Toolbar_AspectRate_Text.Text = "Free";
            }
            else
            {
                string aRateTxt = pf.AspectRatioH.ToString() + " : " + pf.AspectRatioV.ToString();
                Toolbar_AspectRate_Text.Text = aRateTxt;
            }

            // アス比 チェックマーク更新
            foreach( var child in LogicalTreeHelper.GetChildren( MenuItem_AspectRatio ) )
            {
                MenuItem i = child as MenuItem;
                if(i != null)
                {
                    i.IsChecked = false;

                    if( !pf.NonFixAspectRatio && i.Tag.ToString() != "FREE")
                    {
                        string[] str = i.Tag.ToString().Split('_');
                        int w = int.Parse(str[0]);
                        int h = int.Parse(str[1]);
                        if(w == pf.AspectRatioH && h == pf.AspectRatioV ) i.IsChecked = true;
                    }
                }
            }
            if( pf.NonFixAspectRatio ) Toolbar_AspectRate_Free.IsChecked = true;


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
            switch (pf.SlideDirection)
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
            if (Setting.TempProfile.IsFullScreenMode)
            {
                // 解除
                this.MainContent.Margin = new Thickness(Setting.TempProfile.UI_ResizeGripThickness);
                this.ResizeGrip.Visibility = Visibility.Visible;
                this.Topmost = isTopmostBeforeFullScreen;
                this.Left = windowRectBeforeFullScreen.Left;
                this.Top = windowRectBeforeFullScreen.Top;
                this.Width = windowRectBeforeFullScreen.Width;
                this.Height = windowRectBeforeFullScreen.Height;
                Setting.TempProfile.IsFullScreenMode = false;
                FullScreenBase_TopLeft.Visibility = Visibility.Hidden;
                FullScreenBase_BottomRight.Visibility = Visibility.Hidden;
                UpdateWindowSize();
                FitMainContentToWindow();

                // システムアイコン変更
                SystemButton_Maximize_Image.Source = 
                    new BitmapImage(new Uri("Resources/maximize.png", UriKind.Relative));
            }
            else
            {
                // フルスクリーン開始
                windowRectBeforeFullScreen = new Rect(Left, Top, Width, Height);
                isTopmostBeforeFullScreen = this.Topmost;

                // このウインドウと一番重なりが大きいモニターのサイズを取得
                Rect rcMonitor = Win32.GetScreenRectFromRect(new Rect(Left, Top, Width, Height));

                // サイズ変更
                this.ignoreResizeEvent = true;
                this.Topmost = true;
                this.Left = rcMonitor.Left;
                this.Top = rcMonitor.Top;
                this.Width = rcMonitor.Width;
                this.Height = rcMonitor.Height;
                this.ignoreResizeEvent = false;

                // 適切なコンテナの位置と、拡大率を指定する
                UpdateFullScreenView();

                Setting.TempProfile.IsFullScreenMode = true;
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
            if (this.AllowsTransparency == Setting.TempProfile.AllowTransparency) return;

            SaveWindowRect();
            Setting.TempProfile.LastPageIndex = bitmapPresenter.CurrentIndex;
            MainWindow mw = new MainWindow(this.Setting);

            mw.Show();
            this.Close();
        }

        public void ApplyColorAndOpacitySetting()
        {
            Color bkColor = Setting.TempProfile.BaseGridBackgroundColor;
            if (this.AllowsTransparency)
            {
                if (Setting.TempProfile.ApplyOpacityToAlphaChannelOnly)
                {
                    // アルファチャンネルのみ適用
                    this.BaseGrid_ForAlphaChannel.Visibility = Visibility.Visible;
                    this.BaseGrid_ForAlphaChannel.Background = new SolidColorBrush(bkColor);
                    this.BaseGrid_ForAlphaChannel.Opacity = Setting.TempProfile.BaseGridOpacity;
                    this.BaseGrid.Opacity = 1.0;
                    this.BaseGrid.Background = new SolidColorBrush(Colors.Transparent);
                }
                else
                {
                    // 画像全域に適用
                    this.BaseGrid_ForAlphaChannel.Visibility = Visibility.Hidden;
                    this.BaseGrid.Background = new SolidColorBrush(bkColor);
                    this.BaseGrid.Opacity = Setting.TempProfile.BaseGridOpacity;
                }
            }
            else
            {
                this.BaseGrid_ForAlphaChannel.Visibility = Visibility.Hidden;
                this.BaseGrid.Background = new SolidColorBrush(bkColor);
                this.BaseGrid.Opacity = 1.0;
            }
        }

        private void SaveWindowRect()
        {
            if (Setting.TempProfile.IsFullScreenMode)
                Setting.TempProfile.WindowRect = windowRectBeforeFullScreen;
            else
                Setting.TempProfile.WindowRect = new Rect(Left, Top, Width, Height);
        }

        public void SortAllImage(FileReadingOrder order)
        {
            int grids = Setting.TempProfile.NumofCol * Setting.TempProfile.NumofRow;
            bitmapPresenter.Sort(order);
            bitmapPresenter.FillFileInfoVacancyWithDummy(grids);
            ChangeCurrentImageIndex(0);
        }

        public List<TileContainer> GetTileContainersInCurrentOrder()
        {
            List<TileContainer> containersInCurrentOrder;
            switch (Setting.TempProfile.SlideDirection)
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
    }
}
