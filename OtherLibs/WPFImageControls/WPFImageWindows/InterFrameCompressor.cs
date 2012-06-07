/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AudioClasses;

namespace WPFImageWindows
{
    /// <summary>
    /// Performs JPEG/PNG compression/decompression using WPF libraries
    /// </summary>
    public class InterFrameCompressor : IVideoCompressor
    {
        public InterFrameCompressor(VideoCaptureRate format)
        {
            VideoDataFormat = format;
            Width = format.Width;
            Height = format.Height;
            if (format.UncompressedFormat == AudioClasses.VideoDataFormat.RGB32)
            {
                RowLengthBytes = Width * 4;
                PixelFormat = PixelFormats.Bgr32;
            }
            else if (format.UncompressedFormat == AudioClasses.VideoDataFormat.RGB24)
            {
                RowLengthBytes = Width * 3;
                PixelFormat = PixelFormats.Bgr24;
            }

        }

        public VideoCaptureRate VideoDataFormat = null;
        int Width = 640;
        int Height = 480;
        int RowLengthBytes = 640 * 3;
        PixelFormat PixelFormat = PixelFormats.Bgr24;

        public int QualityLevel = 30;

        #region IVideoCompressor Members

        public byte[] CompressFrame(byte[] bUncompressedFrame)
        {
            BitmapEncoder objImageEncoder = null;
            if (VideoDataFormat.CompressedFormat == AudioClasses.VideoDataFormat.MPNG)
                objImageEncoder = new PngBitmapEncoder();
            else
            {
                objImageEncoder = new JpegBitmapEncoder();
                ((JpegBitmapEncoder)objImageEncoder).QualityLevel = QualityLevel;
            }

            BitmapSource source = BitmapFrame.Create(Width, Height, 96.0f, 96.0f, PixelFormat, null, bUncompressedFrame, RowLengthBytes);
            BitmapFrame frame = BitmapFrame.Create(source);
            objImageEncoder.Frames.Add(frame);

            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            objImageEncoder.Save(ms);

            ms.Seek(0, SeekOrigin.Begin);
            byte [] bCompressedStream = new byte[ms.Length];
            ms.Read(bCompressedStream, 0, bCompressedStream.Length);
            ms.Close();
            ms.Dispose();
            return bCompressedStream;
        }

        public byte[] DecompressFrame(byte[] bCompressedFrame)
        {
            MemoryStream stream = new MemoryStream(bCompressedFrame);
            BitmapDecoder pngDecoder = null;
            if (VideoDataFormat.CompressedFormat == AudioClasses.VideoDataFormat.MPNG)
                pngDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
            else
                pngDecoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.None);

            if (pngDecoder.Frames.Count > 0)
            {
                BitmapFrame frame = pngDecoder.Frames[0];
                byte[] bRGBData = new byte[frame.PixelWidth * 3 * frame.PixelHeight];

                frame.CopyPixels(bRGBData, frame.PixelWidth * 3, 0);
                stream.Close();

                return bRGBData;
                //return new ImageWithPosition((int)frame.PixelWidth, (int)frame.PixelHeight, bRGBData);
            }
            return null;
        }

        #endregion
    }
}
