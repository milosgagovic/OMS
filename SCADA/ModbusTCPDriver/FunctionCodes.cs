using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    public enum FunctionCodes : Byte
    {
        ReadCoils = 1,          // digital output
        ReadDiscreteInput,      // digital input
        ReadHoldingRegisters,   // analog output
        ReadInputRegisters,     // analog input
        WriteSingleCoil,        // digital output
        WriteSingleRegister,    // analog output

        WriteMultipleRegister = 16, // add support later
        Undefined // ? maybe
    }
}
