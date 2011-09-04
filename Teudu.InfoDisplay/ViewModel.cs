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

namespace Teudu.InfoDisplay
{
    public class ViewModel: INotifyPropertyChanged
    {
        private const double PAN_TO_OFFSET = 100;
        private double ARM_SCALE_FACTOR_X = (App.Current.MainWindow.Width / 2) * 3;
        private double ARM_SCALE_FACTOR_Y = (App.Current.MainWindow.Height / 2) * 3;
        private double ARM_SCALE_FACTOR_Z = 500; // Distance to tv

        private const int MAX_SCALE = 4;
        private const int MIN_SCALE = 1;
        private bool isZoomStart = false;

        private static double hotspotRegionX = App.Current.MainWindow.Width / 12;
        private static double hotspotRegionY = App.Current.MainWindow.Height / 8;

        private double hotspotLeft = 0;
        private double hotSpotRight = App.Current.MainWindow.Width - hotspotRegionX;
        private double hotSpotTop = 0;
        private double hotSpotBottom = App.Current.MainWindow.Height - hotspotRegionY;

        IKinectService kinectService;

        public ViewModel(IKinectService kinectService) 
        {
            this.kinectService = kinectService; 
            this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);

            leftArm = new Arm();
            rightArm = new Arm();
            head = new Head();
            torso = new Torso();
        }

        
        void kinectService_SkeletonUpdated(object sender, SkeletonEventArgs e) 
        { 
            if (App.Current.MainWindow != null)
            {
                #region Set vals
                var midpointX = App.Current.MainWindow.ActualWidth / 2; 
                var midpointY = App.Current.MainWindow.ActualHeight / 2 + -300; //TODO: get height off ground

                this.head.Y = e.HeadPosition.Y * ARM_SCALE_FACTOR_Y;

                this.rightArm.HandX = midpointX + (e.RightHandPosition.X * ARM_SCALE_FACTOR_X);
                this.rightArm.HandY = midpointY - (e.RightHandPosition.Y * ARM_SCALE_FACTOR_Y);
                this.rightArm.HandZ = e.RightHandPosition.Z * ARM_SCALE_FACTOR_Z;
                this.rightArm.ElbowX = midpointX + (e.RightElbowPosition.X * ARM_SCALE_FACTOR_X);
                this.rightArm.ElbowY = midpointY - (e.RightElbowPosition.Y * ARM_SCALE_FACTOR_Y);
                this.rightArm.ElbowZ = e.RightElbowPosition.Z * ARM_SCALE_FACTOR_Z;
                this.rightArm.ShoulderX = midpointX + (e.RightShoulderPosition.X * ARM_SCALE_FACTOR_X);
                this.rightArm.ShoulderY = midpointY - (e.RightShoulderPosition.Y * ARM_SCALE_FACTOR_Y);
                this.rightArm.ShoulderZ = e.RightShoulderPosition.Z * ARM_SCALE_FACTOR_Z;

                this.leftArm.HandX = midpointX + (e.LeftHandPosition.X * ARM_SCALE_FACTOR_X);
                this.leftArm.HandY = midpointY - (e.LeftHandPosition.Y * ARM_SCALE_FACTOR_Y);
                this.leftArm.HandZ = e.LeftHandPosition.Z * ARM_SCALE_FACTOR_Z;
                this.leftArm.ElbowX = midpointX + (e.LeftElbowPosition.X * ARM_SCALE_FACTOR_X);
                this.leftArm.ElbowY = midpointY - (e.LeftElbowPosition.Y * ARM_SCALE_FACTOR_Y);
                this.leftArm.ElbowZ = e.LeftElbowPosition.Z * ARM_SCALE_FACTOR_Z;
                this.leftArm.ShoulderX = midpointX + (e.LeftShoulderPosition.X * ARM_SCALE_FACTOR_X);
                this.leftArm.ShoulderY = midpointY - (e.LeftShoulderPosition.Y * ARM_SCALE_FACTOR_Y);
                this.leftArm.ShoulderZ = e.LeftShoulderPosition.Z * ARM_SCALE_FACTOR_Z;

                this.spine.Z = e.SpinePosition.Z * ARM_SCALE_FACTOR_Z;

                this.torso.Y = midpointY - (e.TorsoPosition.Y * ARM_SCALE_FACTOR_Y);
                #endregion

                if (ViewChangeMode == HandsState.Panning)
                {
                    if (!panPreparing && (IsNearBot || IsNearTop || IsNearLeft || IsNearRight))
                        PreparePan();
                    else if(!IsNearBot && !IsNearTop && !IsNearLeft && !IsNearRight)
                    {
                        this.OnPropertyChanged("DominantArmHandOffsetX");
                        this.OnPropertyChanged("DominantArmHandOffsetY");
                    }

                    lastScale = ScaleLevel;
                    isZoomStart = true;
                }
                else if (ViewChangeMode == HandsState.Zooming)
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

                Trace.WriteLineIf(LeftArmInFront, "Left arm z " + leftArm.HandZ + " , spine z " + spine.Z);
                Trace.WriteLineIf(RightArmInFront, "Right arm z " + rightArm.HandZ + " , spine z " + spine.Z);
                
                //Trace.WriteLineIf(IsNearLeft, "Left at" + DominantHand.HandX);
                //Trace.WriteLineIf(IsNearRight, "Right at" + DominantHand.HandX);
                //Trace.WriteLineIf(IsNearTop, "Top at" + DominantHand.HandY);
                //Trace.WriteLineIf(IsNearBot, "Bot at" + DominantHand.HandY);
            } 
        }

