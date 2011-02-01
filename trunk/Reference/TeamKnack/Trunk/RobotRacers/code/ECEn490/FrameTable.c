/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    FrameTable.c
AUTHOR:  Barrett Edwards
CREATED: 21 Aug 2006

DESCRIPTION
	

******************************************************************************/


/* Includes -----------------------------------------------------------------*/
#include <plb_camera.h>

#include "FrameTable.h"
#include "InterruptControl.h"
#include "xparameters.h"
#include "MemAlloc.h"
#include "TX.h"
#include "State.h"
#include "Navigation.h"
#include "Vision.h"
#include "Timer.h"

/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Global Memory ------------------------------------------------------------*/
FrameTableEntry* FrameTable[NUM_FRAME_TABLE_ENTRIES];
static FrameTableEntry* fte_RecentFromHardware;

uint32 requestedoptions = 0;
uint8  requestready = FALSE;
uint32 requestedsize = 0;
int32  framesPerSecond_HW = 0;
uint8  sizerequestready = FALSE;
uint8 hsvSettingsChange = FALSE;


/* Code ---------------------------------------------------------------------*/



/* ==========================================================================*/
/* Code: Function Groups													 */
/* ==========================================================================*/
/*
 * Functional Groups:
 * 1: Helper Functions
 * 2: Frame Handling
 * 3: Real-Time Camera API Interface 
 */
  
  
  
  
 /*****************************************************************************
 * Functional Group: Helper Functions       
 * 	1: InitFrameTable()
 * 	2: PrintFrameTable()
 * 	3: StartCapture()
 * 	4: InterruptHandlerFrameTable()
*/   
void FT_InitFrameTable(){
	framesPerSecond_HW = 0;
	fte_RecentFromHardware = NULL;
	int i;
	
	//allocate memory for FrameTable
	for(i = 0 ; i < NUM_FRAME_TABLE_ENTRIES ; i++){	
		FrameTable[i] = (FrameTableEntry*) MemAlloc(sizeof(FrameTableEntry));
	}
	
	//allocate memory for the image data from camera
	for(i = 0 ; i < NUM_FRAME_TABLE_ENTRIES ; i++){	
		FrameTable[i]->id					= i;
		FrameTable[i]->owner				= FTE_OWNER_NOONE;
		FrameTable[i]->frameCount			= 0;
		
		FrameTable[i]->checkedOut 			= FALSE;
		FrameTable[i]->status				= FT_STATUS_EMPTY;
		FrameTable[i]->camStatus			= 0;
		FrameTable[i]->saveoptions			= 0;
		FrameTable[i]->transmitoptions		= 0;		
				
		FrameTable[i]->originalFrame		= (uint8*) MemCalloc(FRAME_WIDTH * FRAME_HEIGHT * 2);
		FrameTable[i]->hsvFrame				= (uint8*) MemCalloc(FRAME_WIDTH * FRAME_HEIGHT * 2); //H and S
		
		//add a little bit of an offset to the end of each of the MASK and GREYscale locations (size + 640)
		FrameTable[i]->maskFrameOrange		= (uint8*) MemCalloc(FRAME_WIDTH * FRAME_HEIGHT/8 + 640); //8 pixels per byte
		FrameTable[i]->maskFrameGreen		= (uint8*) MemCalloc(FRAME_WIDTH * FRAME_HEIGHT/8 + 640); //8 pixels per byte
		FrameTable[i]->greyscaleFrameOrange		= (uint8*) MemCalloc(FRAME_WIDTH * FRAME_HEIGHT + 640);   //1 byte per pixel
		FrameTable[i]->greyscaleFrameGreen		= (uint8*) MemCalloc(FRAME_WIDTH * FRAME_HEIGHT + 640);   //1 byte per pixel
		
		FrameTable[i]->colCountOrangeData	= (uint16*) MemCalloc(FRAME_WIDTH*2);				//this is a column sum for all columns (width of the image width)
		FrameTable[i]->colCountGreenData	= (uint16*) MemCalloc(FRAME_WIDTH*2);				//this is a column sum for all columns (width of the image width)

		FrameTable[i]->visiblePylonArray	= (visiblePylon*) MemCalloc(FRAME_WIDTH/2 * sizeof(visiblePylon));	//processed pylon data
		FrameTable[i]->numVisiblePylons		= 0;
	}
}//End: FT_InitFrameTable()


FrameTableEntry* currentHardwareFrame(){
	return FrameTable[0];
}
FrameTableEntry* lastHardwareFrameCompleted(){
	return fte_RecentFromHardware;
}



