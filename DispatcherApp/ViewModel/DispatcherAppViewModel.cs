using CommunicationEngineContract;
using DispatcherApp.Model;
using DispatcherApp.View;
using DispatcherApp;
using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using PubSubscribe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DMSCommon.Model;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;
using DMSContract;
using TransactionManagerContract;
using System.ServiceModel;

namespace DispatcherApp.ViewModel
{
    public class DispatcherAppViewModel : INotifyPropertyChanged
    {
        private List<string> elements;
        private List<MeasResult> dataGridElements;
        private string selectedItem;
        public event PropertyChangedEventHandler PropertyChanged;
        private ModelGda model;
        private IOMSClient proxyToTransactionManager;
        #region Subscriber
        private Subscriber subscriber;
        private CommEngProxyUpdate proxy = new CommEngProxyUpdate("CommEngineEndpoint");
        #endregion


        private List<Source> sources = new List<Source>();
        private ObservableCollection<UIElement> networkElements = new ObservableCollection<UIElement>();
        private int networkDepth = 5;
        private Canvas mainCanvas = new Canvas() { Width = 400, Height = 400 };

        #region Bindings

        private ObservableCollection<TabItem> leftTabControlTabs = new ObservableCollection<TabItem>();
        private int leftTabControlIndex = 0;
        private Visibility leftTabControlVisibility = Visibility.Collapsed;

        private ObservableCollection<TreeViewItem> networkMapsBySource = new ObservableCollection<TreeViewItem>();
        private ObservableCollection<Button> networkMapsBySourceButton = new ObservableCollection<Button>();

        private ObservableCollection<TabItem> centerTabControlTabs = new ObservableCollection<TabItem>();
        private int centerTabControlIndex = 0;

        private NetworkExplorerControl networkExplorer = new NetworkExplorerControl();
        private Dictionary<string, NetworkModelControl> networModelControls = new Dictionary<string, NetworkModelControl>();

        #endregion

        #region Commands
        private RelayCommand _measCommand;

        private RelayCommand _openControlCommand;
        private RelayCommand _closeControlCommand;

        #endregion

        public DispatcherAppViewModel(List<string> ele)
        {
            #region Init
            Elements = ele;
            DataGridElements = new List<MeasResult>();
            model = new ModelGda();
            ChannelFactory<IOMSClient> factoryToTMS = new ChannelFactory<IOMSClient>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:6080/TransactionManagerService"));
            proxyToTransactionManager = factoryToTMS.CreateChannel();

            subscriber = new Subscriber();
            subscriber.Subscribe();
            //subscriber.PublishDeltaEvent += GetDelta;

            TreeViewItem tvi1 = new TreeViewItem() { Header = "ES_1" };
            TreeViewItem tvi2 = new TreeViewItem() { Header = "ES_2" };
            networkMapsBySource.Add(tvi1);
            networkMapsBySource.Add(tvi2);

            Button but1 = new Button() { Content = "ES_1", Command = OpenControlCommand, CommandParameter = "ES_1" };
            Button but2 = new Button() { Content = "ES_2", Command = OpenControlCommand, CommandParameter = "ES_2" };

            this.NetworkMapsBySourceButton.Add(but1);
            //this.NetworkMapsBySourceButton.Add(but2);

            NetworkModelControl nmc1 = new NetworkModelControl();
            NetworkModelControl nmc2 = new NetworkModelControl();
            this.networModelControls.Add("ES_1", nmc1);
            this.networModelControls.Add("ES_2", nmc2);
            #endregion

            #region FakeNetwork
            //Source s1 = new Source(0, null, "ES_1");
            //Node n1 = new Node(0, "CN_1");
            //n1.Parent = s1;
            //s1.End2 = n1;
            //ACLine b1 = new ACLine(0, "ACLS_1");
            //b1.End1 = n1;
            //n1.Children.Add(b1);
            //Node n2 = new Node(0, "CN_2");
            //b1.End2 = n2;
            //n2.Parent = b1;
            //Switch b2 = new Switch(0, "BR_1");
            //b2.End1 = n2;
            //n2.Children.Add(b2);
            //ACLine b3 = new ACLine(0, "ACLS_3");
            //b3.End1 = n2;
            //n2.Children.Add(b3);
            //Node n3 = new Node(0, "CN_3");
            //b2.End2 = n3;
            //n3.Parent = b2;
            //Switch b4 = new Switch(0, "BR_2");
            //b4.End1 = n3;
            //n3.Children.Add(b4);
            //ACLine b5 = new ACLine(0, "ACLS_2");
            //b5.End1 = n3;
            //n3.Children.Add(b5);
            //Node n4 = new Node(0, "CN_4");
            //b4.End2 = n4;
            //n4.Parent = b4;
            //Consumer b6 = new Consumer(0, "EC_1");
            //b6.End1 = n4;
            //n4.Children.Add(b6);
            //Node n5 = new Node(0, "CN_5");
            //b5.End2 = n5;
            //n5.Parent = b5;
            //Consumer b7 = new Consumer(0, "EC_2");
            //b7.End1 = n5;
            //n5.Children.Add(b7);
            //Node n6 = new Node(0, "CN_6");
            //b3.End2 = n6;
            //n6.Parent = b3;
            //Switch b8 = new Switch(0, "BR_3");
            //b8.End1 = n6;
            //n6.Children.Add(b8);
            //Node n7 = new Node(0, "CN_7");
            //b8.End2 = n7;
            //n7.Parent = b8;
            //Consumer b9 = new Consumer(0, "EC_3");
            //b9.End1 = n7;
            //b9.End2 = null;
            //n7.Children.Add(b9);

            //sources.Add(s1);
            #endregion

            this.NetworkElements.Add(mainCanvas);

            // dobaviti mreze od DMS-a (za sad su lazni podaci)

            TMSAnswerToClient answerFromTransactionManager = proxyToTransactionManager.GetNetwork();
            foreach (Source source in sources)
            {
                DrawGraph(source);
            }
        }

