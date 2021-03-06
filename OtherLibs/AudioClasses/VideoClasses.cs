﻿/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.ServiceModel;
using System.ComponentModel;

namespace AudioClasses
{
    public enum VideoDataFormat : int
	{
		Unknown = 0,
        RGB32 = 1, // RGB32 frames
        RGB24 = 2,
        MJPEG = 3, /// motion jpeg frames (each frame a jpeg)
        MPNG = 4, /// motion png
        MP4 = 5, // H.264 in mp4 container
        WMV9 = 6, // windows media 9 in asf container
        WMVSCREEN = 7, /// windows media screen in asf container
        VC1 = 8, /// vc1 in asf container
        H264 = 9, /// Raw h.264 stream returned from cam
	};



    [DataContractFormat]
    [DataContract]
    public class VideoCaptureRate : INotifyPropertyChanged
    {
        public VideoCaptureRate()
        {
        }

        public VideoCaptureRate(int nWidth, int nHeight, int nFrameRate, int nBitRate)
        {
            m_nWidth = nWidth;
            m_nHeight = nHeight;
            m_nFrameRate = nFrameRate;
            m_nEncodingBitRate = nBitRate;
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strProp)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strProp));
        }

        #endregion


        public override string ToString()
        {
            return string.Format("{0} x {1}, {2} fps - {3}, Stream: {4}", Width, Height, FrameRate, VideoFormatString, StreamIndex);
        }

        private int m_nWidth = 640;

        [DataMember]
        public int Width
        {
            get { return m_nWidth; }
            set { m_nWidth = value; FirePropertyChanged("Width"); }
        }

        private int m_nHeight = 480;

        [DataMember]
        public int Height
        {
            get { return m_nHeight; }
            set { m_nHeight = value; FirePropertyChanged("Height"); }
        }

        private int m_nFrameRate = 30;

        public int PixelCount
        {
            get
            {
                return Width * Height;
            }
        }

        [DataMember]
        public int FrameRate
        {
            get { return m_nFrameRate; }
            set { m_nFrameRate = value; FirePropertyChanged("FrameRate"); }
        }

        public TimeSpan FrameDuration
        {
            get
            {
                return new TimeSpan(0, 0, 0, 0, 1000 / FrameRate);
            }
        }

        private bool m_bActive = false;
        [DataMember]
        public bool Active
        {
            get { return m_bActive; }
            set { m_bActive = value; }
        }

        //private int m_nEncodingBitRate = 5000000;
        private int m_nEncodingBitRate = 2000000;
        [DataMember]
        public int EncodingBitRate
        {
            get { return m_nEncodingBitRate; }
            set { m_nEncodingBitRate = value; FirePropertyChanged("EncodingBitRate"); }
        }

        public override bool Equals(object obj)
        {
            if (obj is VideoCaptureRate)
            {
                VideoCaptureRate cr = obj as VideoCaptureRate;
                if ((Width == cr.Width) && (Height == cr.Height) && (FrameRate == cr.FrameRate))
                    return true;
            }
            return false;
        }

        //private VideoDataFormat m_eVideoDataFormat = VideoDataFormat.RGB32;
        //[DataMember]
        //public VideoDataFormat VideoDataFormat
        //{
        //    get { return m_eVideoDataFormat; }
        //    set { m_eVideoDataFormat = value; }
        //}

        private string m_strVideoFormatString = "";

        public string VideoFormatString
        {
            get { return m_strVideoFormatString; }
            set { m_strVideoFormatString = value; }
        }


        protected VideoDataFormat m_objUncompressedFormat = VideoDataFormat.RGB32;
        [DataMember]
        public virtual VideoDataFormat UncompressedFormat
        {
            get { return m_objUncompressedFormat; }
            set { m_objUncompressedFormat = value; }
        }

        protected VideoDataFormat m_objCompressedFormat = VideoDataFormat.Unknown;
        [DataMember]
        public virtual VideoDataFormat CompressedFormat
        {
            get { return m_objCompressedFormat; }
            set { m_objCompressedFormat = value; }
        }


        private int m_nStreamIndex = 0;

        public int StreamIndex
        {
            get { return m_nStreamIndex; }
            set { m_nStreamIndex = value; }
        }
    }


    /// <summary>
    /// Our main WCF interface for controlling network cameras
    /// </summary>
    public delegate void DelegateRawFrame(byte[] bRawData, VideoCaptureRate format, object objSource);

    public interface ICameraControl
    {
        void PanLeft();
        void PanRight();
        void PanRelative(int Units);

        void TiltUp();
        void TiltDown();
        void TiltRelative(int Units);

        void Zoom(int Factor);

        void TurnOffLED();

        void SetExposure(int nExposure);
    }

    public interface IVideoSource
    {
        List<VideoCaptureRate> VideoFormats { get; }

        event DelegateRawFrame OnNewFrame;

        VideoCaptureRate ActiveVideoCaptureRate { get; }

        string Name { get; }

        MediaSample PullFrame();

    }

    public class ImageRectangle
    {
        public ImageRectangle()
        {
        }
        public ImageRectangle(int nX, int nY, int nWidth, int nHeight)
        {
            X = nX;
            Y = nY;
            Width = nWidth;
            Height = nHeight;
        }

        private int m_nX = 0;

        public int X
        {
            get { return m_nX; }
            set { m_nX = value; }
        }
        private int m_nY = 0;

        public int Y
        {
            get { return m_nY; }
            set { m_nY = value; }
        }
        private int m_nWidth = 0;

        public int Width
        {
            get { return m_nWidth; }
            set { m_nWidth = value; }
        }
        private int m_nHeight = 0;

        public int Height
        {
            get { return m_nHeight; }
            set { m_nHeight = value; }
        }
    }
    /// <summary>
    ///  Video recognition occurred.  Can't use existing rectangle classes since they're dependent on window classes and may not work in mono
    /// </summary>
    /// <param name="strRecognitionName"></param>
    /// <param name="objBounds"></param>
    /// <param name="bRawData"></param>
    /// <param name="format"></param>
    /// <param name="objDevice"></param>
    public delegate void DelegateImageRecognition(string strRecognitionName,  ImageRectangle objBounds, byte[] bRawData, VideoCaptureRate format, object objDevice);
    /// <summary>
    /// Used by device that support some type of recognition event
    /// </summary>
    public interface IImageRecognitionDevice
    {
        event DelegateImageRecognition OnRecogntion;
    }

    public interface IMotionDetector
    {
        bool Detect(ref byte [] bPixelData, int nWidth, int nHeight, bool bRetMotion);
        double Threshold { get; set; }
        double LastMeasuredValue { get; }
        string StatusString { get; }
        event EventHandler OnMotionDetected;
    }

    public interface IVideoSink
    {
        string Name { get; set; }
        void PushFrame(MediaSample sample);
    }

    public interface IVideoCompressor
    {
        byte[] CompressFrame(byte[] bUncompressedFrame);
        byte[] CompressFrameWithDimensions(byte[] bUncompressedFrame, int nWidth, int nHeight);
        byte[] DecompressFrame(byte[] bCompressedFrame);
        byte[] DecompressFrameWithDimensions(byte[] bCompressedFrame, out int nWidth, out int nHeight, out int nBytesPerPixel);
        byte[] ResizeFrame(byte[] bUncompressedFrame, int nWidth, int nHeight, int nNewWidth, int nNewHeight);
        
    }

    public interface IVideoRecorder
    {
        bool StartRecording();
        string StopRecording();
        bool Recording { get; set; }
    }

    public interface ICameraController
    {
        void PanLeft();
        void PanRight();
        void TiltUp();
        void TiltDown();
        void SetFocus(int nFocus);
        void SetExposure(int nExposure);
        void Zoom(int nFactor);

    }

}
