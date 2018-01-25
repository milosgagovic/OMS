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
using DMSCommon.TreeGraph;
using DispatcherApp.Model.Properties;
using TransactionManagerContract;
using System.ServiceModel;
using System.Windows.Data;
using IMSContract;

namespace DispatcherApp.ViewModel
{
    public class DispatcherAppViewModel : INotifyPropertyChanged
    {
        private List<string> elements;
        private List<MeasResult> dataGridElements;
        private string selectedItem;
        public event PropertyChangedEventHandler PropertyChanged;
        private ModelGda model;

        #region Subscriber
        private Subscriber subscriber;
        #endregion

        private IOMSClient proxyToTransactionManager;

        private Dictionary<long, Element> Network = new Dictionary<long, Element>();
        private List<long> Sources = new List<long>();

        private Dictionary<long, ObservableCollection<UIElement>> uiNetworks = new Dictionary<long, ObservableCollection<UIElement>>();
        private ObservableCollection<UIElement> mainCanvases = new ObservableCollection<UIElement>();
        private Dictionary<long, int> networkDepth = new Dictionary<long, int>();
        private Canvas mainCanvas;

        #region Bindings

        private ObservableCollection<TabItem> leftTabControlTabs = new ObservableCollection<TabItem>();
        private int leftTabControlIndex = 0;
        private Visibility leftTabControlVisibility = Visibility.Collapsed;

        private ObservableCollection<TabItem> rightTabControlTabs = new ObservableCollection<TabItem>();
        private int rightTabControlIndex = 0;
        private Visibility rightTabControlVisibility = Visibility.Collapsed;

        private ObservableCollection<TabItem> bottomTabControlTabs = new ObservableCollection<TabItem>();
        private int bottomTabControlIndex = 0;
        private Visibility bottomTabControlVisibility = Visibility.Collapsed;

        private ObservableCollection<TreeViewItem> networkMapsBySource = new ObservableCollection<TreeViewItem>();
        private ObservableCollection<Button> networkMapsBySourceButton = new ObservableCollection<Button>();

        private ObservableCollection<TabItem> centerTabControlTabs = new ObservableCollection<TabItem>();
        private int centerTabControlIndex = 0;

        private NetworkExplorerControl networkExplorer = new NetworkExplorerControl();
        private IncidentExplorer incidentExplorer = new IncidentExplorer();
        private OutputControl output = new OutputControl();
        private Dictionary<long, NetworkModelControlExtended> networModelControls = new Dictionary<long, NetworkModelControlExtended>();

        private Dictionary<long, ElementProperties> properties = new Dictionary<long, ElementProperties>();
        private Dictionary<long, ResourceDescription> resourceProperties = new Dictionary<long, ResourceDescription>();
        private ElementProperties currentProperty;
        private long currentPropertyMRID;

        private int commandsIndex = 0;
        private bool test = true;

        private ObservableCollection<IncidentReport> incidentReports = new ObservableCollection<IncidentReport>();

        #endregion

        #region Commands
        private RelayCommand _measCommand;

        private RelayCommand _openControlCommand;
        private RelayCommand _closeControlCommand;

        private RelayCommand _propertiesCommand;

        private RelayCommand _sendCrewCommand;

        #endregion

