using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSCommon.Model
{
    public class Branch : Element
    {
        private Node _end1;
        private Node _end2;
        public Node End1 { get => _end1; set => _end1 = value; }
        public Node End2 { get => _end2; set => _end2 = value; }

        public Branch() { }
        public Branch (long gid):base(gid)
        {

        }
        public Branch(long gid,string mrid) : base(gid,mrid)
        {

        }
        public Branch (Node end1,Node end2)
        {
            End1 = end1;
            End2 = end2;
        }

      
    }
}
