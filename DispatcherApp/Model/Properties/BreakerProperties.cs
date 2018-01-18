using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DispatcherApp.Model.Properties
{
    public class BreakerProperties : ElementProperties
    {
        private List<string> validCommands;

        private string state;

        public List<string> ValidCommands
        {
            get
            {
                return validCommands;
            }
            set
            {
                validCommands = value;
                RaisePropertyChanged("ValidCommands");
            }
        }

        public string State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
                RaisePropertyChanged("State");
            }
        }
    }
}
