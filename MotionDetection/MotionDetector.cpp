#include "MotionDetector.h"

#include <cv.h>
#include <cxcore.h>
#include <cvaux.hpp>
#include <cvwimage.h>
#include <highgui.h>



MotionDetection::ContourAreaMotionDetector::ContourAreaMotionDetector(void)
{
	ptrCurrentFrame = System::IntPtr(0);
	ptrGrayFrame = System::IntPtr(0);
	ptrAverageFrame = System::IntPtr(0);
	ptrAbsDiffFrame = System::IntPtr(0);
	ptrPreviousFrame = System::IntPtr(0);
	m_fSurfaceArea = 0.0f;
	m_ptrCurrentCountours = System::IntPtr(0);
	m_fThreshold = 8.0f;
	m_fLastMeasuredValue = 0.0f;
	m_nTriggerTime = 0.0f;
}


MotionDetection::ContourAreaMotionDetector::~ContourAreaMotionDetector(void)
{
	IplImage *curimage = (IplImage *) ptrCurrentFrame.ToPointer();
	if (curimage != NULL)
		cvReleaseImage( &curimage );  
	IplImage *imageAbsDiffFrame = (IplImage *) ptrAbsDiffFrame.ToPointer();
	if (imageAbsDiffFrame != NULL)
		cvReleaseImage( &imageAbsDiffFrame );  
	IplImage *imageGrayFrame = (IplImage *) ptrGrayFrame.ToPointer();
	if (imageGrayFrame != NULL)
		cvReleaseImage( &imageGrayFrame );  
	IplImage *imageAverageFrame = (IplImage *) ptrAverageFrame.ToPointer();
	if (imageAverageFrame != NULL)
		cvReleaseImage( &imageAverageFrame );  
	IplImage *imagePreviousFrame = (IplImage *) ptrPreviousFrame.ToPointer();
	if (imagePreviousFrame != NULL)
		cvReleaseImage( &imagePreviousFrame );  

	
	ptrCurrentFrame = System::IntPtr(0);
	ptrGrayFrame = System::IntPtr(0);
	ptrAverageFrame = System::IntPtr(0);
	ptrAbsDiffFrame = System::IntPtr(0);
	ptrPreviousFrame = System::IntPtr(0);
}


// Get thresholded image in HSV format  
IplImage* GetThresholdedImageHSV( IplImage* img )  
{  
    // Create an HSV format image from image passed  
    IplImage* imgHSV = cvCreateImage( cvGetSize( img ),   
                                      8,   
                                      3 );     
  
    cvCvtColor( img, imgHSV, CV_BGR2HSV );  
  
    // Create binary thresholded image acc. to max/min HSV ranges  
    // For detecting blue gloves in "MOV.MPG - HSV mode  
    IplImage* imgThresh = cvCreateImage( cvGetSize( img ),   
                                         8,   
                                         1 );             
  
    cvInRangeS( imgHSV,  
                cvScalar( 104, 178, 70  ),  
                cvScalar( 130, 240, 124 ),  
                imgThresh );  
  
    // Tidy up and return thresholded image  
    cvReleaseImage( &imgHSV );  
    return imgThresh;  
}  


bool MotionDetection::ContourAreaMotionDetector::Detect(array<unsigned char> ^bPixelData, int nWidth, int nHeight)
{
	if (bPixelData == nullptr)
		return false;
	if ( (nWidth == 0) || (nHeight == 0) )
		return false;

	bool bRet = false;

	IplImage *curimage = (IplImage *) ptrCurrentFrame.ToPointer();
	CvSize size = cvSize(nWidth, nHeight);
	
	if (curimage == NULL)
	{
		/// Create an image of or data
		curimage = cvCreateImage(size, IPL_DEPTH_8U, 4);
		ptrCurrentFrame = System::IntPtr(curimage);
	}

	uchar *pData = (uchar *) curimage->imageData;


	/// Copy our array to the image
	pin_ptr<unsigned char> ppPixelData = &bPixelData[0];
    unsigned char* pPixelData = (unsigned char *) ppPixelData;
	memcpy(pData, pPixelData, bPixelData->Length);

	/// Blur the image
	cvSmooth(curimage, curimage);

	IplImage *imageAbsDiffFrame = (IplImage *) ptrAbsDiffFrame.ToPointer();
	IplImage *imageGrayFrame = (IplImage *) ptrGrayFrame.ToPointer();
	IplImage *imageAverageFrame = (IplImage *) ptrAverageFrame.ToPointer();
	IplImage *imagePreviousFrame = (IplImage *) ptrPreviousFrame.ToPointer();
	if (imageAbsDiffFrame == nullptr)
	{

		m_fSurfaceArea = nWidth * nHeight;
 
		imageGrayFrame = cvCreateImage(size, IPL_DEPTH_8U, 1);
		ptrGrayFrame = System::IntPtr(imageGrayFrame);

		imageAverageFrame = cvCreateImage(size, IPL_DEPTH_32F, 4);
		ptrAverageFrame = System::IntPtr(imageAverageFrame);

		imageAbsDiffFrame = cvCloneImage(curimage);
		ptrAbsDiffFrame = System::IntPtr(imageAbsDiffFrame);

		imagePreviousFrame = cvCloneImage(curimage);
		ptrPreviousFrame = System::IntPtr(imagePreviousFrame);

		cvConvert(curimage, imageAverageFrame);
		ptrAverageFrame = System::IntPtr(imageAverageFrame);
	}
	else
	{
		cvRunningAvg(curimage, imageAverageFrame, 0.05);
	}
	cvConvert(imageAverageFrame, imagePreviousFrame);
	cvAbsDiff(curimage, imagePreviousFrame, imageAbsDiffFrame);

	cvCvtColor(imageAbsDiffFrame, imageGrayFrame, CV_RGB2GRAY);
	cvThreshold(imageGrayFrame, imageGrayFrame, 50, 255, CV_THRESH_BINARY);

	cvDilate(imageGrayFrame, imageGrayFrame, NULL, 15);
	cvErode(imageGrayFrame, imageGrayFrame, NULL, 10);

	CvMemStorage *storage = cvCreateMemStorage(0);
	CvSeq *countours = NULL;
	cvFindContours(imageGrayFrame, storage, &countours, 88, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE);

	m_ptrCurrentCountours = System::IntPtr(countours); // save countours

	m_fCurrentSurfaceArea = 0.0f;
	while (countours != NULL)
	{
		m_fCurrentSurfaceArea += cvContourArea(countours);
		countours = countours->h_next;
	}

	m_fLastMeasuredValue = (m_fCurrentSurfaceArea*100)/m_fSurfaceArea;
    m_fCurrentSurfaceArea = 0;
        
    if (m_fLastMeasuredValue > m_fThreshold)
       bRet = true;
    else
       bRet = false;


	return bRet;
}
