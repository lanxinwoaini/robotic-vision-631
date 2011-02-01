/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    TX.c
AUTHOR:  Barrett Edwards
CREATED: 10 Nov 2006

DESCRIPTION

******************************************************************************/

/* Includes -----------------------------------------------------------------*/
#include <plb_camera.h>

#include "SystemTypes.h"
#include "TX.h"
#include "State.h"
#include "Packet.h"
#include "Timer.h"
#include "FrameTable.h"

#include "Wireless.h"
#include "InterruptControl.h"



/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Global Memory ------------------------------------------------------------*/
/* Code ---------------------------------------------------------------------*/
int currentImageRow = 0;
int currentImageCol = 0;
FrameTableEntry* currentImageFTE = NULL;
FrameTableEntry* criticalImageFTE = NULL;
uint8* currentlySendingImageAddr = NULL;
int resendCurrentSectionOnly = FALSE;
static int NUM_PIXELS_TO_SEND_AT_A_TIME = 160;
static int BITS_PER_PIXEL = 16;
static uint8 CURRENT_IMAGE_TYPE = RGB565;
extern HeliosState hstate;

/* ==========================================================================*/
/* Receive Functions ========================================================*/

/******************************************************************************
 * Functional Group: Helper Functions
 * Definition:       
 * 
 *  1: InitMemory()
 *  
*/ 
			
void resetSession(){
	SendPacket(COMMAND, SET_SESSION, 0, 0);
}

void checkinCurrentImage(){
	setSendImage(FALSE); //stop sending image before checking in
	FT_CheckInFrame(currentImageFTE,FTE_OWNER_TX);
}
void setImageToSend()
{
	checkinCurrentImage(); //check in any other image checked out
	currentImageFTE = FT_CheckOutFrame(FTE_OWNER_TX);
}
void setCriticalImage(){
	FT_CheckInFrame(criticalImageFTE,FTE_OWNER_CRITICAL);
	criticalImageFTE = FT_CheckOutFrame(FTE_OWNER_CRITICAL);
}

//this takes control of the FrameTableEntry* and removes it from the other process's control
//WARNING WHEN USING THIS read above:
void setCriticalImage_alreadyCheckedOut(uint8 owner){
	FT_CheckInFrame(criticalImageFTE,FTE_OWNER_CRITICAL);//make sure we're not hanging onto any frames
	FrameTableEntry* fte = FT_GetFrameFromOwner(owner);
	criticalImageFTE = FT_ChangeFrameOwner(fte, owner, FTE_OWNER_CRITICAL);
}

void setSendingImage(uint8 imagetype){
	CURRENT_IMAGE_TYPE = imagetype;
	if (imagetype==RGB565){
		BITS_PER_PIXEL = 16;
		NUM_PIXELS_TO_SEND_AT_A_TIME = 160;
		currentlySendingImageAddr = currentImageFTE->originalFrame;
	} else if (imagetype==HSV){
		BITS_PER_PIXEL = 16;
		NUM_PIXELS_TO_SEND_AT_A_TIME = 160;
		currentlySendingImageAddr = currentImageFTE->hsvFrame;
	} else if (imagetype==SEGMENTED_1){
		BITS_PER_PIXEL = 1;
		NUM_PIXELS_TO_SEND_AT_A_TIME = 640;
		currentlySendingImageAddr = currentImageFTE->maskFrameOrange;
	} else if (imagetype==SEGMENTED_2){
		BITS_PER_PIXEL = 1;
		NUM_PIXELS_TO_SEND_AT_A_TIME = 640;
		currentlySendingImageAddr = currentImageFTE->maskFrameGreen;
	} else if (imagetype==SEGMENTED8_1){
		BITS_PER_PIXEL = 8;
		NUM_PIXELS_TO_SEND_AT_A_TIME = 320;
		currentlySendingImageAddr = currentImageFTE->greyscaleFrameOrange;
	} else if (imagetype==SEGMENTED8_2){
		BITS_PER_PIXEL = 8;
		NUM_PIXELS_TO_SEND_AT_A_TIME = 320;
		currentlySendingImageAddr = currentImageFTE->greyscaleFrameGreen;
	} else {
		BITS_PER_PIXEL = 16;
		NUM_PIXELS_TO_SEND_AT_A_TIME = 160;
		currentlySendingImageAddr = currentImageFTE->originalFrame;
	}
}

