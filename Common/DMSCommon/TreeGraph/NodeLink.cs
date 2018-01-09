using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMSCommon.TreeGraph
{
    namespace Tree
    {
        public class NodeLink
        {
            public long Id { get; set; }
            public long? Parent { get; set; }
            public long? Next { get; set; }
            public long? Child { get; set; }
            public int Depth { get; set; }
        }
    }
}
