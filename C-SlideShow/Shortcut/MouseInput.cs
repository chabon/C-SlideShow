using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Windows.Input;



namespace C_SlideShow.Shortcut
{
    public enum MouseInputHold
    {
        None     = 0,
        L_Button = 1,
        R_Button = 2,
        M_Button = 4,
        XButton1 = 8,   // 戻るボタン
        XButton2 = 16,  // 進むボタン
        Shift    = 32,
        Ctrl     = 64, 
        Alt      = 128, 
    }

    public enum MouseInputClick
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
        public MouseInputHold   MouseInputHold   { get; set; }

        [DataMember]
        public MouseInputClick MouseInputButton { get; set; }

        public MouseInput()
        {
            MouseInputHold = MouseInputHold.None;
            MouseInputButton = MouseInputClick.None;
        }

        public MouseInput(MouseInputHold hold, MouseInputClick btn)
        {
            MouseInputHold = hold;
            MouseInputButton = btn;
        }

        public override string ToString()
        {
            string holdStr = "";

            // ホールドボタン
            if(  ( (int)MouseInputHold & (int)MouseInputHold.L_Button ) != 0  )
            {
                holdStr += "左ボタン + ";
            }
            if(  ( (int)MouseInputHold & (int)MouseInputHold.R_Button ) != 0  )
            {
                holdStr += "右ボタン + ";
            }
            if(  ( (int)MouseInputHold & (int)MouseInputHold.M_Button ) != 0  )
            {
                holdStr += "中ボタン + ";
            }
            if(  ( (int)MouseInputHold & (int)MouseInputHold.XButton1 ) != 0  )
            {
                holdStr += "戻るボタン + ";
            }
            if(  ( (int)MouseInputHold & (int)MouseInputHold.XButton2 ) != 0  )
            {
                holdStr += "進むボタン + ";
            }
            if(  ( (int)MouseInputHold & (int)MouseInputHold.Shift ) != 0  )
            {
                holdStr += "Shift + ";
            }
            if(  ( (int)MouseInputHold & (int)MouseInputHold.Ctrl ) != 0  )
            {
                holdStr += "Ctrl + ";
            }
            if(  ( (int)MouseInputHold & (int)MouseInputHold.Alt ) != 0  )
            {
                holdStr += "Alt + ";
            }

            // キー
            string buttonStr = "";

            switch( MouseInputButton )
            {
                case MouseInputClick.None:
                    return "";
                case MouseInputClick.L_Click:
                    buttonStr = "左クリック";
                    break;
                case MouseInputClick.R_Click:
                    buttonStr = "右クリック";
                    break;
                case MouseInputClick.M_Click:
                    buttonStr = "中クリック";
                    break;
                case MouseInputClick.WheelUp:
                    buttonStr = "Wheel Up";
                    break;
                case MouseInputClick.WheelDown:
                    buttonStr = "Wheel Down";
                    break;
                case MouseInputClick.X1_Click:
                    buttonStr = "戻るボタンクリック";
                    break;
                case MouseInputClick.X2_Click:
                    buttonStr = "進むボタンクリック";
                    break;
                case MouseInputClick.L_DoubleClick:
                    buttonStr = "左ダブルクリック";
                    break;
                case MouseInputClick.R_DoubleClick:
                    buttonStr = "右ダブルクリック";
                    break;
            }

            return holdStr + buttonStr;
        }

        public MouseInput Clone()
        {
            return new MouseInput(this.MouseInputHold, this.MouseInputButton);
        }

        /// <summary>
        /// Dictionary用比較処理
        /// </summary>
        public bool Equals(MouseInput other)
		{
            if( MouseInputHold == other.MouseInputHold && MouseInputButton == other.MouseInputButton )
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
			return ( (int)MouseInputHold | (int)MouseInputButton);
		}
    }
}
