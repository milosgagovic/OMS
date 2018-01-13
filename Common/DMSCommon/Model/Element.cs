﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DMSCommon.Model
{
    [DataContract]
    public class Element
    {
        [DataMember]
        private long _elementGID;
        [DataMember]
        private bool _marker;
        [DataMember]
        private string _mRID;
       
        public long ElementGID
        {
            get { return _elementGID; }
            set { _elementGID = value; }
        }
       
        public bool Marker
        {
            get { return _marker; }
            set { _marker = value; }
        }
        public string MRID
        {
            get { return _mRID; }
            set { _mRID = value; }
        }
        public Element() { }
        public Element(long gid)
        {
            ElementGID = gid;
            Marker = true;
        }

        public Element(long gid,string mrid)
        {
            ElementGID = gid;
            MRID = mrid;
            Marker = true;
        }

    }

}
