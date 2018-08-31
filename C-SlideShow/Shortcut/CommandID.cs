using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_SlideShow.Shortcut
{
    public enum CommandID
    {
        None,                           // 無し

        // 全般
        OpenFolder,                     // フォルダを開く
        OpenAdditionalFolder,           // フォルダ追加読み込み
        OpenFile,                       // ファイルを開く
        OpenAdditionalFile,             // ファイル追加読み込み
        WindowSizeUp,                   // ウインドウサイズを[ ]%大きく
        WindowSizeDown,                 // ウインドウサイズを[ ]%小さく
        LoadProfileFromNum,             // プロファイル番号[ ](ProfileName)をロード
        LoadProfileFromName,            // プロファイル名「[ ]」をロード

        // 通常時
        SlideToForward,                 // 前方向にスライド
        SlideToBackward,                // 後方向にスライド
        SlideToBackwardByOneImage,      // 後方向に画像1枚分だけスライド
        SlideToForwardByOneImage,       // 前方向に画像1枚分だけスライド
        SlideToLeft,                    // 左方向にスライド
        SlideToTop,                     // 上方向にスライド
        SlideToRight,                   // 右方向にスライド
        SlideToBottom,                  // 下方向にスライド
        ZoomImageUnderCursor,           // カーソル下の画像を全体に拡大

        // 画像拡大時
        ZoomInImage,                    // 画像の拡大率を[ ]%アップ
        ZoomOutImage,                   // 画像の拡大率を[ ]%ダウン
        ExitZoom,                       // 画像の拡大をを終了
    }
}
