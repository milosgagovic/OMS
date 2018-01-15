using DMSCommon.Model;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TransactionManagerContract
{
    [DataContract]
    public class TMSAnswerToClient
    {
        private List<ResourceDescription> resourceDescriptions;
        private List<Element> elements;
        private int graphDeep;

        [DataMember]
        public int GraphDeep { get => graphDeep; set => graphDeep = value; }
        [DataMember]
        public List<Element> Elements { get => elements; set => elements = value; }
        [DataMember]
        public List<ResourceDescription> ResourceDescriptions { get => resourceDescriptions; set => resourceDescriptions = value; }

        public TMSAnswerToClient(List<ResourceDescription> rd, List<Element> ele, int deep)
        {
            GraphDeep = deep;
            ResourceDescriptions = rd;
            ele = Elements;
        }
    }
}
