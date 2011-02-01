// MultiView.cpp : Defines the entry point for the console application.
//

#define GREEN
//define SZ320240
#define RUN2GARDENCOURT


#ifdef SZ320240
#define FILTERTHRESHOLD 10
#define FILTERMIN 17
#define FILTERMAX 90
#define VFALLDOWN_BEGINLOC 60
#else
#define FILTERTHRESHOLD 20
#define FILTERMIN 35
#define FILTERMAX 180
#define VFALLDOWN_BEGINLOC 120
#endif



#ifdef GREEN
	#define THRESHMATRIX1 20
	#define THRESHMATRIX2 24
	#define THRESHMATRIX3 104
	#define THRESHMATRIX4 254

	#define PIXELFALLDOWN
	#define HMIN 28
	#define HMAX 88
	#define SMIN 40
	#define SMAX 256
	#define VIM 39
	#define VMAX 256
	#define GREEN2HSV
	#ifdef SZ320240
		#define FILTERTHRESHOLD 25
	#else
		#define FILTERTHRESHOLD 50
	#endif
	#define THRESHOLD1 169
	#define EDGEMETHOD 0
	#define ERODE1 0
	#define DILATE1 0
	#define ERODE2 0
	#define DILATE2 0
	#define THRESHOLD2 0
	#define FINALIMAGECHOICE 3
#endif
#ifdef ORANGE
	#define THRESHMATRIX1 20
	#define THRESHMATRIX2 24
	#define THRESHMATRIX3 104
	#define THRESHMATRIX4 254

	#define PIXELFALLDOWN
	#define HMIN 139
	#define HMAX 8
	#define SMIN 51
	#define SMAX 256
	#define VIM 39
	#define VMAX 256
	#define THRESHOLD1 169
	#define EDGEMETHOD 0
	#define ERODE1 0
	#define DILATE1 0
	#define ERODE2 0
	#define DILATE2 0
	#define THRESHOLD2 0
	#define FINALIMAGECHOICE 3
#endif
#ifdef GREEN_JUSTIN
	#define HMIN 29
	#define HMAX 82
	#define SMIN 31
	#define SMAX 231
	#define VIM 69
	#define VMAX 256
	#define THRESHOLD1 230
	#define EDGEMETHOD 1
	#define ERODE1 0
	#define DILATE1 0
	#define ERODE2 0
	#define DILATE2 0
	#define THRESHOLD2 0
	#define FINALIMAGECHOICE 3
#endif
#ifdef GREEN_EDGE
	#define HMIN 28
	#define HMAX 88
	#define SMIN 40
	#define SMAX 256
	#define VIM 39
	#define VMAX 256
	#define THRESHOLD1 169
	#define EDGEMETHOD 10
	#define ERODE1 3
	#define DILATE1 3
	#define ERODE2 1
	#define DILATE2 0
	#define THRESHOLD2 120
	#define FINALIMAGECHOICE 3
#endif
#ifdef ORANGE_EDGE
	#define HMIN 139
	#define HMAX 8
	#define SMIN 51
	#define SMAX 256
	#define VIM 39
	#define VMAX 256
	#define THRESHOLD1 169
	#define EDGEMETHOD 10
	#define ERODE1 3
	#define DILATE1 3
	#define ERODE2 0
	#define DILATE2 0
	#define THRESHOLD2 120
	#define FINALIMAGECHOICE 3
#endif

/*
	1	Original image
	2	Edge Detection on image
		Dilate1
		Erode1
		Mask HSV values
	3	Dilate2
		Erode2
	4	Final image
	5	Analysis


*/
#include <string>
using std::string;
#include "bitmap.h"
#include "cv.h"
#include "highgui.h"

#define MINIMAGE 0
#define MAXIMAGE 500

