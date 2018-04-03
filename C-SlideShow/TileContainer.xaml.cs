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

using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.IO;



namespace C_SlideShow
{
    /// <summary>
    /// TileWraper.xaml の相互作用ロジック
    /// </summary>
    public partial class TileContainer : UserControl
    {
        List<Tile> tiles;
        SlideDirection slideDirection;
        Point startPoint;
        Point wrapPoint; // この座標までスライドすると最初(startPoint)に戻る
        TileOrigin tileOrigin;
        TileOrientation tileOrientation;
        Storyboard storyboard;
        EventHandler onStoryboardCompleted;
        Point activeSlideEndPoint;

        public static Thread bitmapLoadThread;
        public static bool reqireThreadRelease = false;

        // プロパティ
        public MainWindow MainWindow { get; set; }
        public BitmapPresenter BitmapPresenter { get; set; }
        public TileContainer ForwardContainer { get; set; }
        public bool IsActiveSliding { get; set; }
        public bool IsContinuousSliding { get; set; }
        public bool IsHorizontalSlide
        {
            get
            {
                if (slideDirection == SlideDirection.Left || slideDirection == SlideDirection.Right)
                {
                    return true;
                }
                else return false;
            }
        }
        public bool IsVerticalSlide
        {
            get
            {
                if (slideDirection == SlideDirection.Top || slideDirection == SlideDirection.Bottom)
                {
                    return true;
                }
                else return false;
            }
        }
        public bool IsStartPoint
        {
            get
            {
                Point pt = new Point(Margin.Left, Margin.Top);
                if (pt == this.startPoint) return true;
                else return false;
            }
        }
        public int NumofGrids
        {
            get
            {
                return MainGrid.RowDefinitions.Count * MainGrid.ColumnDefinitions.Count;
            }
        }


        public TileContainer()
        {
            InitializeComponent();

            tiles = new List<Tile>();
        }

        public void InitSlideDerection(SlideDirection dir)
        {
            this.slideDirection = dir;
        }

        /// <summary>
        /// グリッド(タイル)の初期化
        /// </summary>
        /// <param name="numofRow"></param>
        /// <param name="numofCol"></param>
        public void InitGrid(int numofRow, int numofCol)
        {
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();

            for(int i=0; i< numofCol; i++)
            {
                ColumnDefinition c = new ColumnDefinition();
                MainGrid.ColumnDefinitions.Add(c);
            }

            for(int i=0; i< numofRow; i++)
            {
                RowDefinition r = new RowDefinition();
                MainGrid.RowDefinitions.Add(r);
            }

            tiles.Clear();
            for(int i=0; i < numofRow * numofCol; i++)
            {
                tiles.Add(new Tile( new Image() ));
            }
        }

        public void InitSizeAndPos(int tileWidth, int tileHeight, int index)
        {
            this.Width = tileWidth * MainGrid.ColumnDefinitions.Count;
            this.MainGrid.Width = this.Width;
            this.Height = tileHeight * MainGrid.RowDefinitions.Count;
            this.MainGrid.Height = this.Height;

            switch (slideDirection)
            {
                case SlideDirection.Left:
                    this.Margin = new Thickness(index * this.Width,0,0,0);
                    break;
                case SlideDirection.Top:
                    this.Margin = new Thickness(0, index * this.Height,0,0);
                    break;
                case SlideDirection.Right:
                    this.Margin = new Thickness(-index * this.Width,0,0,0);
                    break;
                case SlideDirection.Bottom:
                    this.Margin = new Thickness(0, -index * this.Height,0,0);
                    break;
            }
        }

        /// <summary>
        /// タイルの原点、配置方向を指定する
        /// </summary>
        /// <param name="tileOrigin">タイルの原点</param>
        /// <param name="tileOrientation">タイルを配置する方向</param>
        /// <param name="useDefaultTileOrigin">方向に依る、デフォルトのタイル原点を使用</param>
        public void InitTileOrigin(TileOrigin tileOrigin, TileOrientation tileOrientation, bool useDefaultTileOrigin)
        {
            if (useDefaultTileOrigin)
            {
                switch (slideDirection)
                {
                    case SlideDirection.Left:
                    default:
                        this.tileOrigin = TileOrigin.TopLeft;
                        this.tileOrientation = TileOrientation.Vertical;
                        break;
                    case SlideDirection.Top:
                        this.tileOrigin = TileOrigin.TopLeft;
                        this.tileOrientation = TileOrientation.Horizontal;
                        break;
                    case SlideDirection.Right:
                        this.tileOrigin = TileOrigin.TopRight;
                        this.tileOrientation = TileOrientation.Vertical;
                        break;
                    case SlideDirection.Bottom:
                        this.tileOrigin = TileOrigin.BottomRight;
                        this.tileOrientation = TileOrientation.Horizontal;
                        break;
                }
            }
            else
            {
                this.tileOrigin = tileOrigin;
                this.tileOrientation = tileOrientation;
            }
        }

