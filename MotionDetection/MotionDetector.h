#pragma once
public ref class MotionDetector
{
public:
	MotionDetector(void);
	virtual ~MotionDetector(void);

	void Detect(array<unsigned char> ^bPixelData);
};

