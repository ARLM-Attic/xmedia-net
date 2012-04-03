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

    public class RTPStream
    {
        public RTPStream(byte nPayload)
        {
            Payload = nPayload;
            SSRC = (uint) ran.Next();
        }
        static Random ran = new Random();

        private uint m_nSSRC = 0;

        public uint SSRC
        {
            get { return m_nSSRC; }
            set { m_nSSRC = value; }
        }
        private IPEndPoint m_objDestinationEndpoint = new IPEndPoint(IPAddress.Any, 0);

        public IPEndPoint DestinationEndpoint
        {
            get { return m_objDestinationEndpoint; }
            set { m_objDestinationEndpoint = value; }
        }

       private IPEndPoint m_objLocalEndpoint = new IPEndPoint(IPAddress.Any, 0);

        public IPEndPoint LocalEndpoint
        {
            get { return m_objLocalEndpoint; }
            set { m_objLocalEndpoint = value; }
        }

        protected SocketServer.UDPSocketClient RTPUDPClient = null;

        protected object SocketLock = new object();

        private bool m_bIsActive =  false;

        protected bool IsActive
        {
          get { return m_bIsActive; }
          set { m_bIsActive = value; }
        }

        private bool m_bIsBound = false;
        public bool IsBound
        {
            get { return m_bIsBound; }
            set { m_bIsBound = value; }
        }

        private int m_nPTime = 20;

        protected int PTime
        {
          get { return m_nPTime; }
          set { m_nPTime = value; }
        }


        protected IMediaTimer SendTimer = null;
        protected object TimerLock = new object();

        public void Bind(IPEndPoint localEp)
        {
            if (IsBound == false)
            {
                LocalEndpoint = localEp;
                RTPUDPClient = new SocketServer.UDPSocketClient(LocalEndpoint);
                RTPUDPClient.Bind();

                LocalEndpoint = RTPUDPClient.s.LocalEndPoint as IPEndPoint;
                IsBound = true;
                RTPUDPClient.OnReceiveMessage += new SocketServer.UDPSocketClient.DelegateReceivePacket(RTPUDPClient_OnReceiveMessage);
                RTPUDPClient.StartReceiving();

            }
        }

        public const ushort StunPort = 3478;
        bool m_bIsWaitingOnSTUN = false;
        public System.Threading.ManualResetEvent STUNResponseWaitHandle = new System.Threading.ManualResetEvent(false);
        public STUNMessage ResponseMessage = null;

        public IPEndPoint PublicIPEndpoint = null;

        public IPEndPoint PerformSTUNRequest(string strStunServer, int nTimeout)
        {
            IPEndPoint epStun = SocketServer.ConnectMgr.GetIPEndpoint(strStunServer, StunPort);
            return PerformSTUNRequest(epStun, nTimeout);
        }

        /// <summary>
        ///  Send out a stun request to discover our IP address transalation
        /// </summary>
        /// <param name="strStunServer"></param>
        /// <returns></returns>
        public IPEndPoint PerformSTUNRequest(IPEndPoint epStun, int nTimeout)
        {
            ResponseMessage = null;
            m_bIsWaitingOnSTUN = true;
            STUNResponseWaitHandle.Reset();

            STUNMessage msgRequest = new STUNMessage();
            msgRequest.Method = StunMethod.Binding;
            msgRequest.Class = StunClass.Request;


            MappedAddressAttribute mattr = new MappedAddressAttribute();
            mattr.IPAddress = LocalEndpoint.Address;
            mattr.Port = (ushort)LocalEndpoint.Port;


            msgRequest.AddAttribute(mattr);

            byte[] bMessage = msgRequest.Bytes;
            this.RTPUDPClient.SendUDP(bMessage, bMessage.Length, epStun);

            bool m_bGotMessage = STUNResponseWaitHandle.WaitOne(nTimeout);
            m_bIsWaitingOnSTUN = false;

            IPEndPoint retep = null;
            if (m_bGotMessage == true)
            {
                foreach (STUNAttributeContainer cont in ResponseMessage.Attributes)
                {
                    if (cont.ParsedAttribute.Type == StunAttributeType.MappedAddress)
                    {
                        
                        MappedAddressAttribute attrib = cont.ParsedAttribute as MappedAddressAttribute;
                        retep = new IPEndPoint(attrib.IPAddress, attrib.Port);
                    }
                }
            }
            return retep;
        }

        public virtual void Start(IPEndPoint destinationEp, int nPacketTime)
        {
            if (IsActive == true)
                return;
            if (IsBound == false)
                throw new Exception("You first must bind local socket by calling Bind()");

            Reset();
            DestinationEndpoint = destinationEp;
            PTime = nPacketTime;

            SendTimer = SocketServer.QuickTimerControllerCPU.CreateTimer(PTime, new SocketServer.DelegateTimerFired(OnTimeToPushPacket), "", null);
            IsActive = true;           
        }

        public virtual void StopSending()
        {
            if (IsActive == false)
                return;

            IsActive = false;
            IsBound = false;
            RTPUDPClient.StopReceiving();
            RTPUDPClient.OnReceiveMessage -= new UDPSocketClient.DelegateReceivePacket(RTPUDPClient_OnReceiveMessage);
            RTPUDPClient = null;
        }

        void OnTimeToPushPacket(IMediaTimer timer)
        {
            SendNextPacket();
        }

        void  RTPUDPClient_OnReceiveMessage(byte[] bData, int nLength, IPEndPoint epfrom, IPEndPoint epthis, DateTime dtReceived)
        {
            /// TODO... if we are an performing ICE, see if this is an ICE packet instead of an RTP one
            /// 

            if (nLength >= 8)
            {
                //0x2112A442
                if ((bData[4] == 0x21) && (bData[5] == 0x12) && (bData[6] == 0xA4) && (bData[7] == 0x42))
                {
                    /// STUN message
                    STUNMessage smsg = new STUNMessage();
                    byte[] bStun = new byte[nLength];
                    Array.Copy(bData, 0, bStun, 0, nLength);
                    smsg.Bytes = bStun;

                    if (m_bIsWaitingOnSTUN == true)
                    {
                        ResponseMessage = smsg;
                        STUNResponseWaitHandle.Set();
                        m_bIsWaitingOnSTUN = false;
                        return;
                    }

                    /// See if we should send a STUN response
                    /// 
                    if (smsg.Class == StunClass.Request)
                    {
                        if (smsg.Method == StunMethod.Binding)
                        {

                            STUNMessage sresp = new STUNMessage();
                            sresp.TransactionId = smsg.TransactionId;
                            sresp.Method = StunMethod.Binding;
                            sresp.Class = StunClass.Success;
                            MappedAddressAttribute attr = new MappedAddressAttribute();
                            attr.Port = (ushort) epfrom.Port;
                            attr.IPAddress = epfrom.Address;
                            attr.Type = StunAttributeType.MappedAddress;
                            attr.AddressFamily = StunAddressFamily.IPv4;
                            sresp.AddAttribute(attr);
                            
                            byte[] bMessage = sresp.Bytes;
                            this.RTPUDPClient.SendUDP(bMessage, bMessage.Length, epfrom);

                        }
                    }
                         

                    return;
                }
            }

 	        RTPPacket packet = RTPPacket.BuildPacket(bData, 0, nLength);
            if (packet.PayloadType == this.Payload)
                    NewRTPPacket(packet, epfrom, epthis, dtReceived);
        }

        protected virtual void SendNextPacket()
        {
        }

        protected virtual void NewRTPPacket(RTPPacket packet, IPEndPoint epfrom, IPEndPoint epthis, DateTime dtReceived)
        {
        }


     
        private byte m_nPayload = 9;

        public byte Payload
        {
            get { return m_nPayload; }
            set { m_nPayload = value; }
        }

        private ushort m_nSequence = 0;

        protected ushort Sequence
        {
            get { return m_nSequence; }
            set { m_nSequence = value; }
        }

        private uint m_nTimeStamp = 0;

        protected uint TimeStamp
        {
            get { return m_nTimeStamp; }
            set { m_nTimeStamp = value; }
        }

        public void Reset()
        {
            m_nSequence = 0;
            m_nTimeStamp = 0;
        }

        /// <summary>
        /// Get the next timestamp.  Audio is usually in ptime*8 increments, video is in offsetfromstart in seconds*90000
        /// </summary>
        /// <returns></returns>
        public virtual uint GetNextTimeStamp()
        {
            uint nRet = m_nTimeStamp;
            m_nTimeStamp += (uint) (this.PTime*8);
            return nRet;
        }

        protected virtual void FormatNextPacket( RTPPacket packet)
        {
            packet.SSRC = m_nSSRC;
            packet.TimeStamp = GetNextTimeStamp();
            packet.PayloadType = this.Payload;
            packet.Marker = (m_nSequence == 0) ? true : false;

            packet.SequenceNumber = m_nSequence++;
        }
    }


}
