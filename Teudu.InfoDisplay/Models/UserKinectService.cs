using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;
using Kinect.Toolbox;

using System.Diagnostics;
using System.Windows.Threading;

namespace Teudu.InfoDisplay
{
    public class UserKinectService : IKinectService
    {
        
        Runtime runtime;
        bool isTrackingSkeleton;
        DispatcherTimer kinectRetryTimer;
        
        public void Initialize() 
        {
           
            runtime = new Runtime();
            
            runtime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(runtime_SkeletonFrameReady);

            kinectRetryTimer = new DispatcherTimer();
            kinectRetryTimer.Interval = TimeSpan.FromSeconds(5);
            kinectRetryTimer.Tick += new EventHandler(kinectRetryTimer_Tick);
            //TODO: Have timer to poll idle

            
            StartKinect();
                
                /*Smoothing = .75f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.04f*/
            
        }

        void kinectRetryTimer_Tick(object sender, EventArgs e)
        {
            kinectRetryTimer.Stop();
            StartKinect();
        }

        void StartKinect()
        {
            try
            {
                runtime.Initialize(RuntimeOptions.UseSkeletalTracking);
                runtime.SkeletonEngine.TransformSmooth = true;
                runtime.SkeletonEngine.SmoothParameters = new TransformSmoothParameters()
                {
                    Smoothing = 0.75f,
                    Correction = 0.0f,
                    Prediction = 0.0f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.4f
                };
                Trace.WriteLine("Kinect initialized");
            }
            catch (Exception)
            {
                Trace.WriteLine("Error while initializing Kinect. Trying again in 5 seconds...");
                kinectRetryTimer.Start();
            }
        }

        void runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) 
        { 
            var skeleton = e.SkeletonFrame.Skeletons
                .Where(s => s.TrackingState == SkeletonTrackingState.Tracked)
                .OrderBy(x => x.Position.Z) //track closest
                .FirstOrDefault(); 

            if (skeleton == null)
                return;

            var rightHandPosition = skeleton.Joints[JointID.HandRight].ScaleTo(1920, 1080, 0.4f, 0.4f, false).Position;
            var leftHandPosition = skeleton.Joints[JointID.HandLeft].ScaleTo(1920, 1080, 0.4f, 0.4f, false).Position;

            isTrackingSkeleton = true;

            if (this.SkeletonUpdated != null) 
            { 
                this.SkeletonUpdated(this, new SkeletonEventArgs() { 
                    LeftHandPosition = leftHandPosition, 
                    RightHandPosition = rightHandPosition
                }); 
            } 
        }
        
        public void Cleanup() 
        { 
            if (runtime != null) 
            { 
                runtime.Uninitialize(); 
            } 
        }        
        
        public event EventHandler<SkeletonEventArgs> SkeletonUpdated;

        public bool IsIdle
        {
            get { return !isTrackingSkeleton; }
        }
    }
}
