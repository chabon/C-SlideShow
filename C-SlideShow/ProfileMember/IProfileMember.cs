using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.ProfileMember
{
    interface IProfileMember
    {
        /// <summary>
        /// 値
        /// </summary>
        int Value { get; set; }

        /// <summary>
        /// 有効かどうか
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// ツールチップに表示する文字列
        /// </summary>
        string TooltipStr { get; }
    }
}
