using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;  // for colors

namespace RoboSim
{
    class Control
    {
        // Controller outputs
        public double steerAngle;   // degrees, positive for left turn, negative for right turn
        public double carSpeed;     // inches per second

        // Course
        private List<Pylon> pylons;
        // you can choose your own pylon colors
        // set pylonColor function below

        // Pylon Parameters
        public const double PYLON_HEIGHT = 28.5; // inches
        public const double PYLON_WIDTH = 2.0; // inches

        // Car Parameters
        public const double WHEEL_BASE = .296863;   //m   // 11.6875 inches +/- 0.125

        // Camera Frame Size and Parameters
        // the width, height, and center values in Regions will conform to the image size you set
        public const int cameraWidth = 320;   // pixel values 0 ... cameraWidth-1
        public const int cameraHeight = 240;  // pixel values 0 ... cameraHeight-1
        public const double ANGLE_OF_VIEW = 56.0;  // camera viewing angle in degrees
        //Vertical angle has not yet been determined - perhaps 240/320 * 56.0? 
        //  here chosen to be 56 because the camera view seems to be square
        public const double VERT_ANGLE_OF_VIEW = 58.0; //vertical viewing angle in degrees 

        // Controller state
        // Put your persistant state variable here, be sure to initialize them in the constructor
            //private double theta;
            //private double distance; //cheat value
            //private double distanceCalc; //calculated value
            //private PylonType turnDir;  //dead reckoning turning direction
            //private int driveStraightCount; //used to drive straight in dead reckoning for a time
            //private int turnCount; //dead reckoning turning
        private TruckState truckState;
        private CourseData courseData;
        //----- Variables for measuring distance to pylon --------------
        private double lastMeasuredHeight;    //used in measuring distance (2) change in height
        private double lastMeasuredAngle;   //used in measuring distance (3) change in angle
        private int frameCount;         //counts frames in between measurements of height/angle
        private const int frameCountDuration = 30; //number of frames to wait until checking data
        private double distSinceLastFrame;  //cumulates the distance between frame checks
        //-----------------------------------------------------------------

        // Controller parameters
        // for example...
        public const double STRAIGHT_SPEED = .8;    // m/s
        public const double TURN_SPEED = .5;        // m/s
        public const double TURN_RADIUS = .5;       //m (actual min at about 18inches) 
        public const double MIN_PYLON_DIST = 3;     //m
        public const double MAX_PYLON_DIST = 10;     //m
        public const double TURN_ANGLE = WHEEL_BASE / TURN_RADIUS * 180 / Math.PI;   // degrees


        //Debug
        private string debugtxt;
        private const int DEBUG = 1;    //0-off 1-on


    //---------------------------------------------------------------------------------
        // Control Constructor
        public Control(List<Pylon> pylons)
        {
            this.pylons = pylons;
            StateReset();
            
        }

        // Reset Controller
        // Called when the "Reset the Car" button is pressed
        public void StateReset()
        {
            debugtxt = "";
            
            setVelocity(0.0);
            RCsetSteering(0);    

            // Reset your controller state variables
            //setCourseData(); - now called in genCourseDesc() in frmMain
            courseData = new CourseData(pylons);
            truckState = new TruckState(pylons.Count);
            resetDistanceVariables();
        }

        // choose the colors displayed for pylons
        public static System.Drawing.Brush pylonColor(PylonType t)
        {
            return t == PylonType.LEFT ? Brushes.Green : Brushes.Orange;
        }

        // Methods to get Internal State
        // The methods are called to post state in the display
        public int getCurrentPylon()
        {
            return truckState.CurrentPylon + 1;
        }

        public double getCurrentDistance()//distance remaining
        {
            if (truckState.DriveMode == ModeType.GATHER)
            {
                return truckState.DistanceToPylon;
            }
            else if (truckState.DriveMode == ModeType.CORRECT)
            {
                return truckState.DistanceToBLIND;
            }
            else //BLIND
            {
                return courseData.getDistanceToTurn(truckState.CurrentPylon) - truckState.DistanceTraveled;
            }
        }

        public double getCurrentTheta()
        {
            return truckState.AngleToPylon;
        }

