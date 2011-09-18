using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;

using System.Diagnostics;

namespace Teudu.InfoDisplay
{
    public class UserKinectService : IKinectService
    {
        Runtime runtime;
        bool isTrackingSkeleton;
        
        public void Initialize() 
        {
            runtime = new Runtime();
            
            runtime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(runtime_SkeletonFrameReady);

            //TODO: Have timer to poll idle

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

        void runtime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) 
        { 
            var skeleton = e.SkeletonFrame.Skeletons
                .Where(s => s.TrackingState == SkeletonTrackingState.Tracked)
                .FirstOrDefault(); 

            if (skeleton == null)
                return;

            var headPosition = skeleton.Joints[JointID.Head].ScaleTo(1920, 1080, 0.4f, 0.4f).Position;
            var rightHandPosition = skeleton.Joints[JointID.HandRight].ScaleTo(1920,1080, 0.4f, 0.4f).Position;
            var leftHandPosition = skeleton.Joints[JointID.HandLeft].ScaleTo(1920, 1080, 0.4f, 0.4f).Position;
            var torsoPosition = skeleton.Joints[JointID.HipCenter].ScaleTo(1920, 1080, 0.4f, 0.4f).Position;
            var leftElbowPosition = skeleton.Joints[JointID.ElbowLeft].ScaleTo(1920, 1080, 0.4f, 0.4f).Position;
            var rightElbowPosition = skeleton.Joints[JointID.ElbowRight].ScaleTo(1920, 1080, 0.4f, 0.4f).Position;
            var leftShoulderPosition = skeleton.Joints[JointID.ShoulderLeft].ScaleTo(1920, 1080, 0.4f, 0.4f).Position;
            var rightShoulderPosition = skeleton.Joints[JointID.ShoulderRight].ScaleTo(1920, 1080, 0.4f, 0.4f).Position;
            var spinePosition = skeleton.Joints[JointID.Spine].ScaleTo(1920, 1080, 0.4f, 0.4f).Position;

            isTrackingSkeleton = true;

            if (this.SkeletonUpdated != null) 
            { 
                this.SkeletonUpdated(this, new SkeletonEventArgs() { 
                    LeftHandPosition = leftHandPosition, 
                    RightHandPosition = rightHandPosition, 
                    LeftShoulderPosition = leftShoulderPosition,
                    RightShoulderPosition = rightShoulderPosition,
                    LeftElbowPosition = leftElbowPosition,
                    RightElbowPosition = rightElbowPosition,
                    SpinePosition = spinePosition,
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

        public bool IsIdle
        {
            get { return !isTrackingSkeleton; }
        }
    }
}
