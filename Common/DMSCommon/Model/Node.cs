using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSCommon.Model
{
    public class Node : Element
    {
        private Branch _parent;
        private List<Branch> children;
        private long upTerminal;

        public Branch Parent { get => _parent; set => _parent = value; }
        public List<Branch> Children { get => children; set => children = value; }
        public long UpTerminal { get => upTerminal; set => upTerminal = value; }

        public Node() { }
        public Node(long gid) : base(gid)
        {
            Children = new List<Branch>();
            Parent = null;
        }
        public Node(Branch parent)
        {
            Parent = parent;
            Children = new List<Branch>();
        }
        public Node(long gid,string mrid) : base(gid,mrid)
        {
            Children = new List<Branch>();
            Parent = null;
        }

        public Node(long gid, string mrid,Branch parent,long upTerminal) : base(gid, mrid)
        {
            Children = new List<Branch>();
            Parent = parent;
            UpTerminal = upTerminal;
        }


    }
}
