#include <string>
#include <math.h>

#include "bitmap.h"
#include "cv.h"
#include "highgui.h"

#define SAVETOFILE 1
#define SHOWHSVAVERAGES 1


#define PROCESSORANGE 1
#define PROCESSGREEN 1
#define PROCESSBLUE 1

#define MINIMAGE 30
#define MAXIMAGE 90


//using namespace std;
int main(int argc, char** argv)
{
		IplImage* frame1, *frame2, *hue, *saturation, *value, *tempimage, *huedest, *saturationdest, *valuedest, *combination, *grayscale;
		
		char b[10];
		char temp[100];
		
		CvSize sz; 
		
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

			cvNamedWindow("Source", 1);
			cvShowImage("Source", frame1);

			sz.height = frame1->height;
			sz.width = frame1->width;

			frame2 = cvCreateImage(sz, IPL_DEPTH_8U,3);						//create image to store HSV, convert to HSV
			grayscale = cvCreateImage(sz, IPL_DEPTH_8U,1);
			hue = cvCreateImage(sz, IPL_DEPTH_8U, 1);
			saturation = cvCreateImage(sz, IPL_DEPTH_8U, 1);
			value = cvCreateImage(sz, IPL_DEPTH_8U, 1);
			tempimage = cvCreateImage(sz, IPL_DEPTH_8U, 1);
			huedest = cvCreateImage(sz, IPL_DEPTH_16U,1);
			saturationdest = cvCreateImage(sz, IPL_DEPTH_16U,1);
			valuedest = cvCreateImage(sz, IPL_DEPTH_16U,1);
			combination = cvCreateImage(sz, IPL_DEPTH_16U,1);

			cvCvtColor(frame1, grayscale, CV_BGR2GRAY);
			cvCvtColor(frame1, frame2, CV_BGR2HSV);
			//cvCopy(frame1, frame2);
			//cvSetImageCOI(frame2, 1);
			//cvCopy(frame2, hue);
			//cvSetImageCOI(frame2, 2);
			//cvCopy(frame2, saturation);
			//cvSetImageCOI(frame2, 3);
			//cvCopy(frame2, value);

			cvSobel(grayscale, grayscale, 1, 0, 5);
			//cvSobel(hue, huedest, 1, 0, 7);
			//cvSobel(saturation, saturationdest, 1, 0, 7);
			//cvSobel(value, valuedest, 1, 0, 7);

			//cvAdd(valuedest, saturationdest, combination);
			//cvAdd(combination, valuedest, combination);


			cvNamedWindow("Sobel", 1);
			cvShowImage("Sobel", grayscale);
			//cvNamedWindow( "Sobelx hue", 1 );
			//cvShowImage( "Sobelx hue", huedest );
			//cvNamedWindow( "Sobelx sat", 1 );
			//cvShowImage( "Sobelx sat", saturationdest );
			//cvNamedWindow( "Sobelx val", 1 );
			//cvShowImage( "Sobelx val", valuedest  );
			//cvNamedWindow( "Comb", 1 );
			//cvShowImage( "Comb", combination  );
			



			cvWaitKey(0); // very important, contains event processing loop inside
    



			

			
		}
		

        return 0;
	
}