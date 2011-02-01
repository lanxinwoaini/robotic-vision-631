// *****************************************************************************
//
// File Name	: 'uart2.c'
// Title		: SC16IS752 Dual UART driver
// Author		: Jared Havican
// Created		: 01/24/2010
// Revised		: 01/24/2010
// Version		: 0.1
//
// *****************************************************************************

#include "global.h"

#include <avr/io.h>
#include <avr/interrupt.h>

#include "uart2.h"
#include "spi.h"
#include "UnitTest.h"
#include "rprintf.h"
#include "timer.h"

/* Buffers to store rx packets */
volatile static u08 rxBuffers[UART2_PACKET_BUFFER_NUM][UART2_PACKET_BUFFER_SIZE];
volatile static u08 currBuff;


/* User-defined RX interrupt handlers */
typedef void (*voidFuncPtru08)(unsigned char);
volatile static voidFuncPtru08 Uart2LaserRxFunc;
volatile static voidFuncPtru08 Uart2GPIORxFunc;
typedef void (*voidFuncPtrvPtr)(void *);
volatile static voidFuncPtrvPtr Uart2XBeeRxFunc;

/* Flag indicating able to transmit -- Not Used */
//volatile static u08 uart2ReadyTx[2];

void uart2DisableLaserInt(){
	// Disable Interrupt
	uart2WriteReg(UART2_LASER, UART2_IER_REGISTER, 0);
}

void uart2EnableLaserInt(){
	// Clear RX Fifo
	uart2WriteReg(UART2_LASER, UART2_FCR_REGISTER, uart2ReadReg(UART2_LASER, UART2_FCR_REGISTER)|0x01);
	// Enable RX Interrupt
	uart2WriteReg(UART2_LASER, UART2_IER_REGISTER, BV(0));
}

void uart2DisableXbeeInt(){
	// Disable Interrupt
	uart2WriteReg(UART2_XBEE, UART2_IER_REGISTER, 0);
}

void uart2EnableXbeeInt(){
	// Clear RX Fifo
	uart2WriteReg(UART2_XBEE, UART2_FCR_REGISTER, uart2ReadReg(UART2_XBEE, UART2_FCR_REGISTER)|0x01);
	// Enable RX Interrupt
	uart2WriteReg(UART2_XBEE, UART2_IER_REGISTER, BV(0));
}

void uart2DisableGPIOInt(){
	// Disable Interrupts
	uart2WriteReg(0, UART2_IOIntEna_REGISTER, 0);
}

void uart2EnableGPIOInt(){
	// Enable Interrupts
	uart2WriteReg(0, UART2_IOIntEna_REGISTER, UART2_GPIO_DEFAULT_INT);
}

/* Send byte to dual uart over SPI */
void uart2SPISendByte(u08 data){
	u08 sreg;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	spiSetCS( UART2_SPI_SS );
	
	spiSendByte(data);
	
	spiSetCS( OFF_SPI_SS );
	SREG = sreg;//restore interrupts
}

/* Send word to dual uart over SPI */
void uart2SPISendWord(u16 data){
	u08 sreg;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	spiSetCS( UART2_SPI_SS );
	
	spiSendByte(data>>8 & 0x00FF); // Send MSB
	spiSendByte(data & 0x00FF); //Send LSB
	
	spiSetCS( OFF_SPI_SS );
	SREG = sreg;//restore interrupts
}

/* Send/Receive byte to/from dual uart over SPI */
u08 uart2SPITransferByte(u08 data){
	u08 sreg;
	u08 retval;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	spiSetCS( UART2_SPI_SS );

	spiTransferByte(data);
	retval = spiTransferByte(0xFF);
	
	spiSetCS( OFF_SPI_SS );
	SREG = sreg;//restore interrupts
	return retval;
}

