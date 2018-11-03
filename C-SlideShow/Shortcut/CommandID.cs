using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut
{
    public enum CommandID
    {
        // 全般
        OpenFolder,                         // フォルダを開く
        OpenAdditionalFolder,               // フォルダ追加読み込み
        OpenFile,                           // ファイルを開く
        OpenAdditionalFile,                 // ファイル追加読み込み
        Reload,                             // 再読込み
        ToggleTopMost,                      // 常に最前面表示 ON/OFF
        ToggleFullScreen,                   // フルスクリーンモード ON/OFF
        WindowSizeUp,                       // ウインドウサイズを[ ]%大きく
        WindowSizeDown,                     // ウインドウサイズを[ ]%小さく
        ShowContextMenu,                    // コンテキストメニューを表示
        OpenFolderByExplorer,               // エクスプローラでフォルダを開く
        OpenNextArchiver,                   // 次のフォルダ(書庫)を開く
        OpenPrevArchiver,                   // 前のフォルダ(書庫)を開く
        ShowAppSettingDialog,               // アプリの設定ダイアログを表示
        CloseFile,                          // ファイルを閉じる
        MinimizeWindow,                     // ウインドウを最小化
        ExitApp,                            // アプリケーションを終了
        OpenImageUnderCursorByExplorer,     // カーソル下の画像をエクスプローラーで開く
        OpenImageUnderCursorByDefaultApp,   // カーソル下の画像を規定のプログラムで開く
        OpenImageUnderCursorByExternalApp,  // カーソル下の画像を外部プログラム名「[ ]」で開く
        CopyImageFileUnderCursor,           // カーソル下の画像ファイルをコピー
        CopyImageDataUnderCursor,           // カーソル下の画像データをコピー
        CopyImageFilePathUnderCursor,       // カーソル下の画像ファイルパスをコピー
        CopyImageFileNameUnderCursor,       // カーソル下の画像ファイル名をコピー
        LoadProfileFromNum,                 // プロファイル番号[ ](ProfileName)をロード
        LoadProfileFromName,                // プロファイル名「[ ]」をロード

        // 通常時
        ToggleSlideShowPlay,                // スライドショー 再生/停止
        ZoomImageUnderCursor,               // カーソル下の画像を全体に拡大
        SlideToForward,                     // 前方向にスライド
        SlideToBackward,                    // 後方向にスライド
        SlideToLeft,                        // 左方向にスライド
        SlideToTop,                         // 上方向にスライド
        SlideToRight,                       // 右方向にスライド
        SlideToBottom,                      // 下方向にスライド
        SlideToCursorDirection,             // カーソルのある方向へスライド
        SlideToCursorDirectionRev,          // カーソルのある方向の逆方向へスライド
        SlideToBackwardByOneImage,          // 後方向に画像1枚分だけスライド
        SlideToForwardByOneImage,           // 前方向に画像1枚分だけスライド
        ChangeSlideDirectionToLeft,         // スライド方向を左に変更
        ChangeSlideDirectionToTop,          // スライド方向を上に変更
        ChangeSlideDirectionToRight,        // スライド方向を右に変更
        ChangeSlideDirectionToBottom,       // スライド方向を下に変更
        ChangeSlideDirectionToRev,          // スライド方向を逆方向に変更
        ShiftForward,                       // 画像[]枚分ずらし進める
        ShiftBackward,                      // 画像[]枚分ずらし戻す
        AddColumn,                          // 列数を[]増やす
        AddRow,                             // 行数を[]増やす
        ReduceColumn,                       // 列数を[]減らす
        ReduceRow,                          // 行数を[]減らす
        AddColumnAndRow,                    // 列数と行数を[]増やす
        ReduceColumnAndRow,                 // 列数と行数を[]減らす
        ChangeNumOfColumn,                  // 列数を[]に変更
        ChangeNumOfRow,                     // 行数を[]に変更
        ToggleTileImageStretch,             // グリッド枠内への画像の収め方を切り替え
        OepnSubMenu_Load,                   // メニューの表示： 読み込み
        OepnSubMenu_AspectRatio,            // メニューの表示： グリッドのアスペクト比
        OepnSubMenu_Profile,                // メニューの表示： プロファイル

        // 画像拡大時
        ExitZoom,                           // 画像の拡大をを終了
        ZoomInImage,                        // 画像の拡大率を[ ]%アップ
        ZoomOutImage,                       // 画像の拡大率を[ ]%ダウン
        ZoomReset,                          // 拡大率をリセット
        GoToForwardImage,                   // []枚先の画像へ移動
        GoToBackwardImage,                  // []枚前の画像へ移動
        MoveZoomImageToLeft,                // 画像を[]px左に移動
        MoveZoomImageToTop,                 // 画像を[]px上に移動
        MoveZoomImageToRight,               // 画像を[]px右に移動
        MoveZoomImageToBottom,              // 画像を[]px下に移動
        ToggleDisplayOfFileInfo,            // ファイル情報の表示 ON/OFF
    }

}