        public void InitWrapPoint()
        {
            switch (slideDirection)
            {
                case SlideDirection.Left:
                    startPoint = new Point(Width * 2, 0);
                    wrapPoint = new Point(-Width, 0);
                    break;
                case SlideDirection.Top:
                    startPoint = new Point(0, Height * 2);
                    wrapPoint = new Point(0, -Height);
                    break;
                case SlideDirection.Right:
                    startPoint = new Point(-Width * 2, 0);
                    wrapPoint = new Point(Width, 0);
                    break;
                case SlideDirection.Bottom:
                    startPoint = new Point(0, -Height * 2);
                    wrapPoint = new Point(0, Height);
                    break;
            }
        }

        /// <summary>
        /// はめ込む位置を決めてから、画像を読み込む
        /// </summary>
        /// <param name="isPlayback">マウスホイール巻き戻しによるロードか</param>
        /// <param name="bAsync">非同期で読み込み</param>
        public void LoadImageToGrid(bool isPlayback, bool bAsync)
        {
            MainGrid.Children.Clear();

            TileOrigin orig = this.tileOrigin;
            TileOrientation orie = this.tileOrientation;

            if (isPlayback)
            {
                switch (orig)
                {
                    case TileOrigin.TopLeft:
                    default:
                        orig = TileOrigin.BottomRight;
                        break;
                    case TileOrigin.TopRight:
                        orig = TileOrigin.BottomLeft;
                        break;
                    case TileOrigin.BottomLeft:
                        orig = TileOrigin.TopRight;
                        break;
                    case TileOrigin.BottomRight:
                        orig = TileOrigin.TopLeft;
                        break;
                }
            }

            int rowCnt = MainGrid.RowDefinitions.Count;
            int colCnt = MainGrid.ColumnDefinitions.Count;

            int k = 0;
            switch (orig)
            {
                case TileOrigin.TopLeft:
                default:
                    if(orie == TileOrientation.Horizontal)
                    {
                        for (int i = 0; i < rowCnt; i++) for (int j = 0; j < colCnt; j++)
                            { tiles[k].SetGridPos(i, j, isPlayback); k++; }
                    }
                    else
                    {
                        for (int j = 0; j < colCnt; j++) for (int i = 0; i < rowCnt; i++)
                            { tiles[k].SetGridPos(i, j, isPlayback); k++; }
                    }
                    break;
                case TileOrigin.TopRight:
                    if(orie == TileOrientation.Horizontal)
                    {
                        for (int i = 0; i < rowCnt; i++) for (int j = colCnt -1; j >= 0; j--)
                            { tiles[k].SetGridPos(i, j, isPlayback); k++; }
                    }
                    else
                    {
                        for (int j = colCnt -1; j >= 0; j--) for (int i = 0; i < rowCnt; i++)
                            { tiles[k].SetGridPos(i, j, isPlayback); k++; }
                    }
                    break;
                case TileOrigin.BottomLeft:
                    if(orie == TileOrientation.Horizontal)
                    {
                        for (int i = rowCnt -1 ; i >= 0; i--) for (int j = 0; j < colCnt; j++)
                            { tiles[k].SetGridPos(i, j, isPlayback); k++; }
                    }
                    else
                    {
                        for (int j = 0; j < colCnt; j++) for (int i = rowCnt -1; i >= 0; i--)
                            { tiles[k].SetGridPos(i, j, isPlayback); k++; }
                    }
                    break;
                case TileOrigin.BottomRight:
                    if(orie == TileOrientation.Horizontal)
                    {
                        for (int i = rowCnt -1; i >=0; i--) for (int j = colCnt -1; j >=0; j--)
                            { tiles[k].SetGridPos(i, j, isPlayback); k++; }
                    }
                    else
                    {
                        for (int j = colCnt -1; j >=0; j--) for (int i = rowCnt -1; i >=0; i--)
                            { tiles[k].SetGridPos(i, j, isPlayback); k++; }
                    }
                    break;
            }

            //LoadTilesImage();
            //LoadTilesImageAsync();

            if (bAsync) LoadTileImageAsync();
            else LoadTileImage();
        }


