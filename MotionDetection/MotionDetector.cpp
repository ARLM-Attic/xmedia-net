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
	ptrMask = System::IntPtr(0);
	m_fSurfaceArea = 0.0f;
	m_ptrCurrentCountours = System::IntPtr(0);
	m_fThreshold = 8.0f;
	m_fLastMeasuredValue = 0.0f;
	m_nTriggerTime = 0.0f;
	m_fContourAreaThreshold = 0.0f;
	m_fLastMaxContourAreaDetected = 0.0f;
	m_strFileNameMotionMask = nullptr;
	m_bShowText = true;
	cvUseOptimized(1);
	m_dtLastFrameProcessed = DateTime(System::DateTime::MinValue);
	m_nMaxAnalyzedFramesPerSecond = 0;
	m_bActive = true;
	m_bMotionActive = false;
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
	IplImage *imageTemp = (IplImage *) ptrTemp.ToPointer();
	if (imageTemp != NULL)
		cvReleaseImage( &imageTemp );  
	IplImage *imageMask = (IplImage *) ptrMask.ToPointer();
	if (imageMask != NULL)
		cvReleaseImage( &imageMask );  


	
	ptrCurrentFrame = System::IntPtr(0);
	ptrGrayFrame = System::IntPtr(0);
	ptrAverageFrame = System::IntPtr(0);
	ptrAbsDiffFrame = System::IntPtr(0);
	ptrPreviousFrame = System::IntPtr(0);
	ptrTemp = System::IntPtr(0);
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

/// Mask the image...  Source must be a 4 channel 8-bit image, Mask must be a 1 channel 8 bit image
int MaskImage(IplImage *imgSource, IplImage *imgMask)
{
	int nWhitePixels = 0;
	//unsigned char *pSrcA = (unsigned char *) imgSource->imageData;
	unsigned int *pSrcA = (unsigned int *) imgSource->imageData;
	unsigned char *pMask = (unsigned char *) imgMask->imageData;
	for (int y=0; y<imgSource->height; y++)
	{
		for (int x=0; x<imgSource->width; x++)
		{
 		    if ((*pMask) == 0)
				*pSrcA = 0;
			else
				nWhitePixels ++;
			pSrcA++;
			pMask++;
		}
	}
	return nWhitePixels;
}

