/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.


#pragma once

using namespace System;
using namespace AudioClasses;

namespace ImageAquisition 
{


	public ref class MFVideoCaptureDevice : public AudioClasses::IVideoSource
	{
	public:
		MFVideoCaptureDevice(IntPtr MFActivate);
		~MFVideoCaptureDevice();

		static array<MFVideoCaptureDevice ^> ^GetCaptureDevices();

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
			void raise(array<unsigned char> ^data, AudioClasses::VideoCaptureRate ^videoformat)
			{
				delNewFrame(data, videoformat);
			}
		}

		virtual System::String ^ToString() override
		{
			return Name;
		}


		delegate void DelegateError(String ^strError);
		event DelegateError ^OnFailStartCapture;

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

		property int MaxFrameRate
		{
			int get()
			{
				return m_nMaxFrameRate;
			}

			void set(int nValue)
			{
				m_nMaxFrameRate = nValue;
				m_dtLastFrameSent = DateTime::MinValue;
			}

		}

		String ^DisplayName;

		String ^UniqueName;

		IntPtr SourceDevice; //webcam, file, etc

	protected:

		event AudioClasses::DelegateRawFrame ^delNewFrame;

		System::Collections::Generic::List<VideoCaptureRate  ^> ^m_listVideoFormats;

		VideoCaptureRate ^ActiveVideoFormat;

		System::Threading::Thread ^CaptureThread;
		void Load();
		void OurCaptureThread();
		bool quit;

		IntPtr MFActivate;
		IntPtr SourceReader;

		int m_nMaxFrameRate;
		DateTime m_dtLastFrameSent;
	};

	public ref class MFAudioDevice
	{
	public:
		MFAudioDevice(IntPtr MFActivate);
		~MFAudioDevice();

		static array<MFAudioDevice ^> ^GetCaptureDevices();

		bool Start();
		void Stop();

		delegate void DelegateNewAudioFrame(array<unsigned char> ^bPCMData);
		event DelegateNewAudioFrame ^OnNewPCMFrame;

		String ^Name;

	protected:

		System::Threading::Thread ^CaptureThread;
		void Load();
		void OurCaptureThread();
		bool quit;

		IntPtr MFActivate;
		IntPtr SourceDevice; //webcam, file, etc
		IntPtr SourceReader;
	};


	public ref class MFVideoEncoder
	{
	public:
		MFVideoEncoder();
		//MFVideoEncoder(IMFByteSTream ^bytestrem);
		~MFVideoEncoder();


		bool Start(String ^strFileName, VideoCaptureRate ^videoformat, System::DateTime dtStart, bool Supply48by16Audio);
		bool Start(String ^strFileName, VideoCaptureRate ^videoformat, System::DateTime dtStart, bool SupplyVideo, bool Supply48by16Audio);
		void AddVideoFrame(array<unsigned char> ^RGBData, DateTime dtStart);
		void AddAudioFrame(array<unsigned char> ^PCMData48KHz16Bit, DateTime dtStart);
		void Stop();

	protected:

		IntPtr SinkWriter;
		int StreamIndexVideo;
		int StreamIndexAudio;
		VideoCaptureRate ^VideoFormat;
		DateTime StartTime;

	};


		// Resamples blocks of a fixed sample size - introduces a delay 1/4 of the sample size
	public ref class SampleConvertor
	{
    public:
		 /// example... Downsampling from 48000 to 8000, a source factor of 48 and a dest factor of 8 would work for small sample sizes (sizes over 48)
		 /// for a larger sample size, 480 and 80 would work
		 SampleConvertor(int nSourceFactor, int nDestFactor, int nSampleSize);
		 SampleConvertor(int nSourceFactor, int nDestFactor, int nSampleSize, float fPreLowPassFilterFrequencyZeroToPointFive, float fPostLowPassFilterFrequency);
		 SampleConvertor(int nSourceFactor, int nDestFactor, int nSampleSize, int nStartEndBufferSize);
		 virtual ~SampleConvertor() ;

		 array<short>^ Convert(array<short>^ SourcePCM);
		 array<short>^ ConvertAnySize(array<short>^ SourcePCM);

	protected:

		array<short>^ DoubleSource;
		array<short>^ DoubleSourcePreFiltered;
		array<short>^ LastSource;
		array<short>^ Resample;					/// Resampled data on the DoubleSource between 0 and .5 
		array<short>^ ResampleAndFiltered;  /// Resampled data on the DoubleSource between 0 and .5 

		int StartEndBufferSize;
		int SampleSize;
		int ResampleSize;
		int ResampleSizeBuffer;
		int ResampleSizeWithBuffer;
		IntPtr Taps;
		IntPtr DelayLine;
		int filterLen;
		short delayLen;
		int UpSample;
		int DownSample;


		bool bDoPostLowPassFilter;
		IntPtr TapsPostLP;
		int PostLPFilterLen;
		IntPtr DelayLinePost;
		short delayLenPost;

		bool bDoPreLowPassFilter;
		IntPtr TapsPreLP;
		int PreLPFilterLen;
		IntPtr DelayLinePre;
		short delayLenPre;

	};


}
