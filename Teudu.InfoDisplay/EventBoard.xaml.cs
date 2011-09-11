using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Teudu.InfoDisplay
{
    /// <summary>
    /// Interaction logic for EventBoard.xaml
    /// </summary>
    public partial class EventBoard : UserControl
    {
        private List<Event> events;
        public EventBoard()
        {
            InitializeComponent();
        }

        public List<Event> Events
        {
            get { return events; }
            set
            {
                events = value;
                this.Board.Children.Clear();
                events.ForEach(x => this.Board.Children.Add(new EventControl() { 
                    Event = x, 
                    Width = 170,
                    MinHeight = 210,
                    Margin = new Thickness(5)
                }));
            }
        }

        public static readonly RoutedEvent ShiftEvent = EventManager.RegisterRoutedEvent(
       "Shift", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(EventBoard));

        public static readonly RoutedEvent SnapEvent = EventManager.RegisterRoutedEvent(
       "Snap", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(EventBoard));

        // Provide CLR accessors for the event
        public event RoutedEventHandler Shift
        {
            add { AddHandler(ShiftEvent, value); }
            remove { RemoveHandler(ShiftEvent, value); }
        }

        // Provide CLR accessors for the event
        public event RoutedEventHandler Snap
        {
            add { AddHandler(SnapEvent, value); }
            remove { RemoveHandler(SnapEvent, value); }
        }

        private void DoubleAnimationUsingKeyFrames_Completed(object sender, EventArgs e)
        {
            //SnapToNearestEvent();
        }

        private void SnapToNearestEvent()
        {
            UIElement closestElement = GetCentermostElement();
            double centerX = (double)App.Current.MainWindow.ActualWidth / 2;
            double centerY = (double)App.Current.MainWindow.ActualHeight / 2;
            double deltaX, deltaY;

            if (closestElement == null)
                return;
            
            Point topCorner = closestElement.TransformToAncestor(Board).Transform(new Point(0, 0));
            deltaX = topCorner.X - centerX;
            deltaY = topCorner.Y - centerY;
        
            Point boardCorner = Board.TransformToAncestor(App.Current.MainWindow).Transform(new Point(0,0));

            snapX = boardCorner.X - deltaX;
            snapY = boardCorner.Y - deltaY;

            System.Windows.Media.Animation.Storyboard sbdSnapAnimation = (System.Windows.Media.Animation.Storyboard)FindResource("SnapAnimation");
            sbdSnapAnimation.Begin(this);
            
            
        }

        private UIElement GetCentermostElement()
        {
            UIElement closestElement = null;
            double centerX = (double)Board.Parent.GetValue(ActualWidthProperty) / 2;
            double centerY = (double)Board.Parent.GetValue(ActualHeightProperty) / 2;

            double deltaX,deltaY;
            deltaX = deltaY = centerX + centerY;

            

            foreach (UIElement element in Board.Children)
            {
                if (!element.GetType().AssemblyQualifiedName.ToLower().Contains("rectangle"))
                    continue;

                Point topCorner = element.TransformToAncestor(Board).Transform(new Point(0, 0));

                if (Math.Abs(topCorner.X - centerX) < deltaX || Math.Abs(topCorner.Y - centerY) < deltaY)
                {
                    deltaX = Math.Abs(topCorner.X - centerX);
                    deltaY = Math.Abs(topCorner.Y - centerY);

                    closestElement = element;
                }
            }

            return closestElement;
        }

        double snapX, snapY;

        public double SnapToX
        {
            get { return snapX; }
        }

        public double SnapToY
        {
            get { return snapY; }
        }
    }
}
