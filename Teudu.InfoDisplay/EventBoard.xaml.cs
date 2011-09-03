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
    /// Interaction logic for EventBoard.xaml
    /// </summary>
    public partial class EventBoard : UserControl
    {
        public EventBoard()
        {
            InitializeComponent();
        }



        public static readonly RoutedEvent ShiftEvent = EventManager.RegisterRoutedEvent(
       "Shift", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(EventBoard));

        // Provide CLR accessors for the event
        public event RoutedEventHandler Shift
        {
            add { AddHandler(ShiftEvent, value); }
            remove { RemoveHandler(ShiftEvent, value); }
        }
    }
}
