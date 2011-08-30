using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

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
        }        
        
        void kinectService_SkeletonUpdated(object sender, SkeletonEventArgs e) 
        { 
            if (App.Current.MainWindow != null) 
            { 
                var midpointX = App.Current.MainWindow.Width / 2; 
                var midpointY = App.Current.MainWindow.Height / 2;
                this.RightHandOffsetX = midpointX + (e.RightHandPosition.X * 500);
                this.RightHandOffsetY = midpointY - (e.RightHandPosition.Y * 500);
                this.LeftHandOffsetX = midpointX + (e.LeftHandPosition.X * 500);
                this.LeftHandOffsetY = midpointY - (e.LeftHandPosition.Y * 500); 
            } 
        }        
        
        double rightHandOffsetX; 
        
        public double RightHandOffsetX 
        {
            get { return this.rightHandOffsetX; }
            set { this.rightHandOffsetX = value; this.OnPropertyChanged("RightHandOffsetX"); } 
        }        
        
        double rightHandOffsetY; 
        public double RightHandOffsetY 
        {
            get { return this.rightHandOffsetY; }
            set { this.rightHandOffsetY = value; this.OnPropertyChanged("RightHandOffsetY"); } 
        }

        double leftHandOffsetX;

        public double LeftHandOffsetX
        {
            get { return this.leftHandOffsetX; }
            set { this.leftHandOffsetX = value; this.OnPropertyChanged("LeftHandOffsetX"); }
        }

        double leftHandOffsetY;
        public double LeftHandOffsetY
        {
            get { return this.leftHandOffsetY; }
            set { this.leftHandOffsetY = value; this.OnPropertyChanged("LeftHandOffsetY"); }
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
