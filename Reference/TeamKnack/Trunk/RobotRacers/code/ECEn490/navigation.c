/******************************************************************************

FILE:    navigation.c
AUTHOR:  Kevin Ellsworth
CREATED: 12 Feb 2009

UPDATED: 06 Mar 2009

DESCRIPTION
This file is the main file for the navigation algorithm


******************************************************************************/

/* Includes ----------------------------------------------------------------------*/
#include "navigation.h"
#include "MemAlloc.h"
#include "ServoControl.h"
#include "TX.h"
#include "State.h"
#include "pidControl.h"
#include "math.h"
#include "FrameTable.h"
#include "vision.h"
#include "InterruptControl.h"
#include "Timer.h"


// Controller parameters
float STRAIGHT_SPEED =			2.0f;	// m/s
float BLIND_STRAIGHT_SPEED =	1.75f;	//m/s
float TURN_SPEED	=			1.5f;	// m/s
float TURN_RADIUS		=		0.55f;	//m (actual min at about 18inches) 
int FIND_RANGE			=		250;	//The range of pixels used to look for a pylon in findTargetPylon()
int TURN_ANGLE			=		55;
static int NUM_PYLON_WEIGHTS = 12;
int unitLength	 = 3.0;


/* Global Memory ------------------------------------------------------------*/
	
	//Flags
	static uint8 GUIstate = NAV_DEBUG;

	//Computation constants (that have to be calculated)
	
	static float STRAIGHT_BLIND_DIST = 0;	//Distance to travel straight after losing vision of pylon
	static float ADDED_ANGLE = 0;			//This angle represents the additional turning angle that is 
									//	added when coming from, or going to a different colored pylon
									//The calculation for the angle is 
									//		sin-1(turnRadius/((Average distance between pylons)/2))
	static float findCenter = 0;			//Represents the angle of the center of the field of view to look for a pylon

	//Course variables - populated by GUI
	int32* courseAngles;
	int32* courseAngleTrims;
	int32* courseDistances;
	int numAngles = 0;
	int absoluteAngle = 0;
	int framesPerSecond_SW = 0;

	static FindPylonWeight* findPylonWeightsGreen;
	static FindPylonWeight* findPylonWeightsOrange;
	static FindPylonWeight* findPylonWeightsTracking;

	// Controller state
	TruckState truckState;							//Keeps track of the state of the truck
	static CoursePylon* coursePylons;			//The individual pylons in the course
	extern visiblePylon* visiblePylons;	//Array for the pylons from vision - continually updated
	extern int numVisiblePylons;
	extern FrameTableEntry* checkedOutFrameForNav;

	float deltaDistance = 0.0f;

	static int gathered_frames = 0;
	static int frames_missed_in_gather = 0;

//----- Variables for measuring distance to pylon --------------
//float lastMeasuredHeight;		//used in measuring distance (2) change in height
//float lastMeasuredAngle;		//used in measuring distance (3) change in angle
//int frameCount;				//counts frames in between measurements of height/angle
//#define frameCountDuration = 30;		//number of frames to wait until checking data
//float distSinceLastFrame;		//cumulates the distance between frame checks


/* Code ---------------------------------------------------------------------*/

