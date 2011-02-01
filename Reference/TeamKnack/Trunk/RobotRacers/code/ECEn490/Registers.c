#include "Registers.h"
#include "pidControl.h"
#include "MemAlloc.h"
#include "InterruptHandlers.h"
#include "State.h"
#include "InterruptControl.h"
#include "Vision.h"
#include "plb_camera.h"
#include "xparameters.h"

/* Area for declaring external variables for registers */
int* encoderPtr_PID = (int*)ENCODER_BASE_ADDR;

extern PID pid;
extern float outputPID;
extern float desiredVelocityPID;
extern int desiredAnglePID;
extern int numMillisecondsToRun;
extern float numMetersToRun;
extern TransmitState transmitState;
extern HSVSettings hsvSettings;
extern float deltaDistance;
extern int actualPITFreq;
extern int framesPerSecond_SW;
extern int framesPerSecond_HW;

extern float STRAIGHT_SPEED;
extern float BLIND_STRAIGHT_SPEED;
extern float TURN_SPEED;
extern float TURN_RADIUS;

extern int TURN_ANGLE;
extern int FIND_RANGE;

/* End external variables */

RegInfo* regs;
int regCount = 0;

void initRegisters(){

	regs = (RegInfo*) MemCalloc(sizeof(RegInfo)*MAX_NUM_REGS);

	int i;
	int c = 0;

	for(i = 0; i < MAX_NUM_REGS; i ++){
		regs[i].regId = -1;
		regs[i].functionPtrInt = 0;
		regs[i].functionPtrFloat = 0;
		regs[i].dataPtr = 0;
	}


	// end testing
	
	/* Instructions to add a register */
    // 1. Add to the registerId enum in RegisterList.cs with a unique variable name for the register
    // 2. Add a line in the RegisterList constructor to add to the list with the string in quotes being the tag name
    // 3. Add a corresponding register ID to Registers.c and Registers.h
	// 4. Make sure that there MAX_NUM_REGISTERS is big enough for the number of registers added in Registers.c

	/* Add registers here */

	regs[c].regId = ENCODER_VALUE_INT;
	regs[c++].dataPtr = (void*)encoderPtr_PID;

	regs[c].regId = NUM_MILLISECONDS_TO_RUN_INT;
	regs[c].functionPtrInt = &setNumMillisecondsToRun;
	regs[c++].dataPtr = (void*)&numMillisecondsToRun;

	regs[c].regId = NUM_METERS_TO_RUN_FLOAT;
	regs[c].functionPtrFloat = &setNumMetersToRun;
	regs[c++].dataPtr = (void*)&numMetersToRun;

	
	regs[c].regId = KD_REG_FLOAT;
	regs[c++].dataPtr = (void*)&pid.kD;

	regs[c].regId = KI_REG_FLOAT;
	regs[c++].dataPtr = (void*)&pid.kI;

	regs[c].regId = KP_REG_FLOAT;
	regs[c++].dataPtr = (void*)&pid.kP;

	regs[c].regId = I_MAX_FLOAT;
	regs[c++].dataPtr = (void*)&pid.iMax;

	regs[c].regId = I_MIN_FLOAT;
	regs[c++].dataPtr = (void*)&pid.iMin;

	regs[c].regId = OUTPUT_PID_FLOAT;
	regs[c++].dataPtr = (void*)&outputPID;

	regs[c].regId = DESIRED_VELOCITY_FLOAT;
	regs[c++].dataPtr = (void*)&desiredVelocityPID;

	regs[c].regId = DESIRED_ANGLE_INT;
	regs[c++].dataPtr = (void*)&desiredAnglePID;

	regs[c].regId = DESIRED_ENCODER_VALUE_INT;
	regs[c++].dataPtr = (void*)&pid.desiredEncoder;

	regs[c].regId = PID_DISTANCE_MODE_INT;
	regs[c++].dataPtr = (void*)&pid.distanceMode;

	regs[c].regId = TRANSMIT_STATE_VELOCITY_FLOAT;
	regs[c++].dataPtr = (void*)&transmitState.velocity;


	// Orange HSV Registers
	regs[c].regId = H_HIGH_ORANGE;
	regs[c++].dataPtr = (void*)&hsvSettings.hHighOrange;

	regs[c].regId = H_LOW_ORANGE;
	regs[c++].dataPtr = (void*)&hsvSettings.hLowOrange;

	regs[c].regId = S_HIGH_ORANGE;
	regs[c++].dataPtr = (void*)&hsvSettings.sHighOrange;

	regs[c].regId = S_LOW_ORANGE;
	regs[c++].dataPtr = (void*)&hsvSettings.sLowOrange;

	regs[c].regId = V_HIGH_ORANGE;
	regs[c++].dataPtr = (void*)&hsvSettings.vHighOrange;

	regs[c].regId = V_LOW_ORANGE;
	regs[c++].dataPtr = (void*)&hsvSettings.vLowOrange;

	regs[c].regId = CONV_THRESHOLD_ORANGE;
	regs[c++].dataPtr = (void*)&hsvSettings.convSegOrange;
	// End orange HSV Registers

	// Green HSV Registers
	regs[c].regId = H_HIGH_GREEN;
	regs[c++].dataPtr = (void*)&hsvSettings.hHighGreen;

	regs[c].regId = H_LOW_GREEN;
	regs[c++].dataPtr = (void*)&hsvSettings.hLowGreen;

	regs[c].regId = S_HIGH_GREEN;
	regs[c++].dataPtr = (void*)&hsvSettings.sHighGreen;

	regs[c].regId = S_LOW_GREEN;
	regs[c++].dataPtr = (void*)&hsvSettings.sLowGreen;

	regs[c].regId = V_HIGH_GREEN;
	regs[c++].dataPtr = (void*)&hsvSettings.vHighGreen;

	regs[c].regId = V_LOW_GREEN;
	regs[c++].dataPtr = (void*)&hsvSettings.vLowGreen;

	regs[c].regId = CONV_THRESHOLD_GREEN;
	regs[c++].dataPtr = (void*)&hsvSettings.convSegGreen;
	// End Green HSV Registers


	regs[c].regId = DELTA_DISTANCE_FLOAT;
	regs[c++].dataPtr = (void*)&deltaDistance;

	regs[c].regId = PIT_INT;
	regs[c++].dataPtr = (void*)&actualPITFreq;

	regs[c].regId = FPS_SW_INT;
	regs[c++].dataPtr = (void*)&framesPerSecond_SW;

	regs[c].regId = FPS_HW_INT;
	regs[c++].dataPtr = (void*)&framesPerSecond_HW;

	//Navigation registers

	regs[c].regId = STRAIGHT_SPEED_FLOAT;
	regs[c++].dataPtr = (void*)&STRAIGHT_SPEED;

	regs[c].regId = BLIND_STRAIGHT_FLOAT;
	regs[c++].dataPtr = (void*)&BLIND_STRAIGHT_SPEED;

	regs[c].regId = BLIND_TURN_FLOAT;
	regs[c++].dataPtr = (void*)&TURN_SPEED;

	regs[c].regId = TURN_RADIUS_FLOAT;
	regs[c++].dataPtr = (void*)&TURN_RADIUS;


	regs[c].regId = TURN_ANGLE_INT;
	regs[c++].dataPtr = (void*)&TURN_ANGLE;

	regs[c].regId = FIND_RANGE_INT;
	regs[c++].dataPtr = (void*)&FIND_RANGE;

	/* End add registers */

}

