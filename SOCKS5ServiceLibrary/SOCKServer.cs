﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SocketServer;
using System.Net;
using System.Net.Sockets;


namespace SOCKS5ServiceLibrary
{

///  No server components in windows phone, just client for now until we need it
#if !WINDOWS_PHONE

    public enum ServerSessionState
    {
        None,
        WaitingForMethodSelections,
        WaitingForSocksRequestMessage,
        JustExisting,
        Forwarding
    }
    public class SOCKSServerSession : SocketClient
    {
        public SOCKSServerSession(Socket s, SOCKServer parent) 
        {
            Init(s, null);
            Parent = parent;
            ConnectClient = new SocketClient();
            ConnectClient.DisconnectHandler += new SocketEventHandler(ConnectClient_DisconnectHandler);
            ConnectClient.ReceiveHandlerBytes += new SocketReceiveHandler(ConnectClient_ReceiveHandlerBytes);
        }

        void ConnectClient_ReceiveHandlerBytes(SocketClient client, byte[] bData, int nLength)
        {
            if (this.Connected == true)
            {
                Send(bData, nLength);
            }
        }

        void ConnectClient_DisconnectHandler(object sender, EventArgs e)
        {
            if (this.Connected == true)
            {
                this.Disconnect();
            }
        }

        public override void OnDisconnect(string strReason)
        {
            base.OnDisconnect(strReason);
            try
            {
                if ((this.ConnectClient != null) && (this.ConnectClient.Connected == true))
                {
                    this.ConnectClient.Disconnect();
                    this.ConnectClient = null;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                this.ConnectClient = null;
            }
        }

        


        SOCKServer Parent = null;
        SocketClient ConnectClient = null;


        public void Start()
        {
            ServerSessionState = SOCKS5ServiceLibrary.ServerSessionState.WaitingForMethodSelections;
            DoAsyncRead();
        }

        ByteBuffer ReceiveBuffer = new ByteBuffer();

        private ServerSessionState m_eServerSessionState = ServerSessionState.None;

        public ServerSessionState ServerSessionState
        {
            get { return m_eServerSessionState; }
            set { m_eServerSessionState = value; }
        }

        protected override void OnRecvData(byte[] bData, int nLen)
        {
            if (nLen == 0)
            {
                OnDisconnect("Normal closing");
                return;
            }

            //Console.WriteLine(string.Format("<-- {0}", ByteSize.ByteUtils.HexStringFromByte(bData, true)));

            if (ServerSessionState == SOCKS5ServiceLibrary.ServerSessionState.Forwarding)
            {
                try
                {
                    ConnectClient.Send(bData, nLen);

                    DoAsyncRead();
                }
                catch (Exception ex)
                { }

                return;
            }

            ReceiveBuffer.AppendData(bData, 0, nLen);

            if (ServerSessionState == SOCKS5ServiceLibrary.ServerSessionState.JustExisting)
            {
                DoAsyncRead(); // go read some more
                return;
            }

            byte[] bCurData = ReceiveBuffer.PeekAllSamples();

            if (bCurData.Length <= 0)
            {
                DoAsyncRead();
                return;
            }

            if (ServerSessionState == SOCKS5ServiceLibrary.ServerSessionState.WaitingForMethodSelections)
            {
                int nVersion = bCurData[0];

                if (nVersion == 5)
                {
                    MethodSelectionsMessage msg = new MethodSelectionsMessage();
                    int nRead = msg.ReadFromBytes(bData, 0);
                    if (nRead > 0)
                    {
                        ReceiveBuffer.GetNSamples(nRead);

                        /// Determine which method we support
                        /// 
                        bool bCanDoNoAuth = false;
                        foreach (SockMethod method in msg.Methods)
                        {
                            if (method == SockMethod.NoAuthenticationRequired)
                            {
                                bCanDoNoAuth = true;
                                break;
                            }
                        }

                        if (bCanDoNoAuth == false)
                        {
                            MethodSelectedMessage retmsg = new MethodSelectedMessage();
                            retmsg.Version = 5;
                            retmsg.SockMethod = SockMethod.NoAcceptableMethods;
                            this.Send(retmsg.GetBytes());
                            this.Disconnect();
                            return;
                        }
                        else
                        {
                            ServerSessionState = SOCKS5ServiceLibrary.ServerSessionState.WaitingForSocksRequestMessage;
                            MethodSelectedMessage retmsg = new MethodSelectedMessage();
                            retmsg.Version = msg.Version;
                            retmsg.SockMethod = SockMethod.NoAuthenticationRequired;
                            this.Send(retmsg.GetBytes());
                        }
                    }
                }
                else if (nVersion == 4)
                {
                    MethodSelectionsVersionFourMessage msg = new MethodSelectionsVersionFourMessage();
                    int nRead = msg.ReadFromBytes(bData, 0);
                    if (nRead > 0)
                    {
                        ReceiveBuffer.GetNSamples(nRead);


                        MethodSelectedVersionFourMessage reply = new MethodSelectedVersionFourMessage();
                        /// See what the man wants.  It appears that mozilla immediately starts sending data if we return success here, so let's do it
                        /// 
                        this.ServerSessionState = SOCKS5ServiceLibrary.ServerSessionState.Forwarding;
                        /// Let's try to connect
                        /// 
                        bool bConnected = false;
                        if (msg.DomainName != null)
                            bConnected = ConnectClient.Connect(msg.DomainName, msg.DestinationPort, true);
                        else
                            bConnected = ConnectClient.Connect(msg.DestIPAddress.ToString(), msg.DestinationPort, true);

                        if (bConnected == false)
                        {
                            reply.SOCKS4Status = SOCKS4Status.RequestRejected;
                            Send(reply.GetBytes());
                            Disconnect();
                        }
                        else
                        {
                            reply.SOCKS4Status = SOCKS4Status.RequestGranted;
                            Send(reply.GetBytes());
                        }

                        

                    }
                }
                else
                {
                    Console.WriteLine("Version {0} not supported", nVersion);
                    MethodSelectedMessage retmsg = new MethodSelectedMessage();
                    retmsg.Version = 5;
                    retmsg.SockMethod = SockMethod.NoAcceptableMethods;
                    this.Send(retmsg.GetBytes());
                    this.Disconnect();
                    return;
                }
            }
            else if (ServerSessionState == ServerSessionState.WaitingForSocksRequestMessage)
            {
                /// Read in our SocksRequestMessage
                /// 
                SocksRequestMessage reqmsg = new SocksRequestMessage();
                int nRead = reqmsg.ReadFromBytes(bData, 0);
                if (nRead > 0)
                {
                    ReceiveBuffer.GetNSamples(nRead);

                    if (reqmsg.Version != 0x05)
                        Console.WriteLine("No version 5, client wants version: {0}", reqmsg.Version);


                    //Parent.HandleRequest(reqmsg, this);
                    if (reqmsg.SOCKSCommand == SOCKSCommand.Connect)
                    {
                        /// See what the man wants.  It appears that mozilla immediately starts sending data if we return success here, so let's do it
                        /// 
                        this.ServerSessionState = SOCKS5ServiceLibrary.ServerSessionState.Forwarding;
                        /// Let's try to connect
                        /// 
                        bool bConnected = false;
                        if (reqmsg.AddressType == AddressType.DomainName)
                            bConnected = ConnectClient.Connect(reqmsg.DestinationDomain, reqmsg.DestinationPort, true);
                        else
                            bConnected = ConnectClient.Connect(reqmsg.DestinationAddress.ToString(), reqmsg.DestinationPort, true);

                        SocksReplyMessage reply = new SocksReplyMessage();

                        if (bConnected == false)
                        {
                            reply.SOCKSReply = SOCKSReply.ConnectionRefused;
                        }
                        else
                        {
                            reply.SOCKSReply = SOCKSReply.Succeeded;
                        }

                        Send(reply.GetBytes());
                    }
                    else
                    {
                        SocksReplyMessage reply = new SocksReplyMessage();
                        reply.SOCKSReply = SOCKSReply.CommandNotSupported;
                        reply.AddressType = AddressType.IPV4;
                        Send(reply.GetBytes());
                    }
                }
            }


            DoAsyncRead(); // go read some more
        }


        public override int Send(byte[] bData)
        {
            int nRet = 0;
            try
            {
                //Console.WriteLine(string.Format("--> {0}", ByteSize.ByteUtils.HexStringFromByte(bData, true)));
                nRet = base.Send(bData);
            }
            catch (Exception ex)
            {
                this.Disconnect();
            }
            return nRet;
        }

        public override int Send(byte[] bData, int nLength)
        {
            int nRet = 0;
            try
            {
                //Console.WriteLine(string.Format("--> {0}", ByteSize.ByteUtils.HexStringFromByte(bData, true)));
                nRet = base.Send(bData, nLength);
            }
            catch (Exception ex)
            {
                this.Disconnect();
            }
            return nRet;
        }

    }



