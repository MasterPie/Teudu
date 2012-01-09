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
    /// Interaction logic for EventSashControl.xaml
    /// </summary>
    public partial class EventSashControl : UserControl, INotifyPropertyChanged
    {
        public EventSashControl()
        {
            InitializeComponent();
        }

        private string text;
        public string Text
        {
            get { return text; }
            set { text = value; this.OnPropertyChanged("Text"); }
        }

        private Color backgroundColor;
        public Color TriangleFill
        {
            get { return backgroundColor; }
            set { backgroundColor = value; this.OnPropertyChanged("TriangleFill"); }
        }

        private SolidColorBrush textColor;
        public SolidColorBrush TextColor
        {
            get { return textColor; }
            set { textColor = value; this.OnPropertyChanged("TextColor"); }
        }

        private double fontSize;
        [LocalizabilityAttribute(LocalizationCategory.None)]
        [TypeConverterAttribute(typeof(FontSizeConverter))]
        public double TextSize
        {
            get { return fontSize; }
            set { fontSize = value; this.OnPropertyChanged("TextSize"); }
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
