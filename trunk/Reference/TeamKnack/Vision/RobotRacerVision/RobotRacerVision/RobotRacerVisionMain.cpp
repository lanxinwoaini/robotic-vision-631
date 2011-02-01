#include <string>

#include "bitmap.h"
#include "cv.h"
#include "highgui.h"

#define SAVETOFILE 0
#define SHOWHSVAVERAGES 0
#define SOBEL 1
#define SHOWIMAGES 1


#define PROCESSORANGE 0
//define PROCESSGREEN
//define PROCESSBLUE

#define MINIMAGE 0
#define MAXIMAGE 130

void ReedTest();
void justinTest();
void mattTest();
void spencerTest();
void adjustImageRange();

bool init;

IplImage* src = 0;
IplImage* image = 0;
IplImage* dest = 0;
IplImage* dest1 = 0;
IplImage* dest2 = 0;

CvMat kernel;
CvPoint anchor;

CvSize sz; 

// Red
#ifdef PROCESSORANGE
CvScalar rangemin = cvScalar(166,40,29);
CvScalar rangemax = cvScalar(9,207,256);
#endif
#ifdef PROCESSGREEN
CvScalar rangemin = cvScalar(29,31,69);
CvScalar rangemax = cvScalar(82,231,256);
#endif



IplImage *frame1, *frame2, *orangeframe, *greenframe, *blueframe, *blocked, *final, *grayscale;
IplImage *anded, *filterGS;

int hmin,  hmax, smin, smax, vmin, vmax;

//using namespace std;
int main(int argc, char** argv)
{
		//ReedTest();
		init = false;
		justinTest();
        return 0;
}

void setInRangeS(){
	if (rangemin.val[0] <= rangemax.val[0]){
		cvInRangeS(image, rangemin, rangemax, dest);			//find pixels in range
		//cvInRangeS(frame2, orangelow, orangehigh, orangeframe);			//find pixels in range
	} else {
		CvScalar Min1 = CvScalar(rangemin); Min1.val[0] = 0;				//from 0
		CvScalar Max1 = CvScalar(rangemax); Max1.val[0] = rangemax.val[0];	//to rangemax.val[0]
		CvScalar Min2 = CvScalar(rangemin); Min2.val[0] = rangemin.val[0];	//from rangemin.val[0]
		CvScalar Max2 = CvScalar(rangemax); Max2.val[0] = 255;				//to 255
		cvInRangeS(image, Min1, Max1, dest1);			//find pixels in range
		cvInRangeS(image, Min2, Max2, dest2);			//find pixels in range
		cvAdd(dest1,dest2,dest);
	}
}

void hminchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	hmin = cvGetTrackbarPos("Hmin", "HSV ranging");
	rangemin.val[0] = (double)hmin;
	setInRangeS();
	if(init)
	{	
		adjustImageRange();
	}
}   
void hmaxchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	hmax = cvGetTrackbarPos("Hmax", "HSV ranging");
	rangemax.val[0] = (double)hmax;
	setInRangeS();
	if(init)
	{	
		adjustImageRange();
	}
}   
void sminchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	smin = cvGetTrackbarPos("Smin", "HSV ranging");
	rangemin.val[1] = (double)smin;
	setInRangeS();
	if(init)
	{	
		adjustImageRange();
	}
}   
void smaxchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	smax = cvGetTrackbarPos("Smax", "HSV ranging");
	rangemax.val[1] = (double)smax;
	setInRangeS();
	if(init)
	{	
		adjustImageRange();
	}
}   
void vminchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	vmin = cvGetTrackbarPos("Vmin", "HSV ranging");
	rangemin.val[2] = (double)vmin;
	setInRangeS();
	if(init)
	{	
		adjustImageRange();
	}
}   

void vmaxchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	vmax = cvGetTrackbarPos("Vmax", "HSV ranging");
	rangemax.val[2] = (double)vmax;
	setInRangeS();
	if(init)
	{	
		adjustImageRange();
	}
} 

void changeVal(int id)   
{
    id;
	if(init)
	{	
		adjustImageRange();
	}
} 



static int threshMatrix1 = 230;

