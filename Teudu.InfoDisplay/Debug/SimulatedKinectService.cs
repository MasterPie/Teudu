using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay.Debug
{
    public class SimulatedKinectService : IKinectService
    {
        System.Windows.Threading.DispatcherTimer movementTimer;
        System.Windows.Threading.DispatcherTimer startTimer;
        public void Initialize()
        {
            movementTimer = new System.Windows.Threading.DispatcherTimer();
            movementTimer.Interval = new TimeSpan(20000);
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
            SlowMoveRight();
        }

        public bool IsIdle
        {
            get { return false; }
        }
        float x = -100f;
        float y = -100f;
        private void SlowMoveRight()
        {
            
            //.ScaleTo(1920, 1080, 0.4f, 0.4f,true)
            //if (x < 1f)
            //{
                x += 1f;
                y += 1f;

                if (this.SkeletonUpdated != null)
                {
                    this.SkeletonUpdated(this, new SkeletonEventArgs()
                    {
                        RightHandPosition = new Microsoft.Research.Kinect.Nui.Vector() { Z = 1, X = x, Y = (float)Math.Sin(Math.Log(x)) }
                    });
                }
            //}
                
        }

        private void JagToDown()
        {

            //.ScaleTo(1920, 1080, 0.4f, 0.4f,true)

            x += 1f;
            y += 1f;

            if (this.SkeletonUpdated != null)
            {
                this.SkeletonUpdated(this, new SkeletonEventArgs()
                {
                    RightHandPosition = new Microsoft.Research.Kinect.Nui.Vector() { Z = 1, X = (float)Math.Sin(y), Y = y }
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
