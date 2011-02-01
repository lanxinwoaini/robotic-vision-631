//TODO:  implement read parameters, write parameters functions...maybe

//test functionality!
/*
******************************************
* FileName	:eeprom.c
* Author	:Russell LeBaron
* Date		:1/21/10
* Version	:0.1
*
******************************************
*
* This code contains driver functions for the Microchip 25LC512 EEPROM.
*  This includes ability to initialize the chip, read and save data
*


*/
#include "eeprom.h"
#include "global.h"

//initializes the EEPROM
void EEPROMInit(void)
{
	sbi(PORTD, EEPROM_nHOLD);//set to high
	sbi(DDRD, DD7); // set direction to output 
		
}
//init nHOLD as output and high



//EEPROMRead - reads back data at specified address, continues to read until nCS is pulled high
// inputs:
//	u16 address //16 bit address of the data you want to access
//	u08 buffer // buffer for reading data into
//	u08 size	// size of the buffer
//output:
//  u08 returns TRUE if successful
//	FALSE otherwise
//this needs testing
u08 EEPROMRead(u16 address, u08* buffer, u08 size)
{	u08 i;
	u08 sreg;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	spiSetCS(SOUND_SPI_SS);
	//while(EEPROMReadStatus() & EEPROM_STATUS_WIP);
	spiSendByte(EEPROM_READ);
	spiSendByte((address>>8)&0x00FF);
	spiSendByte(address & 0xFF);
	for(i = 0; i<size; i++){
		buffer[i]=spiTransferByte(0xFF);
	}
	sbi(PORTC, SOUND_SPI_SS); // stop read
	SREG = sreg;
	return TRUE;
}

//EEPROMWriteByte  write one byte to EEPROM
// input:
//	u16 address // 16 bit address of where you want to write data
//	u08 data	// data you want written to EEPROM
// output:
// 	
void EEPROMWriteByte(u16 address, u08 data)
{	
//	cbi(PORTC, SOUND_SPI_SS);
	u08 sreg;
	//u08 status;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	spiSetCS(SOUND_SPI_SS);
	while(EEPROMReadStatus() & EEPROM_STATUS_WIP){
		//status=EEPROMReadStatus();
		//rprintf("\n\r%x",status);
	}//wait for previous write cycle to finish
	
	cbi(PORTC, SOUND_SPI_SS);
	spiSendByte(EEPROM_WREN);
	sbi(PORTC, SOUND_SPI_SS);
	
	//cbi(PORTC, SOUND_SPI_SS);//RDL DEBUG
	//spiSendByte(EEPROM_WRSR);//RDL DEBUG
	//spiSendByte(0x0);//RDL DEBUG
	//sbi(PORTC, SOUND_SPI_SS);//RDL DEBUG
	//wait?
	//timerPause(255);
	//rprintf("Status = %x  \n\r",EEPROMReadStatus());
	cbi(PORTC, SOUND_SPI_SS);
	spiSendByte(EEPROM_WRITE);
	spiSendByte((address>>8)&0x00FF);
	spiSendByte((address&0x00FF));
	spiSendByte(data);
	sbi(PORTC, SOUND_SPI_SS); // pull nCS high to start write cycle
	//timerPause(255);//delay for 10 ms
	cbi(PORTC, SOUND_SPI_SS);
	while(EEPROMReadStatus() & EEPROM_STATUS_WIP){//wait for previous write to finish
		//rprintf("waiting");
	}
	sbi(PORTC, SOUND_SPI_SS);
	SREG = sreg;
}
//requires EEPROM_WREN instruction to be passed before any data can be written
//nCS must then be toggled high to set the WREN latch
//nCS can then be pulled low again and write may proceed
//Pass the EEPROM_WRITE instruction followed by address to write to
//page writes up to 128 Bytes, must all be in the same page
//after sending data, must release nCS(pull high) for write cycle to complete
//no READ until write cycle is complete, you can read the WRSR(write status register)

//EEPROMWrite write up to one page(128 bytes) to EEPROM
//
void EEPROMWritePage(u16 address, u08* buffer, u08 size)
{
	u08 i;
	u08 sreg;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	spiSetCS(SOUND_SPI_SS);
	while(EEPROMReadStatus() & EEPROM_STATUS_WIP);//wait for previous write to finish
	
	cbi(PORTC, SOUND_SPI_SS);
	spiSendByte(EEPROM_WREN);
	sbi(PORTC, SOUND_SPI_SS);
	//wait?
	cbi(PORTC, SOUND_SPI_SS);
	spiSendByte(EEPROM_WRITE);
	spiSendByte((address>>8)&0x00FF);
	spiSendByte((address&0x00FF));
	for(i=0; i < size; i++){		
		spiSendByte(buffer[i]); //write out size # of bytes
	}
	sbi(PORTC, SOUND_SPI_SS); // pull nCS high to start write cycle
	while(EEPROMReadStatus() & EEPROM_STATUS_WIP){//wait for previous write to finish
		//rprintf("waiting %x",EEPROMReadStatus());
	}
	SREG = sreg;
}

u08 EEPROMReadStatus(void)
{
	u08 status;
	cbi(PORTC, SOUND_SPI_SS);
	// send command
	spiSendByte(EEPROM_RDSR);
	status = spiTransferByte(0xFF);
	// get status register value
	//status = spiTransferByte(0xFF);
	sbi(PORTC, SOUND_SPI_SS);
	return status;
}

#ifdef TESTING_ON
s08 EEPROMTest(void){
	volatile u08 test;
	u08 test2[128];
	u08 i;
	s08 success=TRUE;
	//rprintf("1");
	EEPROMWriteByte(0x0001, 0xDD);
	//rprintf("2");
	EEPROMRead(0x0001, &test, 1);
	//rprintf("Testing single Byte write" );
	TEST(0xDD == test);
	for(i=0;i<128;i++){
		test2[i]=i;
	}
	//rprintf("testing 128 byte write");
	EEPROMWritePage(128, test2, 128);
	for(i=0;i<128;i++){
		test2[i]=i+128;
	}
	EEPROMWritePage(256, test2, 128);
	EEPROMRead(128,test2, 128);
	for(i = 0; i<128; i++){
		//rprintf("test2 at: %d = %x\n\r",i,test2[i]);
		TEST(i == test2[i]);
	}
	EEPROMRead(256,test2, 128);
	for(i = 0; i<128; i++){
		//rprintf("test2 at: %d = %x\n\r",i,test2[i]);
		TEST(i+128 == test2[i]);
	}
	return success;
}
#endif
