/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:	 plb_usb.c
AUTHOR:	 Wade Fife
CREATED: 02/25/06

DESCRIPTION

Driver API for the plb_usb module.

NOTES

Efficiency

For the most efficient operation, data should be sent in large chunks. If data
is sent in small chuncks or a word at a time, performance will be very poor on
both the host and peripheral ends of the USB link.

Committing Packets

The Cypress FX2LP chip consists of several buffers. As soon as a buffer fills
the buffer is automatically committed for transfer by the Cypress chip. The
packet can't be sent over USB until it is committed. The buffer sizes are
typically 512 bytes for high-speed USB and 64 bytes for full-speed USB. To
commit packets smaller than 512 bytes you can manually commit the current
buffer by calling the USB_Commit() macro. If you are not keeping track of the
number of bytes sent, you may want to always commit after writing your
data. This will ensure that the last packet gets sent.

Blocking

The send and receive functions in this file will busy wait until data can be
sent or received by polling the USB core status word.

CHANGE LOG

02/25/06 WSF Created initial file.
03/07/06 WSF Replaced macros with functions. Made USB_Read16/USB_Write16
             blocking. Added macro version in ALL caps for non-blocking
             use. Added USB_CTRL_COMMIT #define. Revised comments and
             documentation.

******************************************************************************/

#include "plb_usb.h"



// DEFINES ////////////////////////////////////////////////////////////////////

#define USB_CTRL_COMMIT          0x00000001

#define USB_OFFSET_STATUS_CONTROL		0x40
#define USB_OFFSET_DATA					0x48
#define USB_OFFSET_PLB_BURST_SIZE		0x50
#define USB_OFFSET_BURST_TRANS_SIZE		0x58
#define USB_OFFSET_BURST_START_ADDR		0x60

// MACROS /////////////////////////////////////////////////////////////////////

#define PTR(OFFSET) ((Xuint32 *)(XPAR_PLB_USB_0_BASEADDR + OFFSET))

// GLOBALS ////////////////////////////////////////////////////////////////////

volatile Xuint32 *gUSB_StatusControlAddr;
volatile Xuint16 *gUSB_Data16;

volatile Xuint32 *gUSB_BurstStartAddr;
volatile Xuint32 *gUSB_BurstTransSize;

// API FUNCTIONS //////////////////////////////////////////////////////////////



// Call to initialize USB memory address pointers before using any API
// function.
void USB_Initialize()
{
	Xuint32 baseAddr 		= XPAR_PLB_USB_0_BASEADDR;
	gUSB_StatusControlAddr	= (Xuint32 *)(baseAddr + USB_OFFSET_STATUS_CONTROL);
	gUSB_Data16				= (Xuint16 *)(baseAddr + USB_OFFSET_DATA);
	gUSB_BurstStartAddr		= (Xuint32 *)(baseAddr + USB_OFFSET_BURST_START_ADDR);
	gUSB_BurstTransSize		= (Xuint32 *)(baseAddr + USB_OFFSET_BURST_TRANS_SIZE);	
}



// Returns the status word for the USB core;
Xuint32 USB_Status(void){
	return (*gUSB_StatusControlAddr);
}

Xuint32 USB_DataReady(){
	return ((USB_Status() & USB_RECV_EMPTY) == 0);
}

// Writes a single word to the USB. Does NOT commit. Busy waits if the send
// FIFO is full.
void USB_Write16(Xuint16 value)
{
	// Wait until send FIFO not full and not in a burst_session
	while((USB_Status() & 0x00F1) != 0x30);
	//while(*gUSB_StatusControlAddr & (USB_SEND_FULL | 0x40));

	*gUSB_Data16 = value;
}



// Writes numWords 16-bit values to the USB. Data NOT commit. Busy waits if the
// send FIFO is full.
void USB_SendData16(Xuint16 *source, int numWords){
	int i;
	for(i = 0 ; i < numWords ;i++)
		USB_Write16(source[i]);
	
	/*while(numWords--){
		USB_Write16(*source);
		source++;
	}*/ 
}




// Writes numWords 16-bit values to the USB (using USB_SendData16()) then
// commits the current buffer.
void USB_SendPacket16(Xuint16 *source, int numWords)
{
	USB_SendData16(source, numWords);
	USB_Commit();
}



// Reads a single word from the USB. Busy waits if data is not available.
Xuint16 USB_Read16(void)
{
	// Wait until data available and not in a burst_session
	while(*gUSB_StatusControlAddr & (USB_RECV_EMPTY | 0x40));

	return *gUSB_Data16;
}



// Reads numWords 16-bit values from the USB. Busy waits if data is not
// available.
void USB_RecvData16(Xuint16 *dest, int numWords)
{
	while(numWords--) {		
		*dest = USB_Read16();
		dest++;
	}
}


// Sends commit instruction to the USB chip.
void USB_Commit(void){
	while(*gUSB_StatusControlAddr & (USB_SEND_FULL | 0x40));
	
	*gUSB_StatusControlAddr = USB_CTRL_COMMIT;
}



/*
 *
 *
 */
void USB_BurstData16(Xuint16 *source, int num16bitWords){
	while((USB_Status() & 0x00F1) != 0x30);
	*gUSB_BurstStartAddr  	= (Xuint32) source;		
	*gUSB_BurstTransSize  	= (Xuint32) num16bitWords;
	*gUSB_StatusControlAddr	= 0;
	//while(USB_Status() & 0x0040); // DO NOT RETURN UNTIL THE ENTIRE FRAME HAS BEEN TRANSMITTED
	// this is a temporary solution until i can find out why the PLB bus looses data
}

void USB_SetPLBBurstSize(Xuint32 size){
	*PTR(USB_OFFSET_PLB_BURST_SIZE) = size;
}