        public DispatcherAppViewModel(List<string> ele)
        {
            Thread.Sleep(7000);
            #region Init
            //Elements = ele;
            //DataGridElements = new List<MeasResult>();
            model = new ModelGda();

            subscriber = new Subscriber();
            subscriber.Subscribe();
            subscriber.publishUpdateEvent += GetUpdate;
            subscriber.publishCrewEvent += GetCrewUpdate;
            subscriber.publishIncident += GetIncident;

            TreeViewItem tvi1 = new TreeViewItem() { Header = "ES_1" };
            TreeViewItem tvi2 = new TreeViewItem() { Header = "ES_2" };
            networkMapsBySource.Add(tvi1);
            networkMapsBySource.Add(tvi2);

            //IncidentReports.Add(new IncidentReport() { Id = 10 });

            //Button but1 = new Button() { Content = "ES_1", Command = OpenControlCommand, CommandParameter = "ES_1" };
            //Button but2 = new Button() { Content = "ES_2", Command = OpenControlCommand, CommandParameter = "ES_2" };

            //this.NetworkMapsBySourceButton.Add(but1);
            //this.NetworkMapsBySourceButton.Add(but2);
            #endregion

            NetTcpBinding binding = new NetTcpBinding();
            binding.CloseTimeout = new TimeSpan(1, 0, 0, 0);
            binding.OpenTimeout = new TimeSpan(1, 0, 0, 0);
            binding.ReceiveTimeout = new TimeSpan(1, 0, 0, 0);
            binding.SendTimeout = new TimeSpan(1, 0, 0, 0);
            binding.MaxBufferPoolSize = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;
            binding.MaxBufferSize = 2147483647;

            ChannelFactory<IOMSClient> factoryToTMS = new ChannelFactory<IOMSClient>(binding, new EndpointAddress("net.tcp://localhost:6080/TransactionManagerService"));
            proxyToTransactionManager = factoryToTMS.CreateChannel();
            TMSAnswerToClient answerFromTransactionManager = new TMSAnswerToClient();
            try
            {
                answerFromTransactionManager = proxyToTransactionManager.GetNetwork();
            }
            catch (Exception e) { }

            #region FakeNetwork

            Source s1 = new Source(0, -1, "ES_2") { ElementGID = 0 };
            Source s2 = new Source(0, -1, "ES_3") { ElementGID = 23 };
            s2.Marker = false;
            Node n10 = new Node(24, "CN_10");
            n10.Parent = s2.ElementGID;
            s2.End2 = n10.ElementGID;
            ACLine b15 = new ACLine(25, "ACLS_1");
            b15.End1 = n10.ElementGID;
            n10.Children.Add(b15.ElementGID);
            Consumer b16 = new Consumer(27, "EC_4");
            b16.End1 = n10.ElementGID;
            b16.End2 = -1;
            n10.Children.Add(b16.ElementGID);
            Node n11 = new Node(26, "CN_2");
            b15.End2 = n11.ElementGID;
            n11.Parent = b15.ElementGID;
            Consumer b17 = new Consumer(28, "EC_4");
            b17.End1 = n11.ElementGID;
            b17.End2 = -1;
            n11.Children.Add(b17.ElementGID);
            Consumer b18 = new Consumer(29, "EC_4");
            b18.End1 = n11.ElementGID;
            b18.End2 = -1;
            n11.Children.Add(b18.ElementGID);
            Node n1 = new Node(1, "CN_1") { ElementGID = 1 };
            n1.Parent = s1.ElementGID;
            s1.End2 = n1.ElementGID;
            ACLine b1 = new ACLine(2, "ACLS_1");
            b1.End1 = n1.ElementGID;
            n1.Children.Add(b1.ElementGID);
            Node n2 = new Node(3, "CN_2");
            b1.End2 = n2.ElementGID;
            n2.Parent = b1.ElementGID;
            Switch b2 = new Switch(4, "BR_1");
            b2.End1 = n2.ElementGID;
            n2.Children.Add(b2.ElementGID);
            ACLine b3 = new ACLine(5, "ACLS_3");
            b3.End1 = n2.ElementGID;
            n2.Children.Add(b3.ElementGID);
            Consumer b10 = new Consumer(17, "EC_4");
            b10.End1 = n2.ElementGID;
            b10.End2 = -1;
            n2.Children.Add(b10.ElementGID);
            Node n3 = new Node(6, "CN_3");
            b2.End2 = n3.ElementGID;
            n3.Parent = b2.ElementGID;
            Switch b4 = new Switch(7, "BR_2");
            b4.End1 = n3.ElementGID;
            n3.Children.Add(b4.ElementGID);
            ACLine b5 = new ACLine(8, "ACLS_2");
            b5.End1 = n3.ElementGID;
            b5.Marker = false;
            n3.Children.Add(b5.ElementGID);
            Node n4 = new Node(9, "CN_4");
            b4.End2 = n4.ElementGID;
            n4.Parent = b4.ElementGID;
            Consumer b6 = new Consumer(10, "EC_1");
            b6.End1 = n4.ElementGID;
            n4.Children.Add(b6.ElementGID);
            Node n5 = new Node(11, "CN_5");
            b5.End2 = n5.ElementGID;
            n5.Parent = b5.ElementGID;
            Consumer b7 = new Consumer(12, "EC_2");
            b7.End1 = n5.ElementGID;
            n5.Children.Add(b7.ElementGID);
            b7.Marker = false;
            Node n6 = new Node(13, "CN_6");
            b3.End2 = n6.ElementGID;
            n6.Parent = b3.ElementGID;
            n6.Marker = false;
            Switch b8 = new Switch(14, "BR_3");
            b8.End1 = n6.ElementGID;
            b8.Marker = false;
            n6.Children.Add(b8.ElementGID);
            Node n7 = new Node(15, "CN_7");
            b8.End2 = n7.ElementGID;
            n7.Parent = b8.ElementGID;
            Consumer b9 = new Consumer(16, "EC_3");
            b9.End1 = n7.ElementGID;
            b9.End2 = -1;
            n7.Children.Add(b9.ElementGID);
            Consumer b11 = new Consumer(20, "EC_5");
            b11.End1 = n7.ElementGID;
            b11.End2 = -1;
            n7.Children.Add(b11.ElementGID);
            //Consumer b12 = new Consumer(21, "EC_5");
            //b12.End1 = n1.ElementGID;
            //b12.End2 = -1;
            //n1.Children.Add(b12.ElementGID);

            Sources.Add(s1.ElementGID);
            Sources.Add(s2.ElementGID);

            Network.Add(s1.ElementGID, s1);
            Network.Add(s2.ElementGID, s2);
            Network.Add(n1.ElementGID, n1);
            Network.Add(n2.ElementGID, n2);
            Network.Add(n3.ElementGID, n3);
            Network.Add(n4.ElementGID, n4);
            Network.Add(n5.ElementGID, n5);
            Network.Add(n6.ElementGID, n6);
            Network.Add(n7.ElementGID, n7);
            Network.Add(b1.ElementGID, b1);
            Network.Add(b2.ElementGID, b2);
            Network.Add(b3.ElementGID, b3);
            Network.Add(b4.ElementGID, b4);
            Network.Add(b5.ElementGID, b5);
            Network.Add(b6.ElementGID, b6);
            Network.Add(b7.ElementGID, b7);
            Network.Add(b8.ElementGID, b8);
            Network.Add(b9.ElementGID, b9);
            Network.Add(b10.ElementGID, b10);
            Network.Add(b11.ElementGID, b11);
            Network.Add(b15.ElementGID, b15);
            Network.Add(n10.ElementGID, n10);
            Network.Add(n11.ElementGID, n11);
            Network.Add(b16.ElementGID, b16);
            Network.Add(b17.ElementGID, b17);
            Network.Add(b18.ElementGID, b18);
            //Network.Add(b12.ElementGID, b12);
            //Network.Add(n8.ElementGID, n8);
            #endregion

            if (answerFromTransactionManager != null && answerFromTransactionManager.Elements != null && answerFromTransactionManager.ResourceDescriptions != null)
            {


                foreach (Element element in answerFromTransactionManager.Elements)
                {
                    this.Network.Add(element.ElementGID, element);
                }

                foreach (ResourceDescription rd in answerFromTransactionManager.ResourceDescriptions)
                {
                    Element element = null;
                    this.Network.TryGetValue(rd.GetProperty(ModelCode.IDOBJ_GID).AsLong(), out element);

                    if (element != null)
                    {
                        if (element is Source)
                        {
                            this.Sources.Add(element.ElementGID);
                            this.properties.Add(element.ElementGID, new EnergySourceProperties()
                            {
                                GID = rd.GetProperty(ModelCode.IDOBJ_GID).AsLong(),
                                MRID = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString(),
                                Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString(),
                                IsEnergized = element.Marker
                            });
                        }
                        else if (element is Consumer)
                        {
                            this.properties.Add(element.ElementGID, new EnergyConsumerProperties()
                            {
                                GID = rd.GetProperty(ModelCode.IDOBJ_GID).AsLong(),
                                MRID = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString(),
                                Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString(),
                                IsEnergized = element.Marker
                            });
                        }
                        else if (element is ACLine)
                        {
                            this.properties.Add(element.ElementGID, new ACLineSegmentProperties()
                            {
                                GID = rd.GetProperty(ModelCode.IDOBJ_GID).AsLong(),
                                MRID = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString(),
                                Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString(),
                                IsEnergized = element.Marker,
                                Length = rd.GetProperty(ModelCode.CONDUCTOR_LEN).AsFloat()
                            });
                        }
                        else if (element is Node)
                        {
                            this.properties.Add(element.ElementGID, new ConnectivityNodeProperties()
                            {
                                GID = rd.GetProperty(ModelCode.IDOBJ_GID).AsLong(),
                                MRID = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString(),
                                Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString(),
                                IsEnergized = element.Marker
                            });
                        }
                        else if (element is Switch)
                        {
                            BreakerProperties prop = new BreakerProperties()
                            {
                                GID = rd.GetProperty(ModelCode.IDOBJ_GID).AsLong(),
                                MRID = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString(),
                                IsEnergized = element.Marker,
                                Name = rd.GetProperty(ModelCode.IDOBJ_NAME).AsString()
                            };

                            prop.ValidCommands.Add("CLOSE");
                            this.CommandIndex = 0;

                            //if (element.Marker)
                            //{
                            //    prop.State = "CLOSED";
                            //}
                            //else
                            //{
                            //    prop.State = "OPENED";
                            //}

                            this.properties.Add(element.ElementGID, prop);
                        }
                    }
                    else
                    {
                        long el = rd.GetProperty(ModelCode.MEASUREMENT_PSR).AsLong();
                        ElementProperties prop;
                        properties.TryGetValue(el, out prop);

                        if (prop != null)
                        {
                            BreakerProperties prop1 = prop as BreakerProperties;
                            //prop1.ValidCommands = rd.GetProperty(ModelCode.DISCRETE_VALIDCOMMANDS).AsEnums();
                        }
                    }
                }
            }

            foreach (ResourceDescription rd in answerFromTransactionManager.ResourceDescriptionsOfMeasurment)
            {
                ResourceDescription meas = answerFromTransactionManager.ResourceDescriptions.Where(p => p.GetProperty(ModelCode.IDOBJ_MRID).AsString() == rd.GetProperty(ModelCode.IDOBJ_MRID).AsString()).FirstOrDefault();

                if (meas != null)
                {
                    long breaker = meas.GetProperty(ModelCode.MEASUREMENT_PSR).AsLong();
                    int state = rd.GetProperty(ModelCode.DISCRETE_NORMVAL).AsInt();

                    ElementProperties prop;
                    properties.TryGetValue(breaker, out prop);

                    if (prop != null)
                    {
                        BreakerProperties prop1 = prop as BreakerProperties;
                        if (state == 0)
                        {
                            prop1.State = "CLOSED";
                        }
                        else if (state == 1)
                        {
                            prop1.State = "OPENED";
                        }
                    }
                }
            }

            //this.networkDepth.Add(0, 5);
            //this.networkDepth.Add(23, 3);

            foreach (long sourceGid in Sources)
            {
                Element element = null;

                Network.TryGetValue(sourceGid, out element);

                Source source = element as Source;

                this.UINetworks.Add(source.ElementGID, new ObservableCollection<UIElement>());

                Canvas canvas = new Canvas() { Width = 400, Height = 400 };
                this.UINetworks[source.ElementGID].Add(canvas);
                this.MainCanvases.Add(canvas);

                NetworkModelControlExtended nmc = new NetworkModelControlExtended() { ItemsSourceForCanvas = this.UINetworks[source.ElementGID] };
                this.networModelControls.Add(source.ElementGID, nmc);

                Button but = new Button() { Content = source.MRID, Command = OpenControlCommand, CommandParameter = source.ElementGID };

                this.NetworkMapsBySourceButton.Add(but);

                this.mainCanvas = canvas;

                this.networkDepth.Add(source.ElementGID, answerFromTransactionManager.GraphDeep);

                if (source != null)
                {
                    DrawGraph(source as Source);
                }
            }
        }

