/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    RX.h
AUTHOR:  Barrett Edwards
CREATED: 13 July 2006

DESCRIPTION
	Helios IO. This file should contain the subroutines to Communicate with 
	 external devices and GUI


******************************************************************************/
#ifndef RX_H_
#define RX_H_

/* Includes -----------------------------------------------------------------*/
#include "SystemTypes.h"
#include "Packet.h"
#include "FrameTable.h"

/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/


/* =========================================================================*/
/* Receive Functions =======================================================*/

/*********************************************************************
 * Functional Group: Helper Functions
 * Definition:       
 * 
 *  1: InitMemory()
 *  
*/ 
void RX_InitMemory();



/*********************************************************************
 * Functional Group: General Receive
 * Definition:       
 * 
 * 	0:  processPacket();		
 *
*/ 
void RX_ProcessPacket			(HeliosCommHeader *hch, uint8* buffer);
	
void RX_processText				(HeliosCommHeader *hch, uint8* buffer);			
void RX_processPrimative		(HeliosCommHeader *hch, uint8* buffer);			
void RX_processState			(HeliosCommHeader *hch, uint8* buffer);
void RX_processCommand			(HeliosCommHeader *hch, uint8* buffer);
void RX_processImage			(HeliosCommHeader *hch, uint8* buffer);	
void RX_processCourse			(HeliosCommHeader *hch, uint8* buffer);			
void RX_processRegister			(HeliosCommHeader *hch, uint8* buffer);			
void RX_processDataTransfer		(HeliosCommHeader *hch, uint8* buffer);


#endif