//initialization of all variables
void navigationMemInit()
{
	findPylonWeightsGreen = (FindPylonWeight*) MemCalloc(sizeof(FindPylonWeight)*NUM_PYLON_WEIGHTS);
	findPylonWeightsOrange = (FindPylonWeight*) MemCalloc(sizeof(FindPylonWeight)*NUM_PYLON_WEIGHTS);
	findPylonWeightsTracking = (FindPylonWeight*) MemCalloc(sizeof(FindPylonWeight)*NUM_PYLON_WEIGHTS);
	coursePylons = (CoursePylon*) MemCalloc(sizeof(CoursePylon)*MAX_PYLONS);

	courseAngles = (int32*) MemCalloc(MAX_PYLONS);
	courseAngleTrims = (int32*) MemCalloc(MAX_PYLONS);
	courseDistances = (int32*) MemCalloc(MAX_PYLONS);

	initializePylonWeights();
	navigationInit();
}
void navigationInit()
{
	setVelocity(0);
	setSteeringAngle(0);
	
	//------computatational variables---------
	// An approximation, for more accuracy use asin() of the ratio, weighted towards the minimum
	ADDED_ANGLE = TURN_RADIUS / ( ((MAX_PYLON_DIST + 2*MIN_PYLON_DIST)/3.0) /2.0 ) 
					 * 180.0 / PI ; //degrees
	//TURN_ANGLE = (int)(2*WHEEL_BASE / TURN_RADIUS * 180.0 / PI);   // degrees
	//TURN_ANGLE	=	54;
	STRAIGHT_BLIND_DIST = TURN_RADIUS / tan(ANGLE_OF_VIEW /2 * PI / 180);	
	STRAIGHT_BLIND_DIST += CAMERA_FRONT_DISPLACEMENT;  //add the length of the car from the camera to the back wheels.

	if (messagesOn() && GUIstate == NAV_DEBUG){
		SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,25,"Navigation Initialization");
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,13,"ADDED_ANGLE: ");
		sendInt(ADDED_ANGLE, DISPLAY_IN_GUI_NEWLINE);
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,21,"STRAIGHT_BLIND_DIST: ");
		sendInt(STRAIGHT_BLIND_DIST*100.0, DISPLAY_IN_GUI_NEWLINE);
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,13,"TURN_RADIUS: ");
		sendInt(TURN_RADIUS*100.0, DISPLAY_IN_GUI_NEWLINE);
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,12,"TURN_ANGLE: ");
		sendInt(TURN_ANGLE, DISPLAY_IN_GUI_NEWLINE);
	}

	//This value is reinitialized in setCourseData each time a course is sent
	findCenter = 0;	
	
	//------truckState initialization----------
	truckState.totalPylons		= 0;
	truckState.mode				= GATHER;
	truckState.currentPylon		= -1;	//this gets incremented in the first call to transitionToGATHER, so start at 0
	truckState.angleToPylon		= 0;
	truckState.distToPylon		= 0;
	truckState.distTraveled		= 0;

	framesPerSecond_SW = 0;
}

void initializePylonWeights(){
	int offset = 225;
	const int minX = -1;
	const int maxX = 641;
	
	int center;

	///
	//GREEN
	center=offset;
	findPylonWeightsGreen[0].x=minX;
	findPylonWeightsGreen[0].y=1;//default to zero
	findPylonWeightsGreen[1].x=center - 180;
	findPylonWeightsGreen[1].y=6;
	findPylonWeightsGreen[2].x=center - 160;
	findPylonWeightsGreen[2].y=25;
	findPylonWeightsGreen[3].x=center - 100;
	findPylonWeightsGreen[3].y=65;
	findPylonWeightsGreen[4].x=center - 30;
	findPylonWeightsGreen[4].y=100;
	findPylonWeightsGreen[5].x=center + 30;
	findPylonWeightsGreen[5].y=100;
	findPylonWeightsGreen[6].x=center + 100;
	findPylonWeightsGreen[6].y=65;
	findPylonWeightsGreen[7].x=center + 160;
	findPylonWeightsGreen[7].y=25;
	findPylonWeightsGreen[8].x=center + 180;
	findPylonWeightsGreen[8].y=6;
	findPylonWeightsGreen[9].x=maxX;
	findPylonWeightsGreen[9].y=1;//default to zero
	findPylonWeightsGreen[10].x=maxX;
	findPylonWeightsGreen[10].y=1;//default to zero
	findPylonWeightsGreen[11].x=maxX;
	findPylonWeightsGreen[11].y=1;//default to zero
	calculateMBValues(findPylonWeightsGreen);

	///
	//Orange
	center=640-offset;
	findPylonWeightsOrange[0].x=minX;
	findPylonWeightsOrange[0].y=1;//default to zero
	findPylonWeightsOrange[1].x=center - 180;
	findPylonWeightsOrange[1].y=6;
	findPylonWeightsOrange[2].x=center - 160;
	findPylonWeightsOrange[2].y=25;
	findPylonWeightsOrange[3].x=center - 100;
	findPylonWeightsOrange[3].y=65;
	findPylonWeightsOrange[4].x=center - 30;
	findPylonWeightsOrange[4].y=100;
	findPylonWeightsOrange[5].x=center + 30;
	findPylonWeightsOrange[5].y=100;
	findPylonWeightsOrange[6].x=center + 100;
	findPylonWeightsOrange[6].y=65;
	findPylonWeightsOrange[7].x=center + 160;
	findPylonWeightsOrange[7].y=25;
	findPylonWeightsOrange[8].x=center + 180;
	findPylonWeightsOrange[8].y=6;
	findPylonWeightsOrange[9].x=maxX;
	findPylonWeightsOrange[9].y=1;//default to zero
	findPylonWeightsOrange[10].x=maxX;
	findPylonWeightsOrange[10].y=1;//default to zero
	findPylonWeightsOrange[11].x=maxX;
	findPylonWeightsOrange[11].y=1;//default to zero
	calculateMBValues(findPylonWeightsOrange);

	///
	//Tracking  --skinny and tall
	findPylonWeightsTracking[0].x=-700; //beyond the screen
	findPylonWeightsTracking[0].y=1; //beyond the screen
	findPylonWeightsTracking[1].x=-300;
	findPylonWeightsTracking[1].y=4;
	findPylonWeightsTracking[2].x=-200;
	findPylonWeightsTracking[2].y=45;
	findPylonWeightsTracking[3].x=-150;
	findPylonWeightsTracking[3].y=75;
	findPylonWeightsTracking[4].x=-100;
	findPylonWeightsTracking[4].y=100;
	findPylonWeightsTracking[5].x=100;
	findPylonWeightsTracking[5].y=100;
	findPylonWeightsTracking[6].x=150;
	findPylonWeightsTracking[6].y=75;
	findPylonWeightsTracking[7].x=200;
	findPylonWeightsTracking[7].y=45;
	findPylonWeightsTracking[8].x=300;
	findPylonWeightsTracking[8].y=4;
	findPylonWeightsTracking[9].x=700; //beyond the screen
	findPylonWeightsTracking[9].y=1; //beyond the screen
	findPylonWeightsTracking[10].x=700; //beyond the screen
	findPylonWeightsTracking[10].y=1; //beyond the screen
	findPylonWeightsTracking[11].x=700; //beyond the screen
	findPylonWeightsTracking[11].y=1; //beyond the screen
	calculateMBValues(findPylonWeightsTracking);
}

