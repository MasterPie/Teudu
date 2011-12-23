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
                if (value.Events.Count > 0)
                    this.BoardStats.Text = String.Format("{0} events {1}.", value.Events.Count, Helper.ToCountableTime("in the next", value.Events.Max(x => x.StartTime)));
                else
                    this.BoardStats.Text = "There are no events.";
            }
        }
    }
}
