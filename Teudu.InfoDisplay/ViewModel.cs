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
       

        private int maxEventHeight;

        private bool firstEntry = true;
        private double EntryX = 0;
        private double EntryY = 0;
        private bool updatingViewState = false;

        IKinectService kinectService;
        ISourceService sourceService;
        IBoardService boardService;
        IHelpService helpService;

        DispatcherTimer appIdleTimer;
        UserState user;

        public ViewModel(IKinectService kinectService, ISourceService sourceService, IBoardService boardService, IHelpService helpService) 
        {
            user = new UserState();

            if (!Int32.TryParse(ConfigurationManager.AppSettings["MaxEventHeight"], out maxEventHeight))
                maxEventHeight = 340;

            idleJobQueue = new Queue<Action>();

            this.helpService = helpService;
            this.helpService.NewHelpMessage += new EventHandler<HelpMessageEventArgs>(helpService_NewHelpMessage);
            this.helpService.NewWarningMessage += new EventHandler<HelpMessageEventArgs>(helpService_NewWarningMessage);

            this.kinectService = kinectService; 

            this.sourceService = sourceService;
            this.sourceService.EventsUpdated += new EventHandler<SourceEventArgs>(sourceService_EventsUpdated);

            this.boardService = boardService;
            this.boardService.BoardsUpdated += new EventHandler(boardService_BoardsChanged);


            appIdleTimer = new DispatcherTimer();
            appIdleTimer.Interval = TimeSpan.FromSeconds(5);
            appIdleTimer.Tick += new EventHandler(appIdle_Tick);
            appIdleTimer.Start();
            helpService.NewUser(user);
            BeginBackgroundJobs();
        }

        void helpService_NewWarningMessage(object sender, HelpMessageEventArgs e)
        {
            warningMessage = e.Message;
            this.OnPropertyChanged("WarningMessage");
            this.OnPropertyChanged("ShowIndicators");
        }

        void helpService_NewHelpMessage(object sender, HelpMessageEventArgs e)
        {
            helpMessage = e.Message;
            helpImage = e.SupplementaryImage;
            this.OnPropertyChanged("HelpMessage");
            this.OnPropertyChanged("HelpImage");
            this.OnPropertyChanged("ShowIndicators");
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

            appIdleTimer.Start();
        }

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
        /// 
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
        /// Notifies boardsupdated subscribers
        /// </summary>
        private void NotifyBoardSubscribers()
        {
            this.kinectService.SkeletonUpdated -= new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);
            this.kinectService.NewPlayer -= new EventHandler(kinectService_NewPlayer);
            if (BoardsUpdated != null)
                BoardsUpdated(this, new BoardEventArgs() { BoardService = this.boardService});
            this.kinectService.NewPlayer += new EventHandler(kinectService_NewPlayer);
            this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);
        }

        #region Kinect

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinectService_SkeletonUpdated(object sender, SkeletonEventArgs e) 
        { 
            if (App.Current.MainWindow != null)
            {
                #region Set vals

                bool wasEngaged = user.Touching;

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

                if(user.Touching)
                    firstEntry = false;

                if (updatingViewState)
                    return;

                if (!wasEngaged)
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
                if (!user.TooClose)
                    this.OnPropertyChanged("DistanceFromInvisScreen");

                helpService.UserStateUpdated(user);
            } 
        }

        void kinectService_NewPlayer(object sender, EventArgs e)
        {
            helpService.NewUser(user);
        }

        #endregion

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

        public bool ShowIndicators
        {
            get
            {
                return helpImage == null || !helpImage.Equals("");
            }
        }
                
        #region Hand States

        public bool Engaged
        {
            get { return user.Touching; }
        }

        public bool TooClose
        {
            get { return user.TooClose; }
        }

        public double DistanceFromInvisScreen
        {
            get
            {
                return user.DistanceFromInvisScreen;
            }
        }

        public double DominantArmHandOffsetX
        {
            get { return user.DominantArmHandOffsetX; }
        }

        public double DominantArmHandOffsetY
        {
            get { return user.DominantArmHandOffsetY; }
        }

        public bool MoreCategoriesRight
        {
            get
            {
                return false;
            }
        }

        public bool MoreCategoriesLeft
        {
            get
            {
                return false;
            }
        }

        public bool MoreEventsDown
        {
            get
            {
                return false;
            }
        }

        public bool MoreEventsUp
        {
            get
            {
                return false;
            }
        }

        private double oldGlobalX = 0;
        private double globalX = 0;
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

        private double oldGlobalY = 0;
        private double globalY = 0;
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
        public event EventHandler MadeLargeMovement;
    }
}
