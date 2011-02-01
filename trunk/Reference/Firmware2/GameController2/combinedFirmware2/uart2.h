// *****************************************************************************
//
// File Name	: 'uart2.h'
// Title		: SC16IS752 Dual UART driver
// Author		: Jared Havican
// Created		: 01/24/2010
// Revised		: 01/24/2010
// Version		: 0.1
//
// *****************************************************************************

#ifndef UART2_H
#define UART2_H

#include "global.h"

/* Used for packet transmission */
#define UART2_PACKET_BUFFER_SIZE	26 //Size of buffers in bytes used to store received packets
#define UART2_PACKET_BUFFER_NUM		4 //Number of packet buffers
#define UART2_PACKET_STARTBYTE		0x5B // Two of these in a row will indicate a valid packet

/* Default direction of GPIO pins */
/* MSB to LSB = 7 to 0, output=1, input=0 */
#define UART2_GPIO_DEFAULT_IODIRS	0xF8 //Pins 0-2 Inputs, 3-7 Outputs
/* Default IOPin State */
#define UART2_GPIO_DEFAULT_IOSTATE	0x08
/* Default pins on which GPIO will generate interrupt */
//#define UART2_GPIO_DEFAULT_INT		0x07 //Pins 0-2
#define UART2_GPIO_DEFAULT_INT		0x07 //Pins 0-2


/* UART Channels */
#define UART2_LASER	0
#define UART2_XBEE	1

/* Default BAUD Rate Divisors (CLK_RATE/(DIVISOR*16))=BAUD_RATE */
/* CLK_RATE = 1.8432 MHz */
/*
96	=>	1200bps
48	=>	2400bps
24	=>	4800bps
12	=>	9600bps
6	=>	19200bps
3	=>	38400bps
*/
#define UART2_LASER_BAUD_DIVISOR	12
#define UART2_XBEE_BAUD_DIVISOR		3

#define UART2_MODE_READ  1
#define UART2_MODE_WRITE 0


/* Register Offsets */
#define UART2_THR_REGISTER			0x00 // Transmit Holding Register
#define UART2_RHR_REGISTER			0x00 // Receive Holding Register
#define UART2_IER_REGISTER			0x01 // Interrupt Enable Register
#define UART2_FCR_REGISTER			0x02 // FIFO Control Register
#define UART2_IIR_REGISTER			0x02 // Interrupt Identification Register
#define UART2_LCR_REGISTER			0x03 // Line Control Register
#define UART2_MCR_REGISTER			0x04 // Modem Control Register  *MCR[7] can only be modified when EFR[4] is set.
#define UART2_LSR_REGISTER			0x05 // Line Status Register
#define UART2_MSR_REGISTER			0x06 // Modem Status Register
#define UART2_SPR_REGISTER			0x07 // Scratchpad Register
#define UART2_TCR_REGISTER			0x06 // Transmission Control Register  *Accessible only when ERF[4] = 1 and MCR[2] = 1, that is, EFR[4] and MCR[2] are read/write enables.
#define UART2_TLR_REGISTER			0x07 // Trigger Level Register  *Accessible only when ERF[4] = 1 and MCR[2] = 1, that is, EFR[4] and MCR[2] are read/write enables.
#define UART2_TXLVL_REGISTER		0x08 // Transmit Fifo Level Register
#define UART2_RXLVL_REGISTER		0x09 // Receive FIFO Level Register
#define UART2_IODir_REGISTER		0x0A // I/O Pin Direction Register
#define UART2_IOState_REGISTER		0x0B // I/O pins State Register
#define UART2_IOIntEna_REGISTER		0x0C // I/O Interrupt Enable Register
#define UART2_IOControl_REGISTER	0x0E // I/O pins Control Register
#define UART2_EFCR_REGISTER			0x0F // Extra Features Control Register

