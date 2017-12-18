using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCCommon;

namespace ModbusTCPDriver
{
    // Concrete protocol handler class
    public class ModbusHandler : IIndustryProtocolHandler
    {
        public IndustryProtocols ProtocolType
        {
            get => ProtocolType;
            set => ProtocolType = IndustryProtocols.Modbus;
        }

        // Modbus data format:
        //  MBAP (16+16+16+8) + Function (8) + Data (n x 8 bit)

        // slave adress je sadrzana u RTU odnosno IORBu, function takodje u IORBu
        public void PackData(ushort transId, byte[] data)
        {
            ModbusApplicationHeader mbap = new ModbusApplicationHeader();

            mbap.ProtocolId = (ushort)ProtocolType;

            /*
             mozda ovde na nivou modbus handlera da vodimo racuna o transaction Id-u, ako se broj uvecava sa bilo kojom
             transakcijom, nebitno kom kontroleru je prosledjena.
             ili da imamo na nivou kontrolera polje...
             */
            mbap.TransactionId = transId;


        }

        // ovo se sve poziva iz akvizicionog taska. ili se samo pack data pozove, pa on sve ovo odradi...

        // 0x04 ili 0x03
        // citanje analognih izlaza
        private void ReadHoldingRegisterRequest()
        {

        }

        //0x01
        // citanje digitalnih izlaza
        private void ReadCoilsRequest()
        {

        }

        // 0x02 mi fali, i 0x03/04

        //0x06
        // pisanje analognih izlaza
        private void WriteSingleRegisterRequest()
        {

        }

        //0x05
        // pisanje digitalnih izlaza
        private void WriteSingleCoilRequest()
        {

        }

        // 0x10 (16) mi fali
    }
}
