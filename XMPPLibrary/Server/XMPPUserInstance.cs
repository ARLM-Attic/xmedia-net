/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;

using xmedianet.socketserver;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using System.Runtime.Serialization;
using System.Collections.ObjectModel;

using System.Net.Security;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;


#if !MONO
using System.Windows.Threading;
#endif

namespace System.Net.XMPP.Server
{
    /// <summary>
    ///  A user instance and it's connection
    /// </summary>
    public class XMPPUserInstance 
    {

        public XMPPUserInstance(XMPPServer server, xmedianet.socketserver.ILogInterface loginterface)
        {
            LogInterface = loginterface;
            Server = server;
           

        }

        protected void LogMessage(string strMsg, params object [] msgparams)
        {
            if (LogInterface != null)
            {
                LogInterface.LogMessage(this.JID, MessageImportance.Medium, strMsg, msgparams);
            }
            else
                System.Diagnostics.Debug.WriteLine(strMsg, msgparams);
        }

        protected XMPPServer Server = null;
        protected ILogInterface LogInterface = null;

        private XMPPUser m_objUser = null;
        public XMPPUser User
        {
            get { return m_objUser; }
            protected set { m_objUser = value; }
        }

        public void Disconnect()
        {
            XMPPConnection.Disconnect();
            Cleanup();
        }

        public void GotDisconnected()
        {
            Cleanup();
        }

        void Cleanup()
        {
           

            m_bIsAuthenticated = false;
            this.XMPPState = XMPP.XMPPState.Unknown;
            if (this.PresenceStatus.PresenceType != PresenceType.unavailable)
            {
                this.PresenceStatus.PresenceType = PresenceType.unavailable;
                if (this.OnPresenceStatusChanged != null)
                    this.OnPresenceStatusChanged(this);
            }
            

            /// Remove all our logics
            lock (LogicLock)
            {
                foreach (XMPPServerLogic log in ActiveServices)
                {
                    log.Close();
                }

                ActiveServices.Clear();
            }

            if (User != null)
            {
                User.RemoveInstance(this);
                User = null;
            }
        }

        private bool m_bIsAuthenticated = false;
        public bool IsAuthenticated
        {
            get { return m_bIsAuthenticated; }
            
        }

        private int m_nPriority = 10;
        public int Priority
        {
            get { return m_nPriority; }
            set { m_nPriority = value; }
        }


        protected JID m_objJID = new JID();
        public JID JID
        {
            get { return m_objJID; }
            set
            {
                if (m_objJID != value)
                {
                    m_objJID = value;
                }
            }
        }

        public delegate void DelegateUserInstance(XMPPUserInstance instance);
        public event DelegateUserInstance OnPresenceStatusChanged = null;

        protected PresenceStatus m_objPresenceStatus = new PresenceStatus();
        public PresenceStatus PresenceStatus
        {
            get { return m_objPresenceStatus; }
            set
            {
                if (m_objPresenceStatus != value)
                {
                    m_objPresenceStatus = value;
                    if (OnPresenceStatusChanged != null)
                        OnPresenceStatusChanged(this);
                }
            }
        }

        private bool m_bHasRequestRoster = false;
        /// <summary>
        ///  Set to true if this client has requested a roster
        /// </summary>
        public bool HasRequestRoster
        {
            get { return m_bHasRequestRoster; }
            set { m_bHasRequestRoster = value; }
        }

        public bool Authenticate(string strUser, string strPassword)
        {
            m_bIsAuthenticated = Server.Domain.AuthenticateUser(strUser, strPassword);
            if (m_bIsAuthenticated == true)
            {
                this.JID.User = strUser;
                this.JID.Domain = Server.Domain.DomainName;
                this.JID.Resource = Guid.NewGuid().ToString();
                this.User = Server.Domain.UserList.FindUser(strUser);
            }
            return m_bIsAuthenticated;
        }

        public bool Bind(string strResource)
        {
            if (m_bIsAuthenticated == false)
                return false;

            this.User.BindUserInstance(this, strResource);
            return true;
        }
        

