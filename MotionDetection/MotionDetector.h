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

		property String ^StatusString
		{
			virtual String ^ get()
			{
				return String::Format("{0}, Motion: {1}, MaxBlob: {2}", DateTime::Now,  Math::Round(LastMeasuredValue, 2), Math::Round(LastMaxContourAreaDetected, 2));
			}
		}

		 
		///The minimum area for a contour for it to be considered motion - smaller ones are ignored
		property double ContourAreaThreshold
		{
			double get()
			{
				return m_fContourAreaThreshold;
			}
			void set(double value)
			{
				m_fContourAreaThreshold = value;
			}
		}

		property double LastMaxContourAreaDetected
		{
			double get()
			{
				return m_fLastMaxContourAreaDetected;
			}
		}

		property bool ShowText
		{
			 bool get()
			{
				return m_bShowText;
			}
			 void set(bool value)
			 {
				 m_bShowText = value;
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

		double m_fContourAreaThreshold;
		double m_fLastMaxContourAreaDetected;
		bool m_bShowText;
		System::String ^m_strFileNameMotionMask;
	};

}