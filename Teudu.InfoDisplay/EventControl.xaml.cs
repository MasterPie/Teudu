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
    /// Interaction logic for EventControl.xaml
    /// </summary>
    public partial class EventControl : UserControl
    {
        private Event eventModel;

        public EventControl()
        {
            InitializeComponent();
            
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

                src.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\image_stash\" + this.eventModel.Image);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                this.image.Source = src;
            }
        }
    }
}
