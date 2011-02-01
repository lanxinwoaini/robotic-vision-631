/////////////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2004 Xilinx, Inc. All Rights Reserved.
//
// You may copy and modify these files for your own internal use solely with
// Xilinx programmable logic devices and  Xilinx EDK system or create IP
// modules solely for Xilinx programmable logic devices and Xilinx EDK system.
// No rights are granted to distribute any files unless they are distributed in
// Xilinx programmable logic devices.
//
/////////////////////////////////////////////////////////////////////////////////

/*
 *      Xilinx EDK - Simple SREC Bootloader
 *      This simple bootloader is provided with Xilinx EDK for you to easily re-use in your
 *      own software project. It is capable of booting an SREC format image file 
 *      (Mototorola S-record format), given the location of the image in memory.
 *      In particular, this bootloader is designed for images stored in non-volatile flash
 *      memory that is addressable from the processor. 
 *
 *      Please modify the define "BOOT_IMAGE_BASEADDR" in the bootloader.h header file 
 *      to point to the memory location from which the bootloader has to pick up the 
 *      flash image from.
 *
 *      You can include these sources in your software application project in XPS and 
 *      build the project for the processor for which you want the bootload to happen.
 *      You can also subsequently modify these sources to adapt the bootloader for any
 *      specific scenario that you might require it for.
 *
 */


#include "bootloader.h"
#include "xcache_l.h"
#include "SystemTypes.h"
#include <string.h>
#include "errors.h"
#include "srec.h"

#include "TX.h"
#include "state.h"

/* Defines */

/* Comment the following line, if you want faster and non-verbose bootloading */
//define DISPLAY_PROGRESS

/* Declarations */
extern int srec_line;

/* Data structures */
srec_info_t srinfo;
uint8 sr_buf[SREC_MAX_BYTES];
uint8 sr_data_buf[SREC_DATA_MAX_BYTES];

uint8 *flbuf = (uint8*)BOOT_IMAGE_BASEADDR;
uint8 *errors[] = { 
    "Error while copying executable image into RAM",
    "Error while reading an SREC line from flash",
    "SREC line is corrupted",
    "SREC has invalid checksum."
};
uint8 errorsSize[] = { 45, 43, 22, 26 };

//used for messages


int load_exec ()
{
    int8 ret;
    void (*laddr)();
	laddr = (void*) 0xfffffffc; //just temporary(it should be something like that)
    int8 done = 0;
    
    srinfo.sr_data = sr_data_buf;

    SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,11,"Orig Data: ");
	sendHex(*(uint32*)0xfffffffc,DISPLAY_IN_GUI_NEWLINE);
	usleep(500);

    while (!done) {
        if ((ret = flash_get_srec_line (sr_buf)) != 0) 
			return ret;
        
		if ((ret = decode_srec_line (sr_buf, &srinfo)) != 0)
			return ret;
        
		switch (srinfo.type) {
			case SREC_TYPE_0:
			break;
			case SREC_TYPE_1:
			case SREC_TYPE_2:
			case SREC_TYPE_3:
			memcpy ((void*)srinfo.addr, (void*)srinfo.sr_data, srinfo.dlen);
			break;
			case SREC_TYPE_5:
			break;
			case SREC_TYPE_7:
			case SREC_TYPE_8:
			case SREC_TYPE_9:
			laddr = (void*)srinfo.addr;
			done = 1;
			ret = 0;
			break;
		}
    }

	SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,22,"Executing program at: ");
	sendHex((uint32)laddr,DISPLAY_IN_GUI_NEWLINE);
	SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,11,"FirstData: ");
	sendHex(*(uint32*)laddr,DISPLAY_IN_GUI_NEWLINE);

	usleep(1000);
	//invalidate any instruction cache
	XCache_InvalidateICache();
    (*laddr)();
  
    /* We will be dead at this point */
	return -1;
}


int flash_get_srec_line (uint8 *buf)
{
    uint8 c;
    int8 count = 0;

    while (1) {
	c  = *flbuf++;
	if (c == 0xD) {   
            /* Eat up the 0xA too */
	    c = *flbuf++; 
	    return 0;
	}
	
	*buf++ = c;
	count++;
	if (count > SREC_MAX_BYTES) 
	    return LD_SREC_LINE_ERROR;
    }
}

void bootload()
{
    int8 ret;
    XCache_DisableICache();
	XCache_DisableDCache();


	SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,11,"Bootloader:");
    usleep(500);
	ret = load_exec ();

    /* If we reach here, we are in error */
    if (ret > LD_SREC_LINE_ERROR) {
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,20,"ERROR in SREC line: ");
		sendUint(srec_line,DISPLAY_IN_GUI_NEWLINE);
		SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,errorsSize[ret],errors[ret]);
    } else {
		SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,7,"ERROR: ");
		sendUint(srec_line,DISPLAY_IN_GUI_NEWLINE);
		SendPacket(TEXT,DISPLAY_IN_GUI_NEWLINE,errorsSize[ret],errors[ret]);
    }
    usleep(500);
}

