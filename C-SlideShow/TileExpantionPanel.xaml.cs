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
using System.Windows.Media.Animation;

using System.IO;
using System.Diagnostics;
using WpfAnimatedGif;
using C_SlideShow.Core;
using System.Threading;

namespace C_SlideShow
{
    /// <summary>
    /// TileExpantionPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class TileExpantionPanel : UserControl
    {
        private Storyboard  storyboard;
        private double      zoomFactor = 1.0;
        private Point       lastZoomedPos; // 最後に拡大縮小をした後の位置(ExpandedBorder上の座標系)

        public MainWindow       MainWindow              { private get; set; }
        public bool             ExpandedDuringPlay      { get; set; } = false;
        public bool             IsShowing               { get; private set; } = false;
        public bool             IsAnimationCompleted    { get; private set; } = true;

        public Border           TargetBorder            { get; private set; }
        public ImageFileContext TargetImgFileContext    { get; private set; }
        public ImgContainer     ParentContainer         { get; private set; }

        public CancellationTokenSource Cts { get; private set; }    // Taskのキャンセルを管理


        public TileExpantionPanel()
        {
            InitializeComponent();

            this.FileInfoTextBlock.MouseLeftButtonDown += (s, e) =>
            {
                //MainWindow.Setting.ShowFileInfoInTileExpantionPanel = false;
                //UpdateFileInfoAreaVisiblity();
                //e.Handled = true;
            };
        }

        public async Task Show(Border border)
        {
            // まだ拡大パネルを閉じるアニメーション中
            if( !IsAnimationCompleted ) return;

            // ターゲット
            this.TargetBorder = border;
            ParentContainer = WpfTreeUtil.FindAncestor<ImgContainer>(border);
            if( ParentContainer == null ) return;
            int idx = ParentContainer.MainGrid.Children.IndexOf(border);
            this.TargetImgFileContext = ParentContainer.ImageFileContextMapList[idx];

            // 設定プロファイル
            Profile pf = MainWindow.Setting.TempProfile;

            // ダミーをクリックした時
            if( TargetImgFileContext.IsDummy ) return;

            // 再生中だったら、レジュームの準備
            if( MainWindow.IsPlaying )
            {
                MainWindow.ImgContainerManager.StopSlideShow(false);
                ExpandedDuringPlay = true;
            }
            else
            {
                ExpandedDuringPlay = false;
            }

            // タイル画像をコピー
            ExpandedImage.Source = TargetImgFileContext.BitmapImage;
            if( ExpandedImage.Source == null ) return;

            // ボーダー色、背景色
            ExpandedBorder.BorderBrush = new SolidColorBrush(pf.GridLineColor.Value);
            if( pf.UsePlaidBackground.Value )
            {
                ExpandedBorder.Background = Util.CreatePlaidBrush(
                    pf.BaseGridBackgroundColor.Value, pf.PairColorOfPlaidBackground.Value);
            }
            else
                ExpandedBorder.Background = new SolidColorBrush(pf.BaseGridBackgroundColor.Value);

            // 表示
            this.Visibility = Visibility.Visible;

            // 表示済みフラグ
            IsShowing = true;

            // ファイル情報のテキストを更新
            UpdateFileInfoText();

            // ファイルパスのテキストを更新
            //UpdateFilePathText();

            // 外部プログラムで開くボタンのツールチップ
            Toolbar_OpenByExternalApp.ToolTip = "規定のプログラムで画像を開く";
            if(MainWindow.Setting.ExternalAppInfoList.Count > 0 )
            {
                string appName = MainWindow.Setting.ExternalAppInfoList[0].GetAppName();
                if(appName != null )
                {
                    Toolbar_OpenByExternalApp.ToolTip = appName + "で画像を開く";
                }
            }

            // 現在のTileContainerの拡大率
            double containerScale = MainWindow.MainContent.LayoutTransform.Value.M11;

            // タイルの矩形を取得し位置を合わせる
            Rect rc = GetTileRect();
            this.Width = rc.Width;
            this.Height = rc.Height;
            this.Margin = new Thickness(rc.Left, rc.Top, 0, 0);

            // タイル拡大パネルの枠の太さ(拡大前)
            ExpandedBorder.BorderThickness = 
                new Thickness(pf.TilePadding.Value * containerScale);

            #region アニメーション準備
            // 拡大アニメーション(パネル自体)
            // --------------------------------------------------------
            storyboard = new Storyboard();
            double duration = 0.2;

            var a1 = new ThicknessAnimation(); // Margin
            Storyboard.SetTarget(a1, this);
            Storyboard.SetTargetProperty(a1, new PropertyPath(TileExpantionPanel.MarginProperty));
            Point mcMargin = new Point( // メインコンテンツのマージン
                MainWindow.MainContent.Margin.Left, MainWindow.MainContent.Margin.Top);
            a1.From = this.Margin;
            a1.To = new Thickness(0 + mcMargin.X, 0 + mcMargin.Y, 0, 0);
            a1.Duration = TimeSpan.FromSeconds(duration);
            storyboard.Children.Add(a1);

            var a2 = new DoubleAnimation(); // Width
            Storyboard.SetTarget(a2, this);
            Storyboard.SetTargetProperty(a2, new PropertyPath(TileExpantionPanel.WidthProperty));
            a2.From = this.Width;
            a2.To = MainWindow.Width - mcMargin.X * 2;
            a2.Duration = TimeSpan.FromSeconds(duration);
            storyboard.Children.Add(a2);

            var a3 = new DoubleAnimation(); // Height
            Storyboard.SetTarget(a3, this);
            Storyboard.SetTargetProperty(a3, new PropertyPath(TileExpantionPanel.HeightProperty));
            a3.From = this.Height;
            a3.To = MainWindow.Height - mcMargin.Y * 2;
            a3.Duration = TimeSpan.FromSeconds(duration);
            storyboard.Children.Add(a3);


            // 拡大アニメーション(枠の太さ)
            // --------------------------------------------------------

            // タイル拡大パネルの拡大率
            double panelScale = pf.NumofMatrix.Col;
            if( pf.NumofMatrix.Col > pf.NumofMatrix.Row ) panelScale = pf.NumofMatrix.Row;

            var a4 = new ThicknessAnimation(); // BorderThickness
            Storyboard.SetTarget(a4, this.ExpandedBorder);
            Storyboard.SetTargetProperty(a4, new PropertyPath(Border.BorderThicknessProperty));
            a4.From = ExpandedBorder.BorderThickness;
            a4.To = new Thickness( ExpandedBorder.BorderThickness.Left * panelScale );
            a4.Duration = TimeSpan.FromSeconds(duration);
            storyboard.Children.Add(a4);

            storyboard.Completed += (s, e) =>
            {
                // コンテナを隠す
                MainWindow.ImgContainerManager.Hide();

                // ファイル情報表示
                UpdateFileInfoAreaVisiblity();

                IsAnimationCompleted = true;
            };
            #endregion

            // アニメーションを開始
            IsAnimationCompleted = false;
            storyboard.Begin();

            // アニメーションしながら、大きいサイズの画像をロード
            await LoadImage();
        }

        public void Hide()
        {
            // まだ表示されていない
            if( !IsShowing || !IsAnimationCompleted ) return;

            // まだ読み込み中のTaskがあるならキャンセル
            Cts?.Cancel();

            // ファイル情報を隠す
            this.FileInfoGrid.Visibility = Visibility.Hidden;
            IsShowing = false;

            // ズーム解除
            ResetZoomAndMove();

            // タイルの矩形を取得
            Rect rc = GetTileRect();

            // 現在のTileContainerの拡大率
            double zoomFactor = MainWindow.MainContent.LayoutTransform.Value.M11;

            // 縮小アニメーション(パネル自体)
            // --------------------------------------------------------
            storyboard = new Storyboard();
            double duration = 0.2;

            var a1 = new ThicknessAnimation(); // Margin
            Storyboard.SetTarget(a1, this);
            Storyboard.SetTargetProperty(a1, new PropertyPath(TileExpantionPanel.MarginProperty));
            double frameThickness = MainWindow.MainContent.Margin.Left; // メインウインドウ枠
            a1.From = this.Margin;
            a1.To = new Thickness(rc.Left, rc.Top, 0, 0);
            a1.Duration = TimeSpan.FromSeconds(duration);
            storyboard.Children.Add(a1);

            var a2 = new DoubleAnimation(); // Width
            Storyboard.SetTarget(a2, this);
            Storyboard.SetTargetProperty(a2, new PropertyPath(TileExpantionPanel.WidthProperty));
            a2.From = this.Width;
            a2.To = rc.Width;
            a2.Duration = TimeSpan.FromSeconds(duration);
            storyboard.Children.Add(a2);

            var a3 = new DoubleAnimation(); // Height
            Storyboard.SetTarget(a3, this);
            Storyboard.SetTargetProperty(a3, new PropertyPath(TileExpantionPanel.HeightProperty));
            a3.From = this.Height;
            a3.To = rc.Height;
            a3.Duration = TimeSpan.FromSeconds(duration);
            storyboard.Children.Add(a3);


            // 縮小アニメーション(枠の太さ)
            // --------------------------------------------------------

            // 現在のTileContainerの拡大率
            double containerScale = MainWindow.MainContent.LayoutTransform.Value.M11;

            // タイル拡大パネルの枠の太さ(縮小後)
            Thickness destThickness = 
                new Thickness(MainWindow.Setting.TempProfile.TilePadding.Value * containerScale);

            var a4 = new ThicknessAnimation(); // BorderThickness
            Storyboard.SetTarget(a4, this.ExpandedBorder);
            Storyboard.SetTargetProperty(a4, new PropertyPath(Border.BorderThicknessProperty));
            a4.From = new Thickness( ExpandedBorder.BorderThickness.Left );
            a4.To = destThickness;
            a4.Duration = TimeSpan.FromSeconds(duration);
            storyboard.Children.Add(a4);


            storyboard.Completed += (s, e) =>
            {
                storyboard.Remove();
                //Margin = new Thickness(Margin.Left, Margin.Top, 0, 0);
                this.Visibility = Visibility.Hidden;
                if( ExpandedDuringPlay ) MainWindow.ImgContainerManager.StartSlideShow(false);
                IsAnimationCompleted = true;
            };

            // コンテナを表示
            MainWindow.ImgContainerManager.Show();

            // アニメーションを開始
            storyboard.Begin();
            IsAnimationCompleted = false;
        }


        private async Task LoadImage()
        {
            ImageFileInfo fi = TargetImgFileContext.Info;

            // gif拡大時
            if(TargetImgFileContext.Archiver.CanReadFile && Path.GetExtension( TargetImgFileContext.FilePath ).ToLower() == ".gif" )
            {
                var source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.CreateOptions = BitmapCreateOptions.None;
                source.UriSource = new Uri(TargetImgFileContext.FilePath);
                ImageBehavior.SetAnimatedSource(ExpandedImage, source);
                ExpandedImage.Source = source;
                source.EndInit();
                source.Freeze();
            }
            else
            {
                ImageBehavior.SetAnimatedSource(ExpandedImage, null);
                int pixel = MainWindow.Setting.TempProfile.BitmapDecodeTotalPixel.Value;

                // ウインドウサイズに合わせたBitmapをロード
                double p = MainWindow.MainContent.LayoutTransform.Value.M11;
                Size winSize = new Size(MainWindow.ImgContainerManager.ContainerWidth * p, MainWindow.ImgContainerManager.ContainerHeight * p);
                if(winSize.Width < fi.PixelSize.Width && winSize.Height < fi.PixelSize.Height ) // 本来の画像サイズがウインドウサイズより大きい時のみロード
                {
                    var cts1 = new CancellationTokenSource();
                    this.Cts = cts1;
                    var bitmap = await TargetImgFileContext.GetImage(winSize);
                    if( cts1.Token.IsCancellationRequested ) return;
                    this.ExpandedImage.Source = bitmap;
                }

                // 本来のサイズでBitmapをロード
                var cts2 = new CancellationTokenSource();
                this.Cts = cts2;
                var trueBitmap = await TargetImgFileContext.GetImage(Size.Empty);
                if( cts2.Token.IsCancellationRequested ) return;
                this.ExpandedImage.Source = trueBitmap;
            }

        }

        private void UpdateFileInfoText()
        {
            // 初期化
            FileInfoGrid.Width  = double.NaN;
            FileInfoGrid.Height = double.NaN;
            string newText = "";

            try
            {
                ImageFileInfo ifi = TargetImgFileContext.Info;

                // ファイルサイズ
                long length = 0;
                if( ifi.Length != 0 )
                {
                    length = ifi.Length;
                }
                else if(TargetImgFileContext.Archiver.CanReadFile)
                {
                    FileInfo fi = new FileInfo(TargetImgFileContext.FilePath);
                    length = fi.Length;
                }

                // 更新日時取得
                if( ifi.LastWriteTime == null ) TargetImgFileContext.ReadLastWriteTime();

                newText += "ファイル名: " + Path.GetFileName(TargetImgFileContext.FilePath) + "\n";
                newText += "画像サイズ: " + length / 1024 + "KB\n"; // 取得エラーの場合は0KBと表示される
                if(ifi.LastWriteTime != null)
                    newText += "更新日時: " + ifi.LastWriteTime.Value.DateTime + "\n";
                if( ifi.ExifInfo.DateTaken != null )
                    newText += "撮影日時: " + ifi.ExifInfo.DateTaken.Value.DateTime + "\n";
                newText += "ピクセル数: " + ifi.PixelSize.Width + "x" + ifi.PixelSize.Height;

                // ツールチップでファイルパス表示
                if( TargetImgFileContext.Archiver.CanReadFile )
                {
                    FileInfoGrid.ToolTip = TargetImgFileContext.FilePath;
                }
                else
                {
                    FileInfoGrid.ToolTip = TargetImgFileContext.Archiver.ArchiverPath + "\\" + TargetImgFileContext.FilePath;
                }
            }
            catch
            {
                newText = "ファイル情報の取得失敗";
            }
            finally
            {
                FileInfoTextBlock.Text = newText;

                // FileInfoGridのサイズをFileInfoTextBlockに合わせる
                FileInfoTextBlock.Dispatcher.BeginInvoke(
                    new Action( () =>
                    {
                        FileInfoGrid.Width = FileInfoTextBlock.RenderSize.Width;
                        FileInfoGrid.Height = FileInfoTextBlock.RenderSize.Height;
                    }),
                    System.Windows.Threading.DispatcherPriority.Loaded
                );
            }

        }


        //private void UpdateFilePathText()
        //{
        //    if( TargetImgFileContext.Archiver.CanReadFile )
        //    {
        //        FilePathTextBlock.Text = TargetImgFileContext.FilePath;
        //    }
        //    else
        //    {
        //        FilePathTextBlock.Text = TargetImgFileContext.Archiver.ArchiverPath + "\\" + TargetImgFileContext.FilePath;
        //    }
        //}

        public void UpdateFileInfoAreaVisiblity()
        {
            if( MainWindow.Setting.ShowFileInfoInTileExpantionPanel )
            {
                this.FileInfoGrid.Visibility = Visibility.Visible;
                //this.FilePathGrid.Visibility = Visibility.Visible;
            }
            else
            {
                this.FileInfoGrid.Visibility = Visibility.Hidden;
                //this.FilePathGrid.Visibility = Visibility.Hidden;
            }
        }

        public void FitToMainWindow()
        {
            storyboard.Remove();
            Profile pf = MainWindow.Setting.TempProfile;

            // 拡大と移動をリセット
            ResetZoomAndMove();

            // 拡大パネルをMainWindowに合わせる
            double mwFrameThickness = MainWindow.MainContent.Margin.Left; // メインウインドウ枠
            Margin = new Thickness(0 + mwFrameThickness, 0 + mwFrameThickness, 0, 0);
            Width = MainWindow.ActualWidth - mwFrameThickness * 2;;
            Height = MainWindow.ActualHeight - mwFrameThickness * 2;

            // 枠の太さ更新
            UpdateBorderThickness();
        }

        public void FitToFullScreenWindow()
        {
            storyboard.Remove();
            Profile pf = MainWindow.Setting.TempProfile;

            // 拡大と移動をリセット
            ResetZoomAndMove();

            // 位置とサイズ
            Margin = new Thickness(MainWindow.MainContent.Margin.Left, MainWindow.MainContent.Margin.Top, 0, 0);
            double containerScale = MainWindow.MainContent.LayoutTransform.Value.M11;
            Width  = MainWindow.ImgContainerManager.CurrentContainer.Width * containerScale;
            Height = MainWindow.ImgContainerManager.CurrentContainer.Height * containerScale;

            // 枠の太さ更新
            UpdateBorderThickness();
        }

        private void UpdateBorderThickness()
        {
            Profile pf = MainWindow.Setting.TempProfile;

            // タイル拡大パネルの枠の太さ(拡大前)
            double containerScale = MainWindow.MainContent.LayoutTransform.Value.M11;
            double srcThicknessVal = pf.TilePadding.Value * containerScale;

            // 現在のウインドウサイズに合わせて枠の太さを拡大
            double panelScale = pf.NumofMatrix.Col;
            if( pf.NumofMatrix.Col > pf.NumofMatrix.Row ) panelScale = pf.NumofMatrix.Row;
            this.ExpandedBorder.BorderThickness = new Thickness(srcThicknessVal * panelScale);
        }

        /// <summary>
        /// ターゲットとなるタイルの矩形を取得(MainWindowからの座標)
        /// </summary>
        /// <returns></returns>
        private Rect GetTileRect()
        {
            Rect rect = new Rect();
            if( TargetBorder == null ) return rect;

            // 大きさ
            double zoomFactor = MainWindow.MainContent.LayoutTransform.Value.M11;
            rect.Width  = TargetBorder.RenderSize.Width + TargetBorder.Margin.Left * 2;
            rect.Height = TargetBorder.RenderSize.Height + TargetBorder.Margin.Top * 2;
            rect.Width  *= zoomFactor;
            rect.Height *= zoomFactor;

            // 位置
            var parent = TargetBorder.Parent as UIElement;
            var location = TargetBorder.TranslatePoint(new Point(0, 0), parent);
            location.X -= TargetBorder.Margin.Left;
            location.Y -= TargetBorder.Margin.Top;
            location.X += ParentContainer.Margin.Left; // コンテナのスライド量を反映
            location.Y += ParentContainer.Margin.Top;
            location.X *= zoomFactor;
            location.Y *= zoomFactor;
            location.X += MainWindow.MainContent.Margin.Left; // メインウインドウ枠の分
            location.Y += MainWindow.MainContent.Margin.Top;

            rect.X = location.X;
            rect.Y = location.Y;

            return rect;
        }


        public void Zoom(double vari)
        {
            double zoomFactorPrev = zoomFactor;
            zoomFactor += vari;

            // カーソル位置取得(ExpandedBorder上の座標系)
            Point pos = Mouse.GetPosition(ExpandedBorder);

            // カーソル位置はみ出しチェック
            Point posOfPanel = Mouse.GetPosition(this);
            if(posOfPanel.X < 0 || posOfPanel.X > this.ActualWidth || posOfPanel.Y < 0 || posOfPanel.Y > this.ActualHeight )
            {
                if( zoomFactorPrev == 1.0 ) pos = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
                else pos = new Point(lastZoomedPos.X, lastZoomedPos.Y);
            }

            // 拡大
            if(zoomFactor > 1.0 )
            {
                //ExpandedBorder.RenderTransform = new ScaleTransform(zoomFactor, zoomFactor);
                ExpandedBorder.Width  = this.ActualWidth  * zoomFactor;
                ExpandedBorder.Height = this.ActualHeight * zoomFactor;
            }
            else
            {
                ResetZoomAndMove();
                return;
            }

            // 拡大に依る移動量算出
            Point move = new Point();
            move.X = pos.X * (zoomFactor / zoomFactorPrev) - pos.X;
            move.Y = pos.Y * (zoomFactor / zoomFactorPrev) - pos.Y;

            // 移動した分だけ、引き戻す
            if(zoomFactor > 1.0 )
            {
                double left = ExpandedBorder.Margin.Left - move.X;
                double top  = ExpandedBorder.Margin.Top  - move.Y;
                ExpandedBorder.Margin = new Thickness( left, top, 0, 0);
                lastZoomedPos = new Point(pos.X + move.X, pos.Y + move.Y);
            }

            Debug.WriteLine("pos: " + pos.ToString() );
            Debug.WriteLine("move: " + move.ToString() );
            Debug.WriteLine("this Width: " + this.ActualWidth );
            Debug.WriteLine("this Height: " + this.ActualHeight );
            Debug.WriteLine("Window Width: " + MainWindow.Width );
            Debug.WriteLine("Window Height: " + MainWindow.Height );
            Debug.WriteLine("ExpandedBorder Width: " + ExpandedBorder.ActualWidth );
            Debug.WriteLine("ExpandedBorder Height: " + ExpandedBorder.ActualHeight );
            Debug.WriteLine("ExpandedBorder Left: " + ExpandedBorder.Margin.Left );
            Debug.WriteLine("ExpandedBorder Top: " + ExpandedBorder.Margin.Top );
            Debug.WriteLine("--------------------------------------------------------------------------------------------------------");
        }

        public void ZoomIn(double param)
        {
            Zoom(param);
        }

        public void ZoomOut(double param)
        {
            Zoom(- param);
        }

        public void ResetZoomAndMove()
        {
            zoomFactor = 1.0;
            ExpandedBorder.Width = double.NaN;
            ExpandedBorder.Height = double.NaN;
            ExpandedBorder.Margin = new Thickness( 0, 0, 0, 0);
        }

        public void Move(int x, int y)
        {
            var m = ExpandedBorder.Margin;

            ExpandedBorder.Width  = ExpandedBorder.ActualWidth;
            ExpandedBorder.Height = ExpandedBorder.ActualHeight;
            ExpandedBorder.Margin = new Thickness(m.Left + x, m.Top + y, m.Right, m.Bottom);
        }

        /* ---------------------------------------------------- */
        //     ツールバー
        /* ---------------------------------------------------- */
        // ファイル情報を表示
        private void Toolbar_ShowFileInfo_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Setting.ShowFileInfoInTileExpantionPanel = !MainWindow.Setting.ShowFileInfoInTileExpantionPanel;
            UpdateFileInfoAreaVisiblity();
        }

