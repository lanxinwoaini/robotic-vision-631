#include <string>
#include <iostream>
#include <fstream>

#include "bitmap.h"
#include "signature.h"
#include "cv.h"
#include "highgui.h"

#define SHOWIMAGES 1


IplImage* src = 0;
IplImage* image = 0;
IplImage* dest = 0;
IplImage* dest1 = 0;
IplImage* dest2 = 0;

IplImage* orangeresult, *orangeresult2, *blueresult, *greenresult;

CvSize sz; 
CvScalar rangemin = cvScalar(0,0,0);
CvScalar rangemax = cvScalar(256,256,256);

int hmin,  hmax, smin, smax, vmin, vmax;
int ohmin, ohmax, osmin, osmax, ovmin, ovmax, ghmin, ghmax, gsmin, gsmax, gvmin, gvmax, bhmin, bhmax, bsmin, bsmax, bvmin, bvmax;
int orangecount[640]; int greencount[640]; int bluecount[640]; //used to count nonzero column pixels
int vertocount[480]; int vertgcount[480]; int vertbcount[480];


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
		cvOr(dest1,dest2,dest);
	}
}
void setInRangeS(IplImage * src, IplImage *dest_1, IplImage * dest_2){
	if (rangemin.val[0] <= rangemax.val[0]){
		cvInRangeS(image, rangemin, rangemax, dest_1);			//find pixels in range
	} else {
		CvScalar Min1 = CvScalar(rangemin); Min1.val[0] = 0;				//from 0
		CvScalar Max1 = CvScalar(rangemax); Max1.val[0] = rangemax.val[0];	//to rangemax.val[0]
		CvScalar Min2 = CvScalar(rangemin); Min2.val[0] = rangemin.val[0];	//from rangemin.val[0]
		CvScalar Max2 = CvScalar(rangemax); Max2.val[0] = 255;				//to 255
		cvInRangeS(src, Min1, Max1, dest_1);			//find pixels in range
		cvInRangeS(src, Min2, Max2, dest_2);			//find pixels in range
		cvOr(dest_1,dest_2,dest_1);
	}
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
void filter(int * countarray, int max, float percent, int width)
{
	int sub = (int)(float(max) * percent /2);
	for(int i=0; i< width; i++)
	{
		countarray[i] -= sub;
		if(countarray[i] <0)
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


int main( int argc, char** argv )
{
		init_signatures();

		unsigned int signature =0;

		
		if(argc !=2)
		{
			cout << "invalid parameter.  Specify a single image filename." << endl;
			return -1;
		}
	
		

					src = cvLoadImage(argv[1], 1);
					if(!src)
					{
						cout <<"File does not exist. " << endl;
						return -1;		//if the image doesn't exist, skips it.
					}

						sz.height = src->height;
						sz.width = src->width;

				image = cvCreateImage(sz, IPL_DEPTH_8U,3);						//create image to store HSV, convert to HSV
						cvCvtColor(src, image, CV_BGR2HSV);

						orangeresult = cvCreateImage(sz, IPL_DEPTH_8U,1);
						orangeresult2 = cvCreateImage(sz, IPL_DEPTH_8U,1);
						greenresult = cvCreateImage(sz, IPL_DEPTH_8U,1);
						blueresult = cvCreateImage(sz, IPL_DEPTH_8U,1);

						for(int k =0; k < sz.width; k++)
						{
							signature += (unsigned char)*(src->imageData + k*13);
						}

				int whichsig = findSigIndex(signature);
				if(whichsig !=-1)
				{
						//we found the signature, we already have the proper range for this image.  Let's use the ranges to find the pylons.
					
					

					//read the proper ranges for all three colors.
					ohmin = orangetable[whichsig].hmin;
					ohmax = orangetable[whichsig].hmax;
					osmin = orangetable[whichsig].smin;
					osmax = orangetable[whichsig].smax;
					ovmin = orangetable[whichsig].vmin;
					ovmax = orangetable[whichsig].vmax;

					ghmin = greentable[whichsig].hmin;
					ghmax = greentable[whichsig].hmax;
					gsmin = greentable[whichsig].smin;
					gsmax = greentable[whichsig].smax;
					gvmin = greentable[whichsig].vmin;
					gvmax = greentable[whichsig].vmax;

					bhmin = bluetable[whichsig].hmin;
					bhmax = bluetable[whichsig].hmax;
					bsmin = bluetable[whichsig].smin;
					bsmax = bluetable[whichsig].smax;
					bvmin = bluetable[whichsig].vmin;
					bvmax = bluetable[whichsig].vmax;

					rangemin.val[0] = (double)ohmin; 
					rangemin.val[1] = (double)osmin; 
					rangemin.val[2] = (double)ovmin;
					rangemax.val[0] = (double)ohmax; 
					rangemax.val[1] = (double)osmax; 
					rangemax.val[2] = (double)ovmax;
					setInRangeS(image,orangeresult,orangeresult2); //now orange1 holds the orange image.

					rangemin.val[0] = (double)ghmin;
					rangemin.val[1] = (double)gsmin;
					rangemin.val[2] = (double)gvmin;
					rangemax.val[0] = (double)ghmax; 
					rangemax.val[1] = (double)gsmax; 
					rangemax.val[2] = (double)gvmax;
					setInRangeS(image, greenresult, orangeresult2);

					rangemin.val[0] = (double)bhmin;
					rangemin.val[1] = (double)bsmin;
					rangemin.val[2] = (double)bvmin;
					rangemax.val[0] = (double)bhmax; 
					rangemax.val[1] = (double)bsmax; 
					rangemax.val[2] = (double)bvmax;
					setInRangeS(image, blueresult, orangeresult2);

				}// if we found the signature
				else
				{ //orange: 140	7	74	212	178	256


					rangemin.val[0] =  140; 
					rangemin.val[1] =  74; 
					rangemin.val[2] =  178;
					rangemax.val[0] =  7; 
					rangemax.val[1] =  212; 
					rangemax.val[2] =  256;
					setInRangeS(image,orangeresult,orangeresult2); //now orange1 holds the orange image.

					//green: 30	65	46	156	126	256
					rangemin.val[0] =  30;
					rangemin.val[1] =  46;
					rangemin.val[2] =  126;
					rangemax.val[0] =  65; 
					rangemax.val[1] =  156; 
					rangemax.val[2] =  256;
					setInRangeS(image, greenresult, orangeresult2);

					//blue   75	154	101	230	135	256


					rangemin.val[0] =  75;
					rangemin.val[1] =  101;
					rangemin.val[2] =  135;
					rangemax.val[0] =  154; 
					rangemax.val[1] =  230; 
					rangemax.val[2] =  256;
					setInRangeS(image, blueresult, orangeresult2);

				}

					countcolumnbits(orangeresult, orangecount, image->width, image->height);
					countcolumnbits(greenresult, greencount, image->width, image->height);
					countcolumnbits(blueresult, bluecount, image->width, image->height);

					int orangeindex = findmax(orangecount, image->width); //find the column with the most pixels in it.  returns -1 if all black.
					int greenindex = findmax(greencount, image->width);
					int blueindex = findmax(bluecount, image->width);

					float nonzorange = count(orangecount, image->width);
					float nonzgreen = count(greencount, image->width);
					float nonzblue = count(bluecount, image->width);


					int orangemax = orangecount[orangeindex];
					int greenmax = greencount[greenindex];
					int bluemax = bluecount[blueindex];

					//filter out pixels based on how many columns are nonzero, and the maximum value.
					filter(orangecount, orangemax, nonzorange, image->width);
					filter(greencount, greenmax, nonzgreen,  image->width);
					filter(bluecount, bluemax, nonzblue,  image->width);

					int orangeright, orangeleft, greenright, greenleft, blueright, blueleft;
					int orangecenter = findcenter(orangecount, orangeindex, orangeright, orangeleft, image->width);
					int greencenter = findcenter(greencount, greenindex, greenright, greenleft, image->width);
					int bluecenter = findcenter(bluecount, blueindex, blueright, blueleft, image->width);

					countrowbits(orangeresult, vertocount, image->height, orangeleft, orangeright);
					countrowbits(greenresult, vertgcount, image->height, greenleft, greenright);
					countrowbits(blueresult, vertbcount, image->height, blueleft, blueright);

					int vertoindex = findmax(vertocount, image->height);
					int vertgindex = findmax(vertgcount, image->height);
					int vertbindex = findmax(vertbcount, image->height);

					float vertnzorange = count(vertocount, image->height);
					float vertnzgreen = count(vertgcount, image->height);
					float vertnzblue = count(vertbcount, image->height);

					//filter(vertocount, vertocount[vertoindex], vertnzorange, image->width); //**don't filter in the vertical direction
					//filter(vertgcount, vertgcount[vertgindex], vertnzgreen,  image->width);
					//filter(vertbcount, vertbcount[vertbindex], vertnzblue,  image->width);

					int orangetop, orangebottom, greentop, greenbottom, bluetop, bluebottom; //blue bottom.  funny.
					int orangemiddle = findcenter(vertocount, vertoindex, orangetop, orangebottom, image->height);
					int greenmiddle =  findcenter(vertgcount, vertgindex, greentop,  greenbottom,  image->height);
					int bluemiddle =   findcenter(vertbcount, vertbindex, bluetop,   bluebottom,   image->height);

					if (orangeindex != -1) //there is an orange pylon.
					{
						
						cout<< "orange\t" << "(" << orangecenter << ", " << orangebottom << ")\t" << orangetop-orangebottom << "\t" << orangeright - orangeleft << "\t" << "??" << endl;
					}
					if(greenindex != -1)//there is a green pylon
					{
						cout<< "green\t" << "(" << greencenter << ", " << greenbottom << ")\t" << greentop-greenbottom << "\t" << greenright - greenleft << "\t" << "??" << endl;
					}
					if(blueindex != -1)//there is a blue pylon
					{
						cout<< "blue\t" << "(" << bluecenter << ", " << bluebottom << ")\t" << bluetop-bluebottom << "\t" << blueright - blueleft << "\t" << "??" << endl;
					}
					

				
		
    return 0;
}



