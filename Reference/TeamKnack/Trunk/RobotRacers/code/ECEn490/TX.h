/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    TX.h
AUTHOR:  Barrett Edwards
CREATED: 10 Nov 2006

DESCRIPTION
	
******************************************************************************/
#ifndef TX_H_
#define TX_H_

/* Includes -----------------------------------------------------------------*/
#include "SystemTypes.h"
#include "Packet.h"
#include "FrameTable.h"


/* Register IDs */

#define KP_REG 10
#define KD_REG 11
#define KI_REG 12

/* End Register IDs */


/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/

void SendState();
void SendImage(uint8 imagetype, int isCritical);
int  SendPacket(uint8 type, uint8 subtype, uint16 bufferSize, uint8* buffer);
int  SendPacket_lowPriority(uint8 type, uint8 subtype, uint16 bufferSize, uint8* buffer);
void ContinueSendingImage();
void ResendPartOfImage(uint8 imagetype,int row, int col);
void setImageToSend();
void setCriticalImage();
void setImageToSend_critical();
void setCriticalImage_alreadyCheckedOut(uint8 owner);
void checkinCurrentImage();
int  TX_acknowledgeRX(HeliosCommHeader *rxHeader, uint8* buffer);
void resetSession();
void sendInt(int32 i, uint8 subtype);
void sendUint(uint32 i, uint8 subtype);
void sendFloat(float f, uint8 subtype);
void sendHex(uint32 i, uint8 subtype);
void sendOneHex(uint32 i, uint8 subtype);
void sendOther(uint32 i, uint8 dataType, uint8 type, uint8 subtype);
void sendString(char string[]);


#endif
