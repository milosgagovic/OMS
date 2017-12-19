using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCCommon
{
    public interface IIndustryProtocolHandler
    {
        IndustryProtocols ProtocolType { get; set; }

        // zamisljeno je da se ova metoda poziva nezavisno od toga koji industrijski protokol koristimo
        // znaci u njoj obezbediti podatke za formiranje poruke koji su nezavisni od protokola 
        byte[] PackData();

        void UnpackData(byte[] data);

        // mozda ovde da budu metode send data, i receive data
        // kao npr sto na TCPClient klasi imamo metode za slanje i primanje
        // tako i ovde napraviti.... 
       
    }
}
