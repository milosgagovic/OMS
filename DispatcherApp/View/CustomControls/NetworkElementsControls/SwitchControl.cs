using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DispatcherApp.View.CustomControls
{
    public class SwitchControl : Grid
    {
        public Button ButtonCanvas { get; set; }
        public Canvas Canvas1 { get; set; }
        public Canvas Canvas2 { get; set; }

        private List<SolidColorBrush> ClosedPalette = new List<SolidColorBrush>();
        private List<SolidColorBrush> OpenedPalette = new List<SolidColorBrush>();
        private Dictionary<int, List<SolidColorBrush>> Palettes = new Dictionary<int, List<SolidColorBrush>>();

        private List<SolidColorBrush> ActivePalette;

        public SwitchControl(object dataContext, double buttonSize /*double left, double top, int z*/)
        {
            #region Init
            FrameworkElement frameworkElement = new FrameworkElement();

            this.DataContext = dataContext;

            //Canvas.SetLeft(this, left);
            //Canvas.SetTop(this, top);
            //Canvas.SetZIndex(this, z);

            this.ButtonCanvas = new Button();
            this.Canvas1 = new Canvas();
            this.Canvas2 = new Canvas();

            this.ClosedPalette.Add((SolidColorBrush)frameworkElement.FindResource("SwitchColorClosed"));
            this.ClosedPalette.Add((SolidColorBrush)frameworkElement.FindResource("SwitchColorMouseOverClosed"));
            this.ClosedPalette.Add((SolidColorBrush)frameworkElement.FindResource("SwitchColorPressedClosed"));

            this.OpenedPalette.Add((SolidColorBrush)frameworkElement.FindResource("SwitchColorClosedDeenergized"));
            this.OpenedPalette.Add((SolidColorBrush)frameworkElement.FindResource("SwitchColorMouseOverClosedDeenergized"));
            this.OpenedPalette.Add((SolidColorBrush)frameworkElement.FindResource("SwitchColorPressedClosedDeenergized"));

            this.Palettes.Add(0, this.ClosedPalette);
            this.Palettes.Add(1, this.OpenedPalette);

            this.ActivePalette = this.Palettes[0];

            ColumnDefinition buttonCol = new ColumnDefinition();
            buttonCol.Width = new GridLength(buttonSize);
            ColumnDefinition space1Col = new ColumnDefinition();
            space1Col.Width = new GridLength(2);
            ColumnDefinition canvas1Col = new ColumnDefinition();
            canvas1Col.Width = new GridLength(buttonSize);
            ColumnDefinition canvas2Col = new ColumnDefinition();
            canvas2Col.Width = new GridLength(buttonSize);

            this.ColumnDefinitions.Add(buttonCol);
            this.ColumnDefinitions.Add(space1Col);
            this.ColumnDefinitions.Add(canvas1Col);
            this.ColumnDefinitions.Add(canvas2Col);

            RowDefinition mainRow = new RowDefinition();
            mainRow.Height = new GridLength(buttonSize);
            this.RowDefinitions.Add(mainRow);

            this.Children.Add(this.ButtonCanvas);
            Grid.SetColumn(this.ButtonCanvas, 0);
            this.Children.Add(this.Canvas1);
            Grid.SetColumn(this.Canvas1, 2);
            this.Children.Add(this.Canvas2);
            Grid.SetColumn(this.Canvas2, 3);

            this.Canvas1.Background = new ImageBrush(new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/../../View/Resources/Images/incident.png")));
            this.Canvas2.Background = new ImageBrush(new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "/../../View/Resources/Images/crew.png")));

            this.Canvas1.VerticalAlignment = VerticalAlignment.Center;
            this.Canvas1.HorizontalAlignment = HorizontalAlignment.Center;
            this.Canvas1.Height = buttonSize - 5;
            this.Canvas1.Width = buttonSize - 5;
            this.Canvas2.VerticalAlignment = VerticalAlignment.Center;
            this.Canvas2.HorizontalAlignment = HorizontalAlignment.Center;
            this.Canvas2.Height = buttonSize - 5;
            this.Canvas2.Width = buttonSize - 5;
            #endregion

            #region Style

            Style style = new Style();
            style.TargetType = typeof(Button);

            Setter setter = new Setter();
            setter.Property = Button.TemplateProperty;

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory elemFactory = new FrameworkElementFactory(typeof(Border));
            elemFactory.Name = "Border";
            elemFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(1));
            elemFactory.SetValue(Border.BackgroundProperty, this.ActivePalette[0]);
            template.VisualTree = elemFactory;

            Trigger trigger1 = new Trigger();
            trigger1.Property = Button.IsMouseOverProperty;
            trigger1.Value = true;

            Setter setter1 = new Setter();
            setter1.Property = Border.BackgroundProperty;
            setter1.Value = this.ActivePalette[1];
            setter1.TargetName = "Border";
            trigger1.Setters.Add(setter1);

            template.Triggers.Add(trigger1);

            Trigger trigger2 = new Trigger();
            trigger2.Property = Button.IsPressedProperty;
            trigger2.Value = true;

            Setter setter2 = new Setter();
            setter2.Property = Border.BackgroundProperty;
            setter2.Value = this.ActivePalette[2];
            setter2.TargetName = "Border";
            trigger2.Setters.Add(setter2);

            template.Triggers.Add(trigger2);

            DataTrigger dataTrigger1 = new DataTrigger();
            dataTrigger1.Binding = new Binding("IsEnergized");
            dataTrigger1.Value = false;

            Setter dataSetter1 = new Setter();
            dataSetter1.Property = Border.BackgroundProperty;
            dataSetter1.Value = Brushes.Blue;
            dataSetter1.TargetName = "Border";
            dataTrigger1.Setters.Add(dataSetter1);

            template.Triggers.Add(dataTrigger1);

            setter.Value = template;

            style.Triggers.Clear();
            style.Setters.Add(setter);
            this.ButtonCanvas.Style = style;

            this.Canvas1.SetBinding(Canvas.VisibilityProperty, new Binding("Incident") { Converter = new BooleanToVisibilityConverter() });
            this.Canvas2.SetBinding(Canvas.VisibilityProperty, new Binding("CrewSent") { Converter = new BooleanToVisibilityConverter() });
            #endregion
        }
    }
}
