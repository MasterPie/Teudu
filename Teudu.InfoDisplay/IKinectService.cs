using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public interface IKinectService 
    { 
        /// <summary>
        /// Initializes the KinectService
        /// </summary>
        void Initialize();
        /// <summary>
        /// Returns true if the Kinect is currently idle (not detecting a skeleton)
        /// </summary>
        bool IsIdle { get; }
        /// <summary>
        /// Releases all resources
        /// </summary>
        void Cleanup(); 
        /// <summary>
        /// Event raised when the KinectService calculates skeletons (e.g. updates the locations of joints and/or tracks a new skeleton)
        /// </summary>
        event EventHandler<SkeletonEventArgs> SkeletonUpdated;
        /// <summary>
        /// Event raised when the KinectService begins tracking a new user
        /// </summary>
        event EventHandler NewPlayer;
    }
}
