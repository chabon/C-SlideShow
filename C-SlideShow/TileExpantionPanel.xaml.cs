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


namespace C_SlideShow
{
    /// <summary>
    /// TileExpantionPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class TileExpantionPanel : UserControl
    {
        private Tile targetTile;
        private Storyboard storyboard;
        private double zoomFactor = 1.0;
        private Point lastZoomedPos; // 最後に拡大縮小をした後の位置(ExpandedBorder上の座標系)

        public MainWindow MainWindow { private get; set; }
        public ImageFileManager ImageFileManager { private get; set; }
        public bool ExpandedDuringPlay { get; set; } = false;
        public bool IsShowing { get; private set; } = false;


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

        public void Show(Tile tile)
        {
            // 設定プロファイル
            Profile pf = MainWindow.Setting.TempProfile;

            // ダミーをクリックした時
            if( tile.ImageFileInfo.IsDummy ) return;

            // 再生中だったら、レジュームの準備
            if( MainWindow.IsPlaying )
            {
                MainWindow.StopSlideShow();
                ExpandedDuringPlay = true;
            }
            else
            {
                ExpandedDuringPlay = false;
            }

            // ターゲットとなるタイルを保持
            this.targetTile = tile;

            // 画像をロード
            LoadImage();
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
                MainWindow.TileContainer1.Visibility = Visibility.Hidden;
                MainWindow.TileContainer2.Visibility = Visibility.Hidden;
                MainWindow.TileContainer3.Visibility = Visibility.Hidden;

                // ファイル情報表示
                UpdateFileInfoAreaVisiblity();
            };

            // アニメーションを開始
            storyboard.Begin();
        }

        public void Hide()
        {
            // ファイル情報を隠す
            this.FileInfoGrid.Visibility = Visibility.Hidden;
            IsShowing = false;

            // ズーム解除
            ZoomReset();

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
                if( ExpandedDuringPlay ) MainWindow.StartSlideShow();
            };

            // コンテナを表示
            MainWindow.TileContainer1.Visibility = Visibility.Visible;
            MainWindow.TileContainer2.Visibility = Visibility.Visible;
            MainWindow.TileContainer3.Visibility = Visibility.Visible;

