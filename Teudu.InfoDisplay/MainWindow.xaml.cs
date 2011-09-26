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
            ((ViewModel)this.DataContext).PanRequest += new EventHandler<PanEventArgs>(MainWindow_PanRequest);
            ((ViewModel)this.DataContext).ScaleRequest += new EventHandler<ScaleEventArgs>(MainWindow_ScaleRequest);

            current = new EventBoard();
            current.Initialized += new EventHandler(current_Initialized);
            this.BoardContainer.Children.Add(current);
            
        }

        void current_PanCompleted(object sender, EventArgs e)
        {
            ((ViewModel)this.DataContext).PanRequest += new EventHandler<PanEventArgs>(MainWindow_PanRequest);
        }

        void allowEventHandlingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ((ViewModel)this.DataContext).ScaleRequest += new EventHandler<ScaleEventArgs>(MainWindow_ScaleRequest);
            ((ViewModel)this.DataContext).PanRequest += new EventHandler<PanEventArgs>(MainWindow_PanRequest);

        }

        void MainWindow_ScaleRequest(object sender, ScaleEventArgs e)
        {
            RaiseStopShiftEvent(); 
            current.ScaleLevel = e.ScaleLevel;
        }

        void MainWindow_BoardChanged(object sender, BoardEventArgs e)
        {
            BoardChanging(new EventBoard() { BoardModel = e.Board});
            
            
        }

        private void BoardChanging(EventBoard newBoard)
        {
            //do some interesting animation
            current.Initialized -= new EventHandler(current_Initialized);
            current.BoardChanged -= new EventHandler<CategoryChangeEventArgs>(current_BoardChanged);
            current.PanCompleted -= new EventHandler(current_PanCompleted);
            current.HoveredEventChanged -= new EventHandler<HoveredEventArgs>(current_HoveredEventChanged);
            current = newBoard;
            this.BoardContainer.Children.Clear();
            this.BoardContainer.Children.Add(current);
            current.Initialized += new EventHandler(current_Initialized);
        }

        void current_Initialized(object sender, EventArgs e)
        {
            current.TrackCenterEvent();
            current.BoardChanged += new EventHandler<CategoryChangeEventArgs>(current_BoardChanged);
            current.PanCompleted += new EventHandler(current_PanCompleted);
            current.HoveredEventChanged += new EventHandler<HoveredEventArgs>(current_HoveredEventChanged);
        }

        void current_BoardChanged(object sender, CategoryChangeEventArgs e)
        {
            ((ViewModel)this.DataContext).ChangeBoard(e.Right);
        }

        void current_HoveredEventChanged(object sender, HoveredEventArgs e)
        {
            this.EventDetails.EventTitle.Text = e.CurrentEvent.Name;
            this.EventDetails.EventDescription.Text = e.CurrentEvent.Description;
        }

        void MainWindow_PanRequest(object sender, PanEventArgs e)
        {
            current.MoveToX = e.HorizontalOffset;
            current.MoveToY = e.VerticalOffset;
            
            RaiseShiftEvent();
        }

        private void RaiseStopShiftEvent()
        {
            ((ViewModel)this.DataContext).PanRequest -= new EventHandler<PanEventArgs>(MainWindow_PanRequest);
            ((ViewModel)this.DataContext).ScaleRequest -= new EventHandler<ScaleEventArgs>(MainWindow_ScaleRequest);

            RoutedEventArgs newEventArgs = new RoutedEventArgs(EventBoard.StopShiftEvent);
            current.RaiseEvent(newEventArgs);

            ((ViewModel)this.DataContext).ScaleRequest += new EventHandler<ScaleEventArgs>(MainWindow_ScaleRequest);
            ((ViewModel)this.DataContext).PanRequest += new EventHandler<PanEventArgs>(MainWindow_PanRequest);

        }

        private void RaiseShiftEvent()
        {
            ((ViewModel)this.DataContext).PanRequest -= new EventHandler<PanEventArgs>(MainWindow_PanRequest);

            RoutedEventArgs newEventArgs = new RoutedEventArgs(EventBoard.ShiftEvent);
            current.RaiseEvent(newEventArgs);

            //Board.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate() { Board.RaiseEvent(newEventArgs); }));
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
