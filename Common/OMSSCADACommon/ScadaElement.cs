using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OMSSCADACommon
{
    [Serializable]
    [DataContract]
    public class ScadaElement
    {
        private DeviceTypes type;

        private string name;

        private List<CommandTypes> validCommands = new List<CommandTypes>();

        private List<States> validStates = new List<States>();

        [DataMember]
        public DeviceTypes Type
        {
            get { return type; }
            set { type = value; }
        }

        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMember]
        public List<CommandTypes> ValidCommands
        {
            get { return validCommands; }
            set { validCommands = value; }
        }

        [DataMember]
        public List<States> ValidStates
        {
            get { return validStates; }
            set { validStates = value; }
        }
    }
}