        public event EventHandler OnStateChanged = null;
        private XMPPState m_eXMPPState = XMPPState.Unknown;


        /// <summary>
        /// The XMPP State machine.  SHould be 'Ready' before message are exchanged by external programs
        /// </summary>
        public XMPPState XMPPState
        {
            get { return m_eXMPPState; }
            set 
            {
                if (m_eXMPPState != value)
                {
                    m_eXMPPState = value;
                    LogMessage("Setting state to {0}", m_eXMPPState);

                    StateChanged();
                }
            }
        }

        protected virtual void StateChanged()
        {
            // Fire state changed event
            if (OnStateChanged != null)
                OnStateChanged(this, new EventArgs());

            if (XMPPState == System.Net.XMPP.XMPPState.CanBind)
            {
            }
            else if (XMPPState == System.Net.XMPP.XMPPState.Bound)
            {

            }
            else if (XMPPState == System.Net.XMPP.XMPPState.Session)
            {
                XMPPState = System.Net.XMPP.XMPPState.Ready;
            }
            else if (XMPPState == System.Net.XMPP.XMPPState.Unknown)  // We be logged out
            {
                /// Update presence
                /// 

            }
            else if (XMPPState == System.Net.XMPP.XMPPState.Ready)
            {
                
            }
            else
            {
            }

        }

        public XMPPClientConnection XMPPConnection = null;



       
        public bool Ready
        {
            get
            {
                if (this.XMPPState == XMPP.XMPPState.Ready)
                    return true;

                return false;
            }
        }

        public bool Connected
        {
            get
            {
                if (XMPPConnection == null)
                    return false;
                return XMPPConnection.Connected;
            }
            set
            { 
            }
        }

        public void SendRawXML(string strXML)
        {
            try
            {
                XMPPConnection.Send(strXML);
            }
            catch (Exception ex)
            {
                LogMessage("Exception sending raw XML: {0}", ex);
                throw new Exception("Exception sending raw XML", ex);
            }
        }

        /// <summary>
        /// Sends an iq, message, or presence message
        /// </summary>
        /// <param name="iq"></param>
        public void SendXMPP(XMPPMessageBase iq)
        {
            try
            {
                XMPPConnection.Send(iq.MessageXML);
            }
            catch (Exception ex)
            {
                LogMessage("Exception sending XMPP class: {0}", ex);
                throw new Exception("Exception sending XMPP class", ex);
            }
        }

        public void SendObject(object objXMLSerializable)
        {
            try
            {
                XMPPConnection.Send(Utility.GetXMLStringFromObject(objXMLSerializable));
            }
            catch (Exception ex)
            {
                LogMessage("Exception sending object: {0}", ex);
                throw new Exception("Exception sending object", ex);
            }
        }

        public void StartTLS(X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new Exception("Certificate is null, Cannot start TLS server");
            XMPPConnection.StartTLSAsServer(certificate);
        }

        /// <summary>
        /// Adds a logic handler to our list so it will receive IQ and message events
        /// </summary>
        /// <param name="log"></param>
        public void AddLogic(XMPPServerLogic log)
        {
            lock (LogicLock)
            {
                ActiveServices.Add(log);
            }
        }

        /// <summary>
        /// Removes a logic handler from our list.  Logic handlers are automatically removed if they set their state to completed
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public bool RemoveLogic(XMPPServerLogic log)
        {
            bool bRet = false;
            lock (LogicLock)
            {
                if (ActiveServices.Contains(log) == true)
                {
                    ActiveServices.Remove(log);
                    bRet = true;
                }
            }
            return bRet;
        }

        public XMPPServerLogic FindService(Type type)
        {
            lock (LogicLock)
            {
                foreach (XMPPServerLogic log in ActiveServices)
                {
                    if (log.GetType() == type)
                        return log;
                }
            }
            return null;
        }

