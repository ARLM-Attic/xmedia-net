/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using AudioClasses;
using SocketServer;

namespace RTP
{
    public class RTPVideoStream : RTPStream, IVideoSource, IVideoSink
    {
        public RTPVideoStream(byte nPayload, Codec codec)
            : base(nPayload)
        {
            VideoCodec = codec;
            UseInternalTimersForPacketPushPull = false;
        }

        Codec m_objVideoCodec = null;

        public Codec VideoCodec
        {
            get { return m_objVideoCodec; }
            set 
            { 
                m_objVideoCodec = value;
            }
        }


        public override bool Start(IPEndPoint destinationEp, int nPacketTimeTx, int nPacketTimeRx)
        {
            if (this.VideoCodec == null)
                return false;
            this.VideoCodec.ReceivePTime = nPacketTimeRx;
            this.VideoCodec.TransmitPTime = nPacketTimeTx;
            return base.Start(destinationEp, nPacketTimeTx, nPacketTimeRx);
        }

        protected override void HandleIncomingRTPPacket(RTPPacket packet)
        {
            if (VideoCodec == null)
                return;

            byte [] bFrame = VideoCodec.DecodeToBytes(packet);
            if ( (bFrame != null) && (OnNewFrame != null) && (VideoFormat != null) )
            {
                OnNewFrame(bFrame, VideoFormat);
            }
        }


        /// <summary>
        /// Sends the next sample directly.  A client can either do the default, with UseInternalTimersForPacketPushPull set to true, which uses timers and reads audio from the
        /// SendAudioQueue, or it can push them directly by turning off UseInternalTimersForPacketPushPull and calling SendNextSample at the appropriate ptime
        /// </summary>
        /// <param name="bData"></param>
        public override void SendNextSample(byte[] bUncompressedVideo)
        {
            if (IsActive == false)
                return;

            if (VideoCodec == null)
                return;

            if (DestinationEndpoint == null)
                return;

            SendVideo(bUncompressedVideo);
        }

        object SendLock = new object();


        protected void SendVideo(byte[] bUncompressedVideo)
        {


            RTPPacket[] packets = new RTPPacket[] { };
            lock (SendLock)
            {
                packets = VideoCodec.Encode(bUncompressedVideo);
            }

            lock (SocketLock)
            {
                foreach (RTPPacket packet in packets)
                {
                    FormatNextPacket(packet);
                    byte[] bDataPacket = packet.GetBytes();
                    if (RTPUDPClient != null)
                        RTPUDPClient.SendUDP(bDataPacket, bDataPacket.Length, DestinationEndpoint);
                }
            }
        }


        #region IVideoSource Members


        // Call when we have a new video fram available
        protected VideoCaptureRate VideoFormat = null;
        public VideoCaptureRate ActiveVideoCaptureRate
        {
            get
            {
                return VideoFormat;
            }
            set
            {
                VideoFormat = value;   
            }
        }

        public string Name
        {
            get
            {
                return "RTP Video Source";
            }
            set
            {
                
            }
        }

        public List<VideoCaptureRate> VideoFormats
        {
            get { throw new NotImplementedException(); }
        }

        public event DelegateRawFrame OnNewFrame = null;

        public bool Start(VideoCaptureRate videoformat)
        {
            return true; /// don't really have these
        }

        public void Stop()
        {
            
        }

        #endregion
        #region IVideoSink Members


        public void NewRawFrame(byte[] bVideoData)
        {
            /// New uncompressed data, send it
            SendVideo(bVideoData);
        }

        #endregion

     
    }
}
