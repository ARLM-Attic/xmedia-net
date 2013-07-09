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

		virtual bool Detect(array<unsigned char> ^%bPixelData, int nWidth, int nHeight, bool bRetMotion);

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

		/// An file of an image the same size as the current capture where white areas are where motion should be detected and black areas should be ignored
		property System::String ^ FileNameMotionMask
		{
			virtual System::String ^ get()
			{
				return m_strFileNameMotionMask;
			}
			virtual void set(System::String ^ value)
			{
				m_strFileNameMotionMask = value;
			}
		}

	protected:
		System::IntPtr ptrCurrentFrame;
		System::IntPtr ptrGrayFrame;
		System::IntPtr ptrAverageFrame;
		System::IntPtr ptrAbsDiffFrame;
		System::IntPtr ptrPreviousFrame;
		System::IntPtr ptrTemp;
		System::IntPtr ptrMask;
		double m_fCurrentSurfaceArea;
		double m_fSurfaceArea;
		System::IntPtr m_ptrCurrentCountours;
		double m_fThreshold;
		double m_fLastMeasuredValue;
		double m_nTriggerTime;

		System::String ^m_strFileNameMotionMask;
	};

}