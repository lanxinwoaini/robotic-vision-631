/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    Wireless.c
AUTHOR:  Wade Fife
CREATED: 12/10/04

DESCRIPTION

This module contains code to handle sending and receiving to wireless
port. It consists of send and receive queues for buffering data as
well as the functions for enqueing data to be sent and dequeuing data
that has been received. The actual transfer of data to and from the
serial port is interrupt driven.

CHANGE LOG

02/04/06 WSF Updated to be uC/OS independent.

******************************************************************************/


// INCLUDES ///////////////////////////////////////////////////////////////////

#include <xgpio_l.h>
#include <xuartlite_l.h>
#include <xparameters.h>

#include "SystemTypes.h"
#include "Wireless.h"
#include "InterruptControl.h"

#include "RX.h"
#include "Packet.h"
#include "MemAlloc.h"
#include "State.h"
#include "tx.h"


// DEFINES ////////////////////////////////////////////////////////////////////

#define RECV_BUFFER_SIZE 2048
#define SEND_BUFFER_SIZE 1024



// MACROS /////////////////////////////////////////////////////////////////////

#define RECV_ENQUEUE(DATA) recvBuffer.data[recvBuffer.tail++] = (DATA); \
                           recvBuffer.size++; \
		                   if(recvBuffer.tail >= RECV_BUFFER_SIZE) \
                               recvBuffer.tail = 0;

#define SEND_ENQUEUE(DATA) sendBuffer.data[sendBuffer.tail++] = (DATA); \
		                   sendBuffer.size++; \
		                   if(sendBuffer.tail >= SEND_BUFFER_SIZE) \
                               sendBuffer.tail = 0;

#define RECV_DEQUEUE() recvBuffer.data[recvBuffer.head++]; \
		               recvBuffer.size--; \
		               if(recvBuffer.head >= RECV_BUFFER_SIZE) \
                           recvBuffer.head = 0;

#define SEND_DEQUEUE() sendBuffer.data[sendBuffer.head++]; \
		               sendBuffer.size--; \
		               if(sendBuffer.head >= SEND_BUFFER_SIZE) \
                           sendBuffer.head = 0;



// DATA TYPES /////////////////////////////////////////////////////////////////

typedef struct {
	int head;
	int tail;
	int size;
	uint8 *data;
} Queue;



// STATIC GLOBALS /////////////////////////////////////////////////////////////

static uint8* recvBufferData;
static uint8* sendBufferData;
static uint8* bpBufferWireless;
static Queue sendBuffer;
static Queue recvBuffer;


// WIRELESS INTERRUPT HANDLER /////////////////////////////////////////////////

// IMPORTANT NOTE ON MODEM'S CTS SIGNAL: If CTS is de-asserted (when
// the modem's output FIFO is full) the interrupt handler can't
// transfer the next byte and must return. Also, since no byte is
// queued in the UARTS TX FIFO, the interrupt may not be asserted
// again, which means the transfer mechanism may stop functioning.
//
// To remedy this situation, the WirelessInterruptHandler() must be
// called manually. An easy solution is to call
// WirelessInterruptHandler() in a periodic timer handler (i.e., the
// system timer interrupt).
//
// Also, if the wireless handler is called at regular intervals
// (e.g., by a timer handler) then it becomes unnecessary to use the
// wireless handler as an interrupt handler for the UART, unless a
// quick response time is desired.

void WirelessInterruptHandler(void)
{
	uint8 byte;


	DISABLE_INTERRUPTS();

	// Check if data has been received:
	if(!XUartLite_mIsReceiveEmpty(WIRELESS_BASEADDR)) {
		RECV_ENQUEUE((Xuint8)XIo_In32(WIRELESS_BASEADDR + XUL_RX_FIFO_OFFSET));
	}

#ifdef CHECK_MODEM_CTS
	if(MODEM_CTS())
#endif
		// Check for data to send and check if transmit buffer has room
		while(sendBuffer.size > 0 &&
			  !XUartLite_mIsTransmitFull(WIRELESS_BASEADDR)) {

			// Get byte from send buffer and send it to serial port
			byte = SEND_DEQUEUE();
			XUartLite_SendByte(WIRELESS_BASEADDR, byte);
		}

	RESTORE_INTERRUPTS(msr);
}

