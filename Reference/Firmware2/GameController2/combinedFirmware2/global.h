
//*****************************************************************************
//
// File Name	: 'global.h'
// Title		: AVR project global include 
// Author		: Pascal Stang
// Created		: 7/12/2001
// Revised		: 9/30/2002
// Version		: 1.1
// Target MCU	: Atmel AVR series
// Editor Tabs	: 4
//
//	Description : This include file is designed to contain items useful to all
//					code files and projects.
//
// This code is distributed under the GNU Public License
//		which can be found at http://www.gnu.org/licenses/gpl.txt
//
//*****************************************************************************

#ifndef GLOBAL_H
#define GLOBAL_H

// global AVRLIB defines
#include "avrlibdefs.h"
// global AVRLIB types definitions
#include "avrlibtypes.h"

// project/system dependent defines

#ifndef F_CPU
// CPU clock speed
//#define F_CPU          20000000               		// 20MHz processor
//#define F_CPU        14745000               		// 14.745MHz processor
#define F_CPU        12288000               		// 12.288MHz processor
//#define F_CPU        8000000               		// 8MHz processor
//#define F_CPU        7372800               		// 7.37MHz processor
//#define F_CPU        4000000               		// 4MHz processor
//#define F_CPU        3686400               		// 3.69MHz processor
#endif

#define CYCLES_PER_US ((F_CPU+500000)/1000000) 	// cpu cycles per microsecond
//#define __AVR_ATmega168__ 	//processor

//spi slave device selects
#define OFF_SPI_SS		0
#define PWM_SPI_SS		PORTC3
#define SOUND_SPI_SS	PORTC5
#define UART2_SPI_SS	PORTC4

//Debug LED
#define DEBUG_LED		PORTD4
//Include testing code
//#define TESTING_ON


//global game parameters
/*
ruleFlags - 8 bits where: 
								Flags:
								-w
	bit 0 represent the flag TeamMemberKillShot
	TeamMemberKillShot = 1: a kill shot to your own team member will disable them.
	TeamMemberKillShot = 0: a kill shot to your own team member will do nothing.
	
								-x
	bit 1 represent the flag YourBaseKillShot
	YourBaseKillShot = 1: a kill shot to your own base will disable the shooter.
	YourBaseKillShot = 0: a kill shot to your own base will do nothing.
	
								-y
	bit 2 represent the flag OppossingPlayerReviveShot
	OppossingPlayerReviveShot = 1: a revive shot to your opponent will revive them.
	OppossingPlayerReviveShot = 0: a revive shot to your opponent will do nothing.
	
								-z
	bit 3 represent the flag OppossingBaseReviveShot
	OppossingBaseReviveShot = 1: a revive shot to your opponent's base will disable the shooter.
	OppossingBaseReviveShot = 0: a revive shot to your opponent's base will do nothing.
	
	bit 4 - open
	bit 5 - open
	bit 6 - open
	bit 7 - open
*/
typedef struct _gameParameters{
	u08 ruleFlags;
	s08 isBase;  //this is defined as 1 if this controller is operating as a base station, 0 if truck
	u08 myTeamID;
	u08 myTruckID;
	u08 myTruckName[16];
	u08 myGameState;
	u08 myColor[3];//red green and blue value
	u08 team1Color[3];
	u08 team2Color[3];
	u08 statusEnabled[3]; //red green and blue value
	u08 statusDisabled[3]; //red green and blue value
	u08 statusHaveFlag[3]; //red green and blue value
	u08 hlIntensity;  //headlight intensity value
	u08 tlIntensity; //taillight intensity
	u16 phrasesInMem;
 	u16 volume;
	u16 xbeeID;
	u08 xbeeChannel;

}gameParameters;



extern gameParameters gp;	//gameParameters for the game controller
//Location of game parameters in EEPROM

#define GAME_PARAMETERS_ADDR 0xFF80
#define GAME_PARAMETERS_SIZE 42

#endif
