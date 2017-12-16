﻿using OMSSCADACommon;
using SCADA.RealtimeDatabase.Catalogs;
using System;
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

        public Digital()
        {
            this.type = VariableTypes.DIGITAL;
            ValidCommands = new List<CommandTypes>();
            ValidStates = new List<States>();
        }
    }
}
