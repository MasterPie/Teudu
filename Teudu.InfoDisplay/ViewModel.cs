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
using System.Configuration;

namespace Teudu.InfoDisplay
{
    public class ViewModel: INotifyPropertyChanged
    {
        private const double SCALE_OFFSET = 250;
        private double HeightOffGround = 0;
        private double CORRESPONDENCE_SCALE_FACTOR = 4;
        private double UserClearanceDistance;

        private bool firstEntry = true;
        private double EntryX = 0;
        private double EntryY = 0;
        private bool updatingViewState = false;

        IKinectService kinectService;
        ISourceService sourceService;
        IBoardService boardService;

        public ViewModel(IKinectService kinectService, ISourceService sourceService, IBoardService boardService) 
        {
            UserClearanceDistance = 1.3;
            Double.TryParse(ConfigurationManager.AppSettings["UserDistanceRequired"], out UserClearanceDistance);
            HeightOffGround = 0;
            Double.TryParse(ConfigurationManager.AppSettings["HeightOffGround"], out HeightOffGround);


            this.kinectService = kinectService; 

            this.sourceService = sourceService;
            this.sourceService.EventsUpdated += new EventHandler<SourceEventArgs>(sourceService_EventsUpdated);

            this.boardService = boardService;
            this.boardService.BoardsUpdated += new EventHandler(boardService_BoardsChanged);

            leftArm = new Arm();
            rightArm = new Arm();

            this.sourceService.BeginPoll();
        }

        public void UpdateBrowse(double x, double y)
        {
            updatingViewState = true;
            oldGlobalX = x;
            oldGlobalY = y;
            EntryX = DominantArmHandOffsetX;
            EntryY = DominantArmHandOffsetY;
            globalX = oldGlobalX;
            globalY = oldGlobalY;
            updatingViewState = false;
        }

        void boardService_BoardsChanged(object sender, EventArgs e)
        {
            NotifyBoardSubscribers();
        }

        private void NotifyBoardSubscribers()
        {
            this.kinectService.SkeletonUpdated -= new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);
            if (BoardsUpdated != null)
                BoardsUpdated(this, new BoardEventArgs() { BoardService = this.boardService});
            this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);
        }

        void sourceService_EventsUpdated(object sender, SourceEventArgs e)
        {
            boardService.Events = e.Events;
        }

        #region Kinect

        void kinectService_SkeletonUpdated(object sender, SkeletonEventArgs e) 
        { 
            if (App.Current.MainWindow != null)
            {
                #region Set vals

                bool wasEngaged = Engaged;

                this.rightArm.HandX = e.RightHandPosition.X;
                this.rightArm.HandY = e.RightHandPosition.Y;
                this.rightArm.HandZ = e.RightHandPosition.Z;

                this.leftArm.HandX = e.LeftHandPosition.X;
                this.leftArm.HandY = e.LeftHandPosition.Y;
                this.leftArm.HandZ = e.LeftHandPosition.Z;

                if(Engaged)
                    firstEntry = false;

                if (updatingViewState)
                    return;

                if (!wasEngaged)
                {
                    oldGlobalX = GlobalOffsetX;
                    oldGlobalY = GlobalOffsetY;
                    EntryX = DominantArmHandOffsetX;
                    EntryY = DominantArmHandOffsetY;
                }
                GlobalOffsetX = DominantArmHandOffsetX;
                GlobalOffsetY = DominantArmHandOffsetY;
                #endregion                

                if (ViewChangeMode == HandsState.Panning)
                {
                    this.OnPropertyChanged("DominantArmHandOffsetX");
                    this.OnPropertyChanged("DominantArmHandOffsetY");
                }

                this.OnPropertyChanged("Engaged");
            } 
        #endregion

        
        }

        Arm leftArm;
        Arm rightArm;

        public bool Engaged
        {
            get
            {
                return LeftHandActive || RightHandActive;
            }
        }

        public bool LeftArmInFront
        {
            get { return leftArm.HandZ < UserClearanceDistance; }
                //return leftArm.ArmAlmostStraight && LeftHandAboveTorso || (leftArm.HandZ < (spine.Z - 100)); }
        }

        public bool RightArmInFront
        {
            get { return rightArm.HandZ < UserClearanceDistance; }
                //return rightArm.ArmAlmostStraight && RightHandAboveTorso || (rightArm.HandZ < (spine.Z - 100)); }
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

        private double oldGlobalX = 0;
        private double oldGlobalY = 0;
        private double globalX = 0;
        private double globalY = 0;
        public double GlobalOffsetX
        {
            set { globalX = EntryX - value + oldGlobalX; this.OnPropertyChanged("GlobalOffsetX"); Trace.WriteLine("GlobalX: " + globalX); }
            get {
                if (firstEntry)
                    return 0;
                return globalX; }
        }

        public double GlobalOffsetY
        {
            set {
                if (EntryY - value + oldGlobalY > App.Current.MainWindow.ActualHeight / 2)
                    globalY = App.Current.MainWindow.ActualHeight / 2;
                else
                    globalY = EntryY - value + oldGlobalY; this.OnPropertyChanged("GlobalOffsetY"); }
            get
            {
                if (firstEntry)
                    return 0; 
                return globalY; }
        }

        public double DominantArmHandOffsetX
        {
            get { return -DominantHand.HandOffsetX * CORRESPONDENCE_SCALE_FACTOR; }
        }

        public double DominantArmHandOffsetY
        {
            get { return -DominantHand.HandOffsetY * CORRESPONDENCE_SCALE_FACTOR; }
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
        public event EventHandler<BoardEventArgs> BoardsUpdated;

    }
}