static int currentlyAnalyzing = 0;
static const string ext = ".bmp";
#ifdef SZ320240
static const string directory = "..\\sources\\320240\\";
static const string base = "capture"; //the name of the saved images from the micron demo board
static const string base = "capture"; //the name of the saved images from the micron demo board
#else
	#ifdef RUN2GARDENCOURT
	static const string directory = "U:\\Projects\\RoboRacer\\Roboracer\\Vision\\Garden Court Feb 3\\Micron\\second run 6 pylon course\\";
	static const string base = "capture"; //the name of the saved images from the micron demo board
	#else
	static const string directory = "..\\sources\\640480\\";
	static const string base = "capture"; //the name of the saved images from the micron demo board
	#endif
#endif

static IplImage *src,*srcHSV,*srcGray,
				* dst,*dst3_1,*dst3_2,*dst1_1,*dst1_2,
				*dst_h_1,*dst_s_1,*dst_v_1,
				*imgMaskHSV,*imgMaskPFD,*imgMask,*dstGray,*dstHSV,*dstRGB,*dstEdge,
				*analysisPixelDropdown;
static CvSize sz;
static int *vertigram_total;
static int *vertigram_maxContiguous;
static int hmin,  hmax, smin, smax, vmin, vmax, threshold1, threshold2, displayHSV, displayFinalResult;
static int amntDilate1,amntDilate2,amntErode1,amntErode2,vfalldownBeginLoc;

static CvScalar rangemin = cvScalar(0,0,0);
static CvScalar rangemax = cvScalar(256,256,256);
 
static float array1[] = {
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              1,  1,  1,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0 };
static float array2[] = {
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0,
              0,  1,  0 };
static float array3[] = {
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1,
              1,  1,  1 };

CvMat matrix1 = cvMat(19,3,CV_32FC1,array1);
CvMat matrix2 = cvMat(19,3,CV_32FC1,array2);
CvMat matrix3 = cvMat(35,3,CV_32FC1,array3);


