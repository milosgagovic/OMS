﻿using System;
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
            ModbusRequestMessage mrm = new ModbusRequestMessage()
            {
                Header = this.Header,
                Request = this.Request
            };

            // message must be in big endian format

            // mbap
            //Console.WriteLine("     Header ->");
            //Console.WriteLine(BitConverter.ToString(Header.getByteHeader()));
            //Console.WriteLine("     Request ->");
            //Console.WriteLine(BitConverter.ToString(Request.getByteRequest()));

            var bHeader = Header.getByteHeader();
            var bRequest = Request.getByteRequest();

            byte[] packedData = new byte[bHeader.Length + bRequest.Length];

            //BinaryFormatter bf = new BinaryFormatter();
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    bf.Serialize(ms, mrm);
            //    return ms.ToArray();
            //}

            bHeader.CopyTo(packedData, 0);
            bRequest.CopyTo(packedData, Header.Length);
            return packedData;
        }

        public void UnpackData(byte[] data)
        {
            // obrnuto
            throw new NotImplementedException();
        }


        //public IndustryProtocols ProtocolType
        //{
        //    get => ProtocolType;
        //    set => ProtocolType = IndustryProtocols.ModbusTCP;
        //}

        // ModbusTCP data format:
        //  MBAP (16+16+16+8) + Function (8) + Data (n x 8 bit)

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
