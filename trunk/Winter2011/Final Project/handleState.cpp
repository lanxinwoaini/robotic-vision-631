#include "stdafx.h"
#include "Final Project.h"



#define ONE_SECOND			30

//external variables relating to gathered data
extern CString myPassword;
extern unsigned diffDots;
extern char getSign();

unsigned MODE = NOT_SIGNING;
unsigned delayCounter = 0;
unsigned entryIndex = 0;
string modeString;
string currentEntry = "fd";

void handleChar(char c)
{

}

void handleFrame()
{
	char c;
	switch(MODE){
		case NOT_SIGNING:	
			modeString = "Not Signing!";	
			if(currentEntry.length() > 0 || entryIndex != 0){
				entryIndex = 0;
			//	currentEntry.clear();
			}
			break;
		case WAIT_MVMT:
			if(diffDots >= NUMDOTS_MVMT-1000){
				delayCounter++;
				if(delayCounter == ONE_SECOND/2){
					MODE = WAIT_STILL;
					delayCounter = 0;
				}
			} 
			else delayCounter = 0;
			modeString = "Waiting for movement.";
			break;
		case WAIT_STILL:		//wait for several consecutive frames of no movement
			if(diffDots < NUMDOTS_MVMT+1000){
				delayCounter++;
				if(delayCounter == ONE_SECOND/2){
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
				currentEntry += c;
				MODE = DISPLAY_RESULT;
				delayCounter = 0;
			}
			delayCounter++;
			if(delayCounter == ONE_SECOND*2){
				delayCounter = 0;
				MODE = NO_RESULT;
			}
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
			modeString = "Displaying no result found.";
			delayCounter++;
			if(delayCounter == ONE_SECOND*2){
				delayCounter = 0;
				MODE = WAIT_MVMT;
			}
			break;
		default: ;
	}
}