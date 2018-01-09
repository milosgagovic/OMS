using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMSCommon.Model
{
    public class Source : Branch
    {
        public Source() { }
        public Source(long gid,Node end1,string mrid) : base(gid,mrid)
        {
            End1 = end1;
        }

        public Source(long gid, Node end1,Node end2, string mrid) : base(gid, mrid)
        {
            End1 = end1;
            End2 = end2;
        }
    }
}
