using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSim
{
    class Regions : IComparable<Regions>
    {
        // color of Pylon
        private PylonType type; // PylonType.LEFT or PylonType.RIGHT

        // details for pylon visible in camera image
        private int center;     // Distance of pylon center from center of camera view
                                //  Left of center is positive distance, right negative
        private int height;     // pixels high
        private int width;      // pixels wide

        // cheat values
        private double distance;
        private double angle;

        public Regions(double pyDistance, double pyAngle, PylonType pyType)
        {
            distance = pyDistance;
            angle = pyAngle;
            type = pyType;
        }

        public double Distance
        {
            get { return distance; }
        }
        
        public double Angle
        {
            get { return angle; }
        }

        public PylonType Type
        {
            get { return type; }
        }

        public int Center
        {
            get { return center; }
            set 
            {
                if (value == -1)
                    center = -1;
                else
                {
                    //change center to be dist from center of camera view
                    //  right of center is negative distance, left is positive
                    center = Control.cameraWidth / 2 - value;
                }
            }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public static bool operator >(Regions a, Regions b)
        {
            return a.distance > b.distance;
        }

        public static bool operator ==(Regions a, Regions b)
        {
            if (((a as object) == null || (b as object) == null) && 
                ((a as object) != null || (b as object) != null))
                return false;
            else if ((a as object) == null && (b as object) == null)
                return true;
            return a.distance == b.distance;
        }

        public static bool operator !=(Regions a, Regions b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            bool retValue = false;
            if (obj is Regions)
            {
                retValue = (obj as Regions).distance == distance;
            }
            return retValue;
        }

        public static bool operator <(Regions a, Regions b)
        {
            return a.distance < b.distance;
        }

        #region IComparable<Regions> Members

        int IComparable<Regions>.CompareTo(Regions other)
        {
            return (int)(distance - other.distance);
        }

        #endregion
    }
}
