#include "shots.h"
#include "rprintf.h"
#include "gameState.h"
#include "packets.h"
#include "global.h"
#include "timer.h"
#include "uart2.h"
#include "pwmled.h"
#include "soundchip.h"

extern gameParameters gp;//Game Parameter struct

void handleKillShot(struct returnShotPacket* shotReceived)
{
	rprintf("Handle Kill Shot\n");
	// If you shoot your own team member or base
	if(gp.myTeamID == shotReceived->targetTeamID)
	{
		// If flag teamMemberKillShot == 1 
		// If you shoot your own teammate
		if(shotReceived->targetID != BASE && (gp.ruleFlags & 0x01))
		{
			// Check is the truck that was shot had the flag. If so return the flag.
			if(shotReceived->targetState == ENABLED_WITH_FLAG)
			{
				//return the flag to opponents base
				struct changeTruckBasePacket flagState;
				returnFlag(&flagState, shotReceived->targetTeamID);
			}
			// tell truck he is disabled
			struct changeTruckBasePacket truckState;
			truckState.packetType = CHANGE_TRUCK_BASE_STATE;
			truckState.targetID = shotReceived->targetID;
			truckState.targetTeamID = shotReceived->targetTeamID;
			truckState.targetState = DISABLED;
			uart2SendXBeePacket(&truckState, CHANGE_TRUCK_BASE_SIZE);
		}
		// If yourBaseKillshot == 1
		// Check if you shot your base and the rule flag for shooting your base is enabled.
		else if(shotReceived->targetID == BASE && gp.ruleFlags & 0x02)
		{
			// if shooter has opponents flag
			if(gp.myGameState == ENABLED_WITH_FLAG)
			{
				//return the flag to opponents base
				struct changeTruckBasePacket flagState;
				returnFlag(&flagState, shotReceived->targetTeamID);
				gp.myGameState = DISABLED;
			}
		}
	}
	// If you shoot an opposing player or base
	else if(gp.myTeamID != shotReceived->targetTeamID)
	{
		// If target truck state is enabled with flag
		//TODO put case where you shoot your opponents base to get the flag.
		if(shotReceived->targetID != BASE)
		{
			if(shotReceived->targetState == ENABLED_WITH_FLAG)
			{
				//return the flag to opponents base
				struct changeTruckBasePacket flagState;
				returnFlag(&flagState, shotReceived->targetTeamID);
			}
		
			// Disable the truck
			struct changeTruckBasePacket truckState;
			truckState.packetType = CHANGE_TRUCK_BASE_STATE;
			truckState.targetID = shotReceived->targetID;
			truckState.targetTeamID = shotReceived->targetTeamID;
			truckState.targetState = DISABLED;
			uart2SendXBeePacket(&truckState, CHANGE_TRUCK_BASE_SIZE);
		}
		else if(shotReceived->targetID == BASE && shotReceived->targetState == ENABLED_WITH_FLAG)
		{
			gp.myGameState = ENABLED_WITH_FLAG;
			
			// enable_without_flag the base
			struct changeTruckBasePacket truckState;
			truckState.packetType = CHANGE_TRUCK_BASE_STATE;
			truckState.targetID = shotReceived->targetID;
			truckState.targetTeamID = shotReceived->targetTeamID;
			truckState.targetState = ENABLED_WITHOUT_FLAG;
			uart2SendXBeePacket(&truckState, CHANGE_TRUCK_BASE_SIZE);
		}
	}
	
	colorUpdate();
}

void returnFlag(struct changeTruckBasePacket* objectState, u08 flagCarrierID)
{
	objectState->packetType = CHANGE_TRUCK_BASE_STATE;
	objectState->targetID = BASE;
	if(flagCarrierID == TEAM1)
	{
		objectState->targetTeamID = TEAM2;
	}
	else
	{
		objectState->targetTeamID = TEAM1;
	}
	objectState->targetState = ENABLED_WITH_FLAG;
	uart2SendXBeePacket(objectState, CHANGE_TRUCK_BASE_SIZE);
}

void handleReviveShot(struct returnShotPacket* shotReceived)
{
	//TODO put in check for ruleFlags when you revive opponent truck and base
	
	// If you shoot any truck that is disabled with a revive shot, enable them.
	if(shotReceived->targetTeamID == gp.myTeamID && shotReceived->targetState == DISABLED && gp.myGameState != DISABLED)
	{
		// tell truck he is enabled
		struct changeTruckBasePacket truckState;
		truckState.packetType = CHANGE_TRUCK_BASE_STATE;
		truckState.targetID = shotReceived->targetID;
		truckState.targetTeamID = shotReceived->targetTeamID;
		truckState.targetState = ENABLED_WITHOUT_FLAG;
		uart2SendXBeePacket(&truckState, CHANGE_TRUCK_BASE_SIZE);
	}
	else if(shotReceived->targetTeamID != gp.myTeamID && shotReceived->targetState == DISABLED 
			&& gp.myGameState != DISABLED && gp.ruleFlags & 0x04)
	{
		// tell truck he is enabled
		struct changeTruckBasePacket truckState;
		truckState.packetType = CHANGE_TRUCK_BASE_STATE;
		truckState.targetID = shotReceived->targetID;
		truckState.targetTeamID = shotReceived->targetTeamID;
		truckState.targetState = ENABLED_WITHOUT_FLAG;
		uart2SendXBeePacket(&truckState, CHANGE_TRUCK_BASE_SIZE);
	}
	
	if(shotReceived->targetID == BASE)
	{
		if(gp.myTeamID == shotReceived->targetTeamID && gp.myGameState == DISABLED)
		{
			gp.myGameState = ENABLED_WITHOUT_FLAG;
			PlayAudioPhrase(REVIVED_PHRASE);
		}
		else if(gp.myTeamID != shotReceived->targetTeamID && gp.myGameState != DISABLED && gp.ruleFlags & 0x08)
		{
			//If I have the flag return it to the opponents base
			struct changeTruckBasePacket flagState;
			returnFlag(&flagState, gp.myTeamID);
			gp.myGameState = DISABLED;
		}
	}
	
	colorUpdate();
}

