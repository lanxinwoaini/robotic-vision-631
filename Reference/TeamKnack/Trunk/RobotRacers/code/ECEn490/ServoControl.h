/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    ServoControl.h
AUTHOR:  Wade Fife
CREATED: 01/20/05

DESCRIPTION

Header file for ServoControl.c. See ServoControl.c for more information.

******************************************************************************/

#ifndef RCCONTROL_H
#define RCCONTROL_H

#include "xparameters.h"
#include "SystemTypes.h"
#include "pidControl.h"

#define CONST_STEERING_OFFSET -4
extern int velocityMultiple;
extern int steeringTrim;


// USER CONFIGURATION SECTION /////////////////////////////////////////////////

#define SERVO_BASE_ADDR XPAR_OPB_PWM_BASEADDR    // PWM peripheral base address
#define SERVO_OPB_FREQ  100000000                // OPB bus clock frequency

///////////////////////////////////////////////////////////////////////////////



// DEFINES ////////////////////////////////////////////////////////////////////

#define RC_STR_SERVO 0    // PWM number used for steering
#define RC_VEL_SERVO 1    // PWM number used for speed control



// MACROS /////////////////////////////////////////////////////////////////////

// These are for RC car control, where one servo is assumed to be
// steering and the other is assumed to be speed control.
#define RCSetSteering(str) SetServo(RC_STR_SERVO, (str)+ CONST_STEERING_OFFSET +steeringTrim);
#define RCSetThrottle(thr) SetServo(RC_VEL_SERVO, (thr)/velocityMultiple);



// FUNCTION PROTOTYPES ////////////////////////////////////////////////////////

void ServoInit(int servoNum);
void SetServo(int servoNum, int8 position);

#endif
