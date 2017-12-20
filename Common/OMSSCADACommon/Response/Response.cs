using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMSSCADACommon.Response
{
    public class Response
    {
        public List<ResponseVariable> Variables = null;

        public Response()
        {
            Variables = new List<ResponseVariable>();
        }
    }
}
