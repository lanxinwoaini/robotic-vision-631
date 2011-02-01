
#include "Vision.h"
#include "TX.h"
#include "State.h"
#include "Packet.h"
#include "plb_camera.h"
#include "xparameters.h"
#include "InterruptControl.h"
#include "MemAlloc.h"



visiblePylon* visiblePylons; //this points to the SDRAM to processed pylon data
int numVisiblePylons = 0;
FrameTableEntry* checkedOutFrameForNav; //current checked-out frame
HSVSettings hsvSettings;
HSVSettings* hsvDynamicSettings;
int numPylons = 0;

visiblePylon fpgaPylons[MAX_PYLONS_IN_VIEW];	// Temporary for testing
int fpgaPylonCount = 0;					// Temporary for testing
int rand();

//Variables for vision post processing
//	Initialized in initHSVSettings
uint16* rowCount;
uint8* byteArray;


void initHSVSettings(){
	//static int zeroStartColors = 253; //what is filled in at the beginning and end of image
	//static int zeroEndColors = 126;	//when not debug, should both be 0's
	//zeroStartColors = (zeroStartColors + 64) % 256; //just rotate the colors at each initialization
	//zeroEndColors   = (zeroEndColors   + 64) % 256; //again, remove when not debugging

	hsvSettings.hHighOrange = 8;
	hsvSettings.hLowOrange = 139;
	hsvSettings.hHighGreen = 88;
	hsvSettings.hLowGreen = 28;

	hsvSettings.sHighOrange = 255;
	hsvSettings.sLowOrange = 110;  //51
	hsvSettings.sHighGreen = 255;
	hsvSettings.sLowGreen = 40;

	hsvSettings.vHighOrange = 255;
	hsvSettings.vLowOrange = 39;
	hsvSettings.vHighGreen = 255;
	hsvSettings.vLowGreen = 39;

	hsvSettings.convSegOrange = 60;
	hsvSettings.convSegGreen = 60;

	setHSVSettings();
	//SetRegisterValue(XPAR_PLB_CAM0_BASEADDR, OFFSET_CONV_ZEROS_VAL, zeroStartColors<<8 | zeroEndColors);
	//SetDisplayOptions(XPAR_PLB_CAM0_BASEADDR, CAM_DISPLAY_ALL_MASK, CAM_DISPLAY_PIXCOUNT_ORG);


	//Variables for Vision post processing
	rowCount = (uint16*) MemCalloc(FRAME_HEIGHT * 2);
	byteArray = (uint8*) MemCalloc(FRAME_HEIGHT*FRAME_WIDTH);
	hsvDynamicSettings = (HSVSettings*) MemCalloc(360/30 * HSVSETTINGS_STRUCT_SIZE);
	initHSVDynamicSettings();
}

void initHSVDynamicSettings(){
	int i;
	for (i = 0; i < 360/30; i++){
		hsvDynamicSettings[i].hHighOrange = hsvSettings.hHighOrange;
		hsvDynamicSettings[i].hLowOrange = hsvSettings.hLowOrange;
		hsvDynamicSettings[i].hHighGreen = hsvSettings.hHighGreen;
		hsvDynamicSettings[i].hLowGreen = hsvSettings.hLowGreen;

		hsvDynamicSettings[i].sHighOrange = hsvSettings.sHighOrange;
		hsvDynamicSettings[i].sLowOrange = hsvSettings.sLowOrange;
		hsvDynamicSettings[i].sHighGreen = hsvSettings.sHighGreen;
		hsvDynamicSettings[i].sLowGreen = hsvSettings.sLowGreen;

		hsvDynamicSettings[i].vHighOrange = hsvSettings.vHighOrange;
		hsvDynamicSettings[i].vLowOrange = hsvSettings.vLowOrange;
		hsvDynamicSettings[i].vHighGreen = hsvSettings.vHighGreen;
		hsvDynamicSettings[i].vLowGreen = hsvSettings.vLowGreen;

		hsvDynamicSettings[i].convSegOrange = hsvSettings.convSegOrange;
		hsvDynamicSettings[i].convSegGreen = hsvSettings.convSegGreen;
	}
}

