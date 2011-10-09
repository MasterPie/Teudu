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

namespace Teudu.InfoDisplay
{
    /// <summary>
    /// Interaction logic for EventDetailsControl.xaml
    /// </summary>
    public partial class EventDetailsControl : UserControl, INotifyPropertyChanged
    {
        public EventDetailsControl()
        {
            InitializeComponent();
        }

        private Event eventModel;
        public Event Event
        {
            set 
            { 
                eventModel = value;
                this.OnPropertyChanged("Title");
                this.OnPropertyChanged("Description");
                this.OnPropertyChanged("Date");
            }
        }

        public string Title
        {
            get { return eventModel.Name; }
        }

        public string Date
        {
            get { return eventModel.PrettyTime(); }
        }

        public string Description
        {
            get { return eventModel.Description; }
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
