/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    HeliosIO.h
AUTHOR:  Wade Fife
CREATED: 11/07/05

DESCRIPTION

Header file for HeliosIO.c

CHANGE LOG

11/07/05 WSF  Created original file.
12/02/05 WSF  Added code to control sleep pins on SRAM and flash.

******************************************************************************/

#ifndef HELIOS_IO_H
#define HELIOS_IO_H

#include "SystemTypes.h"



// USER CONFIGURATION /////////////////////////////////////////////////////////

// Baseaddress of GPIO module for board I/O
#include "xparameters.h"
#define HELIOS_IO_BASEADDR    XPAR_OPB_GPIO_HELIOS_BASEADDR

// Define the components for which you would like to include code for
// compilation.
#define HELIOS_USE_BTN             // Code to read user button
#define HELIOS_USE_SW1             // Code to read DIP SW1
#define HELIOS_USE_SW2             // Code to read DIP SW2
#define HELIOS_USE_LED1            // Code to read/write user LED1
#define HELIOS_USE_LED2            // Code to read/write user LED2
#define HELIOS_USE_SLEEP_SRAM      // Code to sleep/awake SRAM
#define HELIOS_USE_SLEEP_FLASH     // Code to sleep/awake FLASH

///////////////////////////////////////////////////////////////////////////////



// FUNCTION PROTOTYPES ////////////////////////////////////////////////////////

uint8  HeliosReadBTN(void);
uint8  HeliosReadSW1(void);
uint8  HeliosReadSW2(void);
uint8  HeliosReadLED1(void);
void   HeliosSetLED1(uint8 value);
uint8  HeliosReadLED2(void);
void   HeliosSetLED2(uint8 value);
void   HeliosSetSleepSRAM(uint8 state);
uint8  HeliosGetSleepSRAM(void);
void   HeliosSetSleepFlash(uint8 state);
uint8  HeliosGetSleepFlash(void);


#endif
