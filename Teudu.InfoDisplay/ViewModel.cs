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
    public class ViewModel: DependencyObject, INotifyPropertyChanged
    {
        private const int maxEventHeight = 400;

        IKinectService kinectService;
        ISourceService sourceService;
        IBoardService boardService;
        IHelpService helpService;
        UserState user;

        DispatcherTimer appIdleTimer;        

        public ViewModel(IKinectService kinectService, ISourceService sourceService, IBoardService boardService, IHelpService helpService) 
        {
            user = new UserState();

            //if (!Int32.TryParse(ConfigurationManager.AppSettings["MaxEventHeight"], out maxEventHeight))
                //maxEventHeight = 340;

            idleJobQueue = new Queue<Action>();

            this.helpService = helpService;
            this.helpService.NewHelpMessage += new EventHandler<HelpMessageEventArgs>(helpService_NewHelpMessage);

            this.kinectService = kinectService; 

            this.sourceService = sourceService;
            this.sourceService.EventsUpdated += new EventHandler<SourceEventArgs>(sourceService_EventsUpdated);

            this.boardService = boardService;
            this.boardService.BoardsUpdated += new EventHandler(boardService_BoardsChanged);

            appIdleTimer = new DispatcherTimer();
            appIdleTimer.Interval = TimeSpan.FromMinutes(1);
            appIdleTimer.Tick += new EventHandler(appIdle_Tick);
            appIdleTimer.Start();
            
            BeginBackgroundJobs();
        }

        #region Momentum [inactive]
        private double displacementX;
        private double displacementY;
        private DateTime momentumStart;

        public TimeSpan MovementDuration
        {
            get { return TimeSpan.FromSeconds(1); }
        }

        public void RunMomentum(double toX, double toY)
        {
            DateTime startTime = DateTime.Now;
            while (!user.Touching && DateTime.Now <= startTime.AddSeconds(2))
            {
                if (toX < GlobalOffsetX)
                    GlobalOffsetX = GlobalOffsetX - 1;

                if (toX > GlobalOffsetX)
                    GlobalOffsetX = GlobalOffsetX + 1;

                if (toY < GlobalOffsetY)
                    GlobalOffsetY = GlobalOffsetY - 1;

                if (toY > GlobalOffsetY)
                    GlobalOffsetY = GlobalOffsetY + 1;
            }
        }

        #endregion

        #region Model Event Handlers

        /// <summary>
        /// Saves help message text and image when NewHelpMessage event is fired
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void helpService_NewHelpMessage(object sender, HelpMessageEventArgs e)
        {
            helpMessage = e.Message;
            helpImage = e.SupplementaryImage;
            
            this.OnPropertyChanged("HelpMessage");
            this.OnPropertyChanged("HelpImage");
        }

        /// <summary>
        /// Sets boardservice to new events when EventsUpdated is fired
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sourceService_EventsUpdated(object sender, SourceEventArgs e)
        {
            idleJobQueue.Enqueue(new Action(delegate { boardService.Events = e.Events; }));
        }

        /// <summary>
        /// Fires when boardservice fires a BoardsChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void boardService_BoardsChanged(object sender, EventArgs e)
        {
            NotifyBoardSubscribers();
        }

        /// <summary>
        /// Handles Skeleton Updated event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinectService_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            if (App.Current.MainWindow != null)
            {


                bool wasTouching = user.Touching;
                #region Set vals
                user.rightArm.HandX = e.RightHandPosition.X;
                user.rightArm.HandY = e.RightHandPosition.Y;
                user.rightArm.HandZ = e.RightHandPosition.Z;

                user.leftArm.HandX = e.LeftHandPosition.X;
                user.leftArm.HandY = e.LeftHandPosition.Y;
                user.leftArm.HandZ = e.LeftHandPosition.Z;

                user.torso.X = e.TorsoPosition.X;
                user.torso.Y = e.TorsoPosition.Y;
                user.torso.Z = e.TorsoPosition.Z;
                #endregion

                if (user.Touching)
                    firstEntry = false;

                if (updatingViewState)
                    return;

                if (!wasTouching)
                {
                    oldGlobalX = GlobalOffsetX;
                    oldGlobalY = GlobalOffsetY;

                    if (user.Touching)
                    {
                        EntryX = user.DominantArmHandOffsetX;
                        EntryY = user.DominantArmHandOffsetY;
                    }
                }

                if (user.Touching)
                {
                    GlobalOffsetX = user.DominantArmHandOffsetX;
                    GlobalOffsetY = user.DominantArmHandOffsetY;
                    if (user.InteractionMode == HandsState.Panning)
                    {
                        this.OnPropertyChanged("DominantArmHandOffsetX");
                        this.OnPropertyChanged("DominantArmHandOffsetY");
                    }
                    appIdleTimer.Stop();
                    appIdleTimer.Start();
                }

                this.OnPropertyChanged("Engaged");
                this.OnPropertyChanged("TooClose");
                this.OnPropertyChanged("OutOfBounds");
                this.OnPropertyChanged("InRange");
                this.OnPropertyChanged("OutOfRange");

                if (!user.TooClose)
                    this.OnPropertyChanged("DistanceFromInvisScreen");

                

                this.OnPropertyChanged("ShowingWarning");
                this.OnPropertyChanged("ShowHelp");
                helpService.UserStateUpdated(user);
            }
        }

        void kinectService_NewPlayer(object sender, EventArgs e)
        {
            helpService.NewUser(user);
        }
        #endregion

        /// <summary>
        /// Notifies boardsupdated subscribers
        /// </summary>
        private void NotifyBoardSubscribers()
        {
            this.kinectService.SkeletonUpdated -= new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);
            this.kinectService.NewPlayer -= new EventHandler(kinectService_NewPlayer);
            if (BoardsUpdated != null)
                BoardsUpdated(this, new BoardEventArgs() { BoardService = this.boardService });
            this.kinectService.NewPlayer += new EventHandler(kinectService_NewPlayer);
            this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);
        }

        /// <summary>
        /// Starts up background jobs
        /// </summary>
        public void BeginBackgroundJobs()
        {
            this.sourceService.BeginPoll();
        }

        private Queue<Action> idleJobQueue;
        /// <summary>
        /// Routine that runs whenever the application isn't engaged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void appIdle_Tick(object sender, EventArgs e)
        {
            appIdleTimer.Stop();

            while (idleJobQueue.Count > 0)
            {
                Action action = idleJobQueue.Dequeue();
                action();
            }
            boardService.Reset();
            this.OnPropertyChanged("OutOfRange");
            appIdleTimer.Start();
        }


        private bool updatingViewState = false;
        /// <summary>
        /// Resets movement
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void UpdateBrowse(double x, double y)
        {
            updatingViewState = true;
            oldGlobalX = x;
            oldGlobalY = y;
            EntryX = user.DominantArmHandOffsetX;
            EntryY = user.DominantArmHandOffsetY;
            globalX = oldGlobalX;
            globalY = oldGlobalY;
            updatingViewState = false;
        }

        #region Help Properties
        private bool warningShown = false;
        public bool ShowingWarning
        {
            set{
                warningShown = value;
            }
            get
            {
                return warningShown;
            }
        }

        public bool ShowHelp
        {
            get
            {
                return !ShowingWarning;
            }
        }

        private string warningMessage = "";
        public string WarningMessage
        {
            get
            {
                return warningMessage;
            }
        }

        private string helpMessage = "";
        public string HelpMessage
        {
            get
            {
                return helpMessage;
            }
        }

        private string helpImage = "";
        public string HelpImage
        {
            get
            {
                return helpImage;
            }
        }

        #endregion

        #region Hand Movement Properties

        /// <summary>
        /// Returns true if user's hand is out of the viewing area
        /// </summary>
        public bool OutOfBounds
        {
            get
            {
                return Engaged && (OutOfBoundsLeft || OutOfBoundsRight || OutOfBoundsTop || OutOfBoundsBottom);
            }
        }

        /// <summary>
        /// Returns true if user's hand is far right out of the viewing area
        /// </summary>
        public bool OutOfBoundsRight
        {
            get
            {
                return user.DominantHand.HandX >= 1910;
            }
        }

        /// <summary>
        /// Returns true if user's hand is far bottom out of the viewing area
        /// </summary>
        public bool OutOfBoundsBottom
        {
            get
            {
                return user.DominantHand.HandY <= 10;
            }
        }

        /// <summary>
        /// Returns true if user's hand is far top out of the viewing area
        /// </summary>
        public bool OutOfBoundsTop
        {
            get
            {
                return user.DominantHand.HandY >= 1080;
            }
        }

        /// <summary>
        /// Returns true if the user's hand is far left out of the viewing area
        /// </summary>
        public bool OutOfBoundsLeft
        {
            get
            {
                return user.DominantHand.HandX <= 10;
            }
        }

        #endregion

        #region Board Properties
        private double maxBoardWidth = 0;
        /// <summary>
        /// Max width of current board
        /// </summary>
        public double MaxBoardWidth
        {
            set
            {
                maxBoardWidth = value;

            }
        }

        private double maxBoardHeight = 0;
        /// <summary>
        /// Max height of current board
        /// </summary>
        public double MaxBoardHeight
        {
            set
            {
                maxBoardHeight = value;
            }
        }
        /// <summary>
        /// Returns true if there are more categories to the right of the current board
        /// </summary>
        public bool MoreCategoriesRight
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if there are more categories to the left of the current board
        /// </summary>
        public bool MoreCategoriesLeft
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if there are more events hidden currently at the bottom of the current board
        /// </summary>
        public bool MoreEventsDown
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if there are events hidden currently at the top of the current board
        /// </summary>
        public bool MoreEventsUp
        {
            get
            {
                return false;
            }
        }

        #endregion
                
        #region Hand States

        /// <summary>
        /// Returns true if user is "touching" the invisible screen
        /// </summary>
        public bool Engaged
        {
            get { return user.Touching; }
        }

        /// <summary>
        /// Returns true if the user is standing too close to the screen
        /// </summary>
        public bool TooClose
        {
            get { return user.TooClose; }
        }

        /// <summary>
        /// Returns the user's distance from the invisible touch screen
        /// </summary>
        public double DistanceFromInvisScreen
        {
            get
            {
                return user.DistanceFromInvisScreen;
            }
        }

        /// <summary>
        /// Returns the user's active hand's absolute X position
        /// </summary>
        public double DominantArmHandOffsetX
        {
            get { return user.DominantArmHandOffsetX; }
        }

        /// <summary>
        /// Returns the user's active hand's absolute Y position
        /// </summary>
        public double DominantArmHandOffsetY
        {
            get { return user.DominantArmHandOffsetY; }
        }

        private bool firstEntry = true;

        private double oldGlobalX = 0;
        private double globalX = 0;
        private double EntryX = 0;
        /// <summary>
        /// Returns the relative X offset of the user's hand position for use with panning the UI surface
        /// </summary>
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
            get 
            {
                if (firstEntry)
                    return 0;
                return globalX; 
            }
        }

        private double EntryY = 0;
        private double oldGlobalY = 0;
        private double globalY = 0;
        /// <summary>
        /// Returns the relative Y offset of the user's hand position for use with panning the UI surface
        /// </summary>
        public double GlobalOffsetY
        {
            set 
            {
                if (EntryY - value + oldGlobalY > (App.Current.MainWindow.ActualHeight / 2 - 250))
                    globalY = App.Current.MainWindow.ActualHeight / 2 - 250;
                else if (EntryY - value + oldGlobalY < (-maxBoardHeight + maxEventHeight))
                    globalY = (-maxBoardHeight +maxEventHeight);
                else
                    globalY = EntryY - value + oldGlobalY; this.OnPropertyChanged("GlobalOffsetY");
            }
            get
            {
                if (firstEntry)
                    return 0; 
                return globalY; 
            }
        }

        public bool UserPresent
        {
            get { return !kinectService.IsIdle; }
        }

        public bool InRange
        {
            get { return user.TorsoInRange; }
        }

        public bool OutOfRange
        {
            get { return !InRange && UserPresent; }
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
            this.kinectService.NewPlayer -= kinectService_NewPlayer;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<BoardEventArgs> BoardsUpdated;
    }
}
