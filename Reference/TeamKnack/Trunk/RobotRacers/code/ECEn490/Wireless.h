/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    Wireless.h
AUTHOR:  Wade Fife
CREATED: 12/10/04

DESCRIPTION

Header file for Wireless.c

******************************************************************************/

#ifndef WIRELESS_H
#define WIRELESS_H



// USER CONFIGURATION /////////////////////////////////////////////////////////

// Define wich functions you wish to include in compilation. Please
// note that there may be dependencies between functions.
#define USE_WIRE_SEND_DATA    // Queues a packet of data for trasnfer


// Define the following to be the base address of the wireless UART
#define WIRELESS_BASEADDR (XPAR_OPB_UART_WIRELESS_BASEADDR)

// Define this if you want CTS to be checked before sending more
// data. Otherwise undef it. See note for WirelessInterruptHandler()
// in Wireless.c for more information.
#define CHECK_MODEM_CTS 1

// Define this to return the value of the CTS signal (positive asserted)
#define MODEM_CTS() \
        ((~XGpio_mGetDataReg(XPAR_OPB_GPIO_HELIOS_BASEADDR, 1)) & 0x80)

///////////////////////////////////////////////////////////////////////////////



// FUNCTION PROTOTYPES ////////////////////////////////////////////////////////

// Wireless Handler
void WirelessInterruptHandler(void);
void WirelessReceiveParser();

// Function primitives
void  WirelessInit(void);
int   WireSendData(uint8 *data, int numBytes);
int   WireSendDataFull(uint8 *dataHeader, int numBytesHeader, uint8 *dataData, int numBytesData);
int   WireSendDataFull_lowPriority(uint8 *dataHeader, int numBytesHeader, uint8 *dataData, int numBytesData);
int   WireSendDataTri(uint8 *dataHeader, int numBytesHeader, uint8 *dataData1, int numBytesData1, uint8 *dataData2, int numBytesData2);
int   WireSendDataTri_lowPriority(uint8 *dataHeader, int numBytesHeader, uint8 *dataData1, int numBytesData1, uint8 *dataData2, int numBytesData2);
int   getSendBufferFreeSpace();
int   getSendBufferSize();


#endif
