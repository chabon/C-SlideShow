using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Threading;
using System.Windows;

namespace C_SlideShow
{
    public static class ExtensionMethods
    {
        private static readonly Action EmptyDelegate = delegate { };
        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }


        /// <summary>
        /// 縦横比を維持したまま、領域内に収まるよう伸縮する
        /// </summary>
        /// <param name="self">伸縮するSize</param>
        /// <param name="dest">伸縮先の領域となるSize</param>
        /// <returns>伸縮後のSize</returns>
        public static Size StreachAsUniform(this Size self, Size dest)
        {
            if( self == Size.Empty || dest == Size.Empty ) return self;

            var rateX = self.Width  / dest.Width;
            var rateY = self.Height / dest.Height;

            var scale = 1.0 / (rateX > rateY ? rateX : rateY);

            return new Size(self.Width * scale, self.Height * scale);
        }


        /// <summary>
        /// Size構造体の値を整数値に丸める
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Size Round(this Size self)
        {
            return new Size( Math.Round(self.Width), Math.Round(self.Height) );
        }
    }
}