        private void DrawGraph(Source source)
        {
            double cellHeight = mainCanvas.Height / networkDepth;

            for (int i = 1; i < cellHeight; i++)
            {
                PlaceGridLines(i, cellHeight);
            }

            Point point1 = new Point()
            {
                X = mainCanvas.Width / 2,
                Y = 5
            };
            Point point2 = new Point()
            {
                X = mainCanvas.Width / 2,
                Y = cellHeight
            };

            PlaceBranch(point1, point2, cellHeight, source);

            //PlaceGraph(source.End2, 1, 1, mainCanvas.Width, cellHeight, 1, 0, null);
        }

        private void PlaceGraph(Node currentNode, int x, int y, double cellWidth, double cellHeight, double currentDivision, double offset, Point? point1)
        {
            if(currentNode == null)
            {
                return;
            }

            Point? point2 = PlaceNode(x, y++, cellWidth, cellHeight, offset, currentNode.MRID); 

            if (point1 != null)
            {
                //PlaceBranch((Point)point1, (Point)point2, cellHeight, currentNode.Parent);
            }

            cellWidth = mainCanvas.Width / (currentDivision * currentNode.Children.Count);

            for (int x1 = 1; x1 <= currentNode.Children.Count; x1++)
            {
                offset += (x1 - 1) * cellWidth;
                //PlaceGraph(currentNode.Children[x1 - 1].End2, x1, y, cellWidth, cellHeight, currentDivision * currentNode.Children.Count, offset, point2);

                if(currentNode.Children[x1 - 1] is Consumer)
                {
                    PlaceConsumer(y, cellWidth, cellHeight, offset, (Point)point2);
                }
            }
        }