static void writeOnImageToScreen(IplImage * img, string s, string window);
static void initializeEdgeDetectionWindow();
static void initializeHSVRGBWindow();
static void initializeWindows();
static int vertiThresh = 10;
static void vertigram(IplImage* img, IplImage* output){
	static int firstRun = 1;
	if (firstRun){
		firstRun=0;
		vertigram_total = new int[sz.width];
		vertigram_maxContiguous = new int[sz.width];
	}
	//zero out the matrices
	for (int i = 0; i < sz.width; i++){ vertigram_total[i] = 0;vertigram_maxContiguous[i] = 0;}
	//add up each column's num non-zero pixels
	for (int col = 0; col < sz.width; col++){
		int currentContiguous = 0;
		int currentTotal = 0;
		unsigned char* currentDataPtr = (unsigned char*)img->imageData + col*img->nChannels + vfalldownBeginLoc*img->widthStep;
		for (int row = vfalldownBeginLoc; row < sz.height; row++){
			if (*currentDataPtr>vertiThresh){ //if there are any values in the pixels at this location
				currentContiguous++;
				currentTotal++;
			}
			else{ //the contiguous broke so:
				if (currentContiguous){
					vertigram_maxContiguous[col] = vertigram_maxContiguous[col]>currentContiguous?vertigram_maxContiguous[col]:currentContiguous;
					currentContiguous = 0;
				}
			}
			currentDataPtr += img->widthStep;
		} //done checking the current column:
		vertigram_total[col] = currentTotal;
		vertigram_maxContiguous[col] = vertigram_maxContiguous[col]>currentContiguous?vertigram_maxContiguous[col]:currentContiguous;
	}

	//now that we have the data, convert to image to display:
	cvSetZero(output);
	for (int col = 0; col < sz.width; col++){
		int contiguousPixelsLeft = vertigram_maxContiguous[col];
		int totalPixelsToDraw = vertigram_total[col];
		int sumPixels = totalPixelsToDraw - contiguousPixelsLeft;
		unsigned char* currentDataPtr = (unsigned char*)output->imageData + col*output->nChannels + (sz.height-totalPixelsToDraw) * output->widthStep;//we start so many rows down
		for (;totalPixelsToDraw>0;totalPixelsToDraw--){
			if (sumPixels-->0){
				*(currentDataPtr+1)=0xFF;
			} else {
				*(currentDataPtr+2)=0xFF;
			}
			*currentDataPtr = 2;
			currentDataPtr+= output->widthStep;
		}
	}
	//display:

	
}
float count(int * countarray, int max)
{
	int ct = 0;
	for(int i=0;i<max; i++)
	{
		if(countarray[i] != 0)
			ct++;
	}

	return (float)ct / (float)max;
}
void showFilter(IplImage* img,int max, float percent){
	char* currentDataPtr;
	int row = (int)(float(max) * percent) + FILTERTHRESHOLD;
	row = row<FILTERMIN?FILTERMIN:row;
	row = row>FILTERMAX?FILTERMAX:row;
	currentDataPtr = img->imageData + (img->height-row)*img->widthStep;
	for (int col = 0; col < img->width; col++){
		currentDataPtr += img->nChannels ; //go to the next pixel on the same row
		*currentDataPtr = 0xFF;
	}
}
void filter(int * countarray, int max, float percent, int width)
{
	int sub = (int)(float(max) * percent /2);
	for(int i=0; i< width; i++)
	{
		if(countarray[i] < sub)
			countarray[i] = 0;
	}
}
void countrowbits(IplImage * image, int * countarray, int height, int left, int right)
{
	unsigned char pixel;

	for(int i=0; i<height;i++)
		{
			countarray[i] = 0;
		}
	for(int row=0; row<height; row++)
	{
		for(int col=left; col<right; col++)
		{
			pixel = ((uchar*)(image->imageData + image->widthStep*row))[col];
			if(pixel != 0)
				countarray[row]++;

		}
	}

}
void countcolumnbits(IplImage *image, int * countarray, int width, int height)
{
	unsigned char pixel;
	

	for(int i=0; i<width;i++)
	{
		countarray[i] = 0;
	}
	for(int row=0; row < height; row++)
		{
		for(int col=0; col<width;col++)
			{
		
			//I(x,y) ~ ((uchar*)(img->imageData + img->widthStep*y))[x]
			pixel = ((uchar*)(image->imageData + image->widthStep*row))[col];
			if(pixel != 0)
				countarray[col]++;
		}
	}
}
int findmax(int * countarray, int width)
{
	int max = -1;
	int index = -1;
	for(int i=0; i< width; i++)
	{
		if(countarray[i] !=0 && countarray[i] > max)
		{
			max = countarray[i];
			index = i;
		}
	}
	return index;
}
int findcenter(int * countarray, int index, int &right, int &left, int width)
{
	left = index;
	right = index;


	while(countarray[left--] >0)
	{
		if(left == 0)
			break;
	}
	while(countarray[right++] > 0)
	{
		if(right == width)
			break;

	}
	return (right+left)/2;
	

}

static void writeOnImage(IplImage * img, string s){
	static CvFont font;			//setup the font information for writing to the image.
	static double hScale = 0.5;
	static double vScale = 0.5;
	static int lineWidth = 1;
	static int firstRun = 1;

	if (firstRun){
		cvInitFont(&font, CV_FONT_HERSHEY_SIMPLEX, hScale, vScale, 0, lineWidth);
		firstRun = 0;
	}
	
	if (img->nChannels>1) cvPutText(img,s.c_str(),cvPoint(20,20),&font,cvScalar(0,255,0));
	else cvPutText(img,s.c_str(),cvPoint(20,20),&font,cvScalar(255,0,0));

}
static void writeOnImageToScreen(IplImage * img, string s, string window){
	IplImage* screenImage;
	screenImage = cvCloneImage(img);
	writeOnImage(screenImage,s);
	cvShowImage(window.c_str(),screenImage);
	cvReleaseImage(&screenImage);
}

static int minConsecutivePixels = 1;
static void removeArtifacts(IplImage * img){
	char* currentDataPtr = img->imageData;
	int numConsecutivePixels;
	for (int row = 0; row < sz.height; row++){
		numConsecutivePixels = 0;
		for (int col = 0; col < sz.width; col++){
			if (*currentDataPtr) numConsecutivePixels++;
			else {
				if (numConsecutivePixels && numConsecutivePixels<=minConsecutivePixels){
					//delete all the previous pixels since they didn't meet min
					for (int i = minConsecutivePixels; i > 0; i--){
						*(currentDataPtr-i) = 0;
					}
				}
				numConsecutivePixels = 0;
			}

			currentDataPtr++; //if there's only one channel
		}
	}
}