        public string getCurrentState()
        {
            switch (truckState.DriveMode)
            {
                case ModeType.BLIND:
                        return "BLIND";
                case ModeType.CORRECT:
                        return "CORRECT";
                case ModeType.GATHER:
                        return "GATHER";
                default:
                    return "Car State";
            }
        }

        public string getDebugOutput()
        {
            string output = debugtxt;
            debugtxt = "";
            return output;
        }

        // Control Method
        // Called when car is running to control where the car goes
        public void computeControl(double deltaDistance, List<Regions> visiblePylons)
        {
            //this function will be what is contained in the while loop inside runCourse()
            //Any variables external to this function can be defined inside runCourse(), but 
            //outside the while loop

            truckState.DistanceTraveled += inchesToMeters(deltaDistance);
            
            switch(truckState.DriveMode)
            {
                case ModeType.GATHER: //Coming from D.R. looking for new image info 
                    {
                        //see if there are visible pylons in the array
                        //  if yes  - find closest, check color and lock on
                        //  if not  - keep turning until we do
                        if (visiblePylons.Count > 0)
                        {
                            int pylonIndex = findTargetPylon(visiblePylons);

                            //if (didn't find one that was the right color)
                            if (pylonIndex == -1)
                                break;

                            //otherwise - found the pylon we're looking for
                            //  move to CORRECT mode
                            //  later add a check for several frames of same information
                            truckState.DistanceToPylon = getDistanceToPylon(visiblePylons[pylonIndex], deltaDistance);
                            courseData.setDistanceToTravel(truckState.CurrentPylon, getDistanceToTravel(visiblePylons[pylonIndex], deltaDistance));
                            truckState.DistanceToBLIND = Control.TURN_RADIUS / Math.Sin(Control.ANGLE_OF_VIEW / 2 * Math.PI / 180);
                            //truckState.DistanceToBLIND = courseData.getDistanceToTravel(truckState.CurrentPylon);
                            truckState.DriveMode = ModeType.CORRECT;
                            truckState.AngleToPylon = getAngleToPylon(visiblePylons[pylonIndex]);
                            setVelocity(STRAIGHT_SPEED);
                            RCsetSteering(0);
                        }
                        else
                        {
                            resetDistanceVariables();
                            
                        }
                        
                        break;
                    }
                case ModeType.CORRECT: //Using image info to continually correct course
                    {
                        truckState.DistanceToBLIND -= inchesToMeters(deltaDistance);

                        if (truckState.DistanceToBLIND <= 0)
                        {
                            //Change to BLIND mode
                            //  1 - Change modes
                            //  2 - Adjust steering and velocity
                            //  3 - Reset distTraveled
                            
                            truckState.DriveMode = ModeType.BLIND;
                            //set steering, velocity for next mode
                            setVelocity(TURN_SPEED);
                            if (courseData.getType(truckState.CurrentPylon) == PylonType.LEFT)
                            {
                                RCsetSteering(-TURN_ANGLE);
                            }
                            else
                            {
                                RCsetSteering(TURN_ANGLE);
                            }
                            truckState.resetDistanceTraveled();
                        }//end if(change to blind mode)

                        //if the angleToPylon is greater than this it will be off screen soon
                        //  so we don't need to check for pylons in view
                        //  
                        if ( truckState.AngleToPylon > (ANGLE_OF_VIEW / 2) )
                            break;

                        if (visiblePylons.Count > 0)
                        {
                            int pylonIndex = findTargetPylon(visiblePylons);

                            //if (found one that was the right color)
                            if (pylonIndex != -1)
                            {
                                Regions pylon = visiblePylons[pylonIndex];
                                truckState.DistanceToPylon = getDistanceToPylon(pylon, deltaDistance);
                                truckState.AngleToPylon = getAngleToPylon(pylon);
                                truckState.DistanceToBLIND = getDistanceToTravel(pylon, deltaDistance);

                                //Check heading error and set steering
                                RCsetSteering(getHeadingError());
                            }
                        }//end of if(visble pylons)
                        else
                        {
                            resetDistanceVariables();
                        }

                        break;  //break from switch
                    }
                case ModeType.BLIND: //totally on dead reckoning
                    {
                        //Steering and velocity will be set in transition to this mode
                        //  this way we don't have to reset it every time as it will be constant
                        //The distanceTraveled will also have been reset before enter the first time

                        //if (DEBUG == 1)
                        //{
                        //    debugtxt += "BLIND on pylon: " + truckState.CurrentPylon.ToString() + "\r\n"
                        //            + "angle to turn: " + courseData.getAngleToNextPylon(truckState.CurrentPylon).ToString()
                        //            + "\r\ndistToTurn: " + courseData.getDistanceToTurn(truckState.CurrentPylon).ToString("000.00")
                        //            + "\r\ndistTraveled: " + truckState.DistanceTraveled.ToString("000.00")
                        //            + "\r\n";
                        //}
                        
                        //measure how far to turn
                        if (truckState.DistanceTraveled >= courseData.getDistanceToTurn(truckState.CurrentPylon))
                        {
                            //transition to GATHER mode
                            //  1 - reset mode
                            //  2 - move to next pylon
                            //  3 - reset odometer
                            //  4 - speed, steering stay same incase we can't see a pylon yet
                            truckState.DriveMode = ModeType.GATHER;
                            truckState.incrementPylon();
                            truckState.resetDistanceTraveled();
                        }

                        break;
                    }
                default:
                    {
                        //if we ever get here we should probably stop
                        setVelocity(0);
                        RCsetSteering(0);
                        break;
                    }
            }//end switch


        }

