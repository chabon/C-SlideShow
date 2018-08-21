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
    [DataContract(Name = "MouseInputMap")]
    public class MouseInputMap
    {
        [DataMember]
        public MouseInput MouseInput { get; set; }

        [DataMember]
        public CommandID CommandID { get; set; }


        public MouseInputMap(MouseInput mouseInput, CommandID commandId)
        {
            MouseInput = mouseInput;
            CommandID = commandId;
        }

        public MouseInputMap Clone()
        {
            return new MouseInputMap(this.MouseInput, this.CommandID);
        }
    }
}
