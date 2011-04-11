// Final Project.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols

#include <cv.h>
#include <cxcore.h>
#include <cvaux.h>
#include "ml.h"
#include <cvwimage.h>
#include <cxmisc.h>
#include <highgui.h>

//define all of the possible modes
#define NOT_SIGNING			0x0
#define WAIT_MVMT			0x1
#define WAIT_STILL			0x2
#define CHECK_TEMPLATES		0x4
#define DISPLAY_RESULT		0x8
#define NO_RESULT			0x10

#define NUMDOTS_MVMT		3000

using namespace cv;
using namespace std;


// CFinalProjectApp:
// See Final Project.cpp for the implementation of this class
//

class CFinalProjectApp : public CWinApp
{
public:
	CFinalProjectApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CFinalProjectApp theApp;