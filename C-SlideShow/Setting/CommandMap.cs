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
    [DataContract(Name = "CommandMap")]
    public class CommandMap
    {
        [DataMember]
        public CommandID CommandID { get; set; }

        [DataMember]
        public int CommandValue { get; set; }

        [DataMember]
        public string CommandStrValue { get; set; }

        [DataMember]
        public KeyInput KeyInput { get; set; }

        [DataMember]
        public MouseInput MouseInput { get; set; }

        [DataMember]
        public MouseGestureInput MouseGestureInput { get; set; }


        public CommandMap(CommandID commandId, int commandValue, string commandStrValue, KeyInput keyInput, MouseInput mouseInput, MouseGestureInput mouseGestureInput)
        {
            CommandID = commandId;
            CommandValue = commandValue;
            CommandStrValue = commandStrValue;

            KeyInput = keyInput;
            MouseInput = mouseInput;
            MouseGestureInput = mouseGestureInput;
        }

        public CommandMap Clone()
        {
            return new CommandMap( CommandID, CommandValue, CommandStrValue, KeyInput.Clone(), MouseInput.Clone(), MouseGestureInput.Clone() );
        }
    }
}