void TransmitRegisterData(uint16 regId, uint8 subtype, uint8* buffer)
{
	int i;

	for(i = 0; i < MAX_NUM_REGS; i ++){
		if(regs[i].regId == regId)
			break;
	}

	RegInfo reg = regs[i];

	float regValueFloat = 0.0f;
	int regValueInt = 0;

	uint8* regValuePtr;
	
	if(regId <= INT_FLOAT_DIVIDER){
		regValueInt = *((int*)(reg.dataPtr));
		if(regId >= H_HIGH_ORANGE && regId <= CONV_THRESHOLD_ORANGE){
			regValueInt = (int)((uint8)(*((uint8*)reg.dataPtr)));
		}
		regValuePtr = (uint8*)&regValueInt;
	}
	else{
		regValueFloat = *((float*)(reg.dataPtr));
		regValuePtr = (uint8*)&regValueFloat;
	}

	uint8 regData[6];
	regData[0] = buffer[0];
	regData[1] = buffer[1];
	regData[2] = regValuePtr[0];
	regData[3] = regValuePtr[1];
	regData[4] = regValuePtr[2];
	regData[5] = regValuePtr[3];
	
	if(regId <= 100){
		SendPacket(REGISTER, TRANSMIT_INT, 6, regData);
	}
	else{
		SendPacket(REGISTER, TRANSMIT_FLOAT, 6, regData);
	}
	//if (messagesOn()){ //acknowledge:
	//	SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,6,"GetReg");
	//	if(regId <= 100)
	//		sendInt(regValueInt,DISPLAY_IN_GUI_NEWLINE);
	//	else
	//		sendFloat(regValueFloat,DISPLAY_IN_GUI_NEWLINE);
	//}
}

void SetRegister(uint16 regId, uint8 subtype, uint8* buffer)
{
	int i;

	for(i = 0; i < MAX_NUM_REGS; i ++){
		if(regs[i].regId == regId){
			if(regId <= INT_FLOAT_DIVIDER){
				int data = *((int*)(buffer + 2));
				DISABLE_INTERRUPTS();
				if(regs[i].regId >= H_HIGH_ORANGE && regs[i].regId <= CONV_THRESHOLD_ORANGE){
					*((uint8*)regs[i].dataPtr) = (uint8)data;
					FT_hsvSettingsChange();
				}
				else{
					*((int*)regs[i].dataPtr) = data;
				}
				if(regs[i].functionPtrInt > 0){
					(regs[i].functionPtrInt)(data);
				}
				RESTORE_INTERRUPTS(msr);
			}
			else{
				float data = *((float*)(buffer + 2));
				DISABLE_INTERRUPTS();
				*((float*)regs[i].dataPtr) = data;
				if(regs[i].functionPtrFloat > 0){
					(regs[i].functionPtrFloat)(data);
				}
				RESTORE_INTERRUPTS(msr);
			}
		}
	}
}



RegInfo* getRegister(int regId){
	int i;
	for(i = 0; i < MAX_NUM_REGS; i ++){
		if(regs[i].regId == regId){
			return &regs[i];
		}
	}
	return 0;
}

