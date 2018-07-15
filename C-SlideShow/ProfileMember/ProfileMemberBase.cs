using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;


namespace C_SlideShow.ProfileMember
{
    public abstract class ProfileMemberBase : IProfileMember
    {
        public int Value { get; set; }

        public bool IsEnabled { get; set; }

        // virtual
        public virtual string TooltipStr { get { return "this is profile member base class"; } }
    }
}
