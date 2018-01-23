using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSContract
{
	public class IncidentReport
	{
		private DateTime time;
		private string mrID;
		private string state;

		public IncidentReport()
		{

		}
		//[Key]
		public DateTime Time { get => time; set => time = value; }

		public string MrID { get => mrID; set => mrID = value; }
		public string State { get => state; set => state = value; }
	}
}
