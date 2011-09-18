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
using System.ComponentModel;
using System.Diagnostics;

namespace Teudu.InfoDisplay
{
    /// <summary>
    /// Interaction logic for EventBoard.xaml
    /// </summary>
    public partial class EventBoard : UserControl, INotifyPropertyChanged
    {
        private Board board;
        public EventBoard()
        {
            InitializeComponent();
            ScaleLevel = 1;
        }

        public Board BoardModel
        {
            get { return board; }
            set
            {
                board = value;
                this.Board.Children.Clear();
                BoardModel.Events.ForEach(x => this.Board.Children.Add(new EventControl()
                {
                    Event = x,
                    Width = 170,
                    Margin = new Thickness(5)
                }));
                TrackCenterEvent();
            }
        }

        public void TrackCenterEvent()
        {
            EventControl centerEvent = GetCentermostEvent();
            if (centerEvent != null)
                HoveredEvent = centerEvent.Event;

            HoveredEvent = BoardModel.Events[0];
        }

        public static readonly RoutedEvent StopShiftEvent = EventManager.RegisterRoutedEvent(
       "StopShift", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(EventBoard));

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
        public event RoutedEventHandler StopShift
        {
            add { AddHandler(StopShiftEvent, value); }
            remove { RemoveHandler(StopShiftEvent, value); }
        }

        // Provide CLR accessors for the event
        public event RoutedEventHandler Snap
        {
            add { AddHandler(SnapEvent, value); }
            remove { RemoveHandler(SnapEvent, value); }
        }


        private void SnapToNearestEvent()
        {
            UIElement closestElement = GetCentermostEvent();
            double centerX = (double)App.Current.MainWindow.ActualWidth / 2;
            double centerY = (double)App.Current.MainWindow.ActualHeight / 2;
            double deltaX, deltaY;

            if (closestElement == null)
                return;
            
            Point topCorner = closestElement.TransformToAncestor(Board).Transform(new Point(0, 0));
            deltaX = topCorner.X - centerX;
            deltaY = topCorner.Y - centerY;

            Point boardCorner = Board.TranslatePoint(new Point(0, 0), App.Current.MainWindow);//Board.TransformToAncestor(App.Current.MainWindow).Transform(new Point(0,0));

            snapX = boardCorner.X - deltaX;
            snapY = boardCorner.Y - deltaY;

            System.Windows.Media.Animation.Storyboard sbdSnapAnimation = (System.Windows.Media.Animation.Storyboard)FindResource("SnapAnimation");
            sbdSnapAnimation.Begin(this);
            
            
        }

        private EventControl GetCentermostEvent()
        {
            EventControl closestElement = null;
            double centerX = (double)Board.Parent.GetValue(ActualWidthProperty) / 2;
            double centerY = ((double)Board.Parent.GetValue(ActualHeightProperty) / 2) + 100;

            double deltaX,deltaY;
            deltaX = deltaY = centerX + centerY;
            

            foreach (UIElement element in Board.Children)
            {
                if (!element.GetType().AssemblyQualifiedName.ToLower().Contains("eventcontrol"))
                    continue;

                Point topCorner = element.TransformToAncestor(Board).Transform(new Point(0, 0));
                double elementCenterX = topCorner.X + (double)element.GetValue(ActualWidthProperty) / 2;
                double elementCenterY = topCorner.Y + (double)element.GetValue(ActualHeightProperty) / 2;

                if (Math.Abs(topCorner.X - centerX) < deltaX || Math.Abs(topCorner.Y - centerY) < deltaY)
                {
                    deltaX = Math.Abs(topCorner.X - centerX);
                    deltaY = Math.Abs(topCorner.Y - centerY);

                    closestElement = (EventControl)element;
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

        double scaleLevel = 1;
        public double ScaleLevel
        {
            set { scaleLevel = value; this.OnPropertyChanged("ScaleLevel"); }
            get { return scaleLevel; }
        }

        double toX, toY;
        
        public double MoveToX
        {
            set 
            {
                if (value / ScaleLevel >= -(App.Current.MainWindow.Width / 2) && value / ScaleLevel <= (App.Current.MainWindow.Width / 2))
                {
                    toX = this.TranslatePoint(new Point(0, 0), App.Current.MainWindow).X + value;
                    this.OnPropertyChanged("MoveToX");
                }
            }
            get { return toX; }
        }

        public double MoveToY
        {
            set 
            {
                if (value / ScaleLevel >= -(App.Current.MainWindow.Height / 2) && value / ScaleLevel <= (App.Current.MainWindow.Height / 2))
                {
                    toY = this.TranslatePoint(new Point(0, 0), App.Current.MainWindow).Y + value;
                    this.OnPropertyChanged("MoveToY");
                }
            }
            get { return toY; }
        }

        Event currentEvent;
        public Event HoveredEvent
        {
            get { return currentEvent; }
            set { currentEvent = value; if(HoveredEventChanged != null) HoveredEventChanged(this, new HoveredEventArgs(){CurrentEvent = currentEvent});  }
        }

        private void PanAnimationStoryboard_Completed(object sender, EventArgs e)
        {
            EventControl centerEvent = GetCentermostEvent();
            if (centerEvent != null)
                HoveredEvent = centerEvent.Event;
            //SnapToNearestEvent();
            if (PanCompleted != null)
                PanCompleted(this, new EventArgs());
        }

        void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event EventHandler PanCompleted;
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<HoveredEventArgs> HoveredEventChanged;
    }
}
