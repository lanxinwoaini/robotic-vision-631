/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:	 opb_quad_encoder.h
AUTHOR:	 Wade Fife
CREATED: 02/02/06

DESCRIPTION

This is the driver file for the OPB quadrature encoder core. The core
interface consists simply of the macros in this file.

CHANGE LOG

02/02/06  WSF  Created inital file.

******************************************************************************/

#include <xio.h>


// MACRO DEFINITIONS ////////////////////////////////////////

#define ENCODER_COUNT(BASE_ADDR) \
        (*((Xuint32 volatile *)((Xuint32)(BASE_ADDR))))
