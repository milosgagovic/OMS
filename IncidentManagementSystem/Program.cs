using IncidentManagementSystem.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncidentManagementSystem
{
	class Program
	{
		static void Main(string[] args)
		{
			IncidentManagementSystemService ims = new IncidentManagementSystemService();
			Console.WriteLine("Incident Management System started");

			ims.Start();

			Console.ReadLine();

			ims.Stop();
		}
	}
}
