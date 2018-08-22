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
        L_Click  = 1,
        R_Click  = 2,
        M_Click  = 4,
        Shift    = 8,
        Ctrl     = 16, 
        Alt      = 32, 
    }

    public enum MouseInputButton
    {
        None,
        L_Click,
        R_Click,
        M_Click,
        WheelUp,
        WheelDown, 
        L_DoubleClick,
        R_DoubleClick,
    }


    [DataContract(Name = "MouseInput")]
    public class MouseInput : IEquatable<MouseInput>
    {
        [DataMember]
        public MouseInputHold   MouseInputHold   { get; set; } = MouseInputHold.None;

        [DataMember]
        public MouseInputButton MouseInputButton { get; set; } = MouseInputButton.None;

        public MouseInput()
        {
            MouseInputHold = MouseInputHold.None;
            MouseInputButton = MouseInputButton.None;
        }

        public MouseInput(MouseInputHold hold, MouseInputButton btn)
        {
            MouseInputHold = hold;
            MouseInputButton = btn;
        }

        public override string ToString()
        {
            string holdStr = "";

            // ホールドボタン
            if(  ( (int)MouseInputHold & (int)MouseInputHold.L_Click ) != 0  )
            {
                holdStr += "左クリック + ";
            }
            if(  ( (int)MouseInputHold & (int)MouseInputHold.R_Click ) != 0  )
            {
                holdStr += "右クリック + ";
            }
            if(  ( (int)MouseInputHold & (int)MouseInputHold.M_Click ) != 0  )
            {
                holdStr += "中クリック + ";
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
