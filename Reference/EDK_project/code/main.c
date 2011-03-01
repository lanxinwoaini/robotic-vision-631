/**
\file
\author Barrett Edwards

FILE:				main.c
AUTHOR:				Barrett Edwards
CREATED:			25 Sep 2006
PLATFORM:			Helios Computing Platform
FRAMEWORK:			Fundamental Software Framework
COMPANION FILES:	Header.c	
DESCRIPTION:		

CHANGE LOG
*/ 


/* Includes -----------------------------------------------------------------*/
#ifdef SIMULATOR
#include <cv.h>
#include <highgui.h>
#endif

#include <xcache_l.h>
#include <xtime_l.h>
#include <xgpio_l.h>
#include <xexception_l.h>
#include <xintc_l.h>

#include <plb_vision.h>
#include <USB_IO.h>

#include "Header.h"
#include "Timer.h"
#include "init.h"
#include "FrameTable.h"
#include "State.h"
#include "mpmc_mem_test_example.h"
/* Macros ------------------------------------------------------------------*/
/* Enumerations -------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* Global Memory  -----------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Global Memory ------------------------------------------------------------*/


/** The entry point for the Helios Power Software
	This function is responsible for initializing the Helios Computing platform
	and excuting the while(1) loop that creates a Round-Robin with Interrupts
	style of embedded computing. 

	Initialization of the Helios Computing Platform consists of three main 
	tasks. The first is to initialize and configure the FPGA / PowerPC 
	computing hardware so that they can actually function. And the second is to
	initialize the software components of the system so that the code will run.
	And the thrid is to configure any external devices (i.e. camera).
*/
int main(void)
{       
	Init(); //Initialize the computing platform 
	ClockTimer ct;
	/* Processing steps of the mainloop:
	-# Call the LoopScheduler
	-# Checkout an FTE
		- Perform background tasks
		- {#OR{#
		- Process newly acquired image data
			- Transmitt brief FrameTableEntry data to the GUI
			- Accurately determine camera core list counts
			- Transmitt raw video to GUI
			- Perform application specific image processing algorithms
			- Transmitt Entire FrameTableEntry to the GUI
			- Check in processed FTE 		
	-# Send State Registers to GUI (if it is time to do so)
	-# General system upkeep
	-# Handle received ISR data
	*/
	FrameTableEntry* fte;
	/*	MAIN STEP: Call the LoopScheduler
		The LoopScheduler is a framework that provides the ability to schedule
		events at periodic time intervals
	*/
	Xuint32 startTime, stop1, stop2;
	while(1)
	{
	LoopScheduler();
	/*	MAIN STEP: Checkout an FTE
		As this is an embedded system designed for image processing, the main 
		work to be done is the analysis of images acquired from an onboard 
		camera. The mainloop basically just spins and spins and waits until
		there is a new image to process. When there is no new image to process,
		the FT_CheckOutFrame() function will return NULL. But when there is a
		new image to process, it will return an FTE pointer.
	*/
	fte = FT_CheckOutFrame();
	/*	MAIN STEP: Perform background tasks 
		Only perform this stuff in the absence of any real work to do.
	*/
	if(fte == NULL)
	{
		// STEP : Misc Requests, stuff to do if I don't have a valid frame (yet)	
	}


	/*	MAIN STEP: Process newly acquired image data
		If the FTE is not NULL, then we have some new image data to analyze.
	*/
	if(fte != NULL)
	{	
		// STEP: Transmitt brief FrameTableEntry data to the GUI
		FrameTableEntrySmall fte_small = { fte->id, fte->ms, fte->frameCount, fte->saveoptions };
		TX_SendPacket(PKT_FRAME_TABLE_ENTRY_SMALL, sizeof(FrameTableEntrySmall), (uint8*) &fte_small);
		// STEP: Transmitt raw video to GUI
		if(rdchar(C_SENDING_VIDEO))
		  {
			TX_SendImage(fte,TRUE);
		  }
		/*	STEP: Perform application specific image processing algorithms */
		if(rdchar(C_RUNNING))
		{					
			/*xil_printf("Running memory test...\r\n");
			int TotalErrors = MpmcMemTestExample(XPAR_MPMC_0_DEVICE_ID);
			if (TotalErrors) {
			  xil_printf("\r\n### Program finished with errors ###\r\n");
			} else {
			  xil_printf("\r\n### Program finished successfully ###\r\n");
			}*/
			//****************YOUR "GO" CODE HERE....*****************
		}	
		// STEP: Transmitt Entire FrameTableEntry to the GUI
		TX_SendPacket(PKT_FRAME_TABLE_ENTRY_FULL, sizeof(FrameTableEntry), (uint8*) fte);
		// STEP: Check in processed FTE 
		FT_CheckInFrame(fte);
	} //end if FTE != NULL
	// MAIN STEP: Send State Registers to GUI (if it is time to do so)
	wrhex(X_CAM0_STATUS, CameraStatus(camera0.base_address));
	if(testchar(C_REQ_STATE_DATA))
		if(rdchar(C_SENDING_STATE))
			TX_SendState();	
	/*	MAIN STEP: General system upkeep
		If the USB is connected then lets use it, if not then switch to 
		backup channel (serial, wireless...)
	*/
	if(USB_Connected())	wrchar(C_GUI_COMM_CHANNEL, CHAN_USB);				
	else				wrchar(C_GUI_COMM_CHANNEL, rdchar(C_GUI_COMM_CHANNEL_BACKUP));	
	
	/** MAIN STEP: Handle received ISR data
		Handle ISR data here. We do this here in the main loop instead of in 
		the actual ISR because it is a bad idea to handle incomming data inside
		an ISR. 
	*/
	ISR_HandleAll();
	
	}
}
