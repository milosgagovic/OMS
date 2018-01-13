using DMSCommon.TreeGraph.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMSCommon.TreeGraph
{
    public sealed class TreeNode<T>
    {
        public T Data { get; set; }
        public NodeLink Link { get; set; }
    }
}
