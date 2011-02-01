/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    Init.c
AUTHOR:  Barrett Edwards
CREATED: 25 Sep 2006

DESCRIPTION


******************************************************************************/


/* Includes -----------------------------------------------------------------*/
#include <stdio.h>

#include <xtime_l.h>
#include <xintc_l.h>
#include <xexception_l.h>
#include <xcache_l.h>
#include <plb_camera.h>
#include <xuartlite_l.h>

#include "xparameters.h"

#include "Init.h"
#include "MemAlloc.h"
#include "Timer.h"
#include "TX.h"
#include "FrameTable.h"
#include "State.h"
#include "InterruptHandlers.h"
#include "HeliosIO.h"
#include "ServoControl.h"
#include "xgpio_l.h" 
#include "Wireless.h"
#include "Registers.h"
#include "pidControl.h"
#include "navigation.h"
#include "Vision.h"

/* Defines ------------------------------------------------------------------*/

#define CPU_CLOCK_FREQ  XPAR_CPU_PPC405_CORE_CLOCK_FREQ_HZ
#define SYSTEM_TICK_FREQ  100      // 100 - Generate a 100 Hz tick interrupt


// MEMORY DEFINITIONS
#define SDRAM_ADDR ((uint8 *)XPAR_PLB_SDRAM_0_BASEADDR)
#define SDRAM_SIZE (XPAR_PLB_SDRAM_0_HIGHADDR-XPAR_PLB_SDRAM_0_BASEADDR+1)
#define SDRAM_CACHE_REGION 0x80000000



#define BRAM_ADDR ((uint8 *)XPAR_OPB_BRAM_IF_CNTLR_0_BASEADDR)
#define BRAM_SIZE (XPAR_OPB_BRAM_IF_CNTLR_0_HIGHADDR-\
                   XPAR_OPB_BRAM_IF_CNTLR_0_BASEADDR+1)
#define BRAM_CACHE_REGION 0x00000001


#define I_CACHE_REGION (BRAM_CACHE_REGION | SDRAM_CACHE_REGION)
#define D_CACHE_REGION (SDRAM_CACHE_REGION | BRAM_CACHE_REGION)


/* Structs ------------------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Global Memory ------------------------------------------------------------*/
/* Code ---------------------------------------------------------------------*/





/******************************************************************************
 * MainInit
 *  This function should call inorder all the init functions for the various
 *  parts of the system. For example, if a .c file has some memory pointers that
 *  need to be malloc'd then call the memory init function here.
 *
 */
void MainInit(){
	InitializeMemory(); //this needs to be called first

	WirelessInit();
	StateInit();
	
	FT_InitFrameTable();
	SetupCamera();
	initHSVSettings();
	FT_StartCapture();
	ServoInit(RC_STR_SERVO);
	ServoInit(RC_VEL_SERVO);
	initPID();
	initRegisters();
	
	navigationMemInit();	
	
	InitializePIT();
	InitializeInterrupts();
	XExc_mEnableExceptions(XEXC_NON_CRITICAL); // Processor resets with interrupts disabled so Enable interrupts
}





/******************************************************************************
 * InitializePIT
 *  The Programmable-Interval Timer (PIT)	is an interrupt that occurs at a 
 *   preset interval of time
 *
 */
void InitializePIT(){
	XTime_PITSetInterval(CPU_CLOCK_FREQ / SYSTEM_TICK_FREQ);	// Make it go off SYSTEM_TICK_FREQ per second
	XTime_PITEnableAutoReload();
	XTime_PITEnableInterrupt(); 										// Enable CPU's PIT interrupt feature
}





/******************************************************************************
 * Initialize Interrupts
 *	STEPS:
 *  1: Enable interrupt on all uartlite cores. 
 *     This must be done BEFORE the uartlite interrupt is registerd with the PIC.
 *     Or else the interrupt line will ALWAYS be on.
 *  2: Initialize table and register each interrupt handler with the PIC
 *  3: Enable the PIC to respond to the individual interrupts
 *  4: Enable the PIC
 *
 *
 * Note the XExc_* and XIntc_* prefixes. XExc_* controls the CPU's internal 
 *  interrupts, XIntc_* are driver calls for the external interrupt controller.
 *
 */
