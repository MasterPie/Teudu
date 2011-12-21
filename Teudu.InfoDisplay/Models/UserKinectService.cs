using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;
using Kinect.Toolbox;

using System.Diagnostics;
using System.Windows.Threading;
using System.Configuration;

namespace Teudu.InfoDisplay
{
    public class UserKinectService : IKinectService
    {
        
        Runtime runtime;
        bool isTrackingSkeleton = false;
        DispatcherTimer kinectRetryTimer;
        double maxUserDistance = -1;

        int currentPlayerId = -1;

        public void Initialize() 
        {
            kinectRetryTimer = new DispatcherTimer();
            kinectRetryTimer.Interval = TimeSpan.FromSeconds(5);
            kinectRetryTimer.Tick += new EventHandler(kinectRetryTimer_Tick);

            if (!Double.TryParse(ConfigurationManager.AppSettings["MaxUserDistance"], out maxUserDistance))
                maxUserDistance = 3.0;

            StartKinect();
        }

        /// <summary>
        /// Runs when Kinect camera fails to initialize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinectRetryTimer_Tick(object sender, EventArgs e)
        {
            kinectRetryTimer.Stop();
            StartKinect();
        }

        void StartKinect()
        {
            try
            {
                runtime = Runtime.Kinects[0];

                runtime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(runtime_SkeletonFrameReady);

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
        
        /// <summary>
        /// Runs when the Kinect recalculates the skeletons in a scene (which happens constantly)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) 
        { 
            var skeleton = e.SkeletonFrame.Skeletons
                .Where(s => s.TrackingState == SkeletonTrackingState.Tracked && s.Position.Z <=  maxUserDistance) //Tracked skeletons and those skeletons in range
                .OrderBy(x => Math.Sqrt(Math.Pow(Math.Abs(x.Position.X),2) + Math.Pow(Math.Abs(x.Position.Z),2))) //track closest
                //.ThenBy(y => Math.Abs(y.Position.X)) //track centermost
                .FirstOrDefault();

            if (skeleton == null)
            {
                isTrackingSkeleton = false;
                return;
            }

            if (skeleton.TrackingID != currentPlayerId)
            {
                currentPlayerId = skeleton.TrackingID;
                this.NewPlayer(this, new EventArgs());
            }

            var rightHandPosition = skeleton.Joints[JointID.HandRight].ScaleTo(1920, 1080, 0.4f, 0.4f, false).Position;
            var leftHandPosition = skeleton.Joints[JointID.HandLeft].ScaleTo(1920, 1080, 0.4f, 0.4f, false).Position;
            var torsoPosition = skeleton.Joints[JointID.HipCenter].ScaleTo(1920, 1080, 0.4f, 0.4f, false).Position;

            isTrackingSkeleton = true;

            if (this.SkeletonUpdated != null) 
            { 
                this.SkeletonUpdated(this, new SkeletonEventArgs() { 
                    LeftHandPosition = leftHandPosition, 
                    RightHandPosition = rightHandPosition,
                    TorsoPosition = torsoPosition
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
        
        

        public bool IsIdle
        {
            get { return !isTrackingSkeleton; }
        }

        public event EventHandler<SkeletonEventArgs> SkeletonUpdated;
        public event EventHandler NewPlayer;
    }
}
