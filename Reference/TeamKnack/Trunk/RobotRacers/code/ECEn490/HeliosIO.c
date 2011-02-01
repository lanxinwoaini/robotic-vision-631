/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:	 HeliosIO.c
AUTHOR:	 Wade Fife
CREATED: 11/7/05

DESCRIPTION

This module contains code to use the user button, two DIP switches, and LED on
the Helios board, rev. 1A. It assumes use of a the OPB GPIO core (v3.01.b) and
the gpio driver (v2.00.a). It also assumes the following connections to the
GPIO core:

  NET<0> = GPIO<31> - User button (LSB)
  NET<1> = GPIO<30> - Switch 1
  NET<2> = GPIO<29> - Switch 2
  NET<3> = GPIO<28> - LED 1
  NET<4> = GPIO<27> - LED 2
  NET<5> = GPIO<26> - SRAM sleep
  NET<6> = GPIO<25> - Flash sleep (MSB)

  * GPIO refers to the register read/written from the GPIO device via software
    and NET refers to the net conntected to the GPIO_IO port on the GPIO core.

Note that NET is assumed to be defined as VEC 4:0 in the MHS file, which causes
its lease significant bit (LSB) to be NET<0> whereas on the PPC the LSB of a
32-bit signal is generally considered to be bit 31. Unfortunately this leads to
a lot of confusion so great care must be excercised. To work properly the LED
bits should be set as output only (i.e., the GPIO_TRI register should be
initialized to 0xFFFFFFE7).

This code also assumes GPIO driver 2.00.a.

CHANGE LOG

11/07/05 WSF Created original file.
12/02/05 WSF Added code to control sleep pins on SRAM and flash.

******************************************************************************/

#include "xgpio_l.h"
#include "HeliosIO.h"



// FUNCTIONS //////////////////////////////////////////////////////////////////


#ifdef HELIOS_USE_BTN /////////////////////////////////////////////////////////

// Read user button
// Returns 0 or 1, 1 means the button is depressed.
uint8 HeliosReadBTN(void)
{
	return (uint8)(
		XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1) & 0x1
		);
}

#endif



#ifdef HELIOS_USE_SW1 /////////////////////////////////////////////////////////

// Read DIP switch 1
// Returns 0 or 1, 1 means the switch is in the ON position.
uint8 HeliosReadSW1(void)
{
	return (uint8)(
		(XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1) >> 1) & 0x1
		);
}

#endif



#ifdef HELIOS_USE_SW2 /////////////////////////////////////////////////////////

// Read DIP switch 2
// Returns 0 or 1, 1 means the switch is in the ON position.
uint8 HeliosReadSW2(void)
{
	return (uint8)(
		(XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1) >> 2) & 0x1
		);
}

#endif



#ifdef HELIOS_USE_LED1 ////////////////////////////////////////////////////////

// Read current state of LED1
// Returns 0 or 1, 1 means the LED is ON.
uint8 HeliosReadLED1(void)
{
	return (uint8)(
		(XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1) >> 3) & 0x1
		);
}


// Set user LED1 state
// Set to 0 for OFF, 1 for ON.
void HeliosSetLED1(uint8 value)
{
	uint32 readValue;

	readValue = XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1);
	readValue &= ~(0x00000008);		   // Clear the LED bit
	readValue |= (value & 0x1) << 3;   // Set LED bit
	XGpio_mSetDataReg(HELIOS_IO_BASEADDR, 1, readValue);
}

#endif



#ifdef HELIOS_USE_LED2 ////////////////////////////////////////////////////////

// Read current state of LED2
// Returns 0 or 1, 1 means the LED is ON.
uint8 HeliosReadLED2(void)
{
	return (uint8)(
		(XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1) >> 4) & 0x1
		);
}


// Set user LED2 state
// Set to 0 for OFF, 1 for ON.
void HeliosSetLED2(uint8 value)
{
	uint32 readValue;

	readValue = XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1);
	readValue &= ~(0x00000010);		   // Clear the LED bit
	readValue |= (value & 0x1) << 4;   // Set LED bit
	XGpio_mSetDataReg(HELIOS_IO_BASEADDR, 1, readValue);
}

#endif



#ifdef HELIOS_USE_SLEEP_SRAM //////////////////////////////////////////////////

// Sets the sleep state of the SRAM chip (i.e., the ZZ pin). A state
// argument of 1 puts the SRAM into sleep mode. A state argument of 0
// makes the SRAM operational.
//
// NOTE: Test show that putting the SRAM into sleep mode saves about
// 14.7 mW of power with a 5V board input.
void HeliosSetSleepSRAM(uint8 state)
{
	uint32 readValue;

	readValue = XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1);
	readValue &= ~(0x00000020);		   // Clear the ZZ bit
	readValue |= (state & 0x1) << 5;   // Set SRAM ZZ signal
	XGpio_mSetDataReg(HELIOS_IO_BASEADDR, 1, readValue);	
}



// Gets the current sleep state of the SRAM. Returns 1 if the SRAM is
// in sleep mode. Returns 0 if it is in operating mode.
uint8 HeliosGetSleepSRAM(void)
{
	uint32 readValue;

	readValue = XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1);
	readValue = (readValue >> 5) & 1;

	return readValue;
}

#endif



#ifdef HELIOS_USE_SLEEP_FLASH /////////////////////////////////////////////////

// Sets the sleep state of the flash chip (i.e., the RP# pin). A state
// argument of 1 puts the flash into deep power-down mode. A state
// argument of 0 makes the flash operational.
//
// NOTE: The flash, a Micron Q-Flash, already has such a low idle
// current that the deep-power down mode typically has a negligable
// effect on the overall power consumption.
void HeliosSetSleepFlash(uint8 state)
{
	uint32 readValue;

	readValue = XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1);
	readValue &= ~(0x00000040);		    // Clear the RP# bit
	readValue |= ((~state) & 0x1) << 6; // Set flash RP# signal
	XGpio_mSetDataReg(HELIOS_IO_BASEADDR, 1, readValue);	
}



// Gets the current sleep state of the flash. Returns 1 if the flash
// is in deep power-down mode. Returns 0 if it is in operating mode.
uint8 HeliosGetSleepFlash(void)
{
	uint32 readValue;

	readValue = XGpio_mGetDataReg(HELIOS_IO_BASEADDR, 1);
	readValue = ((~readValue) >> 6) & 1;

	return readValue;
}

#endif

