using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using C_SlideShow.Shortcut.Command;

namespace C_SlideShow.Shortcut
{
    public static class CommandFactory
    {
        public static ICommand CreateById(CommandID id)
        {
            switch( id )
            {
                // 全般
                case CommandID.OpenFolder:                        return new OpenFolder();                          // フォルダを開く
                case CommandID.OpenAdditionalFolder:              return new OpenAdditionalFolder();                // フォルダ追加読み込み
                case CommandID.OpenFile:                          return new OpenFile();                            // ファイルを開く
                case CommandID.OpenAdditionalFile:                return new OpenAdditionalFile();                  // ファイル追加読み込み
                case CommandID.Reload:                            return new Reload();                              // 再読込み
                case CommandID.ToggleTopMost:                     return new ToggleTopMost();                       // 常に最前面表示 ON/OFF
                case CommandID.ToggleFullScreen:                  return new ToggleFullScreen();                    // フルスクリーンモード ON/OFF
                case CommandID.WindowSizeUp:                      return new WindowSizeUp();                        // ウインドウサイズを[ ]%大きく
                case CommandID.WindowSizeDown:                    return new WindowSizeDown();                      // ウインドウサイズを[ ]%小さく
                case CommandID.ShowContextMenu:                   return new ShowContextMenu();                     // コンテキストメニューを表示
                case CommandID.OpenFolderByExplorer:              return new OpenFolderByExplorer();                // エクスプローラでフォルダを開く
                case CommandID.OpenNextArchiver:                  return new OpenNextArchiver();                    // 次のフォルダ(書庫)を開く
                case CommandID.OpenPrevArchiver:                  return new OpenPrevArchiver();                    // 前のフォルダ(書庫)を開く
                case CommandID.ShowAppSettingDialog:              return new ShowAppSettingDialog();                // アプリの設定ダイアログを表示
                case CommandID.CloseFile:                         return new CloseFile();                           // ファイルを閉じる
                case CommandID.MinimizeWindow:                    return new MinimizeWindow();                      // ウインドウを最小化
                case CommandID.ExitApp:                           return new ExitApp();                             // アプリケーションを終了
                case CommandID.OpenImageUnderCursorByExplorer:    return new OpenImageUnderCursorByExplorer();      // カーソル下の画像をエクスプローラーで開く
                case CommandID.OpenImageUnderCursorByDefaultApp:  return new OpenImageUnderCursorByDefaultApp();    // カーソル下の画像を規定のプログラムで開く
                case CommandID.OpenImageUnderCursorByExternalApp: return new OpenImageUnderCursorByExternalApp();   // カーソル下の画像を外部プログラム名「[ ]」で開く
                case CommandID.CopyImageFileUnderCursor:          return new CopyImageFileUnderCursor();            // カーソル下の画像ファイルをコピー
                case CommandID.CopyImageDataUnderCursor:          return new CopyImageDataUnderCursor();            // カーソル下の画像データをコピー
                case CommandID.CopyImageFilePathUnderCursor:      return new CopyImageFilePathUnderCursor();        // カーソル下の画像ファイルパスをコピー
                case CommandID.CopyImageFileNameUnderCursor:      return new CopyImageFileNameUnderCursor();        // カーソル下の画像ファイル名をコピー
                case CommandID.LoadProfileFromNum:                return new LoadProfileFromNum();                  // プロファイル番号[ ](ProfileName)をロード
                case CommandID.LoadProfileFromName:               return new LoadProfileFromName();                 // プロファイル名「[ ]」をロード

                // 通常時
                case CommandID.ToggleSlideShowPlay:               return new ToggleSlideShowPlay();                 // スライドショー 再生/停止
                case CommandID.ZoomImageUnderCursor:              return new ZoomImageUnderCursor();                // カーソル下の画像を全体に拡大
                case CommandID.SlideToForward:                    return new SlideToForward();                      // 前方向にスライド
                case CommandID.SlideToBackward:                   return new SlideToBackward();                     // 後方向にスライド
                case CommandID.SlideToLeft:                       return new SlideToLeft();                         // 左方向にスライド
                case CommandID.SlideToTop:                        return new SlideToRight();                        // 上方向にスライド
                case CommandID.SlideToRight:                      return new SlideToTop();                          // 右方向にスライド
                case CommandID.SlideToBottom:                     return new SlideToBottom();                       // 下方向にスライド
                case CommandID.SlideToCursorDirection:            return new SlideToCursorDirection();              // カーソルのある方向へスライド
                case CommandID.SlideToCursorDirectionRev:         return new SlideToCursorDirectionRev();           // カーソルのある方向の逆方向へスライド
                case CommandID.SlideToBackwardByOneImage:         return new SlideToForwardByOneImage();            // 後方向に画像1枚分だけスライド
                case CommandID.SlideToForwardByOneImage:          return new SlideToBackwardByOneImage();           // 前方向に画像1枚分だけスライド
                case CommandID.ChangeSlideDirectionToLeft:        return new ChangeSlideDirectionToLeft();          // スライド方向を左に変更
                case CommandID.ChangeSlideDirectionToTop:         return new ChangeSlideDirectionToRight();         // スライド方向を上に変更
                case CommandID.ChangeSlideDirectionToRight:       return new ChangeSlideDirectionToTop();           // スライド方向を右に変更
                case CommandID.ChangeSlideDirectionToBottom:      return new ChangeSlideDirectionToBottom();        // スライド方向を下に変更
                case CommandID.ChangeSlideDirectionToRev:         return new ChangeSlideDirectionToRev();           // スライド方向を逆方向に変更
                case CommandID.ShiftForward:                      return new ShiftForward();                        // 画像[]枚分ずらし進める
                case CommandID.ShiftBackward:                     return new ShiftBackward();                       // 画像[]枚分ずらし戻す
                case CommandID.AddColumn:                         return new AddColumn();                           // 列数を[]増やす
                case CommandID.AddRow:                            return new AddRow();                              // 行数を[]増やす
                case CommandID.ReduceColumn:                      return new ReduceColumn();                        // 列数を[]減らす
                case CommandID.ReduceRow:                         return new ReduceRow();                           // 行数を[]減らす
                case CommandID.ChangeNumOfColumn:                 return new ChangeNumOfColumn();                   // 列数を[]に変更
                case CommandID.ChangeNumOfRow:                    return new ChangeNumOfRow();                      // 行数を[]に変更
                case CommandID.AddColumnAndRow:                   return new AddColumnAndRow();                     // 列数と行数を[]増やす
                case CommandID.ReduceColumnAndRow:                return new ReduceColumnAndRow();                  // 列数と行数を[]減らす
                case CommandID.ToggleTileImageStretch:            return new ToggleTileImageStretch();              // グリッド枠内への画像の収め方を切り替え
                case CommandID.OepnSubMenu_Load:                  return new OepnSubMenu_Load();                    // メニューの表示： 読み込み
                case CommandID.OepnSubMenu_AspectRatio:           return new OepnSubMenu_AspectRatio();             // メニューの表示： グリッドのアスペクト比
                case CommandID.OepnSubMenu_Profile:               return new OepnSubMenu_Profile();                 // メニューの表示： プロファイル

                // 画像拡大時
                case CommandID.ExitZoom:                          return new ExitZoom();                            // 画像の拡大をを終了
                case CommandID.ZoomInImage:                       return new ZoomInImage();                         // 画像の拡大率を[ ]%アップ
                case CommandID.ZoomOutImage:                      return new ZoomOutImage();                        // 画像の拡大率を[ ]%ダウン
                case CommandID.ZoomReset:                         return new ZoomReset();                           // 拡大率をリセット
                case CommandID.GoToForwardImage:                  return new GoToForwardImage();                    // []枚先の画像へ移動
                case CommandID.GoToBackwardImage:                 return new GoToBackwardImage();                   // []枚前の画像へ移動
                case CommandID.MoveZoomImageToLeft:               return new MoveZoomImageToLeft();                 // 画像を[]px左に移動
                case CommandID.MoveZoomImageToTop:                return new MoveZoomImageToRight();                // 画像を[]px上に移動
                case CommandID.MoveZoomImageToRight:              return new MoveZoomImageToTop();                  // 画像を[]px右に移動
                case CommandID.MoveZoomImageToBottom:             return new MoveZoomImageToBottom();               // 画像を[]px下に移動
                case CommandID.ToggleDisplayOfFileInfo:           return new ToggleDisplayOfFileInfo();             // ファイル情報の表示 ON/OFF

                default: return null;
            }
        }
    }
}
