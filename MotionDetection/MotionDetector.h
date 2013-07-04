#pragma once

using namespace System;
using namespace AudioClasses;


namespace MotionDetection
{
/// Contour area motion detector... Based on the python script at.   
// /https://github.com/RobinDavid/Motion-detection-OpenCV/blob/master/MotionDetectorContours.py

public ref class ContourAreaMotionDetector : public AudioClasses::IMotionDetector
{
	public:
		ContourAreaMotionDetector(void);
		virtual ~ContourAreaMotionDetector(void);

		virtual bool Detect(array<unsigned char> ^bPixelData, int nWidth, int nHeight);

		property double Threshold
		{
			virtual double get()
			{
				return m_fThreshold;
			}
			virtual void set(double value)
			{
				m_fThreshold = value;
			}
		}

		property double LastMeasuredValue
		{
			virtual double get()
			{
				return m_fLastMeasuredValue;
			}
		}

	protected:
		System::IntPtr ptrCurrentFrame;
		System::IntPtr ptrGrayFrame;
		System::IntPtr ptrAverageFrame;
		System::IntPtr ptrAbsDiffFrame;
		System::IntPtr ptrPreviousFrame;
		double m_fCurrentSurfaceArea;
		double m_fSurfaceArea;
		System::IntPtr m_ptrCurrentCountours;
		double m_fThreshold;
		double m_fLastMeasuredValue;
		double m_nTriggerTime;
	};

}