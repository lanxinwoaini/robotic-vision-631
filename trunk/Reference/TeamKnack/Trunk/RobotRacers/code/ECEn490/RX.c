/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    RX.c
AUTHOR:  Barrett Edwards
CREATED: 13 July 2006

DESCRIPTION

******************************************************************************/

/* Includes -----------------------------------------------------------------*/
#include <plb_camera.h>

#include "RX.h"
#include "Timer.h"
#include "State.h"
#include "MemAlloc.h"
#include "Vision.h"
#include "TX.h"
#include "ServoControl.h"
#include "Registers.h"
#include "bootloader.h"
#include "InterruptControl.h"
#include "navigation.h"

extern int encoderValue;
extern float desiredVelocityPID;
extern int desiredAnglePID;
extern int velocityMultiple;
extern int steeringTrim;
extern int32* courseAngles;
extern int32* courseAngleTrims;
extern int32* courseDistances;
extern int numAngles;
extern int absoluteAngle;
extern int unitLength;
extern HeliosState hstate;
extern int TURN_SPEED;

/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Global Memory ------------------------------------------------------------*/
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
void HIO_InitMemory(){}



/******************************************************************************
 * Functional Group: General Receive
 * Definition:       
 * 
 *  1: ProcessPacket()
 *
*/ 
void RX_ProcessPacket(HeliosCommHeader *hch, uint8* buffer){	

	//check if it requested an acknowledgement:
	if (hch->pcount & 0x80){
		TX_acknowledgeRX(hch, buffer); //acknowledge we got this packet
	}
	switch(hch->type){
	case TEXT:			RX_processText			(hch,buffer);	break;
	case PRIMATIVE:		RX_processPrimative		(hch,buffer);	break;
	case STATE:			RX_processState			(hch,buffer);	break;
	case COMMAND:		RX_processCommand		(hch,buffer);	break;	
	case IMAGE:			RX_processImage			(hch,buffer);	break;
	case COURSE:		RX_processCourse		(hch,buffer);	break;
	case REGISTER:		RX_processRegister		(hch,buffer);	break;
	case DATATRANSFER:	RX_processDataTransfer	(hch,buffer);	break;
	default: break;
	}
}




void RX_processText(HeliosCommHeader *hch, uint8* buffer){}			

void RX_processPrimative(HeliosCommHeader *hch, uint8* buffer){}			

void RX_processState(HeliosCommHeader *hch, uint8* buffer){}


void RX_processCommand(HeliosCommHeader *hch, uint8* buffer){	
	uint8 prevSession;

	// Interrupts are already disabled
	switch(hch->subtype){
	default:
	case  ALLSTOP:			setRunning(FALSE);	
							setSendImage(FALSE);
							setBootloadReady(FALSE);
							resetHeartbeatCountdown(1); //kill it on the next tick
							//SendPacket(COMMAND,ALLSTOP,0,0); //inform the GUI we got it //deprecated
							if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,10,"AllStopped");
							break; 
	case  STARTSTOP:		if (buffer[3]) //START signal
							{
								setRunning(TRUE);	
								if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,7,"Started");
							} else { //STOP signal
								setRunning(FALSE);	
								resetHeartbeatCountdown(1); //kill it on the next tick
								if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,7,"Stopped");
								//navStateReset();
								//if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,11,"State Reset");
							}
							break; 
	case  SPEED_STEERING:	//shift all the way left and then back to correctly sign-extend ints
							if (heartbeatCountdown()>0) //only drive if the heartbeat is being received
							{
								DISABLE_INTERRUPTS();
								int desiredVelocity = ((buffer[0] << 24 | buffer[1] << 16)>>16);
								int desiredAngle = ((buffer[2] << 24 | buffer[3] << 16)>>16);
								
									desiredVelocityPID = (float)desiredVelocity / 20.0;
									desiredAnglePID = desiredAngle;
								RESTORE_INTERRUPTS(msr);
							}
							break;
	case  SET_SESSION:		
							prevSession = session();
							setSession(buffer[0]);
							if (buffer[0] == SN_RACE){
								FT_RequestSaveOptions(CAM_SAVE_OPTIONS_COL_COUNT | CAM_SAVE_OPTIONS_MASK);
								if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,26,"HSV,RGB,GREYSCALE disabled");
							} else if (prevSession == SN_RACE){
								FT_RequestSaveOptions(CAM_SAVE_OPTIONS_COL_COUNT | CAM_SAVE_OPTIONS_GREYSCALE | CAM_SAVE_OPTIONS_MASK | CAM_SAVE_OPTIONS_HSV | CAM_SAVE_OPTIONS_ORG);
								if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,25,"HSV,RGB,GREYSCALE enabled");
							}
							break;
	case  SET_EMOTION:		
							break;
	case  RESET_CAR:		
							break;
	case  HEARTBEAT:		resetHeartbeatCountdown(310);//will have chance to receive 2 ticks w/out dying (being sent at 150ms)
							break;
	case  MESSAGES_ON_OFF:	setMessagesOn(buffer[3]);
							//if (messagesOn()){ //always inform whether messages are set on or off
								if (buffer[3]){
									SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,10,"MessagesOn");
								} else 
								{
									SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,11,"MessagesOff");
								}
							//}
							break;
	case  VIDEO_ON_OFF:		if (buffer[3]){
								setSendingVideo(TRUE);
								if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,7,"VideoOn");
							}
							else {
								setSendingVideo(FALSE);
								if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,8,"VideoOff");
							}
							break;
	case  SET_VELOCITY_MUL:	velocityMultiple = (buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]); //just need least significant bits
							
							if (messagesOn()){ //acknowledge:
								SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,9,"VelMul=1/");
								sendInt(velocityMultiple,DISPLAY_IN_GUI_NEWLINE);
							}
							break;
	case  SET_STEERING_TRM:	steeringTrim = (buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]); //just need least significant bits
							
							if (messagesOn()){ //acknowledge:
								SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,10,"SteerTrim=");
								sendInt(steeringTrim,DISPLAY_IN_GUI_NEWLINE);
							}
							break;
	case  SET_COMM_CHANNEL:	setGuiCommChannel(buffer[0]);
							break;
	}
}