//for y=mx+b, given a series of (x,y), this calculates the appropriate m and b values
void  calculateMBValues(FindPylonWeight* pWeight){
	int i;
	for (i = 0; i < NUM_PYLON_WEIGHTS-1; i++){
		pWeight[i].m = ((float) pWeight[i].y - pWeight[i+1].y) / ((float) pWeight[i].x - pWeight[i+1].x);
		pWeight[i].b = ((float) pWeight[i].y) - pWeight[i].m * ((float) pWeight[i].x);
	}
}

float getFindPylonsWeight(FindPylonWeight* pWeight, int col){
	int i;
	for (i = 0; i < NUM_PYLON_WEIGHTS-1; i++){
		if (col <= pWeight[i+1].x){
			return pWeight[i].m * (float)col + pWeight[i].b;
		}
	}
	return 0.0;
}

void navStateReset()
{
	navigationInit();
}

void NAV_Process()
{
	static uint32 lastFrameProcessed = 0;
	static uint32 lastClockTicks = 0;
	static uint32 numFramesSinceLastTransfer = 0;

	FrameTableEntry* unprocessed = FT_CheckOutFrame(FTE_OWNER_NAVIGATION);			//check out the frame
	if (unprocessed == NULL) return;//it's already been processed
	uint32 currentFrameNum = unprocessed->frameCount;
	if (currentFrameNum <= lastFrameProcessed){
		FT_CheckInFrame(unprocessed,FTE_OWNER_NAVIGATION);
		if (currentFrameNum < lastFrameProcessed-NUM_FRAME_TABLE_ENTRIES) lastFrameProcessed = currentFrameNum;
		return; //After updating lastFrameProcessed (to be updated to ignore uin32 wrap-around) check if this is newer
				//in the case that we have exceeded uint32, this will be false 1x because we update lastFrameProcessed
	}
	lastFrameProcessed = currentFrameNum;
	
	uint32 nowTicks = ClockTime();
	framesPerSecond_SW = XPAR_CPU_PPC405_CORE_CLOCK_FREQ_HZ / (nowTicks - lastClockTicks);
	lastClockTicks = nowTicks;
	//**************Vision post processing******************************
	//unprocessed->maskFrameGreen += 64;		// To adjust for the current -1 byte offset in memory
	//unprocessed->maskFrameOrange += 64;		// (Image is shifted by -1 bytes currently)

	setColumnCount(unprocessed->maskFrameOrange, unprocessed->colCountOrangeData);
	setColumnCount(unprocessed->maskFrameGreen, unprocessed->colCountGreenData);
	
	VIS_PopulateVisiblePylons(unprocessed->maskFrameGreen,unprocessed->colCountGreenData,GREEN_PYLON,
		unprocessed->visiblePylonArray,&unprocessed->numVisiblePylons);
	VIS_PopulateVisiblePylons(unprocessed->maskFrameOrange,unprocessed->colCountOrangeData,ORANGE_PYLON,
		unprocessed->visiblePylonArray,&unprocessed->numVisiblePylons);

	// ******************END vision post processing*********************
	
	//Navigation Code 
	GUIstate = NAV_DEBUG;
	if(session() == SN_DEBUG && running())	
	{
		GUIstate = NAV_DEBUG;
		runCourse(unprocessed->visiblePylonArray, unprocessed->numVisiblePylons);
	}
	else if(session() == SN_RACE && running())	
	{
		GUIstate = NAV_RACE;
		runCourse(unprocessed->visiblePylonArray, unprocessed->numVisiblePylons);
	}
	else if(session() == SN_JOYSTICK && running())	
	{
		GUIstate = NAV_JOYSTICK;
		runCourse(unprocessed->visiblePylonArray, unprocessed->numVisiblePylons);
	}
	
	numFramesSinceLastTransfer++;
	if(sendingVideo() && numFramesSinceLastTransfer >= (unprocessed->numVisiblePylons/3 < 5 ? 5 : unprocessed->numVisiblePylons/3)){ //send less frequently with more pylons
		//decrease frequency with more pylons to avoid buildup in the TX_FIFO which adds a large lag between the data and gui
		transmitPylonData(unprocessed->visiblePylonArray, unprocessed->numVisiblePylons);
		numFramesSinceLastTransfer=0;
	}

	FT_CheckInFrame(unprocessed,FTE_OWNER_NAVIGATION);

}