        // エクスプローラーで開く
        private void Toolbar_OpenExplorer_Click(object sender, RoutedEventArgs e)
        {
            TargetImgFileContext.OpenByExplorer();
        }

        // クリップボードへコピー
        private void MenuItem_Copy_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem_Copy.Items.Clear();

            // ファイル
            MenuItem mi_file = new MenuItem();
            mi_file.Header = Path.GetFileName("ファイル");
            mi_file.ToolTip = Path.GetFileName("コピー後、エクスプローラーで貼り付けが出来ます");
            mi_file.Click += (s, ev) => { TargetImgFileContext.CopyFile(); };
            MenuItem_Copy.Items.Add(mi_file);

            // 画像データ
            MenuItem mi_image = new MenuItem();
            mi_image.Header = Path.GetFileName("画像データ");
            mi_image.ToolTip = Path.GetFileName("コピー後、ペイント等の画像編集ソフトへ貼り付けが出来ます");
            mi_image.Click += (s, ev) => { var t = TargetImgFileContext.CopyImageData(); };
            MenuItem_Copy.Items.Add(mi_image);

            // ファイルパス
            string filePath;
            if( TargetImgFileContext.Archiver.CanReadFile ) filePath = TargetImgFileContext.FilePath;
            else filePath = TargetImgFileContext.Archiver.ArchiverPath;
            MenuItem mi_filePath = new MenuItem();
            mi_filePath.Header = Path.GetFileName("ファイルパス");
            mi_filePath.ToolTip = filePath;
            mi_filePath.Click += (s, ev) => { TargetImgFileContext.CopyFilePath(); };
            MenuItem_Copy.Items.Add(mi_filePath);