void FT_StartCapture(){
	FrameTable[0]->status							= FT_STATUS_ACQUIRING;
	StartFrameCapture(XPAR_PLB_CAM0_BASEADDR,	(Xuint32)FrameTable[0]->originalFrame, 
												(Xuint32)FrameTable[0]->hsvFrame,
												(Xuint32)FrameTable[0]->maskFrameOrange,
												(Xuint32)FrameTable[0]->maskFrameGreen,
												(Xuint32)FrameTable[0]->greyscaleFrameOrange,
												(Xuint32)FrameTable[0]->greyscaleFrameGreen,
												(Xuint32)FrameTable[0]->colCountOrangeData,
												(Xuint32)FrameTable[0]->colCountGreenData);
}




 
/*
 *	This handler is only called when the frame grabber hardware core has finnished reading in a frame to memory
 * The First entry in the FrameTable[0] is the frame that has been read in
 *
 * STEPS: 
 *  1: We should update the status of that FrameTableEntry
 *	2: Find the FrameTableEntry closest to the end of the table with the field status = FT_STATUS_COMPLETED
 *  3: Barrel Shift the FrameTable. everything below the lowest entry that was found in step 2
 *  4: Clear out the next frame to read into FrameTable[0] 
 *  5: Initiate the next frame capture
 *	#6: Search the FrameTable[] to find the next frame that needs histogram analysis
 *  #7: Initiate the histogram core on frame found in step 6
 *	8: Search the FrameTable[] to find the next frame that needs thresholding
 *  9: Initiate the thresholding core on the frame found in step 8
 *
 *	X: The connected components algorithm should then run on the frame that has all the preprocessing done on it
 *     This will not be initiated by the interrupt handler. but will be requested by a pooling loop or a pend on a semaphore
 *
 */ 
void FT_InterruptHandlerFrameTable(){
	static uint32 lastClockTicks = 0;
	static uint32 guiFrameCount = 0;
	
	DISABLE_INTERRUPTS();

	uint32 nowTicks = ClockTime();
	framesPerSecond_HW = XPAR_CPU_PPC405_CORE_CLOCK_FREQ_HZ / (nowTicks - lastClockTicks);
	lastClockTicks = nowTicks;

	/* STEP 1: Update status of recently completed frame */
	FrameTable[0]->checkedOut 		= FALSE;
	FrameTable[0]->owner	 		= FTE_OWNER_NOONE;
	FrameTable[0]->status 			= FT_STATUS_AVAILABLE;
	FrameTable[0]->camStatus		= CameraStatus(XPAR_PLB_CAM0_BASEADDR);
	FrameTable[0]->frameCount 		= guiFrameCount++;
	FrameTable[0]->saveoptions 		= (uint8)getFrameSaveOptions(XPAR_PLB_CAM0_BASEADDR);
	FrameTable[0]->transmitoptions	= 0;
	FrameTable[0]->numVisiblePylons = 0;
	
	fte_RecentFromHardware = FrameTable[0];

	// STEP 2: Find oldest frame that is not checked out
	int nextframe = NUM_FRAME_TABLE_ENTRIES -1;
	for( ; nextframe > 0 ; nextframe--)
		if(FrameTable[nextframe]->checkedOut == FALSE)
			break;
	

	// STEP 3: Barrel Shift the FrameTable. everything below the lowest entry that was found in step 2	
	FrameTableEntry* temp = FrameTable[nextframe];
	
	for( ; nextframe > 0 ; nextframe--)
		FrameTable[nextframe] = FrameTable[nextframe - 1];	
		
	FrameTable[0] = temp;
	
	
	/* STEP 4: Clear out the next frame to read into FrameTable[0] */
	FrameTable[0]->checkedOut 		= FALSE;
	FrameTable[0]->status			= FT_STATUS_EMPTY;
	FrameTable[0]->owner			= FTE_OWNER_NOONE;

	/* STEP 5: Perform requested opertion on the plb camera core */	
	if(requestready == TRUE){
		requestready = FALSE;
		setFrameSaveOptions(XPAR_PLB_CAM0_BASEADDR, requestedoptions);
		requestedoptions = 0;
	}
	
	if(sizerequestready == TRUE){
		sizerequestready = FALSE;
		setPLBBurstSize(XPAR_PLB_CAM0_BASEADDR, requestedsize);
		requestedsize = 0;
	}
	
	if(hsvSettingsChange == TRUE){
		hsvSettingsChange = FALSE;
		setHSVSettings();
	}
	/* STEP 5: Initiate the next frame capture */
	ConvReset(XPAR_PLB_CAM0_BASEADDR); //reset the convolution unit and FIFOs

	FT_StartCapture();
	acknowledgeCameraInterrupt(XPAR_PLB_CAM0_BASEADDR); //this must be sent AFTER FT_StartCapture


	RESTORE_INTERRUPTS(msr);	
}//End: FT_InterruptHandler_FrameGrabber()


