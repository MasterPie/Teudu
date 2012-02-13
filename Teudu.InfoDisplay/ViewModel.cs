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
        }

        /// <summary>
        /// Starts up background jobs
        /// </summary>
        public void BeginBackgroundJobs()
        {
            this.sourceService.BeginPoll();
        }

        #region Background jobs

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
            NewUserGuidesShowing = true;
            appIdleTimer.Start();
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

        private bool initialEventSet = true;
        /// <summary>
        /// Sets boardservice to new events when EventsUpdated is fired
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sourceService_EventsUpdated(object sender, SourceEventArgs e)
        {
            if (initialEventSet)
            {
                initialEventSet = false;
                boardService.Events = e.Events;
            }
            else
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

                    NewUserGuidesShowing = false; // because he finished his instructional training
                }

                this.OnPropertyChanged("Engaged");
                this.OnPropertyChanged("TooClose");
                this.OnPropertyChanged("OutOfBounds");
                this.OnPropertyChanged("OutOfBoundsLeft");
                this.OnPropertyChanged("OutOfBoundsTop");
                this.OnPropertyChanged("OutOfBoundsRight");
                this.OnPropertyChanged("OutOfBoundsBottom");
                this.OnPropertyChanged("InRange");
                this.OnPropertyChanged("OutOfRange");

                if (!user.TooClose)
                    this.OnPropertyChanged("DistanceFromInvisScreen");

                helpService.UserStateUpdated(user);
            }
        }

        void kinectService_NewPlayer(object sender, EventArgs e)
        {
            helpService.NewUser(user);
            NewUserGuidesShowing = true;
        }
        #endregion


        private bool updatingViewState = false;
        /// <summary>
        /// Updates the View's current browse state location (based on which board is currently viewing)
        /// </summary>
        /// <param name="x">x displacement of the view</param>
        /// <param name="y">y displacement of the view</param>
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
        private string helpMessage = "";
        /// <summary>
        /// The current help message to be shown
        /// </summary>
        public string HelpMessage
        {
            get
            {
                return helpMessage;
            }
        }

        private string helpImage = "";
        /// <summary>
        /// The current help image to be shown
        /// </summary>
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
                if (user.DominantHand.HandX >= 1910)
                    System.Console.WriteLine("out of bounds right: {0}", user.DominantHand.HandX);
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
                return user.DominantHand.HandY >= 1080;
                
            }
        }

        /// <summary>
        /// Returns true if user's hand is far top out of the viewing area
        /// </summary>
        public bool OutOfBoundsTop
        {
            get
            {
                return user.DominantHand.HandY <= 10;
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

        /// <summary>
        /// Returns true if the KinectService is tracking a user
        /// </summary>
        public bool UserPresent
        {
            get { return !kinectService.IsIdle; }
        }

        /// <summary>
        /// Returns true if the user's body is in range to use the application
        /// </summary>
        public bool InRange
        {
            get { return user.TorsoInRange; }
        }

        /// <summary>
        /// Returns true if the user is not in range to use the application
        /// </summary>
        public bool OutOfRange
        {
            get { return !InRange && UserPresent; }
        }

        #endregion


        private bool newUserGuidesShowing = false;
        public bool NewUserGuidesShowing
        {
            get
            {
                return newUserGuidesShowing;
            }

            set
            {
                newUserGuidesShowing = value;
                this.OnPropertyChanged("NewUserGuidesShowing");
            }

        }

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
        /// <summary>
        /// Event raised when the boards of the BoardService are updated
        /// </summary>
        public event EventHandler<BoardEventArgs> BoardsUpdated;
    }
}
