// Final ProjectDlg.cpp : implementation file
//

//TODO
/*
	-debug and demo modes:
		-for demo: none of the debug windows
	-save and load last known threshold value (save with button, automatically load it)
*/

#include "stdafx.h"
#include "Final Project.h"
#include "Final ProjectDlg.h"

#include <process.h>
#include <sstream>
#include <string>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define TEMPLATES_IN_SET 34
char templateNames[TEMPLATES_IN_SET] = 
	{'0','1','2','3','4','5','6',
	'7','8','9','a','b','c','d',
	'e','f','g','h','i','k','l',
	'm','n','o','p','q','r','s',
	't','u','v','w','x','y'};


//unsigned __stdcall getSign(void *ArgList);
extern string modeString;
extern string currentEntry;
extern unsigned MODE;
extern void handleFrame();
char getSign();
unsigned diffDots = 0;
int numWhitePixels(Mat img);

VideoCapture myCamera(-1);
Mat frame[2];
Mat templates[10];
Mat lastHandFrame, handFrame_color, handFrame_resized, handFrame_bw;
Mat lastTemplateFrame, templateFrame_color, templateFrame_bw, templateFrame_thresh;
Mat diffBig, diffSmall;
CString myPassword;
bool doAcquisition = false;
bool haveImage = false;
const UINT ID_TIMER_CAPTURE = 0x1000;
bool signing = false;
bool debug = true;
bool processing = false;
HANDLE signThread=0;
Rect handRegion = Rect(50, 50, 160, 160);
Mat display;
Mat result, resultT;

int threshMin = 130;


// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()


// CFinalProjectDlg dialog




CFinalProjectDlg::CFinalProjectDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CFinalProjectDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CFinalProjectDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CFinalProjectDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_TIMER()
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_SAVEIMG, &CFinalProjectDlg::OnBnClickedSaveimg)
	ON_BN_CLICKED(IDC_STARTENTRY, &CFinalProjectDlg::OnBnClickedStartentry)
	ON_BN_CLICKED(IDC_BTN_SAVETEMPLATE, &CFinalProjectDlg::OnBnClickedBtnSavetemplate)
	ON_NOTIFY(NM_CUSTOMDRAW, IDC_THRESHOLDSLIDER, &CFinalProjectDlg::OnNMCustomdrawThresholdslider)
END_MESSAGE_MAP()


// CFinalProjectDlg message handlers

