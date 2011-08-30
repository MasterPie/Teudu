using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace Teudu.InfoDisplay
{
    public class UserKinectService : IKinectService
    {
        Runtime runtime; 
        public void Initialize() 
        { 
            runtime = new Runtime(); 
            runtime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(runtime_SkeletonFrameReady); 
            runtime.Initialize(RuntimeOptions.UseSkeletalTracking); 
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
            
            if (this.SkeletonUpdated != null) 
            { 
                this.SkeletonUpdated(this, new SkeletonEventArgs() { LeftHandPosition = leftHandPosition, RightHandPosition = rightHandPosition}); 
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
