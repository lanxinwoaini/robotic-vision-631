/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    opb_pwm_ctrl.c
AUTHOR:  Wade S. Fife
CREATED: 01/19/05

DESCRIPTION

Driver software for the opb_pwm_ctrl (Pulse Width Modulation) entity.

******************************************************************************/

#include <opb_pwm_ctrl.h>

// Allows you to set the duty cycle by specifying the percentage of
// positive duty (0 to 100).
// IMPORTANT NOTE: This only works if (percent * period_reg) is within the 
// range of unsigned! This version also rounds down to nearest cycle.
void SetPWMDutyPercent(Xuint32 baseAddr, int pwmNum, unsigned percent)
{
    Xuint32 period;
    Xuint32 volatile *ptr;

    ptr = PWM_WPTR(baseAddr, pwmNum);

    // Read period cycles
    period = *(ptr + PWM_PERIOD_OFFSET);

    // Set duty
    *(ptr + PWM_DUTY_OFFSET) = (percent * period) / 100;
}

