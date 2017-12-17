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
    public class SCADAProxy : ChannelFactory<ISCADAContract>, ISCADAContract
    {
        private ISCADAContract factory;

        public ISCADAContract Factory
        {
            get
            {
                return factory;
            }

            set
            {
                factory = value;
            }
        }
        public SCADAProxy() { }

        public SCADAProxy(NetTcpBinding binding, string address)
            : base(binding, address)
        {
            this.Factory = this.CreateChannel();
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