            // アニメーションを開始
            storyboard.Begin();
        }


        private void LoadImage()
        {
            ImageFileInfo fi = targetTile.ImageFileInfo;

            // gif拡大時
            if(fi.Archiver.CanReadFile && Path.GetExtension( fi.FilePath ).ToLower() == ".gif" )
            {
                var source = new BitmapImage();
                source.BeginInit();
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.CreateOptions = BitmapCreateOptions.None;
                source.UriSource = new Uri(fi.FilePath);
                ImageBehavior.SetAnimatedSource(ExpandedImage, source);
                ExpandedImage.Source = source;
                source.EndInit();
                source.Freeze();
            }
            else
            {
                ImageBehavior.SetAnimatedSource(ExpandedImage, null);
                int pixel = MainWindow.Setting.TempProfile.BitmapDecodeTotalPixel.Value;
                Size pixelSize = new Size(pixel, pixel);
                var bitmap = ImageFileManager.LoadBitmap( fi, pixelSize );

                if(bitmap != null) this.ExpandedImage.Source = bitmap;
                else this.ExpandedImage.Source = null;
            }

        }

        private void UpdateFileInfoText()
        {
            string newText = "";

            try
            {
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif
                ImageFileInfo ifi =
                    ImageFileManager.ImgFileInfo.First(i => i.FilePath == targetTile.ImageFileInfo.FilePath);

                // ファイルサイズ
                long length = 0;
                if( ifi.Length != 0 )
                {
                    length = ifi.Length;
                }
                else
                {
                    FileInfo fi = new FileInfo(targetTile.ImageFileInfo.FilePath);
                    length = fi.Length;
                }

                // 更新日時
                if( ifi.LastWriteTime == null ) // 更新日時情報がない場合取得
                    ifi.ReadLastWriteTime();

                // (todo)撮影日時


                newText += "ファイル名: " + Path.GetFileName(targetTile.ImageFileInfo.FilePath) + "\n";
                newText += "画像サイズ: " + length / 1024 + "KB\n";
                if(ifi.LastWriteTime != null)
                    newText += "更新日時: " + ifi.LastWriteTime.Value.DateTime + "\n";
                newText += "ピクセル数: " + ifi.PixelSize.Width + "x" + ifi.PixelSize.Height;
#if DEBUG
                sw.Stop();
                Debug.WriteLine("-----------------------------------------------------");
                Debug.WriteLine(" expanded image info loaded  time: " + sw.Elapsed);
                Debug.WriteLine("-----------------------------------------------------");
#endif
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


        public void UpdateFileInfoAreaVisiblity()
        {
            if( MainWindow.Setting.ShowFileInfoInTileExpantionPanel )
            {
                this.FileInfoGrid.Visibility = Visibility.Visible;
            }
            else
            {
                this.FileInfoGrid.Visibility = Visibility.Hidden;
            }
        }

        public void FitToMainWindow()
        {
            storyboard.Remove();
            Profile pf = MainWindow.Setting.TempProfile;

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

            // 位置とサイズ
            Margin = new Thickness(MainWindow.MainContent.Margin.Left, MainWindow.MainContent.Margin.Top, 0, 0);
            double containerScale = MainWindow.MainContent.LayoutTransform.Value.M11;
            Width = MainWindow.TileContainer1.ActualWidth * containerScale;
            Height = MainWindow.TileContainer1.ActualHeight * containerScale;

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
            if( targetTile == null ) return rect;

            // 大きさ
            double zoomFactor = MainWindow.MainContent.LayoutTransform.Value.M11;
            rect.Width = targetTile.Border.RenderSize.Width + targetTile.Border.Margin.Left * 2;
            rect.Height = targetTile.Border.RenderSize.Height + targetTile.Border.Margin.Top * 2;
            rect.Width *= zoomFactor;
            rect.Height *= zoomFactor;

            // 位置
            var parent = targetTile.Border.Parent as UIElement;
            var location = targetTile.Border.TranslatePoint(new Point(0, 0), parent);
            location.X -= targetTile.Border.Margin.Left;
            location.Y -= targetTile.Border.Margin.Top;
            location.X += targetTile.ParentConteiner.Margin.Left; // コンテナのスライド量を反映
            location.Y += targetTile.ParentConteiner.Margin.Top;
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
                ExpandedBorder.Width = this.ActualWidth * zoomFactor;
                ExpandedBorder.Height = this.ActualHeight * zoomFactor;
            }
            else
            {
                //ExpandedBorder.RenderTransform = new ScaleTransform(1.0, 1.0);
                ExpandedBorder.Width = double.NaN;
                ExpandedBorder.Height = double.NaN;
            }

            // 拡大に依る移動量算出
            Point move = new Point();
            move.X = pos.X * (zoomFactor / zoomFactorPrev) - pos.X;
            move.Y = pos.Y * (zoomFactor / zoomFactorPrev) - pos.Y;

            // 移動した分だけ、引き戻す(拡大率が1.0になったら位置リセット)
            if(zoomFactor > 1.0 )
            {
                ExpandedBorder.Margin = new Thickness(
                    ExpandedBorder.Margin.Left - move.X, ExpandedBorder.Margin.Top - move.Y, 0, 0);
                lastZoomedPos = new Point(pos.X + move.X, pos.Y + move.Y);
            }
            else
            {
                zoomFactor = 1.0;
                ExpandedBorder.Margin = new Thickness( 0, 0, 0, 0);
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

        public void ZoomIn()
        {
            Zoom(0.5);
        }

        public void ZoomOut()
        {
            Zoom(-0.5);
        }

        public void ZoomReset()
        {
            if(zoomFactor != 1.0 )
            {
                zoomFactor = 1.0;
                ExpandedBorder.Width = double.NaN;
                ExpandedBorder.Height = double.NaN;
                ExpandedBorder.Margin = new Thickness( 0, 0, 0, 0);
            }
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
            string folderDirPath;
            string filePath;
            if( targetTile.ImageFileInfo.Archiver.CanReadFile )
            {
                folderDirPath = Directory.GetParent(targetTile.ImageFileInfo.FilePath).FullName;
                filePath = targetTile.ImageFileInfo.FilePath;
            }
            else
            {
                folderDirPath = Directory.GetParent(targetTile.ImageFileInfo.Archiver.ArchiverPath).FullName;
                filePath = targetTile.ImageFileInfo.Archiver.ArchiverPath;
            }
            Process.Start("explorer.exe", "/select,\"" + filePath + "\"");
            //Process.Start(folderDirPath);
        }

        // クリップボードへコピー
        private void MenuItem_Copy_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem_Copy.Items.Clear();

            ImageFileInfo fi = targetTile.ImageFileInfo;

            // ファイル(書庫内ファイルは現在不可)
            if( fi.Archiver.CanReadFile )
            {
                MenuItem mi_file = new MenuItem();
                mi_file.Header = Path.GetFileName("ファイル");
                mi_file.ToolTip = Path.GetFileName("コピー後、エクスプローラーで貼り付けが出来ます");
                mi_file.Click += (s, ev) => {
                    System.Collections.Specialized.StringCollection files = new System.Collections.Specialized.StringCollection();
                    files.Add(fi.FilePath);
                    Clipboard.SetFileDropList(files);
                };
                MenuItem_Copy.Items.Add(mi_file);
            }

            // 画像データ
            MenuItem mi_image = new MenuItem();
            mi_image.Header = Path.GetFileName("画像データ");
            mi_image.ToolTip = Path.GetFileName("コピー後、ペイント等の画像編集ソフトへ貼り付けが出来ます");
            mi_image.Click += (s, ev) => {
                BitmapSource source = ImageFileManager.LoadBitmap( fi, new Size(0, 0) );
                Clipboard.SetImage(source);
            };
            MenuItem_Copy.Items.Add(mi_image);

            // ファイルパス
            string filePath;
            if( fi.Archiver.CanReadFile ) filePath = fi.FilePath;
            else filePath = fi.Archiver.ArchiverPath;
            MenuItem mi_filePath = new MenuItem();
            mi_filePath.Header = Path.GetFileName("ファイルパス");
            mi_filePath.ToolTip = filePath;
            mi_filePath.Click += (s, ev) => { Clipboard.SetText(filePath); };
            MenuItem_Copy.Items.Add(mi_filePath);

            // ファイル名
            MenuItem mi_fileName = new MenuItem();
            mi_fileName.Header = "ファイル名";
            mi_fileName.ToolTip = Path.GetFileName( fi.FilePath );
            mi_fileName.Click += (s, ev) => { Clipboard.SetText( Path.GetFileName(fi.FilePath) ); };
            MenuItem_Copy.Items.Add(mi_fileName);

        }

        // 外部プログラムで画像を開く(書庫内ファイルは現在不可)
        private void Toolbar_OpenByExternalApp_Click(object sender, RoutedEventArgs e)
        {
            ImageFileInfo fi = targetTile.ImageFileInfo;
            if( fi.Archiver.CanReadFile )
            {
                ExternalAppInfo exAppInfo = MainWindow.Setting.ExternalAppInfoList[0];
                string filePathFormat = "$FilePath$";

                string arg = exAppInfo.Arg;
                if( arg == "" ) arg = "\"" + filePathFormat + "\"";

                if(exAppInfo.Path != "" )
                {
                    // プログラムの指定あり
                    try { Process.Start( exAppInfo.Path, arg.Replace(filePathFormat, fi.FilePath) ); }
                    catch { }
                }
                else
                {
                    // プログラムの指定がなければ、拡張子で関連付けられているプログラムで開く
                    try { Process.Start( "\"" +  fi.FilePath +"\"" ); }
                    catch { }
                }
            }
        }



        // end of class
    }
}
