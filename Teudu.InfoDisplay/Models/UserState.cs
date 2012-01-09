// -----------------------------------------------------------------------
// <copyright file="UserState.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Teudu.InfoDisplay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Configuration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class UserState
    {
        public HandsState hands;
        public Arm leftArm;
        public Arm rightArm;
        public Torso torso;

        private double HALF_ARMSPAN = 0.3;
        private double CORRESPONDENCE_SCALE_FACTOR_X = 4;
        private double CORRESPONDENCE_SCALE_FACTOR_Y = 6;
        private const double SCALE_OFFSET = 250;
        private double userMinDistance;
        private double invisibleScreenLocation;
        private bool inverted;
        private double inversionFactor;

        public UserState()
        {
            if (!Double.TryParse(ConfigurationManager.AppSettings["InvisibleScreenLocation"], out invisibleScreenLocation))
                invisibleScreenLocation = 1.3;
            if (!Double.TryParse(ConfigurationManager.AppSettings["MinUserDistance"], out userMinDistance))
                userMinDistance = 3.0;
            if (!Boolean.TryParse(ConfigurationManager.AppSettings["Inverted"], out inverted))
                inverted = false;
            if (!Double.TryParse(ConfigurationManager.AppSettings["CorrespondenceScaleX"], out CORRESPONDENCE_SCALE_FACTOR_X))
                CORRESPONDENCE_SCALE_FACTOR_X = 4;
            if (!Double.TryParse(ConfigurationManager.AppSettings["CorrespondenceScaleY"], out CORRESPONDENCE_SCALE_FACTOR_Y))
                CORRESPONDENCE_SCALE_FACTOR_Y = 6;

            if (inverted)
                inversionFactor = 1;
            else
                inversionFactor = -1;
        }

        public HandsState InteractionMode
        {
            get
            {
                HandsState currentState;
                if (LeftHandActive && RightHandActive)
                    currentState = HandsState.Zooming;
                else if ((LeftHandActive && !RightHandActive) || (!LeftHandActive && RightHandActive))
                    currentState = HandsState.Panning;
                else
                    currentState = HandsState.Resting;

                return currentState;
            }
        }

        public bool Touching 
        {
            get
            {
                return (LeftHandActive || RightHandActive) && !(LeftHandActive && RightHandActive) && !TooClose;
            }
        }

        public Arm DominantHand
        {
            get
            {
                if (LeftHandActive)
                    return leftArm;
                else
                    return rightArm;

            }
        }

        public double DominantArmHandOffsetX
        {
            get {return inversionFactor * DominantHand.HandOffsetX * CORRESPONDENCE_SCALE_FACTOR_X; }
        }

        public double DominantArmHandOffsetY
        {
            get { return inversionFactor * DominantHand.HandOffsetY * CORRESPONDENCE_SCALE_FACTOR_Y; }
        }

        public double HandsDistance
        {
            get
            {
                return Math.Sqrt(Math.Pow(this.leftArm.HandX - this.rightArm.HandX, 2) +
                    Math.Pow(this.leftArm.HandY - this.rightArm.HandY, 2)) / SCALE_OFFSET;
            }
        }

        public bool LeftHandActive
        {
            get { return LeftArmInFront; }
        }

        public bool LeftArmInFront
        {
            get { return leftArm.HandZ < invisibleScreenLocation; }
        }

        public bool RightHandActive
        {
            get { return RightArmInFront; }
        }

        public bool RightArmInFront
        {
            get { return rightArm.HandZ < invisibleScreenLocation; }
        }

        public bool TooClose
        {
            get
            {
                return torso.Z < (invisibleScreenLocation + HALF_ARMSPAN);
            }
        }

        public bool TorsoInRange
        {
            get
            {
                return torso.Z <= userMinDistance;
            }
        }

        public double DistanceFromInvisScreen
        {
            get
            {
                if (leftArm.HandZ < rightArm.HandZ)
                    return leftArm.HandZ - invisibleScreenLocation;
                else if (rightArm.HandZ <= leftArm.HandZ)
                    return rightArm.HandZ - invisibleScreenLocation;
                else
                    return 2;

            }
        }

        public double TorsoDistanceFromInvisScreen
        {
            get
            {
                return torso.Z - invisibleScreenLocation;
            }
        }

        public bool OutOfBounds
        {
            get
            {
                return Touching && (OutOfBoundsLeft || OutOfBoundsRight || OutOfBoundsTop || OutOfBoundsBottom);
            }
        }

        /// <summary>
        /// Returns true if user's hand is far right out of the viewing area
        /// </summary>
        public bool OutOfBoundsRight
        {
            get
            {
                return DominantHand.HandX >= 1910;
            }
        }

        /// <summary>
        /// Returns true if user's hand is far bottom out of the viewing area
        /// </summary>
        public bool OutOfBoundsBottom
        {
            get
            {
                return DominantHand.HandY <= 10;
            }
        }

        /// <summary>
        /// Returns true if user's hand is far top out of the viewing area
        /// </summary>
        public bool OutOfBoundsTop
        {
            get
            {
                return DominantHand.HandY >= 1080;
            }
        }

        /// <summary>
        /// Returns true if the user's hand is far left out of the viewing area
        /// </summary>
        public bool OutOfBoundsLeft
        {
            get
            {
                return DominantHand.HandX <= 10;
            }
        }
    }
}
