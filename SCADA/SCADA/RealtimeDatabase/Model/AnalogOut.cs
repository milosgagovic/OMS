﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    public class AnalogOut : ProcessVariable
    {
        public AnalogOut()
        {
            this.Type = VariableTypes.ANALOGOUT;
        }
    }
}