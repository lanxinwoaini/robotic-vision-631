/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    MemAlloc.h
AUTHOR:  Wade Fife
CREATED: 10/22/04

DESCRIPTION

Header file for MemAlloc.c.

******************************************************************************/

#ifndef MEM_ALLOC_H
#define MEM_ALLOC_H


#include "SystemTypes.h"

void InitializeMemAlloc(uint32 *memAllocBase);
void *MemAlloc(unsigned numBytes);
void *MemCalloc(unsigned numBytes);
void *MemTop();

#endif // MEM_ALLOC_H


