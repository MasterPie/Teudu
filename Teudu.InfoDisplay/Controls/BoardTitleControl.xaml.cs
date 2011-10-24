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
    /// Interaction logic for BoardTitleControl.xaml
    /// </summary>
    public partial class BoardTitleControl : UserControl
    {
        public BoardTitleControl()
        {
            InitializeComponent();
        }

        public Board Board
        {
            set
            {
                this.BoardTitle.Text = value.Title;
                this.BoardStats.Text = String.Format("{0} events in the next {1}.", value.Events.Count, ToSensibleDate((value.Events.Max(x=>x.StartTime) - DateTime.Now).TotalMinutes));
            }
        }

        private string ToSensibleDate(double minutes)
        {
            string sensibleDate = "";
            if (minutes < 60)
                sensibleDate = minutes.ToString() + " minutes";

            int hours = TimeSpan.FromMinutes(minutes).Hours;

            if (hours < 24)
                sensibleDate = hours.ToString() + " hours";
            else
                sensibleDate = TimeSpan.FromMinutes(minutes).Days + " days";

            return sensibleDate;

        }

        
    }
}
