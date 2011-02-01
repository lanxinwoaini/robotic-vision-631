/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    plb_camera.h
AUTHOR:  Barrett Edwards
CREATED: 5 Sep 2006

DESCRIPTION

Driver API header for the plb_camera module.


******************************************************************************/

#include <xbasic_types.h>


#define CAM_SAVE_OPTIONS_NONE					0
#define CAM_SAVE_OPTIONS_ORG					1
#define CAM_SAVE_OPTIONS_HSV					2
#define CAM_SAVE_OPTIONS_MASK					4
#define CAM_SAVE_OPTIONS_GREYSCALE				8
#define CAM_SAVE_OPTIONS_COL_COUNT				16

#define CAM_BURST_SIZE_8						0x01
#define CAM_BURST_SIZE_16						0x02
#define CAM_BURST_SIZE_32						0x04
#define CAM_BURST_SIZE_64						0x08
#define CAM_BURST_SIZE_128						0x10

//these are the register IDs to select which thresholder to read/write (regid)
#define THRESH_SEG_ORANGE_ID					0x80
#define THRESH_SEG_GREEN_ID						0x98
#define THRESH_SEG_TYPE_HUE_MAXMIN_OFFSET		0x00
#define THRESH_SEG_TYPE_SAT_MAXMIN_OFFSET		0x08
#define THRESH_SEG_TYPE_VAL_MAXMIN_OFFSET		0x10
//these are the registers to set the threshold after the convolution happens
#define THRESH_POSTCONVOLUTION_ORANGE			0xB0
#define THRESH_POSTCONVOLUTION_GREEN			0xB8


#define OFFSET_START_CAPTURE				0x00
#define OFFSET_WR_CAM_REG					0x08
#define OFFSET_START_CAM_REG_READ			0x10

#define OFFSET_STATUS						0x00
#define OFFSET_CAM_SER_READY				0x08
#define OFFSET_CAM_SER_DATA					0x10

#define OFFSET_CAM_BURST_SIZE				0x18
#define OFFSET_RESET_CAM_CORE 				0x20
#define OFFSET_ACKNOWLEDGE_INT 				0x28
#define OFFSET_SAVE_OPTIONS					0x30
#define OFFSET_ORG_FRAME_MEM_ADDR			0x38
#define OFFSET_HSV_FRAME_MEM_ADDR			0x40
#define OFFSET_MASK_ORANGE_FRAME_MEM_ADDR	0x48
#define OFFSET_MASK_GREEN_FRAME_MEM_ADDR	0x50
#define OFFSET_GREYSCALE_ORANGE_FRAME_MEM_ADDR	0x58
#define OFFSET_GREYSCALE_GREEN_FRAME_MEM_ADDR	0x60
#define OFFSET_COLCOUNT_ORANGE_MEM_ADDR		0x68
#define OFFSET_COLCOUNT_GREEN_MEM_ADDR		0x70

#define OFFSET_CAM_DISPLAY					0xC0
#define OFFSET_CONV_ZEROS_VAL				0xC8
#define OFFSET_CONV_COL_INIT				0xD0
#define OFFSET_CONV_ROW_INIT				0xD8
#define OFFSET_CONV_BETWEEN_FRAMES_INIT		0xE0
#define OFFSET_CONV_INIT_INVALID_INIT		0xE8
#define OFFSET_CONV_INITIAL_ZERO_INIT		0xF0

#define OFFSET_CONV_RESET					0xF8

#define PTR(X) ((volatile Xuint32 *)(X))

/////////// Display Options
//output to binary display:
#define CAM_DISPLAY_ORANGE_CONVOLUTION			0x0
#define CAM_DISPLAY_GREEN_CONVOLUTION			0x0
#define CAM_DISPLAY_ALL_CONVOLUTION				0x0
#define CAM_DISPLAY_ORANGE_MASK					0x1
#define CAM_DISPLAY_GREEN_MASK					0x2
#define CAM_DISPLAY_ALL_MASK					0x3
//count pixels from which signal:
#define CAM_DISPLAY_PIXCOUNT_ORG				0x0
#define CAM_DISPLAY_PIXCOUNT_HSV				0x0
#define CAM_DISPLAY_PIXCOUNT_1MASK_ORANGE		0x0
#define CAM_DISPLAY_PIXCOUNT_1MASK_GREEN		0x0
#define CAM_DISPLAY_PIXCOUNT_1CONV_ORANGE		0x0
#define CAM_DISPLAY_PIXCOUNT_1CONV_GREEN		0x0
#define CAM_DISPLAY_PIXCOUNT_8CONV_ORANGE		0x0
#define CAM_DISPLAY_PIXCOUNT_8CONV_GREEN		0x0
#define CAM_DISPLAY_PIXCOUNT_COLCOUNT_ORANGE	0x0
#define CAM_DISPLAY_PIXCOUNT_COLCOUNT_GREEN		0x0

// FUNCTION PROTOTYPES ////////////////////////////////////////////////////////

void ResetCameraCore(Xuint32 baseAddr);
void ConvReset(Xuint32 baseAddr);
void StartFrameCapture(Xuint32 baseAddr, Xuint32 originalFrameAddr, Xuint32 hsvFrameAddr, Xuint32 maskOrangeFrameAddr, Xuint32 maskGreenFrameAddr, Xuint32 greyscaleOrangeFrameAddr, Xuint32 greyscaleGreenFrameAddr, Xuint32 colcountOrangeAddr, Xuint32 colcountGreenAddr);
Xuint32 CameraStatus(Xuint32 baseAddr);

void SetRegisterValue(Xuint32 baseAddr, Xuint8 offset, Xuint16 val); ///these are mainly for setting and getting quick values to/from baseAddr+addr
Xuint32 GetRegisterValue(Xuint32 baseAddr, Xuint8 offset);
void SetDisplayOptions(Xuint32 baseAddr, Xuint8 options, Xuint8 pixSel);
Xuint32 GetPixelCount(Xuint32 baseAddr);

void WriteCameraRegister(Xuint32 baseAddr, Xuint8 addr, Xuint16 data);
Xuint32 ReadCameraRegister(Xuint32 baseAddr, Xuint8 addr);

void setFrameSaveOptions(Xuint32 baseAddr,Xuint32 options);
void setPLBBurstSize(Xuint32 baseAddr, Xuint32 value);

Xuint32 getFrameSaveOptions(Xuint32 baseAddr);
Xuint32 getPLBBurstSize(Xuint32 baseAddr);

void setSegmenterValues(Xuint32 baseAddr, Xuint8 regid, Xuint8 huemax, Xuint8 huemin, Xuint8 satmax, Xuint8 satmin, Xuint8 valmax, Xuint8 valmin);
void getSegmenterValues(Xuint32 baseAddr, Xuint8 regid, Xuint8* huemax, Xuint8* huemin, Xuint8* satmax, Xuint8* satmin, Xuint8* valmax, Xuint8* valmin);

void setHueMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8 huemax, Xuint8 huemin);
void setSatMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8 satmax, Xuint8 satmin);
void setValMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8 valmax, Xuint8 valmin);
void setPostConvolutionThreshold(Xuint32 baseAddr, Xuint8 regid, Xuint8 threshold);
Xuint32 getPostConvolutionThreshold(Xuint32 baseAddr, Xuint8 regid, Xuint8* threshold);
Xuint32 getHueMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8* valmax, Xuint8* valmin);
Xuint32 getSatMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8* valmax, Xuint8* valmin);
Xuint32 getValMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8* valmax, Xuint8* valmin);

void acknowledgeCameraInterrupt(Xuint32 baseAddr);

