using System;
using System.ComponentModel.DataAnnotations;

namespace IMSContract
{
   public class ElementStateReport
    {
        private string mrID;
        private DateTime time;
        private int state;

        public  ElementStateReport()
        {

        }
        public string MrID { get => mrID; set => mrID = value; }
        [Key]
        public DateTime Time { get => time; set => time = value; }
        public int State { get => state; set => state = value; }
    }
}
