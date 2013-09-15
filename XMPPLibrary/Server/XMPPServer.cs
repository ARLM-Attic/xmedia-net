using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

using xmedianet.socketserver;

namespace System.Net.XMPP.Server
{
    /// <summary>
    ///  Main XMPP Server class.  Accepts incoming connections, holds global classes and default logics used by each ServerXMPPClient
    /// </summary>
    public class XMPPServer
    {
        public XMPPServer(XMPPServerConfig config, xmedianet.socketserver.ILogInterface logmgr)
        {
            m_objDomain = new XMPPDomain(this);
            m_objDomain.DomainName = config.DomainName;
            LogManager = logmgr;
            XMPPServerConfig = config;
            ConnectionManger = new ConnectMgr(new XMPPServerClientCreator(this, logmgr));

            lock (LogicLock)
            {
                ClientServices.Add(new MainStreamLogic(this, null));  // stream establishment logic, copied to each tcp session
                ClientServices.Add(Domain); // Message routing logic, copied to each tcp session
                ClientServices.Add(new ServerPresenceLogic(this, null));
            }


            Domain.AddServiceLogic(new ServerServiceDiscoveryLogic(this, null));
            Domain.AddServiceLogic(new ServerRosterLogic(this, null));
            Domain.AddServiceLogic(new ServerPingLogic(this, null));
            Domain.AddServiceLogic(new ServerRegisterLogic(this, null));
            Domain.AddServiceLogic(new ServerPresenceLogic(this, null)); /// Also added to each instance to monitor changes in presence status in addition to domain logic
            Domain.AddServiceLogic(new DomainPubSubLogic(this));

            /// Load our certificate if we have one
            if ((XMPPServerConfig.Certificate != null) && (XMPPServerConfig.Certificate.Length > 0))
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                ServerCertificate = store.Certificates.Find(X509FindType.FindBySubjectName, XMPPServerConfig.Certificate, false)[0];

                //byte[] bPublicKey = ServerCertificate.GetPublicKey();
                //RSAParameters rsaparams = Kishore.X509.Parser.X509PublicKeyParser.GetRSAPublicKeyParameters(bPublicKey);
            }
        }

        private XMPPMessageFactory m_objXMPPMessageFactory = new XMPPMessageFactory();
        public XMPPMessageFactory XMPPMessageFactory
        {
            get { return m_objXMPPMessageFactory; }
        }

        private X509Certificate2 m_objServerCertificate = null;
        public X509Certificate2 ServerCertificate
        {
            get { return m_objServerCertificate; }
            protected set { m_objServerCertificate = value; }
        }

        public List<AuthenticationMechanismLogic> AuthenticationMethods = new List<AuthenticationMechanismLogic>();

        public xmedianet.socketserver.ILogInterface LogManager = null;

        private XMPPServerConfig m_objXMPPServerConfig = null;
        public XMPPServerConfig XMPPServerConfig
        {
            get { return m_objXMPPServerConfig; }
            set { m_objXMPPServerConfig = value; }
        }

        private XMPPDomain m_objDomain = null;
        public XMPPDomain Domain
        {
            get { return m_objDomain; }
            set { m_objDomain = value; }
        }


        protected object m_objClientLock = new object();
        private List<XMPPUserInstance> m_listClients = new List<XMPPUserInstance>();
        protected List<XMPPUserInstance> Clients
        {
            get { return m_listClients; }
            set { m_listClients = value; }
        }

        /// <summary>
        ///  A list of services supported by this XMPP server.  Each of these are added to 
        ///  each individual XMPP connection
        /// </summary>
        protected List<XMPPServerLogic> ClientServices = new List<XMPPServerLogic>();
        protected object LogicLock = new object();

        public ConnectMgr ConnectionManger = null;

        private bool m_bIsStarted = false;
        public bool IsStarted
        {
            get { return m_bIsStarted; }
            protected set { m_bIsStarted = value; }
        }

        protected IPEndPoint m_epListen = null;

        public bool Start()
        {
            if (IsStarted == true)
                return false;
            ///  Start listening for incoming connections
            ///  

            m_epListen = ConnectMgr.GetIPEndpoint(XMPPServerConfig.IPAddress, XMPPServerConfig.Port);
            IsStarted = ConnectionManger.Listen(m_epListen);
            return IsStarted;
        }

        public void Stop()
        {
            if (IsStarted == true)
            {
                ConnectionManger.StopListen(m_epListen.Port);
                
                /// Close all connections
                /// 

                lock (m_objClientLock)
                {
                    foreach (XMPPUserInstance instance in Clients)
                        instance.Disconnect();
                }
            }
        }

        /// <summary>
        /// We have just received a new socket connection, make a new serverxmppclient that will handle all the logic and state information for the stream
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        internal XMPPUserInstance CreateNewUserInstance(XMPPClientConnection connection)
        {
            XMPPUserInstance client = new XMPPUserInstance(this, this.LogManager);
            client.XMPPConnection = connection;

            /// Should lock here, but probably won't add new logics except startup
            foreach (XMPPServerLogic logic in ClientServices)
            {
                client.AddLogic(logic.Clone(client));
            }

            lock (m_objClientLock)
            {
                Clients.Add(client);
            }

            return client;
        }

        internal void UserInstanceDisconnected(XMPPUserInstance client)
        {
            lock (m_objClientLock)
            {
                if (Clients.Contains(client) == true)
                    Clients.Remove(client);
            }

            client.GotDisconnected();

        }
    }
}