int getAdjustedAngle(int angle, float turnSpeed){
	int adjAngle = angle;

    if (angle > 0) // Green pylon
    {
        if (turnSpeed < 1.8)
        {
            adjAngle = (int)(angle * 0.97);
        }
        else if (turnSpeed < 2.1)
        {
            adjAngle = (int)(angle * 0.94);
        }
        else
        {
            adjAngle = (int)(angle * 0.90);
        }
    }
    else  // Orange pylon
    {
        if (turnSpeed < 1.8)
        {
            adjAngle = (int)(angle * 1.03);
        }
        else if (turnSpeed < 2.1)
        {
            adjAngle = (int)(angle * 1.08);
        }
        else
        {
            adjAngle = (int)(angle * 1.09);
        }
    }
	return adjAngle;
}

uint8 setCourseData()
{
	//When this is called the three values for course data should be populated
	uint8 pylonIndex;

	truckState.totalPylons = numAngles;
	
	for(pylonIndex = 0; pylonIndex < numAngles; pylonIndex++)
	{
		//set color
		if(courseAngles[pylonIndex] < 0)
		{
			coursePylons[pylonIndex].color = LEFT;
		}
		else
		{
			coursePylons[pylonIndex].color = RIGHT;
		}

		//set angle
		coursePylons[pylonIndex].angleToPylon = getAdjustedAngle(courseAngles[pylonIndex] + courseAngleTrims[pylonIndex], TURN_SPEED);
		coursePylons[pylonIndex].originalAngle = courseAngles[pylonIndex];
	}

	//update angles based on prev, next pylon, calc dist to turn, set compass
	updateAngles();
	calculateDistToTurn();
	setCompassAngles();

	//initialize for Gather mode
	//truckState.currentPylon should be set to -1 so that the ++ in this funtion sets it to zero
	transitionToGATHER();
	//initialize findCenter for the color of the first pylon
	//if(coursePylons[0].color == LEFT)	//ORANGE	
	//{
	//	findCenter = CAMERA_WIDTH/2;
	//}
	//else	//GREEN
	//{
	//	findCenter = CAMERA_WIDTH/2 - FIND_RANGE;
	//}

	return 0;	//No error
}