static int PFD_numRowsToCheck = 5;
static void pixelFallDown(IplImage * img){
	//this masks out pixels that don't meet a certain range
	static char* colData;
	static int firstRun = 1;
	if (firstRun){
		firstRun=0;
		colData = new char[sz.height];
	}
	//zero out the matrices
	cvSetZero(imgMaskPFD);
	for (int col = 0; col < sz.width; col++){
		//Grab a column's data to analyze
		char* currentDataPtr = img->imageData + col*img->nChannels;
		for (int row = 0; row < sz.height; row++){
			colData[row] = *currentDataPtr;
			currentDataPtr += img->widthStep;
		} //done grabbing the current column:
		//for this column run analysis on pixels
		currentDataPtr = imgMaskPFD->imageData + col*imgMaskPFD->nChannels;
		for (int row = 0; row < sz.height; row++){
			float vdiff;
			char pixCol = colData[row];
			float sum = pixCol?255.0:0;
			if (pixCol == 0){
				//skip to the next, it's zero:
				*currentDataPtr = 0;
				currentDataPtr+= imgMaskPFD->widthStep;
				continue;
			}
			int bRow = row - PFD_numRowsToCheck < 0? 0: row - PFD_numRowsToCheck; //
			int eRow = row + PFD_numRowsToCheck > sz.height? sz.height: row + PFD_numRowsToCheck;
			for (;bRow < eRow; bRow++){
				//get the value of the pixels and diff them
				vdiff = (unsigned char)(pixCol) - (unsigned char)(colData[bRow]);

				vdiff = vdiff<0?-vdiff:vdiff;//PFD_valWeight; //square (ensures negative values
				sum -= vdiff;
			}
			*currentDataPtr = (unsigned int)(sum<0?0:sum);
			currentDataPtr+= imgMaskPFD->widthStep;
		}
	}
	removeArtifacts(imgMaskPFD);
	writeOnImageToScreen(imgMaskPFD,"FallDown","Step 2.2 - FallDownWindow");
}
static void setInRangeS(){
	if (currentlyAnalyzing) return;
	currentlyAnalyzing = 1;

	IplImage* image = srcHSV;
	if (rangemin.val[0] <= rangemax.val[0]){
		cvInRangeS(image, rangemin, rangemax, imgMaskHSV);			//find pixels in range
	} else {
		CvScalar Min1 = CvScalar(rangemin); Min1.val[0] = 0;				//from 0
		CvScalar Max1 = CvScalar(rangemax); Max1.val[0] = rangemax.val[0];	//to rangemax.val[0]
		CvScalar Min2 = CvScalar(rangemin); Min2.val[0] = rangemin.val[0];	//from rangemin.val[0]
		CvScalar Max2 = CvScalar(rangemax); Max2.val[0] = 255;				//to 255
		cvInRangeS(image, Min1, Max1, dst1_1);			//find pixels in range
		cvInRangeS(image, Min2, Max2, dst1_2);			//find pixels in range
		cvMax(dst1_1,dst1_2,imgMaskHSV);
	}
#ifdef GREEN2HSV
	CvScalar Min1,Max1;
	Min1.val[0] = 0;
	Max1.val[0] = 255;
	Min1.val[1] = 0;
	Max1.val[1] = 6;
	Min1.val[2] = 240;
	Max1.val[2] = 255;
	CvRect imageRect = cvRect(0,image->height*2/5,image->width,image->height/3);
	cvSetZero(dst1_1);
	{
		cvSetImageROI(image,imageRect);
		cvSetImageROI(dst1_1,imageRect);
		cvInRangeS(image,Min1, Max1, dst1_1);
		cvResetImageROI(image);
		cvResetImageROI(dst1_1);
	}
	{ //dilate and & the pixels
		cvDilate(imgMaskHSV,dst1_2,0,5);
		cvThreshold(dst1_2,dst1_2,20,255,CV_THRESH_BINARY);
		cvAnd(dst1_2,dst1_1,dst1_1);
	}
	{ //
		//cvMax(dst1_1,imgMaskHSV,dst1_1);
	}

	//cvMax(imgMaskHSV,dst1_1,imgMaskHSV);
#endif
	cvSplit(src,dst_h_1,dst_s_1,dst_v_1,0); //split the hsv into its components
	cvMin(imgMaskHSV,dst_h_1,dst_h_1);					//filter the image with Mask
	cvMin(imgMaskHSV,dst_s_1,dst_s_1);					//filter the image with Mask
	cvMin(imgMaskHSV,dst_v_1,dst_v_1);					//filter the image with Mask
	cvMerge(dst_h_1,dst_s_1,dst_v_1,0,dst);		//copy the 3 channels into one image
	
	cvShowImage("Step 2.1 - HSV ranging",dst);
	currentlyAnalyzing=0;
}

