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

        [DataMember]
        public long Gid
        {
            get { return gid; }
            set {gid = value; }
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
        }


    }
}
