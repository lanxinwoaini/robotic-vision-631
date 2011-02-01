/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    State.c
AUTHOR:  Barrett Edwards
CREATED: 10 Nov 2006

DESCRIPTION

******************************************************************************/

/* Includes -----------------------------------------------------------------*/
#include "TX.h"
#include "SystemTypes.h"
#include "InterruptControl.h"
#include "State.h"
#include "pidControl.h"
#include "navigation.h"


/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Global Memory ------------------------------------------------------------*/
HeliosState hstate;
extern int encoderValue;
extern int desiredAnglePID;
extern TruckState truckState;
extern int actualPITFreq;
extern int framesPerSecond_HW;
extern int framesPerSecond_SW;

TransmitState transmitState;

/* Code ---------------------------------------------------------------------*/

/* ==========================================================================*/
/* Receive Functions ========================================================*/

/******************************************************************************
 * Functional Group: Helper Functions
 * Definition:       
 * 
 *  1: InitMemory()
 *  
*/ 
void StateInit(){

	transmitState.velocity = 0;
	transmitState.headingAngle = 0;
	transmitState.sentSteeringAngle = 0;
	transmitState.state_currentPylon = 0;
	transmitState.state_mode = 0;
	transmitState.state_distTraveled = 0;
	transmitState.state_angleToPylon = 0;
	transmitState.state_distToPylon = 0;
	transmitState.state_distToBLIND = 0;
	transmitState.hwFPS = 0;
	transmitState.swFPS = 0;

	hstate.running			= FALSE;
	hstate.fps				= 0;
	hstate.speed			= 0;
	hstate.sendingVideo		= TRUE;
	//hstate.guiCommChannel	= CHAN_USB;   // CHAN_USB breaks the code
	hstate.session			= SN_DISABLED;
	hstate.manualControl	= FALSE;
	hstate.loopDelay		= 0;
	hstate.sendingState		= TRUE;
	hstate.stateDelay		= 1000;//was 100;
	hstate.readingCamRegs	= FALSE;
	hstate.sendingCamRegs	= FALSE;
	hstate.camRegsDelay		= 500;
	hstate.x				= 0;
	hstate.y				= 0;
	hstate.imageWidth		= FRAME_WIDTH;
	hstate.imageHeight		= FRAME_HEIGHT;
	hstate.memoryInit		= 0;
	hstate.camVersion		= 0;
	hstate.messagesOn		= TRUE;
	hstate.sendImage		= FALSE;
	hstate.heartbeatCountdown = 0;
	hstate.bootloadReady	= FALSE;
	hstate.pylonsReady		= FALSE;
	
}

 // returns a pointer to the HeliosState struct for transmission
void stateData(uint8* buffer){
	DISABLE_INTERRUPTS();
	uint8* ptr = (uint8*) &transmitState;
	int i;
	for(i = 0 ; i < sizeof(TransmitState) ; i++)
		buffer[i] = ptr[i];
	RESTORE_INTERRUPTS(msr);
}

//avoid using this, because this just eats system time, lol
//especially avoid using this during a disable_interrupts time period
void   usleep(uint32 time){
	int i = 0;
	int j = 0;
	int max = 150000*time;
	for (i = 0; i < max; i++){
		j = i;
	}
}

