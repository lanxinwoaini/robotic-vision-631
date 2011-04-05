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

unsigned __stdcall getSign(void *ArgList);

VideoCapture myCamera(-1);
Mat frame[2];
Mat templates[10];
Mat lastFrame;
Mat handFrame;
CString myPassword;
bool doAcquisition = false;
bool haveImage = false;
const UINT ID_TIMER_CAPTURE = 0x1000;
bool signing = false;
bool debug = true;
bool processing = false;
HANDLE signThread=0;
Rect handRegion = Rect(150, 100, 175, 175);

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
	ON_BN_CLICKED(IDC_BTN_SETPASSWORD, &CFinalProjectDlg::OnBnClickedBtnSetPassword)
	ON_BN_CLICKED(IDC_SAVEIMG, &CFinalProjectDlg::OnBnClickedSaveimg)
	ON_BN_CLICKED(IDC_STARTENTRY, &CFinalProjectDlg::OnBnClickedStartentry)
	ON_BN_CLICKED(IDC_BTN_SAVETEMPLATE, &CFinalProjectDlg::OnBnClickedBtnSavetemplate)
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
    pheader->biWidth    = 640;
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
		if(doAcquisition && haveImage)
			::SetDIBitsToDevice(
			ImageDC->GetSafeHdc(), 0, 0,
			m_bitmapInfo.bmiHeader.biWidth, 
			::abs(m_bitmapInfo.bmiHeader.biHeight),
			0, 0, 0, 
			::abs(m_bitmapInfo.bmiHeader.biHeight),
			frame[0].data, 
			&m_bitmapInfo, DIB_RGB_COLORS);
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
		frame[1].adjustROI(-dtop, -dbottom, -dleft, -dright);
		frame[1].copyTo(handFrame);
		frame[1].adjustROI(dtop, dbottom, dleft, dright);
		threshold(handFrame, handFrame, threshMin, 255, CV_THRESH_BINARY);
		imshow("HAND", handFrame);
//		Mat diff;
//		absdiff(lastFrame, frame[1], diff);
		

		rectangle(frame[0], handRegion.tl(), handRegion.br(), Scalar(128, 255,0));
		Mat img = cv::imread("Templates\\1.jpg", CV_LOAD_IMAGE_GRAYSCALE);
		threshold(img, templates[1], threshMin, 255, CV_THRESH_BINARY);
		imshow("template 1", templates[1]);
		
		haveImage = true;
		this->InvalidateRect(DispRect, FALSE); // Initiate redrawing of Images for display
    }
}


void CFinalProjectDlg::OnBnClickedBtnSetPassword()
{
	this->GetDlgItemText(IDC_PASSWORDTEXT, myPassword);
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
	signing = !signing;
}
Mat result, resultT;
unsigned __stdcall getSign(void *ArgList)
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
