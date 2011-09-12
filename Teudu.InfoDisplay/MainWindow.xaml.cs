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
        private EventBoard current;

        public MainWindow()
        {
            InitializeComponent();
            ((ViewModel)this.DataContext).ActiveBoardChanged += new EventHandler<BoardEventArgs>(MainWindow_BoardChanged);
            ((ViewModel)this.DataContext).PanRequest += new EventHandler(MainWindow_PanRequest);
            ((ViewModel)this.DataContext).ScaleRequest += new EventHandler(MainWindow_ScaleRequest);

            current = new EventBoard();//{BoardModel = ((ViewModel)this.DataContext).CurrentBoard};
            this.BoardContainer.Children.Add(current);
        }

        void MainWindow_ScaleRequest(object sender, EventArgs e)
        {
            RaiseStopShiftEvent();
        }

        void MainWindow_BoardChanged(object sender, BoardEventArgs e)
        {
            BoardChanging(new EventBoard() { BoardModel = e.Board });
            
            this.BoardContainer.Children.Clear();
            this.BoardContainer.Children.Add(current);
        }

        private void BoardChanging(EventBoard newBoard)
        {
            //do some interesting animation
            current = newBoard;
        }

        void MainWindow_PanRequest(object sender, EventArgs e)
        {
            RaiseShiftEvent();
        }

        private void RaiseStopShiftEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(EventBoard.StopShiftEvent);
            current.RaiseEvent(newEventArgs);
        }

        private void RaiseShiftEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(EventBoard.ShiftEvent);
            current.RaiseEvent(newEventArgs);

            //Board.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate() { Board.RaiseEvent(newEventArgs); }));
        }


    }
}
