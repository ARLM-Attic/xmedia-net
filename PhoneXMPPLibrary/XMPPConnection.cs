using System;
using System.Net;

using System.Collections.Generic;

namespace System.Net.XMPP
{
    public class XMPPConnection : SocketServer.SocketClient
    {
        public XMPPConnection(XMPPClient client) : base()
        {
            XMPPClient = client;
        }

        XMPPClient XMPPClient = null;
        public void Connect()
        {
            XMPPClient.XMPPState = XMPPState.Connecting;
            if (XMPPClient.XMPPAccount.UseSOCKSProxy == true)
                this.SetSOCKSProxy(XMPPClient.XMPPAccount.SOCKSVersion, XMPPClient.XMPPAccount.ProxyName, XMPPClient.XMPPAccount.ProxyPort, "User");

            ConnectAsync(XMPPClient.Server, XMPPClient.Port);
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
            if (Client.Connected == true)
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


        public delegate void DelegateStanza(XMPPStanza stanza, object objFrom);
        public event DelegateStanza OnStanzaReceived = null;

        internal void FireStanzaReceived(XMPPStanza stanza)
        {
            if (OnStanzaReceived != null)
            {
                OnStanzaReceived(stanza, this);
            }
        }

        protected override void OnConnected(bool bSuccess, string strErrors)
        {
            if (bSuccess == true)
            {
                this.Client.NoDelay = true;
                XMPPClient.XMPPState = XMPPState.Connected;
                XMPPClient.FireConnectAttemptFinished(true);
                System.Diagnostics.Debug.WriteLine(string.Format("Successful TCP connection"));
            }
            else
            {
                XMPPClient.XMPPState = XMPPState.Unknown;
                XMPPClient.FireConnectAttemptFinished(false);
                System.Diagnostics.Debug.WriteLine(string.Format("Failed to connect: {0}", strErrors));
                return;
            }

            if (XMPPClient.UseOldStyleTLS == true)
            {
                StartTLS();
            }


            /// Send stream header if we haven't yet
            XMPPClient.XMPPState = XMPPState.Authenticating;

            OpenStreamStanza open = new OpenStreamStanza(this.XMPPClient);
            string strSend = open.XML;
            
            byte[] bStanza = System.Text.UTF8Encoding.UTF8.GetBytes(strSend);
            this.Send(bStanza);
        }

        bool m_bStartedTLS = false;
        public void StartTLS()
        {
            if ((XMPPClient.UseTLS == true) && (m_bStartedTLS == false) )
            {
                m_bStartedTLS = true;
                this.StartTLS(XMPPClient.Server);
            }
        }


        public override void OnDisconnect(string strReason)
        {
            XMPPClient.XMPPState = XMPPState.Unknown;
            m_bStartedTLS = false;
            System.Diagnostics.Debug.WriteLine(string.Format("TCP disconnected: {0}", strReason));
            XMPPClient.FireDisconnectedFromServer();
            base.OnDisconnect(strReason);
        }

        public override int Send(byte[] bData, int nLength, bool bTransform)
        {
            if (Client.Connected == false)
            {
                XMPPClient.XMPPState = XMPPState.Unknown;
                throw new Exception("XMPP Client is not connected");
            }
            

            string strSend = System.Text.UTF8Encoding.UTF8.GetString(bData, 0, nLength);
            XMPPClient.FireXMLSent(strSend);

            return base.Send(bData, nLength, bTransform);
        }

        XMPPStream XMPPStream = new XMPPStream();
        protected override void OnMessage(byte[] bData)
        {

            string strXML = System.Text.UTF8Encoding.UTF8.GetString(bData, 0, bData.Length);

            
            XMPPClient.FireXMLReceived(strXML);

            XMPPStream.Append(strXML);
            XMPPStream.ParseStanzas(this, XMPPClient);
            XMPPStream.Flush();


            /// Parse out our stanza's
            /// 

        }
        
    }
}
