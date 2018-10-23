using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using C_SlideShow.Core;


namespace C_SlideShow.Archiver
{
    public class FolderArchiver : ArchiverBase
    {
        public FolderArchiver(string archiverPath) : base(archiverPath)
        {
            LeaveHistory = true;
            CanReadFile  = true;
        }

        public override Stream OpenStream(string path)
        {
            if( File.Exists(path) )
                return File.OpenRead(path);
            else
                return Stream.Null;
        }

        public override List<ImageFileContext> LoadImageFileContextList()
        {
            // サブディレクトリを含める/含めない オプション
            SearchOption searchOpt;
            if( MainWindow.Current.Setting.SerachAllDirectoriesInFolderReading ) searchOpt = SearchOption.AllDirectories;
            else searchOpt = SearchOption.TopDirectoryOnly;

            // フォルダ内のファイルパスを取得し、拡張子でフィルタ
            var imgPathes = Directory.GetFiles( this.ArchiverPath, "*.*", searchOpt );
            var filteredFiles = imgPathes.Where(file => AllowedFileExt.Any(ext => 
                file.ToLower().EndsWith(ext)));

            // ロード
            List<ImageFileContext> newList = new List<ImageFileContext>();
            foreach (string imgPath in filteredFiles)
            {
                ImageFileContext imageFileContext = new ImageFileContext(imgPath);
                imageFileContext.Archiver = this;
                newList.Add(imageFileContext);
            }

            return newList;
        }
    }
}
