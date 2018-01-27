using OMSSCADACommon;
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
        private long gid;
        private bool isEnergized;
        private States state;
        private CrewResponse response;
        private bool isElementAdded;

        [DataMember]
        public long Gid
        {
            get { return gid; }
            set { gid = value; }
        }

        [DataMember]
        public bool IsEnergized
        {
            get { return isEnergized; }
            set { isEnergized = value; }
        }

        [DataMember]
        public States State
        {
            get { return state; }
            set { state = value; }
        }
        [DataMember]
        public CrewResponse Response { get => response; set => response = value; }
        [DataMember]
        public bool IsElementAdded { get => isElementAdded; set => isElementAdded = value; }

        public SCADAUpdateModel(long mrid, bool isEnergized)
        {
            Gid = mrid;
            IsEnergized = isEnergized;

            if (isEnergized)
                State = States.CLOSED;
            else
            {
                State = States.OPENED;
            }
            isElementAdded = false;
        }
        public SCADAUpdateModel(long mrid, bool isEnergised, CrewResponse response)
        {
            Gid = mrid;
            IsEnergized = isEnergised;
            Response = response;

            if (isEnergized)
                State = States.CLOSED;
            else
            {
                State = States.OPENED;
            }
            isElementAdded = false;
        }

        public SCADAUpdateModel(bool isElementAdded, long mrid)
        {
            Gid = mrid;
            this.isElementAdded = isElementAdded;
        }


    }
}