static void displayWithMask(IplImage* m, std::string text, std::string windowName){
	cvSplit(srcHSV,dst_h_1,dst_s_1,dst_v_1,0); //split the hsv into its co0mponents
	cvMin(m,dst_v_1,dst_v_1);					//filter the image with Mask
	cvMerge(dst_h_1,dst_s_1,dst_v_1,0,dstHSV);		//copy the 3 channels into one image
	cvCvtColor(dstHSV, dst, CV_HSV2BGR);
	writeOnImageToScreen(dst,text,windowName);
	
}

static int threshMatrix1 = THRESHMATRIX1;
static int threshMatrix2 = THRESHMATRIX2;
static int threshMatrix3 = THRESHMATRIX3;
static int threshMatrix4 = THRESHMATRIX4;

static void analyzeImages(){
		//create mask filter
		setInRangeS(); //outputs to imgMaskHSV. inputs src or srcHSV

		cvThreshold( imgMaskHSV, dst1_1, 1, 1, CV_THRESH_BINARY );

		cvFilter2D( dst1_1, dst1_2, &matrix1, cvPoint(-1,-1));
		cvThreshold( dst1_2, dst1_2, threshMatrix1, 256, CV_THRESH_BINARY );
		cvMin(dst1_2,imgMaskHSV,dst1_2);
		writeOnImageToScreen(dst1_2,"Matrix1","Matrix1Window");

		cvFilter2D( dst1_1, dst1_2, &matrix2, cvPoint(-1,-1));
		cvThreshold( dst1_2, dst1_2, threshMatrix2, 256, CV_THRESH_BINARY );
		cvMin(dst1_2,imgMaskHSV,dst1_2);
		writeOnImageToScreen(dst1_2,"Matrix2","Matrix2Window");

		cvFilter2D( dst1_1, dst1_2, &matrix3, cvPoint(-1,-1));
		cvThreshold( dst1_2, dst1_2, threshMatrix3, 256, CV_THRESH_BINARY );
		cvMin(dst1_2,imgMaskHSV,dst1_2);
		writeOnImageToScreen(dst1_2,"Matrix3","Matrix3Window");

				float a1[108];
				
				for(int i = 0; i < 3*35; i++)
				{
					//a1[i] = 5;
					a1[i]=1.0/(3*35);
				};

				CvMat kernel = cvMat( 35, 3, CV_32FC1, a1);
			cvThreshold( imgMaskHSV, dst1_1, 1, 256, CV_THRESH_BINARY );
			cvFilter2D(dst1_1, dst1_2, &kernel, cvPoint(-1,-1));
			cvThreshold( dst1_2, dst1_2, threshMatrix4, 256, CV_THRESH_BINARY );
			cvMin(dst1_2,imgMaskHSV,dst1_2);
		writeOnImageToScreen(dst1_2,"Matrix4","Matrix4Window");

		pixelFallDown(imgMaskHSV); //inputs IplImage* and outputs imgMaskPFD
		cvMin(imgMaskPFD,imgMaskHSV,imgMask);

		//displayWithMask(imgMask,"Composite Mask","CompositeMaskWindow");

		//if (amntDilate1) cvDilate(imgMask,imgMask,NULL,amntDilate1);
		//if (amntErode1) cvErode(imgMask,imgMask,NULL,amntErode1);

		cvSplit(srcHSV,dst_h_1,dst_s_1,dst_v_1,0); //split the hsv into its components
		cvMin(imgMask,dst_s_1,dst_s_1);					//filter the image with Mask
		cvMin(imgMask,dst_v_1,dst_v_1);					//filter the image with Mask
		cvMax(dst_v_1,dst_s_1,dst_v_1);
		IplImage* imageToUse = dst_v_1;
		//	cvShowImage("Step 3 - Result ImageHSV", imageToUse);	//display composite after masking

		//display the results
		if (amntDilate2) cvDilate(imageToUse,imageToUse,NULL,amntDilate2);
		if (amntErode2) cvErode(imageToUse,imageToUse,NULL,amntErode2);
		if (threshold2>0) cvThreshold( imageToUse, imageToUse, threshold2,255, CV_THRESH_BINARY );
		
		writeOnImageToScreen(imageToUse,"Max(S,V)","Step 4 - Choose Final Image");	//display composite after masking

		//now that they've chosen the final image to use and it's on 1 channel
		vertigram(imageToUse,analysisPixelDropdown);
		int max_index = findmax(vertigram_total,analysisPixelDropdown->width);
		int max = vertigram_total[max_index];
		float percent = count(vertigram_total,analysisPixelDropdown->width);
		showFilter(analysisPixelDropdown,max,percent);
		writeOnImageToScreen(analysisPixelDropdown,"Final Vertigram(tm)","Step 5 - vertiGram(tm)");
		//
		//initializeWindows();
}
static void updateImages(){
	if (!currentlyAnalyzing){
		currentlyAnalyzing = 1;
		analyzeImages();
		currentlyAnalyzing = 0;
	}
}

