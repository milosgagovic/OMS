using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DMSCommon.Model
{
    [DataContract]
    public class Branch : Element
    {
        [DataMember]
        private long _end1;
        [DataMember]
        private long _end2;
        
        public long End1 { get => _end1; set => _end1 = value; }
        public long End2 { get => _end2; set => _end2 = value; }

        public Branch() { }
        public Branch (long gid):base(gid)
        {

        }
        public Branch(long gid,string mrid) : base(gid,mrid)
        {

        }
        public Branch (long end1, long end2)
        {
            End1 = end1;
            End2 = end2;
        }

      
    }
}
