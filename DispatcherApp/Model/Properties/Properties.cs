using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherApp.Model.Properties
{
    public abstract class Properties
    {
        public string MRID { get; private set; }
        public string Name { get; private set; }
        public bool IsEnergized { get; private set; }
        public bool IsUnderScada { get; private set; }
    }
}