// called repeatedly in while loop of main
void runCourse(visiblePylon * visiblePylonArray, uint8 numPylons)
{
	
	//check to make sure there is course data
	if(truckState.totalPylons == 0)
	{
		SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,14,"No Course Data");

		return;
	}

	visiblePylons = visiblePylonArray;
	numVisiblePylons = numPylons;
	
	//update odometer
	deltaDistance = getDeltaDistance();	//will use this in other places
	truckState.distTraveled += deltaDistance;

	switch(truckState.mode)
	{
		case GATHER:		//transfered from BLIND_TURN, look for pylon
			{
				if (messagesOn() && GUIstate == NAV_DEBUG) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,2,"GA");
				//Values set here for first time called
				if (GUIstate != NAV_JOYSTICK){
					setVelocity(STRAIGHT_SPEED);
					setSteeringAngle(0);
				}
				
				
				if(NAV_NO_VISION)
				{
					transitionToCORRECT();
					break;
				}

				//See that there are pylons in the array
				//	if yes - find the target pylon and set pylon information from vision input
				//	if no - keep turning (already set from BLIND_TURN mode
				if (numVisiblePylons > 0){
					if(courseDistances[truckState.currentPylon] > 1){	// Only if there is a longer unit distance
						if(truckState.distTraveled < (float)unitLength / 3.0 * courseDistances[truckState.currentPylon]){
							break;	// This is to allow the truck to get close enough to the pylon to see it clearly
						}
					}
					int targetPylon = findTargetPylon_weighted();
					if (messagesOn() && GUIstate == NAV_DEBUG){
						SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,5,"  TP:");
						if (targetPylon>=0){
							sendInt(visiblePylons[targetPylon].center, DISPLAY_IN_GUI_NEWLINE);
						} else
						{
							sendInt(targetPylon, DISPLAY_IN_GUI_NEWLINE);
						}
					}
					//if didn't find a pylon of the right color, break
					if(targetPylon == -1)
					{
						frames_missed_in_gather++;
						break;
					}
									
					gathered_frames++;	
					
					updateTruckState(targetPylon,deltaDistance);
					if(gathered_frames > FRAMES_IN_GATHER)
					{
						transitionToCORRECT();
					}
				}
				else
				{
					frames_missed_in_gather++;
				}
				
				break;
			}
		case CORRECT:
			{
				
				if (messagesOn() && GUIstate == NAV_DEBUG) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,2,"CO");
				truckState.distToBLIND -= deltaDistance;

				if(NAV_NO_VISION)
				{
					transitionToBLIND_STRAIGHT();
					break;
				}
				
               if (numVisiblePylons > 0)
			   {
					int targetPylon = findTargetPylon_weighted();
					if (messagesOn() && GUIstate == NAV_DEBUG){
						SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,5,"  TP:");
						if (targetPylon>=0){
							sendInt(visiblePylons[targetPylon].center, DISPLAY_IN_GUI_NEWLINE);
						} else
						{
							sendInt(targetPylon, DISPLAY_IN_GUI_NEWLINE);
						}
					}
					//if didn't find a pylon of the right color, break
					if(targetPylon == -1)
					{
						if (truckState.distToBLIND <= .1)
						{
							transitionToBLIND_STRAIGHT();
						}
						else
						{
							break;
						}
					}
						
					updateTruckState(targetPylon, deltaDistance);
					if (GUIstate != NAV_JOYSTICK){
						setSteeringAngle(2*getHeadingError());
					}
					
                }//end of if(visble pylons)
                
				if(truckState.distToBLIND <= .1)
				{
					transitionToBLIND_STRAIGHT();
				}

				/*if ( truckState.angleToPylon > (ANGLE_OF_VIEW / 2) - 5 || truckState.angleToPylon < -((ANGLE_OF_VIEW / 2) - 5))
					transitionToBLIND_STRAIGHT();*/
    
				break;
			}
		case BLIND_STRAIGHT:
			{
				if (messagesOn() && GUIstate == NAV_DEBUG)
				{
					SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,4,"STRT");
				}
				if (GUIstate != NAV_JOYSTICK){
					setSteeringAngle(0);
				}
				if (truckState.distTraveled >= STRAIGHT_BLIND_DIST)
				{
					transitionToBLIND_TURN();
				}

				break;
			}
		case BLIND_TURN:
			{
				if (messagesOn() && GUIstate == NAV_DEBUG) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,4,"TURN");
				
				if (GUIstate != NAV_JOYSTICK){
					//continually update steering angle
					if (coursePylons[truckState.currentPylon].color == LEFT)
					{
						setSteeringAngle(-TURN_ANGLE);
					}
					else
					{
						setSteeringAngle(TURN_ANGLE);
					}
				}

				//check on transition
				if(truckState.distTraveled >= coursePylons[truckState.currentPylon].distToTurn)
				{
					transitionToGATHER();
				}

				break;
			}
		default:
			{
				//if we get here we should probably stop
				if (GUIstate != NAV_JOYSTICK){
					setVelocity(0);
					setSteeringAngle(0);
				}
				break;
			}
	}//end switch	
	
}


int getAngleToPylon(int pylon)
{
	//An approximation
	int angle = (int)(( (float)ANGLE_OF_VIEW / (float)CAMERA_WIDTH ) * 
				(float)(CAMERA_WIDTH/2 - visiblePylons[pylon].center)) ;
    
    return angle;
}