        private void LoadTileImage()
        {
            tiles.ForEach(tile => {
                try
                {
                    if (BitmapPresenter.FileInfo.Count < 1) throw new Exception();

                    Image image = tile.Image;
                    MainGrid.Children.Add(image);

                    ImageFileInfo iFileInfo = BitmapPresenter.PickImageFileInfo(tile.ByPlayback);
                    tile.filePath = iFileInfo.FilePath;

                    BitmapPresenter.SlideIndex(tile.ByPlayback);
                    BitmapImage bitmap = BitmapPresenter.LoadBitmap(tile.filePath);

                    tile.Image.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            tile.Image.Source = bitmap;
                            Grid.SetColumn(tile.Image, tile.Col);
                            Grid.SetRow(tile.Image, tile.Row);
                        })
                    );
                    //string filePath = BitmapPresenter.PickImageFileInfo(tile.ByPlayback).FilePath;
                    //BitmapPresenter.SlideIndex(tile.ByPlayback);
                    //Task.Run(() =>
                    //{
                    //    //Image newImage = new Image();
                    //    //newImage.Source = BitmapPresenter.LoadBitmap(filePath);
                    //    image.Dispatcher.BeginInvoke(
                    //        new Action(() =>
                    //        {
                    //            //image = newImage;
                    //            image.Source = BitmapPresenter.LoadBitmap(filePath);
                    //        })
                    //    );
                    //    //Thread.Sleep(500);
                    //});
                }
                catch 
                {
                    //Grid g = new Grid();
                    //g.Background = new SolidColorBrush(Colors.Gray);
                    //Grid.SetColumn(g, tile.Col);
                    //Grid.SetRow(g, tile.Row);
                    //MainGrid.Children.Add(g);
                    BitmapPresenter.SlideIndex(tile.ByPlayback);
                }
                finally
                {

                }

            });
        }

        // 非同期でロード
        private void LoadTileImageAsync()
        {
            if (BitmapPresenter.FileInfo.Count < 1) return;

            foreach (Tile tile in this.tiles)
            {
                Image image = tile.Image;
                MainGrid.Children.Add(image);
                ImageFileInfo iFileInfo = BitmapPresenter.PickImageFileInfo(tile.ByPlayback);
                tile.filePath = iFileInfo.FilePath;
                BitmapPresenter.SlideIndex(tile.ByPlayback);
            }

            if (bitmapLoadThread != null) bitmapLoadThread.Join();

            bitmapLoadThread = new Thread(() =>
            {
#if DEBUG
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
#endif
                foreach(Tile tile in tiles)
                {
                    BitmapImage bitmap = BitmapPresenter.LoadBitmap(tile.filePath); // エラー時はnullが返る

                    tile.Image.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            tile.Image.Source = bitmap;
                            Grid.SetColumn(tile.Image, tile.Col);
                            Grid.SetRow(tile.Image, tile.Row);
                        })
                    );
                    Debug.WriteLine("Total Memory = {0} KB", GC.GetTotalMemory(true) / 1024);
                    if (reqireThreadRelease) break;
                }
                //Thread.Sleep(300);

                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.SystemIdle);
                Dispatcher.Run();

#if DEBUG
                sw.Stop();
                Debug.WriteLine( tiles.Count + " files loaded:  " + sw.Elapsed);
