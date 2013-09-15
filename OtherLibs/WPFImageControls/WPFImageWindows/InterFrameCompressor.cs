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
            return CompressFrameWithDimensions(bUncompressedFrame, Width, Height);
        }

        public byte[] CompressFrameWithDimensions(byte[] bUncompressedFrame, int nWidth, int nHeight)
        {
            BitmapEncoder objImageEncoder = null;
            int nBytesPerPixel = PixelFormat.BitsPerPixel / 8;
            int nRowLengthBytes = nWidth * nBytesPerPixel;

            if (VideoDataFormat.CompressedFormat == AudioClasses.VideoDataFormat.MPNG)
                objImageEncoder = new PngBitmapEncoder();
            else
            {
                objImageEncoder = new JpegBitmapEncoder();
                ((JpegBitmapEncoder)objImageEncoder).QualityLevel = QualityLevel;
            }
            byte[] bCompressedStream = null;
            try
            {
                BitmapSource source = BitmapFrame.Create(nWidth, nHeight, 96.0f, 96.0f, PixelFormat, null, bUncompressedFrame, nRowLengthBytes);
                BitmapFrame frame = BitmapFrame.Create(source);
                frame.Freeze();
                objImageEncoder.Frames.Add(frame);

                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                WrappingStream wstream = new WrappingStream(stream);

                //save to memory stream
                using (wstream)
                {
                    objImageEncoder.Save(wstream);

                    wstream.Seek(0, SeekOrigin.Begin);
                    bCompressedStream = new byte[wstream.Length];
                    wstream.Read(bCompressedStream, 0, bCompressedStream.Length);
                    wstream.Close();
                }

                //if ((objImageEncoder.Dispatcher != null) && (objImageEncoder.Dispatcher.Thread.IsAlive == true))
                //    objImageEncoder.Dispatcher.InvokeShutdown();
                //if ((frame.Dispatcher != null) && (frame.Dispatcher.Thread.IsAlive == true))
                //    frame.Dispatcher.InvokeShutdown();
                //if ((source.Dispatcher != null) && (source.Dispatcher.Thread.IsAlive == true))
                //    source.Dispatcher.InvokeShutdown();

                stream.Dispose();
                frame = null;
                source = null;
                objImageEncoder = null;
            }
            catch (Exception ex)
            {
                string strError = string.Format("Error encoding jpeg/png of width {0}, height {1}, rowlength {2}, bytes in {3}, error {4}", nWidth, nHeight, RowLengthBytes, bUncompressedFrame.Length, ex.Message);
                System.Diagnostics.EventLog.WriteEntry("xmedianet", strError, System.Diagnostics.EventLogEntryType.Error);
                throw new Exception(strError, ex);
            }
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

        public byte[] DecompressFrameWithDimensions(byte[] bCompressedFrame, out int nWidth, out int nHeight, out int nBytesPerPixel)
        {
            nWidth = 0;
            nHeight = 0;
            nBytesPerPixel = 0;

            MemoryStream stream = new MemoryStream(bCompressedFrame);

            BitmapDecoder pngDecoder = null;
            if (VideoDataFormat.CompressedFormat == AudioClasses.VideoDataFormat.MPNG)
                pngDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
            else
                pngDecoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.None);

            if (pngDecoder.Frames.Count > 0)
            {
                BitmapFrame frame = pngDecoder.Frames[0];
                nWidth = frame.PixelWidth;
                nHeight = frame.PixelHeight;
                nBytesPerPixel = 3;
                byte[] bRGBData = new byte[frame.PixelWidth * 3 * frame.PixelHeight];

                frame.CopyPixels(bRGBData, frame.PixelWidth * 3, 0);
                stream.Close();

                return bRGBData;
                //return new ImageWithPosition((int)frame.PixelWidth, (int)frame.PixelHeight, bRGBData);
            }
            return null;
        }

        public byte[] ResizeFrame(byte[] bUncompressedFrame, int nWidth, int nHeight, int nNewWidth, int nNewHeight)
        {
            try
            {
                int nBytesPerPixel = PixelFormat.BitsPerPixel/8;
                int nRowLengthBytes = nWidth * nBytesPerPixel;
                BitmapSource source = BitmapFrame.Create(nWidth, nHeight, 96.0f, 96.0f, PixelFormat, null, bUncompressedFrame, nRowLengthBytes);

                BitmapSource transbmp = new TransformedBitmap();
                ((TransformedBitmap)transbmp).BeginInit();
                ((TransformedBitmap)transbmp).Source = source;
                double fXScale = (double)nNewWidth / (double)nWidth;
                double fYScale = (double)nNewHeight / (double)nHeight;
                ((TransformedBitmap)transbmp).Transform = new ScaleTransform(fXScale, fYScale);
                ((TransformedBitmap)transbmp).EndInit();


                byte[] DestPixels = new byte[nBytesPerPixel * nNewWidth * nNewHeight];
                transbmp.CopyPixels(DestPixels, nBytesPerPixel * nNewWidth, 0);

                return DestPixels;

            }
            catch (Exception ex)
            {
                string strError = string.Format("Error resizing jpeg/png of width {0}, height {1}, rowlength {2}, bytes in {3}, error {4}", nWidth, nHeight, RowLengthBytes, bUncompressedFrame.Length, ex.Message);
                throw new Exception(strError, ex);
            }
        }

        #endregion
    }


    /// <summary>

    /// A <see cref="Stream"/> that wraps another stream. The major feature of <see cref="WrappingStream"/> is that it does not dispose the

    /// underlying stream when it is disposed; this is useful when using classes such as <see cref="BinaryReader"/> and

    /// <see cref="System.Security.Cryptography.CryptoStream"/> that take ownership of the stream passed to their constructors.

    /// </summary>

    public class WrappingStream : Stream
    {

        /// <summary>

        /// Initializes a new instance of the <see cref="WrappingStream"/> class.

        /// </summary>

        /// <param name="streamBase">The wrapped stream.</param>

        public WrappingStream(Stream streamBase)
        {

            // check parameters

            if (streamBase == null)

                throw new ArgumentNullException("streamBase");



            m_streamBase = streamBase;

        }



        /// <summary>

        /// Gets a value indicating whether the current stream supports reading.

        /// </summary>

        /// <returns><c>true</c> if the stream supports reading; otherwise, <c>false</c>.</returns>

        public override bool CanRead
        {

            get { return m_streamBase == null ? false : m_streamBase.CanRead; }

        }



        /// <summary>

        /// Gets a value indicating whether the current stream supports seeking.

        /// </summary>

        /// <returns><c>true</c> if the stream supports seeking; otherwise, <c>false</c>.</returns>

        public override bool CanSeek
        {

            get { return m_streamBase == null ? false : m_streamBase.CanSeek; }

        }



        /// <summary>

        /// Gets a value indicating whether the current stream supports writing.

        /// </summary>

        /// <returns><c>true</c> if the stream supports writing; otherwise, <c>false</c>.</returns>

        public override bool CanWrite
        {

            get { return m_streamBase == null ? false : m_streamBase.CanWrite; }

        }



        /// <summary>

        /// Gets the length in bytes of the stream.

        /// </summary>

        public override long Length
        {

            get { ThrowIfDisposed(); return m_streamBase.Length; }

        }



        /// <summary>

        /// Gets or sets the position within the current stream.

        /// </summary>

        public override long Position
        {

            get { ThrowIfDisposed(); return m_streamBase.Position; }

            set { ThrowIfDisposed(); m_streamBase.Position = value; }

        }



        /// <summary>

        /// Begins an asynchronous read operation.

        /// </summary>

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {

            ThrowIfDisposed();

            return m_streamBase.BeginRead(buffer, offset, count, callback, state);

        }



        /// <summary>

        /// Begins an asynchronous write operation.

        /// </summary>

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {

            ThrowIfDisposed();

            return m_streamBase.BeginWrite(buffer, offset, count, callback, state);

        }



        /// <summary>

        /// Waits for the pending asynchronous read to complete.

        /// </summary>

        public override int EndRead(IAsyncResult asyncResult)
        {

            ThrowIfDisposed();

            return m_streamBase.EndRead(asyncResult);

        }



        /// <summary>

        /// Ends an asynchronous write operation.

        /// </summary>

        public override void EndWrite(IAsyncResult asyncResult)
        {

            ThrowIfDisposed();

            m_streamBase.EndWrite(asyncResult);

        }



        /// <summary>

        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.

        /// </summary>

        public override void Flush()
        {

            ThrowIfDisposed();

            m_streamBase.Flush();

        }



        /// <summary>

        /// Reads a sequence of bytes from the current stream and advances the position

        /// within the stream by the number of bytes read.

        /// </summary>

        public override int Read(byte[] buffer, int offset, int count)
        {

            ThrowIfDisposed();

            return m_streamBase.Read(buffer, offset, count);

        }



        /// <summary>

        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.

        /// </summary>

        public override int ReadByte()
        {

            ThrowIfDisposed();

            return m_streamBase.ReadByte();

        }



        /// <summary>

        /// Sets the position within the current stream.

        /// </summary>

        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>

        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>

        /// <returns>The new position within the current stream.</returns>

        public override long Seek(long offset, SeekOrigin origin)
        {

            ThrowIfDisposed();

            return m_streamBase.Seek(offset, origin);

        }



        /// <summary>

        /// Sets the length of the current stream.

        /// </summary>

        /// <param name="value">The desired length of the current stream in bytes.</param>

        public override void SetLength(long value)
        {

            ThrowIfDisposed();

            m_streamBase.SetLength(value);

        }



        /// <summary>

        /// Writes a sequence of bytes to the current stream and advances the current position

        /// within this stream by the number of bytes written.

        /// </summary>

        public override void Write(byte[] buffer, int offset, int count)
        {

            ThrowIfDisposed();

            m_streamBase.Write(buffer, offset, count);

        }



        /// <summary>

        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.

        /// </summary>

        public override void WriteByte(byte value)
        {

            ThrowIfDisposed();

            m_streamBase.WriteByte(value);

        }



        /// <summary>

        /// Gets the wrapped stream.

        /// </summary>

        /// <value>The wrapped stream.</value>

        protected Stream WrappedStream
        {

            get { return m_streamBase; }

        }



        /// <summary>

        /// Releases the unmanaged resources used by the <see cref="WrappingStream"/> and optionally releases the managed resources.

        /// </summary>

        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>

        protected override void Dispose(bool disposing)
        {

            // doesn't close the base stream, but just prevents access to it through this WrappingStream

            if (disposing)

                m_streamBase = null;



            base.Dispose(disposing);

        }



        private void ThrowIfDisposed()
        {

            // throws an ObjectDisposedException if this object has been disposed

            if (m_streamBase == null)

                throw new ObjectDisposedException(GetType().Name);

        }



        Stream m_streamBase;

    }



}