    public class ListenPortToRemoteEndpointForwardService
    {
        public ListenPortToRemoteEndpointForwardService(SOCKServer parent)
        {
            Parent = parent;
            RemoteClient = new SocketClient();
            Listener = new SocketListener();
            Listener.OnNewConnection += new SocketListener.DelegateNewConnectedSocket(Listener_OnNewConnection);
            RemoteClient.ReceiveHandlerBytes += new SocketClient.SocketReceiveHandler(RemoteClient_ReceiveHandlerBytes);
            RemoteClient.DisconnectHandler += new SocketClient.SocketEventHandler(RemoteClient_DisconnectHandler);
        }

      
        SOCKServer Parent = null;
        SocketClient RemoteClient = null;
        SocketClient IncomingClient = null;
        SocketListener Listener = null;

        public SocksReplyMessage Start(string strRemoteHost, int nRemotePort)
        {
            SocksReplyMessage reply = new SocksReplyMessage();
            reply.AddressType = AddressType.IPV4;

            bool bAccept = Listener.EnableAccept(0);
            if (bAccept == false)
            {
                reply.SOCKSReply = SOCKSReply.GeneralServerFailure;
                return reply;
            }

            IPEndPoint BoundEp = Listener.ListeningSocket.LocalEndPoint as IPEndPoint;

            bool bConnected = RemoteClient.Connect(strRemoteHost, nRemotePort, false);
            if (bConnected == false)
            {
                reply.SOCKSReply = SOCKSReply.ConnectionRefused;
                Listener.Close();
                Listener = null;
                return reply;
            }

            reply.SOCKSReply = SOCKSReply.Succeeded;
            reply.BindAddress = BoundEp.Address;
            reply.BindPort = (ushort) BoundEp.Port;
            reply.AddressType = AddressType.IPV4;

            return reply;
        }