//creates a mask from the original hsv image based on min and max hsv values
//callback function for slider , implements opening 
static void pfdChange(int id)
{
	id;
	updateImages();
}
static void hminchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	rangemin.val[0] = (double)hmin;
	setInRangeS();
	updateImages();
}   
static void hmaxchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	rangemax.val[0] = (double)hmax;
	setInRangeS();
	updateImages();
}   
static void sminchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	rangemin.val[1] = (double)smin;
	setInRangeS();
	updateImages();
}   
static void smaxchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	rangemax.val[1] = (double)smax;
	setInRangeS();
	updateImages();
}   
static void vminchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	rangemin.val[2] = (double)vmin;
	setInRangeS();
	updateImages();
}   
static void vmaxchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	rangemax.val[2] = (double)vmax;
	setInRangeS();
	updateImages();
}   
static void updateImagesHndl(int id)
{
	id;
	updateImages();
}

static void initializeImages(){
	if (srcHSV) return;//only need to initialize once
	srcHSV = cvCreateImage(sz, IPL_DEPTH_8U,3);				//create images to store segmented versions
	srcGray = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	imgMaskPFD = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	imgMaskHSV = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	imgMask = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	dst = cvCreateImage(sz, IPL_DEPTH_8U,3);				//create images to store segmented versions
	dst3_1 = cvCreateImage(sz, IPL_DEPTH_8U,3);				//create images to store segmented versions
	dst3_2 = cvCreateImage(sz, IPL_DEPTH_8U,3);				//create images to store segmented versions
	dst1_1 = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	dst1_2 = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	dst_h_1 = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	dst_s_1 = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	dst_v_1 = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	dstGray = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	dstHSV = cvCreateImage(sz, IPL_DEPTH_8U,3);				//create images to store segmented versions
	dstRGB = cvCreateImage(sz, IPL_DEPTH_8U,3);				//create images to store segmented versions
	dstEdge = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions

	analysisPixelDropdown = cvCreateImage(sz,IPL_DEPTH_8U,3);
}
static void releaseImages(){
		cvReleaseImage(&srcGray);
		cvReleaseImage(&srcHSV);
		cvReleaseImage(&dst3_1);
		cvReleaseImage(&dst3_2);
		cvReleaseImage(&dst1_1);
		cvReleaseImage(&dst1_2);
		cvReleaseImage(&dst_h_1);
		cvReleaseImage(&dst_s_1);
		cvReleaseImage(&dst_v_1);
		cvReleaseImage(&imgMaskHSV);
		cvReleaseImage(&imgMaskPFD);
		cvReleaseImage(&imgMask);
		cvReleaseImage(&dst);
		cvReleaseImage(&dstGray);
		cvReleaseImage(&dstHSV);
		cvReleaseImage(&dstRGB);
		cvReleaseImage(&dstEdge);
}

