using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Archiver
{
    public class TarArchiver : SharpCompressArchiver
    {
        public TarArchiver(string archiverPath) : base(archiverPath)
        {

        }
    }
}
