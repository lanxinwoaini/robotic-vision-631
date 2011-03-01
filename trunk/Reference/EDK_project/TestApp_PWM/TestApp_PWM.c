/*
 *
 * Xilinx, Inc.
 * XILINX IS PROVIDING THIS DESIGN, CODE, OR INFORMATION "AS IS" AS A 
 * COURTESY TO YOU.  BY PROVIDING THIS DESIGN, CODE, OR INFORMATION AS
 * ONE POSSIBLE   IMPLEMENTATION OF THIS FEATURE, APPLICATION OR 
 * STANDARD, XILINX IS MAKING NO REPRESENTATION THAT THIS IMPLEMENTATION 
 * IS FREE FROM ANY CLAIMS OF INFRINGEMENT, AND YOU ARE RESPONSIBLE 
 * FOR OBTAINING ANY RIGHTS YOU MAY REQUIRE FOR YOUR IMPLEMENTATION
 * XILINX EXPRESSLY DISCLAIMS ANY WARRANTY WHATSOEVER WITH RESPECT TO 
 * THE ADEQUACY OF THE IMPLEMENTATION, INCLUDING BUT NOT LIMITED TO 
 * ANY WARRANTIES OR REPRESENTATIONS THAT THIS IMPLEMENTATION IS FREE 
 * FROM CLAIMS OF INFRINGEMENT, IMPLIED WARRANTIES OF MERCHANTABILITY 
 * AND FITNESS FOR A PARTICULAR PURPOSE.
 */

/*
 * Xilinx EDK 11.1 EDK_L.29.1
 *
 * This file is a sample test application
 *
 * This application is intended to test and/or illustrate some 
 * functionality of your system.  The contents of this file may
 * vary depending on the IP in your system and may use existing
 * IP driver functions.  These drivers will be generated in your
 * XPS project when you run the "Generate Libraries" menu item
 * in XPS.
 *
 * Your XPS project directory is at:
 *    C:\EDK11\Rebuild2\
 */


// Located in: ppc405_0/include/xparameters.h
#include "xparameters.h"

#include "xcache_l.h"

#include "stdio.h"

#include "xutil.h"

#include "SystemTypes.h"

#include "ServoControl.h"
//====================================================

int main (void) {
	int8 str,thr;
	str = 0;
	thr = 8;  //this seems to keep the truck stopped.

   XCache_EnableICache(0x00000001);
   XCache_EnableDCache(0x00000001);
   xil_printf("-- Starting PWM Test Mode --\r\n");
	xil_printf("NOTICE: Make sure car is placed on stand before proceeding!!\r\n");
	xil_printf("Use the number pad to control the car via Terminal\r\n");
	xil_printf("8 Increment forward throttle\r\n");
	xil_printf("4 Steer Left\r\n");
	xil_printf("5 Center Servos\r\n");
	xil_printf("6 Steer Right\r\n");
	xil_printf("2 Increment reverse throttle\r\n");
	xil_printf("x Center servos and exit\r\n\r\n");
	
	//Initialze servos to center position
	ServoInit(RC_STR_SERVO);
	ServoInit(RC_VEL_SERVO);
	xil_printf("Steering Setting: %d\r\n",GetServo(RC_STR_SERVO));
	xil_printf("Throttle Setting: %d\r\n\r\n",GetServo(RC_VEL_SERVO));

	char c;
	
	while(1)
	{
	   read(0,&c,1);
		switch (c)
		{
			case '4': //left
				if(str < 64)
					str += 8;
				break;
			case '5'://center
				str = 0;
				thr = 8;
				break;
			case '6'://right
				if(str > -64)
					str -= 8;
				break;
			case '8'://forward
				if(thr < 64)
					thr += 2;
				break;
			case '2'://reverse
				if(thr > -64)
					thr -= 2;
				break;
			case 'x'://quit
				ServoInit(RC_STR_SERVO);
				ServoInit(RC_VEL_SERVO);
				print("-- Exiting main() --\r\n");
				XCache_DisableDCache();
				XCache_DisableICache();
				return 0;
				break;
			default:
				break;
		}
		SetServo(RC_STR_SERVO, str);
		SetServo(RC_VEL_SERVO, thr);
		xil_printf("Steering Setting: %d\r\n",GetServo(RC_STR_SERVO));
		xil_printf("Throttle Setting: %d\r\n\r\n",GetServo(RC_VEL_SERVO));
	}

   print("-- Exiting main() --\r\n");
   XCache_DisableDCache();
   XCache_DisableICache();
   return 0;
}