void InitializeInterrupts(void){
	
	// Enable the Serial Port Interrupt. This must be done BEFORE it is registered or else it will always stay on
	XUartLite_mEnableIntr(XPAR_OPB_UART_WIRELESS_BASEADDR);
	//XUartLite_mEnableIntr(TTL_BASEADDR);

	// Initialize PowerPC exception table
	XExc_Init();

	// Setup up PIT interrupt handler
	XExc_RegisterHandler(	XEXC_ID_PIT_INT,
									(XExceptionHandler)IH_TimerHandler, 
									(void *)NULL);

	// Setup external interrupt controller handler (defined in xintc_l.c/h)
	XExc_RegisterHandler(	XEXC_ID_NON_CRITICAL_INT,
									(XExceptionHandler)XIntc_DeviceInterruptHandler,
									(void *)XPAR_OPB_INTC_0_DEVICE_ID);

	// Register Wireless interrupt handler with interrupt controller driver
	XIntc_RegisterHandler(	XPAR_OPB_INTC_0_BASEADDR,
									XPAR_OPB_INTC_0_OPB_UART_WIRELESS_INTERRUPT_INTR,
									//(XInterruptHandler)IH_InterruptHandlerWireless,
									(XInterruptHandler)WirelessInterruptHandler,
									(void *)NULL);
									
	// Register Frame Grabber interrupt handler with interrupt controller driver
	XIntc_RegisterHandler(	XPAR_OPB_INTC_0_BASEADDR,
									XPAR_OPB_INTC_0_PLB_CAM0_INTERRUPT_INTR,
									(XInterruptHandler)FT_InterruptHandlerFrameTable,
									(void *)NULL);		

	// Enable individual interrupts with interrupt controller
	XIntc_mEnableIntr(XPAR_OPB_INTC_0_BASEADDR,  XPAR_OPB_UART_WIRELESS_INTERRUPT_MASK | XPAR_PLB_CAM0_INTERRUPT_MASK);

	// Enable interrupt controller master
	XIntc_mMasterEnable(XPAR_OPB_INTC_0_BASEADDR);
}





/******************************************************************************
 * Initialize Memory
 * STEPS:
 *  1: Initialize MemAlloc (not malloc) to use the SDRAM
 *  2: Enable the Instruction Cache
 *  3: Enable the Data Cache
 */
void InitializeMemory(void){
	//move the beginning of the memory up by 1 MB: 1048576
	//this allows part of the heap, stack, and other data to reside on SDRAM
	InitializeMemAlloc((uint32 *)(SDRAM_ADDR + 1048576));//((uint32 *)SDRAM_ADDR + 32);		// Initialize MemAlloc to use SDRAM
	
	XCache_InvalidateICache();
	XCache_EnableICache(I_CACHE_REGION);				// Enable and test the instruction cache	

	XCache_EnableDCache(D_CACHE_REGION);				// Enable data cache
}





/******************************************************************************
 * SETUP CAMERA
 *
 */
void SetupCamera(){
	WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x01, 3); // Select core reg
	WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x0D, 1); // Sensor reset
	WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x0D, 0); // Clear sensor reset
	WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x01, 1); // Select IFP reg set

	uint32 version = ReadCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x36);	// Read camera version
	setCamVersion(version);

	// Reset camera's image flow processor
	WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x07, 1);  // Assert IFP reset
	WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x07, 0);  // Clear IFP reset

	// Configure Camera to give desired image size
	WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0xA7, FRAME_WIDTH);
	WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0xAA, FRAME_HEIGHT);					
	WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x08, ReadCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x08) | 0x1000);	// set output to RGB 
	//WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x08, ReadCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x08) & 0xEFFF);	// set output to YUV 
	WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x05, 0);               	                                        // Turn off sharpenning
	
	//WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x25, ReadCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x25) & 0xFFC0);	// Set AWB to fastest
	//WriteCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x2F, ReadCameraRegister(XPAR_PLB_CAM0_BASEADDR, 0x2F) & 0xFFC0);	// Set AE to fastest
	
	// Barrett's PLB Camera Core Registers
	setPLBBurstSize(XPAR_PLB_CAM0_BASEADDR, CAM_BURST_SIZE_128);
	ResetCameraCore(XPAR_PLB_CAM0_BASEADDR); //resets FIFOs and other image processing units
	ConvReset(XPAR_PLB_CAM0_BASEADDR); //reset the convolution unit and FIFOs

	//setFrameSaveOptions(XPAR_PLB_CAM0_BASEADDR, CAM_SAVE_OPTIONS_COL_COUNT | CAM_SAVE_OPTIONS_GREYSCALE | CAM_SAVE_OPTIONS_MASK | CAM_SAVE_OPTIONS_HSV | CAM_SAVE_OPTIONS_ORG);
	//when the camera is capturing, use FT_RequestSaveOptions instead of setFrameSaveOptions
	setFrameSaveOptions(XPAR_PLB_CAM0_BASEADDR, CAM_SAVE_OPTIONS_GREYSCALE | CAM_SAVE_OPTIONS_MASK | CAM_SAVE_OPTIONS_HSV | CAM_SAVE_OPTIONS_ORG);
	
	SetRegisterValue(XPAR_PLB_CAM0_BASEADDR, OFFSET_CONV_ZEROS_VAL, 0); //leave this at 0, it's for debug (fills top of image w/ 1 value and bottom w/ another)
	SetRegisterValue(XPAR_PLB_CAM0_BASEADDR,OFFSET_CONV_COL_INIT,638);
	SetRegisterValue(XPAR_PLB_CAM0_BASEADDR,OFFSET_CONV_ROW_INIT,447);
	SetRegisterValue(XPAR_PLB_CAM0_BASEADDR,OFFSET_CONV_BETWEEN_FRAMES_INIT,640*16);//9601-264-256);
	SetRegisterValue(XPAR_PLB_CAM0_BASEADDR,OFFSET_CONV_INIT_INVALID_INIT,9601);
	SetRegisterValue(XPAR_PLB_CAM0_BASEADDR,OFFSET_CONV_INITIAL_ZERO_INIT,9601+640); //9601

	
}


