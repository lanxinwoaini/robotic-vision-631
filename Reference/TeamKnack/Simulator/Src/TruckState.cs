using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSim
{
    enum ModeType { GATHER, CORRECT, BLIND };
    
    class TruckState
    {
        private ModeType mode;       //mode the car is in
        private int curPylon;   //pylon that truck is currently heading for
        private int totalPylons;    //total pylons in the course - used for wrap-around
        private double distTraveled;    //reset in BLIND mode for turn, GATHER for distToTravel
        private double angle;       //angle to the pylon (heading)
        private double distToPylon; //distance from current position to pylon
        private double distToBLIND; //distance left to travel until switch to BLIND mode


        public TruckState(int totPylons)
        {
            mode = ModeType.GATHER;
            curPylon = 0;
            totalPylons = totPylons;
            distTraveled = 0.0;
            angle = 0.0;
            distToPylon = 0.0;
            distToBLIND = 0.0 ;
        }

        public ModeType DriveMode
        {
            get { return mode; }
            set { mode = value; }
        }

        public int CurrentPylon
        {
            get { return curPylon; }
            set { curPylon = value; }
        }

        public void incrementPylon()
        {
            if (++curPylon >= totalPylons)
            {
                curPylon = 0;
            }
        }

        public double DistanceTraveled
        {
            get { return distTraveled; }
            set { distTraveled = value; }
        }

        public void resetDistanceTraveled()
        {
            distTraveled = 0;
        }

        public double AngleToPylon
        {
            get { return angle; }
            set { angle = value; }
        }
        
        public double DistanceToPylon
        {
            get { return distToPylon; }
            set { distToPylon = value; }
        }

        public double DistanceToBLIND
        {
            get { return distToBLIND; }
            set { distToBLIND = value; }
        }

        


    }
}
