using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow
{
    public static class Format
    {
        // 外部連携用書式
        public static readonly string FilePathFormat         = "$FilePath$";
        public static readonly string FolderPathFormat       = "$FolderPath$";
        public static readonly string ParentFolderPathFormat = "$ParentFolderPath$";
    }
}
