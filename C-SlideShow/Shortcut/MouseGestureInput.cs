using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Windows.Input;


namespace C_SlideShow.Shortcut
{
    /// <summary>
    /// マウスジェスチャ入力情報
    /// </summary>
    [DataContract(Name = "MouseGestureInput")]
    public class MouseGestureInput : IEquatable<MouseGestureInput>
    {
        /// <summary>
        /// 始動ボタン
        /// </summary>
        [DataMember]
        public MouseButton StartingButton;

        /// <summary>
        /// ストローク
        /// </summary>
        [DataMember]
        public string Stroke;

        public MouseGestureInput(MouseButton startingBtn, string stroke)
        {
            this.StartingButton = startingBtn;
            this.Stroke = stroke;
        }

        public MouseGestureInput Clone()
        {
            return new MouseGestureInput(this.StartingButton, this.Stroke);
        }

        public override string ToString()
        {
            string start = "";

            switch( StartingButton )
            {
                case MouseButton.Left:
                    start = "(L)";
                    break;
                case MouseButton.Right:
                    start = "(R)";
                    break;
                case MouseButton.Middle:
                    start = "(M)";
                    break;
                case MouseButton.XButton1:
                    start = "(X1)";
                    break;
                case MouseButton.XButton2:
                    start = "(X2)";
                    break;
            }

            return start + " " + Stroke;
        }

        /// <summary>
        /// Dictionary用比較処理
        /// </summary>
        public bool Equals(MouseGestureInput other)
        {
            if( (StartingButton.Equals(other.StartingButton)) && (Stroke == other.Stroke) )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Dictionary用比較処理
        /// </summary>
        public override bool Equals(object obj)
        {
            if( obj.GetType() != this.GetType() ) return false;
            return this.Equals((MouseGestureInput)obj);
        }

        /// <summary>
        /// Dictionary用比較処理
        /// </summary>
        public override int GetHashCode()
        {
            return (int)StartingButton + int.Parse(Stroke);
        }
    }
}
