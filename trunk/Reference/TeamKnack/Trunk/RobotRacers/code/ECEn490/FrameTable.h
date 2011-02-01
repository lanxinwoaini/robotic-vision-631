/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    FrameTable.h
AUTHOR:  Barrett Edwards
CREATED: 21 Aug 2006

RESPONSIBILITY: 
	
******************************************************************************/
#ifndef FRAMETABLE_H_
#define FRAMETABLE_H_

/* Includes -----------------------------------------------------------------*/
#include "SystemTypes.h"
#include "Packet.h"
#include "Vision.h"


/* Defines ------------------------------------------------------------------*/
#define NUM_FRAME_TABLE_ENTRIES 				6 //an odd number creates blocky images

#define FT_STATUS_EMPTY						0x00
#define FT_STATUS_ACQUIRING					0x01
#define FT_STATUS_AVAILABLE					0x02
#define FT_STATUS_INUSE						0x04
#define FT_STATUS_COMPLETED 				0x08
#define FT_STATUS_NULL						0x10 // The memory that this object was pointing to has been claimed by another entry

#define FT_ORIGINAL_FRAME_TYPE_BW8			0
#define FT_ORIGINAL_FRAME_TYPE_RGB565		1
#define FT_ORIGINAL_FRAME_TYPE_HSV1688		2
#define FT_ORIGINAL_FRAME_TYPE_HSV888		3

#define FT_THRESHOLD_FRAME_TYPE_HUE			0

#define CONVOLUTION_HEIGHT					33
#define CONVOLUTION_WIDTH					3

#define FT_COL_MIN_THRESHOLD				35
#define FT_COL_MAX_THRESHOLD				180
#define FT_ORANGE_THRESHOLD_CONST			10//20//120//20
#define FT_GREEN_THRESHOLD_CONST			10//50//90//50

#define ORANGE_PYLON	1
#define GREEN_PYLON		0

enum FTE_OWNER {
	FTE_OWNER_NOONE				= 0,
	FTE_OWNER_NAVIGATION		= 1,
	FTE_OWNER_VISION			= 2, 
	FTE_OWNER_CRITICAL			= 3,
	FTE_OWNER_MAIN				= 4,
	FTE_OWNER_INTERRUPT_HNDL	= 5,
	FTE_OWNER_TX				= 6
};


/* Structs ------------------------------------------------------------------*/
typedef struct {
	uint8			id;
	uint8			owner; //this is who it belongs to
	uint32			frameCount;
	uint8			checkedOut;
	uint8			status;
	uint8			camStatus;
	uint8			saveoptions;
	uint8			transmitoptions;
	uint8*			originalFrame;
	uint8*			hsvFrame;
	uint8*			maskFrameOrange; 
	uint8*			maskFrameGreen; 
	uint8*			greyscaleFrameOrange; 
	uint8*			greyscaleFrameGreen; 
	uint16*			colCountOrangeData;		//this is a column sum for all columns (width of the image width)
	uint16*			colCountGreenData;		//this is a column sum for all columns (width of the image width)
	visiblePylon*	visiblePylonArray;	//this is PPC processed data
	uint8			numVisiblePylons;	//this is num of pylons in pylonTable
} FrameTableEntry;

/* External Memory-----------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/


/* ==========================================================================*/
/* Code: Function Groups																	  */
/* ==========================================================================*/
/*
 * Functional Groups:
 * 1: Helper Functions
 * 2: Frame Handling
 * 3: Real-Time Camera API Interface 
 */
  
 

 
 /*****************************************************************************
 * Functional Group: Helper Functions
 *  The FT_InitFrameTable() function is called at bootup time and is used to 
 *   malloc the memory that will be used to store the various frames and tables
 *   that will be pointed to by each entry in the FrameTable.
 *
 *  The StartCapture() function is called at the end of the 
 *   FT_InterruptHandlerFrameTable() interrupt handler and will configure the 
 *   camera core to write the various frames to those pointed to by the 
 *   FrameTable[0] entry. and it will then initiate a new frame capture.
 *
 *  The InterruptHandlerFrameTable() is the interrupt handler that is called 
 *   each time the camera core has finished captureing a frame from the camera.
 *
 * Definition:        
 * 	1: InitFrameTable()
 * 	2: PrintFrameTable()
 * 	3: StartCapture()
 * 	4: InterruptHandlerFrameTable()
*/ 
void FT_InitFrameTable(); 
void FT_StartCapture(); 
void FT_InterruptHandlerFrameTable();
//void sendPacketFrameOwner(FrameTableEntry* fte);




/******************************************************************************
 * Functional Group: Frame Handling
 *  These are the two main functions called by user code. The FT_CheckOutFrame()
 *   function will return the most recent captured frame to the user. It will 
 *   return null if no frames are currently available. FT_CheckOutFrame() returns 
 *   a pointer to a FrameTableEntry struct. This struct contains pointers to the 
 *   various types of frames that have been created(RGB,HSV,Segmented...). The user
 *   will typically call FT_CheckOutFrame() and perform some apllication specific 
 *   processing on the frames and then once finished, the user will then check that 
 *   FrameTableEntry back in so that it can be used again.
 *
 *
 * Definition:        
 * 	1: CheckOutFrame()
 * 	2: CheckInFrame()
*/ 
FrameTableEntry*	currentHardwareFrame();
FrameTableEntry*	lastHardwareFrameCompleted();
FrameTableEntry* 	FT_CheckOutFrame(uint8 owner);
FrameTableEntry* 	FT_ChangeFrameOwner(FrameTableEntry* fte, uint8 curOwner, uint8 newOwner);
void 				FT_CheckInFrame(FrameTableEntry* fte, uint8 owner);
FrameTableEntry* 	FT_GetFrameFromOwner(uint8 owner);

/******************************************************************************
 * Functional Group: Real-Time Camera API Interface
 *  Durring operation, the camera core will constantly be grabbing frames from
 *  the camera. While grabbing frames, some registers in the core should NOT be 
 *  modified. These two core settings can then only set in the time between when
 *  a frame has finished and before the nexty frame begins. So the following two 
 *  functions can be called at any time and the requested settings will be 
 *  written to the camera core when the FT_InterruptHandlerFrameTable() function
 *  is run.
 *
 * Definition:        
 * 	1: RequestSaveOptions()
 * 	2: RequestPLBBurstSize()
*/ 
void FT_RequestSaveOptions(uint32 options); 
void FT_RequestPLBBurstSize(uint32 size); 
void FT_hsvSettingsChange();




 
#endif