/* Special Register Set - Accessible only when LCR[7] = logic 1 and LCR is not 0xBF. */
#define UART2_DLL_REGISTER			0x00 // Divisor Latch LSB
#define UART2_DLH_REGISTER			0x01 // Divisor Latch MSB

/* Enhanced Register Set - Accessible when LCR = 0xBF. */
#define UART2_EFR_REGISTER			0x02 // Enhanced Features Register
#define UART2_XON1_REGISTER			0x04 // Xon1 Word
#define UART2_XON2_REGISTER			0x05 // Xon2 Word
#define UART2_XOFF1_REGISTER		0x06 // Xoff1 Word
#define UART2_XOFF2_REGISTER		0x07 // Xoff2 Word

/* Interrupt Identification Register (IIR) values */
#define UART2_IIR_RHR	0x04 // Receive Holding Register Interrupt
#define UART2_IIR_RTO	0x0C // Receive Time-out Interrupt
#define UART2_IIR_THR	0x02 // Transmission Holding Register Interrupt
#define UART2_IIR_IO	0x30 // Input pin changed state

/* Dual UART Interrupt Pin */
#define UART2_INT_SIGNAL			INT1_vect

#ifndef UART2_INTERRUPT_HANDLER
#define UART2_INTERRUPT_HANDLER		SIGNAL
#endif

void uart2DisableLaserInt();
void uart2EnableLaserInt();

void uart2DisableXbeeInt();
void uart2EnableXbeeInt();

void uart2DisableGPIOInt();
void uart2EnableGPIOInt();

/* Send byte to dual uart over SPI */
void uart2SPISendByte(u08 data);

/* Send word to dual uart over SPI */
void uart2SPISendWord(u16 data);

/* Send/Receive byte to/from dual uart over SPI */
u08 uart2SPITransferByte(u08 data);

/* Writes to a register in the dual uart */
void uart2WriteReg(u08 channel, u08 reg, u08 data);

/* Reads a register in the dual uart */
u08 uart2ReadReg(u08 channel, u08 reg);

/* Sets the baud rate divisor for each channel, BAUD Rate=(Clock/(16*divisor))*/
void uart2SetBaudDivisor(u08 channel, u16 divisor);

/* Send null terminated string over XBEE */
void uart2XbeeSendString(u08* str);

/* Send null terminated string over Laser */
void uart2LaserSendString(u08* str);

/* Initialize Dual UART and enable interrupts */
void uart2Init(void);

/* Waits for transmit register to clear, then tranfers byte */
void uart2SendByte(u08 channel, u08 data);

/* Sends a packet starting at start of length size */
void uart2SendXBeePacket(void * start, u08 size);

/* Reads a byte from uart, returns -1 if no data available */
u08 uart2ReadByte(u08 channel);

/* Sets the GPIO pin as input=0 or output=1 */
/* Ordering is pin 7 downto 0, MSB to LSB */
void uart2GPIOSetIODir(u08 dirs);

/* Reads the state of all GPIO pins */
/* Ordering is pin 7 downto 0, MSB to LSB */
u08 uart2GPIOReadState();

/* Sets the state of all GPIO pins */
/* Ordering is pin 7 downto 0, MSB to LSB */
void uart2GPIOSetState(u08 data);

/* Sets the state of GPIO pins specified by the mask */
/* Sets the 1 bits in the mask to value */
void uart2GPIOSetMask(u08 mask, u08 value);

/* Sets the GPIO Handler for the IOs */
void uart2SetGPIOHandler(void (*rx_func)(u08 c));

/* Sets the RX Handler for a UART channel */
void uart2SetRxHandler(u08 channel, void (*rx_func)(u08 c));

/* Sets the RX Handler for the Xbee */
void uart2SetXBeeHandler(void (*rx_func)(void * c));

/* Sets the RX Handler for the Laser/Photodiode */
void uart2SetLaserHandler(void (*rx_func)(u08 c));

/* Tests the Dual UART */
#ifdef TESTING_ON
s08 UART2Test();
#endif

#endif