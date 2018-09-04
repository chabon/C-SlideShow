using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace C_SlideShow
{
    public class ExifInfo
    {
        // 静的で空の C_SlideShow.ExifInfo を表す値を取得します。
        public static ExifInfo Empty { get; }

        public Rotation         Rotation        = Rotation.Rotate0; // 回転情報
        public ScaleTransform   ScaleTransform  = null;             // 反転情報
        public DateTimeOffset?  DateTaken       = null;             // 撮影日時
        public string           CameraMaker     = null;             // カメラメーカー
        public string           CameraModel     = null;             // カメラモデル
        public string           Software        = null;             // ソフトウェア

        public ExifInfo()
        {

        }
    }
}
