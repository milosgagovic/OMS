using DMSCommon.Model;
using FTN.Common;
using IMSContract;
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

        public void PublishCrewUpdate(SCADAUpdateModel update)
        {
            foreach (IPublishing subscriber in PubSubscribeDB.Subscribers)
            {
                PublishThreadData threadObj = new PublishThreadData(subscriber, update);

                Thread thread = new Thread(threadObj.PublishCrewDelta);
                thread.Start();
            }
        }

        public void PublishIncident(IncidentReport report)
        {
            foreach (IPublishing subscriber in PubSubscribeDB.Subscribers)
            {
                PublishThreadData threadObj = new PublishThreadData(subscriber, report);

                Thread thread = new Thread(threadObj.PublishIncidentDelta);
                thread.Start();
            }
        }
    }

    internal class PublishThreadData
    {
        private IPublishing subscriber;

        private List<SCADAUpdateModel> update;

        private SCADAUpdateModel crewUpdate;

        private IncidentReport report;


        public PublishThreadData(IPublishing subscriber, List<SCADAUpdateModel> update)
        {

            this.subscriber = subscriber;
            this.update = update;

        }
        public PublishThreadData(IPublishing subscriber, SCADAUpdateModel update)
        {

            this.subscriber = subscriber;
            this.crewUpdate = update;

        }

        public PublishThreadData(IPublishing subscriber, IncidentReport report)
        {
            this.subscriber = subscriber;
            this.report = report;
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

        public SCADAUpdateModel CrewUpdate { get => crewUpdate; set => crewUpdate = value; }

        public IncidentReport Report { get => report; set => report = value; }

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

        public void PublishCrewDelta()
        {
            try
            {
                subscriber.PublishCrewUpdate(CrewUpdate);
            }
            catch (Exception e)
            {
                PubSubscribeDB.RemoveSubsriber(subscriber);
            }
        }

        public void PublishIncidentDelta()
        {
            try
            {
                subscriber.PublishIncident(Report);
            }
            catch (Exception e)
            {
                PubSubscribeDB.RemoveSubsriber(subscriber);
            }
        }
    }
}
