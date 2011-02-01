/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    Packet.h
AUTHOR:  Barrett Edwards
CREATED: 20 July 2006

DESCRIPTION

******************************************************************************/
#ifndef PACKET_H_
#define PACKET_H_

/* Includes -----------------------------------------------------------------*/
#include "SystemTypes.h"
#include <stdio.h>


/* Defines ------------------------------------------------------------------*/
#define HEAD_INTRO1 0xFACA
#define HEAD_INTRO2 0xDE
#define _A(y,x) (((y)*320) + (x))
#define _B(y,x) (((y)*640) + (x))

#define PACKET_HEADER_SIZE		10

/* Structs ------------------------------------------------------------------*/
// NOTE: The headers MUST be an even number of bytes!
typedef struct {
	uint16 head_intro1;  //0xFACA
	uint8  head_intro2;  //0xDE //the last Hex byte (B1) isn't used, and is replaced with the pcount when transmitted
	uint8  pcount;		 //msb is request acknowledge bit
	uint8  type; 
	uint8  subtype;
	uint16 bufferSize;
	uint16 checkSum;
} __attribute__((__packed__)) HeliosCommHeader;

typedef struct  {
	short realid; //this number counts down to zero. If there are 10 elements, the first element will be '10'
				  //and the last will be '1'
	short height;
	short width;
	char color;
	short center;
	short middle;
	char reliability;
} blob;

enum PacketTypes {
	TEXT			= 0,
	PRIMATIVE		= 1, // not used right now
	STATE			= 2,
	COMMAND			= 3,
	IMAGE			= 4,
	COURSE			= 5,
	REGISTER		= 6,
	ACKNOWLEDGE		= 7,
	DATATRANSFER	= 8
};

enum PktSubType_Text {
	DISPLAY_IN_GUI_NEWLINE   = 0,
	DISPLAY_IN_GUI_NONEWLINE = 1 // not used right now
};

enum PktSubType_Primatives {
	PRIM_CHAR	= 0, //only send 1 byte
	PRIM_UCHAR	= 1, //only send 1 byte
	PRIM_INT	= 2, //send 4 bytes
	PRIM_UINT	= 3, //send 4 bytes
	PRIM_FLOAT	= 4, //send 4 bytes
	PRIM_HEX4   = 5, //send 4 bytes
	PRIM_HEX1   = 6  //send 4 bytes
};

enum PktSubType_DataTransfer {
	REQUEST_BIN		= 0, //Storage pointer for binary (4 bytes)
	BIN_INFO		= 1, //Binary file exists (1 byte) Binary file size (4 bytes)
	WRITE_DATA		= 2, //Binary data start loc (4 bytes) then 2 byte checksum of following data
	READ_DATA		= 3, //data request location (4 bytes)
	RUN_BOOTLOADER	= 4,
	READ_DATA_REPLY	= 5, //Data sent with 2 byte checksum at beginning
	WRITE_SUCCESS	= 6  //sent if the data checksum'd
};
enum PktSubType_Commands {
	ALLSTOP			= 0,
	STARTSTOP		= 1,
	SPEED_STEERING	= 2,
	REQUEST_STATE	= 3,
	SET_SESSION		= 4,
	SET_EMOTION		= 5,
	RESET_CAR		= 6,
	HEARTBEAT		= 7,
	MESSAGES_ON_OFF	= 8,
	VIDEO_ON_OFF	= 9,
	SET_VELOCITY_MUL= 10,
	SET_STEERING_TRM= 11,
	SET_COMM_CHANNEL= 12

};

enum PktSubType_Images { //this is the most significant nibble of the subtype
	//if a frame is 'captured', any previous frame will be released first
    CAPTURE_AND_TRANSMIT    = 0, //this captures and transmits an image (does not release)
    CAPTURE_CURRENT_FRAME   = 1,
    RELEASE_CURRENT_FRAME   = 2, //not needed to capture another frame, as by capturing, any
								 // previously captured frames are released
    TRANSMIT_FRAME          = 3, //sends the currently captured frame
    RETRANSMIT_SUB_FRAME    = 4, //first 2 data bytes are the row to send, next 2 bytes are the starting
								 // column to send. Will transmit from the starting column until
								 // it reaches the end of the row.
	IMAGE_RESOLUTION		= 5, //sent by the car to tell the GUI the resolution of image 2bytes w, 2bytes h
    IMAGE_DATA				= 6,
	PROCESSED_IMAGE			= 7,
    TRANSMIT_RECENT_CRITICAL= 8  //transmits the most recent critical capture (a frame that can be checked out and held for debug)
};

enum Pkt_Image_Types { //this is the least significant nibble of the subtype (0 if processed)
    RGB565      = 0, //5 bits RED, 6 bits GREEN, 5 bits BLUE
    SEGMENTED_1 = 1, //Binary Segmented image (0 or 1)									
    SEGMENTED_2 = 2, //Binary Segmented image (0 or 1)									
    SEGMENTED8_1= 3, //8-bit Segmented image (each bit represents a threshold match)	
    SEGMENTED8_2= 4, //8-bit Segmented image (each bit represents a threshold match)	
    HSV         = 5 //32-bit HSV (HHSV)					
};


enum PktSubType_Course {
	COURSE_SET					= 0,
	COURSE_SET_ACTIVE_WAYPOINT	= 1,
	COURSE_DELETE_WAYPOINT		= 2,
	COURSE_CHANGE_WAYPOINT		= 3
};

enum PktSubType_Register {
	SET_INT = 0,
    SET_FLOAT = 1,
    GET_INT = 2,
    GET_FLOAT = 3,
    TRANSMIT_INT = 4,
    TRANSMIT_FLOAT = 5,
	SET_DYNAMIC_HSV = 6,
	GET_DYNAMIC_HSV = 7,
	TRANSMIT_DYNAMIC_HSV = 8
};



/* External Memory-----------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/

void ReadInHeader(HeliosCommHeader *hch, uint8* buffer);
void LoadHeader(HeliosCommHeader *hch, uint8 type, uint8 subtype, uint16 bufferSize);
//creates a header that acknowledges receiving the transmission
void LoadHeader_Ack(HeliosCommHeader *hch, HeliosCommHeader *rxHeader);
uint16 GetChecksum(uint8* data, uint16 length);
uint16 GetChecksum_multi(uint8 numData, uint8** data, uint16* length);



#endif
