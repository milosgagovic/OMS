using OMSSCADACommon;
using SCADA.RealtimeDatabase.Catalogs;
using System.Collections.Generic;

namespace SCADA.RealtimeDatabase.Model
{
    public class Digital : ProcessVariable
    {
        public DigitalDeviceClasses Class { get; set; }
        public List<CommandTypes> ValidCommands { get; set; }
        public List<States> ValidStates { get; set; }
        public CommandTypes Command { get; set; }
        public States State { get; set; }

        // to do: add previous command and stuff...

        public Digital()
        {
            this.Type = VariableTypes.DIGITAL;

            ValidCommands = new List<CommandTypes>();
            ValidStates = new List<States>();

            Class = DigitalDeviceClasses.SWITCH;
        }
    }
}
