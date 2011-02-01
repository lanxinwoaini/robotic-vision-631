#ifndef INTERRUPTS_H
#define INTERRUPTS_H

#include "shots.h"
#include "avrlibtypes.h"

void handleXbeeInterrupt(void* packet);
void handleLaserInterrupt(u08);
void handleRobotInterrupt(u08);

void handleState();


#endif
