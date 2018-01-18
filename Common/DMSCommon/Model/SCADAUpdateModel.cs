using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DMSCommon.Model
{
    [Serializable]
    [DataContract]
    public class SCADAUpdateModel
    {
        private string mrID;
        private string state;

        [DataMember]
        public string MrID
        {
            get { return mrID; }
            set {mrID = value; }
        }

        [DataMember]
        public string State
        {
            get { return state; }
            set { state = value; }
        }

        public SCADAUpdateModel(string mrid, string state)
        {
            MrID = mrid;
            State = state;
        }
    }
}
