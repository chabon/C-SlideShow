using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using System.IO;

namespace C_SlideShow.Core
{
    public class ImgContainerAnimation
    {
        /* ---------------------------------------------------- */
        //     フィールド
        /* ---------------------------------------------------- */
        private ImgContainer   container;
        private Storyboard     storyboard;
        Point                  activeSlideEndPoint;

        /* ---------------------------------------------------- */
        //     プロパティ
        /* ---------------------------------------------------- */
        public EventHandler OnStoryboardCompleted { get; set; }
        public bool         IsActiveSliding { get; private set; }

        /* ---------------------------------------------------- */
        //     コンストラクタ
        /* ---------------------------------------------------- */
        public ImgContainerAnimation(ImgContainer container)
        {
            this.container = container;
        }

        /* ---------------------------------------------------- */
        //     メソッド
        /* ---------------------------------------------------- */
        public void BeginActiveSlideAnimation(bool slideBySizeOfOneImage, bool isPlayback, int moveTime, Size currentContainerGridSize)
        {
            // 移動量
            Point diff;
            diff = GetActiveSlideDiff(slideBySizeOfOneImage, isPlayback, currentContainerGridSize);

            // 終点(小数点以下は切り捨て)
            double dest_x = container.Margin.Left + diff.X;
            dest_x = Math.Round(dest_x, 0);
            double dest_y = container.Margin.Top + diff.Y;
            dest_y = Math.Round(dest_y, 0);
            activeSlideEndPoint = new Point(dest_x, dest_y);

            // アニメーション
            storyboard = new Storyboard();
            var a = new ThicknessAnimation();
            a.FillBehavior = FillBehavior.Stop;
            Storyboard.SetTarget(a, container);
            Storyboard.SetTargetProperty(a, new PropertyPath(ImgContainer.MarginProperty));
            a.From = new Thickness(container.Margin.Left, container.Margin.Top, 0, 0);
            a.To = new Thickness(dest_x, dest_y, 0, 0);

            // 時間
            double p, w, h;
            w = container.Width;
            h = container.Height;
            if(MainWindow.Current.Setting.TempProfile.IsHorizontalSlide) p = diff.X / w;
            else p = diff.Y / h;
            if (p < 0) p = -p;
            a.Duration = TimeSpan.FromMilliseconds(moveTime * p);
            //Debug.WriteLine("slide move time: " + moveTime * p );

            storyboard.Children.Add(a);

            // アニメーション終了時
            storyboard.Completed += OnStoryboardCompleted;

            storyboard.Begin();
            IsActiveSliding = true;
        }

        public void EndActiveSlide()
        {
            // 座標を確定させる
            container.Margin = new Thickness(activeSlideEndPoint.X, activeSlideEndPoint.Y, 0, 0);
            IsActiveSliding = false;
        }

        public Point GetActiveSlideDiff(bool slideBySizeOfOneImage, bool isPlayback, Size currentContainerGridSize)
        {
            // 移動量の基準値
            Point standardOfMove; 
            if( slideBySizeOfOneImage )
            {
                standardOfMove = new Point(currentContainerGridSize.Width, currentContainerGridSize.Height);
            }
            else
            {
                standardOfMove = new Point(container.Width, container.Height);
            }

            // 各方向の移動量算出(絶対値)
            Point mod = new Point(container.Margin.Left % standardOfMove.X, container.Margin.Top % standardOfMove.Y);
            double moveLeft, moveTop, moveRight, moveBottom;
            if( mod.X == 0 ) { moveLeft = moveRight = standardOfMove.X; }
            else if( mod.X > 0 )
            {
                moveLeft  = mod.X;
                moveRight = standardOfMove.X - mod.X;
            }
            else
            {
                moveLeft = standardOfMove.X - Math.Abs(mod.X);
                moveRight = Math.Abs(mod.X);
            }
            if( mod.Y == 0 ) { moveTop = moveBottom = standardOfMove.Y; }
            else if( mod.X > 0 )
            {
                moveLeft  = mod.X;
                moveRight = standardOfMove.X - mod.X;
            }
            else
            {
                moveLeft  = standardOfMove.X - Math.Abs(mod.X);
                moveRight = Math.Abs(mod.X);
            }

            if( mod.Y == 0 ) moveTop  = moveBottom = standardOfMove.Y;
            else if(mod.Y > 0 )
            {
                moveTop     = mod.Y;
                moveBottom  = standardOfMove.Y - mod.Y;
            }
            else
            {
                moveTop     = standardOfMove.Y - Math.Abs(mod.Y);
                moveBottom  = Math.Abs(mod.Y);
            }

            // スライド方向取得
            SlideDirection dir = MainWindow.Current.Setting.TempProfile.SlideDirection.Value;
            if( isPlayback ) dir = Profile.GetReversedSlideDirection(dir);

            // 移動量(実数)を返す
            double x, y;
            switch( dir )
            {
                case SlideDirection.Left:
                    x = -moveLeft;
                    y = 0;
                    break;
                case SlideDirection.Top:
                    x = 0;
                    y = -moveTop;
                    break;
                case SlideDirection.Right:
                    x = moveRight;
                    y = 0;
                    break;
                case SlideDirection.Bottom:
                    x = 0;
                    y = moveBottom;
                    break;
                default:
                    x = y = 0;
                    break;
            }
            return new Point(x, y);
        }

        public void BeginContinuousSlideAnimation(Point ptFrom, Point wrapPoint, int moveTime)
        {
            // アニメーション
            storyboard = new Storyboard();
            var a = new ThicknessAnimation();
            //a.FillBehavior = FillBehavior.Stop;
            Storyboard.SetTarget(a, container);
            Storyboard.SetTargetProperty(a, new PropertyPath(ImgContainer.MarginProperty));
            a.From  = new Thickness(ptFrom.X, ptFrom.Y, 0, 0);
            a.To    = new Thickness(wrapPoint.X, wrapPoint.Y, 0, 0);

            // 時間
            double p;
            if(MainWindow.Current.Setting.TempProfile.IsHorizontalSlide) p = (ptFrom.X - wrapPoint.X) / container.Width;
            else p = (ptFrom.Y - wrapPoint.Y) / container.Height;
            if (p < 0) p = -p;
            a.Duration = TimeSpan.FromMilliseconds(moveTime * p);
            //Timeline.SetDesiredFrameRate(storyboard, 30);

            storyboard.Children.Add(a);
            storyboard.Completed += OnStoryboardCompleted;
            storyboard.Begin();
        }

        public void StopSlideAnimation()
        {
            if (storyboard != null)
            {
                container.Margin = new Thickness(container.Margin.Left, container.Margin.Top, 0, 0);
                storyboard.Completed -= OnStoryboardCompleted;
                storyboard.Remove();  // completed イベントが発火する
            }
        }

    }
}
