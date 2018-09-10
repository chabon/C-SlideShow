using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Threading;

namespace C_SlideShow.Shortcut.Command
{
    public class ShowContextMenu : ICommand
    {
        public CommandID ID      { set; get; }
        public Scene     Scene   { set; get; }
        public string    Message { get; }

        public int       Value           { get; set; }
        public string    StrValue        { get; set; }
        public bool      EnableValue     { get; } = false;
        public bool      EnableStrValue  { get; } = false;

        private bool closedEventFinished = true;

        public ShowContextMenu()
        {
            ID    = CommandID.ShowContextMenu;
            Scene = Scene.All;
        }
        
        public bool CanExecute()
        {
            return true;
        }

        public void Execute()
        {
            MainWindow mw = MainWindow.Current;

            // カーソル下のタイル取得
            Tile targetTile = mw.GetTileUnderCursor();

            // 取得失敗
            if( targetTile == null ) return;

            // ダミーだった場合
            if( targetTile != null && targetTile.ImageFileInfo.IsDummy ) return;

            // コンテキストメニューのClosedイベント完了を待つ
            if(!closedEventFinished)
            {
                int cnt = 0;
                while( !closedEventFinished )
                {
                    DoEvents();
                    System.Threading.Thread.Sleep(10);
                    cnt++;
                    if( cnt > 50 ) { closedEventFinished = true; return; }
                }
            }

            // コンテキストメニュー作成
            ContextMenu contextMenu = new ContextMenu();

            // タイルを強調表示(拡大時はしない)
            bool IsExpanded = MainWindow.Current.TileExpantionPanel.IsShowing;
            if( !IsExpanded )
            {
                HighlightTargetTile(targetTile);
                closedEventFinished = false;

                contextMenu.Closed += (se, ev) =>
                {
                    targetTile.Border.BorderBrush = new SolidColorBrush(mw.Setting.TempProfile.GridLineColor.Value);
                    targetTile.Border.BorderThickness = new Thickness(mw.Setting.TempProfile.TilePadding.Value);

                    closedEventFinished = true;
                };
            }

            // ツールチップ
            ImageFileInfo fi = targetTile.ImageFileInfo;
            string toolTip_CopyFile     = "コピー後、エクスプローラーで貼り付けが出来ます";
            string toolTip_CopyFileData = "コピー後、ペイント等の画像編集ソフトへ貼り付けが出来ます";
            string toolTip_FilePath;
            if( fi.Archiver.CanReadFile ) { toolTip_FilePath = fi.FilePath; }
            else { toolTip_FilePath = fi.Archiver.ArchiverPath; }
            string toolTip_FileName = System.IO.Path.GetFileName( fi.FilePath );

            // メニューアイテム作成
            if( !IsExpanded )
            {
                contextMenu.Items.Add( CreateMenuItem("拡大表示", null, (s, e) => { mw.TileExpantionPanel.Show(targetTile); }) );
                contextMenu.Items.Add( new Separator() );
                contextMenu.Items.Add( CreateMenuItem("画像1枚分ずらし進める", null, (s, e) => { mw.ShortcutManager.ExecuteCommand(CommandID.ShiftForward,  1, null); }) );
                contextMenu.Items.Add( CreateMenuItem("画像1枚分ずらし戻す",   null, (s, e) => { mw.ShortcutManager.ExecuteCommand(CommandID.ShiftBackward, 1, null); }) );
                contextMenu.Items.Add( new Separator() );
                contextMenu.Items.Add( CreateMenuItem("ファイルをコピー",      toolTip_CopyFile,     (s, e) => { targetTile.CopyFile(); }) );
                contextMenu.Items.Add( CreateMenuItem("画像データをコピー",    toolTip_CopyFileData, (s, e) => { targetTile.CopyImageData(); }) );
                contextMenu.Items.Add( CreateMenuItem("ファイルパスをコピー",  toolTip_FilePath,     (s, e) => { targetTile.CopyFilePath(); }) );
                contextMenu.Items.Add( CreateMenuItem("ファイル名をコピー",    toolTip_FileName,     (s, e) => { targetTile.CopyFileName(); }) );
                contextMenu.Items.Add( new Separator() );
                contextMenu.Items.Add( CreateMenuItem("エクスプローラーで開く", null, (s, e) => { targetTile.OpenExplorer(); }) );
            }
            else
            {
                contextMenu.Items.Add( CreateMenuItem("拡大表示を終了", null, (s, e) => { mw.TileExpantionPanel.Hide(); }) );
                contextMenu.Items.Add( new Separator() );
                contextMenu.Items.Add( CreateMenuItem("ファイルをコピー",      toolTip_CopyFile,     (s, e) => { targetTile.CopyFile(); }) );
                contextMenu.Items.Add( CreateMenuItem("画像データをコピー",    toolTip_CopyFileData, (s, e) => { targetTile.CopyImageData(); }) );
                contextMenu.Items.Add( CreateMenuItem("ファイルパスをコピー",  toolTip_FilePath,     (s, e) => { targetTile.CopyFilePath(); }) );
                contextMenu.Items.Add( CreateMenuItem("ファイル名をコピー",    toolTip_FileName,     (s, e) => { targetTile.CopyFileName(); }) );
                contextMenu.Items.Add( new Separator() );
                contextMenu.Items.Add( CreateMenuItem("エクスプローラーで開く", null, (s, e) => { targetTile.OpenExplorer(); }) );
            }

            foreach(var exAppInfo in MainWindow.Current.Setting.ExternalAppInfoList)
            {
                if( exAppInfo.ShowContextMenu )
                {
                    string name = exAppInfo.GetAppName();
                    if(name != null)
                    {
                        contextMenu.Items.Add( CreateMenuItem( name + "で開く", null, (s, e) => { targetTile.OpenByExternalApp(exAppInfo); }) );
                    }
                }
            }

            // コンテキストメニュー表示
            contextMenu.IsOpen = true;

            // スライド再生中だったら止める
            if( !IsExpanded && mw.IsPlaying )
            {
                mw.StopSlideShow(false);
                contextMenu.Closed += (s, e) => { mw.StartSlideShow(false); };
            }

            return;
        }

        private MenuItem CreateMenuItem(string header, string toolTip, RoutedEventHandler clickEventHandler)
        {
            MenuItem menuItem = new MenuItem();
            menuItem.Header = header;
            if( toolTip != null ) menuItem.ToolTip = toolTip;
            menuItem.Click += clickEventHandler;
            return menuItem;
        }

        private void HighlightTargetTile(Tile targetTile)
        {
            // タイルの強調表示
            if(targetTile != null )
            {
                Border border = targetTile.Border;

                // 強調表示のボーダーの太さを決める(内部の画像のアス比はキープする)
                double h = border.BorderThickness.Left;
                double v = border.BorderThickness.Top;
                double rate = ( border.ActualHeight - (2 * v) ) / ( border.ActualWidth - (2 * h) );

                const double nh = 16;
                double newImageWidth  = border.ActualWidth - (2 * nh);
                double newImageHeight = newImageWidth * rate;
                double nv = ( border.ActualHeight - newImageHeight ) / 2;

                // ボーダーを強調表示
                border.BorderThickness = new Thickness(nh, nv, nh, nv);
                border.BorderBrush = new SolidColorBrush(Colors.LightGreen);
            }
        }

        public string GetDetail()
        {
            return "コンテキストメニューを表示";
        }

        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrames), frame);
            Dispatcher.PushFrame(frame);
        }

        private object ExitFrames(object f)
        {
            ((DispatcherFrame)f).Continue = false;
           
            return null;
        }

    }
}
