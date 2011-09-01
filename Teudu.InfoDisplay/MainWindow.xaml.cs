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

namespace Teudu.InfoDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ScaleTransform scaler;
        TransformGroup transformer;
        Timer timer;
        public MainWindow()
        {
            InitializeComponent();

            

            timer = new Timer(2000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            transformer = new TransformGroup();
            scaler = new ScaleTransform(1, 1);
            transformer.Children.Add(scaler);
            
            //Board.RenderTransform = transformer;

            //timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            System.Random rand = new Random();
            Dispatcher.BeginInvoke((Action)delegate
            {
                scaler.ScaleX = scaler.ScaleY = rand.Next(1,6);
            });
            
            
        }
    }
}
