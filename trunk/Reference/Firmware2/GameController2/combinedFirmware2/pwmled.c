// *****************************************************************************
//
// File Name	: 'pwmled.c'
// Title		: MAX6966 PWM LED Controller driver
// Author		: Jared Havican
// Created		: 01/30/2010
// Revised		: 01/30/2010
// Version		: 0.1
//
// *****************************************************************************

#include <avr/io.h>

#include "pwmled.h"
#include "spi.h"
#include "rprintf.h"
#include "timer.h"
#include "global.h"


/* Send byte to PWM Controller over SPI */
void pwmledSendByte(u08 data){
	u08 sreg;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	spiSetCS( PWM_SPI_SS );
	
	spiSendByte(data);
	
	SREG = sreg;//restore interrupts
}

/* Send word to PWM Controller over SPI */
void pwmledSendWord(u16 data){
	u08 sreg;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	spiSetCS( PWM_SPI_SS );
	
	pwmledSendByte(data>>8 & 0x00FF); // Send MSB
	pwmledSendByte(data & 0x00FF); //Send LSB
	
	SREG = sreg;//restore interrupts
}

/* Send/Receive byte to/from PWM Controller over SPI */
u08 pwmledTransferByte(u08 data){
	u08 sreg;
	u08 retval;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	spiSetCS( PWM_SPI_SS );
	
	pwmledSendByte(data); // Send data
	pwmledSendByte(0x80); //Send dummy byte
	
	while(!(inb(SPSR) & (1<<SPIF))); //wait for transfer to finish
	retval = inb(SPDR);	//read data received
	
	SREG = sreg;//restore interrupts
	return retval;
}

/* Writes to a register in the pwm controller */
void pwmledWriteReg(u08 reg, u08 data){
	pwmledSendWord(PWMLED_MODE_WRITE<<15 | reg<<8 | data);
	spiSetCS( OFF_SPI_SS );
}

/* Reads a register in the pwm controller */
u08 pwmledReadReg(u08 reg){
	u08 val = pwmledTransferByte(PWMLED_MODE_READ<<7 | reg);
	spiSetCS( OFF_SPI_SS );
	return val;
}


/* Set LED value */
void pwmledSetLED(u08 led, u08 val){
	if (led >=0 && led <=9){
		pwmledWriteReg(led, val);
	}
}

/* Set all LEDs to a value */
void pwmledSetAllLEDs(u08 val){
	pwmledWriteReg(PWMLED_P09_REG, val);
}

/* Initializes the pwm controller */
void pwmledInit(){
	//Set device in run mode, ramp-up off, stagger phase
	pwmledWriteReg(PWMLED_CONFIG_REG, 0x21);
	//Set Global Current to 20mA, set each port output to half current
	pwmledWriteReg(PWMLED_GLOBCURR_REG, PWMLED_CURR_20_0MA);
	pwmledWriteReg(PWMLED_OUTCURR70_REG, 0x00);
	pwmledWriteReg(PWMLED_OUTCURR98_REG, 0x00);
	//Turn off all LEDs
	pwmledSetAllLEDs(PWMLED_OFF_VAL);
}

/* Sets an LED to a given intensity */
void pwmSetLEDIntensity(u08 led, u08 intensity){
	intensity = intensity % 255; // Invert intensity
	if (intensity<3) intensity=3; // 3 is the PWM minimum
	pwmledSetLED(led, intensity);
}

/* Sets an LEDSet to a color */
/* color[] is an array of 3 u08s, index 0=red, 1=green, 2=blue */
void pwmSetLEDColor(u08 ledSet, u08 color[]){
	if (ledSet==STATUS_LEDSET){
		pwmSetLEDIntensity(STATUS_LED_R, color[0]);
		pwmSetLEDIntensity(STATUS_LED_G, color[1]);
		pwmSetLEDIntensity(STATUS_LED_B, color[2]);
	} else if (ledSet==TEAMCOLOR_LEDSET){
		pwmSetLEDIntensity(TEAMCOLOR_LED_R, color[0]);
		pwmSetLEDIntensity(TEAMCOLOR_LED_G, color[1]);
		pwmSetLEDIntensity(TEAMCOLOR_LED_B, color[2]);
	}
}

/* Flashes the LEDs */
#ifdef TESTING_ON
s08 PWMTest(){
	pwmSetLEDIntensity(STATUS_LED_R,128);
	pwmSetLEDIntensity(STATUS_LED_G,128);
	pwmSetLEDIntensity(STATUS_LED_B,128);
	pwmSetLEDIntensity(TEAMCOLOR_LED_R,128);
	pwmSetLEDIntensity(TEAMCOLOR_LED_G,128);
	pwmSetLEDIntensity(TEAMCOLOR_LED_B,128);
	pwmSetLEDIntensity(FRONT_LED_RIGHT,128);
	pwmSetLEDIntensity(FRONT_LED_LEFT,128);
	pwmSetLEDIntensity(BACK_LED_RIGHT,128);
	pwmSetLEDIntensity(BACK_LED_LEFT,128);
	timerPause(500);
	pwmledSetAllLEDs(PWMLED_OFF_VAL);
	return TRUE;
}
#endif
