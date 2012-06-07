/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AudioClasses;

namespace RTP.Codecs
{
    public class FragmentedVideoCodec : Codec
    {

        public FragmentedVideoCodec(string strName, IVideoCompressor interframecompressor, VideoCaptureRate videoformat)
            : base(strName)
        {
            InterFrameCompressor = interframecompressor;
            m_objVideoParameters = videoformat; 
        }

        VideoFrameFragmentor IncomingFragmentor = new VideoFrameFragmentor(null);

        /// <summary>
        /// Decode the packets.  Don't return bytes until we have all the fragments
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public override byte[] DecodeToBytes(RTPPacket packet)
        {
            byte[] bFrame = IncomingFragmentor.NewFragmentReceived(packet);
            if (bFrame != null)
                bFrame = InterFrameCompressor.DecompressFrame(bFrame);
            return bFrame;
        }

        public ushort MTU = 1400;
        public double FrameRate = 30.0f;

        public IVideoCompressor InterFrameCompressor = null;

        uint m_nFrame = 0;
        public override RTPPacket[] Encode(byte[] bUncompressedFrame)
        {
            byte[] bCompressedFrame = InterFrameCompressor.CompressFrame(bUncompressedFrame);
            List<RTPPacket> packets = new List<RTPPacket>();
            int nAt = 0;
            /// Send the data packets
            /// 
            int nPacket = 0;
            while (true)
            {
                int nNextSize = ((bCompressedFrame.Length - nAt) > MTU) ? MTU : (bCompressedFrame.Length - nAt);
                byte[] bNextData = new byte[nNextSize];
                Array.Copy(bCompressedFrame, nAt, bNextData, 0, nNextSize);
                nAt += nNextSize;

                RTPPacket packet = new RTPPacket();
                packet.PayloadData = bNextData;
                packet.TimeStamp = (uint)((m_nFrame*90000)/FrameRate);
                packet.Marker = (nPacket == 0) ? true : false;
                packets.Add(packet);
                
                if (nAt >= (bCompressedFrame.Length - 1))
                    break;
                nPacket++;
                
            }

            m_nFrame++;
            return packets.ToArray();
        }

        public RTPPacket FormatNextPacket(byte[] VideoPayload)
        {
            RTPPacket packet = new RTPPacket();
            packet.PayloadData = VideoPayload;
            packet.TimeStamp = (uint)(m_nFrame * 1000 / m_nFrame);

            return packet;
        }
    }


    /// <summary>
    /// compresses interframe video.  Must supply the compressor so this will work on different platforms, since
    /// jpeg/png compression/decompression is different on each platform.  Windows WPF can use BitmapEncoder, we've define one in WPFImageWindows
    /// </summary>
    public class UDPMotionJpegCodec : FragmentedVideoCodec
    {

        public UDPMotionJpegCodec(IVideoCompressor compressor, VideoCaptureRate videoformat)
            : base("UDP Motion JPEG", compressor, videoformat)
        {
            m_objVideoParameters.CompressedFormat = AudioClasses.VideoDataFormat.MJPEG;
            m_objVideoParameters.UncompressedFormat = AudioClasses.VideoDataFormat.RGB32;
        }
      
    }
}
