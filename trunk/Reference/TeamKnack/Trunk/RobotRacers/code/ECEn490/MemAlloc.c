/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    MemAlloc.c
AUTHOR:  Wade Fife
CREATED: 10/22/04

DESCRIPTION

This file contains a simple memory allocation library. All memory allocated by
this library is considered permanent. That is, it cannot be freed. This library
is very simple, with a very small memory footprint, and it is RTOS compatible.

The user should pass in a memory base address to InitializeMemAlloc(). MemAlloc
will start allocating memory from that point.

NOTES

There is currently no check for overflow.

******************************************************************************/

#include "SystemTypes.h"
#include "MemAlloc.h"
#include "InterruptControl.h"



// GLOBALS ////////////////////////////////////////////////////////////////////


// memTop should always point to the next word of available memory
static uint32 *memTop;


// FUNCTIONS //////////////////////////////////////////////////////////////////


// This function must be called ONCE and only once to initialize the
// allocation unit.
void InitializeMemAlloc(uint32 *memAllocBase)
{
	DISABLE_INTERRUPTS();
	memTop = memAllocBase;
	RESTORE_INTERRUPTS(msr);
}

void *MemTop()
{
	uint32* val;
	DISABLE_INTERRUPTS();
	val = memTop;
	RESTORE_INTERRUPTS(msr);
	return val;
}


// Permanently allocates given number of bytes. Returns a pointer to
// the first byte of allocated memory.
// Reentrancy: This function IS reentrant.
void *MemAlloc(unsigned numBytes)
{

	uint32 *returnValue;
	uint32 numWords;

	// Round up to nearest whole word	
	numWords = (numBytes >> 2);
	if(numBytes & 0x3) numWords++;

	DISABLE_INTERRUPTS();
	returnValue = memTop;
	memTop += numWords;
	RESTORE_INTERRUPTS(msr);

	return (void *)returnValue;
}

void *MemCalloc(unsigned numBytes){
	uint32 *returnValue;
	uint32 numWords;

	// Round up to nearest whole word	
	numWords = (numBytes >> 2);
	if(numBytes & 0x3) numWords++;

	DISABLE_INTERRUPTS();
	returnValue = memTop;
	memTop += numWords;
	RESTORE_INTERRUPTS(msr);

	//clear the data structure to be returned
	uint32 *memLoc = returnValue;
	for (;memLoc<memTop;memLoc++) *memLoc = 0;
	
	return (void *)returnValue;
}

