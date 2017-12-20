﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    public class Counter : ProcessVariable
    {
        public Counter()
        {
            this.Type = VariableTypes.COUNTER;
        }

        public int Value { get; set; }
    }
}