float getDistanceToPylon(int targetPylon, float deltaDistance)
{
    //the algorithm we want to use for calculating distance is dependant on height
	//These equations were all determined experimentally and with linear regressions
    float height;
	float distance;
	float width;
	int reliability;

	height = (float)visiblePylons[targetPylon].height;
	width = (float)visiblePylons[targetPylon].width;
	reliability = visiblePylons[targetPylon].reliability;

	if(reliability < RELIABILITY_CUTOFF)
	{
		distance = MAX_VISIBLE_DISTANCE;	
	}
	else if(height < 60)		// 0-60	Too far
	{
		distance = MAX_VISIBLE_DISTANCE;	//default far distance
	}
	else if(height < 99)	// 99-60 
	{
		distance = ((height*-4.3515)+866.2625) /100.0;
	}
	else if(height < 124)	// 99-124 
	{
		distance = ((height*-3.23427)+749.0035) /100.0;
	}
	else if(height < 151) 	// 124-151 
	{
		distance = ((height*-2.24102)+625.0989) /100.0;
	}
	else if(height < 176)	// 151-176 
	{
		distance = ((height*-1.59301)+528.705) /100.0;
	}
	else if(height < 200)	// 176-200
	{
		distance = ((height*-1.23457)+465.8642) /100.0;
	}
	else if(height < 242)	// 200-242
	{
		distance = ((height*-.94072)+407.1459) /100.0;
	}
	else if(height < 295)	// 242-295
	{
		distance = ((height*-.57485)+318.9151) /100.0;
	}
	else if(height < 369)	// 295-369
	{
		distance = ((height*-.40315)+267.8368) /100.0;
	}
    else if (width < 34)// 300-480 (1-2m) Width
    {
        distance = (float)(width - 52) / -17.9;
    }
    else if (width < 55)// 300-480 (1-2m) Width
    {
        distance = ((width * -1.61111) + 159.6111) / 100.0; //mjd
    }
    else if (width < 81)// 300-480 (1-2m) Width
    {
        distance = ((width * -1) + 126) / 100.0; //mjd
    }
    else if (width < 137)// 300-480 (1-2m) Width
    {
        distance = ((width * -0.28571) + 68.14286) / 100.0; //mjd
    }
    else //if (width < 229)// this should handle to the largest width
    {
        distance = ((width * -0.11957) + 45.38043) / 100.0; //mjd
    }
	
	return distance;
}


/*
Finds the pylon among the visible pylons that is 
  1) the right color (type)
  2) closest to the center of the field of view
  3) closest to the truck
Returns the index of the pylon in visiblePylons
  or -1 if no pylon found of the right color
*/
int findTargetPylon_weighted(){
	int curColor = coursePylons[truckState.currentPylon].color;
	FindPylonWeight* pWeight = curColor == ORANGE_PYLON ? findPylonWeightsOrange : findPylonWeightsGreen;

    int pylonIndex = 0;
    int targetPylon = -1;
	float maxWeight = 0.0;
	float curWeight = 0.0;

	if(truckState.mode == GATHER)
	{
		setCriticalImage_alreadyCheckedOut(FTE_OWNER_NAVIGATION);
	}

	for (pylonIndex = 0; pylonIndex < numVisiblePylons; pylonIndex++)
	{
        //check color
        if (visiblePylons[pylonIndex].color != curColor) continue;
		if(truckState.mode == GATHER)
		{
			curWeight = getFindPylonsWeight(pWeight,visiblePylons[pylonIndex].center);
		}
		else //CORRECT mode
		{
			curWeight = getFindPylonsWeight(findPylonWeightsTracking,visiblePylons[pylonIndex].center-findCenter);
		}
		
		//int heightProbability = getDistanceToPylon(pylonIndex, deltaDistance) / courseDistances[truckState.currentPylon];
		//int heightProbability = visiblePylons[pylonIndex].height * courseDistances[truckState.currentPylon];
		//curWeight *= (1.0 + ((float)visiblePylons[pylonIndex].reliability) / 100.0) * heightProbability;
		curWeight *= visiblePylons[pylonIndex].height;

		if (curWeight > maxWeight){ //the current pylon is better looking than the last best pylon
			maxWeight = curWeight;
			targetPylon = pylonIndex; //so update our best
		}
    }//end for each pylon

	//update findcenter if we found a pylon
	if (targetPylon >= 0)
	{
		findCenter = visiblePylons[targetPylon].center;
	}
	//else don't update findCenter

	return targetPylon;

}


