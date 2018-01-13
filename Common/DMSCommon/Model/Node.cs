using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DMSCommon.Model
{
    [DataContract]
    public class Node : Element
    {
        [DataMember]
        private long _parent;
        [DataMember]
        private List<long> children;
        [DataMember]
        private long upTerminal;

        public long Parent { get => _parent; set => _parent = value; }
        public List<long> Children { get => children; set => children = value; }
        public long UpTerminal { get => upTerminal; set => upTerminal = value; }

        public Node() { }
        public Node(long gid) : base(gid)
        {
            Children = new List<long>();
            Parent = 0;
        }
        public Node(Branch parent)
        {
            Parent = 0;
            Children = new List<long>();
        }
        public Node(long gid,string mrid) : base(gid,mrid)
        {
            Children = new List<long>();
            Parent = 0;
        }

        public Node(long gid, string mrid,Branch parent,long upTerminal) : base(gid, mrid)
        {
            Children = new List<long>();
            Parent = parent.ElementGID;
            UpTerminal = upTerminal;
        }


    }
}
