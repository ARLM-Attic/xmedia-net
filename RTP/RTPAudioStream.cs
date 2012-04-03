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
    public class RTPAudioStream : RTPStream, IAudioSink, IAudioSource
    {
        public RTPAudioStream(byte nPayload, Codec codec)
            : base(nPayload)
        {
            AudioCodec = codec;
        }

        Codec m_objAudioCodec = null;

        public Codec AudioCodec
        {
            get { return m_objAudioCodec; }
            set 
            { 
                m_objAudioCodec = value;
                if (m_objAudioCodec != null)
                {
                    m_nMaxSendBufferSize = AudioCodec.AudioFormat.CalculateNumberOfSamplesForDuration(TimeSpan.FromMilliseconds(100));
                    m_nMaxRecvBufferSize = AudioCodec.AudioFormat.CalculateNumberOfSamplesForDuration(TimeSpan.FromMilliseconds(100));
                }
            }
        }

        public override void Start(IPEndPoint destinationEp, int nPacketTime)
        {
            SendAudioQueue.Clear();
            base.Start(destinationEp, nPacketTime);
        }

        object ReceiveLock = new object();
        protected override void NewRTPPacket(RTPPacket packet, IPEndPoint epfrom, IPEndPoint epthis, DateTime dtReceived)
        {
            lock (ReceiveLock)
            {
                if (AudioCodec == null)
                    return;

                byte[] bNewAudioData = AudioCodec.DecodeToBytes(packet);

                /// TODO... add our jitter buffer class to these projects
                ReceiveAudioQueue.AppendData(bNewAudioData);
            }
        }

        AudioClasses.ByteBuffer SendAudioQueue = new AudioClasses.ByteBuffer();
        AudioClasses.ByteBuffer ReceiveAudioQueue = new AudioClasses.ByteBuffer();

        object SendLock = new object();
        protected override void SendNextPacket()
        {
            if (IsActive == false)
                return;

            if (AudioCodec == null)
                return;


            int nPacketSamples = AudioCodec.AudioFormat.CalculateNumberOfSamplesForDuration(TimeSpan.FromMilliseconds(PTime));
            int nPacketBytes = nPacketSamples * AudioCodec.AudioFormat.BytesPerSample;
            if (SendAudioQueue.Size < nPacketBytes)
                return;


            byte[] bUncompressedAudio = SendAudioQueue.GetNSamples(nPacketBytes);
            short[] sUncompressedAudio = AudioClasses.Utils.ConvertByteArrayToShortArrayLittleEndian(bUncompressedAudio);

            RTPPacket[] packets = new RTPPacket[] { };
            lock (SendLock)
            {
                packets = AudioCodec.Encode(sUncompressedAudio);
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

        private int m_nMaxSendBufferSize = AudioClasses.AudioFormat.SixteenBySixteenThousandMono.CalculateNumberOfSamplesForDuration(TimeSpan.FromMilliseconds(100));
        public int MaxSendBufferSize
        {
            get { return m_nMaxSendBufferSize; }
            set { m_nMaxSendBufferSize = value; }
        }

        private int m_nMaxRecvBufferSize = AudioClasses.AudioFormat.SixteenBySixteenThousandMono.CalculateNumberOfSamplesForDuration(TimeSpan.FromMilliseconds(100));
        public int MaxRecvBufferSize
        {
            get { return m_nMaxRecvBufferSize; }
            set { m_nMaxRecvBufferSize = value; }
        }

        #region IAudioSink Members

        /// <summary>
        /// Push a sample to this filter's outgoing queue.
        /// </summary>
        /// <param name="sample"></param>
        public void PushSample(MediaSample sample)
        {
            if (AudioCodec == null)
                return;

            if ((sample.AudioFormat.AudioSamplingRate == AudioSamplingRate.sr16000) && (AudioCodec.AudioFormat.AudioSamplingRate == AudioSamplingRate.sr8000) )
            {
                /// Downsample the data
                /// 
                short [] sData = Utils.Resample16000To8000(sample.GetShortData());
                SendAudioQueue.AppendData(Utils.ConvertShortArrayToByteArray(sData));
            }
            else if ((sample.AudioFormat.AudioSamplingRate == AudioSamplingRate.sr16000) && (AudioCodec.AudioFormat.AudioSamplingRate == AudioSamplingRate.sr8000))
            {
                /// Upsample the data.  This shouldn't happen because our incoming data should always be higher or equal quality
                /// 
                short[] sData = Utils.Resample8000To16000(sample.GetShortData());
                SendAudioQueue.AppendData(Utils.ConvertShortArrayToByteArray(sData));
            }
            else
            {
                SendAudioQueue.AppendData(sample.Data);
            }
            if (SendAudioQueue.Size > MaxSendBufferSize)
            {
                SendAudioQueue.GetNSamples(SendAudioQueue.Size - MaxSendBufferSize);
            }
        }

        bool m_bIsDeafened = false;
        public bool IsSinkActive
        {
            get
            {
                return !m_bIsDeafened;
            }
            set
            {
                m_bIsDeafened = value;
            }
        }

        public bool Deafened
        {
            get
            {
                return m_bIsDeafened;
            }
            set
            {
                if (m_bIsDeafened != value)
                {
                    m_bIsDeafened = value;
                }
            }
        }

        double m_fSinkAmplitudeMultiplier = 1.0f;
        public double SinkAmplitudeMultiplier
        {
            get { return m_fSinkAmplitudeMultiplier; }
            set { m_fSinkAmplitudeMultiplier = value; }
        }


        #endregion

        #region IAudioSource Members

        public MediaSample PullSample(AudioFormat format, TimeSpan tsDuration)
        {

            if (AudioCodec == null)
                return null;

            int nSamples = AudioCodec.AudioFormat.CalculateNumberOfSamplesForDuration(tsDuration);
            int nBytesNeeded = nSamples * AudioCodec.AudioFormat.BytesPerSample;

            /// Greater than 4 samples in our buffer, remove some
            if (ReceiveAudioQueue.Size > nBytesNeeded * AudioCodec.AudioFormat.BytesPerSample)
                ReceiveAudioQueue.GetNSamples(ReceiveAudioQueue.Size - nBytesNeeded*2);

            if (ReceiveAudioQueue.Size >= nBytesNeeded)
            {
                byte [] bAudioData = ReceiveAudioQueue.GetNSamples(nBytesNeeded);
                MediaSample newsample = null;
                if ((format.AudioSamplingRate == AudioSamplingRate.sr16000) && (AudioCodec.AudioFormat.AudioSamplingRate == AudioSamplingRate.sr8000))
                {
                    /// Upsample the data.
                    /// 
                    short[] sData = Utils.Resample8000To16000(Utils.ConvertByteArrayToShortArrayLittleEndian(bAudioData));
                    bAudioData = Utils.ConvertShortArrayToByteArray(sData);
                }
                else if ((format.AudioSamplingRate == AudioSamplingRate.sr16000) && (AudioCodec.AudioFormat.AudioSamplingRate == AudioSamplingRate.sr8000))
                {
                    /// Downsample the data
                    /// 
                    short[] sData = Utils.Resample16000To8000(Utils.ConvertByteArrayToShortArrayLittleEndian(bAudioData));
                    bAudioData = Utils.ConvertShortArrayToByteArray(sData);
                }
                newsample = new MediaSample(bAudioData, format);
               

                return newsample;
            }

            return null;
        }

        public bool m_bMuted = false;
        public bool IsSourceActive
        {
            get
            {
                return !m_bMuted;
            }
            set
            {
                if (m_bMuted != value)
                {
                    m_bMuted = !value;
                }
            }
        }

        public bool Muted
        {
            get
            {
                return m_bMuted;
            }
            set
            {
                if (m_bMuted != value)
                {
                    m_bMuted = value;
                }
            }
        }

        double m_fSourceAmplitudeMultiplier = 1.0f;
        public double SourceAmplitudeMultiplier
        {
            get { return m_fSourceAmplitudeMultiplier; }
            set { m_fSourceAmplitudeMultiplier = value; }
        }
        

        #endregion
    }
}
