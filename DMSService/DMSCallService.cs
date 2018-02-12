using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using DMSContract;
using OpenPop.Mime;
using OpenPop.Pop3;
using DMSCommon.Model;
using FTN.Common;
using System.Text.RegularExpressions;
using PubSubscribe;

namespace DMSService
{
    public class DMSCallService : IDMSCallContract
    {
        public Dictionary<string, Message> messagesFormClents = new Dictionary<string, Message>();
        public Pop3Client client;


        public DMSCallService()
        {
            Thread t = new Thread(new ThreadStart(Process));
            t.Start();
        }
        public void Process()
        {
            while (true)
            {
                LogIn();
                Message message;
                try
                {
                    var fg = client.GetMessageUids();
                    var count = client.GetMessageCount();
                    List<int> indexforDelete = new List<int>();
                    if (count > 0)
                    {
                        for (int i = 1; i <= count; i++)
                        {
                            message = client.GetMessage(i);
                            MessagePart mp = message.FindFirstPlainTextVersion();
                            messagesFormClents.Add(message.Headers.MessageId, message);
                            /*
                            Console.WriteLine(mp.GetBodyAsText());
                            Pronaci ec na osnovu MRID_ja
                            */
                            SCADAUpdateModel call = TryGetConsumer(mp.GetBodyAsText());
                            if (call.Gid > 0 )
                            {
                                SendMailMessageToClient(message, true);

                                Publisher publisher = new Publisher();
                                publisher.PublishCallIncident(call);
                            }
                            else
                                SendMailMessageToClient(message, false);

                            indexforDelete.Add(i);
                        }
                        foreach (int item in indexforDelete)
                        {
                            client.DeleteMessage(item);
                        }
                        client.Disconnect();
                        LogIn();
                    }
                }
                catch (Exception)
                {
                    client.Disconnect();
                    LogIn();
                }
                //Console.WriteLine(message.Headers.Subject);
                Thread.Sleep(5000);
            }
        }

        private SCADAUpdateModel TryGetConsumer(string mrid)
        {
            SCADAUpdateModel consumer = new SCADAUpdateModel();
            string s = Regex.Match(mrid.ToUpper().Trim(), @"\d+").Value;
            if (mrid.Contains("ec_") || mrid.Contains("EC_"))
            {
                foreach (ResourceDescription rd in DMSService.Instance.EnergyConsumersRD)
                {
                    if (rd.GetProperty(ModelCode.IDOBJ_MRID).AsString() == "EC_"+s)
                    {
                        consumer = new SCADAUpdateModel(rd.Id, false);
                        return consumer;
                    }
                }
            }
            else if (mrid.Contains("ec") || mrid.Contains("EC"))
            {
                foreach (ResourceDescription rd in DMSService.Instance.EnergyConsumersRD)
                {
                    if (rd.GetProperty(ModelCode.IDOBJ_MRID).AsString() == "EC_" + s)
                    {
                        consumer = new SCADAUpdateModel(rd.Id, false);
                        return consumer;
                    }
                }
            }
            return consumer;
        }

        public void LogIn()
        {
            client = new Pop3Client();
            bool canConnectToGmailServer = false;
            do
            {
                try
                {
                    client.Connect("pop.gmail.com", 995, true);
                    client.Authenticate("omscallreport@gmail.com", "omsreport");
                    canConnectToGmailServer = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            } while (!canConnectToGmailServer);
        }
        public void SendMailMessageToClient(Message message, bool canFind)
        {
            bool canConnectToGmailServer = false;
            do
            {
                try
                {
                    var mess = new MailMessage("omscallreport@gmail.com", message.Headers.From.ToString());
                    mess.Subject = "no replay?";
                    SmtpClient mailer = new SmtpClient("smtp.gmail.com", 587);
                    mailer.Credentials = new NetworkCredential("omscallreport@gmail.com", "omsreport");
                    mailer.EnableSsl = true;

                    if (canFind)
                        mess.Body = "Your outage request is in progress!";
                    else
                        mess.Body = "We canot identyfy you as a consumer. \n Please write your id in format EC_number or ec_number. ";

                    mailer.Send(mess);
                    canConnectToGmailServer = true;

                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            } while (!canConnectToGmailServer);
        }

        public void SendCall(string mrid)
        {
            throw new NotImplementedException();
        }
    }
}