uint8  running()						{DISABLE_INTERRUPTS();  uint8  rv = hstate.running;					RESTORE_INTERRUPTS(msr); return rv;}
uint8  fps()							{DISABLE_INTERRUPTS();  uint8  rv = hstate.fps;						RESTORE_INTERRUPTS(msr); return rv;}
uint8  speed()							{DISABLE_INTERRUPTS();  uint8  rv = hstate.speed;					RESTORE_INTERRUPTS(msr); return rv;}
uint8  sendingVideo()					{DISABLE_INTERRUPTS();  uint8  rv = hstate.sendingVideo;			RESTORE_INTERRUPTS(msr); return rv;}
uint8  guiCommChannel()					{DISABLE_INTERRUPTS();  uint8  rv = hstate.guiCommChannel;			RESTORE_INTERRUPTS(msr); return rv;}
uint8  session()						{DISABLE_INTERRUPTS();  uint8  rv = hstate.session;					RESTORE_INTERRUPTS(msr); return rv;}
uint8  manualControl()					{DISABLE_INTERRUPTS();  uint8  rv = hstate.manualControl;			RESTORE_INTERRUPTS(msr); return rv;}
uint16 loopDelay()						{DISABLE_INTERRUPTS();  uint16 rv = hstate.loopDelay;				RESTORE_INTERRUPTS(msr); return rv;}
uint8  sendingState()					{DISABLE_INTERRUPTS();  uint8  rv = hstate.sendingState;			RESTORE_INTERRUPTS(msr); return rv;}
uint16 stateDelay()						{DISABLE_INTERRUPTS();  uint16 rv = hstate.stateDelay;				RESTORE_INTERRUPTS(msr); return rv;}
uint8  sendingRegsUInt()				{DISABLE_INTERRUPTS();  uint8  rv = hstate.sendingRegsUInt;			RESTORE_INTERRUPTS(msr); return rv;}
uint16 delayRegsUInt()					{DISABLE_INTERRUPTS();  uint16 rv = hstate.delayRegsUInt;			RESTORE_INTERRUPTS(msr); return rv;}
uint8  sendingRegsFloat()				{DISABLE_INTERRUPTS();  uint8  rv = hstate.sendingRegsFloat;		RESTORE_INTERRUPTS(msr); return rv;}
uint16 delayRegsFloat()					{DISABLE_INTERRUPTS();  uint16 rv = hstate.delayRegsFloat;			RESTORE_INTERRUPTS(msr); return rv;}
uint8  readingCamRegs()					{DISABLE_INTERRUPTS();  uint8  rv = hstate.readingCamRegs;			RESTORE_INTERRUPTS(msr); return rv;}
uint8  sendingCamRegs()					{DISABLE_INTERRUPTS();  uint8  rv = hstate.sendingCamRegs;			RESTORE_INTERRUPTS(msr); return rv;}
uint16 camRegsDelay()					{DISABLE_INTERRUPTS();  uint16 rv = hstate.camRegsDelay;			RESTORE_INTERRUPTS(msr); return rv;}
void   location(uint32* x, uint32* y)	{DISABLE_INTERRUPTS();  *x=hstate.x; *y=hstate.y;					RESTORE_INTERRUPTS(msr);           }
uint16 imageWidth()						{DISABLE_INTERRUPTS();  uint16 rv = hstate.imageWidth;				RESTORE_INTERRUPTS(msr); return rv;}
uint16 imageHeight()					{DISABLE_INTERRUPTS();  uint16 rv = hstate.imageHeight;				RESTORE_INTERRUPTS(msr); return rv;}
uint8  memoryInit()						{DISABLE_INTERRUPTS();  uint8  rv = hstate.memoryInit;				RESTORE_INTERRUPTS(msr); return rv;}
uint32 camVersion()						{DISABLE_INTERRUPTS();  uint32 rv = hstate.camVersion;				RESTORE_INTERRUPTS(msr); return rv;}
uint8  messagesOn()						{DISABLE_INTERRUPTS();  uint8  rv = hstate.messagesOn;				RESTORE_INTERRUPTS(msr); return rv;}
uint8  sendImage()						{DISABLE_INTERRUPTS();  uint8  rv = hstate.sendImage;				RESTORE_INTERRUPTS(msr); return rv;}
uint8  bootloadReady()					{DISABLE_INTERRUPTS();  uint8  rv = hstate.bootloadReady;			RESTORE_INTERRUPTS(msr); return rv;}
int16 heartbeatCountdown()				{DISABLE_INTERRUPTS();  int32 rv = hstate.heartbeatCountdown;		RESTORE_INTERRUPTS(msr); return rv;}
uint8 pylonsReady()						{DISABLE_INTERRUPTS();  uint8 rv = hstate.pylonsReady;				RESTORE_INTERRUPTS(msr); return rv;}
int16 heartbeatCountdownTick()			{
	DISABLE_INTERRUPTS();
	int16 rv = 0;
	if (hstate.heartbeatCountdown > 0){
		hstate.heartbeatCountdown--;  //heartbeat tick
		if (hstate.heartbeatCountdown == 0) rv = -1; //return -1 if we JUST became 0
	} else {rv = hstate.heartbeatCountdown;} //will be a positive number
	RESTORE_INTERRUPTS(msr); return rv;
}

