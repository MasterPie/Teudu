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
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ((ViewModel)this.DataContext).BoardsUpdated += new EventHandler<BoardEventArgs>(MainWindow_BoardsUpdated);
            ((ViewModel)this.DataContext).BeginBackgroundJobs();
            LoadingContainer.Show("");
        }

        void MainWindow_BoardsUpdated(object sender, BoardEventArgs e)
        {
            this.BoardNavigator.BoardMaster = e.BoardService;
            LoadingContainer.Hide();
        }

        public void PopStatusMessage(string msg)
        {
            LoadingContainer.Opacity = 1;
        }

        public void HideStatusMessage()
        {
            LoadingContainer.Opacity = 0;
        }

        public string VisibleLocation
        {
            get
            {
                double centerX = this.ActualWidth / 2;
                double centerY = this.ActualHeight / 2;
                return "(" + centerX + ", " + centerY + ")";
            }
        }       
    }
}
