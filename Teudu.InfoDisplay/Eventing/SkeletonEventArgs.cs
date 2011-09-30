using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace Teudu.InfoDisplay
{
    public class SkeletonEventArgs : EventArgs 
    {
        public Vector HeadPosition { get; set; }
        public Vector ChestPosition { get; set; }
        public Vector LeftHandPosition { get; set; }
        public Vector RightHandPosition { get; set; }
        public Vector LeftShoulderPosition { get; set; }
        public Vector RightShoulderPosition { get; set; }
        public Vector LeftElbowPosition { get; set; }
        public Vector RightElbowPosition { get; set; }
        public Vector SpinePosition { get; set; }
        public Vector TorsoPosition { get; set; }
    }
}
