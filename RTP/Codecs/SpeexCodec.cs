﻿using System;
using System.Net;
using NSpeex;

namespace RTP
{
    public class SpeexCodec : Codec
    {

        public SpeexCodec(BandMode mode)
            : base("Speex")
        {
            Mode = mode;
            Encoder = new SpeexEncoder(mode);
            Encoder.VBR = false;
            Encoder.Quality = 10;
            Decoder = new SpeexDecoder(mode);
        }

        SpeexEncoder Encoder = null;
        SpeexDecoder Decoder = null;

        BandMode m_eMode = BandMode.Wide;

        public BandMode Mode
        {
            get { return m_eMode; }
            set { m_eMode = value; }
        }

        byte[] bEncodeBuffer = new byte[512];
        short[] bDecodeBuffer = new short[1024];
        
        public override RTPPacket[] Encode(short[] sData)
        {
            if (sData.Length != Encoder.FrameSize)
                throw new Exception("Must provide input data equal to 1 frame size"); // for now, later it can be multiples

            int nRet = 0;

            try
            {
                nRet = Encoder.Encode(sData, 0, sData.Length, bEncodeBuffer, 0, bEncodeBuffer.Length);
            }
            catch (ArgumentNullException)
            { }
            catch (ArgumentOutOfRangeException)
            {
            }

            byte[] bRet = new byte[nRet];
            Array.Copy(bEncodeBuffer, 0, bRet, 0, nRet);

            RTPPacket packet = new RTPPacket();
            packet.PayloadData = bRet;

            return new RTPPacket[] {packet};
        }

        public override short[] DecodeToShorts(RTPPacket packet)
        {
            int nDecoded = 0;
            try
            {
                nDecoded = Decoder.Decode(packet.PayloadData, 0, packet.PayloadData.Length, bDecodeBuffer, 0, false);
            }
            catch (ArgumentNullException)
            { }
            catch (ArgumentOutOfRangeException)
            {
            }

            short[] sRet = new short[nDecoded];
            Array.Copy(bDecodeBuffer, 0, sRet, 0, nDecoded);
            return sRet;
        }

        public override byte[] DecodeToBytes(RTPPacket packet)
        {
            short[] sBytes = DecodeToShorts(packet);
            byte[] bBytes = new byte[sBytes.Length * 2];
            for (int i = 0; i < sBytes.Length; i++)
            {
                // little endian, right?
                bBytes[i] = (byte) (sBytes[i] & 0xFF);
                bBytes[i + 1] = (byte) ((sBytes[i] & 0xFF00) >> 8);
            }

            return bBytes;
        }

    }
}
