using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Archiver
{
    public class RarArchiver : SharpCompressArchiver
    {
        public RarArchiver(string archiverPath) : base(archiverPath)
        {

        }
    }
}
