/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    opb_pwm_ctrl.h
AUTHOR:  Wade S. Fife
CREATED: 01/19/05

DESCRIPTION

Driver software header for the opb_pwm_ctrl (Pulse Width Modulation) entity.

******************************************************************************/

#include <xio.h>



// DEFINITIONS //////////////////////////////////////////////

#define PWM_DUTY_OFFSET   0 // Num words duty_reg is offset from base address
#define PWM_PERIOD_OFFSET 1 // Num words period_reg is offset from base address



// MACRO DEFINITIONS //////////////////////////////////////////////////////////

#define PWM_WPTR(base_addr, pwm_num) \
        ((Xuint32 volatile *)(((Xuint32)(base_addr)) + ((pwm_num) << 4)))

// Macros to set register values
#define SET_PWM_PERIOD(base_addr, pwm_num, cycles) \
        (*(PWM_WPTR(base_addr, pwm_num) + PWM_PERIOD_OFFSET) = (cycles))
#define SET_PWM_DUTY(base_addr, pwm_num, cycles)   \
        (*(PWM_WPTR(base_addr, pwm_num) + PWM_DUTY_OFFSET) = (cycles))

// Macros to read register values
#define GET_PWM_DUTY(base_addr, pwm_num) \
        (*(PWM_WPTR(base_addr, pwm_num) + PWM_DUTY_OFFSET))
#define GET_PWM_PERIOD(base_addr, pwm_num) \
        (*(PWM_WPTR(base_addr, pwm_num) + PWM_PERIOD_OFFSET))


// FUNCTION PROTOTYPES ////////////////////////////////////////////////////////

void SetPWMDutyPercent(Xuint32 baseAddr, int pwmNum, unsigned percent);
