using DMSCommon.Model;
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

        public void Publish(List<SCADAUpdateModel> update)
        {
            foreach (IPublishing subscriber in PubSubscribeDB.Subscribers)
            {
                PublishThreadData threadObj = new PublishThreadData(subscriber, update);

                Thread thread = new Thread(threadObj.PublishDelta);
                thread.Start();
            }

        }
    }

    internal class PublishThreadData
    {
        private IPublishing subscriber;

        private List<SCADAUpdateModel> update;


        public PublishThreadData(IPublishing subscriber, List<SCADAUpdateModel> update)
        {
          
            this.subscriber = subscriber;
            this.update = update;

        }
        public List<SCADAUpdateModel> Update
        {
            get
            {
                return update;
            }

            set
            {
                update = value;
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
                subscriber.Publish(Update);
            }
            catch (Exception e)
            {
                PubSubscribeDB.RemoveSubsriber(subscriber);
            }
        }
    }
}
