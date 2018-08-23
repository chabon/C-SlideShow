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
        public string Gesture { get; set; }

        [DataMember]
        public CommandID CommandID { get; set; }


        public MouseGestureMap(string gesture, CommandID commandId)
        {
            Gesture = gesture;
            CommandID = commandId;
        }

        public MouseGestureMap Clone()
        {
            return new MouseGestureMap(this.Gesture, this.CommandID);
        }
    }
}
