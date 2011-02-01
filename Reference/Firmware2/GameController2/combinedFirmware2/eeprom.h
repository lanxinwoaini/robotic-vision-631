/*
******************************************
* FileName	:eeprom.h 
* Author	:Russell LeBaron
* Date		:1/21/10
* Version	:0.0
*
******************************************
*
* This code contains driver functions for the Microchip 25LC512 EEPROM.
*  This includes ability to initialize the chip, read and save data
*


*/

#ifndef EEPROM_H
#define EEPROM_H

#include "avrlibtypes.h"
#include "global.h"
#include "spi.h"
#include "rprintf.h"
#include <avr/io.h>
#include "UnitTest.h"
#include "timer.h"

//pin definitions
#define EEPROM_nHOLD	PD7

//Commands
#define EEPROM_READ		0x3//0000 0011 //Read data from memory array beginning at selected address
#define EEPROM_WRITE 	0x2//0000 0010 Write data to memory array beginning at selected address
#define EEPROM_WREN 	0x6//0000 0110 Set the write enable latch (enable write operations)
#define EEPROM_WRDI 	0x4//0000 0100 Reset the write enable latch (disable write operations)
#define EEPROM_RDSR 	0x5//0000 0101 Read STATUS register
#define EEPROM_WRSR 	0x1//0000 0001 Write STATUS register
#define EEPROM_PE 		0x42//0100 0010 Page Erase – erase one page in memory array
#define EEPROM_SE 		0xD8//1101 1000 Sector Erase – erase one sector in memory array
#define EEPROM_CE 		0xC7//1100 0111 Chip Erase – erase all sectors in memory array
#define EEPROM_RDID 	0xBC//1010 1011 Release from Deep power-down and read electronic signature
#define EEPROM_DPD 		0xB9//1011 1001 Deep Power-Down mode

//Status bits
#define EEPROM_STATUS_WIP 0x1

//initializes the EEPROM
void EEPROMInit(void);
//init nHOLD as output and high



//EEPROMRead - reads back data at specified address, continues to read until nCS is pulled high
// inputs:
//	u16 address //16 bit address of the data you want to access
//	u08 buffer // buffer for reading data into
//	u08 size	// size of the buffer
//output:
//  u08 returns TRUE if successful
//	FALSE otherwise
u08 EEPROMRead(u16 address, u08* buffer, u08 size);

//EEPROMWriteByte  write one byte to EEPROM
// input:
//	u16 address // 16 bit address of where you want to write data
//	u08 data	// data you want written to EEPROM
// output:
// 
void EEPROMWriteByte(u16 address, u08 data);
//requires EEPROM_WREN instruction to be passed before any data can be written
//nCS must then be toggled high to set the WREN latch
//nCS can then be pulled low again and write may proceed
//Pass the EEPROM_WRITE instruction followed by address to write to
//page writes up to 128 Bytes, must all be in the same page
//after sending data, must release nCS(pull high) for write cycle to complete
//no READ until write cycle is complete, you can read the WRSR(write status register)



//EEPROMWrite write up to one page(128 bytes) to EEPROM
// input:
//	u16 address // 16 bit address of where you want to write data
//	u08* buffer	// data buffer you want written to EEPROM
//	s08 size	// size in Bytes of the data buffer you want to write, 128 is the max
// output:
// 	

void EEPROMWritePage(u16 address, u08* buffer, u08 size);

//returns the status of the EEPROM
u08 EEPROMReadStatus(void);


#ifdef TESTING_ON
s08 EEPROMTest(void);
#endif

#endif
