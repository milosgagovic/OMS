using System.Runtime.Serialization;

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