        #region DrawGraph
        private void DrawGraph(Source source)
        {
            double cellHeight = mainCanvas.Height / networkDepth[source.ElementGID];

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

            Element end2 = null;

            Network.TryGetValue(source.End2, out end2);

            if (end2 != null)
            {
                PlaceGraph(end2 as Node, 1, 1, mainCanvas.Width, cellHeight, 1, 0, null);
            }
        }

        private void PlaceGraph(Node currentNode, int x, int y, double cellWidth, double cellHeight, double currentDivision, double offset, Point? point1)
        {
            if (currentNode == null)
            {
                return;
            }

            Point? point2 = PlaceNode(x, y++, cellWidth, cellHeight, offset, currentNode.ElementGID, currentNode.MRID);

            if (point1 != null)
            {
                Element parent = null;

                Network.TryGetValue(currentNode.Parent, out parent);

                if (parent != null)
                {
                    PlaceBranch((Point)point1, (Point)point2, cellHeight, parent as Branch);
                }
            }

            cellWidth = mainCanvas.Width / (currentDivision * currentNode.Children.Count);

            for (int x1 = 1; x1 <= currentNode.Children.Count; x1++)
            {
                double localOffset = offset + (x1 - 1) * cellWidth;

                Element branch = null;

                Network.TryGetValue(currentNode.Children[x1 - 1], out branch);

                if (branch != null)
                {
                    Branch branch1 = branch as Branch;

                    Element end2 = null;

                    Network.TryGetValue(branch1.End2, out end2);

                    if (end2 != null)
                    {
                        PlaceGraph(end2 as Node, x1, y, cellWidth, cellHeight, currentDivision * currentNode.Children.Count, localOffset, point2);
                    }

                    Element consumer = null;

                    Network.TryGetValue(currentNode.Children[x1 - 1], out consumer);

                    if (consumer is Consumer)
                    {
                        PlaceConsumer(y, cellWidth, cellHeight, localOffset, (Point)point2, consumer as Consumer, consumer.ElementGID, consumer.MRID);
                    }
                }
            }
        }

