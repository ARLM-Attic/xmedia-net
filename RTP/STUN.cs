/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using xmedianet.socketserver;

namespace RTP
{
 

    /// <summary>
    /// Session Traversal Utilities for NAT (RFC 5389)
    /// This class provides stun client and server capabilities - those need bye ICE to determine which candidate to use
    /// </summary>
    public class STUNClientServer
    {
        public STUNClientServer(IPEndPoint localep, string strSTUNServer)
        {
            LocalEndpoint = localep;
            StunServer = strSTUNServer;
        }

        private string m_strStunServer = "stun.ekiga.net";

        public string StunServer
        {
            get { return m_strStunServer; }
            set { m_strStunServer = value; }
        }
        
        private IPEndPoint m_objLocalEndpoint;

        public IPEndPoint LocalEndpoint
        {
            get { return m_objLocalEndpoint; }
            set { m_objLocalEndpoint = value; }
        }

        public const ushort StunPort = 3478;

        public System.Threading.ManualResetEvent WaitHandle = new System.Threading.ManualResetEvent(false);
        public STUNMessage ResponseMessage = null;

        public void PerformRequest()
        {
            ResponseMessage = null;
            WaitHandle.Reset();
            EndPoint epStun = ConnectMgr.GetIPEndpoint(StunServer, StunPort);

            STUNMessage msgRequest = new STUNMessage();
            msgRequest.Method = StunMethod.Binding;
            msgRequest.Class = StunClass.Request;

            
            MappedAddressAttribute mattr = new MappedAddressAttribute();
            mattr.IPAddress = LocalEndpoint.Address;
            mattr.Port = (ushort) LocalEndpoint.Port;


            msgRequest.AddAttribute(mattr);

            UDPSocketClient client = new UDPSocketClient(LocalEndpoint);
            client.OnReceivePacket += new UDPSocketClient.DelegateReceivePacket(client_OnReceivePacket3);
#if !WINDOWS_PHONE
            client.StartReceiving(true);
#else
            client.StartReceiving();
#endif

            byte [] bMessage = msgRequest.Bytes;
            client.SendUDP(bMessage, bMessage.Length, epStun);
        }

        public IPEndPoint PerformRequest(int nTimeout)
        {
            IPEndPoint retep = null;
            PerformRequest();
            WaitHandle.WaitOne(nTimeout);

            if (ResponseMessage != null)
            {
                foreach (STUNAttributeContainer cont in ResponseMessage.Attributes)
                {
                    if (cont.ParsedAttribute.Type == StunAttributeType.MappedAddress)
                    {

                        MappedAddressAttribute attrib = cont.ParsedAttribute as MappedAddressAttribute;
                        retep = new IPEndPoint(attrib.IPAddress, attrib.Port);
                        break;
                    }
                }
            }

            return retep;
        }

        void client_OnReceivePacket3(byte[] bData, int nLength, IPEndPoint epfrom, IPEndPoint epthis, DateTime dtReceived)
        {
            ResponseMessage = new STUNMessage();
            ResponseMessage.Bytes = bData;
            WaitHandle.Set();
        }

    }
}
