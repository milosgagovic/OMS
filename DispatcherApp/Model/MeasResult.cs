using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherApp.Model
{
    public class MeasResult : DataGridResult
    {
        private string mrID;
        private string measValue;

        public string MrID
        {
            get
            { return mrID; }
            set
            { mrID = value; }
        }

        public string MeasValue
        {
            get
            { return measValue; }
            set
            { this.measValue = value; }
        }

        public MeasResult()
        {

        }


        public MeasResult(string id, string value)
        {
            MrID = id;
            MeasValue = value;
        }
    }
}
