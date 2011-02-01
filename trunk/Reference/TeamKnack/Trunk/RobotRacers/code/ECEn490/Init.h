/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    Init.h
AUTHOR:  Barrett Edwards
CREATED: 25 Sep 2006

DESCRIPTION

******************************************************************************/

#ifndef INIT_H_
#define INIT_H_


/* Includes -----------------------------------------------------------------*/
#include "SystemTypes.h"


/* Defines ------------------------------------------------------------------*/
/* Structs ------------------------------------------------------------------*/
/* External Memory-----------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/


/*********************************************************************
 * Functional Group: Helper Functions
 * Definition:       
 * 
 *  1: Init()
 *  
*/ 
void MainInit();
void InitializePIT(void);
void InitializeInterrupts(void);
void InitializeMemory(void);
void SetupCamera();


#endif
