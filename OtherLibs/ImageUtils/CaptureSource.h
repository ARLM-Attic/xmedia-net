#pragma once

using namespace System;

namespace ImageUtils 
{

	public ref class CaptureSource
	{
	public:
		CaptureSource(System::Guid guid);


		void StartCapture();
	
		void StopCapture();

		static void GetCaptureDevices();
		static void GetCaptureDevicesMF();

		//event OnNewFrame(array<unsigned char> ^RGB24Frame, int nWidth, int nHeight);

	protected:

		IntPtr GraphPointer;
		IntPtr ControlPointer;
		IntPtr EventPointer;

	};


}