        private void PlaceConsumer(int y, double cellWidth, double cellHeight, double offset, Point point1, Consumer consumer, long id, string mrid)
        {
            Button sourceButton = new Button() { Width = cellHeight / 3, Height = cellHeight / 3 };
            sourceButton.Background = Brushes.Transparent;
            sourceButton.BorderThickness = new Thickness(0);
            sourceButton.BorderBrush = Brushes.Transparent;
            sourceButton.ToolTip = mrid;
            Ellipse ellipse = new Ellipse()
            {
                Width = sourceButton.Width - 5,
                Height = sourceButton.Height - 5
            };
            if (consumer.Marker)
            {
                ellipse.Fill = new SolidColorBrush(Color.FromRgb(0, 250, 17));
            }
            else
            {
                ellipse.Fill = Brushes.Blue;
                //ellipse.StrokeThickness = 1;
                //ellipse.Stroke = Brushes.LightGray;
            }

            sourceButton.Content = ellipse;
            //sourceButton.Content = new Image() { Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/../../Resources/Images/yellowcircle.png")) };
            //sourceButton.Content = id;
            Canvas.SetLeft(sourceButton, offset + /*x * */cellWidth - cellWidth / 2 - sourceButton.Width / 2);
            Canvas.SetTop(sourceButton, y * cellHeight - sourceButton.Height - 5);
            Canvas.SetZIndex(sourceButton, 5);

            Point point2 = new Point()
            {
                X = offset + /*x * */cellWidth - cellWidth / 2,
                Y = y * cellHeight - sourceButton.Height
            };

            PlaceBranch(point1, point2, cellHeight, consumer);

            mainCanvas.Children.Add(sourceButton);

            SetProperties(sourceButton, id);
        }

