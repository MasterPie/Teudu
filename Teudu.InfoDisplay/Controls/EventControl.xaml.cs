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
using System.Configuration;
using System.ComponentModel;
using System.Windows.Threading;

namespace Teudu.InfoDisplay
{
    /// <summary>
    /// Interaction logic for EventControl.xaml
    /// </summary>
    public partial class EventControl : UserControl, INotifyPropertyChanged
    {
        private Event eventModel;
        private string imageDirectory;
        private DispatcherTimer checkRecencyTimer;
        private DispatcherTimer slideUpTimer;
        private DispatcherTimer centerCheckTimer;
        private DispatcherTimer showEventTimer;
        private bool slideEnabled = false;

        public EventControl()
        {
            InitializeComponent();
            

            imageDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\" + ConfigurationManager.AppSettings["CachedImageDirectory"]  + @"\";
            Boolean.TryParse(ConfigurationManager.AppSettings["TileSlidesEnabled"], out slideEnabled);

            centerCheckTimer = new DispatcherTimer();
            centerCheckTimer.Interval = TimeSpan.FromMilliseconds(300);
            centerCheckTimer.Tick += new EventHandler(animateLiveTimer_Tick);
            centerCheckTimer.Start();

            showEventTimer = new DispatcherTimer();
            showEventTimer.Interval = TimeSpan.FromMilliseconds(ThreadSafeRandom.Next(2500));
            showEventTimer.Tick += new EventHandler(showEventTimer_Tick);

            slideUpTimer = new DispatcherTimer();
            slideUpTimer.Interval = TimeSpan.FromSeconds(60);
            slideUpTimer.Tick += new EventHandler(slideUpTimer_Tick);

            checkRecencyTimer = new DispatcherTimer();
            checkRecencyTimer.Interval = TimeSpan.FromMinutes(10);
            checkRecencyTimer.Tick += new EventHandler(checkRecencyTimer_Tick);

            Details.Width = App.Current.MainWindow.ActualWidth / 3.5;
            //Details.Height = App.Current.MainWindow.ActualHeight / 3.5;
        }

        void checkRecencyTimer_Tick(object sender, EventArgs e)
        {
            slideUpTimer.Interval = TimeSpan.FromSeconds(GetSlideUpFrequency());
            EmphasizeIfNeeded();
        }

        private void EmphasizeIfNeeded()
        {
            TimeSpan recency = GetRecency();

            //if(recency.
        }

        void slideUpTimer_Tick(object sender, EventArgs e)
        {
            slideUpTimer.Stop();
            ((System.Windows.Media.Animation.Storyboard)this.Resources["SlideUpAnimation"]).Begin();
        }

        void showEventTimer_Tick(object sender, EventArgs e)
        {
            ((System.Windows.Media.Animation.Storyboard)this.Resources["AppearAnimation"]).Begin();
            showEventTimer.Stop();
            if (slideEnabled)
            {
                checkRecencyTimer.Start();
                slideUpTimer.Start();
            }
        }

        void animateLiveTimer_Tick(object sender, EventArgs e)
        {
            VisibleLocation_work();

            TranslateTransform shiftLeft = new TranslateTransform(-Details.ActualWidth - App.Current.MainWindow.ActualWidth / 30, 0);
            Details.RenderTransform = shiftLeft;
        }

        private TimeSpan GetRecency()
        {
            DateTime now = DateTime.Now;
            TimeSpan recency = eventModel.StartTime - now;
            return recency;
        }

        private int GetSlideUpFrequency()
        {
            int secondsFrequency = 3600;
            TimeSpan recency = GetRecency();

            if (recency.TotalDays <= 3) //TODO: remove
                secondsFrequency = 60;
            if (recency.TotalHours < 5)
                secondsFrequency = 30;
            if (recency.TotalMinutes < 60)
                secondsFrequency = 15;
            if (recency.TotalMinutes < 30)
                secondsFrequency = 10;
            if (recency.TotalMinutes < 5)
                secondsFrequency = 5;

            return secondsFrequency;
        }

        public Event Event
        {
            get { return this.eventModel; }
            set 
            {
                this.EventContainer.Opacity = 0;
                this.eventModel = value;

                try
                {
                    BitmapImage src = new BitmapImage();
                    src.BeginInit();

                    src.UriSource = new Uri(imageDirectory + this.eventModel.Image);
                    src.CacheOption = BitmapCacheOption.OnLoad;
                    src.EndInit();
                    this.image.Source = src;
                    
                }
                catch (Exception e)
                {

                    System.Diagnostics.Trace.WriteLine(e.ToString());
                }

                this.Details.Event = value;
                slideUpTimer.Interval = TimeSpan.FromSeconds(GetSlideUpFrequency());
                showEventTimer.Start();
            }
        }

        public string StartsIn
        {
            get
            {
                TimeSpan recency = eventModel.StartTime - DateTime.Now;

                return Helper.ToSensibleDate("",recency.TotalMinutes);
            }
        }

        Point loc = new Point(0, 0);

        public string VisibleLocation
        {
            get
            {
                return "(" + Math.Round(loc.X, 1) + ", " + Math.Round(loc.Y,1) + " : " + this.ActualHeight + ")";               
            }
        }

        public void VisibleLocation_work()
        {
            if (!this.IsVisible)
                return;

            double centerX =  App.Current.MainWindow.ActualWidth / 2;
            double centerY = App.Current.MainWindow.ActualHeight / 2;

            Point topCorner = this.TranslatePoint(new Point(0, 0), App.Current.MainWindow);
            double elementCenterX = topCorner.X + this.ActualWidth / 2;
            double elementCenterY = topCorner.Y + this.ActualHeight / 2;

            loc = new Point(elementCenterX, elementCenterY);

            if (Math.Abs(centerX - elementCenterX) <= (this.ActualWidth / 2) && Math.Abs(centerY - elementCenterY) <= (this.ActualHeight / 2))
                IsSelected = true;
            else
                IsSelected = false;

            this.OnPropertyChanged("VisibleLocation");
        }

        public bool IsSelected
        {
            set
            {
                if (value)
                {
                    Details.Opacity = 1;
                    Details.Visibility = System.Windows.Visibility.Visible;
                    //((System.Windows.Media.Animation.Storyboard)this.Resources["DetailsAppearAnimation"]).Begin();
                    //outerBorder.BorderThickness = new Thickness(5);
                }
                else
                {
                    Details.Opacity = 0;
                    Details.Visibility = System.Windows.Visibility.Hidden;
                    outerBorder.BorderThickness = new Thickness(0);
                    //((System.Windows.Media.Animation.Storyboard)this.Resources["DetailsAppearAnimation"]).Stop();
                }
            }
        }

        void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AppearAnimation_Completed(object sender, EventArgs e)
        {
            this.EventContainer.Opacity = 100;
        }

        private void SlideUp_Completed(object sender, EventArgs e)
        {
            slideUpTimer.Start();
        }
    }
}