/*
	This passes the flag to another truck or base. We assume that this will not be called if the shooter of this shot
	does not have the flag.
*/
void handlePassShot(struct returnShotPacket* shotReceived)
{
	// If you shoot a team member with the Pass shot
	if(gp.myTeamID == shotReceived->targetTeamID && shotReceived->targetID != BASE)
	{
		// If I have the flag and my teammate is enabled without the flag
		if(shotReceived->targetState == ENABLED_WITHOUT_FLAG)
		{
			// tell truck he is enabled with the flag
			struct changeTruckBasePacket truckState;
			truckState.packetType = CHANGE_TRUCK_BASE_STATE;
			truckState.targetID = shotReceived->targetID;
			truckState.targetTeamID = shotReceived->targetTeamID;
			truckState.targetState = ENABLED_WITH_FLAG;
			uart2SendXBeePacket(&truckState, CHANGE_TRUCK_BASE_SIZE);

			// Set my state to not have the flag
			gp.myGameState = ENABLED_WITHOUT_FLAG;
		}
	}
	// If you shoot your opponent or opponents base with a pass shot
	else if(gp.myTeamID != shotReceived->targetTeamID)
	{
		//If I have the flag return it to the opponents base
		struct changeTruckBasePacket flagState;
		returnFlag(&flagState, gp.myTeamID);
		//gp.myGameState = DISABLED;
	}
	else if(gp.myTeamID == shotReceived->targetTeamID && shotReceived->targetID == BASE)
	{
		// Win Game
		struct gameStatePacket state;
		state.packetType = GAME_STATE;
		state.gameState = PAUSED;
		uart2SendXBeePacket(&state, GAME_STATE_SIZE);
		
		// Sets missed pin high and wait to shoot low which tells the system the flag has been captured.
		uart2GPIOSetState(0x40);
		PlayAudioPhrase(VICTORY_PHRASE);
		rprintf("Game over\n");
	}
	
	colorUpdate();
}

void fireLaser(u08 shotType)
{
	u08 laserPacket;
	//Create the 8 bit laser shot packet
	laserPacket = (0xc0 & (shotType << 6)) | (0x30 & (gp.myTeamID << 4)) | (0x0F & gp.myTruckID);
	uart2LaserSendString(&laserPacket);
	
	// Set wait to shoot high and enabled pin high
	uart2GPIOSetState(0x88);
	sei();
	timer1SetPrescaler( TIMER_CLK_DIV1024 );
	timerAttach(TIMER1OVERFLOW_INT, timerUpShotFunction );
	cli();
}

void colorUpdate()
{
	uart2DisableLaserInt();
	//set team color led
	if(gp.myTeamID == TEAM1)
	 {		
		 pwmSetLEDColor(TEAMCOLOR_LEDSET, gp.team1Color);
		 //gp.myColor = gp.team1Color;
	 }
	 else
	 {
		 pwmSetLEDColor(TEAMCOLOR_LEDSET, gp.team2Color);
		 //gp.myColor = gp.team2Color;
	 }
	 
	 //set status color led
	 if(gp.myGameState == ENABLED_WITHOUT_FLAG)
	 {
		pwmSetLEDColor(STATUS_LEDSET, gp.statusEnabled);
		// enabled pin high, flag pin will be low
		uart2GPIOSetState(0x08);
	 }
	 else if(gp.myGameState == ENABLED_WITH_FLAG)
	 {
		pwmSetLEDColor(STATUS_LEDSET, gp.statusHaveFlag);
		// sets flag pin high, enabled pin high
		uart2GPIOSetState(0x18);
		//plays the got the flag sound tune
		PlayAudioPhrase(GOT_FLAG_PHRASE);
	 }
	 else if(gp.myGameState == DISABLED)
	 {
		pwmSetLEDColor(STATUS_LEDSET, gp.statusDisabled);
		// sets flag pin low, and enabled pin low
		uart2GPIOSetState(0x00);
		//plays a disabled sound
		PlayAudioPhrase(DISABLED_PHRASE);
	 }
	 else
	 {
		pwmSetLEDColor(STATUS_LEDSET, gp.statusDisabled);
	 }
	 //rprintf("Timers\n");
	 //timerPause(250);
	 //timerPause(250);
	 //timerPause(250);
	 //timerPause(250);
	 //rprintf("End Timers\n");
	uart2EnableLaserInt();
	//rprintf("End Color\n");
}
