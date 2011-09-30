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
    /// Interaction logic for BoardNavigatorControl.xaml
    /// </summary>
    public partial class BoardNavigatorControl : UserControl
    {
        public BoardNavigatorControl()
        {
            InitializeComponent();
        }

        private IBoardService boardMaster;
        public IBoardService BoardMaster
        {
            set 
            { 
                //stop monitoring interactions
                boardMaster = value;
                Current = boardMaster.Current;
                Next = boardMaster.Next;
                //JumpToCenter();
                //begin monitoring
            }
        }

        private Board prev, current, next;
        public Board Previous
        {
            set
            {
                prev = value;
                this.Dispatcher.BeginInvoke(new Action(this.LoadPreviousBoard), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void LoadPreviousBoard()
        {
            //do slick animation
            PreviousBoard.BoardModel = prev;
        }

        public Board Current
        {
            set
            {
                current = value;
                CurrentBoard.BoardModel = current;
            }
        }

        public Board Next
        {
            set
            {
                next = value;
                this.Dispatcher.BeginInvoke(new Action(this.LoadNextBoard), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void LoadNextBoard()
        {
            //do slick animation
            NextBoard.BoardModel = next;
        }

        private void Advance()
        {
            if (!boardMaster.AdvanceCurrent())
                return; 
            //animation => refresh prev, next boards
            Current = next;
            //jump to current board
            JumpToCenter();
            Previous = current;
            Next = boardMaster.Next;
            
        }

        private void Regress()
        {
            if (!boardMaster.RegressCurrent())
                return;
            //animation, completed => refresh prev, next boards
            Current = prev;
            //jump to current board
            JumpToCenter();
            Next = current;
            Previous = boardMaster.Prev;
        }

        private void JumpToCenter()
        {
            //TranslateTransform shift = new TranslateTransform(-this.ActualWidth, 0);
            //BoardContainer.RenderTransform = shift;
            Canvas.SetLeft(BoardContainer, -this.ActualWidth);
        }

        public Point BoardContainerOffset
        {
            get
            {
                return BoardContainer.TransformToAncestor(this).Transform(new Point(0, 0));
            }
        }

        private bool GoPrevious()
        {
            //Point containerCenter = this.TransformToAncestor(App.Current.MainWindow).Transform(new Point(BoardContainer.ActualWidth/2,0));
            return BoardMidLocation().X > BoardContainer.ActualWidth/2;
        }

        private bool GoNext()
        {
            return (BoardMidLocation().X + this.ActualWidth) < 0; ///TODO: change to be based on event width
        }

        private Point BoardMidCoords()
        {
            double midY = this.BoardContainer.ActualHeight / 2;
            double midX = this.BoardContainer.ActualWidth / 2;

            return new Point(midX, midY);
        }

        private Point BoardMidLocation()
        {
            return BoardContainer.TransformToAncestor(this).Transform(BoardMidCoords());
        }

        private void TranslateTransform_Changed(object sender, EventArgs e)
        {
            //System.Diagnostics.Trace.WriteLine("mid of board :" + BoardMidLocation().X);

            //if (GoNext())
            //    Advance();
            //else if (GoPrevious())
            //    Regress();
        }
    }
}