void setHSVDynamicSettings(uint8* registerData){
	int i;
	for (i = 0; i < 360/30; i++){
		hsvDynamicSettings[i].hLowGreen = *(registerData++);
		hsvDynamicSettings[i].hHighGreen = *(registerData++);
		hsvDynamicSettings[i].sLowGreen = *(registerData++);
		hsvDynamicSettings[i].sHighGreen = *(registerData++);
		hsvDynamicSettings[i].vLowGreen = *(registerData++);
		hsvDynamicSettings[i].vHighGreen = *(registerData++);


		hsvDynamicSettings[i].hLowOrange = *(registerData++);
		hsvDynamicSettings[i].hHighOrange = *(registerData++);
		hsvDynamicSettings[i].sLowOrange = *(registerData++);
		hsvDynamicSettings[i].sHighOrange = *(registerData++);
		hsvDynamicSettings[i].vLowOrange = *(registerData++);
		hsvDynamicSettings[i].vHighOrange = *(registerData++);

		hsvDynamicSettings[i].convSegGreen = *(registerData++);
		hsvDynamicSettings[i].convSegOrange = *(registerData++);

		writeConvolutionThresholds(&hsvDynamicSettings[0]);
		
	}
}

void transmitDynamicHSV(){
	SendPacket(REGISTER,TRANSMIT_DYNAMIC_HSV,360/36*HSVSETTINGS_STRUCT_SIZE,(uint8*)hsvDynamicSettings);
}
void writeHSVregisters(HSVSettings * settings)
{
		hsvSettings.hHighGreen = settings->hHighGreen;
		hsvSettings.hLowGreen = settings->hLowGreen;
		hsvSettings.sHighGreen = settings->sHighGreen;
		hsvSettings.sLowGreen = settings->sLowGreen;
		hsvSettings.vHighGreen = settings->vHighGreen;
		hsvSettings.vLowGreen = settings->vLowGreen;
	
		hsvSettings.hHighOrange = settings->hHighOrange;
		hsvSettings.hLowOrange = settings->hLowOrange;
		hsvSettings.sHighOrange = settings->sHighOrange;
		hsvSettings.sLowOrange = settings->sLowOrange;
		hsvSettings.vHighOrange = settings->vHighOrange;
		hsvSettings.vLowOrange = settings->vLowOrange;

		hsvSettings.convSegOrange = settings->convSegOrange;
		hsvSettings.convSegGreen = settings->convSegGreen;

		FT_hsvSettingsChange();  //the next time a frame is finished the values will be changed.

		if (messagesOn())
		{
			SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,16,"Green Settings:");
			sendInt(hsvSettings.hLowGreen, DISPLAY_IN_GUI_NEWLINE);
			sendInt(hsvSettings.hHighGreen, DISPLAY_IN_GUI_NEWLINE);
			sendInt(hsvSettings.sLowGreen, DISPLAY_IN_GUI_NEWLINE);
			sendInt(hsvSettings.sHighGreen, DISPLAY_IN_GUI_NEWLINE);
			sendInt(hsvSettings.vLowGreen, DISPLAY_IN_GUI_NEWLINE);
			sendInt(hsvSettings.vHighGreen, DISPLAY_IN_GUI_NEWLINE);
			
			SendPacket(TEXT,DISPLAY_IN_GUI_NONEWLINE,15,"Orange Settings:");
			sendInt(hsvSettings.hLowOrange, DISPLAY_IN_GUI_NEWLINE);
			sendInt(hsvSettings.hHighOrange, DISPLAY_IN_GUI_NEWLINE);
			sendInt(hsvSettings.sLowOrange, DISPLAY_IN_GUI_NEWLINE);
			sendInt(hsvSettings.sHighOrange, DISPLAY_IN_GUI_NEWLINE);
			sendInt(hsvSettings.vLowOrange, DISPLAY_IN_GUI_NEWLINE);
			sendInt(hsvSettings.vHighOrange, DISPLAY_IN_GUI_NEWLINE);
		}


}
void writeConvolutionThresholds(HSVSettings * settings)
{
		hsvSettings.convSegOrange = settings->convSegOrange;
		hsvSettings.convSegGreen = settings->convSegGreen;

		FT_hsvSettingsChange();  //the next time a frame is finished the values will be changed.

}


