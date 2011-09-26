using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Input;

namespace Teudu.InfoDisplay
{
    public class ViewModel: INotifyPropertyChanged
    {
        private const double PAN_TO_OFFSET = 50;
        private const double SCALE_OFFSET = 250;
        private const double HEIGHT_OFF_GROUND = 0;//300;
        private double ARM_SCALE_FACTOR_X = 1;//(App.Current.MainWindow.Width / 2) * 3;
        private double ARM_SCALE_FACTOR_Y = 1;//(App.Current.MainWindow.Height / 2) * 3;
        private double ARM_SCALE_FACTOR_Z = 1;//500; // Distance to tv

        private const int MAX_SCALE = 4;
        private const int MIN_SCALE = 1;

        private static double hotspotRegionX = App.Current.MainWindow.Width / 12;
        private static double hotspotRegionY = App.Current.MainWindow.Height / 8;

        private double hotspotLeft = 0;
        private double hotSpotRight = App.Current.MainWindow.Width - hotspotRegionX;
        private double hotSpotTop = 0;
        private double hotSpotBottom = App.Current.MainWindow.Height - hotspotRegionY;

        IKinectService kinectService;
        ISourceService sourceService;
        IBoardService boardService;

        private DispatcherTimer categoryChangeTimer;

        public ViewModel(IKinectService kinectService, ISourceService sourceService, IBoardService boardService) 
        {
            categoryChangeTimer = new DispatcherTimer(DispatcherPriority.Loaded);
            categoryChangeTimer.Tick += new EventHandler(categoryChangeTimer_Tick);
            categoryChangeTimer.Interval = new TimeSpan(50000000); //100000000

            this.kinectService = kinectService; 
            //this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);

            this.sourceService = sourceService;
            this.sourceService.EventsUpdated += new EventHandler<SourceEventArgs>(sourceService_EventsUpdated);

            this.boardService = boardService;
            this.boardService.BoardsUpdated += new EventHandler(boardService_BoardsChanged);
            this.boardService.ActiveBoardChanged += new EventHandler<BoardEventArgs>(boardService_ActiveBoardChanged);
        

            leftArm = new Arm();
            rightArm = new Arm();
            head = new Head();
            torso = new Torso();

            this.sourceService.BeginPoll();
        }

        void categoryChangeTimer_Tick(object sender, EventArgs e)
        {
            AdvanceBoard();
            categoryChangeTimer.Stop();
        }


        public void ChangeBoard(bool right)
        {
            if (right)
                AdvanceBoard();
            else
                RegressBoard();
        }

        void boardService_ActiveBoardChanged(object sender, BoardEventArgs e)
        {
            NotifyActiveBoardSubscribers();
        }

        void boardService_BoardsChanged(object sender, EventArgs e)
        {
            //reset UI
            NotifyActiveBoardSubscribers();
        }

        private void NotifyActiveBoardSubscribers()
        {
            categoryChangeTimer.Stop();
            this.kinectService.SkeletonUpdated -= new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);

            if (ActiveBoardChanged != null)
                ActiveBoardChanged(this, new BoardEventArgs() {Previous = PreviousBoard, Board = CurrentBoard, Next=NextBoard });

            this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);

