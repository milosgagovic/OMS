using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMSCommon.Model
{
    public class ACLine : Branch
    {
        private float _length;

        public float Length { get => _length; set => _length = value; }

        public ACLine() { }
        public ACLine(float length)
        {
            this.Length = length;
        }
        public ACLine(long gid) : base(gid)
        {

        }
        public ACLine(long gid, string mrid) : base(gid, mrid)
        {

        }


    }
}
