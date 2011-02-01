#include "packets.h"
#include "shots.h"
#include "timer.h"
#include "gameState.h"
#include "string.h"
#include "global.h"
#include "timer.h"
#include "uart2.h"
#include "rprintf.h"

extern gameParameters gp;//Game Parameter struct
extern volatile u08 truckCount;
extern u08 truckNames[6][16];
u08 targetsHit = 0;
struct returnShotPacket globalShotPacket;

// /* Sends a packet starting at start of length size */
// void uart2SendXBeePacket(void * start, u08 size);


// /* Reads the state of all GPIO pins */
// /* Ordering is pin 7 downto 0, MSB to LSB */
// u08 uart2GPIOReadState();

// /* Sets the state of all GPIO pins */
// /* Ordering is pin 7 downto 0, MSB to LSB */
// void uart2GPIOSetState(u08 data);


/* Sets an LEDSet to a color */
/* color[] is an array of 3 u08s, index 0=red, 1=green, 2=blue */
//void pwmSetLEDColor(u08 ledSet, u08 color[]){

void handleGameInit(struct gameInitPacket* initPacket)
{
	struct gameRespondPacket respondPacket;
	gp.ruleFlags = initPacket->ruleFlags;
	respondPacket.packetType = GAME_RESPOND;
	strcpy(respondPacket.truckName, gp.myTruckName);
	uart2SendXBeePacket(&respondPacket, GAME_RESPOND_SIZE);
	
	// Set paused pin high
	uart2GPIOSetState(0x20);
	
}

void handleGameRespond(struct gameRespondPacket* respondPacket)
{
	static u08 name = 0;
	truckCount++;
	strcpy(truckNames[name], respondPacket->truckName);
	name++;
	
	colorUpdate();
}

void handleTeamSelect(struct teamSelectPacket* teamPacket)
{
	gp.myTeamID = teamPacket->teamID;
	uart2GPIOSetState(0x08);
	
	colorUpdate();
}

void handleGameState(struct gameStatePacket* statePacket)
{
	if(statePacket->gameState == PAUSED)
	{
		// set wait to shoot high for x time
		// set hit/paused pin high
		uart2GPIOSetMask(0x20, 1);
	}
	else if(statePacket->gameState == CAPTURED)
	{
		// Set miss pin high and wait to shoot pin low to show a captured state
		uart2GPIOSetState(0x40);
		// set miss/captured pin high
		if(gp.myTruckID != 0x00)
		{	
			gp.myGameState = DISABLED;			
		}	
		else
		{
			rprintf("Got Flag\n");
			gp.myGameState = ENABLED_WITH_FLAG;
		}
		// Possibly give points
	}
	colorUpdate();
}

void handleReturnShot(struct returnShotPacket* shotPacket)
{
	//rprintf("Handling return shot\n");
	//rprintf("shot type = %d\n", shotPacket->shotType);
	// Check if the wait to shoot pin is high
	if(uart2GPIOReadState() & 0x80)
	{
		targetsHit++;
		globalShotPacket = *shotPacket;
		return;
	}
	
	if(shotPacket->shotType == KILL)
	{	
		//rprintf("kill\n");
		// Shot Hit!!! Keep wait to shoot high until hit pin goes low
		//uart2GPIOSetState(0xA8);
		handleKillShot(shotPacket);
	}
	else if(shotPacket->shotType == PASS)
	{	
		//rprintf("pass\n");
		// Shot Hit!!! Keep wait to shoot high until hit pin goes low
		//uart2GPIOSetState(0xA8);
		handlePassShot(shotPacket);
	}
	else if(shotPacket->shotType == REVIVE)
	{
		//rprintf("revive\n");
		// Shot Hit!!! Keep wait to shoot high until hit pin goes low
		//uart2GPIOSetState(0xA8);
		handleReviveShot(shotPacket);
	}
	else
	{
		rprintf("Shot not recognized\n");
		// Shot Missed!!! Keep wait to shoot high until missed pin goes low
		//uart2GPIOSetState(0xC8);
	}
	
}

void timerUpShotFunction()
{
	rprintf("Set wait pin low\n");
	timerDetach(TIMER1OVERFLOW_INT);
	if(targetsHit == 1)
	{		
		handleReturnShot(&globalShotPacket);
		// Set Hit pin high and enabled high, wait_to_shoot pin is high
		uart2GPIOSetState(0xA8);
		rprintf("HIT!\n");
	}
	else
	{
		// Set Miss pin high and enabled pin high, wait_to_shoot high
		uart2GPIOSetState(0xC8);
		rprintf("MISS!\n");
	}	
	targetsHit = 0;
	// sets enabled pin to high, makes sure hit/miss pin are low
	uart2GPIOSetState(0x08);
}

void handleChangeTruckBaseState(struct changeTruckBasePacket* truckBasePacket)
{
	u08 newState = truckBasePacket->targetState;

	if(truckBasePacket->targetID != gp.myTruckID || truckBasePacket->targetTeamID != gp.myTeamID)
	{
		return;
	}
	
	// If the truck has the flag and gets hit then it needs to change its state
	// and return the flag back to his opponents base.
	if(newState == DISABLED && gp.myGameState == ENABLED_WITH_FLAG)
	{
		struct changeTruckBasePacket statePacket;
		statePacket.packetType = GAME_STATE;
		statePacket.targetState = ENABLED_WITH_FLAG;
		statePacket.targetID = BASE;
		if(gp.myTeamID == TEAM1)
		{
			statePacket.targetTeamID = TEAM2;
		}
		else
		{
			statePacket.targetTeamID = TEAM1;
		}
		uart2SendXBeePacket(&statePacket, CHANGE_TRUCK_BASE_SIZE);
	}
	else if(newState == ENABLED_WITHOUT_FLAG && gp.myGameState == ENABLED_WITH_FLAG)
	{
		// This if statement should only be a base that runs it
		rprintf("I Lost the Flag\n");
	}
	
	// //rprintf("before state = %d\n", gp.myGameState);
	// if( newState == ENABLED_WITH_FLAG && gp.myGameState == ENABLED_WITHOUT_FLAG)
	// {
		// // Flag pin goes low and enabled pin goes low
		// uart2GPIOSetState(0x18);
	// }
	gp.myGameState = newState;
	//rprintf("after state = %d\n", gp.myGameState);
	
	colorUpdate();
}