static void initializeHSVRGBWindow(){
	cvNamedWindow("Step 2.1 - HSV ranging", CV_WINDOW_AUTOSIZE);
	cvCreateTrackbar("Hmin","Step 2.1 - HSV ranging",&hmin,181,hminchange);
	cvCreateTrackbar("Hmax","Step 2.1 - HSV ranging",&hmax,181,hmaxchange);
	cvCreateTrackbar("Smin","Step 2.1 - HSV ranging",&smin,256,sminchange);
	cvCreateTrackbar("Smax","Step 2.1 - HSV ranging",&smax,256,smaxchange);
	cvCreateTrackbar("Vmin","Step 2.1 - HSV ranging",&vmin,256,vminchange);
	cvCreateTrackbar("Vmax","Step 2.1 - HSV ranging",&vmax,256,vmaxchange);
}

static void initializeVariables(){
	hmin = HMIN;smin = SMIN;vmin = VIM;
	hmax = HMAX;smax = SMAX;vmax = VMAX;
	threshold1 = THRESHOLD1;threshold2 = THRESHOLD2;
	displayHSV = 2;displayFinalResult = FINALIMAGECHOICE;
	amntDilate1 = DILATE1;amntDilate2 = DILATE2;amntErode1 = ERODE1;amntErode2 = ERODE2;
	vfalldownBeginLoc = VFALLDOWN_BEGINLOC;
	rangemin = cvScalar(HMIN,SMIN,VIM);
	rangemax = cvScalar(HMAX,SMAX,VMAX);
}

static void initializeWindows(){
	///initialize the windows
	cvNamedWindow("Step 1 - Original Image", CV_WINDOW_AUTOSIZE);
	cvNamedWindow("Step 4 - Choose Final Image", CV_WINDOW_AUTOSIZE);
	cvCreateTrackbar("H-S-V","Step 4 - Choose Final Image",&displayFinalResult,4,updateImagesHndl);
	cvCreateTrackbar("Erode2","Step 4 - Choose Final Image",&amntErode2,5,updateImagesHndl);
	cvCreateTrackbar("Dilate2","Step 4 - Choose Final Image",&amntDilate2,5,updateImagesHndl);
	cvCreateTrackbar("Threshold2","Step 4 - Choose Final Image",&threshold2,256,updateImagesHndl);
	initializeHSVRGBWindow();
	//cvNamedWindow("Step 3 - Result ImageHSV", CV_WINDOW_AUTOSIZE);
	//cvCreateTrackbar("H-S-V","Step 3 - Result ImageHSV",&displayHSV,3,updateImagesHndl);
	//cvCreateTrackbar("Erode1","Step 3 - Result ImageHSV",&amntErode1,5,updateImagesHndl);
	//cvCreateTrackbar("Dilate1","Step 3 - Result ImageHSV",&amntDilate1,5,updateImagesHndl);
	cvNamedWindow("Step 5 - vertiGram(tm)", CV_WINDOW_AUTOSIZE);
	cvCreateTrackbar("Threshold","Step 5 - vertiGram(tm)",&vertiThresh,255,updateImagesHndl);
	cvNamedWindow("Step 2.2 - FallDownWindow", CV_WINDOW_AUTOSIZE);
	cvCreateTrackbar("NumRows X","Step 2.2 - FallDownWindow",&PFD_numRowsToCheck,10,pfdChange);
	cvCreateTrackbar("BeginLoc","Step 2.2 - FallDownWindow",&vfalldownBeginLoc,150,pfdChange);
	cvCreateTrackbar("consecPix","Step 2.2 - FallDownWindow",&minConsecutivePixels,4,pfdChange);
	//cvResizeWindow("Step 2.2 - FallDownWindow",350,600);
	//cvNamedWindow("CompositeMaskWindow", CV_WINDOW_AUTOSIZE);
	cvNamedWindow("Matrix1Window", CV_WINDOW_AUTOSIZE);
	cvCreateTrackbar("Threshold","Matrix1Window",&threshMatrix1,200,pfdChange);
	cvNamedWindow("Matrix2Window", CV_WINDOW_AUTOSIZE);
	cvCreateTrackbar("Threshold","Matrix2Window",&threshMatrix2,200,pfdChange);
	cvNamedWindow("Matrix3Window", CV_WINDOW_AUTOSIZE);
	cvCreateTrackbar("Threshold","Matrix3Window",&threshMatrix3,200,pfdChange);
	cvNamedWindow("Matrix4Window", CV_WINDOW_AUTOSIZE);
	cvCreateTrackbar("Threshold","Matrix4Window",&threshMatrix4,256,pfdChange);
}

