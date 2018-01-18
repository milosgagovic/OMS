using DMSCommon.Model;
using FTN.Common;
using PubSubContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PubSubscribe
{
    public class Publisher
    {
        IPublishing proxy;

        public Publisher()
        {
            CreateProxy();
        }

        public void PublishUpdate(SCADAUpdateModel update)
        {
            proxy.Publish(update);
        }

        private void CreateProxy()
        {
            string address = "";
            try
            {
                address = "net.tcp://localhost:7001/Pub";
                EndpointAddress endpointAddress = new EndpointAddress(address);
                NetTcpBinding netTcpBinding = new NetTcpBinding();
                proxy = ChannelFactory<IPublishing>.CreateChannel(netTcpBinding, endpointAddress);
            }
            catch (Exception e)
            {
                throw e;
                //TODO log error;
            }

        }
    }
}
