using FTN.Common;
using PubSubContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PubSubscribeService.Services
{
    public class PublishingService : IPublishing
    {

        public PublishingService()
        {
        }

        public void Publish(Delta delta)
        {
            foreach (IPublishing subscriber in PubSubscribeDB.Subscribers)
            {
                PublishThreadData threadObj = new PublishThreadData(subscriber, delta);

                Thread thread = new Thread(threadObj.PublishDelta);
                thread.Start();
            }

        }
    }

    internal class PublishThreadData
    {
        private IPublishing subscriber;

        private Delta delta;


        public PublishThreadData(IPublishing subscriber, Delta delta)
        {
          
            this.subscriber = subscriber;
            this.delta = delta;

        }
        public Delta Delta
        {
            get
            {
                return delta;
            }

            set
            {
                delta = value;
            }
        }

        public IPublishing Subscriber
        {
            get
            {
                return subscriber;
            }

            set
            {
                subscriber = value;
            }
        }

        public void PublishDelta()
        {
            try
            {
                subscriber.Publish(Delta);
            }
            catch (Exception e)
            {
                PubSubscribeDB.RemoveSubsriber(subscriber);
            }
        }
    }
}