        private void PlaceSwitch(double cellHeight, Point point1, Point point2, long id, bool isEnergized, string mrid)
        {
            Button button = new Button() { Width = cellHeight / 4, Height = cellHeight / 4 };
            button.BorderThickness = new Thickness(0);
            button.BorderBrush = Brushes.Transparent;

            Style style = new Style();
            style.TargetType = typeof(Button);

            //if (isEnergized)
            //{
            ElementProperties prop;
            properties.TryGetValue(id, out prop);

            if (prop != null)
            {
                BreakerProperties prop1 = prop as BreakerProperties;

                Setter setter3 = new Setter();
                setter3.Property = Button.TemplateProperty;

                ControlTemplate template = new ControlTemplate(typeof(Button));
                FrameworkElementFactory elemFactory = new FrameworkElementFactory(typeof(Border));
                elemFactory.Name = "Border";
                //elemFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
                elemFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0, 250, 17)));
                template.VisualTree = elemFactory;

                Trigger trigger1 = new Trigger();
                trigger1.Property = Button.IsMouseOverProperty;
                trigger1.Value = true;

                Setter setter1 = new Setter();
                setter1.Property = Border.BackgroundProperty;
                setter1.Value = Brushes.Yellow;
                setter1.TargetName = "Border";
                trigger1.Setters.Add(setter1);

                template.Triggers.Add(trigger1);

                DataTrigger trigger2 = new DataTrigger();
                trigger2.Binding = new Binding("Properties[" + id + "].IsEnergized");
                trigger2.Value = false;

                Setter setter2 = new Setter();
                setter2.Property = Border.BackgroundProperty;
                setter2.Value = Brushes.Red;
                setter2.TargetName = "Border";
                trigger2.Setters.Add(setter2);

                template.Triggers.Add(trigger2);

                setter3.Value = template;

                style.Triggers.Clear();
                style.Setters.Add(setter3);
                button.Style = style;

