using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;

using C_SlideShow.CommonControl;


namespace C_SlideShow.Core
{
    public enum SlideShowState
    {
        Stop, Continuous, Interval
    }

    public class ImgContainerManager
    {
        /* ---------------------------------------------------- */
        //     フィールド
        /* ---------------------------------------------------- */
        private int numofBackwardContainer  = 2;    // 巻き戻し方向のコンテナの数
        private int numofForwardContainer   = 2;    // 進む方向のコンテナの数
        private Point wrapPoint     = new Point();  // 前方向スライド後、コンテナが末尾に戻る座標
        private Point wrapPointRev  = new Point();  // 後方向スライド後、コンテナが先頭に戻る座標
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);  // セマフォ

        //private List<CancellationTokenSource> ctsList           = new List<CancellationTokenSource>();    // Taskのキャンセルを管理
        //private List<CancellationTokenSource> ctsListForSlide   = new List<CancellationTokenSource>();    // Taskのキャンセルを管理(スライドに依る画像読み込みのTask)

        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        public List<ImgContainer> Containers { get; set; } = new List<ImgContainer>();
        public SlideShowState SlideShowState { get; set; } = SlideShowState.Stop;
        public bool IsActiveSliding { get { return Containers.Any(c => c.IsActiveSliding); } }
        public ImagePool ImagePool { get; set; } = new ImagePool();
        public int CurrentImageIndex
        {
            get
            {
                ImgContainer ic = CurrentContainer;
                if( ic == null ) return 0;
                else
                {
                    ImageFileContext context = ic.ImageFileContextMapList.FirstOrDefault(ct => !ct.IsDummy);
                    if( context == null ) return 0;
                    else
                    {
                        return ImagePool.ImageFileContextList.IndexOf(context);
                    }
                }
            }
        }
        public ImgContainer CurrentContainer
        {
            get
            {
                return Containers.FirstOrDefault(c => c.CurrentIndex == 0);
            }
        }
        public ImageFileContext CurrentImageFileContext
        {
            get
            {
                ImgContainer ic = CurrentContainer;
                if( ic == null ) return null;
                else
                {
                    return ic.ImageFileContextMapList.FirstOrDefault(ct => !ct.IsDummy);
                }

            }
        }
        public Size CurrentContainerGridSize
        {
            get
            {
                ImgContainer c = CurrentContainer;
                return new Size(c.Width / c.NumofCol, c.Height / c.NumofRow);
            }
        }
        public int ContainerWidth
        {
            get { return (int)Containers[0].Width; }
        }
        public int ContainerHeight
        {
        get { return (int)Containers[0].Height; }
        }
        public Point ContinuousSlideReturnPoint
        {
            get
            {
                Point returnPoint = new Point();
                returnPoint.X = 0; returnPoint.Y = 0;
                SlideDirection dir = MainWindow.Current.Setting.TempProfile.SlideDirection.Value;

                switch( dir )
                {
                case SlideDirection.Left:
                    returnPoint.X   = numofForwardContainer * ContainerWidth;
                    break;
                case SlideDirection.Top:
                    returnPoint.Y   = numofForwardContainer * ContainerHeight;
                    break;
                case SlideDirection.Right:
                    returnPoint.X   = - numofForwardContainer * ContainerWidth;
                    break;
                case SlideDirection.Bottom:
                    returnPoint.Y   = - numofForwardContainer * ContainerHeight;
                    break;
                }

                return returnPoint;
            }
        }
        public DispatcherTimer IntervalSlideTimer { get; set; } = new DispatcherTimer(DispatcherPriority.Normal);
        public int IntervalSlideTimerCount { get; set; } = 0;

        public SlideDirection CurrentSlideDirection
        {
            get
            {
                return MainWindow.Current.Setting.TempProfile.SlideDirection.Value;
            }
        }
        public Profile TempProfile { get { return MainWindow.Current.Setting.TempProfile; } }

          
        /* ---------------------------------------------------- */
        //     コンストラクタ
        /* ---------------------------------------------------- */
        public ImgContainerManager()
        {
            // タイマー
            IntervalSlideTimer.Tick += IntervalSlideTimer_Tick; ;
            IntervalSlideTimer.Interval = new TimeSpan(0, 0, 0, 1);
        }

        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */
        public void Initialize()
        {
            for(int i = -numofBackwardContainer; i <= numofForwardContainer; i++ )
            {
                Containers.Add( new ImgContainer(i) );
            }
        }

        public void InitContainerIndex()
        {
            Containers.ForEach( c => c.InitIndex() );
        }

