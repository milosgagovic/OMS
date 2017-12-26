﻿using CommunicationEngineContract;
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
        private List<MeasResult> dataGridElements;
        private string selectedItem;
        public event PropertyChangedEventHandler PropertyChanged;
        private ModelGda model;
        //Subscriber
        private Subscriber subscriber;
        private CommEngProxyUpdate proxy = new CommEngProxyUpdate("CommEngineEndpoint");
        //Get Meas Command
        private RelayCommand _measCommand;
        #region Properties
        public RelayCommand MeasCommand
        {
            // get { return _measCommand = new ReadAll(); }
            get
            {
                return _measCommand ?? new RelayCommand(ExecuteReadAll);
            }
        }


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
        public List<MeasResult> DataGridElements
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
        public CommEngProxyUpdate Proxy
        {
            get { return proxy; }
            set { proxy = value; }
        }
        #endregion Properties
        private void GetElementsFromNMS()
        {
            switch (SelectedItem)
            {

                case "Breaker":
                    DataGridElements = ConvertToListOfMeasResults(model.GetExtentValues(FTN.Common.ModelCode.BREAKER));
                    break;
                case "ACLineSegment":
                    DataGridElements = ConvertToListOfMeasResults(model.GetExtentValues(FTN.Common.ModelCode.ACLINESEGMENT));
                    break;
                case "EnergySource":
                    DataGridElements = ConvertToListOfMeasResults(model.GetExtentValues(FTN.Common.ModelCode.ENERGSOURCE));
                    break;
                case "EnergyConsumer":
                    DataGridElements = ConvertToListOfMeasResults(model.GetExtentValues(FTN.Common.ModelCode.ENERGCONSUMER));
                    break;
                default:
                    break;
            }

        }
        public DispatcherAppViewModel(List<string> ele)
        {
            Elements = ele;
            DataGridElements = new List<MeasResult>();
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
            Console.WriteLine("Ima li sta: " + delta.TestOperations.Count);
            if(delta.TestOperations.Count != 0)
            {
                ReadResult(delta.TestOperations);
            }

        }

        private List<MeasResult> ConvertToListOfMeasResults(List<long> list)
        {
            List<MeasResult> retValue = new List<MeasResult>();
            ResourceDescription rd = new ResourceDescription();
            foreach (long l in list)
            {
                rd = model.GetValues(l);
                retValue.Add(new MeasResult(rd.Id.ToString(), "Unknown"));
            }
            return retValue;
        }

        private void ReadResult(List<ResourceDescription> result)
        {

            List<MeasResult> rezultat = new List<MeasResult>();
            string status = "";
            foreach (ResourceDescription rd in result)
            {
                MeasResult measResult = new MeasResult();
                if (rd.ContainsProperty(ModelCode.IDOBJ_MRID))
                {
                    measResult.MrID = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString();
                }
                if (rd.ContainsProperty(ModelCode.DISCRETE_NORMVAL))
                {

                    switch (rd.GetProperty(ModelCode.DISCRETE_NORMVAL).AsLong())
                    {
                        case 0:
                            status = "CLOSED";
                            break;
                        case 1:
                            status = "OPEN";
                            break;
                        default:
                            status = "Unkonown";
                            break;

                    }

                    measResult.MeasValue = status;
                }
                rezultat.Add(measResult);
            }
            DataGridElements = rezultat;
        }

        private void ExecuteReadAll(object parameter)
        {
            ///
            /// otvori vezu ka CommEngine i dobavi mjerenja
            ////
            Proxy.ReceiveAllMeasValue(TypeOfSCADACommand.ReadAll);

            //List<MeasResult> rezultat = new List<MeasResult>();
            //ResourceDescription rd1 = new ResourceDescription();
            //rd1.Id = 1;
            //rd1.Properties.Add(new Property(ModelCode.DISCRETE_NORMVAL, 1));
            
            //ResourceDescription rd2 = new ResourceDescription();
            //rd2.Id = 2;
            //rd2.Properties.Add(new Property(ModelCode.DISCRETE_NORMVAL, 0));
            //// ResourceDescription result = proxyToComm().GetaAll();
            //List<ResourceDescription> result = new List<ResourceDescription>();
            //result.Add(rd1);
            //result.Add(rd2);
            //string status = "";
            ////napunjeno zbog testiranjaa
            //foreach (ResourceDescription rd in result)
            //{
            //    switch (rd.Properties[0].PropertyValue.LongValues[0])
            //    {
            //        case 0:
            //            status = "CLOSED";
            //            break;
            //        case 1:
            //            status = "OPEN";
            //            break;
            //        default:
            //            status = "Unkonown";
            //            break;

            //    }

            //    rezultat.Add(new MeasResult(rd.Id.ToString(), status));
            //}
            //DataGridElements = rezultat;
        }
    }
}
