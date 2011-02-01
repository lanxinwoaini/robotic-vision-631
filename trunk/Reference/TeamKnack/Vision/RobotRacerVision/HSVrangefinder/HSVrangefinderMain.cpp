#include <string>
#include <iostream>
#include <fstream>

#include "bitmap.h"
#include "cv.h"
#include "highgui.h"

#define PROCESSORANGE 1
#define PROCESSGREEN 1
#define PROCESSBLUE 1

#define MINIMAGE 0
#define MAXIMAGE 34

IplImage* src = 0;
IplImage* image = 0;
IplImage* dest = 0;
IplImage* dest1 = 0;
IplImage* dest2 = 0;

CvSize sz; 
CvScalar rangemin = cvScalar(0,0,0);
CvScalar rangemax = cvScalar(256,256,256);

ofstream outfile;



//the address of variable which receives trackbar position update 
int hmin,  hmax, smin, smax, vmin, vmax;

void setInRangeS(){
	if (rangemin.val[0] <= rangemax.val[0]){
		cvInRangeS(image, rangemin, rangemax, dest);			//find pixels in range
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

//callback function for slider , implements opening 
void hminchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	hmin = cvGetTrackbarPos("Hmin", "HSV ranging");
	rangemin.val[0] = (double)hmin;
	setInRangeS();
	cvShowImage("HSV ranging",dest);
}   
void hmaxchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	hmax = cvGetTrackbarPos("Hmax", "HSV ranging");
	rangemax.val[0] = (double)hmax;
	setInRangeS();
	cvShowImage("HSV ranging",dest);
}   
void sminchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	smin = cvGetTrackbarPos("Smin", "HSV ranging");
	rangemin.val[1] = (double)smin;
	setInRangeS();
	cvShowImage("HSV ranging",dest);
}   
void smaxchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	smax = cvGetTrackbarPos("Smax", "HSV ranging");
	rangemax.val[1] = (double)smax;
	setInRangeS();
	cvShowImage("HSV ranging",dest);
}   
void vminchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	vmin = cvGetTrackbarPos("Vmin", "HSV ranging");
	rangemin.val[2] = (double)vmin;
	setInRangeS();
	cvShowImage("HSV ranging",dest);
}   
void vmaxchange(int id)   
{
    id;
	//recompute min and max, create new destination image and show.
	vmax = cvGetTrackbarPos("Vmax", "HSV ranging");
	rangemax.val[2] = (double)vmax;
	setInRangeS();
	cvShowImage("HSV ranging",dest);
}   

int main( int argc, char** argv )
{
		char b[10];
		char temp[100];

		string filename = "";
		string level = "level1"; //affects what filename is given to the saved image
		string ext = ".jpg";
		string base = "gardencourt"; //the name of the saved images from the micron demo board
		string resultdirectory = "results\\orange\\";
		int minimagenumber = MINIMAGE; //the minimum image number in 'sources' diretory that you want processed.
		int maximagenumber = MAXIMAGE;//the maximum image number in 'sources' directory, or the last image you want processed.

		CvFont font;			//setup the font information for writing to the image.
		double hScale = 0.5;
		double vScale = 0.5;
		int lineWidth = 1;
		cvInitFont(&font, CV_FONT_HERSHEY_SIMPLEX, hScale, vScale, 0, lineWidth);

		for(int j=0; j<3;j++)
		{

			switch(j)
			{
			case 1:
				if(PROCESSGREEN)
				{
					resultdirectory = "results\\green\\";
					outfile.open("results\\green\\stats.csv");
					break;
				}
				else
					continue;
				
			case 2:
				if(PROCESSBLUE)
				{
					resultdirectory = "results\\blue\\";
					outfile.open("results\\blue\\stats.csv");
					break;
				}
				else
					continue;
				
			default:
				if(PROCESSORANGE)
				{
					resultdirectory = "results\\orange\\";
					outfile.open("results\\orange\\stats.csv", ios::out | ios::app);
					outfile <<"Orange results:" << endl;
					break;
				}
				else
					continue;
				
			}
			for(int i=minimagenumber; i<=maximagenumber; i++) // don't know how many images are in the sequence.
			{
				sprintf(b, "%d", i);
					if(i < 10)
						filename = "..\\sources\\" + base + "000" + string(b) + ext;
					else if(i<100)
						filename = "..\\sources\\" + base + "00" + string(b) + ext;
					else
						filename = "..\\sources\\" + base + "0" + string(b) + ext;

					src = cvLoadImage((const char *) filename.c_str(), 1);
					if(!src)
						continue;		//if the image doesn't exist, skips it.

						sz.height = src->height;
						sz.width = src->width;

				image = cvCreateImage(sz, IPL_DEPTH_8U,3);						//create image to store HSV, convert to HSV
						cvCvtColor(src, image, CV_BGR2HSV);

			

				dest = cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
				dest1 =cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
				dest2 =cvCreateImage(sz, IPL_DEPTH_8U,1);				//create images to store segmented versions
				setInRangeS();											//find pixels in range



				//create windows for output images
   				cvNamedWindow("HSV ranging", 1);
				cvNamedWindow("Source",1);
			    

				cvShowImage("HSV ranging",dest);
				cvShowImage("Source", src);

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

				cvWaitKey(0);

				sprintf(temp, "H:%3d - %3d S:%3d - %3d V:%3d - %3d", hmin, hmax, smin, smax, vmin, vmax);
				string rangestring = string(temp);

				cvPutText(dest,rangestring.c_str(),cvPoint(20,20),&font,cvScalar(255,255,255));
				
				
				outfile << level + string(b) << ", " << hmin << ", " << hmax << ", " << smin << ", " << smax << ", " << vmin <<", " << vmax << endl;
				

				filename = resultdirectory + level  +"_"+ string(b) + ".bmp";	//save the processed images to file.
				cvSaveImage((const char *) filename.c_str(), dest);

				//save the files with the current information
				//releases header an dimage data  
				cvReleaseImage(&src);
				cvReleaseImage(&image);
				cvReleaseImage(&dest);
				//destroys windows 
				cvDestroyWindow("HSV ranging"); 

			} //for loop to loop through each image
			outfile.close(); //close the file
		}//for loop to loop through each pylon color: orange, green, blue.

    return 0;
}
