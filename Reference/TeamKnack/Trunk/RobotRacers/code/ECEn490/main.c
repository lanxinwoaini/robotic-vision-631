/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    main.c
AUTHOR:  Barrett Edwards
CREATED: 25 Sep 2006

DESCRIPTION


******************************************************************************/

/* Includes -----------------------------------------------------------------*/
#include "Timer.h"
#include "State.h"
#include "Init.h"
#include "FrameTable.h"
#include "HeliosIO.h"
#include "Packet.h"
#include "TX.h"
#include <xparameters.h>
#include "ServoControl.h"
#include "Wireless.h"
#include "Vision.h"
#include <xuartlite_l.h>
#include <plb_camera.h>

/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/


/* Global Memory ------------------------------------------------------------*/

int encoderValue	 = 0;
int desiredSpeed     = 0;
int desiredTurn      = 0;
int velocityMultiple = 8; //this is the amount that the speed is DIVIDED by (ie 100/velocityMultiple = max speed)
int steeringTrim     = 0; //this is added to the steering

//HEY!!!!! README!!!
//if you add any global variables, when you compile make sure you rebuild the linker script in Xilinx
//otherwise the variables will be put in the SDRAM instead of the BRAMs


int finishedInit = FALSE;
/* Code ---------------------------------------------------------------------*/




int main(void){

	MainInit();	

	RCSetThrottle(0); 
	RCSetSteering(20);
	usleep(300);
	RCSetSteering(-20);
	usleep(300);
	RCSetSteering(0);


	SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,27,"System(1) Startup Complete!");
	//SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,26,"PROM image 1 April 6:45 PM");
	if (!messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,11,"MessagesOff");

	
	resetSession();
	setSession(SN_DISABLED);

	finishedInit = TRUE;


	while(1) {
		WirelessReceiveParser();//call the wireless method (with interrupts enabled)
		NAV_Process(); //this also runs/calls the nav code
	}// END: while(1);
	
	return 0;
}



