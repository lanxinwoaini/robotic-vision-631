#ifndef BITMAP_H
#define BITMAP_H


#include <iostream>
#include <string>
#include <fstream>
using namespace std;

void copydata(char *src, unsigned int *dest, int length);




typedef struct _WinBMPFileHeader
{
	unsigned int FileType;     /* File type, always 4D42h ("BM") */
	unsigned int FileSize;     /* Size of the file in bytes */
	unsigned int Reserved1;    /* Always 0 WORD */
	unsigned int Reserved2;    /* Always 0 WORD*/
	unsigned int BitmapOffset; /* Starting position of image data in bytes */
} WINBMPFILEHEADER;

typedef struct _WinNtBitmapHeader
{
	unsigned int  Size;            /* Size of this header in bytes */
	unsigned int  Width;           /* Image width in pixels */
	unsigned int  Height;          /* Image height in pixels */
	unsigned int  Planes;          /* Number of color planes */
	unsigned int  BitsPerPixel;    /* Number of bits per pixel */
	unsigned int  Compression;     /* Compression methods used */
	unsigned int  SizeOfBitmap;    /* Size of bitmap in bytes */
	unsigned int  HorzResolution;  /* Horizontal resolution in pixels per meter */
	unsigned int  VertResolution;  /* Vertical resolution in pixels per meter */
	unsigned int  ColorsUsed;      /* Number of colors in the image */
	unsigned int  ColorsImportant; /* Minimum number of important colors */
} WINNTBITMAPHEADER;

typedef struct Pixel
{
	char red;
	char green;
	char blue;

} PIXEL;

class knackBMP {
	WINBMPFILEHEADER fileheader;
	WINNTBITMAPHEADER bmpheader;
	char* data;
	string filename;
	ifstream fileP;
	ofstream outP;
	char * fname;
	unsigned int bytecount;
	bool validbmp;
	

	void copydata(char *src, unsigned int *dest, int length)
	{
		char * desttemp = (char *) dest;
		for(int i=0;i<length;i++)
		{
			*(desttemp+i) = *(src+i);
		}
		bytecount += length;
	}
	void clearheaders()
	{
		fileheader.FileType = 0;
		fileheader.FileSize = 0;
		fileheader.Reserved1 = 0;
		fileheader.Reserved2 = 0;
		fileheader.BitmapOffset = 0;
		
		bmpheader.Size = 0;
		bmpheader.Width = 0;
		bmpheader.Height = 0;
		bmpheader.Planes = 0;
		bmpheader.BitsPerPixel = 0;
		bmpheader.Compression = 0;
		bmpheader.SizeOfBitmap = 0;
		bmpheader.HorzResolution = 0;
		bmpheader.VertResolution = 0;
		bmpheader.ColorsUsed = 0;
		bmpheader.ColorsImportant = 0;		
	}

public:
	PIXEL pixels[480][640];							//2D array of pixels.
	knackBMP()
	{
		filename = "";
		bytecount = 0;
		validbmp = false;
		clearheaders();
		

	}
	knackBMP(string file)
	{
		filename = file;
		bytecount = 0;
		validbmp = false;
		clearheaders();
		init_BMP();
			
	}
	void setname(string file)
	{
		filename = file;
	}

