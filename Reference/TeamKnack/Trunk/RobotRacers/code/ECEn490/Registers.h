
#ifndef REGISTERS_H_
#define REGISTERS_H_

#include "SystemTypes.h"
#include "Packet.h"
#include "TX.h"

#define MAX_NUM_REGS 50
#define INT_FLOAT_DIVIDER 100

enum RegisterIds
{
    //Any register ids below INT_FLOAT_ID_DIVIDER will be ints
    ENCODER_VALUE_INT = 1,
    NUM_MILLISECONDS_TO_RUN_INT = 2,
    DESIRED_ANGLE_INT = 3,
    DESIRED_ENCODER_VALUE_INT = 4,
	PID_DISTANCE_MODE_INT = 5,

	H_HIGH_ORANGE = 6,
    H_LOW_ORANGE = 7,
    S_HIGH_ORANGE = 8,
    S_LOW_ORANGE = 9,
    V_HIGH_ORANGE = 10,
    V_LOW_ORANGE = 11,

    H_HIGH_GREEN = 12,
    H_LOW_GREEN = 13,
    S_HIGH_GREEN = 14,
    S_LOW_GREEN = 15,
    V_HIGH_GREEN = 16,
    V_LOW_GREEN = 17,

    CONV_THRESHOLD_GREEN = 18,
    CONV_THRESHOLD_ORANGE = 19,

	PIT_INT = 24,
	FPS_SW_INT = 25,
	FPS_HW_INT = 28,

	TURN_ANGLE_INT = 26,
    FIND_RANGE_INT = 27,


    //Any register ids above INT_FLOAT_ID_DIVIDER will be floats
    KP_REG_FLOAT = 101,
    KD_REG_FLOAT = 102,
    KI_REG_FLOAT = 103,
    I_MAX_FLOAT = 104,
    I_MIN_FLOAT = 105,
    OUTPUT_PID_FLOAT = 106,
    DESIRED_VELOCITY_FLOAT = 107,
    NUM_METERS_TO_RUN_FLOAT = 108,
	TRANSMIT_STATE_VELOCITY_FLOAT = 109,

	DELTA_DISTANCE_FLOAT = 110,

	BLIND_TURN_FLOAT = 111,
    BLIND_STRAIGHT_FLOAT = 112,
    STRAIGHT_SPEED_FLOAT = 113,
    TURN_RADIUS_FLOAT = 114
};

typedef struct{
	uint16 regId;
	void* dataPtr;
	void (*functionPtrFloat)(float);
	void (*functionPtrInt)(int);
}RegInfo;


void initRegisters();
void TransmitRegisterData(uint16 regId, uint8 subtype, uint8* buffer);
void SetRegister(uint16 regId, uint8 subtype, uint8* buffer);
void addReg(RegInfo* reg);
RegInfo* getRegister(int regId);

#endif