        public IQ SendRecieveIQ(IQ iq, int nTimeoutMS)
        {
            return SendRecieveIQ(iq, nTimeoutMS, SerializationMethod.MessageXMLProperty);
        }
        /// <summary>
        /// Sends an IQ and returns the response.  This is called from separate thread since it waits
        /// </summary>
        /// <param name="iq">The IQ to send</param>
        /// <param name="nTimeoutMS">The timeout in ms to wait for a areponse</param>
        /// <returns>The IQ that was received, or null if one was not received within the timeout period</returns>
        public IQ SendRecieveIQ(IQ iq, int nTimeoutMS, SerializationMethod method)
        {
            ServerSendRecvIQLogic iqlog = new ServerSendRecvIQLogic(this.Server, this, iq);
            iqlog.SerializationMethod = method;
            AddLogic(iqlog);

            iqlog.SendReceive(nTimeoutMS);

            RemoveLogic(iqlog);

            return iqlog.RecvIQ;
        }

        /// <summary>
        /// Allows the client to syncronously wait for an xmpp message of the given type.  Don't call this from the xmpp receiving threads.
        /// </summary>
        /// <param name="msgtype"></param>
        /// <param name="nTimeoutMS"></param>
        /// <returns></returns>
        public Message WaitForMessageType(Type msgtype, int nTimeoutMS)
        {
            ServerWaitForMessageLogic msglog = new ServerWaitForMessageLogic(this.Server, this, msgtype);
            AddLogic(msglog);

            msglog.Wait(nTimeoutMS);

            RemoveLogic(msglog);

            return msglog.RecvMessage;
        }

        /// <summary>
        /// Adds a 1 time message waiter to the logic queue.  This object can be examined to determine when the message has been received
        /// </summary>
        /// <param name="msgtype"></param>
        /// <returns></returns>
        public ServerWaitForMessageLogic AddSingleMessageWaiter(Type msgtype)
        {
            ServerWaitForMessageLogic msglog = new ServerWaitForMessageLogic(this.Server, this, msgtype);
            AddLogic(msglog);

            return msglog;
        }



        internal virtual void NewStream(string strTo, string strFrom, string strId, string strVersion, string strLanguage)
        {
            bool bHandled = false;
            /// See if any of our handlers 
            /// 
            List<XMPPServerLogic> RemoveList = new List<XMPPServerLogic>();
            lock (LogicLock)
            {
                foreach (XMPPServerLogic log in ActiveServices)
                {
                    bHandled = log.NewStream(strTo, strFrom, strId, strVersion, strLanguage, this);
                    if (log.IsCompleted == true)
                        RemoveList.Add(log);
                    if (bHandled == true)
                        break;

                }

                foreach (XMPPServerLogic log in RemoveList)
                    ActiveServices.Remove(log);
            }
        }

