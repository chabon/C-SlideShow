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


namespace C_SlideShow
{
    /// <summary>
    /// TileExpantionPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class TileExpantionPanel : UserControl
    {
        private Tile targetTile;
        private Storyboard storyboard;

        public MainWindow MainWindow { private get; set; }
        public BitmapPresenter BitmapPresenter { private get; set; }
        public bool ExpandedDuringPlay { get; set; } = false;
        public bool IsShowing { get; private set; } = false;


        public TileExpantionPanel()
        {
            InitializeComponent();

            this.MouseRightButtonUp += (s, e) =>
            {
                Hide();
            };

            this.FileInfoTextBlock.MouseLeftButtonDown += (s, e) =>
            {
                MainWindow.Setting.ShowFileInfoInTileExpantionPanel = false;
                ShowFileInfoOrButton();
            };

            this.FileInfoDisplayButton.Click += (s, e) =>
            {
                MainWindow.Setting.ShowFileInfoInTileExpantionPanel = true;
                ShowFileInfoOrButton();
            };
        }

        public void Show(Tile tile)
        {
            // 設定プロファイル
            Profile pf = MainWindow.Setting.TempProfile;

            // ダミーをクリックした時
            if( tile.FilePath == BitmapPresenter.DummyFilePath ) return;

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
            ExpandedBorder.BorderBrush = new SolidColorBrush(pf.GridLineColor);
            if( pf.UsePlaidBackground )
            {
                ExpandedBorder.Background = Util.CreatePlaidBrush(
                    pf.BaseGridBackgroundColor, pf.PairColorOfPlaidBackground);
            }
            else
                ExpandedBorder.Background = new SolidColorBrush(pf.BaseGridBackgroundColor);

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
                new Thickness(pf.TilePadding * containerScale);

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
            double panelScale = pf.NumofCol;
            if( pf.NumofCol > pf.NumofRow ) panelScale = pf.NumofRow;

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
            };

            // アニメーションを開始
            storyboard.Begin();
        }

        public void Hide()
        {
            // ファイル情報を隠す
            this.FileInfoGrid.Visibility = Visibility.Hidden;
            this.FileInfoDisplayButton.Visibility = Visibility.Hidden;
            IsShowing = false;

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
                new Thickness(MainWindow.Setting.TempProfile.TilePadding * containerScale);

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
            int pxcelWidth = MainWindow.Setting.TempProfile.BitmapDecodeTotalPixelWidth;
            var bitmap = BitmapPresenter.LoadBitmap(targetTile.FilePath, pxcelWidth);
            if(bitmap != null)
                this.ExpandedImage.Source = bitmap;
            else
                this.ExpandedImage.Source = null;
        }

        private void UpdateFileInfoText()
        {
            string newText = "";

            try
            {
#if DEBUG
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
#endif
                ImageFileInfo ifi =
                    BitmapPresenter.ImgFileInfo.First(i => i.FilePath == targetTile.FilePath);

                // ファイルサイズ
                long length = 0;
                if( ifi.Length != 0 )
                {
                    length = ifi.Length;
                }
                else if(BitmapPresenter.ReadType == BitmapReadType.File )
                {
                    FileInfo fi = new FileInfo(targetTile.FilePath);
                    length = fi.Length;
                }

                // 更新日時
                DateTimeOffset lastWriteTime;
                DateTimeOffset dateDefault = new DateTimeOffset();
                if(ifi.LastWriteTime.CompareTo(dateDefault) == 0 // 更新日時情報がない場合
                    && BitmapPresenter.ReadType == BitmapReadType.File )
                    lastWriteTime = File.GetLastWriteTime(targetTile.FilePath);
                else
                    lastWriteTime = ifi.LastWriteTime;

                // ピクセル数
                //FileStream fs = new FileStream(targetTile.FilePath, FileMode.Open, FileAccess.Read);
                //int imagew = System.Drawing.Image.FromStream(fs).Width;
                //int imageh = System.Drawing.Image.FromStream(fs).Height;
                //fs.Close();
                //BitmapSource bmp = (BitmapSource)this.ExpandedImage.Source;

                // test
                Size pxSize = ifi.PixelSize;

                // (todo)撮影日時


                newText += "ファイル名: " + Path.GetFileName(targetTile.FilePath) + "\n";
                newText += "画像サイズ: " + length / 1024 + "KB\n";
                newText += "更新日時: " + lastWriteTime.DateTime + "\n";
                //newText += "撮影日時: " + lastWriteTime.DateTime + "\n";
                //newText += "ピクセル数: " + imagew + "x" + imageh;
                newText += "ピクセル数: " + pxSize.Width + "x" + pxSize.Height;
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


        public void ShowFileInfoOrButton()
        {
            if( MainWindow.Setting.ShowFileInfoInTileExpantionPanel )
            {
                this.FileInfoGrid.Visibility = Visibility.Visible;
                this.FileInfoDisplayButton.Visibility = Visibility.Hidden;
            }
            else
            {
                this.FileInfoGrid.Visibility = Visibility.Hidden;
                this.FileInfoDisplayButton.Visibility = Visibility.Visible;
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
            double srcThicknessVal = pf.TilePadding * containerScale;

            // 現在のウインドウサイズに合わせて枠の太さを拡大
            double panelScale = pf.NumofCol;
            if( pf.NumofCol > pf.NumofRow ) panelScale = pf.NumofRow;
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
    }
}
