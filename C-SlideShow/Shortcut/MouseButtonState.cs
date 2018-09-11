using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace C_SlideShow.Shortcut
{
    /// <summary>
    /// マウスボタンを押すときと離す時、どちらもウインドウ内でなければコマンドを発行しないこと
    /// マウスボタンを押している最中に他のインプット方法によってコマンドが実行された場合、離した時の単クリックによるコマンドはキャンセルすること
    /// の２つを管理するための値
    /// </summary>
    public class MouseButtonState
    {
        public MouseButton Button;
        public bool IsPressed;
        public bool CommandExecuted;

        public MouseButtonState(MouseButton button)
        {
            this.Button = button;
            this.Reset();
        }

        public void Reset()
        {
            this.IsPressed = false;
            this.CommandExecuted = true;
        }

        public void SetPress()
        {
            this.IsPressed = true;
            this.CommandExecuted = false;
        }
    }


    public class MouseButtonStateSet
    {
        public MouseButtonState L  = new MouseButtonState(MouseButton.Left);
        public MouseButtonState R  = new MouseButtonState(MouseButton.Right);
        public MouseButtonState M  = new MouseButtonState(MouseButton.Middle);
        public MouseButtonState X1 = new MouseButtonState(MouseButton.XButton1);
        public MouseButtonState X2 = new MouseButtonState(MouseButton.XButton2);

        public MouseButtonState GetState(MouseButton button)
        {
            switch( button )
            {
                case MouseButton.Left:      return L;
                case MouseButton.Right:     return R; 
                case MouseButton.Middle:    return M;
                case MouseButton.XButton1:  return X1;
                case MouseButton.XButton2:  return X2;
                default: return null;
            }
        }

        public void SetPress(MouseButton button)
        {
            MouseButtonState state = GetState(button);
            if(state != null) state.SetPress();
        }

        public void ResetAll()
        {

        }

        public void CommandExecuted()
        {
            L.CommandExecuted  = true;
            R.CommandExecuted  = true;
            M.CommandExecuted  = true;
            X1.CommandExecuted = true;
            X2.CommandExecuted = true;
        }

        public void CommandExecuted(MouseButton button)
        {
            MouseButtonState state = GetState(button);
            if( state != null ) state.CommandExecuted = true;
        }
    }
}
