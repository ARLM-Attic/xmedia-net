
#include "StdAfx.h"
#include "WindowsHelper.h"


void ImageUtils::WindowsHelper::MouseEvent(int x, int y, unsigned int MouseEventType)
{
	SetCursorPos(x, y);

	INPUT input;
	input.mi.dx = x;
	input.mi.dy = y;
	input.mi.dwFlags = MouseEventType;
	input.type = INPUT_MOUSE;

	::SendInput(1, &input, sizeof(INPUT));
}

void ImageUtils::WindowsHelper::KeyboardEvent(unsigned int KeyboardEventType, unsigned short virtualkey, unsigned short scan)
{
	INPUT input;     
	input.type = INPUT_KEYBOARD;
	input.ki.wVk = virtualkey;
	input.ki.wScan = scan; //L'c';     
	input.ki.dwFlags = KeyboardEventType;     
	input.ki.time = 0;     
	input.ki.dwExtraInfo = 0; 

	int retval = SendInput(1, &input, sizeof(INPUT)); 
}
