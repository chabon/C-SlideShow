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
    [DataContract(Name = "KeyMap")]
    public class KeyMap
    {
        [DataMember]
        public KeyInput KeyInput { get; set; }

        [DataMember]
        public CommandID CommandID { get; set; }


        public KeyMap(KeyInput keyInput, CommandID commandId)
        {
            KeyInput = keyInput;
            CommandID = commandId;
        }

        public KeyMap Clone()
        {
            return new KeyMap(this.KeyInput, this.CommandID);
        }
    }
}
