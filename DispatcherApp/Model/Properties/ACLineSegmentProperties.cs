using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherApp.Model.Properties
{
    public class ACLineSegmentProperties : ElementProperties
    {
        private float lenght;

        public float Length
        {
            get
            {
                return lenght;
            }
            set
            {
                lenght = value;
                RaisePropertyChanged("Length");
            }
        }
    }
}
