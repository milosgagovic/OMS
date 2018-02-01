using OMSSCADACommon;
using SCADA.RealtimeDatabase.Catalogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    public class Digital : ProcessVariable
    {
        public DigitalDeviceClasses Class { get; set; }
        public List<CommandTypes> ValidCommands { get; set; }
        public List<States> ValidStates { get; set; }
        public CommandTypes Command { get; set; }
        public States State { get; set; }

        // to do: setovati command. nema smisla bas za neki <ozbiljniji> rad kasnije...sada moze ovako
        public Digital()
        {
            this.Type = VariableTypes.DIGITAL;

            ValidCommands = new List<CommandTypes>();
            ValidStates = new List<States>();

            // kad dodamo uglavnom ga mapiramo na modbus 0, tj. open...
            // PROVERITI
            //Command = CommandTypes.OPEN;

            // i ovo za sada ostaje ovako
            Class = DigitalDeviceClasses.SWITCH;
        }
    }
}