            // ファイル名
            MenuItem mi_fileName = new MenuItem();
            mi_fileName.Header = "ファイル名";
            mi_fileName.ToolTip = Path.GetFileName( TargetImgFileContext.FilePath );
            mi_fileName.Click += (s, ev) => { TargetImgFileContext.CopyFileName(); };
            MenuItem_Copy.Items.Add(mi_fileName);

        }

        // 外部プログラムで画像を開く
        private void Toolbar_OpenByExternalApp_Click(object sender, RoutedEventArgs e)
        {
            if(MainWindow.Setting.ExternalAppInfoList.Count > 0 )
            {
                TargetImgFileContext.OpenByExternalApp(MainWindow.Setting.ExternalAppInfoList[0]);
            }
            else
            {
                TargetImgFileContext.OpenByExternalApp( new ExternalAppInfo() );
            }
        }

        private void ToolbarButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key inputKey;

            if (e.ImeProcessedKey != Key.None) { inputKey = e.ImeProcessedKey; }
            else if(e.Key == Key.System ) { inputKey = e.SystemKey; }
            else { inputKey = e.Key; }

            this.RaiseEvent(new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, inputKey) { RoutedEvent = Keyboard.KeyDownEvent });
            e.Handled = true;
        }


        // end of class
    }
}
