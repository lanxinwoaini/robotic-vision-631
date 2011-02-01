/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    Packet.c
AUTHOR:  Barrett Edwards
CREATED: 10 Nov 2006

DESCRIPTION	

******************************************************************************/


/* Includes -----------------------------------------------------------------*/
#include "Packet.h"
#include "SystemTypes.h"


/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Global Memory ------------------------------------------------------------*/
/* Code ---------------------------------------------------------------------*/
void LoadHeader(HeliosCommHeader *hch, uint8 type, uint8 subtype, uint16 bufferSize){
	static unsigned short packetcount = 0;

	hch->head_intro1	= HEAD_INTRO1;
	hch->head_intro2	= HEAD_INTRO2;
	hch->pcount		= (uint8)(packetcount++)&0x7F; //the msb is used to signify a request for acknowledgement
	hch->type 		= type;
	hch->subtype	= subtype;
	hch->bufferSize	= bufferSize > 0? bufferSize+2 : 0;	//increment by 2 for extra checksum at the end of the databuffer
	hch->checkSum	= GetChecksum((uint8*)hch,PACKET_HEADER_SIZE-2); //checksum up to the hch->checkSum
}
void LoadHeader_Ack(HeliosCommHeader *hch, HeliosCommHeader *rxHeader){
	hch->head_intro1	= HEAD_INTRO1;
	hch->head_intro2	= HEAD_INTRO2;
	hch->pcount		= (uint8)rxHeader->pcount & 0x7F; //the msb is used to signify a request for acknowledgement
	hch->type 		= ACKNOWLEDGE;
	hch->subtype	= 0;
	hch->bufferSize	= 0;
	hch->checkSum	= GetChecksum((uint8*)hch,PACKET_HEADER_SIZE-2); //checksum up to the hch->checkSum
}

void ReadInHeader(HeliosCommHeader *hch, uint8* buffer){
	hch->head_intro1	= buffer[0]  << 8 | buffer[1];
	hch->head_intro2	= buffer[2];
	hch->pcount		= buffer[3];
	hch->type		= buffer[4];
	hch->subtype	= buffer[5];	
	hch->bufferSize	= buffer[6]  << 8 | buffer[7];
	hch->checkSum = GetChecksum((uint8*)buffer,PACKET_HEADER_SIZE); //calculate the checksum of the header
}

//calculate the checksum of the data
uint16 GetChecksum( uint8* data, uint16 length){
	uint16 totalLength = 0;
	uint16 checksum = 0;
	uint8* endOfData;
	uint8* checksumPtr;

	endOfData = (data + length);
	checksumPtr = data; //begin at the checksum data
	for (; checksumPtr < endOfData; checksumPtr++){
        checksum += (totalLength % 2 ? ((uint16)*checksumPtr) : ((uint16)*checksumPtr) << 8);
		totalLength++;
	}
	checksum = -checksum;
	return (totalLength%2) ? ((checksum & 0x00ff) << 8) | ((checksum & 0xff00) >> 8) : checksum;
}

uint16 GetChecksum_multi(uint8 numData, uint8** data, uint16* length){
	uint8 i;
	uint16 totalLength = 0;
	uint16 checksum = 0;
	uint8* endOfData;
	uint8* checksumPtr;
	for (i = 0; i < numData; i++){//for each databuffer sent to checksum
		endOfData = (data[i] + length[i]);
		checksumPtr = data[i]; //begin at the checksum data
		for (; checksumPtr < endOfData; checksumPtr++){
            checksum += (totalLength % 2 ? ((uint16)*checksumPtr) : ((uint16)*checksumPtr) << 8);
			totalLength++;
		}
	}
	checksum = -checksum;
	return (totalLength%2) ? ((checksum & 0x00ff) << 8) | ((checksum & 0xff00) >> 8) : checksum;
}


