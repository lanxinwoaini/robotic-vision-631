#include "interrupts.h"
#include "avrlibtypes.h"
#include "packets.h"
#include "shots.h"
#include "interrupts.h"
#include "string.h"
#include "global.h"
#include "gameState.h"
#include "uart2.h"
#include "rprintf.h"
#include "soundchip.h"

extern gameParameters gp; //Game Parameters struct

void handleXbeeInterrupt(void* packet)
{
	//rprintf("Handle Xbee Interrupt\n");
	u08 packetType = ((struct Packet*)packet)->packetType;
	//get the packet 
	//store packet type in variable packetType
	//rprintf("Receiving Packet Type = %d\n", packetType);
	if(packetType == GAME_INIT)
	{
		struct gameInitPacket* initPacket;
		initPacket = (struct gameInitPacket*)packet;
		handleGameInit(initPacket);
	}
	else if(packetType == GAME_RESPOND)
	{
		struct gameRespondPacket* respondPacket;
		respondPacket = (struct gameRespondPacket*)packet;
		handleGameRespond(respondPacket);
	}
	else if(packetType == TEAM_SELECT)
	{
		struct teamSelectPacket* teamPacket;
		teamPacket = (struct teamSelectPacket*)packet;
		if(strcmp(teamPacket->truckName, gp.myTruckName) == 0)
			handleTeamSelect(teamPacket);
	}
	else if(packetType == GAME_STATE)
	{
		struct gameStatePacket* statePacket;
		statePacket = (struct gameStatePacket*)packet;
		handleGameState(statePacket);
	}
	else if(packetType == RETURN_SHOT)
	{
		//rprintf("Return shot packet recieved\n");
		struct returnShotPacket* shotPacket;
		shotPacket = (struct returnShotPacket*)packet;
		if(gp.myTruckID == shotPacket->shooterID)
			handleReturnShot(shotPacket);
	}
	else if(packetType == CHANGE_TRUCK_BASE_STATE)
	{
		struct changeTruckBasePacket* changePacket;
		changePacket = (struct changeTruckBasePacket*)packet;
		handleChangeTruckBaseState(changePacket);
	}
}

void handleLaserInterrupt(u08 laserData)
{
	u08 shotType = laserData >> 6;
	u08 teamID = laserData & 0x30;
	u08 playerID = laserData & 0x0F;
	
	if (laserData == 0xFF)
		return;
	if(shotType != KILL && shotType != PASS && shotType != REVIVE)
	{
		return;
	}
	if(teamID == 0x30)
	{
		return;
	}
	rprintf("HIT!!");
	//Was working with below code*************
	
	// rprintf("ShotType = %x\n", shotType);
	// rprintf("teamID = %x\n", teamID);
	// rprintf("playerID = %x\n", playerID);
	// gp.myGameState = ENABLED_WITHOUT_FLAG;
	// //rprintf("after state = %d\n", gp.myGameState);
	// colorUpdate();
		
	// if(playerID != TRUCK1 && playerID != TRUCK2 && playerID != BASE)
	// {
		// return;
	// }
	
	
	// Parse information out of the incoming laser
	// struct returnShotPacket returnShot;

	// returnShot.packetType = RETURN_SHOT;
	// returnShot.shotType = shotType;
	// returnShot.shooterID = playerID;
	// returnShot.targetID = gp.myTruckID;
	// returnShot.targetTeamID = gp.myTeamID;
	// returnShot.targetState = gp.myGameState;
	
	// uart2SendXBeePacket(&returnShot, RETURN_SHOT_SIZE);
	
	// Our team is TEAM2 & Our truck is TRUCK1
	// struct changeTruckBasePacket state;
	// state.packetType = CHANGE_TRUCK_BASE_STATE;
	// state.targetID = TRUCK1;
	// state.targetTeamID = TEAM2;
	// // If we shoot enemy base 
	// if(gp.myTeamID == TEAM1)
	// {
		// state.targetState = ENABLED_WITH_FLAG;
		// gp.myGameState = ENABLED_WITHOUT_FLAG;
	// }
	// // If we shoot our base
	// else
	// {
		// state.targetState = ENABLED_WITHOUT_FLAG;
		// gp.myGameState = ENABLED_WITH_FLAG;
	// }
	// uart2SendXBeePacket(&state, CHANGE_TRUCK_BASE_SIZE);
	
	volatile u32 counter = 0;
	uart2EnableLaserInt(); //Clears the interrupt buffer	
	gp.myGameState = DISABLED;
	colorUpdate();
	counter = 0;
	while(counter < 1000000)
	{
		counter++;
	}
	gp.myGameState = ENABLED_WITHOUT_FLAG;
	colorUpdate();
}

void handleRobotInterrupt(u08 data)
{
	//rprintf("data = %x\n", data);
	// Fires the Kill shot
	if((data & 0x01) != 0 && !(data & 0x02))
	{
		if(gp.myGameState != DISABLED)
		{
			rprintf("KILL SHOT\n");
			PlayAudioPhrase(FIRE_SHOT_PHRASE);
			fireLaser(KILL);
			fireLaser(KILL);
			fireLaser(KILL);
			fireLaser(KILL);
			fireLaser(KILL);
			fireLaser(KILL);
			fireLaser(KILL);
			fireLaser(KILL);
			fireLaser(KILL);
			fireLaser(KILL);
		}
	}
	// Fires the Pass shot
	else if((data & 0x02) != 0 && !(data & 0x01))
	{
		if(gp.myGameState == ENABLED_WITH_FLAG)
		{
			rprintf("PASS SHOT\n");
			PlayAudioPhrase(PASS_SHOT_PHRASE);
			fireLaser(PASS);
		}
	}
	// Fires the Revive shot
	else if((data & 0x01) != 0 && (data & 0x02))
	{
		rprintf("REVIVE SHOT\n");
		PlayAudioPhrase(REVIVE_SHOT_PHRASE);
		fireLaser(REVIVE);
	}
}

