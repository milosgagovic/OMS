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

        // register address
        public ushort ReqAddress { get; set; }
         
        // target slave device Id - RTU address
        public string RtuName { get; set; }


        /* request parameters*/

        // public int MaxRepeat { get; set; }

        public int SendMsgLength { get; set; }

        // trsciever buffer 
        public Byte[] SendBuff { get; set; }



        /* reply parameters*/

        public int RcvMsgLength { get; set; }

        // receiver buffer 
        public Byte[] RcvBuff { get; set; }

    }
}