void RX_processImage(HeliosCommHeader *hch, uint8* buffer){
	uint8 imagetype = hch->subtype&0xF;//hch->subtype&0xF is the imagetype (ie RGB565)
	switch(hch->subtype>>4){
		case CAPTURE_AND_TRANSMIT:	//this captures and transmits an image
			if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,10,"hold on...");
			setImageToSend();
			SendImage(imagetype,FALSE);
			if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,25,"TransmittingCapturedImage");
			break;
		case CAPTURE_CURRENT_FRAME:
			setImageToSend();
			if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,13,"ImageCaptured");
			break;
		case RELEASE_CURRENT_FRAME:	//not needed to capture another frame, as by capturing, any
									// previously captured frames are released
			checkinCurrentImage();
			if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,13,"ImageReleased");
			break;
		case TRANSMIT_FRAME:		//sends the currently captured frame
			SendImage(imagetype,FALSE);
			if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,17,"TransmittingImage");
			break;
		case RETRANSMIT_SUB_FRAME:	//next 2 bytes are the row to send, next 2 bytes are the starting
									// column to send. Will transmit from the starting column until
									// it reaches the end of the row.
			ResendPartOfImage(imagetype,buffer[0] << 8 | buffer[1], buffer[2] << 8 | buffer[3]);
			if (messagesOn()){ //acknowledge:
				SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,10,"Resending:");
				sendInt(buffer[0] << 8 | buffer[1],DISPLAY_IN_GUI_NONEWLINE);
				SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,1,",");
				sendInt(buffer[2] << 8 | buffer[3],DISPLAY_IN_GUI_NEWLINE);
			}
			break;
		case TRANSMIT_RECENT_CRITICAL: //send the current critical image
			if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,20,"TransmittingCritical");
			SendImage(imagetype,TRUE);
			break;
	}
}	

void RX_processCourse(HeliosCommHeader *hch, uint8* buffer)
{
	int* data = (int*)buffer;
	int i;
	numAngles = ((hch->bufferSize) - 8) / 4 / 3; //3 ints per angle sent
	absoluteAngle = *data;
	data++;
	unitLength = *data;
	data++;
	for(i = 0; i < numAngles; i ++){
		courseAngles[i] = *data;
		data++;
		courseAngleTrims[i] = *data;
		data++;
		courseDistances[i] = *data;
		data++;
	}
	
	SendPacket(COURSE, 0, 0, 0);
	navStateReset();
	if( setCourseData() )
	{
		SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,21,"Error loading Course!");
	}
	else
	{
		SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,18,"Course Data Loaded");
	}
}			



void RX_processDataTransfer(HeliosCommHeader *hch, uint8* buffer){
	uint32 bootBaseAddress;

	switch(hch->subtype){

		case REQUEST_BIN:	//Storage pointer for binary (4 bytes)
			bootBaseAddress = BOOT_IMAGE_BASEADDR;
			SendPacket(DATATRANSFER,REQUEST_BIN,4,(uint8*)&bootBaseAddress);
			break;
		case BIN_INFO:		//Binary file exists (1 byte) Binary file size (4 bytes)
			;
			break;
		case WRITE_DATA:	//Binary data start loc (4 bytes) then 2 byte checksum of following data
			SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,1,".");

			//acknowledge success of data's checksum
			sendOther((uint32)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]),TRUE,DATATRANSFER,WRITE_SUCCESS);

			//now copy the data to memory
			int i = 0;
			int dataLength = hch->bufferSize - 4;
			uint8* dataSource = buffer + 4; //start after the checksum (at true data)
			uint8* dataDest = (uint8*)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]);
			for (; i < dataLength; i++){
				*dataDest = *dataSource;
				dataDest++;
				dataSource++;
			}
			break;
		case READ_DATA:		//Binary data start loc (4 bytes)
			//SendPacket_checksum(DATATRANSFER,READ_DATA,512,(uint8*)BOOT_IMAGE_BASEADDR);
			break;
		case RUN_BOOTLOADER://loads the program
			setBootloadReady(TRUE);
			break;
	}
}

void RX_processRegister(HeliosCommHeader *hch, uint8* buffer){
	
	uint16 regId = (((buffer[0] << 8) & 0xFFFF0000) | (buffer[1] & 0x0000FFFF)); 
	uint8 subtype = hch->subtype;
	
	if(subtype == GET_INT || subtype == GET_FLOAT){ // GUI asking 
		TransmitRegisterData(regId, subtype, buffer);
	}
	else if(subtype == SET_INT || subtype == SET_FLOAT){  // GUI wants to set a register
		SetRegister(regId, subtype, buffer);
		if (messagesOn()){ //acknowledge:
			SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,6,"SetReg");
			sendInt(regId,DISPLAY_IN_GUI_NEWLINE);
		}
	}
	else if(subtype == SET_DYNAMIC_HSV){//GUI wants to set a huge dynamichsv packet
		setHSVDynamicSettings(buffer);
		if (messagesOn()){ //acknowledge:
			SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,14,"SetDynamicHSV");
		}
	}
	else if(subtype == GET_DYNAMIC_HSV){ //GUI wants to receive a huge dynamichsv packet
		transmitDynamicHSV();
	}
}			
