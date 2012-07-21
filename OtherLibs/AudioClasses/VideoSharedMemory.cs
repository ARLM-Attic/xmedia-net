/// Copyright (c) 2012 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AudioClasses;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization;
using System.IO;

namespace AudioClasses
{

    /// <summary>
    /// Stores video frame information in shared memory so multiple processes can use a video capture device at the same time
    /// </summary>
    public class VideoSharedMemory : IDisposable
    {
        public VideoSharedMemory(VideoCaptureRate rate, string strUniqueName)
        {
            StartSharedMemory(rate, strUniqueName);
        }

        private long m_nVideoBufferOffeset = 0;

        public long VideoBufferOffset
        {
            get { return m_nVideoBufferOffeset; }
            set { m_nVideoBufferOffeset = value; }
        }

        private long m_nVideoBufferSize = 0;

        public long VideoBufferSize
        {
            get { return m_nVideoBufferSize; }
            set { m_nVideoBufferSize = value; }
        }

        protected MemoryMappedFile OurMemoryMappedFile = null;
        protected MemoryMappedViewStream StartStream = null;
        protected MemoryMappedViewStream VideoStream = null;

        protected static DataContractSerializer rateser = new DataContractSerializer(typeof(VideoCaptureRate));

        VideoCaptureRate StartSharedMemory(VideoCaptureRate rate, string strUniqueName)
        {
            MemoryStream mem = new MemoryStream();
            rateser.WriteObject(mem, rate);
            VideoBufferOffset = mem.Length;
            mem.Close();
            mem.Dispose();

            int nBytesPerPixel = 3;
            if (rate.UncompressedFormat == VideoDataFormat.RGB32)
                nBytesPerPixel = 4;

            VideoBufferSize = nBytesPerPixel * rate.Width * rate.Height;


            try
            {
                OurMemoryMappedFile = MemoryMappedFile.OpenExisting(strUniqueName);
                StartStream = OurMemoryMappedFile.CreateViewStream(0, VideoBufferOffset);
                VideoStream = OurMemoryMappedFile.CreateViewStream(VideoBufferOffset, VideoBufferSize);

                StartStream.Seek(0, SeekOrigin.Begin);
                m_objCurrentVideoCaptureRate = rateser.ReadObject(StartStream) as VideoCaptureRate;

                return m_objCurrentVideoCaptureRate;
            }
            catch (FileNotFoundException ex)
            {
                /// Not found, we'll create our own
            }

            OurMemoryMappedFile = MemoryMappedFile.CreateNew(strUniqueName, VideoBufferOffset + VideoBufferSize);
            StartStream = OurMemoryMappedFile.CreateViewStream(0, VideoBufferOffset);
            VideoStream = OurMemoryMappedFile.CreateViewStream(VideoBufferOffset, VideoBufferSize);
            m_objCurrentVideoCaptureRate = rate;

            return m_objCurrentVideoCaptureRate;
        }

        VideoCaptureRate m_objCurrentVideoCaptureRate = null;
        public VideoCaptureRate CurrentVideoCaptureRate
        {
            get
            {
               return m_objCurrentVideoCaptureRate; 
            }
        }

        private string m_strName = "";
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }



        public void WriteVideoFrame(byte[] bVideoFrame)
        {
            if (m_bIsDisposed == true)
                throw new ObjectDisposedException(this.ToString());
            VideoStream.Seek(0, SeekOrigin.Begin);
            VideoStream.Write(bVideoFrame, 0, bVideoFrame.Length);
        }

        public byte [] ReadVideoFrame()
        {
            if (m_bIsDisposed == true)
                throw new ObjectDisposedException(this.ToString());
            byte[] bRet = new byte[VideoBufferSize];
            VideoStream.Seek(0, SeekOrigin.Begin);
            VideoStream.Read(bRet, 0, bRet.Length);

            return bRet;
        }

        public void ReadVideoFrameIntoBuffer(byte [] bBuffer)
        {
            if (m_bIsDisposed == true)
                throw new ObjectDisposedException(this.ToString());
            VideoStream.Seek(0, SeekOrigin.Begin);
            VideoStream.Read(bBuffer, 0, bBuffer.Length);
        }


        bool m_bIsDisposed = false;
        #region IDisposable Members

        public void Dispose()
        {
            if (m_bIsDisposed == true)
                return;

            m_bIsDisposed = true;

            VideoStream.Close();
            VideoStream.Dispose();
            StartStream.Close();
            StartStream.Dispose();

            OurMemoryMappedFile.Dispose();
        }

        #endregion
    }
}
