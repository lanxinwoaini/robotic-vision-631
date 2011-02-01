#ifndef GAMESTATE_H
#define GAMESTATE_H

#include "avrlibtypes.h"
 
// Truck and Base States
#define ENABLED_WITHOUT_FLAG 0x00
#define ENABLED_WITH_FLAG 0x01
#define DISABLED 0x02

// Game states
#define PAUSED 0x00
#define CAPTURED 0x01

// State colors
// magenta - ENABLED_WITH_FLAG
// cyan - ENABLED_WITHOUT_FLAG
// yellow - DISABLED

//Team colors
// Red TEAM1
// Green TEAM2

/*
running - is a boolean expression telling whether the game is running or not. (1 = running) (0 = not running)
numberOfTeams - is the number of teams playing. 

*/
static struct
{
	u08 running;
	u08 numberOfTeams;
} game;




#endif
