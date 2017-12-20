using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCCommon;

namespace ModbusTCPDriver
{
    [Serializable]
    public class ModbusApplicationHeader
    {
        // initialized by the client, copied by the server from request to the response
        // Identification of a MODBUS request/response transaction

        // fiksiramo na 0
        public ushort TransactionId
        {
            //get { return TransactionId; }
            //set
            //{
            //    byte[] transId = BitConverter.GetBytes(value);
            //    Array.Reverse(transId);
            //    value = Convert.ToUInt16(transId);
            //}
            get;set;
        }

        // initialized by the client, copied by the server from request to the response
        // 0 = ModbuTCP protocol
        public ushort ProtocolId
        {
            //get { return ProtocolId; }
            //set
            //{
            //    byte[] protId = BitConverter.GetBytes(value);
            //    Array.Reverse(protId);
            //    value = Convert.ToUInt16(protId);
            //}
            get;set;
        }

        // initialized by the client (request), initialized by the server (response)
        // number of following bytes
        // broj bajtova iza zaglavlja -> bez adrese uracunate
        public ushort Length
        {
            //get { return Length; }
            //set
            //{
            //    byte[] length = BitConverter.GetBytes(value);
            //    Array.Reverse(length);
            //    value = Convert.ToUInt16(length);
            //}
            get;set;
        }

        // initialized by the client (request), copied by the server from request to the response
        // identification of a remote slave
        public Byte DeviceAddress { get; set; }

        public byte[] getByteHeader()
        {
            byte[] transId = BitConverter.GetBytes(TransactionId);
            byte[] protocolId = BitConverter.GetBytes(ProtocolId);
            byte[] length = BitConverter.GetBytes(Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(transId);
                Array.Reverse(protocolId);
                Array.Reverse(length);
            }

            byte[] byteHeader = new byte[7]
            {
                transId[0],transId[1],
                protocolId[0], protocolId[1],
                length[0],length[1],
                DeviceAddress
            };

            return byteHeader;
        }
    }
}
