using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSim
{
    
    class CoursePylon
    {
        private PylonType direction;
        private double angle;
        private double distToPylon;
        private double distToTravel;
        private double distToTurn;


        public CoursePylon(PylonType type, double pylonAngle)
        {
            direction = type;
            angle = pylonAngle;
            distToPylon = 0.0;
            distToTravel = 0.0;
            calcDistToTurn();
        }

        private void calcDistToTurn()
        {
            if(direction == PylonType.RIGHT) 
                distToTurn = angle * Math.PI / 180 * Control.TURN_RADIUS;  //in m
            else
                distToTurn = -angle * Math.PI / 180 * Control.TURN_RADIUS;
            
            //make distance shorter so we'll start looking earlier
            //subtract 10%
            //distToTurn = distToTurn - distToTurn * .1;

        }

        public PylonType Type
        {
            get { return direction; }
        }

        public double AngleToNextPylon
        {
            get { return angle; }
            set 
            { 
                angle = value;
                calcDistToTurn();
            }
        }

        public double DistanceToNextPylon
        {
            get { return distToPylon; }
            set { distToPylon = value; }
        }

        public double DistanceToTravel
        {
            get { return distToTravel; }
            set { distToTravel = value; }
        }

        public double DistanceToTurn
        {
            get { return distToTurn; }
            set { distToTurn = value; }
        }


    }
    
}
