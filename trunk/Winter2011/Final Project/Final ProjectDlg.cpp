// Final ProjectDlg.cpp : implementation file
//

#include "stdafx.h"
#include "Final Project.h"
#include "Final ProjectDlg.h"

#include <process.h>
#include <sstream>
#include <string>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//unsigned __stdcall getSign(void *ArgList);
char getSign();
int numWhitePixels(Mat img);

VideoCapture myCamera(-1);
Mat frame[2];
Mat templates[10];
Mat lastFrame;
Mat handFrame, handFrame_thresh;
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

int threshMin = 160;


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
	namedWindow("template 1");
	createTrackbar("Thresh Min: ", "template 1", &threshMin, 255);

	
	// Set timer for cam capture.
    SetTimer( ID_TIMER_CAPTURE, 30, 0 );
	//cameraThread = (HANDLE)_beginthreadex( NULL, 0, &captureFrames, NULL, 0, &cameraThreadID );
	doAcquisition = true;

	display.create(480, 640+handRegion.width, CV_8UC3);

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
		if(doAcquisition && haveImage){
						
			for(int i = 0; i < frame[0].rows; i++)
				for(int j = 0; j < frame[0].cols; j++)
					display.at<Point3_<uchar>>(i, j) = frame[0].at<Point3_<uchar>>(i,j);

			
			
			int col_off = frame[0].cols;
			int row_off = handFrame.rows;
			for(int i = 0; i < handFrame.rows; i++){
				for(int j = col_off; j < handFrame.cols+col_off; j++){
					int val = handFrame.at<uchar>(i,j-col_off);
					display.at<Point3_<uchar>>(i, j) = Point3_<uchar>(val, val, val);
					val = handFrame_thresh.at<uchar>(i,j-col_off);
					display.at<Point3_<uchar>>(i+row_off, j) = Point3_<uchar>(val, val, val);
					if(diffSmall.data)
						val = diffSmall.at<uchar>(i, j-col_off);
					display.at<Point3_<uchar>>(i+2*row_off, j) = Point3_<uchar>(val, val, val);
				}
			}

			rectangle(display, Point(frame[0].cols,-1), Point(display.cols+1,display.rows),Scalar(100, 200, 50), 2);
			rectangle(display, Point(frame[0].cols,handRegion.height), Point(display.cols+1,2*handRegion.height),Scalar(100, 200, 50), 2);
			
			Point handFrameTop(frame[0].cols, 0);
			Point threshTop(frame[0].cols, handRegion.height);
			Point diffTop(frame[0].cols, 2*handRegion.height);
			putText(display, "Hand Frame", Point(handFrameTop.x+5, handFrameTop.y+15), CV_FONT_HERSHEY_PLAIN, 1.0, Scalar(0,128,0), 2);
			putText(display, "Threshold", Point(threshTop.x+5, threshTop.y+13), CV_FONT_HERSHEY_PLAIN, .8, Scalar(0,128,0), 2);
			putText(display, "Hand Frame", Point(threshTop.x+5, threshTop.y+30), CV_FONT_HERSHEY_PLAIN, 1.0, Scalar(0,128,0), 2);
			if(diffSmall.data)
				putText(display, "Diff Image", Point(diffTop.x+5,diffTop.y+15), CV_FONT_HERSHEY_PLAIN, 1.0, Scalar(0,128,0), 2);
			

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

		if(lastFrame.data){
			absdiff(lastFrame, frame[1], diffBig);
			stringstream s;
			s <<  numWhitePixels(diffBig);
			resize(diffBig, diffSmall, Size(handRegion.width, handRegion.height));
			putText(diffSmall, s.str(), Point(diffSmall.cols-40,10), CV_FONT_HERSHEY_PLAIN, .6, Scalar(128, 128, 128));
		}

		Size imgSize = frame[0].size();
		int dtop = handRegion.y;
		int dbottom = imgSize.height - (dtop+handRegion.height);
		int dleft = handRegion.x;
		int dright = imgSize.width - (dleft+handRegion.width);


		frame[1].adjustROI(-dtop, -dbottom, -dleft, -dright);
		frame[1].copyTo(handFrame);
		frame[1].adjustROI(dtop, dbottom, dleft, dright);
		threshold(handFrame, handFrame_thresh, threshMin, 255, CV_THRESH_BINARY);		

		rectangle(frame[0], handRegion.tl(), handRegion.br(), Scalar(128, 255,0));
		Mat img = cv::imread("Templates\\1.jpg", CV_LOAD_IMAGE_GRAYSCALE);
		threshold(img, templates[1], threshMin, 255, CV_THRESH_BINARY);
		imshow("template 1", templates[1]);
		frame[1].copyTo(lastFrame);
		haveImage = true;
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
	}
	else if(myPassword.GetLength() != 0){
		CString s("Stop Signing");		//Start Signing was just clicked
		SetDlgItemText(IDC_STARTENTRY, s);
		signing = !signing;
	}
	else{
		CString s("Please enter a password before signing.");
		AfxMessageBox(s);
	}
}
Mat result, resultT;
//unsigned __stdcall getSign(void *ArgList)
char getSign()
{
	double minVal=0, maxVal=0;
	Point minLoc, maxLoc;
	//while(1){
	for(int i = 1; i < 2; i++){
		matchTemplate(frame[1], templates[i], result, CV_TM_CCORR_NORMED);
		minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc);
		norm(result);
		threshold(result, resultT, 250, 255, CV_THRESH_BINARY_INV);
		imshow("adf", result);
		imshow("adfT", resultT);
		waitKey(10);
//		if(resultT.at<uchar>(minLoc.y, minLoc.x) != 0)
//			AfxMessageBox((LPCTSTR)"Found template!");
	}
	//}
	processing = false;
	return 0;
}
void CFinalProjectDlg::OnBnClickedBtnSavetemplate()
{
	CString fileString;
	this->GetDlgItemText(IDC_FILENAME, fileString);
	CT2CA pszConvertedAnsiString (fileString);
	std::string nonCString (pszConvertedAnsiString);
	imwrite("Templates\\"+nonCString, handFrame);
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