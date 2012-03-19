// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once


#include <Windows.h>
#include <DShow.h>
#include "dshowutil.h"
//#include <Mfidl.h>
//#include <Mfapi.h>
//#include <Mfreadwrite.h>

/// this is in here to stop qedit.h complaining that it can't find th dxtrans.h header
#pragma include_alias( "dxtrans.h", "qedit.h" )
#define __IDxtCompositor_INTERFACE_DEFINED__
#define __IDxtAlphaSetter_INTERFACE_DEFINED__
#define __IDxtJpeg_INTERFACE_DEFINED__
#define __IDxtKey_INTERFACE_DEFINED__



#include "qedit.h"

template <class T> void SafeRelease(T **ppT)
{
    if (*ppT)
    {
        (*ppT)->Release();
        *ppT = NULL;
    }
}
