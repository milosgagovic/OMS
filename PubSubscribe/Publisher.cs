using DMSCommon;
using IMSContract;
using OMSSCADACommon;
using PubSubContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PubSubscribe
{
    /// <summary>
    /// Client for Publishing service
    /// </summary>
    public class Publisher
    {
        IPublishing proxy;

        public Publisher()
        {
            CreateProxy();
        }

        public void PublishUpdateDigital(List<UIUpdateModel> update)
        {
            try
            {
                proxy.PublishDigitalUpdate(update);
            }
            catch { }
        }

        public void PublishUpdateAnalog(List<UIUpdateModel> update)
        {
            try
            {
                proxy.PublishAnalogUpdate(update);
            }
            catch { }
        }

        // not used
        public void PublishCrew(UIUpdateModel update)
        {
            try
            {
                proxy.PublishCrewUpdate(update);
            }
            catch { }
        }

        public void PublishIncident(IncidentReport report)
        {
            try
            {
                proxy.PublishIncident(report);
            }
            catch { }
        }

        public void PublishCallIncident(UIUpdateModel call)
        {
            try
            {
                proxy.PublishCallIncident(call);
            }
            catch { }
        }

        public void PublishUIBreaker(bool isIncident,long incidentBreaker)
        {
            try
            {
                proxy.PublishUIBreakers(isIncident, incidentBreaker);
            }
            catch { }
        }

        private void CreateProxy()
        {
            string address = "";
            try
            {
                address = "net.tcp://localhost:7001/Pub";
                EndpointAddress endpointAddress = new EndpointAddress(address);
                NetTcpBinding binding = new NetTcpBinding();
                binding.CloseTimeout = TimeSpan.FromMinutes(10);
                binding.OpenTimeout = TimeSpan.FromMinutes(10);
                binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                binding.SendTimeout = TimeSpan.FromMinutes(10);
                binding.MaxReceivedMessageSize = Int32.MaxValue;
                proxy = ChannelFactory<IPublishing>.CreateChannel(binding, endpointAddress);
            }
            catch (Exception e)
            {
                throw e;
                //TODO log error;
            }

        }
    }
}