//using namespace std;
int main(int argc, char** argv)
{
	char b[10];
	int key;
	char temp[100];

	string filename = "";
	
	initializeVariables();
	//initializeWindows only after variables and matrices have been initialized
	initializeWindows();

	///start looping

	int minimagenumber = MINIMAGE; //the minimum image number in 'sources' diretory that you want processed.
	int maximagenumber = MAXIMAGE;//the maximum image number in 'sources' directory, or the last image you want processed.
	int directionseeking = 1;
	int lastValidImageNum = minimagenumber;
	int currentImageNum = minimagenumber;
	int justlooped = 0;
	while(1){
		currentImageNum+= directionseeking;		// don't know how many images are in the sequence.
		if (currentImageNum == minimagenumber-1){
			currentImageNum = maximagenumber;
			justlooped = 1;
		}
		if (currentImageNum==maximagenumber+1){
			currentImageNum = minimagenumber; //check to loop back
			justlooped = 1;
		}
		if (justlooped && currentImageNum==lastValidImageNum) break; //we've already looped around and found nothing, so quit

		sprintf(b, "%d", currentImageNum);
		if(currentImageNum < 10)
			filename = directory + base + "000" + string(b) + ext;
		else if(currentImageNum<100)
			filename = directory + base + "00" + string(b) + ext;
		else
			filename = directory + base + "0" + string(b) + ext;

		if (src)	//make sure we release the data before getting a new image
			cvReleaseImage(&src);
		src = cvLoadImage((const char *) filename.c_str(), 1);
		if(!src)
			continue;		//if the image doesn't exist, skips it.
		lastValidImageNum =  currentImageNum;
		justlooped = 0;

		sz.height = src->height;
		sz.width = src->width;
		initializeImages();			//make sure memory is setup for the images



		// Get a clear copy of source

		cvShowImage("Step 1 - Original Image", src);
		cvCvtColor(src, srcHSV, CV_BGR2HSV);
		cvCvtColor(src, srcGray, CV_BGR2GRAY);
		srcHSV->origin = src->origin;
		
		analyzeImages();
		

		key=cvWaitKey(0);
		if(key==27 || key == 'q') break; //esc or q
		cout << key << endl;
		directionseeking=0;//don't change pics
		switch(key){
			default:
			case 'n':
			case ' ':
			case 2555904: //right arrow
			case 9: //tab
				directionseeking=1;
				break;
			case 'b':
			case 2424832: //left arrow
			case 8: //backspace
				directionseeking=-1;
				break;
				break;
				break;
			case 2490368: //up arrow
				break;
			case 2621440: //down arrow
				break;
		}
	}
	releaseImages();
	cvDestroyAllWindows();
    return 0;
}




