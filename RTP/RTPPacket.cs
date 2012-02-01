﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTP
{
    public class RTPPacket
    {
        public RTPPacket()
        {
        }

        public override string ToString()
        {
            return string.Format("RTPPacket Payload: {0}, Marker: {1}, Sequence: {2}, TimeStamp: {3}",
                PayloadType, Marker, SequenceNumber, TimeStamp);
        }
        /// <summary>
        /// The time the packet should be sent at... This is may be used by client applications
        /// to determine when to send this packet or when it was received
        /// </summary>
        public DateTime PacketTransmitTime = DateTime.MinValue;

        public byte VersionPaddingHeaderCRSCCountByte = 0x80;
        public byte MarkerPayloadByte = 0;

        /// <summary>
        /// RTP Version (2 bits)
        /// </summary>
        public int Version
        {
            get
            {
                return (VersionPaddingHeaderCRSCCountByte & 0xC0)>>6;
            }
            set
            {
                VersionPaddingHeaderCRSCCountByte = (byte) ((VersionPaddingHeaderCRSCCountByte & 0x3F) | ((value & 0x03)<<6)  );
            }
        }

        /// <summary>
        /// RTP Padding bit
        /// </summary>
        public bool Padding
        {
            get
            {
                return ((VersionPaddingHeaderCRSCCountByte & 0x20) > 0);
            }
            set
            {
                if (value == true)
                   VersionPaddingHeaderCRSCCountByte |= 0x20;
                else
                    VersionPaddingHeaderCRSCCountByte &= 0xDF;
            }
        }

        /// <summary>
        /// RTP Extension bit
        /// </summary>
        public bool Extension
        {
            get
            {
                return ((VersionPaddingHeaderCRSCCountByte & 0x10) > 0);
            }
            set
            {
                if (value == true)
                    VersionPaddingHeaderCRSCCountByte |= 0x10;
                else
                    VersionPaddingHeaderCRSCCountByte &= 0xEF;
            }
        }

        public int CRSCCount
        {
            get
            {
                return (VersionPaddingHeaderCRSCCountByte & 0x0F) ;
            }
            set
            {
                VersionPaddingHeaderCRSCCountByte = (byte)((VersionPaddingHeaderCRSCCountByte & 0xF0) | (value & 0x0F));
            }
        }



        /// <summary>
        /// RTP Marker bit
        /// </summary>
        public bool Marker
        {
            get
            {
                return ((MarkerPayloadByte & 0x80) > 0);
            }
            set
            {
                if (value == true)
                    MarkerPayloadByte |= 0x80;
                else
                    MarkerPayloadByte &= 0x7F;
            }
        }


        /// <summary>
        ///  RTP payload types, see the public PayloadTypes enum ( 7 bits)
        /// </summary>
        public int PayloadType
        {
            get
            {
                return (MarkerPayloadByte & 0x7F);
            }
            set
            {
                MarkerPayloadByte = (byte) ( (MarkerPayloadByte & 0x80) | (value & 0x7F) );
            }
        }

        /// <summary>
        /// RTP sequence number, incremented by 1 for each packet (16 bit)
        /// </summary>
        public ushort SequenceNumber = 0;

        /// <summary>
        /// Sampling instance of the first octet in the RTP data packet. (32 bits)
        /// Intially random, incremented by the number of samples for u-law and a-law only)
        /// </summary>
        public uint TimeStamp = 0;

        /// <summary>
        /// Synchornization source Identfier (32 bits)
        /// a random identifier of this source
        /// </summary>
        public uint SSRC = 0;

        /// <summary>
        /// A list of contributing source Identifiers, 0 to 15 items each 32 bits
        /// </summary>
        public List<uint> CSRCList = new List<uint>();

        /// <summary>
        /// RTP extension data, if the extension bit is set
        /// 16 bit profile data, 16 bit length
        /// length header extension
        /// </summary>
        public byte[] ExtensionData = null;

        public byte[] PayloadData = null;

        public static RTPPacket BuildPacket(byte[] bRecvData)
        {
            RTPPacket packet = new RTPPacket();
	        if (bRecvData.Length < 12)
		        return null;

            packet.VersionPaddingHeaderCRSCCountByte = bRecvData[0];
            packet.MarkerPayloadByte = bRecvData[1];
            packet.SequenceNumber = (ushort) ( (bRecvData[2]<<8) | bRecvData[3]);
            packet.TimeStamp = (uint) ( (bRecvData[4]<<24) | (bRecvData[5]<<16) | (bRecvData[6]<<8) | (bRecvData[7]) );
            packet.SSRC = (uint) ( (bRecvData[8]<<24) | (bRecvData[9]<<16) | (bRecvData[10]<<8) | (bRecvData[11]) );

            int nByteAt = 12;
	        for (int i=0; i<packet.CRSCCount; i++)
	        {
                if (bRecvData.Length < (nByteAt+4))
                    return null;

                packet.CSRCList.Add((uint) ( (bRecvData[nByteAt++]<<24) | (bRecvData[nByteAt++]<<16) | (bRecvData[nByteAt++]<<8) | (bRecvData[nByteAt++]) ) );
	        }

	        int nExtensionLen = 0;
	        /// Read in extension data, if there
	        if (packet.Extension == true)
	        {
		        /// ignore this for now, just copy to the data
		        //nExtensionLen = pRTPHeader.
	        }


	        int nDataStartAt = 12 + 4*packet.CRSCCount + nExtensionLen;
	        int nBytesLeft = bRecvData.Length - nDataStartAt;

	        /// Finally, let's find out what is next and add that as data
	        if (nBytesLeft > 0)
	        {
		        packet.PayloadData = new byte[nBytesLeft];
		        Array.Copy(bRecvData, nDataStartAt, packet.PayloadData, 0, nBytesLeft);
	        }

	        return packet;
        }

        public byte[] GetBytes()
        {
	        int ExtensionLength = 0;
	        if (Extension == true)
		        if (ExtensionData != null)
			        ExtensionLength = ExtensionData.Length;

	        int PayloadLength = 0;
	        if (PayloadData != null)
	        {
		        PayloadLength = PayloadData.Length;
	        }

	        int nBytesNeeded = 12 + CSRCList.Count*4 + ExtensionLength + PayloadLength;
	        byte [] bRecvData = new byte[nBytesNeeded]; // Managed destination array



            bRecvData[0] = VersionPaddingHeaderCRSCCountByte;
            bRecvData[1] = MarkerPayloadByte;
            bRecvData[2] = (byte) ((SequenceNumber>>8)&0xFF);
            bRecvData[3] = (byte) (SequenceNumber&0xFF);

            bRecvData[4] = (byte) ((TimeStamp>>24)&0xFF);
            bRecvData[5] = (byte) ((TimeStamp>>16)&0xFF);
            bRecvData[6] = (byte) ((TimeStamp>>8)&0xFF);
            bRecvData[7] = (byte) ((TimeStamp&0xFF));

            bRecvData[8] = (byte) ((SSRC>>24)&0xFF);
            bRecvData[9] = (byte) ((SSRC>>16)&0xFF);
            bRecvData[10] = (byte) ((SSRC>>8)&0xFF);
            bRecvData[11] = (byte) ((SSRC&0xFF));
            
            int nByteAt = 12;
            foreach (uint nextCRSC in CSRCList)
	        {
                bRecvData[nByteAt++] = (byte) ((nextCRSC>>24)&0xFF);
                bRecvData[nByteAt++] = (byte) ((nextCRSC>>16)&0xFF);
                bRecvData[nByteAt++] = (byte) ((nextCRSC>>8)&0xFF);
                bRecvData[nByteAt++] = (byte) ((nextCRSC&0xFF));
	        }


	        /// Copy extenstion data
	        if (ExtensionData != null)
	        {
		        if (ExtensionLength > 0)
		        {
			        Array.Copy(ExtensionData, 0, bRecvData, nByteAt, ExtensionLength);
			        nByteAt +=  ExtensionLength;
		        }
	        }

	        /// Finally, copy actual RTP data
	        if (PayloadData != null)
	        {
		        if (PayloadLength > 0)
		        {
			        Array.Copy(PayloadData, 0, bRecvData, nByteAt, PayloadLength);
                    nByteAt += PayloadLength;
		        }
	        }

           return bRecvData;
        }

        /// <summary>
        /// sets this packet as a notify packet.  When this packet is sent out over the
        /// wire, the RTP session class should fire an event telling the host that this
        /// packet was sent
        /// </summary>
        /// <param name="strNotifyName"></param>
        public void SetAsNotifyPacket(string strNotifyName)
        {
            m_bNotify = true;
            m_strNotify = strNotifyName;
        }

        protected bool m_bNotify = false;
        public bool IsNotify
        {
            get
            {
                return m_bNotify;
            }
        }
        protected string m_strNotify = "";
        public string NotifyString
        {
            get
            {
                return m_strNotify;
            }
        }

        protected System.Net.IPEndPoint m_opjFromEndpoint = null;
        /// <summary>
        /// The UDP endpoint this packet was sent from
        /// </summary>
        public System.Net.IPEndPoint FromEndpoint
        {
            get
            {
                return m_opjFromEndpoint;
            }
            set
            {
                m_opjFromEndpoint = value;
            }
        }

        protected System.Net.IPEndPoint m_opjToEndpoint = null;
        /// <summary>
        /// The UDP endpoint this packet was sent to
        /// </summary>
        public System.Net.IPEndPoint ToEndpoint
        {
            get
            {
                return m_opjToEndpoint;
            }
            set
            {
                m_opjToEndpoint = value;
            }
        }

    }
}
