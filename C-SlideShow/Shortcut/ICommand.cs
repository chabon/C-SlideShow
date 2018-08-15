using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut
{
    /// <summary>
    /// ショートカットコマンドインターフェース
    /// </summary>
    public interface ICommand
    {
        CommandID ID { set; get; }      // コマンドID
        Scene Scene { get; set; }       // シーン
        string Message { get; }

        /// <summary>
        /// コマンドが実行可能かどうか
        /// </summary>
        /// <returns>可能ならばtrue</returns>
        bool CanExecute();

        /// <summary>
        /// コマンドを実行
        /// </summary>
        void Execute();

        /// <summary>
        /// コマンドの詳細の取得
        /// </summary>
        string GetDetail();
    }
}