        public void InitContainerSize()
        {
            Profile pf = this.TempProfile;
            Containers.ForEach( c => c.InitSize(pf.AspectRatio.H, pf.AspectRatio.V, pf.TilePadding.Value, pf.NumofMatrix.Col, pf.NumofMatrix.Row) );
        }

        public void InitContainerPos()
        {
            Containers.ForEach( c => c.InitPos(CurrentSlideDirection) );
        }

        public void InitWrapPoint(SlideDirection dir)
        {
            wrapPoint.X = 0;    wrapPoint.Y = 0;
            wrapPointRev.X = 0; wrapPointRev.Y = 0;
            switch( dir )
            {
            case SlideDirection.Left:
                wrapPoint.X     = -(numofBackwardContainer + 1) * ContainerWidth;
                wrapPointRev.X  = (numofForwardContainer + 1) * ContainerWidth;
                break;
            case SlideDirection.Top:
                wrapPoint.Y     = -(numofBackwardContainer + 1) * ContainerHeight;
                wrapPointRev.Y  = (numofForwardContainer + 1) * ContainerHeight;
                break;
            case SlideDirection.Right:
                wrapPoint.X     = (numofBackwardContainer + 1) * ContainerWidth;
                wrapPointRev.X  = -(numofForwardContainer + 1) * ContainerWidth;
                break;
            case SlideDirection.Bottom:
                wrapPoint.Y     = (numofBackwardContainer + 1) * ContainerHeight;
                wrapPointRev.Y  = -(numofForwardContainer + 1) * ContainerHeight;
                break;
            }
        }

        public void InitContainerGrid()
        {
            Containers.ForEach( c => c.InitGrid(TempProfile.NumofMatrix.Col, TempProfile.NumofMatrix.Row) );
        }

        public void InitBitmapDecodePixelOfTile()
        {
            Containers.ForEach( c => c.InitBitmapDecodePixelOfTile(TempProfile.AspectRatio.H, TempProfile.AspectRatio.V) );
        }

        public void SetImageElementToContainerGrid()
        {
            Containers.ForEach( tc => tc.SetImageElementToGrid() );
        }

        public async Task InitAllContainer(int index)
        {
            StopSlideShow(true);

            ImagePool.InitIndex(index);
            ImagePool.InitImageFileContextRefCount();
            InitContainerIndex();
            InitContainerSize();
            InitContainerPos();
            InitWrapPoint(CurrentSlideDirection);

            InitContainerGrid();
            InitBitmapDecodePixelOfTile();
            SetImageElementToContainerGrid();

            // 前方向マッピング
            MapImageFileContextToContainer(Containers[2], false);
            MapImageFileContextToContainer(Containers[3], false);
            MapImageFileContextToContainer(Containers[4], false);

            // 巻き戻し方向マッピング
            MapImageFileContextToContainer(Containers[1], true);
            MapImageFileContextToContainer(Containers[0], true);

            // メインウインドウ表示更新
            MainWindow.Current.UpdateMainWindowView();

            // シークバー初期化
            MainWindow.Current.InitSeekbar();

            // ページ番号更新
            MainWindow.Current.UpdatePageInfo();

            // まだ読み込み中のTaskがあるならキャンセル
            Containers.ForEach(c => c.Cts?.Cancel());

            // 画像のロード
            await semaphoreSlim.WaitAsync();
            try {
                await LoadAllContainerImage();
                await CorrectAllContainerBitmap();
            }
            finally { semaphoreSlim.Release(); }

            // 使われていないBitmapImageの開放
            ImagePool.ReleaseBitmapImageOutofRefarence();

            // メインウインドウ表示更新
            MainWindow.Current.UpdateMainWindowView();
        }

        
        public async Task ChangeCurrentIndex(int index)
        {
            if( IsActiveSliding ) return;
            StopSlideShow(true);

            ImagePool.InitIndex(index);
            ImagePool.InitImageFileContextRefCount();
            InitContainerIndex();
            InitContainerPos();

            // マッピング
            MapImageFileContextToContainer(Containers[2], false);
            MapImageFileContextToContainer(Containers[3], false);
            MapImageFileContextToContainer(Containers[4], false);
            MapImageFileContextToContainer(Containers[1], true);
            MapImageFileContextToContainer(Containers[0], true);

            // ページ番号
            MainWindow.Current.UpdatePageInfo();

            // まだ読み込み中のTaskがあるならキャンセル
            Containers.ForEach(c => c.Cts?.Cancel());

            // 画像のロード
            await semaphoreSlim.WaitAsync();
            try { await LoadAllContainerImage(); }
            finally { semaphoreSlim.Release(); } 

            // 使われていないBitmapImageの開放
            ImagePool.ReleaseBitmapImageOutofRefarence();
        }


