/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    InterruptHandlers.h
AUTHOR:  Barrett Edwards
CREATED: 26 Sep 2006

DESCRIPTION

******************************************************************************/
#ifndef INTERRUPTHANDLERS_H_
#define INTERRUPTHANDLERS_H_


/* Includes -----------------------------------------------------------------*/



/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/


/*********************************************************************
 * Functional Group: Helper Functions
 * Definition:       
 * 
 *  1: InitMemory()
 *
*/
void IH_InitMemory();



/******************************************************************************
 * Functional Group: IO Port Interrupt Handlers
 * Definition:       
 * 
 *  1: TimerHandler()
*/ 
void IH_TimerHandler(void *ptr);

#endif
