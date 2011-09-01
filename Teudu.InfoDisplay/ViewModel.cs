using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using System.Diagnostics;

namespace Teudu.InfoDisplay
{
    public class ViewModel: INotifyPropertyChanged
    {
        IKinectService kinectService;
        
        public ViewModel(IKinectService kinectService) 
        {
            this.RightHandOffsetX = 100;
            this.RightHandOffsetY = 100; 
            this.kinectService = kinectService; 
            this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);

            leftArm = new Arm();
            rightArm = new Arm();
            torso = new Torso();
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
                    //Trace.WriteLine("Hand (" + DominantHandOffsetX + ", " + DominantHandOffsetY + ")");
                }
                else if (ViewChangeMode.Equals(HandsState.Zooming))
                    this.OnPropertyChanged("HandsDistance");
                else
                {
                    Trace.WriteLine("-");
                }
                //Trace.WriteLine("Left arm length:" + leftArm.MaxArmSpan + ", curr:" + leftArm.CurrentArmSpan);
                //Trace.WriteLineIf(leftArm.HandAlmostParallel, "Left arm almost straight!");

            } 
        }

        Arm leftArm;
        Arm rightArm;
        Torso torso;       

        public HandsState ViewChangeMode
        {
            get
            {
                HandsState currentState;
                if (LeftHandActive && RightHandActive)
                    currentState = HandsState.Zooming;
                else if (LeftHandActive || RightHandActive)
                    currentState = HandsState.Panning;
                else
                    currentState = HandsState.Resting;

                return currentState;
            }
        }

        public bool LeftHandActive
        {
            get { return LeftHandAboveTorso && leftArm.HandAlmostParallel; }
        }

        public bool RightHandActive
        {
            get { return RightHandAboveTorso && rightArm.HandAlmostParallel; }
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
                return Math.Sqrt(Math.Pow(LeftHandOffsetX - RightHandOffsetX, 2) + Math.Pow(LeftHandOffsetY - RightHandOffsetY, 2))/250;
            }
        }

        public double DominantHandOffsetX
        {
            get { return DominantHand.HandX; }
        }

        public double DominantHandOffsetY
        {
            get { return DominantHand.HandY; }
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
    }
}
