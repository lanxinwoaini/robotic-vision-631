/******************************************************************************

FILE:    nagivation.h
AUTHOR:  Kevin Ellsworth
CREATED: 12 Feb 2009

UPDATED: 04 Mar 2009

	
******************************************************************************/
#ifndef NAVIGATION_H_
#define NAVIGATION_H_

/* Includes ----------------------------------------------------------------------*/
#include "SystemTypes.h"
#include "Vision.h"

/* Defines -----------------------------------------------------------------------*/


// Calculation variables
#define PI 3.1415926

// Course Parameters
#define MAX_PYLONS		25
#define MIN_PYLON_DIST	3.0f    //m
#define MAX_PYLON_DIST	8.0f    //m

// Pylon Parameters
#define PYLON_HEIGHT	0.7239f	//m		// 28.5 inches
#define PYLON_WIDTH		0.0508f	//m		 // 2.0 inches

// Car Parameters
#define WHEEL_BASE 0.272f	//m   // 11.6875 inches +/- 0.125 - given, .272m measured

// Camera Frame Size and Parameters
#define CAMERA_WIDTH		640.0f			// pixel values 0 ... cameraWidth-1
#define CAMERA_HEIGHT		480.0f			// pixel values 0 ... cameraHeight-1
#define ANGLE_OF_VIEW		53.0f			// camera viewing angle in degrees
#define VERT_ANGLE_OF_VIEW	46.0f			//vertical viewing angle in degrees 
											//Vertical angle has not yet been determined - perhaps 240/320 * 56.0? 
#define MAX_VISIBLE_DISTANCE		6.5f	//The distance at which pylons disappear from vision
#define CAMERA_SIDE_DISPLACEMENT	.10f	//Distance from camera center to actual turning center of radius
#define	CAMERA_FRONT_DISPLACEMENT	.20f	//Distance from camera to back axle

#define RELIABILITY_CUTOFF	50		//Reliability lower than this is untrustable

#define FRAMES_IN_GATHER	4


//Debug
//if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,7,"Stopped");
#define NAV_NO_VISION 0


/* Structs -----------------------------------------------------------------------*/
enum GUI_Mode {
	NAV_DEBUG = 0,
	NAV_RACE  = 1,
	NAV_GAME  = 2,
	NAV_JOYSTICK = 3
};

enum Drive_Mode	{			//Modes used for navigating a course
	GATHER			= 0,	//Look for the next pylon and head for it
	CORRECT			= 1,	//Keep heading for pylon, use vision to correct path
	BLIND_STRAIGHT	= 2,	//Can't see pylon, but not time to turn yet
	BLIND_TURN		= 3		//Can't see pylon and turning around it - switch to GATHER when done
};

typedef struct {
	char mode;					//mode the car is in
    uint8 currentPylon;			//pylon that truck is currently heading for
    uint8 totalPylons;			//total pylons in the course - used for wrap-around
    float distTraveled;			//reset in BLND_STRIAGHT, BLIND_TURN, GATHER for distToTravel
    int8 angleToPylon;			//angle to the pylon (heading) in degrees
    float distToPylon;			//distance from current position to pylon in meters
    float distToBLIND;			//distance left to travel until switch to BLIND_TURN mode //in meters
} TruckState;

enum Pylon_Type {
	RIGHT = 0,	//Green
	LEFT = 1,	//Orange
};

typedef struct {
	char color;					//LEFT or RIGHT - side to pass on a turn
    float angleToPylon;			//Angle to the next pylon in the course desc file (CDF)
	float angleToTravel;		//Updated angle to turn around pylon
    float distToPylon;			//Distance to the next pylon in CDF
    float distToTravel;			//Distance traveled by the car from RADIUS away from to RADIUS away
    float distToTurn;			//Distance to turn around this pylon during BLIND_TURN mode
	int32 compassAngle;		//Heading relative to the room, based off of starting angle given in course description file
	int32 originalAngle;
} CoursePylon;

typedef struct {
	int32 x;			//x coord of the particular weight point where its values begin affecting
	int32 y;			//y coord of the particular weight point where its values begin affecting
	float m;			//m in y=mx+b
	float b;			//b in y=mx+b for weight result
} FindPylonWeight;

/* Function Prototypes ------------------------------------------------------*/
void runCourse(visiblePylon * visiblePylonArray, uint8 numPylons);
void navigationInit();
void navigationMemInit();
void navStateReset();
uint8 setCourseData();
void NAV_Process();
//float inchesToMeters(float inches);
//float metersToInches(float meters);
int getAngleToPylon(int pylon);
float getDistanceToPylon(int targetPylon, float deltaDistance);
void updateTruckState(int targetPylon, int deltaDistance);
int getHeadingError();
float getDistanceToTravel(int pylon, float deltaDistance);
void updateAngles();
void calculateDistToTurn();
void setCompassAngles();
void transitionToCORRECT();
void transitionToBLIND_STRAIGHT();
void transitionToBLIND_TURN();
void transitionToGATHER();
int getClosestToCenter(uint8 pylon1, uint8 pylon2);

float getFindPylonsWeight(FindPylonWeight* pWeight, int col);
void  calculateMBValues(FindPylonWeight* pWeight);
void initializePylonWeights();
int findTargetPylon_weighted();
int getAdjustedAngle(int angle, float turnSpeed);

#endif