void justinTest(){

		char b[10];
		char temp[100];
		
		int matWidth = 3;
		int matHeight = 35;
/*		
		int matWidth = 5;
		int matHeight = 61;
*/		
		//matWidth * matHeight
		float a1[108];
		
		for(int i = 0; i < matWidth*matHeight; i++)
		{
			//a1[i] = 5;
			a1[i]=1.0/(matWidth*matHeight);
		};

		CvSize sz; 
		cvSize(0, 0);

		kernel = cvMat( matHeight, matWidth, CV_32FC1, a1);
		anchor = cvPoint(-1,-1);


		string directory = "U:\\Projects\\RoboRacer\\Roboracer\\Vision\\Garden Court Feb 3\\Micron\\second run 6 pylon course\\";

		string filename = "";
		string level = "second6pylon"; //affects what filename is given to the saved image
		string ext = ".bmp";
		string base = "capture"; //the name of the saved images from the micron demo board
		int minimagenumber = MINIMAGE; //the minimum image number in 'sources' diretory that you want processed.
		int maximagenumber = MAXIMAGE;//the maximum image number in 'sources' directory, or the last image you want processed.
		
		for(int i=minimagenumber; i<=maximagenumber; i++) // don't know how many images are in the sequence.
		{
			sprintf(b, "%d", i);
			if(i < 10)
				filename = directory + base + "000" + string(b) + ext;
			else if(i<100)
				filename = directory + base + "00" + string(b) + ext;
			else
				filename = directory + base + "0" + string(b) + ext;

			src = cvLoadImage((const char *) filename.c_str(), 1);
			if(!src)
				continue;		//if the image doesn't exist, skips it.

			//frame1 = cvLoadImage("..\\sources\\capture0050.bmp", 1);

			
			sz.height = src->height;
			sz.width = src->width;



			image = cvCreateImage(sz, IPL_DEPTH_8U,3);						//create image to store HSV, convert to HSV
			//orangeframe = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
			grayscale = cvCreateImage(sz, IPL_DEPTH_8U,1);
			anded = cvCreateImage(sz, IPL_DEPTH_8U,1);
			final = cvCreateImage(sz, IPL_DEPTH_8U,1);
			blocked = cvCreateImage(sz, IPL_DEPTH_8U,1);
			filterGS = cvCreateImage(sz, IPL_DEPTH_8U,1);

			dest = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
			dest1 =cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
			dest2 =cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions

			//CvScalar orangelow = cvScalar (20, 27, 91); //115 140 180
			//CvScalar orangehigh = cvScalar(45, 132, 211); //130 200 256

//show original"..\\sources\\" + base + "000" + string(b) + ext;

			cvNamedWindow( "Original", 1 );		cvShowImage( "Original", src );

			cvCvtColor(src, image, CV_BGR2HSV);
			cvCvtColor(src, grayscale, CV_BGR2GRAY);
			
			setInRangeS();

			cvNamedWindow( "HSV ranging", 1 );	

			cvCreateTrackbar("Hmin","HSV ranging",&hmin,256,hminchange);
			cvSetTrackbarPos("Hmin","HSV ranging", (int)rangemin.val[0]);
			cvCreateTrackbar("Hmax","HSV ranging",&hmax,256,hmaxchange);
			cvSetTrackbarPos("Hmax","HSV ranging", (int)rangemax.val[0]);
			cvCreateTrackbar("Smin","HSV ranging",&smin,256,sminchange);
			cvSetTrackbarPos("Smin","HSV ranging", (int)rangemin.val[1]);
			cvCreateTrackbar("Smax","HSV ranging",&smax,256,smaxchange);
			cvSetTrackbarPos("Smax","HSV ranging", (int)rangemax.val[1]);
			cvCreateTrackbar("Vmin","HSV ranging",&vmin,256,vminchange);
			cvSetTrackbarPos("Vmin","HSV ranging", (int)rangemin.val[2]);
			cvCreateTrackbar("Vmax","HSV ranging",&vmax,256,vmaxchange);
			cvSetTrackbarPos("Vmax","HSV ranging", (int)rangemax.val[2]);

			cvNamedWindow( "ConvKernel", 1 );		
			cvNamedWindow( "SecondThresh", 1 );
			cvCreateTrackbar("Threshold","SecondThresh",&threshMatrix1,256,changeVal);
			init = true;
			adjustImageRange();
			cvWaitKey(0);
			if(i==maximagenumber)
			{
				i=minimagenumber; 
			}
		}
}

void adjustImageRange(){



			//cvInRangeS(frame2, orangelow, orangehigh, orangeframe);			//find pixels in range
			//cvInRangeS(image, rangemin, rangemax, dest);			//find pixels in range
			//cvInRangeS(frame2, orangelow, orangehigh, orangeframe);			//find pixels in range

			//cvShowImage( "HSV ranging", dest );
	
			cvShowImage("HSV ranging",dest);
			cvThreshold( dest, blocked, 1, 256, CV_THRESH_BINARY );
			cvFilter2D(blocked, filterGS, &kernel, anchor);

			cvShowImage( "ConvKernel", filterGS );

			cvThreshold( filterGS, filterGS, threshMatrix1, 256, CV_THRESH_BINARY );
			cvShowImage( "SecondThresh", filterGS );		

}