                //Canvas canvas = new Canvas();
                //button.Background = new SolidColorBrush(Color.FromRgb(0, 250, 17));
                //button.Content = new Image()
                //{
                //    Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/../../Resources/Images/energized.png")),
                //    Width = button.Width,
                //    Height = button.Height,
                //    HorizontalAlignment = HorizontalAlignment.Left,
                //    VerticalAlignment = VerticalAlignment.Top
                //};
            }
            //button.Background = new SolidColorBrush(Color.FromRgb(0, 250, 17));
            //}
            //else
            //{
            //    //button.Background = Brushes.Red;
            //}

            button.ToolTip = mrid;

            Canvas.SetLeft(button, point2.X - button.Width / 2);
            Canvas.SetTop(button, point2.Y - (cellHeight / 3) - button.Height / 2);
            Canvas.SetZIndex(button, 5);

            mainCanvas.Children.Add(button);
            SetProperties(button, id);
        }

        private void PlaceACLine(double cellHeight, Point point1, Point point2, long id, bool isEnergized, string mrid)
        {
            Button button = new Button() { Width = 5, Height = cellHeight / 3 };
            button.Background = Brushes.Transparent;
            button.BorderThickness = new Thickness(0);
            button.BorderBrush = Brushes.Transparent;
            button.ToolTip = mrid;

            Rectangle rectangle = new Rectangle() { Width = button.Width, Height = button.Height };

            if (isEnergized)
            {
                rectangle.Fill = new SolidColorBrush(Color.FromRgb(0, 250, 17));
            }
            else
            {
                rectangle.Fill = Brushes.Blue;
            }

            button.Content = rectangle;

            Canvas.SetLeft(button, point2.X - button.Width / 2);
            Canvas.SetTop(button, point2.Y - (cellHeight / 3) - button.Height / 2);
            Canvas.SetZIndex(button, 5);

            mainCanvas.Children.Add(button);
            SetProperties(button, id);
        }

        private void PlaceBranch(Point point1, Point point2, double cellHeight, Branch branch)
        {
            Polyline polyline = new Polyline();
            polyline.Points.Add(point1);
            Point point3 = new Point()
            {
                X = point2.X,
                Y = point1.Y + (cellHeight / 3)
            };

            polyline.Points.Add(point3);
            polyline.Points.Add(point2);
            polyline.StrokeThickness = 1;
            Canvas.SetZIndex(polyline, 0);

            if (branch.Marker)
            {
                polyline.Stroke = new SolidColorBrush(Color.FromRgb(0, 250, 17));
            }
            else
            {
                polyline.Stroke = Brushes.Blue;
            }

            if (branch != null)
            {
                if (branch is Source)
                {
                    PlaceSource(branch.ElementGID, branch.MRID);
                }
                else if (branch is Switch)
                {
                    PlaceSwitch(cellHeight, point1, point2, branch.ElementGID, branch.Marker, branch.MRID);
                }
                else if (branch is ACLine)
                {
                    PlaceACLine(cellHeight, point1, point2, branch.ElementGID, branch.Marker, branch.MRID);
                }
            }

            mainCanvas.Children.Add(polyline);
        }

        private Point? PlaceNode(int x, int y, double cellWidth, double cellHeight, double offset, long id, string mrid)
        {
            Button sourceButton = new Button() { Width = 10, Height = 10 };
            sourceButton.Background = Brushes.Transparent;
            sourceButton.BorderThickness = new Thickness(0);
            sourceButton.BorderBrush = Brushes.Transparent;
            sourceButton.ToolTip = mrid;
            Ellipse ellipse = new Ellipse()
            {
                Fill = Brushes.Black,
                Width = sourceButton.Width - 5,
                Height = sourceButton.Height - 5
            };
            sourceButton.Content = ellipse;
            //sourceButton.Content = new Image() { Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/../../Resources/Images/bluecircle.png")) };
            //sourceButton.Content = id;
            Canvas.SetLeft(sourceButton, offset + /*x * */cellWidth - cellWidth / 2 - sourceButton.Width / 2);
            Canvas.SetTop(sourceButton, y * cellHeight - sourceButton.Height / 2);
            Canvas.SetZIndex(sourceButton, 5);

            mainCanvas.Children.Add(sourceButton);
            SetProperties(sourceButton, id);

            return new Point()
            {
                X = offset + cellWidth - cellWidth / 2,
                Y = y * cellHeight
            };
        }

        private void PlaceSource(long id, string mrid)
        {
            Button sourceButton = new Button() { Width = 18, Height = 18 };
            sourceButton.Background = Brushes.Transparent;
            sourceButton.BorderThickness = new Thickness(0);
            sourceButton.BorderBrush = Brushes.Transparent;
            sourceButton.ToolTip = mrid;
            sourceButton.Content = new Image() { Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/../../Resources/Images/triangle.png")) };
            Canvas.SetLeft(sourceButton, mainCanvas.Width / 2 - sourceButton.Width / 2);
            Canvas.SetZIndex(sourceButton, 5);
            mainCanvas.Children.Add(sourceButton);

            SetProperties(sourceButton, id);
        }

        private void PlaceGridLines(int i, double cellHeight)
        {
            Line line = new Line();
            line.Stroke = Brushes.Gray;
            line.StrokeThickness = 0.5;
            line.X1 = 0;
            line.X2 = mainCanvas.Width;
            line.Y1 = i * cellHeight;
            line.Y2 = i * cellHeight;

            mainCanvas.Children.Add(line);
        }

        private void SetProperties(Button button, long id)
        {
            ElementProperties property;
            Element element;
            this.properties.TryGetValue(id, out property);
            this.Network.TryGetValue(id, out element);

            button.Command = PropertiesCommand;
            button.CommandParameter = id;
        }
        #endregion

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

        public RelayCommand PropertiesCommand
        {
            get
            {
                return _propertiesCommand ?? new RelayCommand(
                    (parameter) =>
                    {
                        ExecutePropertiesCommand(parameter);
                    });
            }
        }

        public RelayCommand SendCrewCommand
        {
            get
            {
                return _sendCrewCommand ?? new RelayCommand(
                    (parameter) =>
                    {
                        ExecuteSendCrewCommand(parameter);
                    });
            }
        }

        private void ExecuteSendCrewCommand(object parameter)
        {
            IncidentReport report = new IncidentReport();
            foreach (IncidentReport ir in IncidentReports)
            {
                if (DateTime.Compare(ir.Time, (DateTime)parameter) == 0)
                {
                    report = ir;
                    break;
                }
            }
            report.CrewSent = true;
            report.IncidentState = IncidentState.PENDING;

            proxyToTransactionManager.SendCrew((DateTime)parameter);
        }

        private void ExecutePropertiesCommand(object parameter)
        {
            Element element;
            properties.TryGetValue((long)parameter, out currentProperty);
            Network.TryGetValue((long)parameter, out element);

            if (currentProperty != null)
            {
                CurrentPropertyMRID = currentProperty.GID;
                bool exists = false;
                int i = 0;

                for (i = 0; i < RightTabControlTabs.Count; i++)
                {
                    if (RightTabControlTabs[i].Header as string == "Properties")
                    {
                        SetTabContent(RightTabControlTabs[i], element);
                        exists = true;
                        this.RightTabControlIndex = i;
                        break;
                    }
                }

                if (!exists)
                {
                    TabItem ti = new TabItem() { Header = "Properties" };
                    SetTabContent(ti, element);
                    //if (!RightTabControlTabs.Contains(ti))
                    //{
                    this.RightTabControlTabs.Add(ti);
                    this.RightTabControlIndex = this.RightTabControlTabs.Count - 1;
                    //}
                }

                this.RightTabControlVisibility = Visibility.Visible;
            }
        }

        private void SetTabContent(TabItem tabItem, Element element)
        {
            if (element != null)
            {
                if (element is Node)
                {
                    tabItem.Content = new NodePropertiesControl();
                }
                else if (element is Switch)
                {
                    tabItem.Content = new SwitchPropertiesControl();
                }
                else if (element is Consumer)
                {
                    tabItem.Content = new ConsumerPropertiesControl();
                }
                else if (element is Source)
                {
                    tabItem.Content = new SourcePropertiesControl();
                }
                else if (element is ACLine)
                {
                    tabItem.Content = new ACLinePropertiesControl();
                }
            }
            else
            {
                tabItem.Content = new EmptyPropertiesControl();
            }
        }

        private void ExecuteOpenControlCommand(object parameter)
        {
            if (parameter as string == "Network Explorer")
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
            else if (parameter as string == "Properties")
            {
                bool exists = false;
                int i = 0;

                for (i = 0; i < RightTabControlTabs.Count; i++)
                {
                    if (RightTabControlTabs[i].Header == parameter)
                    {
                        exists = true;
                        this.RightTabControlIndex = i;
                        break;
                    }
                }

                if (!exists)
                {
                    TabItem ti = new TabItem() { Header = parameter };
                    if (!RightTabControlTabs.Contains(ti))
                    {
                        SetTabContent(ti, null);
                        this.RightTabControlTabs.Add(ti);
                        this.RightTabControlIndex = this.RightTabControlTabs.Count - 1;
                    }
                }

                this.RightTabControlVisibility = Visibility.Visible;
            }
            else if (parameter as string == "Incident Explorer" || parameter as string == "Output")
            {
                bool exists = false;
                int i = 0;

                for (i = 0; i < BottomTabControlTabs.Count; i++)
                {
                    if (BottomTabControlTabs[i].Header == parameter)
                    {
                        exists = true;
                        this.BottomTabControlIndex = i;
                        break;
                    }
                }

                if (!exists)
                {
                    TabItem ti = new TabItem() { Header = parameter };
                    if (parameter as string == "Incident Explorer")
                    {
                        ti.Content = incidentExplorer;
                    }
                    else if (parameter as string == "Output")
                    {
                        ti.Content = output;
                    }

                    if (!BottomTabControlTabs.Contains(ti))
                    {
                        this.BottomTabControlTabs.Add(ti);
                        this.BottomTabControlIndex = this.BottomTabControlTabs.Count - 1;
                    }
                }

                this.BottomTabControlVisibility = Visibility.Visible;
            }
            else
            {
                bool exists = false;
                int i = 0;
                Element element = null;
                if (parameter != null)
                {
                    Network.TryGetValue((long)parameter, out element);

                    if (element != null)
                    {
                        for (i = 0; i < CenterTabControlTabs.Count; i++)
                        {
                            if (CenterTabControlTabs[i].Header as string == element.MRID)
                            {
                                exists = true;
                                this.CenterTabControlIndex = i;
                                break;
                            }
                        }
                    }

                    if (!exists)
                    {
                        TabItem ti = new TabItem()
                        {
                            Content = networModelControls[(long)parameter],
                            Header = element.MRID
                        };

                        if (!CenterTabControlTabs.Contains(ti))
                        {
                            this.CenterTabControlTabs.Add(ti);
                            this.CenterTabControlIndex = this.CenterTabControlTabs.Count - 1;
                        }
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

                if (LeftTabControlTabs.Count == 0)
                {
                    this.LeftTabControlVisibility = Visibility.Collapsed;
                }
            }
            else if ((string)parameter == "Properties")
            {
                int i = 0;

                for (i = 0; i < RightTabControlTabs.Count; i++)
                {
                    if ((string)RightTabControlTabs[i].Header == (string)parameter)
                    {
                        RightTabControlTabs[i].Content = null;
                        RightTabControlTabs[i].Visibility = Visibility.Collapsed;
                        RightTabControlTabs.RemoveAt(i);
                        break;
                    }
                }

                if (RightTabControlTabs.Count == 0)
                {
                    this.RightTabControlVisibility = Visibility.Collapsed;
                }
            }
            else if ((string)parameter == "Incident Explorer" || (string)parameter == "Output")
            {
                int i = 0;

                for (i = 0; i < BottomTabControlTabs.Count; i++)
                {
                    if ((string)BottomTabControlTabs[i].Header == (string)parameter)
                    {
                        BottomTabControlTabs[i].Content = null;
                        BottomTabControlTabs[i].Visibility = Visibility.Collapsed;
                        BottomTabControlTabs.RemoveAt(i);
                        break;
                    }
                }

                if (BottomTabControlTabs.Count == 0)
                {
                    this.BottomTabControlVisibility = Visibility.Collapsed;
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

        public ObservableCollection<TabItem> RightTabControlTabs
        {
            get
            {
                return rightTabControlTabs;
            }
            set
            {
                rightTabControlTabs = value;
            }
        }

        public int RightTabControlIndex
        {
            get
            {
                return rightTabControlIndex;
            }
            set
            {
                rightTabControlIndex = value;
                RaisePropertyChanged("RightTabControlIndex");
            }
        }

        public ObservableCollection<TabItem> BottomTabControlTabs
        {
            get
            {
                return bottomTabControlTabs;
            }
            set
            {
                bottomTabControlTabs = value;
            }
        }

        public int BottomTabControlIndex
        {
            get
            {
                return bottomTabControlIndex;
            }
            set
            {
                bottomTabControlIndex = value;
                RaisePropertyChanged("BottomTabControlIndex");
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

        public int CommandIndex
        {
            get
            {
                return commandsIndex;
            }
            set
            {
                commandsIndex = value;
                RaisePropertyChanged("CommandIndex");
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

        public Visibility RightTabControlVisibility
        {
            get
            {
                return rightTabControlVisibility;
            }
            set
            {
                rightTabControlVisibility = value;
                RaisePropertyChanged("RightTabControlVisibility");
            }
        }

        public Visibility BottomTabControlVisibility
        {
            get
            {
                return bottomTabControlVisibility;
            }
            set
            {
                bottomTabControlVisibility = value;
                RaisePropertyChanged("BottomTabControlVisibility");
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

        public ObservableCollection<IncidentReport> IncidentReports
        {
            get
            {
                return incidentReports;
            }
            set
            {
                incidentReports = value;
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

        public ObservableCollection<UIElement> MainCanvases
        {
            get
            {
                return mainCanvases;
            }
            set
            {
                mainCanvases = value;
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

        public Dictionary<long, ObservableCollection<UIElement>> UINetworks
        {
            get
            {
                return uiNetworks;
            }
            set
            {
                uiNetworks = value;
            }
        }

        public Dictionary<long, ElementProperties> Properties
        {
            get
            {
                return properties;
            }
            set
            {
                properties = value;
                RaisePropertyChanged("Properties");
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

        public ElementProperties CurrentProperty
        {
            get
            {
                return currentProperty;
            }
            set
            {
                currentProperty = value;
                RaisePropertyChanged("CurrentProperty");
            }
        }

        public bool Test
        {
            get
            {
                return test;
            }
            set
            {
                test = value;
                RaisePropertyChanged("Test");
            }
        }

        public long CurrentPropertyMRID
        {
            get
            {
                return currentPropertyMRID;
            }
            set
            {
                currentPropertyMRID = value;
                RaisePropertyChanged("CurrentPropertyMRID");
            }
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

        private void GetUpdate(List<SCADAUpdateModel> update)
        {
            if (update != null)
            {
                foreach (SCADAUpdateModel sum in update)
                {
                    if (Network[sum.Gid] is Source)
                    {
                        mainCanvas.Children.Clear();
                        DrawGraph((Source)Network[sum.Gid]);
                    }
                    Network[sum.Gid].Marker = sum.IsEnergized;

                    //if (Test == true)
                    //    Test = false;
                    //else
                    //    Test = true;

                    ElementProperties property;
                    properties.TryGetValue(sum.Gid, out property);
                    if (property != null)
                    {
                        property.IsEnergized = sum.IsEnergized;

                        Element element;
                        Network.TryGetValue(sum.Gid, out element);
                        if (element != null && element is Switch)
                        {
                            BreakerProperties prop = property as BreakerProperties;
                            prop.State = sum.State.ToString();
                        }
                    }
                }
            }
        }

        private void GetCrewUpdate(SCADAUpdateModel update)
        {
            Console.WriteLine(update.Response.ToString());
        }

        private void GetIncident(IncidentReport report)
        {
            IncidentReport temp = new IncidentReport();
            bool found = false;
            foreach (IncidentReport ir in IncidentReports)
            {
                if (DateTime.Compare(ir.Time, report.Time) == 0)
                {
                    temp = ir;
                    found = true;
                    break;
                }
            }
            if (found)
            {
                temp.Reason = report.Reason;
                temp.RepairTime = report.RepairTime;
                temp.IncidentState = report.IncidentState;
            }
            else
            {
                IncidentReports.Add(report);
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

            //List<MeasResult> rezultat = new List<MeasResult>();
            //string status = "";
            //foreach (ResourceDescription rd in result)
            //{
            //    MeasResult measResult = new MeasResult();
            //    if (rd.ContainsProperty(ModelCode.IDOBJ_MRID))
            //    {
            //        measResult.MrID = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString();
            //    }
            //    if (rd.ContainsProperty(ModelCode.DISCRETE_NORMVAL))
            //    {

            //        switch (rd.GetProperty(ModelCode.DISCRETE_NORMVAL).AsLong())
            //        {
            //            case 0:
            //                status = "CLOSED";
            //                break;
            //            case 1:
            //                status = "OPEN";
            //                break;
            //            default:
            //                status = "Unkonown";
            //                break;

            //        }

            //        measResult.MeasValue = status;
            //    }
            //    rezultat.Add(measResult);
            //}
            //DataGridElements = rezultat;
        }

        private void ExecuteReadAll(object parameter)
        {
            ///
            /// otvori vezu ka CommEngine i dobavi mjerenja
            ////

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
