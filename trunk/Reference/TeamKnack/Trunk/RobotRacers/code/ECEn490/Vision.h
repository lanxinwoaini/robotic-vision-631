#include "SystemTypes.h"
#include "navigation.h"

#ifndef VISION_H_
#define VISION_H_


//#define HSV_REGISTER_BASE_ADDR	0x0

#define HIGH_H_ORANGE_OFFSET	0
#define LOW_H_ORANGE_OFFSET		1
#define HIGH_H_GREEN_OFFSET		2	
#define LOW_H_GREEN_OFFSET		3
#define HIGH_H_BLUE_OFFSET		4
#define LOW_H_BLUE_OFFSET		5

#define HIGH_S_ORANGE_OFFSET	6
#define LOW_S_ORANGE_OFFSET		7
#define HIGH_S_GREEN_OFFSET		8
#define LOW_S_GREEN_OFFSET		9
#define HIGH_S_BLUE_OFFSET		10
#define LOW_S_BLUE_OFFSET		11

#define HIGH_V_ORANGE_OFFSET	12
#define LOW_V_ORANGE_OFFSET		13
#define HIGH_V_GREEN_OFFSET		14
#define LOW_V_GREEN_OFFSET		15
#define HIGH_V_BLUE_OFFSET		16
#define LOW_V_BLUE_OFFSET		17

#define CONV_HEIGHT				33

#define MAX_PYLONS_IN_VIEW	10

#define PYLON_BASE_ADDR		0x0000
#define NUM_PYLONS_OFFSET	0x0000
#define PYLON_DATA_OFFSET	0x0000

#define PYLON_HEIGHT_OFFSET			0
#define PYLON_WIDTH_OFFSET			2
#define PYLON_CENTER_OFFSET			4
#define PYLON_MIDDLE_OFFSET			6
#define PYLON_COLOR_OFFSET			8
#define PYLON_RELIABILITY_OFFSET	9

#define PYLON_STRUCT_SIZE	10
#define HSVSETTINGS_STRUCT_SIZE 14

// Vision Post Processing defines
#define FT_MIN_PYLON_WIDTH_PIXELS			2	// for 640 width
#define FT_HEIGHT_TO_WIDTH_RATIO			12	//Ratio of pylons (measured)
#define FT_INVALID_HEIGHT_TO_WIDTH			2	//ratio at which blob proportions above this will be ignored
#define FT_INVALID_MIDDLE_HEIGHT			120	//height in frame of a pylon middle above which we ignore 

#define FRAME_VALID_ROW_MAX		FRAME_HEIGHT -16 //due to the convolution, top & bottom 16 pixels are bad
#define FRAME_VALID_ROW_MIN		16
#define FRAME_VALID_MAX_HEIGHT	FRAME_HEIGHT -32


typedef struct{
	uint8 hLowGreen;
	uint8 hHighGreen;
	uint8 sLowGreen;
	uint8 sHighGreen;
	uint8 vLowGreen;
	uint8 vHighGreen;

	uint8 hLowOrange;
	uint8 hHighOrange;
	uint8 sLowOrange;
	uint8 sHighOrange;
	uint8 vLowOrange;
	uint8 vHighOrange;

	uint8 convSegGreen;
	uint8 convSegOrange;
} __attribute__((__packed__)) HSVSettings;

typedef struct{
	uint16 height;
	uint16 width;
	uint16 center;			//left-right center
	uint16 middle;			//top-bottom center
	char color;				
	char reliability;		// 1-100 scale
}visiblePylon;

void transmitPylonData(visiblePylon* visiblePylons, uint8 numPylons);

void initHSVSettings();
void setHSVSettings();
void writeHSVregisters(HSVSettings* settings);
void writeConvolutionThresholds(HSVSettings* settings);
void initHSVDynamicSettings();
void setHSVDynamicSettings(uint8* registerData);
void transmitDynamicHSV();

void doSegmentation(uint16* hsvFrame, uint8* greenSegFrame, uint8* orangeSegFrame);
void segmentationTest();
void setColumnCount(uint8* imageData, uint16* colCount);
void VIS_PopulateVisiblePylons(uint8* convolvedImage, uint16* colCount, char color, 
							visiblePylon* visiblePylonArray, uint8* numVisiblePylons);
void navigationSetHSV(uint16 compassAngle);

#endif
