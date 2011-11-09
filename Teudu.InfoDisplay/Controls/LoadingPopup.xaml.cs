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
using System.Windows.Media.Animation;

namespace Teudu.InfoDisplay
{
    /// <summary>
    /// Interaction logic for LoadingPopup.xaml
    /// </summary>
    public partial class LoadingPopup : UserControl
    {
        public LoadingPopup()
        {
            InitializeComponent();
            this.MessageContainer.Opacity = 0;
            //Show();
        }
        public string Message
        {
            set
            {
                ShowMessage();
            }
        }

        public void Show(string msg)
        {
            //((System.Windows.Media.Animation.Storyboard)this.Resources["MainShowAnimation"]).Begin();
            Message = msg;
            ShowMessage();
            //((System.Windows.Media.Animation.Storyboard)this.Resources["ShowAnimation"]).Begin();
        }

        public void Hide()
        {
            ((System.Windows.Media.Animation.Storyboard)this.Resources["MainShowAnimation"]).Stop();
            //((System.Windows.Media.Animation.Storyboard)this.Resources["ShowAnimation"]).Stop();
            ((System.Windows.Media.Animation.Storyboard)this.Resources["MainHideAnimation"]).Begin();
        }

        public void ShowMessage()
        {
            ((System.Windows.Media.Animation.Storyboard)this.Resources["ShowAnimation"]).Begin();
        }

        private void MainShowAnimation_Completed(object sender, EventArgs e)
        {
            this.Opacity = 1;
        }

        private void MainHideAnimation_Completed(object sender, EventArgs e)
        {
            this.Opacity = 0;
            this.PopUpTransform.Y = App.Current.MainWindow.ActualHeight;
        }
    }
}
