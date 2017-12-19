using OMSSCADACommon.Commands;
using System;
using System.Runtime.Serialization;

namespace OMSSCADACommon.Commands
{
    [DataContract]
    [KnownType(typeof(WriteSingleDigital))]
    public abstract class Command
    {
        [IgnoreDataMember]
        public ICommandReceiver Receiver { get; set; }

        [DataMember]
        public string Id { get; set; }

        public abstract ResultMessage Execute();
    }
}