///DO NOT USE BEFORE READING:
// the only place setHSVSettings can be called is within the FrameTable handler or before the camera begins!
// Do not use the setSegmenterValues, but set the registers (hsvSettings.blahblah) and then call FT_hsvSettingsChange()
void setHSVSettings(){
	setSegmenterValues(
		XPAR_PLB_CAM0_BASEADDR,
		THRESH_SEG_GREEN_ID,
		hsvSettings.hHighGreen,
		hsvSettings.hLowGreen,
		hsvSettings.sHighGreen,
		hsvSettings.sLowGreen,
		hsvSettings.vHighGreen,
		hsvSettings.vLowGreen);
	setSegmenterValues(
		XPAR_PLB_CAM0_BASEADDR,
		THRESH_SEG_ORANGE_ID, 
		hsvSettings.hHighOrange,
		hsvSettings.hLowOrange,
		hsvSettings.sHighOrange,
		hsvSettings.sLowOrange,
		hsvSettings.vHighOrange,
		hsvSettings.vLowOrange);
	setPostConvolutionThreshold(XPAR_PLB_CAM0_BASEADDR,THRESH_POSTCONVOLUTION_ORANGE,hsvSettings.convSegOrange);
	setPostConvolutionThreshold(XPAR_PLB_CAM0_BASEADDR,THRESH_POSTCONVOLUTION_GREEN,hsvSettings.convSegGreen);
}


void transmitPylonData(visiblePylon* visiblePylons, uint8 numPylons){
	
	uint8 data[numPylons][PYLON_STRUCT_SIZE];
	uint16* dataptr;

	int i;
	for(i = 0; i < numPylons; i ++){
		dataptr = (uint16*)&data[i][0];		// Set up array for transmission
		*dataptr = visiblePylons[i].height;
		dataptr++;
		*dataptr = visiblePylons[i].width;
		dataptr++;
		*dataptr = visiblePylons[i].center;
		dataptr++;
		*dataptr = visiblePylons[i].middle;
		data[i][8] = visiblePylons[i].color;
		data[i][9] = visiblePylons[i].reliability;
	}


	SendPacket_lowPriority(IMAGE, (PROCESSED_IMAGE << 4), (PYLON_STRUCT_SIZE * numPylons), *data); //the subtype for images resides in the most significant nibble (<<4)
	
}

void setColumnCount(uint8* imageData, uint16* colCount){
    int i, j, k;
    uint8 mask = 0x80;
	for(i = 0; i < FRAME_WIDTH; i ++){
		colCount[i] = 0;
	}

    for(i = FRAME_VALID_ROW_MIN + 90; i < FRAME_VALID_ROW_MAX; i ++){ //start further down the image (90 or so) to skip a lot of noise
  
        for(j = 0; j < FRAME_WIDTH / 8; j ++){
          
            for(k = 0; k < 8; k ++){
              
                if(imageData[i*FRAME_WIDTH/8 + j] & mask){
                    colCount[j*8+k] ++;
                }
                mask = mask >> 1;
                if(mask == 0){
                    mask = 0x80;
                }
            }
        }
    }
	//////TODO: fix hardware so that this can be removed: ...this was added by MJD because convolution was giving a weird output, and this fixes it
	colCount[7] = colCount[6];
	colCount[8] = colCount[9];
	////only remove above code when hardware is fixed
}
 


