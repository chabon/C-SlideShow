﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Windows.Media;


namespace C_SlideShow.Archiver
{
    public class ArchiverBase
    {
        public string ArchiverPath { get; set; }
        public string[] AllowedFileExt = { ".jpg", ".png", ".jpeg", ".bmp", ".gif" };

        public ArchiverBase()
        {
        }

        public ArchiverBase(string archiverPath)
        {
            ArchiverPath = archiverPath;
        }

        public virtual Stream OpenStream(string path)
        {
            if( File.Exists(path) )
                return File.OpenRead(path);
            else
                return Stream.Null;
        }

        public virtual List<ImageFileInfo> LoadImageFileInfoList()
        {
            return new List<ImageFileInfo>();
        }

        public virtual void DisposeArchive()
        {

        }
    }
}
