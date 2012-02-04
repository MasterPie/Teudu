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
using System.ComponentModel;

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
        Dictionary<Board, EventBoard> modelView;

        public BoardPagedControl()
        {
            InitializeComponent();

            positionOffsets = new Dictionary<Board, double>();
            modelView = new Dictionary<Board, EventBoard>();
            sbAdvance = new Storyboard();

            trackingResetTimer = new DispatcherTimer();
            trackingResetTimer.Interval = TimeSpan.FromSeconds(2);
            trackingResetTimer.Tick += new EventHandler(trackingResetTimer_Tick);

            this.Loaded += new RoutedEventHandler(BoardNavigatorControl_Loaded);
            this.BoardContainer.LayoutUpdated += new EventHandler(BoardContainer_LayoutUpdated);           

        }

        void BoardContainer_LayoutUpdated(object sender, EventArgs e)
        {

            int childCount = this.BoardContainer.Children.Count;
            foreach (UIElement element in this.BoardContainer.Children)
            {
                element.SetValue(Canvas.ZIndexProperty, childCount);
                childCount--;
            }
        }

        void trackingResetTimer_Tick(object sender, EventArgs e)
        {
            trackingResetTimer.Stop();
            ((ViewModel)this.DataContext).UpdateBrowse(-positionOffsets[boardMaster.Current], 0);
            ((ViewModel)this.DataContext).MaxBoardWidth = this.BoardContainer.ActualWidth;
            ((ViewModel)this.DataContext).MaxBoardHeight = modelView[boardMaster.Current].ContentHeight;
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
            ((ViewModel)this.DataContext).MaxBoardHeight = modelView[boardMaster.Current].ContentHeight;
            ShiftBoard(from, to);
        }

        void boardMaster_BoardAdvanced(object sender, BoardEventArgs e)
        {
            double from = -positionOffsets[boardMaster.Prev] - this.ActualWidth / 2 + boardInbetween;
            double to = -positionOffsets[boardMaster.Current];
            ((ViewModel)this.DataContext).MaxBoardHeight = modelView[boardMaster.Current].ContentHeight;
            ShiftBoard(from, to);
        }

        private void CreateBoardViews()
        {
            positionOffsets.Clear();
            modelView.Clear();
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
                modelView.Add(x, boardView);

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
            {
                isShifting = false;
                return;
            }

            System.Diagnostics.Trace.WriteLine("Moved to " + boardMaster.Current);
            ClearBindings();
        }

        private void ShiftBoard(double from, double to)
        {
            DoubleAnimationUsingKeyFrames advanceAnimation = new DoubleAnimationUsingKeyFrames();
            advanceAnimation.Completed +=new EventHandler(advanceAnimation_Completed);
            advanceAnimation.Duration = TimeSpan.FromSeconds(1);
            LinearDoubleKeyFrame linear = new LinearDoubleKeyFrame(from, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)));
            EasingDoubleKeyFrame easing = new EasingDoubleKeyFrame(to, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1)), new CircleEase() {EasingMode = EasingMode.EaseInOut });
            advanceAnimation.KeyFrames.Add(linear);
            advanceAnimation.KeyFrames.Add(easing);
            Storyboard.SetTarget(advanceAnimation, BoardContainer);
            Storyboard.SetTargetProperty(advanceAnimation, new PropertyPath("(Canvas.Left)"));

            DoubleAnimationUsingKeyFrames advanceAnimation2 = new DoubleAnimationUsingKeyFrames();
            advanceAnimation2.Duration = TimeSpan.FromSeconds(1);
            LinearDoubleKeyFrame linear2 = new LinearDoubleKeyFrame(from, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)));
            EasingDoubleKeyFrame easing2 = new EasingDoubleKeyFrame(to, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1)), new CircleEase() { EasingMode = EasingMode.EaseInOut });
            advanceAnimation2.KeyFrames.Add(linear2);
            advanceAnimation2.KeyFrames.Add(easing2);
            Storyboard.SetTarget(advanceAnimation2, TitleContainer);
            Storyboard.SetTargetProperty(advanceAnimation2, new PropertyPath("(Canvas.Left)"));


            sbAdvance.Children.Clear();
            sbAdvance.Children.Add(advanceAnimation);
            sbAdvance.Children.Add(advanceAnimation2);
            sbAdvance.Begin();
        }

        void advanceAnimation_Completed(object sender, EventArgs e)
        {
            this.Crumbs.Current = boardMaster.Current;
            sbAdvance.Stop();
            ((ViewModel)this.DataContext).UpdateBrowse(-positionOffsets[boardMaster.Current], 0);
            SetBindings();
            isShifting = false;       
        }

        private void Regress()
        {
            if (boardMaster == null || !boardMaster.RegressCurrent())
            {
                isShifting = false;
                return;
            }

            ClearBindings();
        }

        private bool GoPrevious()
        {
            if (!positionOffsets.ContainsKey(boardMaster.Current))
                return false;
            return BoardLeftEdgeLocation().X > (-positionOffsets[boardMaster.Current] + this.ActualWidth / 2 + boardInbetween);
        }

        private bool GoNext()
        {
            if (!positionOffsets.ContainsKey(boardMaster.Current))
                return false;
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