void VIS_PopulateVisiblePylons(uint8* convolvedImage, uint16* colCount, char color, 
								visiblePylon* visiblePylonArray, uint8* numVisiblePylons){
	int i;
	
	uint16 nonZeroCount = 0;
	uint16 maxHeight = 0;
	uint16 threshold = 0;
	
	uint16 leftBoundary[MAX_PYLONS];
	uint16 rightBoundary[MAX_PYLONS];
	
	int pylonCount = 0;
	
	for(i = 0; i < FRAME_WIDTH; i++)
	{
		uint16 count = colCount[i];
		if(count != 0)
		{
			nonZeroCount ++;
			if(count > maxHeight)
			{
				maxHeight = count;
			}
		}
	}
	
	//Set Threshold value
	if(color == ORANGE_PYLON)
	{
		threshold = ((float)maxHeight * ((float)nonZeroCount / (float)FRAME_WIDTH)) + FT_ORANGE_THRESHOLD_CONST;
		//threshold = FT_ORANGE_THRESHOLD_CONST;
	}
	else
	{
		threshold = ((float)maxHeight * ((float)nonZeroCount / (float)FRAME_WIDTH)) + FT_GREEN_THRESHOLD_CONST;
		//threshold = FT_GREEN_THRESHOLD_CONST;
	}	
	
	if(threshold < FT_COL_MIN_THRESHOLD) threshold = FT_COL_MIN_THRESHOLD;
	else if(threshold > FT_COL_MAX_THRESHOLD) threshold = FT_COL_MAX_THRESHOLD;
	
	//Find LR boundaries
	
	int32 tempLeft = -1;
	
	for(i = 0; i < FRAME_WIDTH && pylonCount < MAX_PYLONS; i++)
	{
		if(colCount[i] >= threshold){
			if(tempLeft == -1){		//have we set a left edge?
				tempLeft = i;		//then set one
			}
			//if we are on the very right edge of the frame (i = FRAME_WIDTH-1)
			//	and we are still above the threshold,
			//	we need to add the right boundary since there will never be a 
			//	right boundary below the threshold if the pylon is on the edge
			else if(i >= FRAME_WIDTH - 1)
			{
				leftBoundary[pylonCount] = tempLeft;
				rightBoundary[pylonCount] = i;	//i not i-1 b/c this col is still part of the pylon
				pylonCount ++;
				tempLeft = -1;
			}
		}
		else{		//if we don't meet threshold
			if(tempLeft != -1){	//have we set a left edge?
				if (i + 1 < FRAME_WIDTH && colCount[i+1]>=threshold) continue; //there's just 1 column that's below the threshold, so merge it
				if((i-1) - tempLeft >= FT_MIN_PYLON_WIDTH_PIXELS)	//if yes, set a right edge
				{
					leftBoundary[pylonCount] = tempLeft;
					rightBoundary[pylonCount] = i - 1;
					pylonCount ++;
				}
				tempLeft = -1;
			}
		}
	}	
	// end of finding Left-Right boundaries
	
	int j, k;
	
	for(i = 0; i < pylonCount; i ++)
	{	
		uint16 maxWidth = 0;
		uint16 maxIndex = FRAME_VALID_ROW_MIN;
		// Clear row count
		for(k = 0; k < FRAME_HEIGHT; k ++){
			rowCount[k] = 0;
		}

		for(k = FRAME_VALID_ROW_MIN; k < FRAME_VALID_ROW_MAX; k++)
		{
			int midPoint = (leftBoundary[i]+rightBoundary[i])/2;
			j = midPoint;
			int lastPointWasValid = FALSE;
			while (j >= leftBoundary[i] || (j >= 0 && lastPointWasValid)) { //count left
				uint8 word = *(convolvedImage + k*FRAME_WIDTH/8 + j/8);
				uint8 mask = 0x80 >> (j%8);
				if(word & mask){
					rowCount[k]++;
					lastPointWasValid = TRUE;
				} else {
					lastPointWasValid = FALSE;
				}
				j--;
			}
			j = midPoint + 1;
			lastPointWasValid = FALSE;
			while (j <= rightBoundary[i] || (j < FRAME_WIDTH && lastPointWasValid)) { //count left
				uint8 word = *(convolvedImage + k*FRAME_WIDTH/8 + j/8);
				uint8 mask = 0x80 >> (j%8);
				if(word & mask){
					rowCount[k]++;
					lastPointWasValid = TRUE;
				} else {
					lastPointWasValid = FALSE;
				}
				j++;
			}
			if (rowCount[k]>maxWidth){
				maxIndex = k;
				maxWidth = rowCount[k];
			}
		}
		
		//Find max row - start from top and find first row that is max wide
		//use rowCount, reuse max

		uint16 pTop = FRAME_VALID_ROW_MAX;
		uint16 pBottom = FRAME_VALID_ROW_MIN;
		uint16 pHeight = 0;
		uint16 pWidth = 0;
		uint16 pCenter = 0;
		uint16 pMiddle = 0;
		

		uint16 minWidth = maxWidth/10 + 1; //this will be the cutoff location of the pylon if it gets too skinny
		if (minWidth > 4) minWidth = 4;

		int16 tempTop = -1;
		for (k = FRAME_VALID_ROW_MIN; k < FRAME_VALID_ROW_MAX; k++)
		{
			if(rowCount[k] > minWidth){
				if (tempTop == -1){		//have we set a top edge?
					tempTop = k;		//then set one
				}
				//if we are on the very bottom of the frame (k = FRAME_VALID_ROW_MAX-1)
				// and we are still above the threshold,
				// we need to add the bottom boundary since there will never be a
				// bottom boundary below the threshold if the pylon extends to the edge
				else if (k >= FRAME_VALID_ROW_MAX-1)
				{
					if (k - tempTop > pHeight){ //if this is a taller section of pylon
						pBottom = k;
						pTop = tempTop;
						pHeight = pBottom - pTop; //the top is actually at a lower coordinate than the bottom
					}
					tempTop = -1;
				}
			} else {	//we don't meet minimum
				if (tempTop != -1){ //have we set a top edge?
					if (k - 1 - tempTop > pHeight){ //if this is a taller section of pylon
						pBottom = k-1;
						pTop = tempTop;
						pHeight = pBottom - pTop; //the top is actually at a lower coordinate than the bottom
					}
					tempTop = -1;
				}
			}
		}


		//adjust pTop & pBottom for the convolution:
		int convOffset = ((color==ORANGE_PYLON?hsvSettings.convSegOrange:hsvSettings.convSegGreen)*2/3 -33)/2;
		pTop -= convOffset;
		pBottom += pBottom > convOffset? convOffset : 0;

		pWidth = maxWidth;
		pCenter = (leftBoundary[i] + rightBoundary[i]) / 2;
		//adjust to make it distance from center of the screen
		//pCenter = FRAME_WIDTH / 2 - pCenter;
		pMiddle = (pTop + pBottom) / 2;
		
		//fprintf(file, "Pylon %d \n\tTop: %d\n\tBottom: %d\n\tHeight: %d\n\tWidth: %d\n\tCenter: %d\n\tMiddle:%d\n\n", i, pTop, pBottom, pHeight, pWidth, pCenter, pMiddle);
		
		//check proportions
		int16 proportions = pHeight / pWidth;
		if(proportions > FT_INVALID_HEIGHT_TO_WIDTH || pHeight >= FRAME_VALID_ROW_MAX - 70) //valid proportions
		{
			if(pBottom >= 230)//throw away blobs on top of the screen - pBottom is the lower boundary, with 0 at top of screen
			{
				if(pMiddle > FT_INVALID_MIDDLE_HEIGHT) //make sure the middle of the pylon isn't in the top 1/4 of the screen 0 = top 480=bot. 
				{
					//passed both tests - add to array
					uint8 index = *numVisiblePylons;
					visiblePylonArray[index].height = pHeight;
					visiblePylonArray[index].width = pWidth;
					visiblePylonArray[index].center = pCenter;
					visiblePylonArray[index].middle = pMiddle;
					visiblePylonArray[index].color = color;
					
					//set reliability
					uint8 pReliability = 0;

					//the farther off the proportions are from 12 the less reliable our vision is
					if(pHeight > FRAME_VALID_MAX_HEIGHT - 10)
					{
						pReliability = 100;
					}
					else
					{
						int diff = proportions - 12;
						if(diff > 12 || diff < -12)
						{
							pReliability = 0;
						}
						else
						{
							pReliability = 100 - ( (diff > 0 ? diff : -diff) * 8 );
						}	
					}
					visiblePylonArray[index].reliability = pReliability;
					
					(*numVisiblePylons)++;
				}
			}
		}
		//else do nothing - don't add to array - blob is wrong dimensions
	}//end for each blob
}

void navigationSetHSV(uint16 compassAngle)
{
	//implement later
	//call setDynamicHSVSettings, pass the base pointer to the correct index of hsvDynamicSettings.
	uint16 index = ((compassAngle+15)%360) / 30;
	writeHSVregisters(&hsvDynamicSettings[index]);
}
