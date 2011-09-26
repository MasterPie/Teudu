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
                    Margin = new Thickness(10)
                }));
                TrackCenterEvent();
                //SnapToNearestEvent();
            }
        }

        public void TrackCenterEvent()
        {
            this.Dispatcher.BeginInvoke(new Action(this.TrackCenterEvent_work), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        EventControl currentSelected;
        public void TrackCenterEvent_work()
        {
            EventControl centerEvent = GetCentermostEvent();
            if (centerEvent != null)
            {
                HoveredEvent = centerEvent.Event;
                centerEvent.IsSelected = true;
                currentSelected = centerEvent;
            }
            else
            {
                HoveredEvent = null;
                currentSelected = null;
            }

            if (currentSelected != null)
                currentSelected.IsSelected = false;
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
            double centerX = App.Current.MainWindow.ActualWidth / 2;//(double)Board.Parent.GetValue(ActualWidthProperty) / 2;
            double centerY = App.Current.MainWindow.ActualHeight / 2;// ((double)Board.Parent.GetValue(ActualHeightProperty) / 2);// +100;

            double deltaX,deltaY;
            deltaX = deltaY = centerX + centerY;

            try
            {
                foreach (UIElement element in Board.Children)
                {
                    if (!element.GetType().AssemblyQualifiedName.ToLower().Contains("eventcontrol"))
                        continue;

                    Point topCorner = element.PointToScreen(new Point(0, 0));
                    double elementCenterX = topCorner.X + (double)element.GetValue(ActualWidthProperty) / 2;
                    double elementCenterY = topCorner.Y + (double)element.GetValue(ActualHeightProperty) / 2;

                    //Point topCorner = ((EventControl)element).VisibleLocation;

                    if (Math.Abs(elementCenterX - centerX) < deltaX || Math.Abs(elementCenterY - centerY) < deltaY)
                    {
                        deltaX = Math.Abs(topCorner.X - centerX);
                        deltaY = Math.Abs(topCorner.Y - centerY);

                        closestElement = (EventControl)element;
                    }
                }
            }
            catch (Exception)
            {
                closestElement = null;
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
                if ((toX + value) / ScaleLevel >= -((App.Current.MainWindow.Width / 2)+100) && (toX + value) / ScaleLevel <= ((App.Current.MainWindow.Width / 2)+100))
                {
                    toX += value;//this.TranslatePoint(new Point(0, 0), App.Current.MainWindow).X + value;
                    this.OnPropertyChanged("MoveToX");
                }
                else
                {
                    if (BoardChanged != null)
                    {
                        if ((toX + value) / ScaleLevel >= -((App.Current.MainWindow.Width / 2) + 100))
                            BoardChanged(this, new CategoryChangeEventArgs() { Right = false });
                        else
                            BoardChanged(this, new CategoryChangeEventArgs() { Right = true });
                    }
                    Trace.WriteLine("Value is out of bounds: " + value);
                }
            }
            get { return toX; }
        }

        public double MoveToY
        {
            set 
            {
                if ((toY + value) / ScaleLevel >= -(App.Current.MainWindow.Height / 2) && (toY + value + 100) / ScaleLevel <= (App.Current.MainWindow.Height / 2))
                {
                    toY += value;// this.TranslatePoint(new Point(0, 0), App.Current.MainWindow).Y + value;
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
            TrackCenterEvent();
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
        public event EventHandler<CategoryChangeEventArgs> BoardChanged;

        private void TranslateTransform_Changed(object sender, EventArgs e)
        {
            //EventControl centerEvent = GetCentermostEvent();
            //if (centerEvent != null)
            //    HoveredEvent = centerEvent.Event;
            TrackCenterEvent();
        }
    }
}
