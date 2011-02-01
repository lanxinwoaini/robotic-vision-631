/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    State.h
AUTHOR:  Barrett Edwards
CREATED: 10 Nov 2006

DESCRIPTION
	
******************************************************************************/
#ifndef STATE_H
#define STATE_H

/* Includes -----------------------------------------------------------------*/
#include "SystemTypes.h"


#define FRAME_WIDTH 	640
#define FRAME_HEIGHT 	480

#define GUI_UPDATES_PER_SECOND	5
#define GUI_MOD_UPDATES			(100 / GUI_UPDATES_PER_SECOND)

/* Defines ------------------------------------------------------------------*/
enum GuiCommChanels {
	CHAN_NULL		= 0,
	CHAN_SERIAL		= 1,
	CHAN_WIRELESS	= 2,
	CHAN_TTL		= 3,
	CHAN_USB		= 4
};

enum Sessions {
	SN_DISABLED				= 0,
	SN_DEBUG				= 1,
	SN_RACE					= 2,
	SN_MANUAL				= 3,
	SN_JOYSTICK				= 4
};

/* Structs ------------------------------------------------------------------*/
typedef struct {
	int	   lastEncoderValue;
	uint8  running;			// bool
	uint8  fps;				// int
	uint8  speed;			// int
	uint8  sendingVideo;	// bool
	uint8  guiCommChannel;	// 
	uint8  session;			//
	uint8  manualControl;	// bool
	uint8  loopDelay;		// milliseconds
	uint8  sendingState;	// bool
	uint16 stateDelay;		// milliseconds	
	uint8  sendingRegsUInt;
	uint16 delayRegsUInt;
	uint8  sendingRegsFloat;
	uint16 delayRegsFloat;
	uint8  readingCamRegs;	// bool
	uint8  sendingCamRegs;	// bool
	uint16 camRegsDelay;	// milliseconds
	uint32 x;				//
	uint32 y;				//
	uint16 imageWidth;		//
	uint16 imageHeight;		// 
	uint8  memoryInit;		// 
	uint32 camVersion;		//
	uint8  bootloadReady;   // true if the bootloader is ready
	uint8  messagesOn;
	uint8  sendImage;
	int16 heartbeatCountdown; //milliseconds
	uint8  pylonsReady;
} HeliosState;

typedef struct {

	float  velocity;
	int8  sentSteeringAngle;
	uint16 headingAngle;
	//Navigation
	uint8 state_currentPylon;
	uint8 state_mode;
	float state_distTraveled;
	int8 state_angleToPylon;
	float state_distToPylon;
	float state_distToBLIND;

	uint8 hwFPS;
	uint8 swFPS;

} __attribute__((__packed__)) TransmitState;


/* External Memory-----------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/
void	StateInit();
void	stateData(uint8* buffer); // fills a pointer (buffer) with the HeliosState
void    usleep(uint32 time); //sleeps (runs a for loop) for a set amount of time
void	updateGUI();

uint8  running();
uint8  fps();
uint8  speed();
uint8  sendingVideo();
uint8  guiCommChannel();
uint8  session();
uint8  manualControl();
uint16 loopDelay();
uint8  sendingState();
uint16 stateDelay();
uint8  sendingRegsUInt();
uint16 delayRegsUInt();
uint8  sendingRegsFloat();
uint16 delayRegsFloat();
uint8  readingCamRegs();
uint8  sendingCamRegs();
uint16 camRegsDelay();
void   location(uint32* x, uint32* y);
uint16 imageWidth();
uint16 imageHeight();
uint8  memoryInit();
uint32 camVersion();
uint8  messagesOn();
uint8  sendImage();
int16 heartbeatCountdown();
int16 heartbeatCountdownTick();
uint8 bootloadReady();
uint8 pylonsReady();

void setRunning(uint8 val);
void setFps(uint8 val);
void setSpeed(uint8 val);
void setSendingVideo(uint8 val);
void setGuiCommChannel(uint8 val);
void setSession(uint8 val);
void setManualControl(uint8 val);
void setLoopDelay(uint16 val);
void setSendingState(uint8 val);
void setStateDelay(uint16 val);
void setSendingRegsUInt(uint8 val);
void setDelayRegsUInt(uint16 val);
void setSendingRegsFloat(uint8 val);
void setDelayRegsFloat(uint16 val);
void setReadingCamRegs(uint8 val);
void setSendingCamRegs(uint8 val);
void setCamRegsDelay(uint16 val);
void setLocation(uint32 x, uint32 y);
void setImageWidth(uint16 val);
void setImageHeight(uint16 val);
void setMemoryInit(uint8 val);
void setCamVersion(uint32 val);
void setMessagesOn(uint8 val);
void setSendImage(uint8 val);
void resetHeartbeatCountdown(int16 val);
void setBootloadReady(uint8 val);
void sendStateData();
void setPylonsReady(uint8 val);

#endif