        /// <summary>
        /// Handle incoming stanza
        /// TODO... pump this to a worker thread pool by default, keep on socket thread for low-cpu clients
        /// </summary>
        /// <param name="stanza"></param>
        internal virtual void NewStanza(XMPPStanza stanza)
        {
            bool bHandled = false;
            /// See if any of our handlers 
            /// 
            List<XMPPServerLogic> RemoveList = new List<XMPPServerLogic>();
            lock (LogicLock)
            {
                foreach (XMPPServerLogic log in ActiveServices)
                {
                    bHandled = log.NewXMLFragment(stanza, this);
                    if (log.IsCompleted == true)
                        RemoveList.Add(log);
                    if (bHandled == true)
                        break;
                    
                }

                foreach (XMPPServerLogic log in RemoveList)
                    ActiveServices.Remove(log);
            }

            if (bHandled == true)
                return;

            /// Now see if this stanza is a higher level IQ or message
            /// 
            try
            {
               XElement elem = XElement.Parse(stanza.XML);
               XMPPMessageBase msg = null;
               if (elem.Name == "iq")
               {
                   msg = this.Server.XMPPMessageFactory.BuildIQ(elem, stanza.XML);
               }
               else if (elem.Name == "message")
               {
                   msg = this.Server.XMPPMessageFactory.BuildMessage(elem, stanza.XML);
               }
               else if (elem.Name == "presence")
               {
                   msg = this.Server.XMPPMessageFactory.BuildPresence(elem, stanza.XML);
                   //msg = new PresenceMessage(stanza.XML);
               }
                   /// TODO.. log IQ, MESSAGE or PRESENCE event, maybe have an event handler
                   /// 
                if (msg != null)
                {
                    if (msg is IQ)
                        bHandled = OnIQ(msg as IQ);
                    else if (msg is Message)
                        bHandled = OnMessage(msg as Message);
                    else if (msg is PresenceMessage)
                        bHandled = OnPresence(msg as PresenceMessage);
               }
            }
            catch(Exception  ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            if (bHandled == true)
                return;

            
        }

        protected virtual bool OnIQ(IQ iq)
        {
            bool bHandled = false;
            XMPPServerLogic[] LogicList = null;
            List<XMPPServerLogic> RemoveList = new List<XMPPServerLogic>();

            lock (LogicLock)
            {
                LogicList = ActiveServices.ToArray();
            }

            foreach (XMPPServerLogic log in LogicList)
            {
                bHandled = log.NewIQ(iq, this);

                if (log.IsCompleted == true)
                    RemoveList.Add(log);
                if (bHandled == true)
                    break;

            }

            lock (LogicLock)
            {
                foreach (XMPPServerLogic log in RemoveList)
                {
                    if (ActiveServices.Contains(log) == true)
                        ActiveServices.Remove(log);
                }
            }
            return bHandled;
        }

        protected virtual bool OnMessage(Message msg)
        {
            bool bHandled = false;
            XMPPServerLogic[] LogicList = null;
            List<XMPPServerLogic> RemoveList = new List<XMPPServerLogic>();

            lock (LogicLock)
            {
                LogicList = ActiveServices.ToArray();
            }

            foreach (XMPPServerLogic log in LogicList)
            {
                bHandled = log.NewMessage(msg, this);

                if (log.IsCompleted == true)
                    RemoveList.Add(log);
                if (bHandled == true)
                    break;

            }

            lock (LogicLock)
            {
                foreach (XMPPServerLogic log in RemoveList)
                {
                    if (ActiveServices.Contains(log) == true)
                        ActiveServices.Remove(log);
                }
            }
            return bHandled;
        }

        protected virtual bool OnPresence(PresenceMessage pres)
        {
            ///Set our presence
            if ((pres.Type == null) || (pres.Type.Length <= 0))
               PresenceStatus = pres.PresenceStatus;

            // See if any other services want to handle our presence change 
            bool bHandled = false;
            XMPPServerLogic[] LogicList = null;
            List<XMPPServerLogic> RemoveList = new List<XMPPServerLogic>();

            lock (LogicLock)
            {
                LogicList = ActiveServices.ToArray();
            }

            foreach (XMPPServerLogic log in LogicList)
            {
                bHandled = log.NewPresence(pres, this);

                if (log.IsCompleted == true)
                    RemoveList.Add(log);
                if (bHandled == true)
                    break;
            }

            lock (LogicLock)
            {
                foreach (XMPPServerLogic log in RemoveList)
                {
                    if (ActiveServices.Contains(log) == true)
                        ActiveServices.Remove(log);
                }
            }
            return bHandled;
        }

        protected List<XMPPServerLogic> ActiveServices = new List<XMPPServerLogic>();
        protected object LogicLock = new object();


        public delegate void DelegateString(XMPPUserInstance client, string strXML);
        public event DelegateString OnXMLSent = null;
        public event DelegateString OnXMLReceived = null;

        protected internal virtual void FireXMLSent(string strXML)
        {
            System.Diagnostics.Debug.WriteLine("-->" + strXML);

            if (OnXMLSent != null)
                OnXMLSent(this, strXML);
        }

        protected internal virtual void FireXMLReceived(string strXML)
        {
            System.Diagnostics.Debug.WriteLine("<--" + strXML);

            if (OnXMLReceived != null)
                OnXMLReceived(this, strXML);
        }


     

    }

}
