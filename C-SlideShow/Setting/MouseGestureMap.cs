using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Windows.Input;

using C_SlideShow.Shortcut;

namespace C_SlideShow
{
    [DataContract(Name = "MouseGestureMap")]
    public class MouseGestureMap
    {
        [DataMember]
        public MouseGestureInput GestureInput { get; set; }

        [DataMember]
        public CommandID CommandID { get; set; }


        public MouseGestureMap(MouseGestureInput gestureInput, CommandID commandId)
        {
            GestureInput = gestureInput;
            CommandID = commandId;
        }

        public MouseGestureMap Clone()
        {
            return new MouseGestureMap(this.GestureInput, this.CommandID);
        }
    }
}
