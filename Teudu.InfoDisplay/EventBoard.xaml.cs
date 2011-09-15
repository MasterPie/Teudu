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
                    MinHeight = 210,
                    Margin = new Thickness(5)
                }));
            }
        }


        //public void Shift2()
        //{
        //    PanAnimationStoryboard.Seek(new TimeSpan(0));
        //    PanAnimationStoryboard.Begin();
        //}

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

        private void PanAnimationStoryboard_Completed(object sender, EventArgs e)
        {
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
    }
}
