using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public interface IKinectService 
    { 
        void Initialize();
        bool IsIdle { get; }
        void Cleanup(); 
        event EventHandler<SkeletonEventArgs> SkeletonUpdated;   
    }
}
