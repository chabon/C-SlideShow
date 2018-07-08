using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace C_SlideShow.Archiver
{
    public class FolderArchiver : ArchiverBase
    {
        public FolderArchiver(string archiverPath) : base(archiverPath)
        {
            LeaveHistory = true;
        }

        public override Stream OpenStream(string path)
        {
            if( File.Exists(path) )
                return File.OpenRead(path);
            else
                return Stream.Null;
        }

        public override List<ImageFileInfo> LoadImageFileInfoList()
        {
            // フォルダ内のファイルパスを取得し、拡張子でフィルタ
            var imgPathes = Directory.GetFiles( this.ArchiverPath, "*.*", SearchOption.AllDirectories );
            var filteredFiles = imgPathes.Where(file => AllowedFileExt.Any(ext => 
                file.ToLower().EndsWith(ext)));

            // ロード
            List<ImageFileInfo> newList = new List<ImageFileInfo>();
            foreach (string imgPath in filteredFiles)
            {
                ImageFileInfo imageFileInfo = new ImageFileInfo();
                imageFileInfo.FilePath = imgPath;
                imageFileInfo.Archiver = this;
                newList.Add(imageFileInfo);
            }

            return newList;
        }
    }
}
