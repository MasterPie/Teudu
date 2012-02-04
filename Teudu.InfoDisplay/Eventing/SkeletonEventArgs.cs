using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Teudu.InfoDisplay
{
    public class SkeletonEventArgs : EventArgs 
    {
        public SkeletonPoint HeadPosition { get; set; }
        public SkeletonPoint ChestPosition { get; set; }
        public SkeletonPoint LeftHandPosition { get; set; }
        public SkeletonPoint RightHandPosition { get; set; }
        public SkeletonPoint LeftShoulderPosition { get; set; }
        public SkeletonPoint RightShoulderPosition { get; set; }
        public SkeletonPoint LeftElbowPosition { get; set; }
        public SkeletonPoint RightElbowPosition { get; set; }
        public SkeletonPoint SpinePosition { get; set; }
        public SkeletonPoint TorsoPosition { get; set; }
    }
}
