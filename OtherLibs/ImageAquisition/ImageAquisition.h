/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.


#pragma once

using namespace System;
using namespace AudioClasses;

namespace ImageAquisition 
{


	public ref class MFVideoCaptureDevice
	{
	public:
		MFVideoCaptureDevice(IntPtr MFActivate);
		~MFVideoCaptureDevice();

		static array<MFVideoCaptureDevice ^> ^GetCaptureDevices();

		bool Start(VideoCaptureRate ^videoformat);
		void Stop();

		delegate void DelegateNewFrame(array<unsigned char> ^pFrame, VideoCaptureRate ^videoformat);
		event DelegateNewFrame ^OnNewFrame;

		delegate void DelegateError(String ^strError);
		event DelegateError ^OnFailStartCapture;

		System::Collections::Generic::List<VideoCaptureRate  ^> ^VideoFormats;

		String ^DisplayName;

		String ^UniqueName;

		IntPtr SourceDevice; //webcam, file, etc

	protected:


		VideoCaptureRate ^ActiveVideoFormat;

		System::Threading::Thread ^CaptureThread;
		void Load();
		void OurCaptureThread();
		bool quit;

		IntPtr MFActivate;
		IntPtr SourceReader;
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


}