BOOL CFinalProjectDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString strAboutMenu;
		strAboutMenu.LoadString(IDS_ABOUTBOX);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	CSliderCtrl *c = (CSliderCtrl *)GetDlgItem(IDC_THRESHOLDSLIDER);
	c->SetRange(0, 255);
	c->SetTicFreq(10);
	c->SetPos(threshMin);

	// TODO: Add extra initialization here
	//init the image, 801-2401699
	WINDOWPLACEMENT lpwndpl;
	GetWindowPlacement(&lpwndpl);
	SetWindowPlacement(&lpwndpl);
	ImageDC = GetDlgItem(IDC_CAPTURE)->GetDC();
	ParentWnd = GetDlgItem(IDC_CAPTURE);
	ParentWnd->GetWindowPlacement(&lpwndpl);
	ImageRect = lpwndpl.rcNormalPosition;
	ParentWnd->GetWindowRect(DispRect);
   BITMAPINFOHEADER* pheader = &m_bitmapInfo.bmiHeader;
   //
   // Initialize permanent data in the bitmapinfo header.
   //
    pheader->biSize          = sizeof( BITMAPINFOHEADER );
    pheader->biPlanes        = 1;
    pheader->biCompression   = BI_RGB;
    pheader->biXPelsPerMeter = 120;
    pheader->biYPelsPerMeter = 120;
    pheader->biClrUsed       = 0;
    pheader->biClrImportant  = 0;
    //
    // Set a default window size.
    // 
	pheader->biWidth    = 640+handRegion.width;
    pheader->biHeight   = -480;
    pheader->biBitCount = 24;
   
    m_bitmapInfo.bmiHeader.biSizeImage = 
      pheader->biWidth * pheader->biHeight * ( pheader->biBitCount / 8 );


	std::string root = "Templates\\";
	std::string jpg = ".jpg";
	for(int i = 0; i < 10; i++){
		std::stringstream s;
		s << i;
		templates[i] = cv::imread(root+s.str()+jpg, CV_LOAD_IMAGE_GRAYSCALE);
	}
	
	// Set timer for cam capture.
    SetTimer( ID_TIMER_CAPTURE, 30, 0 );
	//cameraThread = (HANDLE)_beginthreadex( NULL, 0, &captureFrames, NULL, 0, &cameraThreadID );
	doAcquisition = true;

	display.create(480, 640+handRegion.width, CV_8UC3);
	
	//check the camera to see if it's available
	if(!myCamera.isOpened()){
		CString msg("Camera did not initialize correctly! Please ensure that the camera is attached.");
//		AfxMessageBox(msg);
//		exit(1);
	}

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CFinalProjectDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CFinalProjectDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		//just in case we try to paint before we acquired an image;
		if(doAcquisition && haveImage){
						
			for(int i = 0; i < frame[0].rows; i++)
				for(int j = 0; j < frame[0].cols; j++)
					display.at<Point3_<uchar>>(i, j) = frame[0].at<Point3_<uchar>>(i,j);			
			
			int col_off = frame[0].cols;
			int row_off = handFrame_resized.rows;
			for(int i = 0; i < handFrame_resized.rows; i++){
				for(int j = col_off; j < handFrame_resized.cols+col_off; j++){
					int val=0;
					Point3_<uchar> cp = handFrame_resized.at<Point3_<uchar>>(i,j-col_off);
					display.at<Point3_<uchar>>(i, j) = cp;
					cp = templateFrame_color.at<Point3_<uchar>>(i,j-col_off);
					display.at<Point3_<uchar>>(i+row_off, j) = cp;
					if(diffSmall.data)
						val = diffSmall.at<uchar>(i, j-col_off);
					display.at<Point3_<uchar>>(i+2*row_off, j) = Point3_<uchar>(val, val, val);
				}
			}

			Point regionPoints[2];
			regionPoints[0] = handRegion.tl();
			regionPoints[1] = handRegion.br();
			rectangle(display, Point(frame[0].cols,-1), Point(display.cols+1,display.rows),Scalar(100, 200, 50), 2);
			rectangle(display, Point(frame[0].cols,handRegion.height), Point(display.cols+1,2*handRegion.height),Scalar(100, 200, 50), 2);
			rectangle(display, regionPoints[0], regionPoints[1], Scalar(128, 255,0));
			rectangle(display, Point(regionPoints[0].x-20, regionPoints[0].y-20),
								Point(regionPoints[1].x+20, regionPoints[1].y+20), Scalar(255, 100, 0));
			
			Point handFrameTop(frame[0].cols, 0);
			Point threshTop(frame[0].cols, handRegion.height);
			Point diffTop(frame[0].cols, 2*handRegion.height);
			putText(display, "Hand Frame", Point(handFrameTop.x+5, handFrameTop.y+20), CV_FONT_HERSHEY_PLAIN, 1.2, Scalar(0,128,0), 2);
			putText(display, "Template", Point(threshTop.x+5, threshTop.y+18), CV_FONT_HERSHEY_PLAIN, 1.2, Scalar(0,128,0), 2);
			if(diffSmall.data){
				putText(display, "Diff Image", Point(diffTop.x+5,diffTop.y+20), CV_FONT_HERSHEY_PLAIN, 1.2, Scalar(0,128,0), 2);
				stringstream s;
				s << diffDots;
				putText(display, s.str(), Point(diffTop.x+diffSmall.cols-40,diffTop.y+10), CV_FONT_HERSHEY_PLAIN, .6, Scalar(128, 128, 128));
			}

			if(MODE != NOT_SIGNING){
				putText(display, modeString, Point(5, 25), CV_FONT_HERSHEY_PLAIN, 1.5, Scalar(255,200,100), 2);
				Point charPoint(20, 460);
				CT2CA pszConvertedAnsiString (myPassword);
				std::string nonCString (pszConvertedAnsiString);
				putText(display, nonCString, Point(charPoint.x, charPoint.y - 70), CV_FONT_HERSHEY_TRIPLEX, 1.8, Scalar(100,255,100), 2);
				putText(display, currentEntry, charPoint, CV_FONT_HERSHEY_TRIPLEX, 2.0, Scalar(25,25,255), 2);
				charPoint.x += 40*currentEntry.length()-10;
				putText(display, "_", charPoint, CV_FONT_HERSHEY_TRIPLEX, 2.0, Scalar(0,0,255), 2);
				charPoint.x += 40;
				string toGo = "";
				for(int i = 1; i < myPassword.GetLength() - currentEntry.length(); i++)
					toGo += "_";
				putText(display, toGo, charPoint, CV_FONT_HERSHEY_TRIPLEX, 2.0, Scalar(0,255,0), 2);
			}

			::SetDIBitsToDevice(
			ImageDC->GetSafeHdc(), 0, 0,
			m_bitmapInfo.bmiHeader.biWidth, 
			::abs(m_bitmapInfo.bmiHeader.biHeight),
			0, 0, 0, 
			::abs(m_bitmapInfo.bmiHeader.biHeight),
			display.data, 
			&m_bitmapInfo, DIB_RGB_COLORS);
		}
		CDialog::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CFinalProjectDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

