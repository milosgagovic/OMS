using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OMSSCADACommon.Response
{
    [DataContract]
    [KnownType(typeof(AnalogVariable))]
    [KnownType(typeof(DigitalVariable))]
    [KnownType(typeof(CounterVariable))]
    public class Response
    {
        [DataMember]
        public List<ResponseVariable> Variables = null;

        public Response()
        {
            Variables = new List<ResponseVariable>();
        }
    }
}