bool MotionDetection::ContourAreaMotionDetector::Detect(array<unsigned char> ^%bPixelData, int nWidth, int nHeight, bool bRetMotion)
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

	/// See if we should do motion detection on this frame, may throttle for performance reasons
	bool bShouldAnalyze = m_bActive;
	if ( (MaxAnalyzedFramesPerSecond > 0) && (bShouldAnalyze == true) )
	{
		System::TimeSpan tsElapsed = DateTime::Now - m_dtLastFrameProcessed;
		double fMs = tsElapsed.TotalSeconds;
		if (fMs > 0)
		{
			double fFPS = 1/fMs;
			if (fFPS >MaxAnalyzedFramesPerSecond)
				bShouldAnalyze = false;
		}
		else
			bShouldAnalyze = false;
	}

	/// Copy our array to the image
	pin_ptr<unsigned char> ppPixelData = &bPixelData[0];
    unsigned char* pPixelData = (unsigned char *) ppPixelData;
	memcpy(pData, pPixelData, bPixelData->Length);

	/// Blur the image
	if (bShouldAnalyze == true)
		cvSmooth(curimage, curimage);

	IplImage *imageAbsDiffFrame = (IplImage *) ptrAbsDiffFrame.ToPointer();
	IplImage *imageGrayFrame = (IplImage *) ptrGrayFrame.ToPointer();
	IplImage *imageAverageFrame = (IplImage *) ptrAverageFrame.ToPointer();
	IplImage *imagePreviousFrame = (IplImage *) ptrPreviousFrame.ToPointer();
	IplImage *imageTemp = (IplImage *) ptrTemp.ToPointer();
	IplImage *imageMask = (IplImage *) ptrMask.ToPointer();

	if (imageAbsDiffFrame == nullptr)
	{

		m_fSurfaceArea = nWidth * nHeight;

		/// Load our white/black mask of regions of interest
		if ((m_strFileNameMotionMask != nullptr) && (m_strFileNameMotionMask->Length > 0) )
		{
			System::IntPtr ptrstring = System::Runtime::InteropServices::Marshal::StringToCoTaskMemAnsi(m_strFileNameMotionMask);
			const char *pFileName = (const char *)ptrstring.ToPointer();
			IplImage *imageMaskTemp = cvLoadImage(pFileName, CV_LOAD_IMAGE_COLOR);
			System::Runtime::InteropServices::Marshal::FreeHGlobal(ptrstring);

			if (imageMaskTemp != NULL)
			{
				if ((imageMaskTemp->width == nWidth) && (imageMaskTemp->height == nHeight) )
				{

					imageMask = cvCreateImage(size, IPL_DEPTH_8U, 1);
					cvCvtColor(imageMaskTemp, imageMask, CV_RGB2GRAY);
					ptrMask = System::IntPtr(imageMask);
				}
				else
				{
					Console::WriteLine(String::Format("Mask for motion detector has a different width and height than the camera format, will attempt to scale" ));
					IplImage *imageResized = cvCreateImage(cvSize(nWidth, nHeight), imageMaskTemp->depth, imageMaskTemp->nChannels);
					cvResize(imageMaskTemp, imageResized, CV_INTER_LINEAR);
					imageMask = cvCreateImage(size, IPL_DEPTH_8U, 1);
					cvCvtColor(imageResized, imageMask, CV_RGB2GRAY);
					ptrMask = System::IntPtr(imageMask);
					cvReleaseImage( &imageResized );  
				}
				cvReleaseImage( &imageMaskTemp );  
			}
		}

		if (imageMask != NULL)
		{
			// Mask our image, and get the surface area of the unmasked area so our percentage area calculation only considers the mask
			m_fSurfaceArea = MaskImage(curimage, imageMask);
		}

		imageTemp = cvCreateImage(size, IPL_DEPTH_32F, 4);
		ptrTemp = System::IntPtr(imageTemp);

 
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
	else if (bShouldAnalyze == true)
	{
		if (imageMask != NULL)
			MaskImage(curimage, imageMask);

		cvRunningAvg(curimage, imageAverageFrame, 0.05, imageMask);
	}

	if (bShouldAnalyze == true)
	{
		cvConvert(imageAverageFrame, imagePreviousFrame);
		cvAbsDiff(curimage, imagePreviousFrame, imageAbsDiffFrame);


		cvCvtColor(imageAbsDiffFrame, imageGrayFrame, CV_RGB2GRAY);
		cvThreshold(imageGrayFrame, imageGrayFrame, 50, 255, CV_THRESH_BINARY);

		cvDilate(imageGrayFrame, imageGrayFrame, NULL, 5);
		cvErode(imageGrayFrame, imageGrayFrame, NULL, 2);

		/*if (bRetMotion == true)
		{
			cvCvtColor(imageGrayFrame, imageAbsDiffFrame, CV_GRAY2RGB);
			memcpy(pPixelData, imageAbsDiffFrame->imageData, bPixelData->Length);
		}*/
	

		CvMemStorage *storage = cvCreateMemStorage(0);
		CvSeq *countours = NULL;
		cvFindContours(imageGrayFrame, storage, &countours, 88, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE);

		m_ptrCurrentCountours = System::IntPtr(countours); // save countours

		m_fCurrentSurfaceArea = 0.0f;
		double fMaxContourArea = 0.0f;

		while (countours != NULL)
		{
			double fArea = cvContourArea(countours);
			if (fArea > fMaxContourArea)
				fMaxContourArea = fArea;

			/// See what largest are object is, only trigger on objects above a certain size
			if (fArea > m_fContourAreaThreshold)
			{
				m_fCurrentSurfaceArea += fArea;
			}
			countours = countours->h_next;

		}

		m_fLastMaxContourAreaDetected = (fMaxContourArea*100)/m_fSurfaceArea;
		m_fLastMeasuredValue = (m_fCurrentSurfaceArea*100)/m_fSurfaceArea;
        
		if ((m_fThreshold > 0) && (m_fLastMeasuredValue > m_fThreshold))
			bRet = true;
		else if ((ContourAreaThreshold > 0) && (m_fLastMaxContourAreaDetected >= ContourAreaThreshold) )
			bRet = true;
		else
			bRet = false;

		cvReleaseMemStorage(&storage);

		m_dtLastFrameProcessed = DateTime::Now;
	}

	if ((bRetMotion == true) && (bShouldAnalyze == true) )
	{
		cvCvtColor(imageGrayFrame, imageAbsDiffFrame, CV_GRAY2RGB);
		
		memcpy(pData, pPixelData, bPixelData->Length);
	
		//imageTemp = cvCloneImage(imageAbsDiffFrame);
		//cvAddWeighted(curimage, 0.5, imageAbsDiffFrame, 0.2, 0.0, imageTemp);

		//memcpy(pPixelData, imageTemp->imageData, bPixelData->Length);

		unsigned char *pSrcA = (unsigned char *) pData; //pPixelData;
		unsigned char *pSrcB = (unsigned char *) imageAbsDiffFrame->imageData;
		
		if (bRet == true)
		{
			for (int y=0; y<nHeight; y++)
			{
				for (int x=0; x<nWidth; x++)
				{
					//unsigned char nRedValueSource = *pSrcA;
					unsigned char nRedValueDiffFrame = *pSrcB;
					if (nRedValueDiffFrame > 0)
					{
						*(pSrcA + 2) |= 0x20;
						*(pSrcA + 1) &= 0x7F;
						*(pSrcA + 0) &= 0x7F;
					}


					pSrcA += 4;
					pSrcB += 4;
				}
			}
		}
		else
		{
			for (int y=0; y<nHeight; y++)
			{
				for (int x=0; x<nWidth; x++)
				{
					//unsigned char nRedValueSource = *pSrcA;
					unsigned char nRedValueDiffFrame = *pSrcB;
					if (nRedValueDiffFrame > 0)
					{
						*(pSrcA + 2) &= 0x7F;
						*(pSrcA + 1) &= 0x7F;
						*(pSrcA + 0) |= 0x20;
					}


					pSrcA += 4;
					pSrcB += 4;
				}
			}

		}

	}

	if (m_bShowText == true)
	{
		System::IntPtr ptrstring = System::Runtime::InteropServices::Marshal::StringToCoTaskMemAnsi(StatusString);
		const char *pText = (const char *)ptrstring.ToPointer();

		CvFont font;
		cvInitFont(&font, CV_FONT_HERSHEY_PLAIN, 1.0, 1.0);

		//cvRectangle(curimage, cvPoint(0,0), cvPoint(nWidth, 10), CV_RGB(0,0,0), CV_FILLED);
		cvPutText(curimage, pText, cvPoint(10, 10),  &font, CV_RGB(255,255,255));

		System::Runtime::InteropServices::Marshal::FreeHGlobal(ptrstring);

	}
	memcpy(pPixelData, pData, bPixelData->Length);

	if ((bRet == true) && (m_bMotionActive == false) )
	{
		m_bMotionActive = true;
		FireMotion();
	}
	else if ( (bRet == false) && (bShouldAnalyze == true) )
		m_bMotionActive = false;

	return bRet;
}
