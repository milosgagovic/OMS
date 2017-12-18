using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCCommon;

namespace ModbusTCPDriver
{
    public class ModbusApplicationHeader
    {
        // initialized by the client, copied by the server from request to the response
        // Identification of a MODBUS request/response transaction

        // da li je ovaj broj transakcije vezan za sve transakcije u sistemu,
        // ili za 1 odredjeni rtu? verovatno za taj jedan?
        public ushort TransactionId { get; set; }

        // initialized by the client, copied by the server from request to the response
        // 0 = MODBUS protocol
        public ushort ProtocolId { get; set; }

        // initialized by the client (request), initialized by the server (response)
        // number of following bytes
        public ushort Length { get; set; }

        // initialized by the client (request), copied by the server from request to the response
        // identification of a remote slave
        public Byte DeviceAddress { get; set; }
    }
}
