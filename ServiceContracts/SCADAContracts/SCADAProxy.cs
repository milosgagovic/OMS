using OMSSCADACommon;
using OMSSCADACommon.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SCADAContracts
{
    public class SCADAProxy : DuplexChannelFactory<ISCADAContract>, ISCADAContract, IDisposable
    {
        private ISCADAContract factory;
        private ISCADAContract_Callback callback;

        public SCADAProxy(NetTcpBinding binding, EndpointAddress epAddress, ISCADAContract_Callback callbackMethods) : base(callbackMethods, binding, epAddress)
        {
            this.callback = callbackMethods;
            factory = this.CreateChannel();
        }

        public void CheckIn()
        {
            try
            {
                factory.CheckIn();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public ResultMessage ExecuteCommand(Command command)
        {
            try
            {
                return factory.ExecuteCommand(command);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ResultMessage.INTERNAL_SERVER_ERROR;
            }
        }
    }
}
