/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
/******************************************************************************

FILE:    plb_camera.c
AUTHOR:  Barrett Edwards
CREATED: 5 Sep 2006

DESCRIPTION

Driver API for the plb_camera module.

******************************************************************************/

#include "plb_camera.h"
#include "xparameters.h"

void StartFrameCapture(Xuint32 baseAddr, Xuint32 originalFrameAddr, Xuint32 hsvFrameAddr, Xuint32 maskOrangeFrameAddr, Xuint32 maskGreenFrameAddr, Xuint32 greyscaleOrangeFrameAddr, Xuint32 greyscaleGreenFrameAddr, Xuint32 colcountOrangeAddr, Xuint32 colcountGreenAddr){
	*PTR(baseAddr + OFFSET_ORG_FRAME_MEM_ADDR)			= originalFrameAddr;
	*PTR(baseAddr + OFFSET_HSV_FRAME_MEM_ADDR)			= hsvFrameAddr;
	*PTR(baseAddr + OFFSET_MASK_ORANGE_FRAME_MEM_ADDR)	= maskOrangeFrameAddr;
	*PTR(baseAddr + OFFSET_MASK_GREEN_FRAME_MEM_ADDR)	= maskGreenFrameAddr;
	*PTR(baseAddr + OFFSET_GREYSCALE_ORANGE_FRAME_MEM_ADDR)	= greyscaleOrangeFrameAddr;
	*PTR(baseAddr + OFFSET_GREYSCALE_GREEN_FRAME_MEM_ADDR)	= greyscaleGreenFrameAddr;
	*PTR(baseAddr + OFFSET_COLCOUNT_ORANGE_MEM_ADDR)	= colcountOrangeAddr;
	*PTR(baseAddr + OFFSET_COLCOUNT_GREEN_MEM_ADDR)		= colcountGreenAddr;
	*PTR(baseAddr + OFFSET_START_CAPTURE)				= 0;
}


void ConvReset(Xuint32 baseAddr){
	*PTR(baseAddr + OFFSET_CONV_RESET) = 0;
}


void WriteCameraRegister(Xuint32 baseAddr, Xuint8 addr, Xuint16 data){
	// Wait for any previous read/write to complete
	while(*PTR(baseAddr + OFFSET_CAM_SER_READY) == 0);
	
	*PTR(baseAddr + OFFSET_WR_CAM_REG) = (((Xuint32)addr) << 16) | ((Xuint32)data);	
}


Xuint32 GetRegisterValue(Xuint32 baseAddr, Xuint8 offset){
	return *PTR(baseAddr + offset);
}
void SetRegisterValue(Xuint32 baseAddr, Xuint8 offset, Xuint16 val){
	*PTR(baseAddr + offset) = val;
}
void SetDisplayOptions(Xuint32 baseAddr, Xuint8 options, Xuint8 pixSel){
	*PTR(baseAddr + OFFSET_CAM_DISPLAY) = options<<4 | pixSel;
}
Xuint32 GetPixelCount(Xuint32 baseAddr){
	return *PTR(baseAddr + OFFSET_CAM_DISPLAY);
}

Xuint32 ReadCameraRegister(Xuint32 baseAddr, Xuint8 addr){
	// Wait for any previous read/write to complete
	while(*PTR(baseAddr + OFFSET_CAM_SER_READY) == 0);

	// Signal to start read
	*PTR(baseAddr + OFFSET_START_CAM_REG_READ) = (Xuint32)addr;

	// Wait until read is complete
	while(*PTR(baseAddr + OFFSET_CAM_SER_READY) == 0);

	return *PTR(baseAddr + OFFSET_CAM_SER_DATA);
}


Xuint32 CameraStatus(Xuint32 baseAddr){
	return *PTR(baseAddr + OFFSET_STATUS);
}


void setFrameSaveOptions(Xuint32 baseAddr,Xuint32 options){
	*PTR(baseAddr + OFFSET_SAVE_OPTIONS) = (Xuint32)options;
}

void setSegmenterValues(Xuint32 baseAddr, Xuint8 regid, Xuint8 huemax, Xuint8 huemin, Xuint8 satmax, Xuint8 satmin, Xuint8 valmax, Xuint8 valmin){
	setHueMaxMin(baseAddr, regid, huemax, huemin);
	setSatMaxMin(baseAddr, regid, satmax, satmin);
	setValMaxMin(baseAddr, regid, valmax, valmin);
}

void setHueMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8 huemax, Xuint8 huemin){
	Xuint32 huemaxmin	= ((Xuint32)huemax) << 8 | ((Xuint32)huemin);
	*PTR(baseAddr + regid + THRESH_SEG_TYPE_HUE_MAXMIN_OFFSET) = huemaxmin;
}

void setSatMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8 satmax, Xuint8 satmin){
	Xuint32 satmaxmin	= ((Xuint32)satmax) << 8 | ((Xuint32)satmin);
	*PTR(baseAddr + regid + THRESH_SEG_TYPE_SAT_MAXMIN_OFFSET) = satmaxmin;
}

void setValMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8 valmax, Xuint8 valmin){
	Xuint32 valmaxmin	= ((Xuint32)valmax) << 8 | ((Xuint32)valmin);
	*PTR(baseAddr + regid + THRESH_SEG_TYPE_VAL_MAXMIN_OFFSET) = valmaxmin;
}

void setPostConvolutionThreshold(Xuint32 baseAddr, Xuint8 regid, Xuint8 threshold){
	Xuint32 threshmaxmin	= ((Xuint32)255) << 8 | ((Xuint32)threshold);
	*PTR(baseAddr + regid) = threshmaxmin;
}

void getSegmenterValues(Xuint32 baseAddr, Xuint8 regid, Xuint8* huemax, Xuint8* huemin, Xuint8* satmax, Xuint8* satmin, Xuint8* valmax, Xuint8* valmin){
	getHueMaxMin(baseAddr, regid, huemax, huemin);
	getSatMaxMin(baseAddr, regid, satmax, satmin);
	getValMaxMin(baseAddr, regid, valmax, valmin);
}

Xuint32 getPostConvolutionThreshold(Xuint32 baseAddr, Xuint8 regid, Xuint8* threshold){
	Xuint32 postConvmaxmin = *PTR(baseAddr + regid);
	*threshold = postConvmaxmin & 0xff;
	return postConvmaxmin;
}

Xuint32 getHueMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8* huemax, Xuint8* huemin){
	Xuint32 huemaxmin = *PTR(baseAddr + regid + THRESH_SEG_TYPE_HUE_MAXMIN_OFFSET);
	*huemax = (huemaxmin >> 8) & 0xff;
	*huemin = (huemaxmin >>  0) & 0xff;
	return huemaxmin;
}

Xuint32 getSatMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8* satmax, Xuint8* satmin){
	Xuint32 satmaxmin = *PTR(baseAddr + regid + THRESH_SEG_TYPE_SAT_MAXMIN_OFFSET);
	*satmax = (satmaxmin >> 8) & 0xff;
	*satmin = (satmaxmin >> 0) & 0xff;
	return satmaxmin;
}

Xuint32 getValMaxMin(Xuint32 baseAddr, Xuint8 regid, Xuint8* valmax, Xuint8* valmin){
	Xuint32 valmaxmin = *PTR(baseAddr + regid + THRESH_SEG_TYPE_VAL_MAXMIN_OFFSET);
	*valmax = (valmaxmin >>  8) & 0xff;
	*valmin = (valmaxmin >>  0) & 0xff;
	return valmaxmin;
}

Xuint32 getFrameSaveOptions(Xuint32 baseAddr){
	return (Xuint32) *PTR(baseAddr + OFFSET_SAVE_OPTIONS);
}

void setPLBBurstSize(Xuint32 baseAddr, Xuint32 value){
	*PTR(baseAddr + OFFSET_CAM_BURST_SIZE) = value;
}



Xuint32 getPLBBurstSize(Xuint32 baseAddr){
	return *PTR(baseAddr + OFFSET_CAM_BURST_SIZE);
}



void ResetCameraCore(Xuint32 baseAddr){
	*PTR(baseAddr + OFFSET_RESET_CAM_CORE) = 0;
}

void acknowledgeCameraInterrupt(Xuint32 baseAddr)
{
	*PTR(baseAddr + OFFSET_ACKNOWLEDGE_INT) = 0;
}