        public async Task LoadAllContainerImage()
        {
            // インデックス
            int[] indices = { 2, 3, 1, 4, 0 };

            // キャンセルトークン作成
            CancellationTokenSource[] ctses = new CancellationTokenSource[indices.Length];
            foreach( int i in indices )
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                ctses[i] = cts;
                Containers[i].Cts = cts;
            }

            // ロード
            foreach( int i in indices )
            {
                await Containers[i].LoadImage(ctses[i].Token);
            }
        }


        public async Task CorrectAllContainerBitmap()
        {
            // BitmapDecodePixelOfTileにサイズが足りていないBitmapImageを開放
            Containers.ForEach( c => c.ReleaseIncorrectSizeBitmap() );

            // 更新
            await LoadAllContainerImage();
        }


        public void MapImageFileContextToContainer(ImgContainer container, bool isBackward)
        {
            container.ImageFileContextMapList.Clear();

            bool bReachEnd = false;

            for(int i=0; i < container.NumofGrid; i++ )
            {
                // 前方向
                if( !isBackward )
                {
                    if( bReachEnd ) container.ImageFileContextMapList.Add( ImagePool.DummyImageContext );
                    else container.ImageFileContextMapList.Add( ImagePool.PickForward() );

                    if( !bReachEnd && ImagePool.ForwardIndex == 0 ) bReachEnd = true;
                }

                // 巻き戻し方向
                else
                {
                    if( bReachEnd ) container.ImageFileContextMapList.Insert( 0, ImagePool.DummyImageContext );
                    else container.ImageFileContextMapList.Insert( 0, ImagePool.PickBackward() );

                    if( !bReachEnd && ImagePool.BackwardIndex == ImagePool.ImageFileContextList.Count - 1 ) bReachEnd = true;
                }
            }
        }


        public void ReleaseContainerImage(ImgContainer container)
        {
            foreach( var child in container.MainGrid.Children )
            {
                Image image = child as Image;
                if(image != null )
                {
                    image.Source = null;
                }
            }

            container.ImageFileContextMapList.ForEach( context => 
            {
                context.RefCount--;
                if(context.RefCount <= 0 )
                {
                    context.RefCount = 0;
                    context.BitmapImage = null;
                }
            });
        }


        public void ActiveSlideToForward(bool slideBySizeOfOneImage, int moveTime, bool byIntervalSlideShow)
        {
            if( ImagePool.ImageFileContextList.Count <= 0 ) return;
            if( IsActiveSliding ) return;
            if( SlideShowState == SlideShowState.Continuous ) StopSlideShow(true);
            if( !byIntervalSlideShow && SlideShowState == SlideShowState.Interval ) StopSlideShow(true);

            foreach( ImgContainer container in Containers )
            {
                container.Animation.OnStoryboardCompleted = async (s, e) =>
                {
                    container.Animation.EndActiveSlide();
                    if( container.Pos == wrapPoint ) await ReturnContainer(container);
                };
                container.Animation.BeginActiveSlideAnimation(slideBySizeOfOneImage, false, moveTime, CurrentContainerGridSize);
            }
        }


        public void ActiveSlideToBackward(bool slideBySizeOfOneImage, int moveTime)
        {
            if( ImagePool.ImageFileContextList.Count <= 0 ) return;
            if( IsActiveSliding ) return;
            if( SlideShowState == SlideShowState.Continuous ) StopSlideShow(true);

            foreach( ImgContainer container in Containers )
            {
                container.Animation.OnStoryboardCompleted = async (s, e) =>
                {
                    container.Animation.EndActiveSlide();
                    if( container.Pos == wrapPointRev ) await ReturnContainerRev(container);
                };
                container.Animation.BeginActiveSlideAnimation(slideBySizeOfOneImage, true, moveTime, CurrentContainerGridSize);
            }
        }


        public async Task ReturnContainer(ImgContainer container)
        {
            // まだコンテナにTaskが残っているならキャンセル
            container.Cts?.Cancel();

            // キャンセルトークン作成
            CancellationTokenSource cts = new CancellationTokenSource();
            container.Cts = cts;

            Containers.ForEach(c => c.CurrentIndex -= 1);
            container.CurrentIndex = numofForwardContainer;
            container.InitPos(CurrentSlideDirection);
            ReleaseContainerImage(container);
            ImagePool.ShiftBackwardIndex( container.NumofImage );
            MapImageFileContextToContainer(container, false);
            MainWindow.Current.UpdatePageInfo();
            await container.LoadImage(cts.Token);
        }


        public async Task ReturnContainerRev(ImgContainer container)
        {
            container.Cts?.Cancel();
            CancellationTokenSource cts = new CancellationTokenSource();
            container.Cts = cts;

            Containers.ForEach(c => c.CurrentIndex += 1);
            container.CurrentIndex = -numofBackwardContainer;
            container.InitPos(CurrentSlideDirection);
            ReleaseContainerImage(container);
            ImagePool.ShiftForwardIndex( - container.NumofImage );
            MapImageFileContextToContainer(container, true);
            MainWindow.Current.UpdatePageInfo();
            await container.LoadImage(cts.Token);
        }


        public void StartContinuousSlideShow()
        {
            if( SlideShowState == SlideShowState.Continuous ) return;
            if( SlideShowState == SlideShowState.Interval ) StopSlideShow(false);

            // 速度 → 移動にかける時間パラメータ 3000(ms) to 300000(ms)
            int moveTime = (int)( 300000 / (double)TempProfile.SlideSpeed.Value );

            foreach( ImgContainer container in Containers )
            {
                // 折り返し時
                container.Animation.OnStoryboardCompleted = async (s, e) =>
                {
                    container.Animation.BeginContinuousSlideAnimation(ContinuousSlideReturnPoint, wrapPoint, moveTime);
                    await ReturnContainer(container);
                };

                // アニメーションスタート
                Point ptFrom = new Point(container.Margin.Left, container.Margin.Top);
                container.Animation.BeginContinuousSlideAnimation(ptFrom, wrapPoint, moveTime);
            }

            SlideShowState = SlideShowState.Continuous;
        }


        public void StartIntervalSlideShow()
        {
            IntervalSlideTimer.Start();
            IntervalSlideTimerCount = 0;
            SlideShowState = SlideShowState.Interval;
        }


        public void StartSlideShow(bool allowNotification)
        {
            // ファイル無し
            if( ImagePool.ImageFileContextList.Count < 1 ) return;

            if( TempProfile.SlidePlayMethod.Value == SlidePlayMethod.Continuous )
            {
                // 連続的スライド
                StartContinuousSlideShow();
            }
            else
            {
                // インターバルスライド
                StartIntervalSlideShow();
                if( allowNotification )
                {
                    MainWindow.Current.NotificationBlock.Show("スライドショー開始 (待機時間" + TempProfile.SlideInterval.Value + "秒)", NotificationPriority.Normal, NotificationTime.Short, NotificationType.None);
                }
            }

            // 再生ボタン表示変更
            MainWindow.Current.UpdateToolbarViewing();
        }


        public void StopSlideShow(bool allowNotification)
        {
            Containers.ForEach( c => c.Animation.StopSlideAnimation() );

            if( allowNotification && TempProfile.SlidePlayMethod.Value == SlidePlayMethod.Interval && IntervalSlideTimer.IsEnabled )
            {
                MainWindow.Current.NotificationBlock.Show("スライドショー停止", NotificationPriority.Normal, NotificationTime.Short, NotificationType.None);
            }
            IntervalSlideTimer.Stop();
            IntervalSlideTimerCount = 0;

            SlideShowState = SlideShowState.Stop;
            MainWindow.Current.UpdateToolbarViewing();
        }


        public void ApplyGridDifinition()
        {
            var t = InitAllContainer(CurrentImageIndex);
        }


        public void ApplyAspectRatio(bool bReleaseBitmap)
        {
            MainWindow.Current.UpdateToolbarViewing();
            var t = InitAllContainer(CurrentImageIndex);
        }


        public void ApplyGridLineColor()
        {
            Containers.ForEach( c => c.ApplyGridLineColor() );
        }


        public void ApplyGridLineThickness()
        {
            var t = InitAllContainer(CurrentImageIndex);
            MainWindow.Current.UpdateMainWindowView();
        }


        public void Hide()
        {
            Containers.ForEach(c => c.Visibility = Visibility.Collapsed);
        }


        public void Show()
        {
            Containers.ForEach(c => c.Visibility = Visibility.Visible);
        }


        /* ---------------------------------------------------- */
        //     イベント
        /* ---------------------------------------------------- */
        private void IntervalSlideTimer_Tick(object sender, EventArgs e)
        {
            IntervalSlideTimerCount += 1;

            if(TempProfile.SlidePlayMethod.Value == SlidePlayMethod.Interval)
            {
                if(IntervalSlideTimerCount >= TempProfile.SlideInterval.Value)
                {
                    ActiveSlideToForward(TempProfile.SlideByOneImage.Value, TempProfile.SlideTimeInIntevalMethod.Value, true);

                    int slideTime = (int)( TempProfile.SlideTimeInIntevalMethod.Value / 1000 ); // milisec → sec
                    IntervalSlideTimerCount = 0 - slideTime;
                }
            }
            else
            {
                IntervalSlideTimer.Stop();
            }
        }


    }
}