void ReedTest()
{
IplImage* frame1, *frame2, *orangeframe, *greenframe, *blueframe, *grayscale;
		frame1 = frame2 = orangeframe = greenframe = blueframe = grayscale = 0;

		char b[10];
		char temp[100];
		
		CvSize sz; cvSize(0, 0);
		
		
		CvScalar orangelow = cvScalar (0, 112, 125); //115 140 180
		CvScalar orangehigh = cvScalar(10, 184, 256); //130 200 256
		CvScalar greenlow = cvScalar  (30,   70, 120); // 40 30 190
		CvScalar greenhigh = cvScalar (70, 140, 256); // 80 150 256
		CvScalar bluelow = cvScalar  (100,  100 , 138); // 0 90 190
		CvScalar bluehigh = cvScalar (122, 205, 256); // 45 200 256


		CvScalar HSVaverage = cvScalar(0,0,0); //holds the result of the hue average

		string filename = "";
		string level = "second6pylon"; //affects what filename is given to the saved image
		string ext = ".bmp";
		string base = "capture"; //the name of the saved images from the micron demo board
		int minimagenumber = MINIMAGE; //the minimum image number in 'sources' diretory that you want processed.
		int maximagenumber = MAXIMAGE;//the maximum image number in 'sources' directory, or the last image you want processed.
		
		for(int i=minimagenumber; i<=maximagenumber; i++) // don't know how many images are in the sequence.
		{
			sprintf(b, "%d", i);
			if(i < 10)
				filename = "..\\sources\\" + base + "000" + string(b) + ext;
			else if(i<100)
				filename = "..\\sources\\" + base + "00" + string(b) + ext;
			else
				filename = "..\\sources\\" + base + "0" + string(b) + ext;

			frame1 = cvLoadImage((const char *) filename.c_str(), 1);
			if(!frame1)
				continue;		//if the image doesn't exist, skips it.

			sz.height = frame1->height;
			sz.width = frame1->width;

			frame2 = cvCreateImage(sz, IPL_DEPTH_8U,3);						//create image to store HSV, convert to HSV
			cvCvtColor(frame1, frame2, CV_BGR2HSV);

			orangeframe = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
			greenframe = cvCreateImage(sz, IPL_DEPTH_8U,1);
			blueframe = cvCreateImage(sz, IPL_DEPTH_8U,1);
			grayscale = cvCreateImage(sz, IPL_DEPTH_8U,1);

			cvCvtColor(frame1, grayscale, CV_BGR2GRAY); //get grayscale image

			cvInRangeS(frame2, orangelow, orangehigh, orangeframe);			//find pixels in range
			cvInRangeS(frame2, greenlow, greenhigh, greenframe);
			cvInRangeS(frame2, bluelow, bluehigh, blueframe);

			if(SOBEL)
			{
				cvSobel(grayscale, grayscale, 1, 0, 7); //do sobel in x on the grayscale image.
 
			}

			if(SHOWIMAGES)
			{
				cvNamedWindow("Orange", 1);
				cvNamedWindow("Green", 1);
				cvNamedWindow("Blue", 1);
				cvNamedWindow("Source", 1);
				cvNamedWindow("Gray", 1);

				cvShowImage("Source", frame1);
				cvShowImage("Gray", grayscale);
				cvShowImage("Orange", orangeframe);
				cvShowImage("Green", greenframe);
				cvShowImage("Blue", blueframe);
				cvWaitKey(0);

			}

			if(SHOWHSVAVERAGES)
			{
				HSVaverage = cvAvg(frame2);
				CvFont font;
				double hScale = 0.5;
				double vScale = 0.5;
				int lineWidth = 1;
				cvInitFont(&font, CV_FONT_HERSHEY_SIMPLEX, hScale, vScale, 0, lineWidth);

				//write the average in the corner of each image

				sprintf(temp, "Averages of original image: H:%3.2f S:%3.2f V:%3.2f", HSVaverage.val[0], HSVaverage.val[1], HSVaverage.val[2]);
				string averages = string(temp);

				cvPutText(orangeframe,averages.c_str(),cvPoint(20,20),&font,cvScalar(255,255,0));
				cvPutText(greenframe,averages.c_str(),cvPoint(20,20),&font,cvScalar(255,255,0));
				cvPutText(blueframe,averages.c_str(),cvPoint(20,20),&font,cvScalar(255,255,0));


			}
			
			if(SAVETOFILE)
			{
				filename = "results\\orange\\" + level  +"_"+ string(b) + ".bmp";	//save the processed images to file.
				cvSaveImage((const char *) filename.c_str(), orangeframe);
				filename = "results\\green\\" + level  +"_"+ string(b) + ".bmp";
				cvSaveImage((const char *) filename.c_str(), greenframe);
				filename = "results\\blue\\" + level  +"_"+ string(b) + ".bmp";
				cvSaveImage((const char *) filename.c_str(), blueframe);
			}
		}

		if(frame1)
			cvReleaseImage( &frame1 );
		if(frame2)
			cvReleaseImage( &frame2 );
		if(orangeframe)
			cvReleaseImage( &orangeframe );
		if(greenframe)
			cvReleaseImage( &greenframe );
		if(blueframe)
			cvReleaseImage( &blueframe );

}