void updateTruckState(int targetPylon, int deltaDistance)
{
	truckState.distToPylon = getDistanceToPylon(targetPylon, deltaDistance);
	truckState.angleToPylon = getAngleToPylon(targetPylon);
	//We want to store the distance until transfering to BLIND_STRAIGHT mode
    truckState.distToBLIND = getDistanceToTravel(targetPylon, deltaDistance) - STRAIGHT_BLIND_DIST;
}

int getHeadingError()
{
	int16 error;
	int16 desiredAngle;

	if(coursePylons[truckState.currentPylon].color == LEFT)// Orange
	{
		desiredAngle = asin((TURN_RADIUS-CAMERA_SIDE_DISPLACEMENT)/truckState.distToPylon)*-180/PI;
	}
	else
	{
		desiredAngle = asin((TURN_RADIUS-CAMERA_SIDE_DISPLACEMENT)/truckState.distToPylon)*180/PI;
	}
	
	error = truckState.angleToPylon - desiredAngle;

	//limit error correction 
	if(error > ANGLE_OF_VIEW/2)
	{
		error = ANGLE_OF_VIEW/2;
	}
	else if (error < -ANGLE_OF_VIEW/2)
	{
		error = -ANGLE_OF_VIEW/2;
	}

	//The closer we are the less we want to correct b/c it will throw us off course before going blind
	if(truckState.distToPylon <  1.0)
	{
		error = error * .125;	//error * .125 @1.0m and closer
	} else if(truckState.distToPylon <  2.0)
	{
		error = error * (truckState.distToPylon * .875 -.75); //slope from .125 @1.0m to 1.0 @2.0m	
	}

    return error;
}

float getDistanceToTravel(int pylon, float deltaDistance)
{
    float distToPylon = getDistanceToPylon(pylon, deltaDistance);

    return distToPylon * cos((float)getAngleToPylon(pylon) * PI / 180.0);
}

void updateAngles()
{
	int pylonIndex;

	if(truckState.totalPylons == 2)	//different adjustments for two pylon courses
	{
		for (pylonIndex = 0; pylonIndex < truckState.totalPylons; pylonIndex++)
		{
			coursePylons[pylonIndex].angleToTravel = coursePylons[pylonIndex].angleToPylon;
			
			if(coursePylons[0].color != coursePylons[1].color)
			{
				if (coursePylons[pylonIndex].angleToTravel < 0)//RIGHT, GREEN
					{
						coursePylons[pylonIndex].angleToTravel = 
							(int)( coursePylons[pylonIndex].angleToTravel - 2 * ADDED_ANGLE ) % 360;
					}
					if (coursePylons[pylonIndex].angleToTravel >= 0)
					{
						coursePylons[pylonIndex].angleToTravel =
							(int)(coursePylons[pylonIndex].angleToTravel + 2 * ADDED_ANGLE) % 360;
					}
			}
		}//end for each pylon
	}
	else	//adjust angles for all other coures
	{
		//this algorithm supposes the list is already populated
		// with all the pylons and their types are set
		for (pylonIndex = 0; pylonIndex < truckState.totalPylons; pylonIndex++)
		{
			coursePylons[pylonIndex].angleToTravel = coursePylons[pylonIndex].angleToPylon;
			
			//if not same color as previous - add the extra angle (see def of ADDED_ANGLE)
			if (coursePylons[pylonIndex].color != coursePylons[(pylonIndex-1) < 0 ? (truckState.totalPylons-1) : (pylonIndex-1)].color)
			{
				if (coursePylons[pylonIndex].angleToTravel < 0)
				{
					coursePylons[pylonIndex].angleToTravel = 
						(int)( coursePylons[pylonIndex].angleToTravel - ADDED_ANGLE ) % 360;
				}
				if (coursePylons[pylonIndex].angleToTravel >= 0)
				{
					coursePylons[pylonIndex].angleToTravel =
						(int)(coursePylons[pylonIndex].angleToTravel + ADDED_ANGLE) % 360;
				}
			}
			
			//if not same color as next - add the extra angle
			if (coursePylons[pylonIndex].color != coursePylons[(pylonIndex+1) % truckState.totalPylons].color)
			{
				if (coursePylons[pylonIndex].angleToTravel < 0)
				{
					coursePylons[pylonIndex].angleToTravel =
						(int)(coursePylons[pylonIndex].angleToTravel - ADDED_ANGLE) % 360;
				}
				if (coursePylons[pylonIndex].angleToTravel >= 0)
				{
					coursePylons[pylonIndex].angleToTravel =
						(int)(coursePylons[pylonIndex].angleToTravel + ADDED_ANGLE) % 360;
				}
			}
		}//end for
	}
}

