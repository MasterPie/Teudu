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
        private DispatcherTimer centerCheckTimer;
        private DispatcherTimer showEventTimer;

        public EventControl()
        {
            InitializeComponent();
            

            imageDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\" + ConfigurationManager.AppSettings["CachedImageDirectory"]  + @"\";

            centerCheckTimer = new DispatcherTimer();
            centerCheckTimer.Interval = TimeSpan.FromMilliseconds(300);
            centerCheckTimer.Tick += new EventHandler(animateLiveTimer_Tick);
            centerCheckTimer.Start();

            showEventTimer = new DispatcherTimer();
            showEventTimer.Interval = TimeSpan.FromMilliseconds(new Random().Next(500));
            showEventTimer.Tick += new EventHandler(showEventTimer_Tick);

            Details.Width = App.Current.MainWindow.ActualWidth / 4;
            Details.Height = App.Current.MainWindow.ActualHeight / 4;
        }

        void showEventTimer_Tick(object sender, EventArgs e)
        {
            ((System.Windows.Media.Animation.Storyboard)this.Resources["AppearAnimation"]).Begin();
            showEventTimer.Stop();
        }

        void animateLiveTimer_Tick(object sender, EventArgs e)
        {
            VisibleLocation_work();

            TranslateTransform shiftLeft = new TranslateTransform(-Details.ActualWidth - App.Current.MainWindow.ActualWidth / 30, 0);
            Details.RenderTransform = shiftLeft;
        }

        public Event Event
        {
            get { return this.eventModel; }
            set 
            {
                this.EventContainer.Opacity = 0;
                this.eventModel = value;

                BitmapImage src = new BitmapImage();
                src.BeginInit();

                src.UriSource = new Uri(imageDirectory + this.eventModel.Image);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                this.image.Source = src;

                this.Details.Event = value;

                showEventTimer.Start();
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
                    Details.Opacity = 0;
                    Details.Visibility = System.Windows.Visibility.Visible;
                    ((System.Windows.Media.Animation.Storyboard)this.Resources["DetailsAppearAnimation"]).Begin();
                    //outerBorder.BorderThickness = new Thickness(5);
                }
                else
                {
                    Details.Visibility = System.Windows.Visibility.Hidden;
                    outerBorder.BorderThickness = new Thickness(0);
                    ((System.Windows.Media.Animation.Storyboard)this.Resources["DetailsAppearAnimation"]).Stop();
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
    }
}
