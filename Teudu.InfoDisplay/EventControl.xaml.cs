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

namespace Teudu.InfoDisplay
{
    /// <summary>
    /// Interaction logic for EventControl.xaml
    /// </summary>
    public partial class EventControl : UserControl, INotifyPropertyChanged
    {
        private Event eventModel;
        private string imageDirectory;

        public EventControl()
        {
            InitializeComponent();
            imageDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\" + ConfigurationManager.AppSettings["CachedImageDirectory"]  + @"\";
            //this.LayoutUpdated += new EventHandler(EventControl_LayoutUpdated);
        }

        void EventControl_LayoutUpdated(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(this.VisibleLocation_work), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        public Event Event
        {
            get { return this.eventModel; }
            set 
            { 
                this.eventModel = value;
                this.title.Text = eventModel.Name;
                this.date.Text = eventModel.Time.ToShortTimeString();
                //this.image.Source = eventModel.Image;
                this.description.Text = eventModel.Description;

                BitmapImage src = new BitmapImage();
                src.BeginInit();

                src.UriSource = new Uri(imageDirectory + this.eventModel.Image);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                this.image.Source = src;
            }
        }

        Point loc = new Point(0, 0);

        public string VisibleLocation
        {
            get
            {
                return "(" + Math.Round(loc.X, 1) + ", " + Math.Round(loc.Y,1) + ")";               
            }
        }

        public void VisibleLocation_work()
        {
            double centerX = App.Current.MainWindow.ActualWidth / 2;
            double centerY = App.Current.MainWindow.ActualHeight / 2;

            Point topCorner = this.PointToScreen(new Point(0, 0));
            double elementCenterX = topCorner.X + (double)this.GetValue(ActualWidthProperty) / 2;
            double elementCenterY = topCorner.Y + (double)this.GetValue(ActualHeightProperty) / 2;

            loc = new Point(elementCenterX, elementCenterY);
            this.OnPropertyChanged("VisibleLocation");
        }

        public bool IsSelected
        {
            set
            {
                if (value)
                    outerBorder.BorderThickness = new Thickness(5);
                else
                    outerBorder.BorderThickness = new Thickness(0);
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
    }
}
