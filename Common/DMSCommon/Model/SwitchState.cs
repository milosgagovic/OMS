using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DMSCommon.Model
{
    [DataContract]
    public enum SwitchState
    {
        [EnumMember]
        Open = 1,
        [EnumMember]
        Closed = 0
    }
}