/* maybe convert to macro? */
/* Writes to a register in the dual uart */
void uart2WriteReg(u08 channel, u08 reg, u08 data){
	//u16 data;
	//data = (UART2_MODE_WRITE << 15 & 0x8000) | (reg << 11 & 0x7800) | (channel << 9 & 0x0300) | data;
	uart2SPISendWord(UART2_MODE_WRITE<<15 | reg<<11 | channel<<9 | data);
}

/* maybe convert to macro? */
/* Reads a register in the dual uart */
u08 uart2ReadReg(u08 channel, u08 reg){
	return uart2SPITransferByte(UART2_MODE_READ<<7 | reg<<3 | channel<<1);
}

/* Sets the baud rate divisor for each channel, BAUD Rate=(Clock/(16*divisor))*/
void uart2SetBaudDivisor(u08 channel, u16 divisor){
	u08 lcr_temp = uart2ReadReg(channel, UART2_LCR_REGISTER); //Save LCR value
	
	uart2WriteReg(channel, UART2_LCR_REGISTER, BV(7)); //Enable Divisor Latch
	uart2WriteReg(channel, UART2_DLH_REGISTER, (u08)(divisor>>8)); //Set High Byte
	uart2WriteReg(channel, UART2_DLL_REGISTER, (u08)divisor); //Set Low Byte
	
	uart2WriteReg(channel, UART2_LCR_REGISTER, lcr_temp); //Restore LCR value
}

void uart2XbeeSendString(u08* str){
	while (*str!=0){
		uart2SendByte(UART2_XBEE, *str);
		str++;
	}
}

void uart2LaserSendString(u08* str){
	while (*str!=0){
		uart2SendByte(UART2_LASER, *str);
		str++;
	}
}

/* Initialize Dual UART and enable interrupts */
void uart2Init(void){
	//Disable Interrupts
	u08 sreg;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	
	/* Initialize UARTs */
	currBuff = 0;
	//Clear user defined RX interrupt handlers
	Uart2LaserRxFunc=0;
	Uart2GPIORxFunc=0;
	Uart2XBeeRxFunc=0;
	
	//Set External Interrupt 1 to be rising edge triggered
	//outb(EICRA, BV(ISC11)|BV(ISC10)|inb(EICRA));
	//Set External Interrupt 1 to be low level sensitive
	outb(EICRA, inb(EICRA)&~(BV(ISC11)|BV(ISC10)));
	//Enable External Interrupt 1
	sbi(EIMSK, INT1);
	
	//Enable enhanced functionality in Laser UART
	uart2WriteReg(UART2_LASER, UART2_LCR_REGISTER, 0xBF);
	uart2WriteReg(UART2_LASER, UART2_EFR_REGISTER, BV(4));
	
	//Enable hardware flow control for xbee
	uart2WriteReg(UART2_XBEE, UART2_LCR_REGISTER, 0xBF);
	uart2WriteReg(UART2_XBEE, UART2_EFR_REGISTER, BV(7)|BV(6));
	
	//Set LCR, No Parity, 1 stop bit, 8 bit word length
	uart2WriteReg(UART2_LASER, UART2_LCR_REGISTER, BV(2)|BV(1)|BV(0));
	uart2WriteReg(UART2_XBEE, UART2_LCR_REGISTER, BV(2)|BV(1)|BV(0));
	
	//Set Baud Rate Divisors
	uart2SetBaudDivisor(UART2_LASER, UART2_LASER_BAUD_DIVISOR);
	uart2SetBaudDivisor(UART2_XBEE, UART2_XBEE_BAUD_DIVISOR);

	//Set MCR, Laser UART to operate in IRDA mode	
	uart2WriteReg(UART2_LASER, UART2_MCR_REGISTER, BV(6));
	
	//Set up FIFOs
	uart2WriteReg(UART2_LASER, UART2_FCR_REGISTER, BV(0)); //8 char rx trigger
	uart2WriteReg(UART2_XBEE, UART2_FCR_REGISTER, BV(7)|(BV(0))); //32 char rx trigger
	
	//Enable RX interrupts on Dual UART
	//uart2WriteReg(UART2_LASER, UART2_IER_REGISTER, BV(0));
	//uart2WriteReg(UART2_XBEE, UART2_IER_REGISTER, BV(0));

	//Set TX ready flag to true -- Not used
	//uart2ReadyTx[0] = TRUE;
	//uart2ReadyTx[1] = TRUE;
	
	/* Set up GPIO */
	//Set pins to default IO directions
	uart2GPIOSetIODir(UART2_GPIO_DEFAULT_IODIRS);
	//Set pins to default values
	uart2GPIOSetState(UART2_GPIO_DEFAULT_IOSTATE);
	//Set up GPIO pins to latch data until state register is read
	uart2WriteReg(0, UART2_IOControl_REGISTER, BV(0));
	//Enable interrupt generation on default pins
	//uart2WriteReg(0, UART2_IOIntEna_REGISTER, UART2_GPIO_DEFAULT_INT);

	
	//Enable Interrupts
	SREG=sreg;
}