void spencersTest(){
		IplImage* frame1, *frame2, *orangeframe, *greenframe, *blueframe;

		char b[10];
		char temp[100];
		
		CvSize sz; 
		cvSize(0, 0);
		
		/*
		CvScalar orangelow = cvScalar (115, 90, 180); //115 140 180		//these are the old values when we thought the source was RGB.  It's really BGR.
		CvScalar orangehigh = cvScalar(125, 200, 256); //130 200 256
		CvScalar greenlow = cvScalar  (40,   15, 140); // 40 30 190
		CvScalar greenhigh = cvScalar (90, 165, 256); // 80 150 256
		CvScalar bluelow = cvScalar  (0,  100 , 190); // 0 90 190
		CvScalar bluehigh = cvScalar (45, 160, 256); // 45 200 256
		*/
		CvScalar orangelow = cvScalar (20, 54, 113); //115 140 180
		CvScalar orangehigh = cvScalar(45, 114, 155); //130 200 256
		//CvScalar greenlow = cvScalar  (30,   70, 120); // 40 30 190
		//CvScalar greenhigh = cvScalar (70, 140, 256); // 80 150 256
		//CvScalar bluelow = cvScalar  (100,  100 , 138); // 0 90 190
		//CvScalar bluehigh = cvScalar (122, 205, 256); // 45 200 256


		//CvScalar HSVaverage = cvScalar(0,0,0); //holds the result of the hue average
		//CvScalar Saverage = cvScalar(0); //holds the result of the saturation average
		//CvScalar Vaverage = cvScalar(0); //holds the result of the value average
//show original
		frame1 = cvLoadImage("sources//capture0050.bmp", 1);
		cvNamedWindow( "Orange", 1 );		cvShowImage( "Orange", frame1 );		cvWaitKey(0);

// blur original
		//cvSmooth(frame1,frame1,CV_GAUSSIAN,5,5);
		//cvNamedWindow( "Orange", 1 );		cvShowImage( "Orange", frame1 );		cvWaitKey(0);

//		cvSmooth(frame1,frame1,CV_GAUSSIAN,3,3);
//		cvNamedWindow( "Orange", 1 );		cvShowImage( "Orange", frame1 );		cvWaitKey(0);


//segment
		sz.height = frame1->height;
		sz.width = frame1->width;

		frame2 = cvCreateImage(sz, IPL_DEPTH_8U,3);						//create image to store HSV, convert to HSV
		cvCvtColor(frame1, frame2, CV_BGR2HSV);

		orangeframe = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
		
		cvInRangeS(frame2, orangelow, orangehigh, orangeframe);			//find pixels in range
		
		cvNamedWindow( "Orange", 1 );		cvShowImage( "Orange", orangeframe );		cvWaitKey(0);

		
//erode dilate
		cvDilate(orangeframe,orangeframe,NULL,1);
		cvNamedWindow( "Orange", 1 );		cvShowImage( "Orange", orangeframe );		cvWaitKey(0);
		cvErode(orangeframe,orangeframe,NULL,3);
		cvNamedWindow( "Orange", 1 );		cvShowImage( "Orange", orangeframe );		cvWaitKey(0);
		cvDilate(orangeframe,orangeframe,NULL,3);
		cvNamedWindow( "Orange", 1 );		cvShowImage( "Orange", orangeframe );		cvWaitKey(0);
		cvErode(orangeframe,orangeframe,NULL,1);
		cvNamedWindow( "Orange", 1 );		cvShowImage( "Orange", orangeframe );		cvWaitKey(0);





		cvNamedWindow( "Orange", 1 );
		cvShowImage( "Orange", orangeframe );
		cvWaitKey(0);

		
		
//		cvCvtColor(frame3, frame1, CV_HSV2RGB);
//		cvSaveImage("HSVtoRGBafterSEG.bmp", frame1);

		
	//   string imagename = ("Green"+string(b)+" ");

	//	cvNamedWindow( "Orange", 1 );
    //  cvShowImage( "Orange", orangeframe );
    //   cvWaitKey(0); // very important, contains event processing loop inside
    //   cvDestroyWindow( "Orange" );
	//	cvNamedWindow((const char *)imagename.c_str(), 1 );
    //    cvShowImage( (const char *)imagename.c_str(), greenframe );
    //    cvWaitKey(0); // very important, contains event processing loop inside
    //    cvDestroyWindow( (const char *)imagename.c_str() );

}