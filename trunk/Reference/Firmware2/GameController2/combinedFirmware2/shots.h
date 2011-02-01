#ifndef SHOTS_H
#define SHOTS_H

#include "avrlibtypes.h"
#include "packets.h"

// Shot types
#define NO_SHOT 0
#define KILL 1
#define PASS 2
#define REVIVE 3

// Team names
#define TEAM1 0
#define TEAM2 1

// Truck names
#define BASE 0x00
#define TRUCK1 0x01
#define TRUCK2 0x02
#define TRUCK3 0x03
#define TRUCK4 0x04


/*
This function handles the shot that was fired and hit something of importance.
param@ shooterPackage - shooter's information such as 1. shot type 2. team id 3. player id
param@ targetPackage - target's information such as 1. team id 2.player id
*/
void handleShot(u08 shooterPackage, u08 targetPackage);

/*

*/
void handleKillShot(struct returnShotPacket* shotReceived);
void handlePassShot(struct returnShotPacket* shotReceived);
void handleReviveShot(struct returnShotPacket* shotReceived);
void fireLaser(u08 shotType);
void returnFlag(struct changeTruckBasePacket* objectState, u08 teamID);
void timerUpShotFunction();

void colorUpdate();
#endif