void setRunning			(uint8  val)			{DISABLE_INTERRUPTS();  hstate.running			= val;			RESTORE_INTERRUPTS(msr);}
void setFps				(uint8  val)			{DISABLE_INTERRUPTS();  hstate.fps				= val;			RESTORE_INTERRUPTS(msr);}
void setSpeed			(uint8  val)			{DISABLE_INTERRUPTS();  hstate.speed				= val;			RESTORE_INTERRUPTS(msr);}
void setSendingVideo	(uint8  val)			{DISABLE_INTERRUPTS();  hstate.sendingVideo		= val;			RESTORE_INTERRUPTS(msr);}
void setGuiCommChannel	(uint8  val)			{DISABLE_INTERRUPTS();  hstate.guiCommChannel		= val;			RESTORE_INTERRUPTS(msr);}
void setSession			(uint8  val)			{DISABLE_INTERRUPTS();  hstate.session			= val;			RESTORE_INTERRUPTS(msr);}
void setManualControl	(uint8  val)			{DISABLE_INTERRUPTS();  hstate.manualControl		= val;			RESTORE_INTERRUPTS(msr);}
void setLoopDelay		(uint16 val)			{DISABLE_INTERRUPTS();  hstate.loopDelay			= val;			RESTORE_INTERRUPTS(msr);}
void setSendingState	(uint8  val)			{DISABLE_INTERRUPTS();  hstate.sendingState		= val;			RESTORE_INTERRUPTS(msr);}
void setStateDelay		(uint16 val)			{DISABLE_INTERRUPTS();  hstate.stateDelay			= val;			RESTORE_INTERRUPTS(msr);}
void setSendingRegsUInt	(uint8  val)			{DISABLE_INTERRUPTS();  hstate.sendingRegsUInt	= val;			RESTORE_INTERRUPTS(msr);}
void setDelayRegsUInt	(uint16 val)			{DISABLE_INTERRUPTS();  hstate.delayRegsUInt		= val;			RESTORE_INTERRUPTS(msr);}
void setSendingRegsFloat(uint8  val)			{DISABLE_INTERRUPTS();  hstate.sendingRegsFloat	= val;			RESTORE_INTERRUPTS(msr);}
void setDelayRegsFloat	(uint16 val)			{DISABLE_INTERRUPTS();  hstate.delayRegsFloat		= val;			RESTORE_INTERRUPTS(msr);}
void setReadingCamRegs	(uint8  val)			{DISABLE_INTERRUPTS();  hstate.readingCamRegs		= val;			RESTORE_INTERRUPTS(msr);}
void setSendingCamRegs	(uint8  val)			{DISABLE_INTERRUPTS();  hstate.sendingCamRegs		= val;			RESTORE_INTERRUPTS(msr);}
void setCamRegsDelay	(uint16 val)			{DISABLE_INTERRUPTS();  hstate.camRegsDelay		= val;			RESTORE_INTERRUPTS(msr);}
void setLocation		(uint32 x, uint32 y)	{DISABLE_INTERRUPTS();  hstate.x=x; hstate.y=y;					RESTORE_INTERRUPTS(msr);}
void setImageWidth		(uint16 val)			{DISABLE_INTERRUPTS();  hstate.imageWidth			= val;			RESTORE_INTERRUPTS(msr);}
void setImageHeight		(uint16 val)			{DISABLE_INTERRUPTS();  hstate.imageHeight		= val;			RESTORE_INTERRUPTS(msr);}
void setMemoryInit		(uint8  val)			{DISABLE_INTERRUPTS();  hstate.memoryInit			= val;			RESTORE_INTERRUPTS(msr);}
void setCamVersion		(uint32 val)			{DISABLE_INTERRUPTS();  hstate.camVersion			= val;			RESTORE_INTERRUPTS(msr);}
void setMessagesOn		(uint8  val)			{DISABLE_INTERRUPTS();  hstate.messagesOn			= val;			RESTORE_INTERRUPTS(msr);}
void setSendImage		(uint8  val)			{DISABLE_INTERRUPTS();  hstate.sendImage			= val;			RESTORE_INTERRUPTS(msr);}
void resetHeartbeatCountdown(int16  val)		{DISABLE_INTERRUPTS();  hstate.heartbeatCountdown	= val;			RESTORE_INTERRUPTS(msr);}
void setBootloadReady	(uint8 val)				{DISABLE_INTERRUPTS();  hstate.bootloadReady		= val;			RESTORE_INTERRUPTS(msr);}
void setPylonsReady	(uint8 val)				{DISABLE_INTERRUPTS();  hstate.pylonsReady		= val;				RESTORE_INTERRUPTS(msr);}


void updateGUI(){
	int lastEncoderValue;
	int ticksPerSecond;

	DISABLE_INTERRUPTS();
	
	lastEncoderValue = hstate.lastEncoderValue;
	ticksPerSecond = (encoderValue - lastEncoderValue) * GUI_UPDATES_PER_SECOND * 100 / actualPITFreq;
	transmitState.velocity = ticksPerSecond / TICKS_PER_METER;
	transmitState.sentSteeringAngle = desiredAnglePID;

	//truckstate variables
	transmitState.state_currentPylon	= truckState.currentPylon;
	transmitState.state_mode			= truckState.mode;
	transmitState.state_distTraveled	= truckState.distTraveled;
	transmitState.state_angleToPylon	= truckState.angleToPylon;
	transmitState.state_distToPylon		= truckState.distToPylon;
	transmitState.state_distToBLIND		= truckState.distToBLIND;
	transmitState.hwFPS					= framesPerSecond_HW;
	transmitState.swFPS					= framesPerSecond_SW;

	hstate.lastEncoderValue = encoderValue;

	RESTORE_INTERRUPTS(msr);


	sendStateData();
}

void sendStateData(){

	uint8 buffer[sizeof(TransmitState)];
	
	uint8* ptr = (uint8*) &transmitState;
	int i;
	for(i = 0 ; i < sizeof(TransmitState) ; i++)
		buffer[i] = ptr[i];
	
	SendPacket(STATE, 0, sizeof(TransmitState), buffer);
}

