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

namespace Teudu.InfoDisplay
{
    public class ViewModel: INotifyPropertyChanged
    {
        private const double PAN_TO_OFFSET = 200;
        private const int MAX_SCALE = 5;
        private const int MIN_SCALE = 1;
        private bool isZoomStart = false;

        IKinectService kinectService;

        public ViewModel(IKinectService kinectService) 
        {
            this.kinectService = kinectService; 
            this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);
            this.kinectService.SwipeHappened += new EventHandler<SwipeEventArgs>(kinectService_SwipeHappened);

            leftArm = new Arm();
            rightArm = new Arm();
            torso = new Torso();
        }

        void kinectService_SwipeHappened(object sender, SwipeEventArgs e)
        {
            if (!ViewChangeMode.Equals(HandsState.Panning))
                return;

            SetTargetCoords(e);
            this.OnPropertyChanged("TargetX");
            this.OnPropertyChanged("TargetY");
            Trace.WriteLineIf(e.Swipe.Equals(SwipeType.SwipeLeft), "Swipe Left");
            Trace.WriteLineIf(e.Swipe.Equals(SwipeType.SwipeRight), "Swipe Right");
            if (SwipeHappened != null)
                SwipeHappened(this, e);
        }
        
        void kinectService_SkeletonUpdated(object sender, SkeletonEventArgs e) 
        { 
            if (App.Current.MainWindow != null) 
            { 
                var midpointX = App.Current.MainWindow.Width / 2; 
                var midpointY = App.Current.MainWindow.Height / 2 + -300; //TODO: get height off ground
                this.RightHandOffsetX = midpointX + (e.RightHandPosition.X * 500);
                this.RightHandOffsetY = midpointY - (e.RightHandPosition.Y * 500);
                this.rightArm.HandZ = e.RightHandPosition.Z;
                this.rightArm.ElbowX = midpointX + (e.RightElbowPosition.X * 500);
                this.rightArm.ElbowY = midpointY - (e.RightElbowPosition.Y * 500);
                this.rightArm.ElbowZ = e.RightElbowPosition.Z;
                this.rightArm.ShoulderX = midpointX + (e.RightShoulderPosition.X * 500);
                this.rightArm.ShoulderY = midpointY - (e.RightShoulderPosition.Y * 500);
                this.rightArm.ShoulderZ = e.RightShoulderPosition.Z;

                this.LeftHandOffsetX = midpointX + (e.LeftHandPosition.X * 500);
                this.LeftHandOffsetY = midpointY - (e.LeftHandPosition.Y * 500);
                this.leftArm.HandZ = e.LeftHandPosition.Z;
                this.leftArm.ElbowX = midpointX + (e.LeftElbowPosition.X * 500);
                this.leftArm.ElbowY = midpointY - (e.LeftElbowPosition.Y * 500);
                this.leftArm.ElbowZ = e.LeftElbowPosition.Z;
                this.leftArm.ShoulderX = midpointX + (e.LeftShoulderPosition.X * 500);
                this.leftArm.ShoulderY = midpointY - (e.LeftShoulderPosition.Y * 500);
                this.leftArm.ShoulderZ = e.LeftShoulderPosition.Z;

                this.torso.Y = midpointY - (e.TorsoPosition.Y * 500);
                if (ViewChangeMode.Equals(HandsState.Panning))
                {
                    this.OnPropertyChanged("DominantHandOffsetX");
                    this.OnPropertyChanged("DominantHandOffsetY");
                    lastScale = ScaleLevel;
                    isZoomStart = true;
                }
                else if (ViewChangeMode.Equals(HandsState.Zooming))
                {
                    if (isZoomStart)
                        startHandDistance = HandsDistance;

                    ScaleLevel = lastScale + (HandsDistance - startHandDistance);
                    this.OnPropertyChanged("ScaleLevel");
                    isZoomStart = false;
                }
                else
                {
                    lastScale = ScaleLevel;
                    isZoomStart = true;
                }
                
                //Trace.WriteLine("Left arm length:" + leftArm.MaxArmSpan + ", curr:" + leftArm.CurrentArmSpan);
                //Trace.WriteLineIf(leftArm.HandAlmostParallel, "Left arm almost straight!");

            } 
        }

        Arm leftArm;
        Arm rightArm;
        Torso torso;

        private void SetTargetCoords(SwipeEventArgs e)
        {
            switch (e.Swipe)
            {
                case SwipeType.SwipeLeft:
                    TargetX = lastX - PAN_TO_OFFSET;
                    break;
                case SwipeType.SwipeRight:
                    TargetX = lastX + PAN_TO_OFFSET;
                    break;
            }
        }

        double lastX, lastY, currX, currY;

        public double TargetX
        {
            set { lastX = TargetX; currX = value; Trace.WriteLine("Moving to: " + value);}
            get { return currX; }
        }

        public double TargetY
        {
            set { lastY = TargetY; currY = value; }
            get { return currY; }
        }

        public HandsState ViewChangeMode
        {
            get
            {
                HandsState currentState;
                if (LeftHandActive && RightHandActive)
                    currentState = HandsState.Zooming;
                else if (!LeftHandActive || !RightHandActive)
                    currentState = HandsState.Panning;
                else
                    currentState = HandsState.Resting;

                return currentState;
            }
        }

        double lastScale, scale;
        public double ScaleLevel
        {
            set { if(value >= MIN_SCALE && value <= MAX_SCALE) scale = value; }
            get { return scale; }
        }

        public bool LeftHandActive
        {
            get { return LeftHandAboveTorso && leftArm.ArmAlmostStraight; }
        }

        public bool RightHandActive
        {
            get { return RightHandAboveTorso && rightArm.ArmAlmostStraight; }
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

        double startHandDistance;

        public double HandsDistance
        {
            get
            {
                return Math.Sqrt(Math.Pow(LeftHandOffsetX - RightHandOffsetX, 2) + Math.Pow(LeftHandOffsetY - RightHandOffsetY, 2))/250;
            }
        }

        public double DominantHandOffsetX
        {
            get { return DominantHand.HandOffsetX; }
        }

        public double DominantHandOffsetY
        {
            get { return DominantHand.HandOffsetY; }
        }

        public bool RightHandAboveTorso
        {
            get { return this.rightArm.HandY < torso.Y; }
        }

        public bool LeftHandAboveTorso
        {
            get { return this.leftArm.HandY < torso.Y; }
        }

        public double RightHandOffsetX 
        {
            get { return this.rightArm.HandX; }
            set { this.rightArm.HandX = value; } 
        }        
        
        public double RightHandOffsetY 
        {
            get { return this.rightArm.HandY; }
            set { this.rightArm.HandY = value;} 
        }

        public double LeftHandOffsetX
        {
            get { return this.leftArm.HandX; }
            set { this.leftArm.HandX = value;}
        }

        public double LeftHandOffsetY
        {
            get { return this.leftArm.HandY; }
            set { this.leftArm.HandY = value;}
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<SwipeEventArgs> SwipeHappened;

    }
}
