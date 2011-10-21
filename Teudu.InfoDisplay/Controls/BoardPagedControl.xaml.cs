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
            trackingResetTimer.Interval = new TimeSpan(0,0,0,0,500);
            trackingResetTimer.Tick += new EventHandler(trackingResetTimer_Tick);

            this.Loaded += new RoutedEventHandler(BoardNavigatorControl_Loaded);
        }

        void trackingResetTimer_Tick(object sender, EventArgs e)
        {
            trackingResetTimer.Stop();
            SetBindings();
            //BoardPosition.Changed += new EventHandler(TranslateTransform_Changed);
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
                SetBindings();
            }
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
            BindingOperations.SetBinding(TitlePosition, TranslateTransform.XProperty, bindingX2);
        }
        Storyboard sbAdvance;
        private void Advance()
        {
            if (boardMaster == null || !boardMaster.AdvanceCurrent())
                return;

            ClearBindings();

            double from = -positionOffsets[boardMaster.Prev] - this.ActualWidth / 2 + boardInbetween;
            double to = -positionOffsets[boardMaster.Current];

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
            Canvas.SetLeft(BoardContainer, -positionOffsets[boardMaster.Current]);
            Canvas.SetLeft(TitleContainer, -positionOffsets[boardMaster.Current]);
            sbAdvance.Stop();
            this.Crumbs.Current = boardMaster.Current;
            trackingResetTimer.Start();           
        }

        private void Regress()
        {
            if (boardMaster == null || !boardMaster.RegressCurrent())
                return;

            ClearBindings();

            double from = -positionOffsets[boardMaster.Next] + this.ActualWidth / 2 - boardInbetween;
            double to = -positionOffsets[boardMaster.Current];

            DoubleAnimation regressAnimation = new DoubleAnimation(from, to, new Duration(TimeSpan.FromSeconds(1)));
            regressAnimation.Completed += new EventHandler(regressAnimation_Completed);
            Storyboard.SetTarget(regressAnimation, BoardContainer);
            Storyboard.SetTargetProperty(regressAnimation, new PropertyPath("(Canvas.Left)"));

            DoubleAnimation regressAnimation2 = new DoubleAnimation(from, to, new Duration(TimeSpan.FromSeconds(1)));
            Storyboard.SetTarget(regressAnimation2, TitleContainer);
            Storyboard.SetTargetProperty(regressAnimation2, new PropertyPath("(Canvas.Left)"));
            sbAdvance.Children.Clear();
            sbAdvance.Children.Add(regressAnimation);
            sbAdvance.Children.Add(regressAnimation2);
            sbAdvance.Begin(); 
        }

        void regressAnimation_Completed(object sender, EventArgs e)
        {
            Canvas.SetLeft(BoardContainer, -positionOffsets[boardMaster.Current]);
            Canvas.SetLeft(TitleContainer, -positionOffsets[boardMaster.Current]);
            sbAdvance.Stop();
            this.Crumbs.Current = boardMaster.Current;
            trackingResetTimer.Start();   
        }

        private bool GoPrevious()
        {
            return BoardLeftEdgeLocation().X > (-positionOffsets[boardMaster.Current] + this.ActualWidth / 2 - boardInbetween);
        }

        private bool GoNext()
        {
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
                //Advance();
            }
            else if (GoPrevious())
            {
                isShifting = true;
                //Regress();
            }
        }
    }
}
