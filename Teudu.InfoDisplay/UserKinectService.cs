using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Kinect.Toolbox;

using System.Diagnostics;

namespace Teudu.InfoDisplay
{
    public class UserKinectService : IKinectService
    {
        Runtime runtime;
        SwipeGestureDetector swipeDetector;
        
        
        public void Initialize() 
        {
            swipeDetector = new SwipeGestureDetector();
            //swipeDetector.MinimalPeriodBetweenGestures = 1000;
            //swipeDetector.OnGestureDetected += new Action<string>(swipeDetector_OnGestureDetected);
            runtime = new Runtime();
            
            runtime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(runtime_SkeletonFrameReady);

            try
            {
                runtime.Initialize(RuntimeOptions.UseSkeletalTracking);
                //runtime.SkeletonEngine.TransformSmooth = true;
                //runtime.SkeletonEngine.SmoothParameters = new TransformSmoothParameters()
                //{
                //    Smoothing = .75f,
                //    JitterRadius = 0.05f,
                //    MaxDeviationRadius = 0.04f
                //};
                Trace.WriteLine("Kinect initialized");
                
            }
            catch (Exception)
            {
                Trace.WriteLine("Error while initializing Kinect");
            }
        }

        void swipeDetector_OnGestureDetected(string obj)
        {
            if (SwipeHappened != null)
            {
                if (obj.Equals("SwipeToLeft"))
                    SwipeHappened(this, new SwipeEventArgs() { Swipe = SwipeType.SwipeLeft });

                if (obj.Equals("SwipeToRight"))
                    SwipeHappened(this, new SwipeEventArgs() { Swipe = SwipeType.SwipeRight });
            }
        }        
        
        void runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) 
        { 
            var skeleton = e.SkeletonFrame.Skeletons
                .Where(s => s.TrackingState == SkeletonTrackingState.Tracked)
                .FirstOrDefault(); 

            if (skeleton == null)
                return;

            var headPosition = skeleton.Joints[JointID.Head].Position;
            var rightHandPosition = skeleton.Joints[JointID.HandRight].Position;
            var leftHandPosition = skeleton.Joints[JointID.HandLeft].Position;
            var torsoPosition = skeleton.Joints[JointID.HipCenter].Position;
            var leftElbowPosition = skeleton.Joints[JointID.ElbowLeft].Position;
            var rightElbowPosition = skeleton.Joints[JointID.ElbowRight].Position;
            var leftShoulderPosition = skeleton.Joints[JointID.ShoulderLeft].Position;
            var rightShoulderPosition = skeleton.Joints[JointID.ShoulderRight].Position;

            /*
             * Apparently, doing this check is expensive
             */
            //if(skeleton.Joints[JointID.HandRight].TrackingState == JointTrackingState.Tracked)
            //    swipeDetector.Add(rightHandPosition, runtime.SkeletonEngine);
            //else
            //    swipeDetector.Add(leftHandPosition, runtime.SkeletonEngine);
            //swipeDetector.Add(rightHandPosition, runtime.SkeletonEngine);
            
            if (this.SkeletonUpdated != null) 
            { 
                this.SkeletonUpdated(this, new SkeletonEventArgs() { 
                    LeftHandPosition = leftHandPosition, 
                    RightHandPosition = rightHandPosition, 
                    LeftShoulderPosition = leftShoulderPosition,
                    RightShoulderPosition = rightShoulderPosition,
                    LeftElbowPosition = leftElbowPosition,
                    RightElbowPosition = rightElbowPosition,
                    HeadPosition = headPosition,
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
        
        public event EventHandler<SkeletonEventArgs> SkeletonUpdated;
        public event EventHandler<SwipeEventArgs> SwipeHappened;
    }
}
