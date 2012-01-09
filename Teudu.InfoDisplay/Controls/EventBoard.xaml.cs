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
using System.Configuration;

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
            //ScaleLevel = 1;
            this.Loaded += new RoutedEventHandler(EventBoard_Loaded);
            this.Board.LayoutUpdated += new EventHandler(Board_LayoutUpdated);
        }

        void Board_LayoutUpdated(object sender, EventArgs e)
        {
            int childCount = this.Board.Children.Count;
            foreach (UIElement element in this.Board.Children)
            {
                element.SetValue(Canvas.ZIndexProperty, childCount);
                childCount--;
            }
            contentHeight = this.Board.ActualHeight;
        }

        void EventBoard_Loaded(object sender, RoutedEventArgs e)
        {
            this.Board.MaxWidth = App.Current.MainWindow.ActualWidth - 40;
            this.InvalidateMeasure();
        }

        private double contentHeight = 0;
        public double ContentHeight
        {
            get { return contentHeight; }
        }

        public string BoardTitle
        {
            get
            {
                if (board != null)
                    return board.Name;
                else
                    return "";
            }
        }

        public Board BoardModel
        {
            get { return board; }
            set
            {
                if (value == null)
                    return;

                board = value;
                this.Board.Children.Clear();
                

                int eventWidth = 230;
                int maxEventHeight = 400;


                int numEvents = BoardModel.Events.Count;

                if (numEvents <= 15)
                    this.Height = maxEventHeight * 3 + (20 * 3);
                if(numEvents <= 10)
                    this.Height = maxEventHeight * 2 + (20 * 2);
                if (numEvents <= 5)
                    this.Height = maxEventHeight + 20;

                BoardModel.Events.OrderBy(y=>y.StartTime).ToList<Event>().ForEach(x => this.Board.Children.Add(new EventControl(){
                    Event = x,
                    Width = eventWidth,
                    MaxHeight = maxEventHeight,
                    Margin = new Thickness(0,0,20,20),
                    }));
                
                this.OnPropertyChanged("BoardTitle");
            }
        }

        //public void TrackCenterEvent()
        //{
        //    this.Dispatcher.BeginInvoke(new Action(this.TrackCenterEvent_work), System.Windows.Threading.DispatcherPriority.Loaded);
        //}

        //EventControl currentSelected;
        //public void TrackCenterEvent_work()
        //{
        //    EventControl centerEvent = GetCentermostEvent();
        //    if (centerEvent != null)
        //    {
        //        HoveredEvent = centerEvent.Event;
        //        centerEvent.IsSelected = true;
        //        currentSelected = centerEvent;
        //    }
        //    else
        //    {
        //        HoveredEvent = null;
        //        currentSelected = null;
        //    }

        //    if (currentSelected != null)
        //        currentSelected.IsSelected = false;
        //}

        #region Deprecated
        //private void SnapToNearestEvent()
        //{
        //    UIElement closestElement = GetCentermostEvent();
        //    double centerX = (double)App.Current.MainWindow.ActualWidth / 2;
        //    double centerY = (double)App.Current.MainWindow.ActualHeight / 2;
        //    double deltaX, deltaY;

        //    if (closestElement == null)
        //        return;
            
        //    Point topCorner = closestElement.TransformToAncestor(Board).Transform(new Point(0, 0));
        //    deltaX = topCorner.X - centerX;
        //    deltaY = topCorner.Y - centerY;

        //    Point boardCorner = Board.TranslatePoint(new Point(0, 0), App.Current.MainWindow);//Board.TransformToAncestor(App.Current.MainWindow).Transform(new Point(0,0));

        //    //snapX = boardCorner.X - deltaX;
        //    //snapY = boardCorner.Y - deltaY;

        //    System.Windows.Media.Animation.Storyboard sbdSnapAnimation = (System.Windows.Media.Animation.Storyboard)FindResource("SnapAnimation");
        //    sbdSnapAnimation.Begin(this);
            
            
        //}

        #endregion

        //private EventControl GetCentermostEvent()
        //{
        //    EventControl closestElement = null;
        //    double centerX = App.Current.MainWindow.ActualWidth / 2;//(double)Board.Parent.GetValue(ActualWidthProperty) / 2;
        //    double centerY = App.Current.MainWindow.ActualHeight / 2;// ((double)Board.Parent.GetValue(ActualHeightProperty) / 2);// +100;

        //    double deltaX,deltaY;
        //    deltaX = deltaY = centerX + centerY;

        //    try
        //    {
        //        foreach (UIElement element in Board.Children)
        //        {
        //            if (!element.GetType().AssemblyQualifiedName.ToLower().Contains("eventcontrol"))
        //                continue;

        //            Point topCorner = element.PointToScreen(new Point(0, 0));
        //            double elementCenterX = topCorner.X + (double)element.GetValue(ActualWidthProperty) / 2;
        //            double elementCenterY = topCorner.Y + (double)element.GetValue(ActualHeightProperty) / 2;

        //            //Point topCorner = ((EventControl)element).VisibleLocation;

        //            if (Math.Abs(elementCenterX - centerX) < deltaX || Math.Abs(elementCenterY - centerY) < deltaY)
        //            {
        //                deltaX = Math.Abs(topCorner.X - centerX);
        //                deltaY = Math.Abs(topCorner.Y - centerY);

        //                closestElement = (EventControl)element;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        closestElement = null;
        //    }

        //    return closestElement;
        //}

        //double snapX, snapY;

        //public double SnapToX
        //{
        //    get { return snapX; }
        //}

        //public double SnapToY
        //{
        //    get { return snapY; }
        //}

        //double scaleLevel = 1;
        //public double ScaleLevel
        //{
        //    set { scaleLevel = value; this.OnPropertyChanged("ScaleLevel"); }
        //    get { return scaleLevel; }
        //}

        //double toX, toY;
        
        //public double MoveToX
        //{
        //    set 
        //    {
        //        if ((toX + value) / ScaleLevel >= -((App.Current.MainWindow.Width / 2)+100) && (toX + value) / ScaleLevel <= ((App.Current.MainWindow.Width / 2)+100))
        //        {
        //            toX += value;//this.TranslatePoint(new Point(0, 0), App.Current.MainWindow).X + value;
        //            this.OnPropertyChanged("MoveToX");
        //        }
        //        else
        //        {
        //            if (BoardChanged != null)
        //            {
        //                if ((toX + value) / ScaleLevel >= -((App.Current.MainWindow.Width / 2) + 100))
        //                    BoardChanged(this, new CategoryChangeEventArgs() { Right = false });
        //                else
        //                    BoardChanged(this, new CategoryChangeEventArgs() { Right = true });
        //            }
        //            Trace.WriteLine("Value is out of bounds: " + value);
        //        }
        //    }
        //    get { return toX; }
        //}

        //public double MoveToY
        //{
        //    set 
        //    {
        //        if ((toY + value) / ScaleLevel >= -(App.Current.MainWindow.Height / 2) && (toY + value + 100) / ScaleLevel <= (App.Current.MainWindow.Height / 2))
        //        {
        //            toY += value;// this.TranslatePoint(new Point(0, 0), App.Current.MainWindow).Y + value;
        //            this.OnPropertyChanged("MoveToY");
        //        }
        //    }
        //    get { return toY; }
        //}

        Event currentEvent;
        public Event HoveredEvent
        {
            get { return currentEvent; }
            set { currentEvent = value; if(HoveredEventChanged != null) HoveredEventChanged(this, new HoveredEventArgs(){CurrentEvent = currentEvent});  }
        }

        private void PanAnimationStoryboard_Completed(object sender, EventArgs e)
        {
            //TrackCenterEvent();
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

        private void TranslateTransform_Changed(object sender, EventArgs e)
        {
            //EventControl centerEvent = GetCentermostEvent();
            //if (centerEvent != null)
            //    HoveredEvent = centerEvent.Event;
            //TrackCenterEvent();
        }
    }
}