/* Waits for transmit register to clear, then tranfers byte */
void uart2SendByte(u08 channel, u08 data){
	//while(!uart2ReadyTx[channel]);
	// Set TX ready flag to false -- Not Used
	//uart2ReadyTx[channel] = FALSE;

	// Wait for available spaces in TX FIFO
	while(uart2ReadReg(channel, UART2_TXLVL_REGISTER)==0);
	
	// Send data to transmit register
	uart2WriteReg(channel, UART2_THR_REGISTER, data);
}

/* Sends a packet starting at start of length size */
void uart2SendXBeePacket(void * start, u08 size){
	//disable interupts ? we shouldnt need to as long as this isnt called from interrupt code
	uart2SendByte(UART2_XBEE, UART2_PACKET_STARTBYTE); //Send First Start Byte
	uart2SendByte(UART2_XBEE, UART2_PACKET_STARTBYTE); //Send Second Start Byte
	
	uart2SendByte(UART2_XBEE, size); //Send Packet Size
	
	u08 i;
	u08 * ptr = (u08 *) start;
	for (i=0;i<size;i++){ //Send data
		uart2SendByte(UART2_XBEE, *ptr);
		ptr++;
	}
}

/* Reads a byte from uart, returns -1 if no data available */
u08 uart2ReadByte(u08 channel){
	/* return -1 if no data in receive register */
	if (!(uart2ReadReg(channel, UART2_LSR_REGISTER) & 0x01)){
		return -1;
	}
	/* return received byte */
	return uart2ReadReg(channel, UART2_RHR_REGISTER);
}

/* Sets the GPIO pin as input=0 or output=1 */
/* Ordering is pin 7 downto 0, MSB to LSB */
void uart2GPIOSetIODir(u08 dirs){
	uart2WriteReg(0, UART2_IODir_REGISTER, dirs);
}

/* Reads the state of all GPIO pins */
/* Ordering is pin 7 downto 0, MSB to LSB */
u08 uart2GPIOReadState(){
	return uart2ReadReg(0, UART2_IOState_REGISTER);
}

/* Sets the state of all GPIO pins */
/* Ordering is pin 7 downto 0, MSB to LSB */
void uart2GPIOSetState(u08 data){
	uart2WriteReg(0, UART2_IOState_REGISTER, data);
}

/* Sets the state of GPIO pins specified by the mask */
/* Sets the 1 bits in the mask to value */
void uart2GPIOSetMask(u08 mask, u08 value){
	u08 vals = uart2GPIOReadState();
	if (value) {
		vals = vals | mask; // Set mask bits high
	} else {
		vals = vals & ~mask; // Set mask bits low
	}
	uart2GPIOSetState(vals);
}

/* Sets the GPIO Handler for the IOs */
void uart2SetGPIOHandler(void (*rx_func)(u08 c)){
	Uart2GPIORxFunc = rx_func;
}

/* Sets the RX Handler for the Xbee */
void uart2SetXBeeHandler(void (*rx_func)(void * c)){
	Uart2XBeeRxFunc = rx_func;
}

