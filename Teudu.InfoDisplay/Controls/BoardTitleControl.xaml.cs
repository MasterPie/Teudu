﻿using System;
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
                this.BoardStats.Text = "15 events in the next 2 hours";
                this.BoardStats.Text = String.Format("{0} events in the next {1} hours", value.Events.Count, (DateTime.Now - value.MaxDate).Hours);
            }
        }

        
    }
}
