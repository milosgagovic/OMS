﻿using DMSCommon.Model;
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
    public delegate void PublishUpdateEvent(SCADAUpdateModel update);
    public class Subscriber : IPublishing
    {
        ISubscription proxy = null;

        public event PublishUpdateEvent publishUpdateEvent;
        public Subscriber()
        {
            CreateProxy();
        }

        private void CreateProxy()
        {
            try
            {  //***git
                NetTcpBinding netTcpbinding = new NetTcpBinding();
                EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:7002/Sub");
                InstanceContext instanceContext = new InstanceContext(this);
                DuplexChannelFactory<ISubscription> channelFactory = new DuplexChannelFactory<ISubscription>(instanceContext, netTcpbinding, endpointAddress);
                proxy = channelFactory.CreateChannel();
            }
            catch (Exception e)
            {
                throw e;
                //TODO  Log error : PubSub not started
            }
        }



        public void Subscribe()
        {
            try
            {
                proxy.Subscribe();
            }
            catch (Exception e)
            {
                //throw e;
                //TODO  Log error 
            }

        }

        public void UnSubscribe()
        {
            try
            {
                proxy.UnSubscribe();
            }
            catch (Exception e)
            {
                throw e;
                //TODO  Log error 
            }
        }

        public void Publish(SCADAUpdateModel update)
        {

            publishUpdateEvent?.Invoke(update);

        }


    }
}
