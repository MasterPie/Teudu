using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;

namespace Teudu.InfoDisplay
{
    public class SkeletonEventArgs : EventArgs 
    {
        public Vector LeftHandPosition { get; set; }
        public Vector RightHandPosition { get; set; } 
    }
}
