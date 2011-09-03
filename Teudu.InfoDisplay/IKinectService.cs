using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public interface IKinectService 
    { 
        void Initialize(); 
        void Cleanup(); 
        event EventHandler<SkeletonEventArgs> SkeletonUpdated;   
        event EventHandler<SwipeEventArgs> SwipeHappened;
    }
}
