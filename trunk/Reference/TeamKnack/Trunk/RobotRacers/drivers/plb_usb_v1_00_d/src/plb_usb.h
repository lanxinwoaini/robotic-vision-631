/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:	 plb_usb.h
AUTHOR:	 Wade Fife
CREATED: 02/25/06

DESCRIPTION

Driver API header for the opb_camera module. See the main .c file for
documentation.

CHANGE LOG

02/25/06 WSF Created initial file.
03/07/06 WSF Removed macros and replaced with functions in .c file. Added
             non-blocking macros for read and write of single words.

******************************************************************************/

#include <xbasic_types.h>
#include "xparameters.h" 

// FUNCTION PROTOTYPES ////////////////////////////////////////////////////////

void    USB_Initialize();
Xuint32 USB_Status(void);
Xuint32 USB_DataReady();
void    USB_Write16(Xuint16 value);
void    USB_SendData16(Xuint16 *source, int numWords);
void    USB_SendPacket16(Xuint16 *source, int numWords);
Xuint16 USB_Read16(void);
void    USB_RecvData16(Xuint16 *dest, int numWords);
void    USB_Commit(void);

void    USB_BurstData16(Xuint16 *source, int numWords);
void	USB_SetPLBBurstSize(Xuint32 size);

// STATUS BITS ////////////////////////////////////////////////////////////////

#define USB_SEND_FULL	0x01	// Indicates USB send FIFO full
#define USB_SEND_EMPTY	0x02	// Indicates USB send FIFO full
#define USB_RESERVED_A	0x04	// Reserved for future use
#define USB_RECV_EMPTY	0x08	// Indicates USB receive FIFO is empty
#define USB_READY	   	0x10	// Indicates USB is connected and ready