            if (PreviousBoard != null)
                this.OnPropertyChanged("PreviousBoard");
            if (NextBoard != null)
                this.OnPropertyChanged("NextBoard");
            if (CurrentBoard != null)
                this.OnPropertyChanged("CurrentBoard");
            categoryChangeTimer.Start();
        }


        void sourceService_EventsUpdated(object sender, SourceEventArgs e)
        {
            boardService.Events = e.Events;
        }

        public Board CurrentBoard
        {
            get 
            {
                if (boardService.Current == null)
                    return new Board("No events");
                return boardService.Current; 
            }
        }

        public Board PreviousBoard
        {
            get { return boardService.Prev; }
        }

        public Board NextBoard
        {
            get {return boardService.Next; }
        }

        public void AdvanceBoard()
        {
            boardService.AdvanceCurrent();
        }

        public void RegressBoard()
        {
            boardService.RegressCurrent();
        }

        #region Kinect

        void kinectService_SkeletonUpdated(object sender, SkeletonEventArgs e) 
        { 
            if (App.Current.MainWindow != null)
            {
                #region Set vals
                var midpointX = App.Current.MainWindow.ActualWidth / 2; 
                var midpointY = App.Current.MainWindow.ActualHeight / 2 + - HEIGHT_OFF_GROUND; //TODO: get height off ground

                this.head.Y = e.HeadPosition.Y * ARM_SCALE_FACTOR_Y;

                this.rightArm.HandX = (e.RightHandPosition.X * ARM_SCALE_FACTOR_X);
                this.rightArm.HandY = (e.RightHandPosition.Y * ARM_SCALE_FACTOR_Y);
                this.rightArm.HandZ = e.RightHandPosition.Z * ARM_SCALE_FACTOR_Z;
                this.rightArm.ElbowX = (e.RightElbowPosition.X * ARM_SCALE_FACTOR_X);
                this.rightArm.ElbowY = (e.RightElbowPosition.Y * ARM_SCALE_FACTOR_Y);
                this.rightArm.ElbowZ = e.RightElbowPosition.Z * ARM_SCALE_FACTOR_Z;
                this.rightArm.ShoulderX = (e.RightShoulderPosition.X * ARM_SCALE_FACTOR_X);
                this.rightArm.ShoulderY = (e.RightShoulderPosition.Y * ARM_SCALE_FACTOR_Y);
                this.rightArm.ShoulderZ = e.RightShoulderPosition.Z * ARM_SCALE_FACTOR_Z;

                this.leftArm.HandX = (e.LeftHandPosition.X * ARM_SCALE_FACTOR_X);
                this.leftArm.HandY = (e.LeftHandPosition.Y * ARM_SCALE_FACTOR_Y);
                this.leftArm.HandZ = e.LeftHandPosition.Z * ARM_SCALE_FACTOR_Z;
                this.leftArm.ElbowX = (e.LeftElbowPosition.X * ARM_SCALE_FACTOR_X);
                this.leftArm.ElbowY = (e.LeftElbowPosition.Y * ARM_SCALE_FACTOR_Y);
                this.leftArm.ElbowZ = e.LeftElbowPosition.Z * ARM_SCALE_FACTOR_Z;
                this.leftArm.ShoulderX = (e.LeftShoulderPosition.X * ARM_SCALE_FACTOR_X);
                this.leftArm.ShoulderY = (e.LeftShoulderPosition.Y * ARM_SCALE_FACTOR_Y);
                this.leftArm.ShoulderZ = e.LeftShoulderPosition.Z * ARM_SCALE_FACTOR_Z;

                this.spine.Z = e.SpinePosition.Z * ARM_SCALE_FACTOR_Z;

                this.torso.Y = (e.TorsoPosition.Y * ARM_SCALE_FACTOR_Y);
                #endregion

                //Trace.WriteLine("Right arm is " + rightArm.HandZ);

                if (ViewChangeMode == HandsState.Panning)
                {
                    //this.OnPropertyChanged("DominantArmHandOffsetX");
                    //this.OnPropertyChanged("DominantArmHandOffsetY");
                    Pan();
                }
                //else if (ViewChangeMode == HandsState.Zooming)
                //    Scale();
                //Trace.WriteLineIf(LeftArmInFront, "Left arm z " + leftArm.HandZ + " , spine z " + spine.Z);
                //Trace.WriteLineIf(RightArmInFront, "Right arm z " + rightArm.HandZ + " , spine z " + spine.Z);

                //Trace.WriteLineIf(RightHandActive, "Right HAND active " + rightArm.HandZ + "("+rightArm.HandX + "," + rightArm.HandY + ")");

                //Trace.WriteLineIf(IsNearLeft, "Left at" + DominantHand.HandX);
                //Trace.WriteLineIf(IsNearRight, "Right at" + DominantHand.HandX);
                //Trace.WriteLineIf(IsNearTop, "Top at" + DominantHand.HandY);
                //Trace.WriteLineIf(IsNearBot, "Bot at" + DominantHand.HandY);
            } 
        #endregion

        
        }

        Arm leftArm;
        Arm rightArm;
        Torso torso;
        Head head;
        Spine spine;

        private void Pan()
        {
            if (!IsNearBot && !IsNearTop && !IsNearLeft && !IsNearRight)
                return;

            //Trace.WriteLineIf(IsNearLeft, "Left at" + DominantHand.HandX);
            //Trace.WriteLineIf(IsNearRight, "Right at" + DominantHand.HandX);
            //Trace.WriteLineIf(IsNearTop, "Top at" + DominantHand.HandY);
            //Trace.WriteLineIf(IsNearBot, "Bot at" + DominantHand.HandY);

            NotifyPanSubscribers();
        }

        private void Scale()
        {
            NotifyScaleSubscribers();
        }

        private void NotifyScaleSubscribers()
        {
            double scaleLevel = Math.Log(this.HandsDistance, 3);
            if (scaleLevel >= 5)
                scaleLevel = 5;
            if (scaleLevel <= 1)
                scaleLevel = 1;
            //ScaleLevel = lastScale + (HandsDistance - startHandDistance);
            if (ScaleRequest != null)
                ScaleRequest(this, new ScaleEventArgs()
                {
                    ScaleLevel = scaleLevel
                });
        }

        private void NotifyPanSubscribers()
        {
            double xOffset, yOffset;
            xOffset = yOffset = 0;

            if (IsNearLeft)
                xOffset = PAN_TO_OFFSET;
            if (IsNearRight)
                xOffset = -PAN_TO_OFFSET;
            if (IsNearTop)
                yOffset = PAN_TO_OFFSET;
            if (IsNearBot)
                yOffset = -PAN_TO_OFFSET;

            if (PanRequest != null)
                PanRequest(this, new PanEventArgs()
                {
                    HorizontalOffset = xOffset,
                    VerticalOffset = yOffset
                });
        }

        public bool IsNearLeft
        {
            get { return (DominantHand.HandX >= hotspotLeft) && (DominantHand.HandX < (hotspotLeft + hotspotRegionX)); }
        }

        public bool IsNearRight
        {
            get { return (DominantHand.HandX > hotSpotRight) && (DominantHand.HandX <= (hotSpotRight + hotspotRegionX)); }
        }

        public bool IsNearTop
        {
            get { return (DominantHand.HandY >= hotSpotTop) && (DominantHand.HandY < (hotSpotTop + hotspotRegionY)); }
        }

        public bool IsNearBot
        {
            get{ return (DominantHand.HandY > hotSpotBottom) && (DominantHand.HandY <= (hotSpotBottom + hotspotRegionY)); }
        }

        public bool LeftArmInFront
        {
            get { return leftArm.ArmAlmostStraight && LeftHandAboveTorso || (leftArm.HandZ < (spine.Z - 100)); }
        }

        public bool RightArmInFront
        {
            get { return rightArm.HandZ < 1.25; }
                //rightArm.ArmAlmostStraight && RightHandAboveTorso || (rightArm.HandZ < (spine.Z - 100)); }
        }

        public HandsState ViewChangeMode
        {
            get
            {
                HandsState currentState;
                if (LeftHandActive && RightHandActive)
                    currentState = HandsState.Zooming;
                else if ((LeftHandActive && !RightHandActive) || (!LeftHandActive && RightHandActive))
                    currentState = HandsState.Panning;
                else
                    currentState = HandsState.Resting;

                return currentState;
            }
        }

        #region Hand States

        public bool LeftHandActive
        {
            get{return LeftArmInFront;}
        }

        public bool RightHandActive
        {
            get{return RightArmInFront;}
        }

        public Arm DominantHand
        {
            get 
            {
                if (LeftHandActive)
                    return leftArm;
                else
                    return rightArm;
            }
        }

        public double HandsDistance
        {
            get
            {
                return Math.Sqrt(Math.Pow(this.leftArm.HandX - this.rightArm.HandX, 2) +
                    Math.Pow(this.leftArm.HandY - this.rightArm.HandY, 2)) / SCALE_OFFSET;
            }
        }

        public double DominantArmHandOffsetX
        {
            get { return -DominantHand.HandOffsetX * 7; }
        }

        public double DominantArmHandOffsetY
        {
            get { return -DominantHand.HandOffsetY * 7; }
        }

        #endregion

        #region deprecated
        public bool RightHandAboveTorso
        {
            get { return this.rightArm.HandY < torso.Y; }
        }
        public bool LeftHandAboveTorso
        {
            get { return this.leftArm.HandY < torso.Y; }
        }
        #endregion

        
        void OnPropertyChanged(string property) 
        { 
            if (this.PropertyChanged != null) 
            { 
                this.PropertyChanged(this, new PropertyChangedEventArgs(property)); 
            } 
        }        
        
        public void Cleanup() 
        { 
            this.kinectService.SkeletonUpdated -= kinectService_SkeletonUpdated; 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ScaleEventArgs> ScaleRequest;
        public event EventHandler<PanEventArgs> PanRequest;
        public event EventHandler<BoardEventArgs> ActiveBoardChanged;

    }
}
