using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Archiver
{
    public class LzhArchiver : SharpCompressArchiver
    {
        public LzhArchiver(string archiverPath) : base(archiverPath)
        {

        }
    }
}