#endif

                return;
            })
            {
                Name = "load tile image:",
                IsBackground = true,
            };

            bitmapLoadThread.SetApartmentState(ApartmentState.STA);
            bitmapLoadThread.Start();
            //thread.Join();

        }



        /// <summary>
        /// 連続的スライド開始
        /// </summary>
        /// <param name="moveTime"></param>
        public void BeginContinuousSlideAnimation(Point ptFrom, int moveTime)
        {
            storyboard = new Storyboard();

            var a = new ThicknessAnimation();
            //a.FillBehavior = FillBehavior.Stop;
            Storyboard.SetTarget(a, this);
            Storyboard.SetTargetProperty(a, new PropertyPath(TileContainer.MarginProperty));
            a.From = new Thickness(ptFrom.X, ptFrom.Y, 0, 0);
            a.To = new Thickness(this.wrapPoint.X, wrapPoint.Y, 0, 0);

            double p;
            if(IsHorizontalSlide)
                p = (ptFrom.X - wrapPoint.X) / this.Width;
            else
                p = (ptFrom.Y - wrapPoint.Y) / this.Height;
            if (p < 0) p = -p;
            a.Duration = TimeSpan.FromMilliseconds(moveTime * p);
            //Timeline.SetDesiredFrameRate(storyboard, 30);

            storyboard.Children.Add(a);
            onStoryboardCompleted = (s, e) =>
            {
                if (!IsContinuousSliding) return;
                LoadImageToGrid(false, true);
                this.MainWindow.UpdatePageInfo();

                // 前方のコンテナから、リスタート座標を取得し、ループ
                //Point restartPoint = new Point();
                //restartPoint.X = 0;
                //restartPoint.Y = 0;

                //switch (slideDirection)
                //{
                //    case SlideDirection.Left:
                //        restartPoint.X = ForwardContainer.Margin.Left + ForwardContainer.Width;
                //        break;
                //    case SlideDirection.Top:
                //        restartPoint.Y = ForwardContainer.Margin.Top + ForwardContainer.Height;
                //        break;
                //    case SlideDirection.Right:
                //        restartPoint.X = ForwardContainer.Margin.Left - ForwardContainer.Width;
                //        break;
                //    case SlideDirection.Bottom:
                //        restartPoint.Y = ForwardContainer.Margin.Top - ForwardContainer.Height;
                //        break;
                //}

                //Debug.WriteLine("restart point: " + restartPoint);
                //BeginContinuousSlideAnimation(restartPoint, moveTime);

                // 予め決めておいたリスタート座標からループ
                BeginContinuousSlideAnimation(startPoint, moveTime);
            };
            storyboard.Completed += onStoryboardCompleted;

            IsContinuousSliding = true;
            storyboard.Begin();
        }
        

        /// <summary>
        /// アクティブスライドの移動量を取得
        /// </summary>
        /// <param name="isPlayback">巻き戻しかどうか</param>
        /// <param name="isNoDeviation">位置がずれていない(Sizeで割り切れる)</param>
        /// <param name="byOneImage">画像一枚ずつ移動</param>
        /// <returns>移動量(整数値)</returns>
        private Point GetActiveSlideDiff(bool isNoDeviation, bool isPlayback, bool byOneImage)
        {
            Point diff;

            double wp, hp; // 基準とするサイズ
            if (byOneImage)
            {
                wp = this.Width / this.MainGrid.ColumnDefinitions.Count;
                hp = this.Height / this.MainGrid.RowDefinitions.Count;
            }
            else
            {
                wp = this.Width;
                hp = this.Height;
            }

            if (byOneImage)
            {
                if (IsHorizontalSlide && Margin.Left % wp == 0) isNoDeviation = true;
                else if (IsVerticalSlide && Margin.Top % hp == 0) isNoDeviation = true;
            }

            if (isNoDeviation)
            {
                switch (slideDirection)
                {
                    case SlideDirection.Left:
                        diff = new Point(-wp, 0);
                        break;
                    case SlideDirection.Top:
                        diff = new Point(0, -hp);
                        break;
                    case SlideDirection.Right:
                        diff = new Point(wp, 0);
                        break;
                    case SlideDirection.Bottom:
                        diff = new Point(0, hp);
                        break;
                    default:
                        diff = new Point(0, 0);
                        break;
                }

                if (isPlayback) diff = new Point(-diff.X, -diff.Y);
            }

            else
            {
                switch (slideDirection)
                {
                    case SlideDirection.Left:
                        diff = new Point( -(Margin.Left + Width) % wp, 0);
                        break;
                    case SlideDirection.Top:
                        diff = new Point(0,  -(Margin.Top + Height) % hp);
                        break;
                    case SlideDirection.Right:
                        diff = new Point( -(Margin.Left - Width) % wp, 0);
                        break;
                    case SlideDirection.Bottom:
                        diff = new Point(0,  -(Margin.Top - Height) % hp);
                        break;
                    default:
                        diff = new Point(0, 0);
                        break;
                }

                if (isPlayback)
                {
                    if (IsHorizontalSlide)
                    {
                        if (diff.X < 0) diff = new Point(diff.X + wp, diff.Y);
                        else if(diff.X > 0) diff = new Point(diff.X - wp, diff.Y);
                    }
                    else
                    {
                        if (diff.Y < 0) diff = new Point(diff.X, diff.Y + hp);
                        else if(diff.Y > 0) diff = new Point(diff.X, diff.Y - hp);
                    }
                }
            }


            return diff;
        }


        /// <summary>
        /// アクティブスライド開始
        /// </summary>
        /// <param name="isNoDeviation">位置がずれていない(Sizeで割り切れる)</param>
        /// <param name="isPlayback">巻き戻しかどうか</param>
        /// <param name="slideByOneImage">画像１枚単位でスライド</param>
        /// <param name="moveTime">かける時間(ms)</param>
        public void BeginActiveSlideAnimation( bool isNoDeviation, bool isPlayback, bool slideByOneImage, int moveTime)
        {
            // まだスライド中
            if (IsActiveSliding) return;

            // 末尾のコンテナを逆サイドに移動し、巻き戻し画像を入れる
            if(isPlayback && IsStartPoint)  // wheel up
            {
                ReleaseBitmapLoadThread(); // 末尾コンテナが読み込み中だったら、開放してから
                this.Margin = new Thickness(wrapPoint.X, wrapPoint.Y, 0, 0);
                foreach (Tile tile in this.tiles)
                {
                    tile.Image.Source = null;
                }
                this.LoadImageToGrid(true, false); // 同期読み込み(読み込んでからスライドアニメーション)
                this.MainWindow.UpdatePageInfo();
                Debug.WriteLine("move container touch to wrap point: " + this.Margin);
            }

            // 移動量
            Point diff;
            diff = GetActiveSlideDiff(isNoDeviation, isPlayback, slideByOneImage);

            // 終点
            double dest_x = Margin.Left + diff.X;
            dest_x = Math.Round(dest_x, 0);
            double dest_y = Margin.Top + diff.Y;
            dest_y = Math.Round(dest_y, 0);
            this.activeSlideEndPoint = new Point(dest_x, dest_y);

            // animation
            storyboard = new Storyboard();
            var a = new ThicknessAnimation();
            a.FillBehavior = FillBehavior.Stop;
            Storyboard.SetTarget(a, this);
            Storyboard.SetTargetProperty(a, new PropertyPath(TileContainer.MarginProperty));
            a.From = new Thickness(Margin.Left, Margin.Top, 0, 0);
            a.To = new Thickness(activeSlideEndPoint.X, activeSlideEndPoint.Y, 0, 0);

            // 時間
            double p, w, h;
            w = Width;
            h = Height;
            if (slideByOneImage)
            {
                w = w / MainGrid.ColumnDefinitions.Count;
                h = h / MainGrid.RowDefinitions.Count;
            }
            if(IsHorizontalSlide) p = diff.X / w;
            else p = diff.Y / h;
            if (p < 0) p = -p;
            a.Duration = TimeSpan.FromMilliseconds(moveTime * p);
            Debug.WriteLine("slide move time: " + moveTime * p );

            storyboard.Children.Add(a);

            onStoryboardCompleted = (s, e) =>
            {
                // 先に進めた時、先頭のコンテナを末尾に移動するように、座標を変更
                if (!isPlayback && activeSlideEndPoint == wrapPoint)
                {
                    activeSlideEndPoint = startPoint;
                    LoadImageToGrid(false, true);
                }
                // 座標を確定させる
                Margin = new Thickness(this.activeSlideEndPoint.X, this.activeSlideEndPoint.Y, 0, 0);
                Debug.WriteLine("active slide end: " + Margin );

                // メインウインドウページ情報を更新
                this.MainWindow.UpdatePageInfo();

                IsActiveSliding = false;
            };
            storyboard.Completed += onStoryboardCompleted;

            IsActiveSliding = true;
            storyboard.Begin();
            //storyboard.Begin(this, true);

        }


        /// <summary>
        /// スライド停止(スライドの種別問わず)
        /// </summary>
        public void StopSlideAnimation()
        {
            IsContinuousSliding = false;
            IsActiveSliding = false;

            if (storyboard != null)
            {
                Margin = new Thickness(Margin.Left, Margin.Top, 0, 0);

                storyboard.Completed -= onStoryboardCompleted;
                storyboard.Remove();  // completed イベントが発火する

                //this.Dispatcher.Invoke(new Action(() =>
                //{
                //}), null);

                //storyboard.Pause();
            }
        }


        /// <summary>
        /// ビットマップ読み込み用スレッドを安全に開放
        /// </summary>
        public static void ReleaseBitmapLoadThread()
        {
            reqireThreadRelease = true;
            Debug.WriteLine("wait release thread");
            if(bitmapLoadThread != null)bitmapLoadThread.Join();
            Debug.WriteLine("thread released");
            reqireThreadRelease = false;
        }


        /// <summary>
        /// 画像の不透明度を更新
        /// </summary>
        public void UpdateTileImageOpacity(double opacity)
        {
            foreach(Tile tile in tiles)
            {
                tile.Image.Opacity = opacity;
            }
        }

    }
}
