using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMSCommon.Model
{
    public class Switch:Branch
    {
        private SwitchState _state;

        public SwitchState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public Switch() { }

        public Switch(long gid) : base(gid)
        {

        }
        public Switch(long gid, string mrid) : base(gid, mrid)
        {

        }



    }
}
