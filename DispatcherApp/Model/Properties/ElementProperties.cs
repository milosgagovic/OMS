using FTN.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DispatcherApp.Model.Properties
{
    public class ElementProperties : INotifyPropertyChanged
    {
        private long gID;
        private string mRID;
        private string name;
        private bool isEnergized;
        private bool isUnderScada;
        private bool incident = false;
        private bool crewSent = false;

        public ElementProperties() { }

        public void ReadFromResourceDescription(ResourceDescription rd)
        {
            try { this.GID = rd.GetProperty(ModelCode.IDOBJ_GID).AsLong(); } catch { }
            try { this.MRID = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString(); } catch { }
            try { this.Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString(); } catch { }
        }

        public long GID
        {
            get
            {
                return this.gID;
            }
            set
            {
                this.gID = value;
                RaisePropertyChanged("GID");
            }
        }

        public string MRID
        {
            get
            {
                return this.mRID;
            }
            set
            {
                this.mRID = value;
                RaisePropertyChanged("MRID");
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
                RaisePropertyChanged("Name");
            }
        }

        public bool IsEnergized
        {
            get
            {
                return this.isEnergized;
            }
            set
            {
                this.isEnergized = value;
                RaisePropertyChanged("IsEnergized");
            }
        }

        public bool IsUnderScada
        {
            get
            {
                return this.isUnderScada;
            }
            set
            {
                this.isUnderScada = value;
                RaisePropertyChanged("IsUnderScada");
            }
        }

        public bool Incident
        {
            get
            {
                return this.incident;
            }
            set
            {
                this.incident = value;
                RaisePropertyChanged("Incident");
            }
        }

        public bool CrewSent
        {
            get
            {
                return this.crewSent;
            }
            set
            {
                this.crewSent = value;
                RaisePropertyChanged("CrewSent");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
