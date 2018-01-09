using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMSCommon.Model
{
    public class Consumer : Branch
    {
        public Consumer() { }
        public Consumer(long gid,Node end2) : base(gid)
        {
            End2 = end2;
        }
        public Consumer (long gid,string mrid):base(gid,mrid)
        {
            End2 = null;
        }
        public Consumer(long gid, string mrid,Node node) : base(gid, mrid)
        {
            End1 = node;
            End2 = null;
        }
    }
}
