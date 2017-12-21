using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using PCCommon;

namespace ModbusTCPDriver
{
    // Concrete protocol handler class
    public class ModbusHandler : IIndustryProtocolHandler
    {
        public IndustryProtocols ProtocolType { get; set; }
        public ModbusApplicationHeader Header { get; set; }
        public Request Request { get; set; }
        public Response Response { get; set; }


        public byte[] PackData()
        {
            // ne koristi se
            //ModbusRequestMessage mrm = new ModbusRequestMessage()
            //{
            //    Header = this.Header,
            //    Request = this.Request
            //};

            // message must be in big endian format

            var bHeader = Header.getByteHeader();
            var bRequest = Request.getByteRequest();

            byte[] packedData = new byte[bHeader.Length + bRequest.Length];

            bHeader.CopyTo(packedData, 0);
            bRequest.CopyTo(packedData, bHeader.Length);
            return packedData;
        }

        public void UnpackData(byte[] data, int length)
        {
            
            var mbap = Header.getObjectHeader(data); // nepotrebno za sada?

            byte[] responseData = new byte[length - 7];
            Buffer.BlockCopy(data, 7, responseData, 0, length - 7);

            switch ((FunctionCodes)responseData[0])
            {
                case FunctionCodes.WriteSingleCoil:
                case FunctionCodes.WriteSingleRegister:
                    Response = new WriteResponse();
                    Response.getObjectResponse(responseData);
                        break;
                case FunctionCodes.ReadCoils:
                case FunctionCodes.ReadDiscreteInput:
                case FunctionCodes.ReadHoldingRegisters:
                case FunctionCodes.ReadInputRegisters:
                    Response = new ReadResponse();
                    Response.getObjectResponse(responseData);
                    break;
                default:
                    break;
            }

            Response.getObjectResponse(responseData);
        }


        /*

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

        */
    }
}
