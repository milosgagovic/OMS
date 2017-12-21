using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCCommon
{
    public enum RequestType
    {
        SEND_RECV = 0, // used for unbalanced protocols
        RECV_SEND,
        SEND,
        RECV,
        CONNECT,
        DISCONNECT
    }

    /* 
     * Communication/Acquisition request description
     * 
     * Request defines single (or broadcast) communication transaction
     * between SCADA sw and process controller(s).    
     * 
     */
    public class IORequestBlock
    {   
        // transaction type.
        public RequestType RequestType { get; set; }

        // target slave device Id - RTU address
        public short RTUAddress { get; set; }


        // target slave device Id - RTU address
        // neka bude tipa string, ko zna kakve sve adrese mogu imati 
        // razliciti end device-ovi :) 
        public string RtuName{ get; set; }

        /* request parameters*/

        // max number of request repeating
        public int MaxRepeat { get; set; }

        public int SendMsgLength { get; set; }

        // number of send characters
        public int SendCount { get; set; }

        // trsciever buffer 
        public Byte[] SendBuff { get; set; }

        /* reply parameters*/

        public int RcvMsgLength { get; set; }

        // number of received chahacters
        public int RcvCount { get; set; }

        // receiver buffer 
        public Byte[] RcvBuff { get; set; }

    }
}
