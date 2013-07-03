#include "MotionDetector.h"

#include <cv.h>
#include <cxcore.h>
#include <cvaux.hpp>
#include <cvwimage.h>
#include <highgui.h>



MotionDetector::MotionDetector(void)
{
}


MotionDetector::~MotionDetector(void)
{
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


void MotionDetector::Detect(array<unsigned char> ^bPixelData, int nWidth, int nHeight)
{
	CvSize size;
	size.width = nWidth;
	size.height = nHeight;
	IplImage *image = cvCreateImage(size, 8, 3);
	//cvCvtColor( img, imgHSV, CV_BGR2HSV );  

	cvFree(image);
}
http://www.bukisa.com/articles/263221_connected-components-using-opencv
http://opencv.willowgarage.com/documentation/c/imgproc_feature_detection.html
IplImage* src;
    if( argc == 2 && (src=cvLoadImage(argv[1], 0))!= 0)
    {
        IplImage* dst = cvCreateImage( cvGetSize(src), 8, 1 );
        IplImage* color_dst = cvCreateImage( cvGetSize(src), 8, 3 );
        CvMemStorage* storage = cvCreateMemStorage(0);
        CvSeq* lines = 0;
        int i;
        cvCanny( src, dst, 50, 200, 3 );
        cvCvtColor( dst, color_dst, CV_GRAY2BGR );
#if 1
        lines = cvHoughLines2( dst,
                               storage,
                               CV_HOUGH_STANDARD,
                               1,
                               CV_PI/180,
                               100,
                               0,
                               0 );

        for( i = 0; i < MIN(lines->total,100); i++ )
        {
            float* line = (float*)cvGetSeqElem(lines,i);
            float rho = line[0];
            float theta = line[1];
            CvPoint pt1, pt2;
            double a = cos(theta), b = sin(theta);
            double x0 = a*rho, y0 = b*rho;
            pt1.x = cvRound(x0 + 1000*(-b));
            pt1.y = cvRound(y0 + 1000*(a));
            pt2.x = cvRound(x0 - 1000*(-b));
            pt2.y = cvRound(y0 - 1000*(a));
            cvLine( color_dst, pt1, pt2, CV_RGB(255,0,0), 3, 8 );
        }
#else
        lines = cvHoughLines2( dst,
                               storage,
                               CV_HOUGH_PROBABILISTIC,
                               1,
                               CV_PI/180,
                               80,
                               30,
                               10 );
        for( i = 0; i < lines->total; i++ )
        {
            CvPoint* line = (CvPoint*)cvGetSeqElem(lines,i);
            cvLine( color_dst, line[0], line[1], CV_RGB(255,0,0), 3, 8 );
        }
#endif
        cvNamedWindow( "Source", 1 );
        cvShowImage( "Source", src );

        cvNamedWindow( "Hough", 1 );
        cvShowImage( "Hough", color_dst );

        cvWaitKey(0);
    }
