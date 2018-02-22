using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherApp.Model.Properties
{
    public class EnergyConsumerProperties : ElementProperties
    {
        private bool call;

        public bool Call
        {
            get
            {
                return call;
            }
            set
            {
                call = value;
                RaisePropertyChanged("Call");
            }
        }

        public new void ReadFromResourceDescription(ResourceDescription rd)
        {
            base.ReadFromResourceDescription(rd);
        }
    }
}
