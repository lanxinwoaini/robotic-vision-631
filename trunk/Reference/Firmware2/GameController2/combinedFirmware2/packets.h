#ifndef PACKETS_H
#define PACKETS_H

#include "avrlibtypes.h"

// Packet Types
#define GAME_INIT 0x01
#define GAME_RESPOND 0x02
#define TEAM_SELECT 0x03
#define GAME_STATE 0x04
#define RETURN_SHOT 0x05
#define CHANGE_TRUCK_BASE_STATE 0x06

// Packet Sizes
#define GAME_INIT_SIZE 2
#define GAME_RESPOND_SIZE 17
#define TEAM_SELECT_SIZE 18
#define GAME_STATE_SIZE 2
#define RETURN_SHOT_SIZE 6
#define CHANGE_TRUCK_BASE_SIZE 4

struct Packet
{
	u08 packetType;
};

struct gameInitPacket
{
	u08 packetType;
	u08 ruleFlags;
};

struct gameRespondPacket
{
	u08 packetType;
	s08 truckName[16];
};

struct teamSelectPacket
{
	u08 packetType;
	s08 truckName[16];
	u08 teamID;
	u08 truckID;
	u08 teamColor;
	u08 statusColor;
};

struct gameStatePacket
{
	u08 packetType;
	u08 gameState;
};

struct returnShotPacket
{
	u08 packetType;
	u08 shotType;
	u08 shooterID;
	u08 targetID;
	u08 targetTeamID;
	u08 targetState;
};

struct changeTruckBasePacket
{
	u08 packetType;
	u08 targetID;
	u08 targetTeamID;
	u08 targetState;
};

// TODO possibly create a start game struct

void handleGameInit();
void handleGameRespond();
void handleTeamSelect();
void handleGameState();
void handleReturnShot();
void handleChangeTruckBaseState();
u08 getPacketType();
void printStr(const char * str);
void printInt(u08 val);

#endif
