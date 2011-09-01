using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using System.Diagnostics;

namespace Teudu.InfoDisplay
{
    public class UserKinectService : IKinectService
    {
        Runtime runtime; 
        public void Initialize() 
        { 
            runtime = new Runtime(); 
            runtime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(runtime_SkeletonFrameReady);
            try
            {
                runtime.Initialize(RuntimeOptions.UseSkeletalTracking);
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
            
            var rightHandPosition = skeleton.Joints[JointID.HandRight].Position;
            var leftHandPosition = skeleton.Joints[JointID.HandLeft].Position;
            var spinePosition = skeleton.Joints[JointID.Spine].Position;
            var torsoPosition = skeleton.Joints[JointID.HipCenter].Position;
            var chestPosition = skeleton.Joints[JointID.ShoulderCenter].Position;
            var leftElbowPosition = skeleton.Joints[JointID.ElbowLeft].Position;
            var rightElbowPosition = skeleton.Joints[JointID.ElbowRight].Position;
            var leftShoulderPosition = skeleton.Joints[JointID.ShoulderLeft].Position;
            var rightShoulderPosition = skeleton.Joints[JointID.ShoulderRight].Position;
            
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
                    TorsoPosition = torsoPosition,
                    ChestPosition = chestPosition
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
    }
}
