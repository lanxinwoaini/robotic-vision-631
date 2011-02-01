/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    bootloader.h
AUTHOR:  Matt Diehl
CREATED: 23 Feb 2008

DESCRIPTION

******************************************************************************/
#ifndef BOOTLOADER_H_
#define BOOTLOADER_H_

/* Includes -----------------------------------------------------------------*/
#include "SystemTypes.h"


/* Defines ------------------------------------------------------------------*/
//0x0020000 is at 2MB boundary
#define BOOT_IMAGE_BASEADDR           0x00200000
/* External Memory-----------------------------------------------------------*/
/* Function Prototypes ------------------------------------------------------*/

void display_progress(uint32 lines);
int load_exec();
int flash_get_srec_line(uint8 *buf);
void bootload();


#endif
