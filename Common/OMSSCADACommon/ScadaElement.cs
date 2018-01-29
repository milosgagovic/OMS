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
        // za sada ce ovde sve biti digital...
        private DeviceTypes type;

        // name tog measurementa u cimxml-u. npr "MEAS_D_2"
        private string name;

        // za switch tu ide open, close...ni nema sad drugih komandi
        // iako je ideja da bude unknown i slicno...
        private List<CommandTypes> validCommands = new List<CommandTypes>();

        // validna stanja :) takodje su open i close u opticaju, iako je dodato
        // i unknown..poenta je sto ako ima tri stanja, u simulatoru moram da zauzmem
        // 2 bita u RUNTIME-u, a nisam sigurna da je to trenutno izvodljivo
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
