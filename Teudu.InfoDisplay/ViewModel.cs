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
        private double CORRESPONDENCE_SCALE_FACTOR_X = 4;
        private double CORRESPONDENCE_SCALE_FACTOR_Y = 6;
        private double HALF_ARMSPAN = 0.3;
        private double UserClearanceDistance;
        private int maxEventHeight;

        private bool firstEntry = true;
        private double EntryX = 0;
        private double EntryY = 0;
        private bool updatingViewState = false;

        private bool inverted = false;
        private int inversionFactor = -1;

        IKinectService kinectService;
        ISourceService sourceService;
        IBoardService boardService;

        public ViewModel(IKinectService kinectService, ISourceService sourceService, IBoardService boardService) 
        {
            UserClearanceDistance = 1.3;
            Double.TryParse(ConfigurationManager.AppSettings["UserDistanceRequired"], out UserClearanceDistance);
            HeightOffGround = 0;
            Double.TryParse(ConfigurationManager.AppSettings["HeightOffGround"], out HeightOffGround);
            if (!Int32.TryParse(ConfigurationManager.AppSettings["MaxEventHeight"], out maxEventHeight))
                maxEventHeight = 340;

            if (!Double.TryParse(ConfigurationManager.AppSettings["CorrespondenceScaleX"], out CORRESPONDENCE_SCALE_FACTOR_X))
                CORRESPONDENCE_SCALE_FACTOR_X = 4;

            if (!Double.TryParse(ConfigurationManager.AppSettings["CorrespondenceScaleY"], out CORRESPONDENCE_SCALE_FACTOR_Y))
                CORRESPONDENCE_SCALE_FACTOR_Y = 6;

            if (!Boolean.TryParse(ConfigurationManager.AppSettings["Inverted"], out inverted))
                inverted = false;

            if (inverted)
                inversionFactor = 1;
            else
                inversionFactor = -1;


            this.kinectService = kinectService; 

            this.sourceService = sourceService;
            this.sourceService.EventsUpdated += new EventHandler<SourceEventArgs>(sourceService_EventsUpdated);

            this.boardService = boardService;
            this.boardService.BoardsUpdated += new EventHandler(boardService_BoardsChanged);

            leftArm = new Arm();
            rightArm = new Arm();
            torso = new Torso();
        }

        public void BeginBackgroundJobs()
        {
            this.sourceService.BeginPoll();
        }

        private double maxBoardWidth = 0;
        public double MaxBoardWidth
        {
            set
            {
                maxBoardWidth = value;
                
            }
        }

        private double maxBoardHeight = 0;
        public double MaxBoardHeight
        {
            set
            {
                maxBoardHeight = value;
            }
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

                this.torso.X = e.TorsoPosition.X;
                this.torso.Y = e.TorsoPosition.Y;
                this.torso.Z = e.TorsoPosition.Z;


                if(Engaged)
                    firstEntry = false;

                if (updatingViewState)
                    return;

                if (!wasEngaged)
                {
                    oldGlobalX = GlobalOffsetX;
                    oldGlobalY = GlobalOffsetY;

                    if (Engaged)
                    {
                        EntryX = DominantArmHandOffsetX;
                        EntryY = DominantArmHandOffsetY;
                    }
                }
                
                #endregion                
                if (Engaged)
                {
                    GlobalOffsetX = DominantArmHandOffsetX;
                    GlobalOffsetY = DominantArmHandOffsetY;
                    if (ViewChangeMode == HandsState.Panning)
                    {
                        this.OnPropertyChanged("DominantArmHandOffsetX");
                        this.OnPropertyChanged("DominantArmHandOffsetY");
                    }
                }

                this.OnPropertyChanged("Engaged");
                this.OnPropertyChanged("TooClose");
                this.OnPropertyChanged("DistanceFromInvisScreen");
            } 
        #endregion

        
        }

        Arm leftArm;
        Arm rightArm;
        Torso torso;

        public bool Engaged
        {
            get
            {
                return (LeftHandActive || RightHandActive) && !(LeftHandActive && RightHandActive) && !TooClose;
            }
        }

        public bool TooClose
        {
            get
            {
                return torso.Z < (UserClearanceDistance + HALF_ARMSPAN);
            }
        }

        public double DistanceFromInvisScreen
        {
            get
            {
                if (leftArm.HandZ < rightArm.HandZ)
                    return leftArm.HandZ - UserClearanceDistance;
                else if (rightArm.HandZ <= leftArm.HandZ)
                    return rightArm.HandZ - UserClearanceDistance;
                else
                    return 2;
                
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
            set {
                if (EntryX - value + oldGlobalX > App.Current.MainWindow.ActualWidth / 2)
                    globalX = App.Current.MainWindow.ActualWidth / 2;
                else if (EntryX - value + oldGlobalX < -maxBoardWidth + App.Current.MainWindow.ActualWidth / 2)
                    globalX = -maxBoardWidth + App.Current.MainWindow.ActualWidth / 2;
                else
                    globalX = EntryX - value + oldGlobalX; this.OnPropertyChanged("GlobalOffsetX");
            }
            get {
                if (firstEntry)
                    return 0;
                return globalX; }
        }

        public double GlobalOffsetY
        {
            set {
                if (EntryY - value + oldGlobalY > (App.Current.MainWindow.ActualHeight / 2 - 250))
                    globalY = App.Current.MainWindow.ActualHeight / 2 - 250;
                else if (EntryY - value + oldGlobalY < (-maxBoardHeight + maxEventHeight)) //3*-maxEventHeight - 60
                    globalY = (-maxBoardHeight +maxEventHeight);
                else
                    globalY = EntryY - value + oldGlobalY; this.OnPropertyChanged("GlobalOffsetY");

                //Trace.WriteLine("maxBoardHeight" + -maxBoardHeight);
            }
            get
            {
                if (firstEntry)
                    return 0; 
                return globalY; }
        }

        public double DominantArmHandOffsetX
        {
            get { return inversionFactor * DominantHand.HandOffsetX * CORRESPONDENCE_SCALE_FACTOR_X; }
        }

        public double DominantArmHandOffsetY
        {
            get { return inversionFactor * DominantHand.HandOffsetY * CORRESPONDENCE_SCALE_FACTOR_Y; }
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