int findNewStartIndex(uint8* data,uint32 length){
	uint32 i = 0;
	uint32 index = 0;
	while (++i < length){ 
		if     (index == 0 && data[i] != 0xFA) {index =0; continue;}
		else if(index == 1 && data[i] != 0xCA) {index =0; continue;}
		else if(index == 2 && data[i] != 0xDE) {index =0; continue;}
		index++;
		if(index == 3) break; //we've gotten to the end of the FACADE intro to the header, so quit
	}
	//if we found something, bubble the data to be right
	uint32 srcI = i - index + 1; // i increments before it exits
	uint32 dstI = 0;

	while(srcI < length){
		data[dstI++] = data[srcI++];
	}
	return dstI; //this is the new index
}

//This function is only called when there is data ready
void WirelessReceiveParser()
{
	static uint32 index = 0;
	static HeliosCommHeader hch;

	while(recvBuffer.size>0){
		DISABLE_INTERRUPTS();
		char c = RECV_DEQUEUE();
		RESTORE_INTERRUPTS(msr);
		//sendOneHex((uint32)c,DISPLAY_IN_GUI_NONEWLINE);
			
		//keep skipping data until the proper header intro is found
		if     (index == 0 && c != 0xFA) {index =0; continue;}
		else if(index == 1 && c != 0xCA) {index =0; continue;}
		else if(index == 2 && c != 0xDE) {index =0; continue;}
		bpBufferWireless[index++] = c;

		if(index == PACKET_HEADER_SIZE) {
			ReadInHeader(&hch,bpBufferWireless);
			if (hch.checkSum) {
				SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,3,"~h~");
				//sendHex(hch.checkSum,DISPLAY_IN_GUI_NEWLINE);
				//sendHex(*(((uint32*)&hch)),DISPLAY_IN_GUI_NEWLINE);
				//sendHex(*(((uint32*)&hch)+1),DISPLAY_IN_GUI_NEWLINE);
				//re-search for any beginning of a packet
				index = findNewStartIndex(bpBufferWireless,index); //it was bad
				//sendHex(bpBufferWireless[0],DISPLAY_IN_GUI_NEWLINE);
			}
		}

		if(index >= PACKET_HEADER_SIZE){
			if(index == PACKET_HEADER_SIZE + hch.bufferSize){
				if (GetChecksum(&bpBufferWireless[PACKET_HEADER_SIZE],hch.bufferSize)){
					SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,3,"~d~"); //the packet was bad
					//for (i = PACKET_HEADER_SIZE; i < index; i++){
					//	sendOneHex(bpBufferWireless[i],DISPLAY_IN_GUI_NONEWLINE);
					//}
					//SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,1,".");
					//re-search through the bpBufferWireless for the beginning of another packet (to minimize indirect packet loss):
					index = findNewStartIndex(bpBufferWireless,index);
				} else {
					hch.bufferSize-=2; //remove the checksum from the end of the buffer
					RX_ProcessPacket(&hch,&bpBufferWireless[PACKET_HEADER_SIZE]); //we now have enough data to process
					index =0;
				}
			}		
		}
	}//End: while(recvBuffer.size>0)

}



// WIRLESS I/O CODE ///////////////////////////////////////////////////////////


// Setup hardware and software for interrupt based wireless transfer.
void WirelessInit(void)
{
	//allocate memory for Buffers
	recvBufferData	= (uint8*) MemAlloc(RECV_BUFFER_SIZE);
	sendBufferData  = (uint8*) MemAlloc(SEND_BUFFER_SIZE);
	bpBufferWireless= (uint8*) MemAlloc(RECV_BUFFER_SIZE);


	// Initialize data structures
	sendBuffer.data = sendBufferData;
	sendBuffer.size = 0;
	sendBuffer.head = 0;
	sendBuffer.tail = 0;

	recvBuffer.data = recvBufferData;
	recvBuffer.size = 0;
	recvBuffer.head = 0;
	recvBuffer.tail = 0;

	// Setup wireless handler
	XUartLite_mEnableIntr(WIRELESS_BASEADDR);
}



