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
        MouseInputHold   MouseInputHold   { get; set; } = MouseInputHold.None;
        MouseInputButton MouseInputButton { get; set; } = MouseInputButton.None;

        public MouseInput(MouseInputHold hold, MouseInputButton btn)
        {
            MouseInputHold = hold;
            MouseInputButton = btn;
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