void calculateDistToTurn()
{
	int pylonIndex; 

	for(pylonIndex = 0; pylonIndex < numAngles; pylonIndex++)
	{
		//Calculate distance to Turn
		if(coursePylons[pylonIndex].color == RIGHT) 
                coursePylons[pylonIndex].distToTurn = coursePylons[pylonIndex].angleToTravel * PI / 180.0 * (TURN_RADIUS + 0);  //in m
            else//RIGHT, GREEN
                coursePylons[pylonIndex].distToTurn = -coursePylons[pylonIndex].angleToTravel * PI / 180.0 * (TURN_RADIUS + 0);	//+.14 is to compensate to edge of outside tire
	}
}

//Assumes that update angles has been called
void setCompassAngles()
{
	int pylonIndex; 
	
	//first angle is set from information from course description file
	coursePylons[0].compassAngle = absoluteAngle;

	if (messagesOn() && GUIstate == NAV_DEBUG){
		SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,15,"Compass Angles:");
		sendInt(coursePylons[0].compassAngle, DISPLAY_IN_GUI_NEWLINE);
	}
		
	for(pylonIndex = 1; pylonIndex < numAngles; pylonIndex++)
	{
		int angle = coursePylons[pylonIndex - 1].compassAngle - coursePylons[pylonIndex-1].originalAngle;
		//put in range 0-360
		angle = ((angle < 0) ? 360+angle : angle) % 360;

		//Each angle is based off the previous compass angle minus the angle to turn around the previous pylon
		coursePylons[pylonIndex].compassAngle = angle;
		
		if (messagesOn() && GUIstate == NAV_DEBUG) sendInt(coursePylons[pylonIndex].compassAngle, DISPLAY_IN_GUI_NEWLINE);
	}
}

void transitionToCORRECT()
{
	truckState.mode = CORRECT;
	float straightSpeed = STRAIGHT_SPEED;
	if(courseDistances[truckState.currentPylon] > 1){
		straightSpeed += (float)courseDistances[truckState.currentPylon] / 3.0;
	}
	if (GUIstate != NAV_JOYSTICK){
		setVelocity(straightSpeed);
		setSteeringAngle(0);
	}
}

void transitionToBLIND_STRAIGHT()
{
	truckState.mode = BLIND_STRAIGHT;
	if (GUIstate != NAV_JOYSTICK){
		setVelocity(BLIND_STRAIGHT_SPEED);
		setSteeringAngle(0);
	}
	truckState.distTraveled = 0;
}

void transitionToBLIND_TURN()
{
	if (messagesOn() && GUIstate == NAV_DEBUG){
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,15,"AngleToTravel: ");
		sendInt(coursePylons[truckState.currentPylon].angleToTravel, DISPLAY_IN_GUI_NEWLINE);
	}
			
	truckState.mode = BLIND_TURN;
	if (GUIstate != NAV_JOYSTICK){
		setVelocity(TURN_SPEED);
		if (coursePylons[truckState.currentPylon].color == LEFT)
		{
			setSteeringAngle(-TURN_ANGLE);
		}
		else
		{
			setSteeringAngle(TURN_ANGLE);
		}
	}
    truckState.distTraveled = 0;

	//Reset HSV thresholds for the compass angle for the next pylon (so it will be ready when it comes out of TURN)
	navigationSetHSV( coursePylons[truckState.currentPylon + 1].compassAngle );
}

void transitionToGATHER()
{
	truckState.mode = GATHER;
	truckState.currentPylon = ++truckState.currentPylon % truckState.totalPylons;
	truckState.distTraveled = 0;
	gathered_frames = 0;
	frames_missed_in_gather = 0;

	//update centerFind to look for a pylon based on the pylon to look for next
	//right now we just set it to zero for simplicity
	if(coursePylons[truckState.currentPylon].color == LEFT)	//ORANGE
	{
		findCenter = CAMERA_WIDTH/2;
	}
	else	//GREEN
	{
		findCenter = CAMERA_WIDTH/2 - FIND_RANGE;
	}

	if (messagesOn() && GUIstate == NAV_DEBUG) {
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,15,"Course Pylon#: ");
		sendInt(truckState.currentPylon, DISPLAY_IN_GUI_NEWLINE);
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,19,"Looking for Color: ");
		SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,6, (coursePylons[truckState.currentPylon].color == GREEN_PYLON? "GREEN " : "ORANGE")) ;
	}

}
