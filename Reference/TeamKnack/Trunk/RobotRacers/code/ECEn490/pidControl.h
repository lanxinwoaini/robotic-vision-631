#ifndef PID_CONTROL_H_
#define PID_CONTROL_H_

#include "SystemTypes.h"

#define STOP_AT_DESIRED_ENCODER			0
#define SEND_SIGNAL_AT_DESIRED_ENCODER	1
#define DO_NOTHING_WITH_DESIRED_ENCODER	2

typedef struct
{
  int distanceMode;		// PID mode
  int desiredEncoder;	// Want to do something when encoder get to this value
  int lastEncoder;		// Last position input
  float lastDesiredVelocity;
  float iState;      	// Integrator state
  int	count;
  float iMax;			// Maximum and minimum allowable integrator state
  float iMin;		  	
  float kI;    			// integral gain
  float kP;    			// proportional gain
  float kD;     		// derivative gain
  uint32 lastClockTicks;
} PID;

#define ENCODER_BASE_ADDR 0x7a400000

#define WHEEL_CIRCUMFERENCE			0.366f	
#define TICKS_PER_TURN				3185
#define WHEEL_CIRCUMFERENCE_TICKS	(WHEEL_CIRCUMFERENCE / TICKS_PER_TURN)
#define TICKS_PER_METER				(TICKS_PER_TURN / WHEEL_CIRCUMFERENCE)
#define PID_UPDATES_PER_SECOND		100
#define PID_UPDATE_MOD				(100 / PID_UPDATES_PER_SECOND)


void initPID();

void setVelocity(float velocity);
void setSteeringAngle(int angle);

void updateVelocityOutput(int updatesPerSecond);
void updateSteeringAngle();
void setNumMetersToRun(float numMetersToRun);
void setNumMillisecondsToRun(int numMillisecondsToRun);
float getDeltaDistance();

#endif
