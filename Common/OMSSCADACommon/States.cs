﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMSSCADACommon
{
    public enum States
    {
        OPENED = 0,
        CLOSED,
        UNKNOWN
    }

    public enum CrewResponse
    {
        ShortCircuit = 0,
        GroundFault,
        Overload,

    }
}
