#include "pidControl.h"
#include "ServoControl.h"
#include "InterruptControl.h"
#include "TX.h"

PID pid;
float outputPID;
float desiredVelocityPID = 0.0f;
int desiredAnglePID = 0;

extern int encoderValue;
int lastDeltaDistanceEncoderValue = 0;

void initPID(){
	pid.distanceMode = DO_NOTHING_WITH_DESIRED_ENCODER;
	pid.desiredEncoder = 0;
	pid.lastEncoder = encoderValue;      	
	pid.iState = 0.0f;      	
	pid.iMax = 1.0f;
	pid.iMin = -1.0f;  	
	pid.kP = 25.0f;    		
	pid.kD = 0.012f;	
	pid.kI = 0.6f;	
	pid.count = 0;
	pid.lastDesiredVelocity = 0.0f;
	pid.lastClockTicks = 0;
}


void updateVelocityOutput(int updatesPerSecond)
{
	pid.count ++;
	float desiredVelocity = desiredVelocityPID;;
	float pTerm, dTerm, iTerm, error, currentVelocity;

	if(pid.distanceMode == DO_NOTHING_WITH_DESIRED_ENCODER){
		// Do nothing
	}
	else if(pid.distanceMode == SEND_SIGNAL_AT_DESIRED_ENCODER){
		if(pid.desiredEncoder <= encoderValue){
			// signal semaphore for navigation
		}
	}
	else if(pid.distanceMode == STOP_AT_DESIRED_ENCODER){
		if(encoderValue > pid.desiredEncoder - (TICKS_PER_TURN * desiredVelocity * 1.5)){
			setVelocity(.4);
			desiredVelocity = 0.4;

			if(encoderValue > pid.desiredEncoder - 100 && encoderValue < pid.desiredEncoder + 100){
				setVelocity(0);
				desiredVelocity = 0;
			}	
		}
	}
	
	currentVelocity = ((encoderValue - pid.lastEncoder) * updatesPerSecond) / TICKS_PER_METER;
	error = desiredVelocity - currentVelocity;

	pTerm = pid.kP * error;   // calculate the proportional term
	
	//if(pid.lastDesiredVelocity != desiredVelocity){
	//	pid.count = 0;
	//	pid.iState = 0;
	//}

	pid.iState += error;
	iTerm = pid.kI * pid.iState / pid.count;  // calculate the integral term

	if(iTerm > pid.iMax){
		iTerm = pid.iMax;
	}
	else if(iTerm > pid.iMin){
		iTerm = pid.iMin;
	}

	dTerm = pid.kD * (encoderValue - pid.lastEncoder);
	pid.lastEncoder = encoderValue;

	outputPID = pTerm + iTerm + dTerm;

	if(outputPID > 50)
		outputPID = 50;
	else if(outputPID < -50)
		outputPID = -50;
	
	pid.lastDesiredVelocity = desiredVelocity;
	SetServo(RC_VEL_SERVO, (int)outputPID);
}

void setNumMetersToRun(float numMetersToRun){
	DISABLE_INTERRUPTS();
	pid.distanceMode = STOP_AT_DESIRED_ENCODER;
	pid.desiredEncoder = encoderValue + (TICKS_PER_TURN * numMetersToRun / WHEEL_CIRCUMFERENCE);
	RESTORE_INTERRUPTS(msr);
}

void setNumMillisecondsToRun(int numMilliseconds){
	DISABLE_INTERRUPTS();
	pid.distanceMode = DO_NOTHING_WITH_DESIRED_ENCODER;
	RESTORE_INTERRUPTS(msr);
}

void setVelocity(float velocity){
	if(velocity > 5.0)
		velocity = 5.0;
	else if(velocity < -5.0)
		velocity = -5.0;
	DISABLE_INTERRUPTS();
		desiredVelocityPID = velocity;
	RESTORE_INTERRUPTS(msr);
}

void setSteeringAngle(int angle){
	if(angle > 64)
		angle = 64;
	else if(angle < -64)
		angle = -64;
	DISABLE_INTERRUPTS();
		desiredAnglePID = angle;
	RESTORE_INTERRUPTS(msr);
}

void travelDistance(float meters){
	pid.distanceMode = SEND_SIGNAL_AT_DESIRED_ENCODER;
	int tempEncoder = encoderValue;

	pid.desiredEncoder = tempEncoder + (TICKS_PER_METER * meters);
}

float getDeltaDistance(){
	float metersTraveled = 0.0f;
	DISABLE_INTERRUPTS();
		int tempEncoderValue = encoderValue;	
		metersTraveled = (float)((tempEncoderValue - lastDeltaDistanceEncoderValue)) / ((float)TICKS_PER_METER);
		lastDeltaDistanceEncoderValue = tempEncoderValue;
	RESTORE_INTERRUPTS(msr);
	return metersTraveled;
}

void updateSteeringAngle(){
	
	int angleOutput = desiredAnglePID;
	
	//Our steering is off, so we add an offset for right turns
	if(angleOutput < 0){
		angleOutput -= 5;
	}

	RCSetSteering(angleOutput);
}





