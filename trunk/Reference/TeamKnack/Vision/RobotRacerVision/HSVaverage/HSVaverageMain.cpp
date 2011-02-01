#include <string>
#include "bitmap.h"
#include "cv.h"
#include "highgui.h"

#define SAVETOFILE 1
#define SHOWHSVAVERAGES 1


//using namespace std;
int main(int argc, char** argv)
{
		IplImage* frame1, *frame2, *orangeframe, *greenframe, *blueframe;

		char b[10];
		char temp[100];
		
		CvSize sz; cvSize(0, 0);
		
		
		CvScalar orangelow = cvScalar (115, 90, 180); //115 140 180
		CvScalar orangehigh = cvScalar(125, 200, 256); //130 200 256
		CvScalar greenlow = cvScalar  (40,   15, 140); // 40 30 190
		CvScalar greenhigh = cvScalar (90, 165, 256); // 80 150 256
		CvScalar bluelow = cvScalar  (0,  100 , 190); // 0 90 190
		CvScalar bluehigh = cvScalar (45, 160, 256); // 45 200 256

		CvScalar HSVaverage = cvScalar(0,0,0); //holds the result of the hue average
		//CvScalar Saverage = cvScalar(0); //holds the result of the saturation average
		//CvScalar Vaverage = cvScalar(0); //holds the result of the value average

		string filename = "";
		string level = "capture"; //affects what filename is given to the saved image
		string ext = ".bmp";
		string base = "capture"; //the name of the saved images from the micron demo board
		int minimagenumber = 0; //the minimum image number in 'sources' diretory that you want processed.
		int maximagenumber = 125;//the maximum image number in 'sources' directory, or the last image you want processed.
		
		for(int i=minimagenumber; i<maximagenumber; i++) // don't know how many images are in the sequence.
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

			if(SHOWHSVAVERAGES)
			{
				HSVaverage = cvAvg(frame2);
				CvFont font;
				double hScale = 0.5;
				double vScale = 0.5;
				int lineWidth = 1;
				cvInitFont(&font, CV_FONT_HERSHEY_SIMPLEX, hScale, vScale, 0, lineWidth);

				//write the average in the corner of each image

				sprintf(temp, "Averages: H:%3.2f S:%3.2f V:%3.2f", HSVaverage.val[0], HSVaverage.val[1], HSVaverage.val[2]);
				string averages = string(temp);

				cvPutText(frame1,averages.c_str(),cvPoint(20,20),&font,cvScalar(0,0,255));


			}
			
			if(SAVETOFILE)
			{
				filename = "results\\" + level  + string(b) + ".bmp";	//save the processed images to file.
				cvSaveImage((const char *) filename.c_str(), frame1);
			}
		}
		
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

		if(frame1)
			cvReleaseImage( &frame1 );
		if(frame2)
			cvReleaseImage( &frame2 );


        return 0;
	/*
	knackBMP mybmp;
	mybmp.setname("distance0000.bmp");
	mybmp.init_BMP();
	mybmp.makebmp("howdy.bmp");

	*/
}