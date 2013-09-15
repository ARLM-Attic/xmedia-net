#pragma once

using namespace System;
using namespace AudioClasses;


namespace MotionDetection
{
	ref class OpenCVVideoSource : public AudioClasses::IVideoSource
	{
	public:
		OpenCVVideoSource(int nSource);

		virtual ~OpenCVVideoSource(void);


		virtual bool Start(VideoCaptureRate ^videoformat);
		virtual void Stop();

		event AudioClasses::DelegateRawFrame ^OnNewFrame
		{
		public :
			virtual void add(AudioClasses::DelegateRawFrame ^del)
			{
				delNewFrame += del;
			}
			virtual void remove(AudioClasses::DelegateRawFrame ^del)
			{
				delNewFrame -= del;
			} 
			void raise(array<unsigned char> ^data, AudioClasses::VideoCaptureRate ^videoformat, System::Object ^objDevice)
			{
				delNewFrame(data, videoformat, objDevice);
			}
		}

		property System::Collections::Generic::List<VideoCaptureRate  ^> ^VideoFormats
		{
			virtual System::Collections::Generic::List<VideoCaptureRate  ^> ^get()
			{
				return m_listVideoFormats;
			}
		}

			property VideoCaptureRate  ^ActiveVideoCaptureRate
		{
			virtual VideoCaptureRate  ^get() 
			{
				return ActiveVideoFormat;
			}
		}

		property String ^Name
		{
			virtual String ^get()
			{
				return DisplayName;
			}

		}

		
		virtual System::String ^ToString() override
		{
			return Name;
		}

		virtual MediaSample^ PullFrame() override
		{
			return nullptr; /// Only a push filter
		}

		String ^DisplayName;
		String ^UniqueName;

		delegate void DelegateError(String ^strError, System::Object ^objDevice);
		event DelegateError ^OnFailStartCapture;

	protected:

		event AudioClasses::DelegateRawFrame ^delNewFrame;

		System::Collections::Generic::List<VideoCaptureRate  ^> ^m_listVideoFormats;

		VideoCaptureRate ^ActiveVideoFormat;

		System::Threading::Thread ^CaptureThread;
		void OurCaptureThread();
		bool quit;

		System::IntPtr ptrCvCapture;

		int m_nMaxFrameRate;
		DateTime m_dtLastFrameSent;

	};

}



