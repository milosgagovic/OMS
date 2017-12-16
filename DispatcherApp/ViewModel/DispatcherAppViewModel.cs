using DispatcherApp.Model;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using PubSubscribe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DispatcherApp.ViewModel
{
    public class DispatcherAppViewModel : INotifyPropertyChanged
    {
        private List<string> elements;
        private List<IdentifiedObject> dataGridElements;
        private string selectedItem;
        public event PropertyChangedEventHandler PropertyChanged;
        private ModelGda model;
        //Subscriber
        private Subscriber subscriber;
        #region Properties
        public string SelectedItem
        {
            get
            {
                return selectedItem;
            }

            set
            {
                selectedItem = value;
                GetElementsFromNMS();
            }
        }
        public List<string> Elements
        {
            get
            {
                return elements;
            }
            set
            {
                elements = value;
            }

        }
        public List<IdentifiedObject> DataGridElements
        {
            get
            {
                return dataGridElements;
            }
            set
            {
                dataGridElements = value;
                RaisePropertyChanged("DataGridElements");
            }

        }
        #endregion Properties
        private void GetElementsFromNMS()
        {
            switch (SelectedItem)
            {

                case "Breaker":
                    DataGridElements = ConvertToListOfIdentifiedObjects(model.GetExtentValues(FTN.Common.ModelCode.BREAKER));
                    break;
                case "ACLineSegment":
                    DataGridElements = ConvertToListOfIdentifiedObjects(model.GetExtentValues(FTN.Common.ModelCode.ACLINESEGMENT));
                    break;
                case "EnergySource":
                    DataGridElements = ConvertToListOfIdentifiedObjects(model.GetExtentValues(FTN.Common.ModelCode.ENERGSOURCE));
                    break;
                case "EnergyConsumer":
                    DataGridElements = ConvertToListOfIdentifiedObjects(model.GetExtentValues(FTN.Common.ModelCode.ENERGCONSUMER));
                    break;
                default:
                    break;
            }

        }
        public DispatcherAppViewModel(List<string> ele)
        {
            Elements = ele;
            DataGridElements = new List<IdentifiedObject>();
            model = new ModelGda();

            subscriber = new Subscriber();
            subscriber.Subscribe();
            subscriber.publishDeltaEvent += GetDelta;
        }
        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        
        public DispatcherAppViewModel()
        {
            subscriber = new Subscriber();
            subscriber.Subscribe();
            subscriber.publishDeltaEvent += GetDelta;
        }

        private void GetDelta(Delta delta)
        {
            Console.WriteLine(delta.ToString());
        }

        private List<IdentifiedObject> ConvertToListOfIdentifiedObjects(List<long> list)
        {
            List<IdentifiedObject> retValue = new List<IdentifiedObject>();
            ResourceDescription rd = new ResourceDescription();
            foreach (long l in list)
            {
                rd = model.GetValues(l);
                retValue.Add(new IdentifiedObject(rd.Id));
            }
            return retValue;
        }
    }
}
