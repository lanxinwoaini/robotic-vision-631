// Final ProjectDlg.h : header file
//

#pragma once


// CFinalProjectDlg dialog
class CFinalProjectDlg : public CDialog
{
// Construction
public:
	CFinalProjectDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_FINALPROJECT_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;
	BITMAPINFO  m_bitmapInfo;
	CDC*	ImageDC;
	CWnd*	ParentWnd;
	RECT	ImageRect;
	CRect	DispRect;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	void OnTimer( UINT nIDEvent );
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedSaveimg();
	afx_msg void OnBnClickedStartentry();
	afx_msg void OnBnClickedBtnSavetemplate();
	afx_msg void OnNMCustomdrawThresholdslider(NMHDR *pNMHDR, LRESULT *pResult);
};
