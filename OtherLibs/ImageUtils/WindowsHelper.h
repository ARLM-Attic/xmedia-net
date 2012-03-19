
#pragma once

using namespace System;

namespace ImageUtils 
{

	public ref class WindowsHelper
	{
	public:
	
		static void MouseEvent(int x, int y, unsigned int MouseEventType);
		static void KeyboardEvent(unsigned int KeyboardEventType, unsigned short virtualkey, unsigned short scan);

	};

}