        Arm leftArm;
        Arm rightArm;
        Torso torso;
        Head head;
        Spine spine;

        bool panPreparing = false;

        private void PreparePan()
        {
            panPreparing = true;
            SetTargetCoords();
            this.OnPropertyChanged("TargetX");
            this.OnPropertyChanged("TargetY");

            if (PanRequest != null)
                PanRequest(this, new EventArgs());
            
            panPreparing = false;
        }

        private void SetTargetCoords()
        {
            if (ViewChangeMode != HandsState.Panning)
                return;

            if(IsNearLeft)
                TargetX = lastX + PAN_TO_OFFSET;
            if(IsNearRight)
                TargetX = lastX - PAN_TO_OFFSET;
            if (IsNearTop)
                TargetY = lastY + PAN_TO_OFFSET;
            if (IsNearBot)
                TargetY = lastY - PAN_TO_OFFSET;

            //Trace.WriteLine("Moving to: " + TargetX + " , " + TargetY + " from " + lastX +" , " + lastY);
        }
        double lastX = 0.0;
        double lastY = 0.0;
        double currX, currY;

        public double TargetX
        {
            set 
            { 
                if (value / ScaleLevel >= -(App.Current.MainWindow.Width / 2) && value / ScaleLevel <= (App.Current.MainWindow.Width / 2)) 
                { 
                    lastX = TargetX; 
                    currX = value / ScaleLevel; 
                } 
            }
            get { return currX; }
        }

        public double TargetY
        {
            set 
            {
                if (value / ScaleLevel >= -(App.Current.MainWindow.Height / 2) && value / ScaleLevel <= (App.Current.MainWindow.Height / 2))
                {
                    lastY = TargetY;
                    currY = value / ScaleLevel;
                }
            }
            get { return currY; }
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
            get {
                //return DominantHand.HandY > head.Y;}
                return (DominantHand.HandY >= hotSpotTop) && (DominantHand.HandY < (hotSpotTop + hotspotRegionY)); }
        }

        public bool IsNearBot
        {
            get{return (DominantHand.HandY > hotSpotBottom) && (DominantHand.HandY <= (hotSpotBottom + hotspotRegionY)); }
        }

        public bool LeftArmInFront
        {
            get { return leftArm.HandZ < (spine.Z - 100); }
        }

        public bool RightArmInFront
        {
            get { return rightArm.HandZ < (spine.Z - 100); }
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

        double lastScale, scale = 1;
        public double ScaleLevel
        {
            set 
            { 
                if(value >= MIN_SCALE && value <= MAX_SCALE) 
                    scale = value;
                if (value < MIN_SCALE)
                    lastScale = MIN_SCALE;
                if (value > MAX_SCALE)
                    lastScale = MAX_SCALE;

                Trace.WriteLine("Scale trying set at: " + value);
            }
            get { return scale; }
        }

        public bool LeftHandActive
        {
            get
            {
                return LeftArmInFront;}
                //return LeftHandAboveTorso && leftArm.ArmAlmostStraight; }
        }

        public bool RightHandActive
        {
            get
            {
                return RightArmInFront;}
                //return RightHandAboveTorso && rightArm.ArmAlmostStraight; }
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
                return Math.Sqrt(Math.Pow(this.leftArm.HandX - this.rightArm.HandX, 2) + 
                    Math.Pow(this.leftArm.HandY - this.rightArm.HandY, 2)) / 250; //TODO no hardcoded factor
            }
        }

        public double DominantArmHandOffsetX
        {
            get { return -DominantHand.HandOffsetX; }
        }

        public double DominantArmHandOffsetY
        {
            get { return -DominantHand.HandOffsetY; }
        }

        public bool RightHandAboveTorso
        {
            get { return this.rightArm.HandY < torso.Y; }
        }

        public bool LeftHandAboveTorso
        {
            get { return this.leftArm.HandY < torso.Y; }
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
        public event EventHandler PanRequest;

    }
}