// Timer Handler.
void CFinalProjectDlg::OnTimer( UINT nIDEvent )
{
    // Per 30 milliseconds
    if( nIDEvent == ID_TIMER_CAPTURE )
    {
		
		myCamera >> frame[0];
		cvtColor(frame[0], frame[1], CV_RGB2GRAY);

		Size imgSize = frame[0].size();
		int dtop = handRegion.y;
		int dbottom = imgSize.height - (dtop+handRegion.height);
		int dleft = handRegion.x;
		int dright = imgSize.width - (dleft+handRegion.width);

		frame[0].adjustROI(-dtop, -dbottom, -dleft, -dright);
		frame[0].copyTo(templateFrame_color);
		frame[0].adjustROI(dtop, dbottom, dleft, dright);

		frame[1].adjustROI(-dtop, -dbottom, -dleft, -dright);
		frame[1].copyTo(templateFrame_bw);
		frame[1].adjustROI(dtop, dbottom, dleft, dright);

		frame[0].adjustROI(-dtop+20, -dbottom+20, -dleft+20, -dright+20);
		frame[0].copyTo(handFrame_color);
		frame[0].adjustROI(dtop-20, dbottom-20, dleft-20, dright-20);

		frame[1].adjustROI(-dtop+20, -dbottom+20, -dleft+20, -dright+20);
		frame[1].copyTo(handFrame_bw);
		frame[1].adjustROI(dtop-20, dbottom-20, dleft-20, dright-20);

		//for drawing what we see in the bigger hand frame
		resize(handFrame_color, handFrame_resized, templateFrame_bw.size());
		
		threshold(templateFrame_bw, templateFrame_thresh, threshMin, 255, CV_THRESH_BINARY);

		int numDots = 0;
		if(lastHandFrame.data){
			absdiff(lastHandFrame, handFrame_bw, diffBig);
			diffDots = numWhitePixels(diffBig);
			resize(diffBig, diffSmall, handFrame_resized.size());
		}	
		
		handFrame_bw.copyTo(lastHandFrame);
		haveImage = true;

		//depending on what state we're in this frame, handle the state that we're in
		handleFrame();
		this->InvalidateRect(DispRect, FALSE); // Initiate redrawing of Images for display
    }
}

void CFinalProjectDlg::OnBnClickedSaveimg()
{
	CString fileString;
	this->GetDlgItemText(IDC_FILENAME, fileString);
	CT2CA pszConvertedAnsiString (fileString);
	std::string nonCString (pszConvertedAnsiString);
	cv::imwrite("Saved Images\\"+nonCString, frame[0]);
}

void CFinalProjectDlg::OnBnClickedStartentry()
{
	this->GetDlgItemText(IDC_PASSWORDTEXT, myPassword);
	if(signing){
		CString s("Start Signing");		//Stop Signing was just clicked
		SetDlgItemText(IDC_STARTENTRY, s);
		signing = !signing;
		MODE = NOT_SIGNING;
	}
	else if(myPassword.GetLength() != 0){
		CString s("Stop Signing");		//Start Signing was just clicked
		SetDlgItemText(IDC_STARTENTRY, s);
		signing = !signing;
		MODE = WAIT_MVMT;
	}
	else{
		CString s("Please enter a password before signing.");
		AfxMessageBox(s);
	}
}

//unsigned __stdcall getSign(void *ArgList)
char getSign()
{
	double minVal=0, maxVal=0;
	Point minLoc, maxLoc;
	int best_template = -1;
	bool find_min = true;
	double best_value = find_min ? 1e10 : 0;
	
	//while(1){
	for(int i = 0; i < TEMPLATES_IN_SET; i++){
		matchTemplate(frame[1], templates[i], result, CV_TM_SQDIFF_NORMED);
		minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc);
	
		if((find_min && minVal < best_value) || (!find_min && maxVal > best_value))
		{
			best_value = (find_min ? minVal : maxVal);
			best_template = i;
		}
//		if(resultT.at<uchar>(minLoc.y, minLoc.x) != 0)
//			AfxMessageBox((LPCTSTR)"Found template!");
	}
	
	//assign char
	char result = templateNames[best_template];

	//}
	processing = false;
	return result;
}
void CFinalProjectDlg::OnBnClickedBtnSavetemplate()
{
	CString fileString;
	this->GetDlgItemText(IDC_FILENAME, fileString);
	CT2CA pszConvertedAnsiString (fileString);
	std::string nonCString (pszConvertedAnsiString);
	imwrite("Templates\\"+nonCString, templateFrame_color);
}

void CFinalProjectDlg::OnNMCustomdrawThresholdslider(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMCUSTOMDRAW pNMCD = reinterpret_cast<LPNMCUSTOMDRAW>(pNMHDR);
	CSliderCtrl *c = (CSliderCtrl*)GetDlgItem(IDC_THRESHOLDSLIDER);
	threshMin = c->GetPos();
	CWnd *t = GetDlgItem(IDC_THRESHDISPLAY);
	stringstream s;
	s << threshMin;
	CString num(s.str().c_str());
	t->SetWindowText((LPCTSTR)num);
	*pResult = 0;
}

//counts the number of all non-black pixels
int numWhitePixels(Mat img)
{
	int cnt = 0;
	//type is assumed to be CV_8U
	for(int i = 0; i < img.rows; i++){
		for(int j = 0; j < img.cols; j++){
			cnt += img.at<uchar>(i,j) > 10;
		}
	}
	return cnt;
}