using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;

namespace Teudu.InfoDisplay
{
    public static class KinectTools
    {
        public static Joint ScaleTo(this Joint joint, int width, int height, float skeletonMaxX, float skeletonMaxY, bool originCenter)
        {
            if (!originCenter)
                return Coding4Fun.Kinect.Wpf.SkeletalExtensions.ScaleTo(joint, width, height, skeletonMaxX, skeletonMaxY);

            SkeletonPoint pos = new SkeletonPoint()
            {
                X = Scale(width, skeletonMaxX, joint.Position.X),
                Y = Scale(height, skeletonMaxY, -joint.Position.Y),
                Z = joint.Position.Z,
            };

            joint.Position = pos;

            return joint;
        }

        public static SkeletonPoint ScaleTo(this SkeletonPoint position, int width, int height, float skeletonMaxX, float skeletonMaxY)
        {
            SkeletonPoint pos = new SkeletonPoint()
            {
                X = Scale(width, skeletonMaxX, position.X),
                Y = Scale(height, skeletonMaxY, -position.Y),
                Z = position.Z,
            };

            return pos;
        }

        //From Coding4Fun source
        private static float Scale(int maxPixel, float maxSkeleton, float position)
        {
            float value = ((((maxPixel / maxSkeleton) / 2) * position));
            //if (value > maxPixel)
            //    return maxPixel;
            //if (value < 0)
            //    return 0;
            return value;
        }

    }
}
