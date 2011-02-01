/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    InterruptHandlers.c
AUTHOR:  Barrett Edwards
CREATED: 26 Sep 2006

DESCRIPTION

******************************************************************************/

/* Includes -----------------------------------------------------------------*/
#include <xtime_l.h>

#include "InterruptHandlers.h"
#include "InterruptControl.h"
#include "MemAlloc.h"
#include "RX.h"
#include "TX.h"
#include "Packet.h"
#include "HeliosIO.h"
#include "State.h"
#include "ServoControl.h"
#include "Wireless.h"
#include "pidControl.h"
#include "navigation.h"
#include "bootloader.h"
#include "FrameTable.h"
#include "Timer.h"

/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Global Memory ------------------------------------------------------------*/
int currentTime = 0;

int* encoderPtr = (int*)0x7a400000;

extern int steeringTrim;
extern int encoderValue;
extern TransmitState transmitState;

int actualPITFreq = 0;

/* Code ---------------------------------------------------------------------*/



/* ==========================================================================*/
/* Code: Function Groups											                    */
/* ==========================================================================*/
/*
 * Functional Groups:
 * 1: Helper Functions
 * 2: Interrupt Handlers
*/


/******************************************************************************
 * Functional Group: Helper Functions
 * Definition:       
 * 
 *  1: InitMemory()
 *
*/ 
void IH_InitMemory(){
}





/********************************************************************
 * Functional Group: Interrupt Handlers
 * Definition:       
 * 
 *  1: TimerHandler()
 *  2: InterruptHandlerSerial()
 *  3: InterruptHandlerWireless()
 *  4: InterruptHandlerUSB() 
 *  5: InterruptHandlerSerialTTL()
*/ 

extern float desiredVelocityPID;
extern int desiredAnglePID;
extern PID pid;
extern int encoderValue;

extern int finishedInit;

int numMillisecondsToRun = 0;
float numMetersToRun = 0;

int tmpData = 0;
void IH_TimerHandler(void *ptr){
	currentTime++;

	WirelessInterruptHandler(); //run to make sure
	//flash a little light every second:
	if (!(currentTime%100)){
		HeliosSetLED1(!HeliosReadLED1());
		tmpData++;
	}

	//turn off the car if the hearbeat isn't received:
	if (heartbeatCountdownTick() == -1){ //it JUST turned to 0
		//stop car
		setRunning(FALSE);
		RCSetSteering(steeringTrim);
		desiredVelocityPID = 0;
	}

	//image sending stuff:

	if (currentTime % 3 == 0 && sendImage()) ContinueSendingImage();
	
	//send state data to the GUI:
	

	//update the encoder's value ... this needs to be run before nav code
	encoderValue = *((int*)ENCODER_BASE_ADDR);
	
	/**** Do not delete ****/
	
	uint32 nowTicks = ClockTime();
	actualPITFreq = XPAR_CPU_PPC405_CORE_CLOCK_FREQ_HZ / (nowTicks - pid.lastClockTicks);
	pid.lastClockTicks = nowTicks;


	if(session() == SN_DISABLED){
		if(numMillisecondsToRun > 0){
			
			updateVelocityOutput(actualPITFreq);
			numMillisecondsToRun--;
		}
		else if(pid.distanceMode == STOP_AT_DESIRED_ENCODER){
			updateVelocityOutput(actualPITFreq);
		}
		else{
			
			desiredVelocityPID = 0.0f;
			updateVelocityOutput(actualPITFreq);
		}
	}
	else{
		updateVelocityOutput(actualPITFreq);
	}
	updateSteeringAngle();

	/**** End do not delete ****/

	if (!(currentTime%100) && bootloadReady()){ //only check once a second if user has clicked on "load bootloader"
		bootload();
		setBootloadReady(FALSE); //if it got here then there were problems, so disable the bootloader
	}
	//Vision Post Processing

	
	if(currentTime % GUI_MOD_UPDATES == 0 && finishedInit == TRUE){
		updateGUI();
	}

	XTime_PITClearInterrupt(); // Clear the PIT's interrupt bit
}


