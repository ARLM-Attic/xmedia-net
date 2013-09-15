#include "OpenCVVideoSource.h"

using namespace System;
using namespace MotionDetection;

#include <cv.h>
#include <cxcore.h>
#include <cvaux.hpp>
#include <cvwimage.h>
#include <highgui.h>


OpenCVVideoSource::OpenCVVideoSource(int nSource)
{
	m_nMaxFrameRate = -1;
	ptrCvCapture = System::IntPtr(0);
	quit = true;
	m_listVideoFormats = gcnew System::Collections::Generic::List<VideoCaptureRate ^>();

	UniqueName = nSource.ToString();


	CvCapture *pCapture = cvCreateCameraCapture(nSource);
	ptrCvCapture = System::IntPtr(pCapture);
}

OpenCVVideoSource::~OpenCVVideoSource()
{
	CvCapture *pCapture = (CvCapture *) ptrCvCapture.ToPointer();
	if (pCapture != NULL)
	{
		cvReleaseCapture(&pCapture);
		ptrCvCapture = System::IntPtr(0);
	}
}


bool OpenCVVideoSource::Start(VideoCaptureRate ^videoformat)
{
	if (quit == false)
		return false;

   ActiveVideoFormat = videoformat;
   quit = false;
   CaptureThread = gcnew System::Threading::Thread(gcnew System::Threading::ThreadStart(this, &OpenCVVideoSource::OurCaptureThread));
   CaptureThread->Name = "Video Capture Thread";
   CaptureThread->IsBackground = true;
   CaptureThread->Start();

   return true;
}

void OpenCVVideoSource::Stop()
{
	quit = true;
}

void OpenCVVideoSource::OurCaptureThread()
{
	
	CvCapture *pCapture = (CvCapture *) ptrCvCapture.ToPointer();
	if (pCapture == NULL)
	{
		OnFailStartCapture("Capture failed: Failed to start streaming,  CvCapture device is null", this);
		return;
	}

	CvSize size = cvSize((int) cvGetCaptureProperty(pCapture, CV_CAP_PROP_FRAME_WIDTH),
		                 (int) cvGetCaptureProperty(pCapture, CV_CAP_PROP_FRAME_HEIGHT));
	  
	IplImage *pFrame32Bit = cvCreateImage(size, IPL_DEPTH_8U, 4);
	IplImage *pFrame = cvQueryFrame(pCapture);
	double fps = cvGetCaptureProperty(pCapture, CV_CAP_PROP_FPS);
	
	array<unsigned char> ^ arrayBytesRet = gcnew array<unsigned char>(size.height*size.width*4);
	pin_ptr<unsigned char> ppBytesRet = &arrayBytesRet[0];
	unsigned char *pBytesRet = (unsigned char *)ppBytesRet;

    while (!quit)
    {
    
        if (pFrame != NULL)
        {
			//DWORD dwBufferCount = 0;
			//pSample->GetBufferCount(&dwBufferCount);
			bool bSendFrame = true;

			/// Some clients can't handle our full frame rate, for these we'll throttle the
			/// events to the max framerate they ask for (well, interframe time anyways)
			if (m_nMaxFrameRate > 0)
			{
				TimeSpan tsDif = DateTime::Now - m_dtLastFrameSent;
				double fTsMaxFreq = 1.0f/m_nMaxFrameRate;
				if (tsDif.TotalMilliseconds < fTsMaxFreq)
					bSendFrame = false;
			}
		
			if (bSendFrame == true)
			{
				cvConvert(pFrame, pFrame32Bit);
				memcpy(pBytesRet, pFrame32Bit->imageData, arrayBytesRet->Length);
				OnNewFrame(arrayBytesRet, ActiveVideoFormat, this);
				m_dtLastFrameSent = DateTime::Now;
			}


        }

    	pFrame = cvQueryFrame(pCapture);
   
	}


}
