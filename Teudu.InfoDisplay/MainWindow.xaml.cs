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
using System.Timers;
using System.Windows.Threading;
using System.ComponentModel;

namespace Teudu.InfoDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ((ViewModel)this.DataContext).PanRequest += new EventHandler(MainWindow_PanRequest);
        }

        void MainWindow_PanRequest(object sender, EventArgs e)
        {
            RaiseShiftEvent();
        }

        private void RaiseShiftEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(EventBoard.ShiftEvent);
            Board.RaiseEvent(newEventArgs);

            //Board.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate() { Board.RaiseEvent(newEventArgs); }));
        }


    }
}
