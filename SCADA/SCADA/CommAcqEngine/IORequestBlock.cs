using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCADA.RealtimeDatabase.Model;

namespace SCADA.CommAcqEngine
{

    enum RequestType
    {
        SEND_RECV = 0,
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
     * IZMESTITI U PCCommon
     */
    public class IORequestBlock
    {
        // transaction type
        RequestType ioRequestType;

        // channel identification -> msm da ne treba, preko RTU zna kom channelu pristupa

        // target rtu
        public RTU Rtu { get; set; }

        // ovo ce kanda string biti
        public int RtuId { get; set; }

        /* request parameters*/

        // max number of request repeating
        public int MaxRepeat { get; set; }

        // requestType -> nisam sigurna da ovo treba

        public int sendMsgLength { get; set; }

        // number of send characters
        public int sendCount { get; set; }

        // trsciever buffer 
        public Byte[] sendBuff { get; set; }


        /* reply parameters*/
        // replyType ?

        public int rcvMsgLength { get; set; }

        // number of received chahacters
        public int rcvCount { get; set; }

        // receiver buffer 
        public Byte[] rcvBuff { get; set; }

    }
}
