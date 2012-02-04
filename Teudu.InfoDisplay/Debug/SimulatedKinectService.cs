using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay.Debug
{
    public class SimulatedKinectService : IKinectService
    {
        private bool isTrackingSkeleton = false;
        System.Windows.Threading.DispatcherTimer movementTimer;
        System.Windows.Threading.DispatcherTimer startTimer;
        public void Initialize()
        {
            movementTimer = new System.Windows.Threading.DispatcherTimer();
            movementTimer.Interval = new TimeSpan(70000);
            movementTimer.Tick += new EventHandler(movementTimer_Tick);

            startTimer = new System.Windows.Threading.DispatcherTimer();
            startTimer.Interval = new TimeSpan(0,0,5);
            startTimer.Tick += new EventHandler(startTimer_Tick);

            startTimer.Start();
            
        }

        void startTimer_Tick(object sender, EventArgs e)
        {
            movementTimer.Start();
            startTimer.Stop();
        }

        void movementTimer_Tick(object sender, EventArgs e)
        {
            //SlowMoveRight();
            JagToDown();
        }

        public bool IsIdle
        {
            get { return false; }
        }
        float x = -100f;
        float y = -100f;
        float invert = -1;
        private void SlowMoveRight()
        {
            
            //.ScaleTo(1920, 1080, 0.4f, 0.4f,true)
            //if (x < 1f)
            //{
                x -= 0.5f * invert;
                y += 1f;
                invert = -invert;

                isTrackingSkeleton = true;

                if (this.SkeletonUpdated != null)
                {
                    this.SkeletonUpdated(this, new SkeletonEventArgs()
                    {
                        RightHandPosition = new Microsoft.Kinect.SkeletonPoint() { Z = 1.6f, X = x, Y = (float)Math.Sin(Math.Log(x)) }.ScaleTo(1920, 1080, 0.4f, 0.4f),
                        LeftHandPosition = new Microsoft.Kinect.SkeletonPoint() { Z = 3f, X = x, Y = (float)Math.Sin(Math.Log(x)) }.ScaleTo(1920, 1080, 0.4f, 0.4f),
                        TorsoPosition = new Microsoft.Kinect.SkeletonPoint() { Z = 4.0f, X = 0, Y = 0 }.ScaleTo(1920, 1080, 0.4f, 0.4f)
                    });
                }
            //}
                
        }

        private void JagToDown()
        {

            //.ScaleTo(1920, 1080, 0.4f, 0.4f,true)

            if (x <= -1f)
                invert = 1f;
            if (x >= 1f)
                invert = -1f;

            x += (0.1f * invert);
            y += 0.2f;

            
            if (this.SkeletonUpdated != null)
            {
                this.SkeletonUpdated(this, new SkeletonEventArgs()
                {
                    RightHandPosition = new Microsoft.Kinect.SkeletonPoint() { Z = 1.6f, X = (float)Math.Sin(y), Y = y }.ScaleTo(1920, 1080, 0.4f, 0.4f),
                    LeftHandPosition = new Microsoft.Kinect.SkeletonPoint() { Z = 3f, X = x, Y = (float)Math.Sin(Math.Log(x)) }.ScaleTo(1920, 1080, 0.4f, 0.4f),
                    TorsoPosition = new Microsoft.Kinect.SkeletonPoint() { Z = 4.0f, X = 0, Y = 0 }.ScaleTo(1920, 1080, 0.4f, 0.4f)
                });
            }

        }

        public void Cleanup()
        {
            movementTimer.Stop();
        }

        public event EventHandler<SkeletonEventArgs> SkeletonUpdated;
        public event EventHandler NewPlayer;
    }
}