        private void PlaceConsumer(int y, double cellWidth, double cellHeight, double offset, Point point1)
        {
            Button sourceButton = new Button() { Width = 25, Height = 25 };
            sourceButton.Background = Brushes.Transparent;
            sourceButton.BorderThickness = new Thickness(0);
            sourceButton.BorderBrush = Brushes.Transparent;
            sourceButton.Content = new Image() { Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/../../Resources/Images/yellowcircle.png")) };
            //sourceButton.Content = id;
            Canvas.SetLeft(sourceButton, offset + /*x * */cellWidth - cellWidth / 2 - sourceButton.Width / 2);
            Canvas.SetTop(sourceButton, y * cellHeight - sourceButton.Height - 5);
            Canvas.SetZIndex(sourceButton, 5);

            Point point2 = new Point()
            {
                X = offset + /*x * */cellWidth - cellWidth / 2,
                Y = y * cellHeight - sourceButton.Height
            };

            PlaceBranch(point1, point2, cellHeight, null);

            mainCanvas.Children.Add(sourceButton);
        }

        private void PlaceBranch(Point point1, Point point2, double cellHeight, Branch branch)
        {
            Polyline polyline = new Polyline();
            polyline.Points.Add(point1);
            Point point3 = new Point()
            {
                X = point2.X,
                Y = point2.Y - 2*(cellHeight / 3)
            };

            polyline.Points.Add(point3);
            polyline.Points.Add(point2);
            polyline.Stroke = Brushes.White;
            polyline.StrokeThickness = 1;
            Canvas.SetZIndex(polyline, 0);

            if(branch != null)
            {
                if (branch is Source)
                {
                    PlaceSource();
                }
                else if (branch is Switch)
                {
                    Button button = new Button() { Width = 15, Height = 20 };
                    button.Background = Brushes.Transparent;
                    button.BorderThickness = new Thickness(0);
                    button.BorderBrush = Brushes.Transparent;
                    button.Content = new Rectangle() { Fill = new SolidColorBrush(Color.FromRgb(0, 250, 17)), Height = 15, Width = 20 };

                    Canvas.SetLeft(button, point2.X - button.Width / 2);
                    Canvas.SetTop(button, point2.Y - (cellHeight / 3) - button.Height / 2);
                    Canvas.SetZIndex(button, 5);

                    mainCanvas.Children.Add(button);
                }
            }

            mainCanvas.Children.Add(polyline);
        }

        private Point? PlaceNode(int x, int y, double cellWidth, double cellHeight, double offset, string id)
        {
            Button sourceButton = new Button() { Width = 10, Height = 10 };
            sourceButton.Background = Brushes.Transparent;
            sourceButton.BorderThickness = new Thickness(0);
            sourceButton.BorderBrush = Brushes.Transparent;
            sourceButton.Content = new Image() { Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/../../Resources/Images/bluecircle.png")) };
            //sourceButton.Content = id;
            Canvas.SetLeft(sourceButton, offset + /*x * */cellWidth - cellWidth/2 - sourceButton.Width / 2);
            Canvas.SetTop(sourceButton, y*cellHeight - sourceButton.Height / 2);
            Canvas.SetZIndex(sourceButton, 5);

            mainCanvas.Children.Add(sourceButton);

            return new Point()
            {
                X = offset + cellWidth - cellWidth / 2,
                Y = y * cellHeight
            };
        }

        private void PlaceSource()
        {
            Button sourceButton = new Button() { Width = 15, Height = 15 };
            sourceButton.Background = Brushes.Transparent;
            sourceButton.BorderThickness = new Thickness(0);
            sourceButton.BorderBrush = Brushes.Transparent;
            sourceButton.Content = new Image() { Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/../../Resources/Images/triangle.png")) };
            Canvas.SetLeft(sourceButton, mainCanvas.Width / 2 - sourceButton.Width / 2);
            Canvas.SetZIndex(sourceButton, 5);
            mainCanvas.Children.Add(sourceButton);
        }

        private void PlaceGridLines(int i, double cellHeight)
        {
            Line line = new Line();
            line.Stroke = Brushes.Gray;
            line.StrokeThickness = 0.5;
            line.X1 = 0;
            line.X2 = mainCanvas.Width;
            line.Y1 = i*cellHeight;
            line.Y2 = i*cellHeight;

            mainCanvas.Children.Add(line);
        }

        #region Properties
        public RelayCommand MeasCommand
        {
            // get { return _measCommand = new ReadAll(); }
            get
            {
                return _measCommand ?? new RelayCommand(ExecuteReadAll);
            }
        }

        public RelayCommand OpenControlCommand
        {
            get
            {
                return _openControlCommand ?? new RelayCommand(
                    (parameter) => 
                    {
                        ExecuteOpenControlCommand(parameter);
                    });
            }
        }

        public RelayCommand CloseControlCommand
        {
            get
            {
                return _closeControlCommand ?? new RelayCommand(
                    (parameter) =>
                    {
                        ExecuteCloseControlCommand(parameter);
                    });
            }
        }

        private void ExecuteOpenControlCommand(object parameter)
        {
            if ((string)parameter == "Network Explorer")
            {
                bool exists = false;
                int i = 0;

                for (i = 0; i < LeftTabControlTabs.Count; i++)
                {
                    if (LeftTabControlTabs[i].Header == parameter)
                    {
                        exists = true;
                        this.LeftTabControlIndex = i;
                        break;
                    }
                }

                if (!exists)
                {
                    TabItem ti = new TabItem() { Content = networkExplorer, Header = parameter };
                    if (!leftTabControlTabs.Contains(ti))
                    {
                        this.LeftTabControlTabs.Add(ti);
                        this.LeftTabControlIndex = this.LeftTabControlTabs.Count - 1;
                    }
                }

                this.LeftTabControlVisibility = Visibility.Visible;
            }

            else
            {
                bool exists = false;
                int i = 0;

                for (i = 0; i < CenterTabControlTabs.Count; i++)
                {
                    if (CenterTabControlTabs[i].Header == parameter)
                    {
                        exists = true;
                        this.CenterTabControlIndex = i;
                        break;
                    }
                }

                if (!exists)
                {
                    TabItem ti = new TabItem() { Content = networModelControls[(string)parameter], Header = parameter };
                    if (!CenterTabControlTabs.Contains(ti))
                    {
                        this.CenterTabControlTabs.Add(ti);
                        this.CenterTabControlIndex = this.CenterTabControlTabs.Count - 1;
                    }
                }
            }
        }

        private void ExecuteCloseControlCommand(object parameter)
        {
            if ((string)parameter == "Network Explorer")
            {
                int i = 0;

                for (i = 0; i < LeftTabControlTabs.Count; i++)
                {
                    if ((string)LeftTabControlTabs[i].Header == (string)parameter)
                    {
                        LeftTabControlTabs[i].Content = null;
                        LeftTabControlTabs[i].Visibility = Visibility.Collapsed;
                        LeftTabControlTabs.RemoveAt(i);
                        break;
                    }
                }

                if(LeftTabControlTabs.Count == 0)
                {
                    this.LeftTabControlVisibility = Visibility.Collapsed;
                }
            }
        }

        public ObservableCollection<TabItem> LeftTabControlTabs
        {
            get
            {
                return leftTabControlTabs;
            }
            set
            {
                leftTabControlTabs = value;
            }
        }

        public int LeftTabControlIndex
        {
            get
            {
                return leftTabControlIndex;
            }
            set
            {
                leftTabControlIndex = value;
                RaisePropertyChanged("LeftTabControlIndex");
            }
        }

        public int CenterTabControlIndex
        {
            get
            {
                return centerTabControlIndex;
            }
            set
            {
                centerTabControlIndex = value;
                RaisePropertyChanged("CenterTabControlIndex");
            }
        }

        public Visibility LeftTabControlVisibility
        {
            get
            {
                return leftTabControlVisibility;
            }
            set
            {
                leftTabControlVisibility = value;
                RaisePropertyChanged("LeftTabControlVisibility");
            }
        }

        public ObservableCollection<TabItem> CenterTabControlTabs
        {
            get
            {
                return centerTabControlTabs;
            }
            set
            {
                centerTabControlTabs = value;
            }
        }

        public ObservableCollection<TreeViewItem> NetworkMapsBySource
        {
            get
            {
                return networkMapsBySource;
            }
            set
            {
                networkMapsBySource = value;
            }
        }

        public ObservableCollection<Button> NetworkMapsBySourceButton
        {
            get
            {
                return networkMapsBySourceButton;
            }
            set
            {
                networkMapsBySourceButton = value;
            }
        }

        public ObservableCollection<UIElement> NetworkElements
        {
            get
            {
                return networkElements;
            }
            set
            {
                networkElements = value;
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
            //subscriber.PublishDeltaEvent += GetDelta;
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