/******************************************************************************
 * Functional Group: Frame Handling      
 * 	1: CheckOutFrame()
 * 	2: CheckInFrame()
*/ 

FrameTableEntry* FT_CheckOutFrame(uint8 owner){
	FrameTableEntry* fte = NULL;; 
	int index;	

	DISABLE_INTERRUPTS();
	for(index = 1 ; index < NUM_FRAME_TABLE_ENTRIES  ; index++){
		if(    FrameTable[index]->checkedOut	== FALSE
		    //&& FrameTable[index]->status		== FT_STATUS_AVAILABLE
			//&& FrameTable[index]->frameCount	!= 0)
			)
		{
			fte = FrameTable[index];
			fte->checkedOut = TRUE;		
			fte->owner      = owner;
			break;
		} else {
			//sendPacketFrameOwner(FrameTable[index]);
		}
	}	

	RESTORE_INTERRUPTS(msr);
	return fte;
} 

/*
//this prints to the screen who the owner of a particular frame is. Commented out to decrease code size by 52 bytes
void sendPacketFrameOwner(FrameTableEntry* fte){
	if (messagesOn()){
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,2,"f:");
		sendInt(fte->frameCount,DISPLAY_IN_GUI_NONEWLINE);
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,3," o:");
		switch(fte->owner){
			case FTE_OWNER_NOONE:
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,5,"NOONE");
				break;
			case FTE_OWNER_NAVIGATION:
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,3,"NAV");
				break;
			case FTE_OWNER_VISION:
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,3,"VIS");
				break;
			case FTE_OWNER_CRITICAL:
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,3,"CRI");
				break;
			case FTE_OWNER_MAIN:
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,4,"MAIN");
				break;
			case FTE_OWNER_INTERRUPT_HNDL:
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,7,"INTERPT");
				break;
			case FTE_OWNER_TX:
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,2,"TX");
				break;
			default:
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,3,"???");
		}
	}
}*/

void FT_CheckInFrame(FrameTableEntry* fte, uint8 owner){
	DISABLE_INTERRUPTS();
	if (fte->owner == owner){
		fte->checkedOut   = FALSE;
		fte->owner        = FTE_OWNER_NOONE;
		//fte->status 		= FT_STATUS_COMPLETED;
	}
	fte = NULL; //even if you aren't the owner, you lose the pointer
	RESTORE_INTERRUPTS(msr);
}

FrameTableEntry* 	FT_ChangeFrameOwner(FrameTableEntry* fte, uint8 curOwner, uint8 newOwner){
	DISABLE_INTERRUPTS();
	FrameTableEntry* fteReturn = NULL;
	if(fte->owner == curOwner){
		fte->owner = newOwner;
		fteReturn = fte;
	}
	RESTORE_INTERRUPTS(msr);
	return fteReturn; //returns NULL if it wasn't the 'curOwner's ownership
}

FrameTableEntry* 	FT_GetFrameFromOwner(uint8 owner){
	FrameTableEntry* fteReturn = NULL;
	DISABLE_INTERRUPTS();
	int index;
	for(index = 1 ; index < NUM_FRAME_TABLE_ENTRIES  ; index++){
		if(FrameTable[index]->owner == owner) break;
	}
	fteReturn = FrameTable[index];
	RESTORE_INTERRUPTS(msr);
	if (fteReturn==NULL) {
		if (messagesOn())
			SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,21,"COULDN'T CHANGE OWNER");
	}
	return fteReturn;
}

/******************************************************************************
 * Functional Group: Real-Time Camera API Interface        
 * 	1: RequestSaveOptions()
 * 	2: RequestPLBBurstSize()
*/ 
void FT_RequestSaveOptions(uint32 options){
	DISABLE_INTERRUPTS();
	requestedoptions = options;
	requestready     = TRUE;
	RESTORE_INTERRUPTS(msr);
}

void FT_RequestPLBBurstSize(uint32 size){
	DISABLE_INTERRUPTS();
	requestedsize 		= size;
	sizerequestready  = TRUE;
	RESTORE_INTERRUPTS(msr);
}

void FT_hsvSettingsChange(){
	DISABLE_INTERRUPTS();
	hsvSettingsChange  = TRUE;
	RESTORE_INTERRUPTS(msr);
}



