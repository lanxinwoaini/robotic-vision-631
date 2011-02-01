// StereoVision.cpp : Defines the entry point for the console application.
//
#include <string>
using std::string;
#include "bitmap.h"
#include "cv.h"
#include "highgui.h"

#define MINIMAGE 0
#define MAXIMAGE 30

static const string ext = ".png";
static const string base = "stereo"; //the name of the saved images from the micron demo board
static const string directory = "stereoImages\\";

IplImage *srcLeft,*srcRight,*srcGrayLeft,*srcGrayRight,*dstFinal, *depthImage;
CvStereoBMState* bmState;
CvSize sz;
void updateImagesHndl(int id);
void initializeWindows(){
	///initialize the windows
	cvNamedWindow("Left Image", CV_WINDOW_AUTOSIZE);
	cvNamedWindow("Right Image", CV_WINDOW_AUTOSIZE);
	cvNamedWindow("Disparity", CV_WINDOW_AUTOSIZE);
	cvCreateTrackbar("uniquenessRatio","Disparity",&bmState->uniquenessRatio,20,updateImagesHndl);
	cvCreateTrackbar("minDisparity","Disparity",&bmState->minDisparity,20,updateImagesHndl);
	cvCreateTrackbar("numberOfDisparities","Disparity",&bmState->numberOfDisparities,128,updateImagesHndl);
	cvCreateTrackbar("speckleRange","Disparity",&bmState->speckleRange,20,updateImagesHndl);

}

static void processStereo(){
	//bmState->
	cvFindStereoCorrespondenceBM(srcGrayLeft,srcGrayRight,dstFinal,bmState);
	//cvReprojectImageTo3D(
	cvConvertScale( dstFinal, depthImage);
	cvShowImage("Disparity", depthImage);
}

static void updateImagesHndl(int id)
{
	int processing = 0;
	id;
	if (!processing){
		processing = 1;
		processStereo();
		processing = 0;
	}
}


//using namespace std;
int main(int argc, char** argv)
{
	int key;

	string filename_left = "";
	string filename_right = "";
	
	bmState = cvCreateStereoBMState();
	initializeWindows();

	filename_left = directory + base + "01" + "left" + ext;
	filename_right = directory + base + "01" + "right" + ext;
	srcLeft = cvLoadImage((const char *) filename_left.c_str(), 1);
	srcRight = cvLoadImage((const char *) filename_right.c_str(), 1);
	if (!srcLeft || !srcRight) return -1; 

	sz.height = srcLeft->height;
	sz.width = srcLeft->width;
	srcGrayLeft		= cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	srcGrayRight	= cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
	dstFinal		= cvCreateImage(sz, IPL_DEPTH_16S,1);				//create images to store segmented versions
	depthImage		= cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions

	cvShowImage("Left Image", srcLeft);
	cvShowImage("Right Image", srcRight);
	cvCvtColor(srcLeft, srcGrayLeft, CV_BGR2GRAY);
	cvCvtColor(srcRight, srcGrayRight, CV_BGR2GRAY);

	/*
	cvFindStereoCorrespondence( srcGrayLeft, srcGrayRight, CV_DISPARITY_BIRCHFIELD, depthImage, 50, 15, 3, 6, 8, 15 );
		cvFindStereoCorrespondence(
						   const  CvArr* leftImage, const  CvArr* rightImage,
						   int     mode, CvArr*  depthImage,
						   int     maxDisparity,
						   double  param1, double  param2, double  param3,
						   double  param4, double  param5  );

		leftImage
			Left image of stereo pair, rectified grayscale 8-bit image 
		rightImage
			Right image of stereo pair, rectified grayscale 8-bit image 
		mode
			Algorithm used to find a disparity (now only CV_DISPARITY_BIRCHFIELD is supported) 
		depthImage
			Destination depth image, grayscale 8-bit image that codes the scaled disparity, so that the zero disparity (corresponding to the points that are very far from the cameras) maps to 0, maximum disparity maps to 255. 
		maxDisparity
			Maximum possible disparity. The closer the objects to the cameras, the larger value should be specified here. Too big values slow down the process significantly. 
		param1, param2, param3, param4, param5
			- parameters of algorithm. For example, param1 is the constant occlusion penalty, param2 is the constant match reward, param3 defines a highly reliable region (set of contiguous pixels whose reliability is at least param3), param4 defines a moderately reliable region, param5 defines a slightly reliable region. If some parameter is omitted default value is used. In Birchfield's algorithm param1 = 25, param2 = 5, param3 = 12, param4 = 15, param5 = 25 (These values have been taken from "Depth Discontinuities by Pixel-to-Pixel Stereo" Stanford University Technical Report STAN-CS-TR-96-1573, July 1996.) 
	*/
			/*cvWaitKey(0);
			CvSize size = cvGetSize(srcGrayLeft);
			CvMat* disparity_left = cvCreateMat( size.height, size.width, CV_16S );
			CvMat* disparity_right = cvCreateMat( size.height, size.width, CV_16S );
			CvStereoGCState* state = cvCreateStereoGCState( 16, 2 );
			cvFindStereoCorrespondenceGC( srcGrayLeft, srcGrayRight,
				disparity_left, disparity_right, state, 0 );
			cvReleaseStereoGCState( &state );

			CvMat* disparity_left_visual2 = cvCreateMat( size.height, size.width, CV_8U );
			cvConvertScale( disparity_left, disparity_left_visual2, -16 );
			cvShowImage("Disparity", disparity_left_visual2);
			*/

	while(1){
		processStereo();


		key = cvWaitKey(0);
		if(key==27 || key == 'q') break; //esc or q
		cout << key << endl;
		switch(key){
			default:
			case 'n':
			case ' ':
			case 2555904: //right arrow
			case 9: //tab
				break;
			case 'b':
			case 2424832: //left arrow
			case 8: //backspace
				break;
				//cvSetTrackbarPos("Threshold","Step 2.2 - HSV ranging",threshold<256?threshold+1:256);
				break;
				//cvSetTrackbarPos("Threshold","Step 2.2 - HSV ranging",threshold>0?threshold-1:0);
				break;
			case 2490368: //up arrow
				break;
			case 2621440: //down arrow
				break;
		}
	}

	cvReleaseImage(&srcLeft);
	cvReleaseImage(&srcRight);
	cvReleaseImage(&srcGrayLeft);
	cvReleaseImage(&srcGrayRight);
	cvReleaseImage(&dstFinal);
	cvDestroyAllWindows();
    return 0;
}




