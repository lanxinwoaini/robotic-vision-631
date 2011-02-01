// *****************************************************************************
//
// File Name	: 'pwmled.h' 
// Title		: MAX6966 PWM LED Controller driver
// Author		: Jared Havican
// Created		: 01/30/2010
// Revised		: 01/30/2010
// Version		: 0.1
//
// *****************************************************************************

#ifndef PWMLED_H
#define PWMLED_H

#include "global.h"

// LED Defines
#define STATUS_LEDSET		0
#define TEAMCOLOR_LEDSET	1

#define STATUS_LED_R	2
#define STATUS_LED_G	0
#define STATUS_LED_B	4
#define TEAMCOLOR_LED_R	3
#define TEAMCOLOR_LED_G	1
#define TEAMCOLOR_LED_B	5
#define FRONT_LED_RIGHT	6
#define FRONT_LED_LEFT	7
#define BACK_LED_RIGHT	8
#define BACK_LED_LEFT	9

// Port registers
#define PWMLED_P0_REG	0x00
#define PWMLED_P1_REG	0x01
#define PWMLED_P2_REG	0x02
#define PWMLED_P3_REG	0x03
#define PWMLED_P4_REG	0x04
#define PWMLED_P5_REG	0x05
#define PWMLED_P6_REG	0x06
#define PWMLED_P7_REG	0x07
#define PWMLED_P8_REG	0x08
#define PWMLED_P9_REG	0x09

// Port registers controlling multiple ports
#define PWMLED_P09_REG	0x0A
#define PWMLED_P03_REG	0x0B
#define PWMLED_P47_REG	0x0C
#define PWMLED_P89_REG	0x0D

// Port value for LED off
#define PWMLED_OFF_VAL	0xFF

// Configuration registers
#define PWMLED_CONFIG_REG		0x10
#define PWMLED_RAMPDOWN_REG		0x11
#define PWMLED_RAMPUP_REG		0x12
#define PWMLED_OUTCURR70_REG	0x13
#define PWMLED_OUTCURR98_REG	0x14
#define PWMLED_GLOBCURR_REG		0x15

// Current values
#define PWMLED_CURR_02_5MA	0x00 //current=2.5mA
#define PWMLED_CURR_05_0MA	0x01 //current=5.0mA
#define PWMLED_CURR_07_5MA	0x02 //current=7.5mA
#define PWMLED_CURR_10_0MA	0x03 //current=10.0mA
#define PWMLED_CURR_12_5MA	0x04 //current=12.5mA
#define PWMLED_CURR_15_0MA	0x05 //current=15.0mA
#define PWMLED_CURR_17_5MA	0x06 //current=17.5mA
#define PWMLED_CURR_20_0MA	0x07 //current=20.0mA

// SPI Read/Write modes
#define PWMLED_MODE_READ  1
#define PWMLED_MODE_WRITE 0

/* Send byte to PWM Controller over SPI */
void pwmledSPISendByte(u08 data);

/* Send word to PWM Controller over SPI */
void pwmledSPISendWord(u16 data);

/* Send/Receive byte to/from PWM Controller over SPI */
u08 pwmledSPITransferByte(u08 data);

/* Writes to a register in the pwm controller */
void pwmledWriteReg(u08 reg, u08 data);

/* Reads a register in the pwm controller */
u08 pwmledReadReg(u08 reg);

/* Set LED value */
void pwmledSetLED(u08 led, u08 val);

/* Set all LEDs to a value */
void pwmledSetAllLEDs(u08 val);

/* Initializes the pwm controller */
void pwmledInit();

/* Sets an LED to a given intensity */
void pwmSetLEDIntensity(u08 led, u08 intensity);

/* Sets an LEDSet to a color */
/* color[] is an array of 3 u08s, index 0=red, 1=green, 2=blue */
void pwmSetLEDColor(u08 ledSet, u08 color[]);

/* Tests the LEDs by flashing them*/
#ifdef TESTING_ON
s08 PWMTest();
#endif

#endif
