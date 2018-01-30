using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSContract
{
	public class Crew
	{
		private string id;
		private string crewName;
		[Key]
		public string Id { get => id; set => id = value; }
		public string CrewName { get => crewName; set => crewName = value; }
		public Crew()
		{
		}

		public Crew(string id, string name, ICollection<int> levels)
		{
			this.Id = id;
			this.CrewName = name;
		}


	}
}