#ifdef USE_WIRE_SEND_DATA ////////////////////////

int WireSendDataFull_lowPriority(uint8 *dataHeader, int numBytesHeader, uint8 *dataData, int numBytesData)
{
	DISABLE_INTERRUPTS();
	if((sendBuffer.size/2 > numBytesHeader + numBytesData + 3) ||
		(sendBuffer.size + numBytesHeader + numBytesData + 3 > SEND_BUFFER_SIZE)) { //if there's already too much stuff in queue
		RESTORE_INTERRUPTS(msr);
		return 0;
	}
	RESTORE_INTERRUPTS(msr);
	return WireSendDataFull(dataHeader,numBytesHeader,dataData,numBytesData);
}

int WireSendDataFull(uint8 *dataHeader, int numBytesHeader, uint8 *dataData, int numBytesData)
{
	//const int zeroBuffer = 0;
	{
		DISABLE_INTERRUPTS();
		if(sendBuffer.size + numBytesHeader + numBytesData + 3 > SEND_BUFFER_SIZE) {
			RESTORE_INTERRUPTS(msr);
			return 0;
		}
		RESTORE_INTERRUPTS(msr);
	}

	short checksum = GetChecksum(dataData,numBytesData);

	DISABLE_INTERRUPTS();
	WireSendData(dataHeader, numBytesHeader); //send header
	WireSendData(dataData,   numBytesData); //send data
	WireSendData((uint8*)&checksum, 2);//send the checksum data
	RESTORE_INTERRUPTS(msr);
	return 1;
}

int WireSendDataTri(uint8 *dataHeader, int numBytesHeader, uint8 *dataData1, int numBytesData1, uint8 *dataData2, int numBytesData2)
{
	//const int zeroBuffer = 0;
	{
		DISABLE_INTERRUPTS();
		if(sendBuffer.size + numBytesHeader + numBytesData1 + numBytesData2 + 3 > SEND_BUFFER_SIZE) {
			RESTORE_INTERRUPTS(msr);
			return 0;
		}
		RESTORE_INTERRUPTS(msr);
	}

	//calculate the checksum:
	uint8* dataArray[] = {dataData1, dataData2};
	uint16 numBytesArray[] = {numBytesData1, numBytesData2};
	short checksum = GetChecksum_multi(2,dataArray,numBytesArray);

	DISABLE_INTERRUPTS();
	WireSendData(dataHeader, numBytesHeader); //send header
	WireSendData(dataData1,   numBytesData1); //send data
	WireSendData(dataData2,   numBytesData2); //send data
	WireSendData((uint8*)&checksum, 2);//send the checksum data
	RESTORE_INTERRUPTS(msr);
	return 1;
}

// Buffers the indicated number of bytes for transfer over wireless erial port.
// Returns 1 if successfull, 0 otherwise.
int WireSendData(uint8 *data, int numBytes)
{
	uint8 byte;

	DISABLE_INTERRUPTS();

	// Check for buffer overflow
	if(sendBuffer.size + numBytes + 4 > SEND_BUFFER_SIZE) { //extra size for checksum
		RESTORE_INTERRUPTS(msr);
		return 0;
	}

	// Adjust buffer to indicate new size
	sendBuffer.size += numBytes;

	// Queue up data
	while(numBytes--) {
		sendBuffer.data[sendBuffer.tail++] = *data++;
		if(sendBuffer.tail >= SEND_BUFFER_SIZE) sendBuffer.tail = 0;
	}

	// Check if hardware TX FIFO is empty
#ifdef CHECK_MODEM_CTS
	if(MODEM_CTS())
#endif
		if(XUartLite_mGetStatusReg(WIRELESS_BASEADDR) & XUL_SR_TX_FIFO_EMPTY) {
			// Start transfer of first byte; rest will be sent by int handler.
			byte = SEND_DEQUEUE();
			XUartLite_SendByte(WIRELESS_BASEADDR, byte);
		}

	RESTORE_INTERRUPTS(msr);

	return 1;
}

#endif





//returns the bytes free in the send buffer
int getSendBufferFreeSpace(){
	return SEND_BUFFER_SIZE - sendBuffer.size;
}
int getSendBufferSize(){
	return sendBuffer.size;
}
