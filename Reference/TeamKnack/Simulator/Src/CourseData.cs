using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboSim
{
    class CourseData
    {
        //This angle represents the additional turning angle that is added when coming from,
        // or going to a different colored pylon
        //The calculation for the angle is sin-1(turnRadius/((Average distance between pylons)/2))
        public double ADDED_ANGLE = Math.Asin( 
            Control.TURN_RADIUS / ( ((Control.MAX_PYLON_DIST + Control.MIN_PYLON_DIST)/2) /2 ) 
            ) * 180 / Math.PI ; //degrees
        
        List<CoursePylon> coursePylons;
        int totalPylons;
        
        public CourseData(List<Pylon> pylons)
        {
            setCourseData(pylons);
            //updateAngles();
        }


        public void setCourseData(List<Pylon> pylons)
        {
            totalPylons = pylons.Count;

            //use and array of course pylons in C
            coursePylons = new List<CoursePylon>();

            for (int pylonIndex = 0; pylonIndex < pylons.Count; ++pylonIndex)
            {
                CoursePylon pylon = new CoursePylon(pylons[pylonIndex].Type, pylons[pylonIndex].AngleToNextPylon);
                coursePylons.Add(pylon);
            }

        }

        public void updateAngles(List<Pylon> pylons)
        {
            //this algorithm supposes the list is already populated
            // with all the pylons and their types are set
            for (int pylonIndex = 0; pylonIndex < pylons.Count; ++pylonIndex)
            {
                //start with base angle
                coursePylons[pylonIndex].AngleToNextPylon = pylons[pylonIndex].AngleToNextPylon;
                
                //if not same color as previous - add the extra angle (see def of ADDED_ANGLE)
                if (coursePylons[pylonIndex].Type != pylons[getPrevPylonNumber(pylonIndex)].Type)
                {
                    if (coursePylons[pylonIndex].AngleToNextPylon < 0)
                    {
                        coursePylons[pylonIndex].AngleToNextPylon = 
                            ( coursePylons[pylonIndex].AngleToNextPylon - ADDED_ANGLE ) % 360;
                    }
                    if (coursePylons[pylonIndex].AngleToNextPylon >= 0)
                    {
                        coursePylons[pylonIndex].AngleToNextPylon =
                            (coursePylons[pylonIndex].AngleToNextPylon + ADDED_ANGLE) % 360;
                    }
                }
                //if not same color as next - add the extra angle
                if (coursePylons[pylonIndex].Type != pylons[getNextPylonNumber(pylonIndex)].Type)
                {
                    if (coursePylons[pylonIndex].AngleToNextPylon < 0)
                    {
                        coursePylons[pylonIndex].AngleToNextPylon =
                            (coursePylons[pylonIndex].AngleToNextPylon - ADDED_ANGLE) % 360;
                    }
                    if (coursePylons[pylonIndex].AngleToNextPylon >= 0)
                    {
                        coursePylons[pylonIndex].AngleToNextPylon =
                            (coursePylons[pylonIndex].AngleToNextPylon + ADDED_ANGLE) % 360;
                    }
                }

                //prevent excessive turning - all the way around pylon
                if (Math.Abs(coursePylons[pylonIndex].AngleToNextPylon) > 360 - Control.ANGLE_OF_VIEW/4)
                    coursePylons[pylonIndex].AngleToNextPylon = 0;  

            }
        }

        public int TotalPylons
        {
            get { return totalPylons; }
        }


        public PylonType getType(int pylonNum)
        {
            return coursePylons[pylonNum].Type;
        }

        public double getAngleToNextPylon(int pylonNum)
        {
            return coursePylons[pylonNum].AngleToNextPylon;
        }
        public void setAngleToNextPylon(int pylonNum, double value)
        {
            coursePylons[pylonNum].AngleToNextPylon = value;
        }
        
        public double getDistanceToPylon(int pylonNum)
        {
            return coursePylons[pylonNum].DistanceToNextPylon;
        }
        public void setDistanceToPylon(int pylonNum, double value)
        {
            coursePylons[pylonNum].DistanceToNextPylon = value;
        }

        public double getDistanceToTravel(int pylonNum)
        {
            return coursePylons[pylonNum].DistanceToTravel;
        }
        public void setDistanceToTravel(int pylonNum, double value)
        {
            coursePylons[pylonNum].DistanceToTravel = value;
        }
        
        public double getDistanceToTurn(int pylonNum)
        {
            return coursePylons[pylonNum].DistanceToTurn;
        }
        public void setDistanceToTurn(int pylonNum, double value)
        {
            coursePylons[pylonNum].DistanceToTurn = value;
        }


        private int getNextPylonNumber(int curPylon)
        {
            int nextPylon = curPylon + 1;
            
            if (nextPylon >= totalPylons)   
                nextPylon = 0;

            return nextPylon;
        }
        private int getPrevPylonNumber(int curPylon)
        {
            int prevPylon = curPylon - 1;

            if (prevPylon < 0)
                prevPylon = totalPylons - 1;

            return prevPylon;
        }


    }
}