void SendImage(uint8 imagetype, int isCritical){
	//SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,1,"+");
	if (isCritical){
		if (criticalImageFTE == NULL){
			SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,30,"ERROR:NoCriticalImageAvailable");
			return; //there's nothing we can do, really, just return
		}
		FT_CheckInFrame(currentImageFTE,FTE_OWNER_TX);//checkin any currentImageFTE owned by TX
		currentImageFTE = criticalImageFTE; //don't change the ownership though
	} else {
		if(currentImageFTE == NULL){
			currentImageFTE = FT_CheckOutFrame(FTE_OWNER_TX);
			if (currentImageFTE==NULL){
				//SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,17,"ERROR:CamStatus->");
				//sendHex(CameraStatus(XPAR_PLB_CAM0_BASEADDR),DISPLAY_IN_GUI_NEWLINE);
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,26,"ERROR:ForceSendingBadImage");
				currentImageFTE = currentHardwareFrame();
			} else {
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,11,"SendingRAW:");
			}
			//return;
		}
	}
	//SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,6,"Frame:");
	//sendInt(currentImageFTE->frameCount,DISPLAY_IN_GUI_NEWLINE);
	//sendHex(CameraStatus(XPAR_PLB_CAM0_BASEADDR),DISPLAY_IN_GUI_NEWLINE);
	
	DISABLE_INTERRUPTS();

	currentImageRow = 0;
	currentImageCol = 0;
	setSendingImage(imagetype);
	resendCurrentSectionOnly = FALSE; //we want to send the full image
	int w = imageWidth();
	int h = imageHeight();
	int resolution = w << 16 | h;
	
	HeliosCommHeader hch;
	LoadHeader(&hch, IMAGE, IMAGE_RESOLUTION<<4 | CURRENT_IMAGE_TYPE, 4);
	WireSendDataFull((uint8*)&hch, sizeof(hch), (uint8*)&resolution, 4);
	if (messagesOn()){
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,11,"frameCount:");
		sendInt(currentImageFTE->frameCount,DISPLAY_IN_GUI_NEWLINE);
	}
	setSendImage(TRUE); //set the state to send the image over time
	ContinueSendingImage();
	RESTORE_INTERRUPTS(msr);
}

void ContinueSendingImage(){
	DISABLE_INTERRUPTS();
	if (!sendImage() || currentImageFTE==NULL) return;

	int w = imageWidth();
	int h = imageHeight();
	uint8* nextData = (uint8*)currentlySendingImageAddr + 
		((w*currentImageRow + currentImageCol)*BITS_PER_PIXEL)/8; //convert back to bytes
	int numPixelsToSend = (w - currentImageCol);
	if (numPixelsToSend > NUM_PIXELS_TO_SEND_AT_A_TIME) numPixelsToSend = NUM_PIXELS_TO_SEND_AT_A_TIME;
	int numBytesToSend = (numPixelsToSend * BITS_PER_PIXEL)/8;

	HeliosCommHeader hch;
	
	if ((getSendBufferFreeSpace()/2) > numBytesToSend + sizeof(hch) + 4 + 2 &&
		(getSendBufferSize()) < numBytesToSend + sizeof(hch) + 4 + 2){ //we need enough buffer space for all below
	//if (getSendBufferFreeSpace() > numBytesToSend + sizeof(hch) + 4){ //we need enough buffer space for all below
		int curPos = currentImageRow<<16 | currentImageCol;
		LoadHeader(&hch, IMAGE, IMAGE_DATA<<4 | CURRENT_IMAGE_TYPE, numBytesToSend + 4); //We're sending it all in 1 packet
		WireSendDataTri((uint8*)&hch, sizeof(hch),(uint8*)&curPos,4,nextData, numBytesToSend);

		//setup data for next send
		currentImageCol += numPixelsToSend;
		if (resendCurrentSectionOnly){ //we're done sending this section
			setSendImage(FALSE);
		} else if (currentImageCol >= w){ //we've finished this row
			currentImageCol = 0;
			currentImageRow++;
			if (currentImageRow >= h){ //we've finished the image
				setSendImage(FALSE);
				if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,27,"Finished Transmitting Image");
			}
		}
	}
	
	RESTORE_INTERRUPTS(msr);
}
void ResendPartOfImage(uint8 imagetype,int row, int col){
	if(currentImageFTE == NULL){
		if (messagesOn()) SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,22,"Cannot Send, ImageNULL");
		return;
	}
	DISABLE_INTERRUPTS();

	currentImageRow = row;
	currentImageCol = col;
	resendCurrentSectionOnly = TRUE; //we want to send the full image
	setSendingImage(imagetype);
	setSendImage(TRUE); //set the state to send the image over time

	ContinueSendingImage();
	RESTORE_INTERRUPTS(msr);
}