        private void RCsetSteering(double angle)
        {
            steerAngle = angle;
        }

        private void setVelocity(double msSpeed)
        {
            //speed is in m/s right now, but simulator runs in in/sec
            double velocity = metersToInches(msSpeed);
            
            carSpeed = velocity;
        }


        public double inchesToMeters(double inches)
        {
            double meters = inches * .0254;

            return meters;
        }

        public double metersToInches(double meters)
        {
            double inches = meters / .0254;

            return inches;
        }

        public void updateAngles(List<Pylon> pylons)
        {
            //if(DEBUG == 1)
            //    debugtxt += "Called updateAngles\r\n";

            courseData.updateAngles(pylons);
        }

        //deltaDistance is passed in in inches
        private double getDistanceToPylon(Regions pylon, double deltaDistance)
        {

            // 1) Height of pylon
            //-----------------------------------------------
            double dist1 = ( inchesToMeters(PYLON_HEIGHT) * cameraHeight )
                            / (VERT_ANGLE_OF_VIEW * Math.PI / 180 * pylon.Height );
            //-----------------------------------------------

            // 2) Change in Height
            //  must wait several frames to make any measurements
            //************************************************
            double dist2 = -1;
            //wait four frames to recalculate
            if (frameCount > frameCountDuration - 1)
            {
                if (lastMeasuredHeight != 0)
                {
                    dist2 = inchesToMeters(deltaDistance) * pylon.Height / (pylon.Height - lastMeasuredHeight);
                }
                lastMeasuredHeight = pylon.Height;
                //frame count bookkeeping below in method 3
            }
            //************************************************


            // 3) Change in Angle
            //-----------------------------------------------
                // d = - deltaDistance * sin (angle1) / ( sin(angle1) - sin(angle2) )
            double dist3 = -1;
            if (frameCount > frameCountDuration - 1)
            {
                if (lastMeasuredAngle != 0)
                {
                    dist3 = -1 * inchesToMeters(distSinceLastFrame) * Math.Sin(lastMeasuredAngle * Math.PI / 180)
                        / (Math.Sin(lastMeasuredAngle * Math.PI / 180) - Math.Sin(pylon.Angle * Math.PI / 180));
                }
                lastMeasuredAngle = pylon.Angle;
                frameCount = 0;
                distSinceLastFrame = 0;
            }
            else
            {
                frameCount++;
                distSinceLastFrame += deltaDistance;
            }

            //-----------------------------------------------


            // 4) Width
            //************************************************
            double dist4 = (inchesToMeters(PYLON_WIDTH) * cameraWidth)
                            / (ANGLE_OF_VIEW * Math.PI / 180 * pylon.Width);
            //************************************************


            if (DEBUG == 1 && frameCount == 0)//only report when all have values
            {
                debugtxt += "---------Calculated Distances------------- "
                            + "\r\nDistance1: " + dist1.ToString("00.00")
                            + "\t\tDistance2: " + dist2.ToString("00.00")
                            + "\r\nDistance3: " + dist3.ToString("00.00")
                            + "\t\tDistance4: " + dist4.ToString("00.00")
                            + "\r\nCheat Disatance: " + inchesToMeters(pylon.Distance).ToString("00.00") 
                            + "\r\nHeight: " + pylon.Height.ToString("00.00")
                            + "\t\tAngle: " + pylon.Angle.ToString("00.00")
                            + "\r\nWidth: " + pylon.Width.ToString("00.0")
                            + "\r\n----------------------------------------------------------"
                            + "\r\n";
            }

            return dist1;
            //return inchesToMeters(pylon.Distance);
        }

