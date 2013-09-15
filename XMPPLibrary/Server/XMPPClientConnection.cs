/// Copyright (c) 2013 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;

using System.Collections.Generic;
using xmedianet.socketserver;

namespace System.Net.XMPP.Server
{
    public class XMPPServerClientCreator : xmedianet.socketserver.SocketCreator
    {
        public XMPPServerClientCreator(XMPPServer server, xmedianet.socketserver.ILogInterface logmgr)
            : base()
        {
            Server = server;
            LogManager = logmgr;
        }

        protected XMPPServer Server = null;
        protected xmedianet.socketserver.ILogInterface LogManager = null;

        public override SocketClient AcceptSocket(Sockets.Socket s, ConnectMgr cmgr)
        {
            XMPPClientConnection con = new XMPPClientConnection(s, cmgr, Server, LogManager);
           // con.DoAsyncRead();
            return con;
        }

        public override SocketClient CreateSocket(Sockets.Socket s, ConnectMgr cmgr)
        {
            XMPPClientConnection con = new XMPPClientConnection(s, cmgr, Server, LogManager);
            //con.DoAsyncRead();
            return con;
        }
    }

    public class XMPPClientConnection : xmedianet.socketserver.SocketClient
    {
        public XMPPClientConnection(Sockets.Socket s, ConnectMgr cmgr, XMPPServer server, ILogInterface loginterface)
            : base(s, cmgr)
        {
            this.m_Logger = loginterface;
            Server = server;
            XMPPClient = Server.CreateNewUserInstance(this);

            this.Client.NoDelay = true;
            XMPPClient.XMPPState = XMPPState.Connected;
        }

        public new bool Connected
        {
            get
            {
                if (Client == null)
                    return false;
                return Client.Connected;
            }
        }

        protected XMPPServer Server = null;
        protected XMPPUserInstance XMPPClient = null;

        public void GracefulDisconnect()
        {
            XMPPClient.XMPPState = XMPPState.Unknown;
            if (Client.Connected == true)
            {
                Send("</stream>");
            }
        }

        public override bool Disconnect()
        {
            XMPPClient.XMPPState = XMPPState.Unknown;
            if ( (Client != null) && (Client.Connected == true))
            {
                Send("</stream>");
                bool bRet = base.Disconnect();

                return bRet;
            }
            return false;
        }

        public virtual int SendStanza(XMPPStanza stanza)
        {
            string strSend = stanza.XML;
            byte[] bStanza = System.Text.UTF8Encoding.UTF8.GetBytes(strSend);
            return this.Send(bStanza);
        }



        bool m_bStartedTLS = false;
        public void StartTLS()
        {
            //if ((XMPPClient.UseTLS == true) && (m_bStartedTLS == false) )
            //{
            //    m_bStartedTLS = true;
            //    this.StartTLS(XMPPClient.Server);
            //}
        }


        public override void OnDisconnect(string strReason)
        {
            XMPPClient.XMPPState = XMPPState.Unknown;
            m_bStartedTLS = false;
            System.Diagnostics.Debug.WriteLine(string.Format("TCP disconnected: {0}", strReason));

            Server.UserInstanceDisconnected(XMPPClient);

            base.OnDisconnect(strReason);
        }

        public override int Send(byte[] bData, int nLength, bool bTransform)
        {
            int nRet = base.Send(bData, nLength, bTransform);

            if ( (bTransform == true) && (nRet == nLength) )
            {
                string strSend = System.Text.UTF8Encoding.UTF8.GetString(bData, 0, nLength);
                XMPPClient.FireXMLSent(strSend);
            }

            return nRet;
        }

        XMPPServerStream XMPPStream = new XMPPServerStream();
        protected override void OnMessage(byte[] bData)
        {

            string strXML = System.Text.UTF8Encoding.UTF8.GetString(bData, 0, bData.Length);

            
            XMPPClient.FireXMLReceived(strXML);

            XMPPStream.Append(strXML);
            XMPPStream.ParseStanzas(this, XMPPClient);
            //XMPPStream.Flush();


            /// Parse out our stanza's
            /// 

        }
        
    }
}
