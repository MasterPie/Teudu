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

    public enum HandsState
    {
        Panning, Zooming, Resting
    }
    /// <summary>
    /// Models the interaction state of the current user
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
                userMinDistance = 2.0;
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

        /// <summary>
        /// Returns the user's interaction mode. Currently, only Panning is supported by the application
        /// </summary>
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

        /// <summary>
        /// Returns true if the user is touching the invisible screen
        /// </summary>
        public bool Touching 
        {
            get
            {
                return (LeftHandActive || RightHandActive) && !(LeftHandActive && RightHandActive) && !TooClose;
            }
        }

        /// <summary>
        /// Returns the user's dominant (user's active) hand
        /// </summary>
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

        /// <summary>
        /// Returns the dominant (user's active) hand's X displacement from starting point
        /// </summary>
        public double DominantArmHandOffsetX
        {
            get {return inversionFactor * DominantHand.HandOffsetX * CORRESPONDENCE_SCALE_FACTOR_X; }
        }


        /// <summary>
        /// Returns the dominant (user's active) hand's Y displacement from starting point
        /// </summary>
        public double DominantArmHandOffsetY
        {
            get { return inversionFactor * DominantHand.HandOffsetY * CORRESPONDENCE_SCALE_FACTOR_Y; }
        }

        /// <summary>
        /// Returns the distance between the left and right hands
        /// </summary>
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

        /// <summary>
        /// Returns true if user's left hand is touching the invisible screen
        /// </summary>
        public bool LeftArmInFront
        {
            get { return leftArm.HandZ < invisibleScreenLocation; }
        }

        public bool RightHandActive
        {
            get { return RightArmInFront; }
        }

        /// <summary>
        /// Returns true if user's right hand is touching the invisible screen
        /// </summary>
        public bool RightArmInFront
        {
            get { return rightArm.HandZ < invisibleScreenLocation; }
        }

        /// <summary>
        /// Returns true if the user is too close to the display (and therefore cannot use the application)
        /// </summary>
        public bool TooClose
        {
            get
            {
                return torso.Z < (invisibleScreenLocation + HALF_ARMSPAN);
            }
        }

        /// <summary>
        /// Returns true if the user's body is in range for using the application
        /// </summary>
        public bool TorsoInRange
        {
            get
            {
                return torso.Z <= userMinDistance;
            }
        }

        /// <summary>
        /// Returns the user's hand's distance from the invisible screen
        /// </summary>
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

        /// <summary>
        /// Returns the user's full body distance from the invisible screen
        /// </summary>
        public double TorsoDistanceFromInvisScreen
        {
            get
            {
                return torso.Z - invisibleScreenLocation;
            }
        }

        /// <summary>
        /// Returns true if user's hand is out of the viewing area (from any side)
        /// </summary>
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