	int init_BMP(void)
	{
		char temp[5];
		char *filedata;

		if(filename.empty()){
			cout<<"Use a file name\n";	
			return -1;
		}
		fname = new char[filename.length() + 1];
		strcpy(fname, filename.c_str());
		fileP.open(fname, fstream::in | fstream::binary );
		if(!fileP){
			cerr<<"There was an error opening the file";
			return -1;
		}
		filedata = temp;
		fileP.get(filedata, 3);
		copydata(filedata, &fileheader.FileType, 2);
		fileP.get(filedata, 5);
		copydata(filedata, &fileheader.FileSize, 4);
		fileP.get(filedata, 3);
		copydata(filedata, &fileheader.Reserved1, 2);
		fileP.get(filedata, 3);
		copydata(filedata, &fileheader.Reserved2, 2);
		fileP.get(filedata, 5);
		copydata(filedata, &fileheader.BitmapOffset, 4);
		fileP.get(filedata, 5);
		copydata(filedata, &bmpheader.Size, 4);
		fileP.get(filedata, 5);
		copydata(filedata, &bmpheader.Width, 4);
		fileP.get(filedata, 5);
		copydata(filedata, &bmpheader.Height, 4);
		fileP.get(filedata, 3);
		copydata(filedata, &bmpheader.Planes, 2);
		fileP.get(filedata, 3);
		copydata(filedata, &bmpheader.BitsPerPixel, 2);
		fileP.get(filedata, 5);
		copydata(filedata, &bmpheader.Compression, 4);
		fileP.get(filedata, 5);
		copydata(filedata, &bmpheader.SizeOfBitmap, 4);
		fileP.get(filedata, 5);
		copydata(filedata, &bmpheader.HorzResolution, 4);
		fileP.get(filedata, 5);
		copydata(filedata, &bmpheader.VertResolution, 4);
		fileP.get(filedata, 5);
		copydata(filedata, &bmpheader.ColorsUsed, 4);
		fileP.get(filedata, 5);
		copydata(filedata, &bmpheader.ColorsImportant, 4);

		
		unsigned int width = 0;
		unsigned int height = 0;

		while(fileP.read(filedata, 3))
		{
			pixels[height][width].red = filedata[0];
			pixels[height][width].green = filedata[1];
			pixels[height][width].blue = filedata[2];
			width++;
			if (width >= bmpheader.Width)
			{
				width = 0;
				height++;
			}	
		}

		if(!fileP.eof())
		{
			cerr << "There is still data in the file!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" << endl;
			return -1;
		}	
		validbmp = true;
		return 0;	
	}

	int makebmp(string filename)
	{
		char * outname;
		if(!validbmp)
		{
			cout << "Not a valid bmp to output.  Please run initbmp()" << endl;
		}
		//create a file
		outname = new char[filename.length() + 1];
		strcpy(outname, filename.c_str());
		outP.open(outname, fstream::out | fstream::binary );
		if(!outP){
			cerr<<"There was an error opening the file for writing";
			return -1;
		}

		outP.write((const char *) &fileheader.FileType, 2);
		
		outP.write((const char *) &fileheader.FileSize, 4);
		outP.write((const char *) &fileheader.Reserved1, 2);
		outP.write((const char *) &fileheader.Reserved2, 2);
		outP.write((const char *) &fileheader.BitmapOffset, 4);
		outP.write((const char *) &bmpheader.Size, 4);
		outP.write((const char *) &bmpheader.Width, 4);
		outP.write((const char *) &bmpheader.Height, 4);
		outP.write((const char *) &bmpheader.Planes, 2);
		outP.write((const char *) &bmpheader.BitsPerPixel, 2);
		outP.write((const char *) &bmpheader.Compression, 4);
		outP.write((const char *) &bmpheader.SizeOfBitmap, 4);
		outP.write((const char *) &bmpheader.HorzResolution, 4);
		outP.write((const char *) &bmpheader.VertResolution, 4);
		outP.write((const char *) &bmpheader.ColorsUsed, 4);
		outP.write((const char *) &bmpheader.ColorsImportant, 4);


		unsigned int width = 0;
		unsigned int height = 0;

		for(height = 0; height < bmpheader.Height; height++)
		{
			for(width = 0; width < bmpheader.Width; width++)
			{
			outP.write((const char*) &pixels[height][width].red, 1);
			outP.write((const char*) &pixels[height][width].green, 1);
			outP.write((const char*) &pixels[height][width].blue, 1);
//			pixels[height][width].red = filedata[0];
//			pixels[height][width].green = filedata[1];
//			pixels[height][width].blue = filedata[2];
			}
		}

		outP.close();

		

		


		




	}
};





#endif