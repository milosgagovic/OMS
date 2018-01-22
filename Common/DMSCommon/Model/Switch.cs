using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DMSCommon.Model
{
    [DataContract]
    public class Switch:Branch
    {
        [EnumMember]
        private SwitchState _state;


        public SwitchState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public Switch() { }

        public Switch(long gid) : base(gid)
        {
            State = SwitchState.Closed;
        }
        public Switch(long gid, string mrid) : base(gid, mrid)
        {
            State = SwitchState.Closed;

        }



    }
}