//acknowledge that we got the packet
int TX_acknowledgeRX(HeliosCommHeader *rxHeader, uint8* buffer){
	HeliosCommHeader hch;
	LoadHeader_Ack(&hch, rxHeader); //duplicate the packetNum and change type to ACKNOWLEDGE
	return WireSendDataFull((uint8*)&hch, sizeof(hch),0,0);	
}

int SendPacket(uint8 type, uint8 subtype, uint16 bufferSize, uint8* buffer){
	HeliosCommHeader hch;
	LoadHeader(&hch, type, subtype, bufferSize);
	return WireSendDataFull((uint8*)&hch, sizeof(hch),buffer, bufferSize);	
}
int SendPacket_lowPriority(uint8 type, uint8 subtype, uint16 bufferSize, uint8* buffer){
	HeliosCommHeader hch;
	LoadHeader(&hch, type, subtype, bufferSize);
	return WireSendDataFull_lowPriority((uint8*)&hch, sizeof(hch),buffer, bufferSize);	
}
void sendString(char string[]){

	int strlen = 0;
	int i = 0;

	while(string[i] != 0){
		strlen++;
	}
	strlen++;
	SendPacket(TEXT, DISPLAY_IN_GUI_NEWLINE, strlen, string);

}

void sendInt(int32 i, uint8 subtype){ //the subtype is like (DISPLAY_IN_GUI_NEWLINE)
	uint8 primativeData[5];
	int*  primativeNumPtr = (int*)&primativeData[1];
	primativeData[0] = (uint8)PRIM_INT; //the first byte of the data states what it is
	*primativeNumPtr=i; //set the next 4 bytes
	SendPacket(PRIMATIVE,subtype,5,primativeData);
}
void sendHex(uint32 i, uint8 subtype){
	uint8 primativeData[5];
	int*  primativeNumPtr = (int*)&primativeData[1];
	primativeData[0] = (uint8)PRIM_HEX4; //the first byte of the data states what it is
	*primativeNumPtr=i; //set the next 4 bytes
	SendPacket(PRIMATIVE,subtype,5,primativeData);
}
void sendOneHex(uint32 i, uint8 subtype){
	uint8 primativeData[5];
	int*  primativeNumPtr = (int*)&primativeData[1];
	primativeData[0] = (uint8)PRIM_HEX1; //the first byte of the data states what it is
	*primativeNumPtr=i; //set the next 4 bytes
	SendPacket(PRIMATIVE,subtype,5,primativeData);
}
void sendUint(uint32 i, uint8 subtype){
	uint8 primativeData[5];
	int*  primativeNumPtr = (int*)&primativeData[1];
	primativeData[0] = (uint8)PRIM_UINT; //the first byte of the data states what it is
	*primativeNumPtr=i; //set the next 4 bytes
	SendPacket(PRIMATIVE,subtype,5,primativeData);
}
void sendFloat(float f, uint8 subtype){
	uint8 primativeData[5];
	float*  primativeNumPtr = (float*)&primativeData[1];
	primativeData[0] = (uint8)PRIM_FLOAT; //the first byte of the data states what it is
	*primativeNumPtr=f; //set the next 4 bytes
	SendPacket(PRIMATIVE,subtype,5,primativeData);
}
void sendOther(uint32 i, uint8 dataType, uint8 type, uint8 subtype){
	uint8 primativeData[5];
	int*  primativeNumPtr = (int*)&primativeData[1];
	primativeData[0] = (uint8)dataType; //the first byte of the data states what it is
	*primativeNumPtr=i; //set the next 4 bytes
	SendPacket(type,subtype,5,primativeData);
}
