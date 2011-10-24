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
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace Teudu.InfoDisplay
{
    /// <summary>
    /// Interaction logic for BoardPagedControl.xaml
    /// </summary>
    public partial class BoardPagedControl : UserControl
    {
        private bool isShifting = false;
        const double boardInbetween = 150;
        DispatcherTimer trackingResetTimer;

        Dictionary<Board, double> positionOffsets;

        public BoardPagedControl()
        {
            InitializeComponent();

            positionOffsets = new Dictionary<Board, double>();
            sbAdvance = new Storyboard();

            trackingResetTimer = new DispatcherTimer();
            trackingResetTimer.Interval = TimeSpan.FromSeconds(5);
            trackingResetTimer.Tick += new EventHandler(trackingResetTimer_Tick);

            this.Loaded += new RoutedEventHandler(BoardNavigatorControl_Loaded);
        }

        void trackingResetTimer_Tick(object sender, EventArgs e)
        {
            trackingResetTimer.Stop();
            ((ViewModel)this.DataContext).UpdateBrowse(-positionOffsets[boardMaster.Current], 0);
            SetBindings();
            isShifting = false;
        }

        void BoardNavigatorControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = this.ActualWidth;
        }

        private IBoardService boardMaster;
        public IBoardService BoardMaster
        {
            set
            {
                ClearBindings();
                boardMaster = value;
                CreateBoardViews();
                this.Crumbs.Boards = boardMaster.Boards;
                this.Crumbs.Current = boardMaster.Current;
                BoardPosition.Changed += new EventHandler(TranslateTransform_Changed);
                boardMaster.BoardAdvanced += new EventHandler<BoardEventArgs>(boardMaster_BoardAdvanced);
                boardMaster.BoardRegressed += new EventHandler<BoardEventArgs>(boardMaster_BoardRegressed);
                trackingResetTimer.Start();
            }
        }

        void boardMaster_BoardRegressed(object sender, BoardEventArgs e)
        {
            double from = -positionOffsets[boardMaster.Next] + this.ActualWidth / 2 + boardInbetween;
            double to = -positionOffsets[boardMaster.Current];
            ShiftBoard(from, to);
            //this.Crumbs.Current = e.Board;
            //isShifting = false;
        }

        void boardMaster_BoardAdvanced(object sender, BoardEventArgs e)
        {
            double from = -positionOffsets[boardMaster.Prev] - this.ActualWidth / 2 + boardInbetween;
            double to = -positionOffsets[boardMaster.Current];
            ShiftBoard(from, to);
            //this.Crumbs.Current = e.Board;
            //isShifting = false;
        }

        private void CreateBoardViews()
        {
            positionOffsets.Clear();
            this.TitleContainer.Children.Clear();
            this.BoardContainer.Children.Clear();
            int i = 0;
            boardMaster.Boards.ForEach(x =>
            {
                EventBoard boardView = new EventBoard();
                boardView.MaxWidth = boardView.Width = App.Current.MainWindow.ActualWidth - boardInbetween;
                boardView.Height = this.ActualHeight;
                boardView.MaxHeight = 3 * 340;
                boardView.BoardModel = x;

                BoardTitleControl boardTitle = new BoardTitleControl();
                boardTitle.MaxWidth = boardTitle.Width = App.Current.MainWindow.ActualWidth - boardInbetween;
                boardTitle.Board = x;

                positionOffsets.Add(x, (boardView.Width * i++));

                this.TitleContainer.Children.Add(boardTitle);
                this.BoardContainer.Children.Add(boardView);

            });
        }

        private void ClearBindings()
        {
            BindingOperations.ClearAllBindings(BoardPosition);
            BindingOperations.ClearAllBindings(TitlePosition);
        }

        private void SetBindings()
        {
            Binding bindingX = new Binding
            {
                Path = new PropertyPath("GlobalOffsetX"),
                Source = this.DataContext
            };
            Binding bindingX2 = new Binding
            {
                Path = new PropertyPath("GlobalOffsetX"),
                Source = this.DataContext,
                Converter = new SlowedOffsetNavigationConverter()
            };
            Binding bindingY = new Binding
            {
                Path = new PropertyPath("GlobalOffsetY"),
                Source = this.DataContext
            };
            BindingOperations.SetBinding(BoardPosition, TranslateTransform.XProperty, bindingX);
            BindingOperations.SetBinding(BoardPosition, TranslateTransform.YProperty, bindingY);
            BindingOperations.SetBinding(TitlePosition, TranslateTransform.XProperty, bindingX);
        }
        Storyboard sbAdvance;
        private void Advance()
        {
            if (boardMaster == null || !boardMaster.AdvanceCurrent())
                return;

            System.Diagnostics.Trace.WriteLine("Moved to " + boardMaster.Current);
            ClearBindings();
        }

        private void ShiftBoard(double from, double to)
        {
            DoubleAnimation advanceAnimation = new DoubleAnimation(from, to, new Duration(TimeSpan.FromSeconds(1)));
            advanceAnimation.Completed += new EventHandler(advanceAnimation_Completed);
            Storyboard.SetTarget(advanceAnimation, BoardContainer);
            Storyboard.SetTargetProperty(advanceAnimation, new PropertyPath("(Canvas.Left)"));

            DoubleAnimation advanceAnimation2 = new DoubleAnimation(from, to, new Duration(TimeSpan.FromSeconds(1)));
            Storyboard.SetTarget(advanceAnimation2, TitleContainer);
            Storyboard.SetTargetProperty(advanceAnimation2, new PropertyPath("(Canvas.Left)"));
            sbAdvance.Children.Clear();
            sbAdvance.Children.Add(advanceAnimation);
            sbAdvance.Children.Add(advanceAnimation2);
            sbAdvance.Begin(); 
        }

        void advanceAnimation_Completed(object sender, EventArgs e)
        {
            //TranslateTransform titleTransform = new TranslateTransform(-positionOffsets[boardMaster.Current], 0);
            //TitleContainer.RenderTransform = titleTransform;
            this.Crumbs.Current = boardMaster.Current;
            sbAdvance.Stop();
            ((ViewModel)this.DataContext).UpdateBrowse(-positionOffsets[boardMaster.Current], 0);
            SetBindings();
            //TitleContainer.RenderTransform = TitlePosition;
            isShifting = false;
            //trackingResetTimer.Start();           
        }

        private void Regress()
        {
            if (boardMaster == null || !boardMaster.RegressCurrent())
                return;

            ClearBindings();
        }

        private bool GoPrevious()
        {
            System.Diagnostics.Trace.WriteLine("prev at: " + BoardLeftEdgeLocation().X + ">? thres: " + (-positionOffsets[boardMaster.Current] + this.ActualWidth / 2 - boardInbetween));
            return BoardLeftEdgeLocation().X > (-positionOffsets[boardMaster.Current] + this.ActualWidth / 2 + boardInbetween);
        }

        private bool GoNext()
        {
            System.Diagnostics.Trace.WriteLine("next at: " + BoardLeftEdgeLocation().X + " <? next thres: " + (-positionOffsets[boardMaster.Current] - this.ActualWidth / 2 + boardInbetween));
            return (BoardLeftEdgeLocation().X) < (-positionOffsets[boardMaster.Current] - this.ActualWidth/2 + boardInbetween);
        }

        private Point BoardLeftEdgeLocation()
        {
            return BoardContainer.TransformToAncestor(App.Current.MainWindow).Transform(new Point(0, 0));
        }

        private void TranslateTransform_Changed(object sender, EventArgs e)
        {
            if (isShifting)
                return;

            if (GoNext())
            {
                isShifting = true;
                System.Diagnostics.Trace.WriteLine("Advance!");
                Advance();
            }
            else if (GoPrevious())
            {
                isShifting = true;
                Regress();
            }
        }
    }
}
