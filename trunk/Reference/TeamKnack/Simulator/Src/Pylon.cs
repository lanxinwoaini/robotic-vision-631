using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RoboSim
{
    enum PylonType { LEFT, RIGHT };
    class Pylon
    {
        private PylonType direction;
        private PointF coord;
        private double angle;
        private double distance;


        public Pylon(PointF point, PylonType type)
        {
            coord = point;
            direction = type;
            angle = 0.0;
            distance = 0.0;
        }

        public void AngleDistanceToNext(double angleToNext, double distanceToNext)
        {
            angle = angleToNext;
            distance = distanceToNext;
        }

        public PointF Coordinates
        {
            get { return coord; }
        }

        public PylonType Type
        {
            get { return direction; }
        }

        public double AngleToNextPylon
        {
            get { return angle; }
            set { angle = value; }
        }

        public double DistanceToNextPylon
        {
            get { return distance; }
            set { distance = value; }
        }

    }
}
