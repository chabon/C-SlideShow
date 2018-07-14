using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using System;

namespace C_SlideShow
{
    /// <summary>
    /// @ref http://tawamuredays.blog.fc2.com/blog-entry-82.html
    /// @ref http://blog.xin9le.net/entry/2013/10/29/222336
    /// </summary>
    public static class VisualTreeUtil
    {

        /// <summary>
        /// 指定されたDependencyObjectからビジュアルツリーをさかのぼり、
        /// Genericで指定された型のオブジェクトを検索します。
        /// 【CAUTION!!】DataGridColumnを探す事はできません!)<br/>
        /// 検索できなかった時はnullを返します。<br/>
        /// </summary>
        /// <remarks>
        /// DataGridColumnは、VisualTree、LogicalTree両方に属さない特殊なクラスなので、
        /// このメソッドを使ってDataGridやDaraGridRowオブジェクトを取得しようとしても失敗します。<br/>
        /// </remarks>
        /// <typeparam name="T">検索対象となるクラスの型宣言</typeparam>
        /// <param name="depObj">DependencyObjectインスタンス</param>
        /// <returns>検索対象オブジェクト</returns>
        public static T FindAncestor<T>(this DependencyObject depObj) where T : class {
            var target = depObj;

            try {
                do {
                    //ビジュアルツリー上の親を探します。
                    //T型のクラスにヒットするまでさかのぼり続けます。
                    target = System.Windows.Media.VisualTreeHelper.GetParent(target);

                } while (target != null && !(target is T));

                return target as T;
            } finally {
                target = null;
                depObj = null;
            }
        }

        //--- 子要素を取得
        public static IEnumerable<DependencyObject> Children(this DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var count = VisualTreeHelper.GetChildrenCount(obj);
            if (count == 0)
                yield break;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child != null)
                    yield return child;
            }
        }

        //--- 子孫要素を取得
        public static IEnumerable<DependencyObject> Descendants(this DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            foreach (var child in obj.Children())
            {
                yield return child;
                foreach (var grandChild in child.Descendants())
                    yield return grandChild;
            }
        }

        //--- 特定の型の子要素を取得
        public static IEnumerable<T> Children<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            return obj.Children().OfType<T>();
        }

        //--- 特定の型の子孫要素を取得
        public static IEnumerable<T> Descendants<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            return obj.Descendants().OfType<T>();
        }
    }
}
