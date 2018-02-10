﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMSContract
{
    public class IncidentReport : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int id;
        private DateTime time;
        private string mrID;
        private float lostPower;
        private IncidentState incidentState;
        private bool crewSent;
        private TimeSpan repairTime;
        private ReasonForIncident reason;
        private CrewType type;
        private Crew crew;

        public IncidentReport()
        {
            Time = DateTime.UtcNow;
            Time = Time.AddTicks(-(Time.Ticks % TimeSpan.TicksPerSecond));
            CrewSent = false;
            RepairTime = new TimeSpan();
            LostPower = 0;
        }

        [Key]
        public int Id { get => id; set => id = value; }
        public DateTime Time { get => time; set { time = value; } }
        public string MrID { get => mrID; set => mrID = value; }
        public float LostPower { get => lostPower; set => lostPower = value; }
        public IncidentState IncidentState { get => incidentState; set { incidentState = value; RaisePropertyChanged("IncidentState"); } }
        public bool CrewSent { get => crewSent; set { crewSent = value; RaisePropertyChanged("CrewSent"); } }
        public TimeSpan RepairTime { get => repairTime; set { repairTime = value; RaisePropertyChanged("RepairTime"); } }
        public ReasonForIncident Reason { get => reason; set { reason = value; RaisePropertyChanged("Reason"); } }
        public CrewType Crewtype { get => type; set => type = value; }
        public Crew Crew { get => crew; set => crew = value; }

        protected void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
