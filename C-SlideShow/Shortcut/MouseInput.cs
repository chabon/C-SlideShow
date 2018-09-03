using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Windows.Input;



namespace C_SlideShow.Shortcut
{
    public enum MouseInputButton
    {
        None,
        L_Click,
        R_Click,
        M_Click,
        WheelUp,
        WheelDown, 
        X1_Click,
        X2_Click, 
        L_DoubleClick,
        R_DoubleClick,
    }


    [DataContract(Name = "MouseInput")]
    public class MouseInput : IEquatable<MouseInput>
    {
        [DataMember]
        public MouseInputButton MouseInputButton { get; set; }

        [DataMember]
        public ModifierKeys ModifierKeys;



        public MouseInput()
        {
            MouseInputButton = MouseInputButton.None;
            ModifierKeys = ModifierKeys.None;
        }

        public MouseInput(MouseInputButton btn, ModifierKeys modifierKeys)
        {
            MouseInputButton = btn;
            ModifierKeys = modifierKeys;
        }

        public override string ToString()
        {
            string holdStr = "";

            // 修飾キー
            if(  ( (int)ModifierKeys & (int)ModifierKeys.Control ) != 0  )
            {
                holdStr += "Ctrl + ";
            }
            if(  ( (int)ModifierKeys & (int)ModifierKeys.Shift ) != 0  )
            {
                holdStr += "Shift + ";
            }
            if(  ( (int)ModifierKeys & (int)ModifierKeys.Alt ) != 0  )
            {
                holdStr += "Alt + ";
            }

            // マウスボタン
            string buttonStr = "";

            switch( MouseInputButton )
            {
                case MouseInputButton.None:
                    return "";
                case MouseInputButton.L_Click:
                    buttonStr = "左クリック";
                    break;
                case MouseInputButton.R_Click:
                    buttonStr = "右クリック";
                    break;
                case MouseInputButton.M_Click:
                    buttonStr = "中クリック";
                    break;
                case MouseInputButton.WheelUp:
                    buttonStr = "Wheel Up";
                    break;
                case MouseInputButton.WheelDown:
                    buttonStr = "Wheel Down";
                    break;
                case MouseInputButton.X1_Click:
                    buttonStr = "戻るボタン";
                    break;
                case MouseInputButton.X2_Click:
                    buttonStr = "進むボタン";
                    break;
                case MouseInputButton.L_DoubleClick:
                    buttonStr = "左ダブルクリック";
                    break;
                case MouseInputButton.R_DoubleClick:
                    buttonStr = "右ダブルクリック";
                    break;
            }

            return holdStr + buttonStr;
        }

        public MouseInput Clone()
        {
            return new MouseInput(this.MouseInputButton, this.ModifierKeys);
        }

        /// <summary>
        /// Dictionary用比較処理
        /// </summary>
        public bool Equals(MouseInput other)
        {
            if( MouseInputButton == other.MouseInputButton && ModifierKeys == other.ModifierKeys)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Dictionary用比較処理
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((MouseInput)obj);
        }

        /// <summary>
        /// Dictionary用比較処理
        /// </summary>
        public override int GetHashCode()
        {
            return ( (int)MouseInputButton | (int)ModifierKeys);
        }
    }
}
