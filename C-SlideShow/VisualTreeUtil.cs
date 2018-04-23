using System.Windows;

namespace C_SlideShow
{
    /// <summary>
    /// @ref http://tawamuredays.blog.fc2.com/blog-entry-82.html
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
    }
}
