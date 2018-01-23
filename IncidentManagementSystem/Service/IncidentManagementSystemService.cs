using IMSContract;
using IncidentManagementSystem.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IncidentManagementSystem.Service
{
	public class IncidentManagementSystemService
	{
		private ServiceHost svc = null;
		public void Start()
		{
			Database.SetInitializer<IncidentContext>(new DropCreateDatabaseIfModelChanges<IncidentContext>());
			svc = new ServiceHost(typeof(IMSService));
			var binding = new NetTcpBinding();
			svc.AddServiceEndpoint(typeof(IIMSContract), binding, new
			Uri("net.tcp://localhost:6090/IncidentManagementSystemService"));
			svc.Open();
			Console.WriteLine("IncidentManagementSystemService ready and waiting for requests.");
		}
		public void Stop()
		{
			svc.Close();
			Console.WriteLine("IncidentManagementSystemService server stopped.");
		}
	}
}