        void Listener_OnNewConnection(Socket s)
        {
            IncomingClient = new SocketClient(s, null);
            IncomingClient.ReceiveHandlerBytes += new SocketClient.SocketReceiveHandler(IncomingClient_ReceiveHandlerBytes);
            IncomingClient.DisconnectHandler += new SocketClient.SocketEventHandler(IncomingClient_DisconnectHandler);
            IncomingClient.DoAsyncRead();
            RemoteClient.DoAsyncRead();
        }

    

        void IncomingClient_ReceiveHandlerBytes(SocketClient client, byte[] bData, int nLength)
        {
            if ((RemoteClient != null) && (RemoteClient.Connected == true) )
                RemoteClient.Send(bData, nLength);
        }
        
        void RemoteClient_ReceiveHandlerBytes(SocketClient client, byte[] bData, int nLength)
        {
            if ((IncomingClient != null) && (IncomingClient.Connected == true))
                IncomingClient.Send(bData, nLength);
        }

        void IncomingClient_DisconnectHandler(object sender, EventArgs e)
        {
            Parent.RemoveService(this);
            if (RemoteClient != null)
            {
                RemoteClient.Disconnect();
                RemoteClient = null;
            }
            if (Listener != null)
            {
                Listener.Close();
                Listener = null;
            }
        }

        void RemoteClient_DisconnectHandler(object sender, EventArgs e)
        {
            if (IncomingClient != null)
            {
                IncomingClient.Disconnect();
                IncomingClient = null;
            }
        }

     
    }




    public class SOCKServer
    {
        public SOCKServer()
        {
            Listener.OnNewConnection += new SocketListener.DelegateNewConnectedSocket(Listener_OnNewConnection);
        }


        private int m_nPort = 8080;

        public int Port
        {
            get { return m_nPort; }
            set { m_nPort = value; }
        }

        SocketListener Listener = new SocketListener();

        public void Start()
        {
            Console.WriteLine("SOCKS server listening on port {0}", Port);
            Listener.EnableAccept(Port);
        }

        public List<SOCKSServerSession> Sessions = new List<SOCKSServerSession>();
        public object SessionLock = new object();


        void Listener_OnNewConnection(System.Net.Sockets.Socket s)
        {
            Console.WriteLine("Session Connecting: {0}", s);
            SOCKSServerSession session = new SOCKSServerSession(s, this);
            lock (SessionLock)
            {
                Sessions.Add(session);
            }

            session.DisconnectHandler += new SocketClient.SocketEventHandler(session_DisconnectHandler);
            session.Start();
        }

        void session_DisconnectHandler(object sender, EventArgs e)
        {
            Console.WriteLine("Session Disconnecting: {0}", sender);
            SOCKSServerSession session = sender as SOCKSServerSession;
            lock (SessionLock)
            {
                if (Sessions.Contains(session) == true)
                    Sessions.Remove(session);
            }
        }


        public List<ListenPortToRemoteEndpointForwardService> ConnectServices = new List<ListenPortToRemoteEndpointForwardService>();
        public object LockConnectServices = new object();

        public void RemoveService(ListenPortToRemoteEndpointForwardService service)
        {
            lock (LockConnectServices)
            {
                if (ConnectServices.Contains(service) == true)
                    ConnectServices.Remove(service);
            }
        }

        public void HandleRequest(SocksRequestMessage reqmsg, SOCKSServerSession session)
        {
           /// See what the man wants.. The client above handles connect request, not us
            /// 
            if (reqmsg.SOCKSCommand == SOCKSCommand.Connect)
            {
                /// Let's try to connect
                /// 
                ListenPortToRemoteEndpointForwardService service = new ListenPortToRemoteEndpointForwardService(this);
                IPEndPoint BindPort = null;
                SocksReplyMessage reply = null;
                if (reqmsg.AddressType == AddressType.DomainName)
                    reply = service.Start(reqmsg.DestinationDomain, reqmsg.DestinationPort);
                else
                    reply = service.Start(reqmsg.DestinationAddress.ToString(), reqmsg.DestinationPort);

                reply.BindAddress = IPAddress.Parse("127.0.0.1"); 

                session.Send(reply.GetBytes());
            }
            else
            {
                SocksReplyMessage reply = new SocksReplyMessage();
                reply.SOCKSReply = SOCKSReply.CommandNotSupported;
                reply.AddressType = AddressType.IPV4;
                session.Send(reply.GetBytes());
            }
        }

       

    }

#endif
}