/* Sets the RX Handler for the Laser/Photodiode */
void uart2SetLaserHandler(void (*rx_func)(u08 c)){
	Uart2LaserRxFunc = rx_func;
}

/* Called when data is received from xbee */
/* Builds a packet and then calls Uart2XBeeRxFunc */
void UART2_XBEE_HANDLER(u08 c){
	u08 sreg;
	sreg = SREG; //save interrupt state
	cli();//disable interrupts
	
	static u08 byteCount=0;
	static u08 packetSize=0;
	static u08 * buffPtr=rxBuffers[0];
	
	switch(byteCount){
		case 0: //First Start Byte
			if (c==UART2_PACKET_STARTBYTE){ //Check for first start byte
				byteCount++; 
			}
			break;
		case 1: //Second Start Byte
			if (c==UART2_PACKET_STARTBYTE){ //Check for second start byte
				byteCount++;
			} else {
				byteCount=0; //If not go back to start
			}
			break;
		case 2: //Packet Size Byte
			byteCount++;
			packetSize=c;
			break;
		default: //Data bytes
			*buffPtr = c; //Put byte into buffer
			buffPtr++; //Increment buffer location
			packetSize--; //Count down to 0
			byteCount++; //Increment byte count
			if (packetSize==0) { //End of packet
				void * oldBuff = (void*)rxBuffers[currBuff]; //Save old buffer location
				
				byteCount=0; //Reset byteCount
				currBuff++; // Go to next packet buffer
				if (currBuff>=UART2_PACKET_BUFFER_NUM) currBuff=0; //check for wrap around
				buffPtr=rxBuffers[currBuff]; //Go to next packet buffer
				
				SREG=sreg; // enable Interrupts
				
				if (Uart2XBeeRxFunc) Uart2XBeeRxFunc(oldBuff); //Call xbee handler with pointer to packet
			}
			break;
	}

	SREG=sreg; //enable interrupts
}

// UART2 Interrupt Handler
UART2_INTERRUPT_HANDLER(UART2_INT_SIGNAL){
	u08 chan;
	for(chan=0;chan<2;chan++){
		// Read Interrupt Identification Register and clear top two bits
		u08 iir = uart2ReadReg(chan, UART2_IIR_REGISTER) & 0x3F; 
		// Byte where received byte is stored
		u08 c;
		
		switch(iir){ //Which Interrupt Fired?
			case UART2_IIR_RHR: // Receive Interrupt
			case UART2_IIR_RTO: // Receive time-out Interrupt
				c = uart2ReadReg(chan, UART2_RHR_REGISTER); //Read byte
				if (chan==UART2_LASER){
					if (Uart2LaserRxFunc) Uart2LaserRxFunc(c); // Call Laser Handler
					//if (c!=0xFF) rprintf("%c", c);
				}
				if (chan==UART2_XBEE){
					UART2_XBEE_HANDLER(c); // Call Xbee Handler 
				}
			break;
			case UART2_IIR_THR: // Transmit Interrupt -- Not Used
				//uart2ReadyTx[chan] = TRUE; // Indicate that transmission is ready
			break;
			case UART2_IIR_IO: //GPIO Interrupt
				c = uart2GPIOReadState();
				if (Uart2GPIORxFunc) Uart2GPIORxFunc(c); //Call GPIO Handler
			break;
		}
	}
	
	//Check GPIO Interrupt
}

/* Tests the Dual UART */
#ifdef TESTING_ON
s08 UART2Test(){
	s08 success=TRUE;
	// Write to the scratchpad register and check the result
	uart2WriteReg(UART2_XBEE, UART2_SPR_REGISTER, 0xDF);
	TEST(0xDF == uart2ReadReg(UART2_XBEE, UART2_SPR_REGISTER));
	
	// Write to the scratchpad register and check the result
	uart2WriteReg(UART2_XBEE, UART2_SPR_REGISTER, 0xDD);
	TEST(0xDD == uart2ReadReg(UART2_XBEE, UART2_SPR_REGISTER));
	return success;
}
#endif

