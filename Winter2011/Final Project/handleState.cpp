#include "stdafx.h"

//define all of the possible modes
#define NOT_SIGNING			0x0
#define WAIT_MVMT			0x1
#define WAIT_STILL			0x2
#define CHECK_TEMPLATES		0x4
#define DISPLAY_RESULT		0x8
#define NO_RESULT			0x10

#define ONE_SECOND			30

//external variables relating to gathered data
extern unsigned diffDots;
extern char getSign();

unsigned MODE = NOT_SIGNING;
unsigned delayCounter = 0;
char detectedSign;
char * modeString = "Not Signing!";

void handleState()
{
	char c;
	switch(MODE){
		case NOT_SIGNING:	break;
		case WAIT_MVMT:
			modeString = "Waiting for movement.";
			break;
		case WAIT_STILL:		//wait for several consecutive frames of no movement
			if(diffDots < 20000){
				delayCounter++;
				if(delayCounter == ONE_SECOND){
					MODE = CHECK_TEMPLATES;
					delayCounter = 0;
				}
			} 
			else delayCounter = 0;

			modeString = "Waiting for still.";
			break;
		case CHECK_TEMPLATES:
			c = getSign();
			if(c != 0){
				detectedSign = c;
				MODE = DISPLAY_RESULT;
				delayCounter = 0;
			}
			delayCounter++;
			if(delayCounter == ONE_SECOND*3)
			modeString = "Checking frame against templates.";
			break;
		case DISPLAY_RESULT:
			modeString = "Displaying the result.";
			delayCounter++;
			if(delayCounter == ONE_SECOND*2){
				delayCounter = 0;
				MODE = WAIT_MVMT;
			}
			break;
		case NO_RESULT:
			break;
		default: ;
	}
}