        private void resetDistanceVariables()
        {
            //reset these variables to zero when vision returns no pylons
            //  so distance measure won't use wrong values
            lastMeasuredHeight = 0;
            lastMeasuredAngle = 0;
            frameCount = frameCountDuration;//intialized to Duration so that it will be time to look at a frame
            distSinceLastFrame = 0;
        }


        private double getAngleToPylon(Regions pylon)
        {
            //method 1
            //double angle = pylon.Center;
            //angle = angle / (cameraWidth / 2);
            //angle = Math.Sin(ANGLE_OF_VIEW / 2 * Math.PI / 180) * angle;
            //angle = Math.Asin(angle);
            //angle = angle * 180 / Math.PI;

            //method 2  - much less computationally demanding and just as accurate
            double angle = ANGLE_OF_VIEW / 2 * pylon.Center / (Control.cameraWidth / 2);
            
            //if (DEBUG == 1)
            //{
            //    debugtxt += "Calculated Angle: " + angle.ToString("00.00")
            //                + "\r\nCheat Angle: " + pylon.Angle.ToString("00.00") + "\r\n";
            //}

            return angle;
        }

        private double getHeadingError()
        {
            double r = TURN_RADIUS;
            double rSQ = r*r;
            double d = truckState.DistanceToPylon;
            double dSQ = d*d;
            double sinAngle = Math.Sin(truckState.AngleToPylon * Math.PI / 180);
            double error = 0;

            //this is an approximation, the result should represent sin(error)
            if (courseData.getType(truckState.CurrentPylon) == PylonType.LEFT) //head left
                error = (d * sinAngle + r) / Math.Sqrt(dSQ - rSQ);
            else
                error = (d * sinAngle - r) / Math.Sqrt(dSQ - rSQ);  //head right
            
            //convert to degrees
            error = 180 / Math.PI * error;

            return error;
        }

        private double getDistanceToTravel(Regions pylon, double deltaDistance)
        {
            double distToPylon = getDistanceToPylon(pylon, deltaDistance);

            return distToPylon * Math.Cos(getAngleToPylon(pylon) * Math.PI / 180);
        }

        //finds the pylon among the visible pylons that is 
        //  1) the right color (type)
        //  2) closest to the center of the field of view
        //  3) closest to the truck
        //returns the index of the pylon in visiblePylons
        //  or -1 if no pylon found of the right color
        private int findTargetPylon(List<Regions> visiblePylons)
        {
            int pylonIndex = 0;
            PylonType curType = courseData.getType(truckState.CurrentPylon);
            //int closestToCenter = -1;  
            //int closestToTruck = -1;
            int targetPylon = -1;

            //Pylon[0] should be closest pylon  -- this will not be true on the truck
            //find closest pylon of right color
            for (pylonIndex = 0; pylonIndex < visiblePylons.Count; pylonIndex++)
            {
                //check color
                if (visiblePylons[pylonIndex].Type == curType)
                {
                    //see if first found pylon
                    if (targetPylon < 0)
                    {
                        targetPylon = pylonIndex;    //will be closest b/c of array order
                    }
                    else//else do comparison
                    {
                        //need to check if there is more than one pylon of the right color within
                        //  a certain range of view (here selected to be ANGLE_OF_VIEW/2)
                        //  if so, take closest one.
                        if (Math.Abs(visiblePylons[pylonIndex].Center)
                                < Math.Abs(visiblePylons[targetPylon].Center) && truckState.DistanceToBLIND > 2)
                        {
                            targetPylon = pylonIndex;
                        }
                    }
                }
            }


            return targetPylon;
        }

    }
}


