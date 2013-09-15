/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;

namespace System.Net.XMPP.Server
{
    /// <summary>
    /// Server side logic for handling incoming messages
    /// </summary>
    public class XMPPServerLogic
    {
       
            public XMPPServerLogic(XMPPServer server, XMPPUserInstance client)
            {
                XMPPServer = server;
                UserInstance = client;
            }

            public virtual XMPPServerLogic Clone(XMPPUserInstance newclient)
            {
                return new XMPPServerLogic(XMPPServer, newclient);
            }

            protected XMPPServer m_objXMPPServer = null;
            public XMPPServer XMPPServer
            {
                get { return m_objXMPPServer; }
                set { m_objXMPPServer = value; }
            }

            private XMPPUserInstance m_objUserInstance = null;
            public XMPPUserInstance UserInstance
            {
                get { return m_objUserInstance; }
                set { m_objUserInstance = value; }
            }

            public virtual void Start()
            {
            }

            public virtual void Close()
            {
            }

            /// <summary>
            ///  A new stream statement has been started.  This is not complete XML which is why it is handled differently
            /// </summary>
            public virtual bool NewStream(string strTo, string strFrom, string strId, string strVersion, string strLanguage, XMPPUserInstance instancefrom)
            {
                return false;
            }

            /// <summary>
            /// A new XML fragment has been received
            /// </summary>
            /// <param name="node"></param>
            /// <returns>returns true if we handled this fragment, false if other wise</returns>
            public virtual bool NewXMLFragment(XMPPStanza stanza, XMPPUserInstance instancefrom)
            {
                return false;
            }


            public virtual bool NewIQ(IQ iq, XMPPUserInstance instancefrom)
            {
                return false;
            }

            public virtual bool NewMessage(Message iq, XMPPUserInstance instancefrom)
            {
                return false;
            }

            public virtual bool NewPresence(PresenceMessage iq, XMPPUserInstance instancefrom)
            {
                return false;
            }


            protected bool m_bCompleted = false;

            /// <summary>
            /// Set to true if we have completed our logic and should be removed from the logic list
            /// </summary>
            public bool IsCompleted
            {
                get { return m_bCompleted; }
                set { m_bCompleted = value; }
            }

            private bool m_bSuccess = false;

            public bool Success
            {
                get { return m_bSuccess; }
                set { m_bSuccess = value; }
            }

        }

    public class ServerWaitableLogic : XMPPServerLogic, IDisposable
        {
        public ServerWaitableLogic(XMPPServer server, XMPPUserInstance client)
                : base(server, client)
            {
            }


            protected System.Threading.ManualResetEvent GotEvent = new System.Threading.ManualResetEvent(false);

            public virtual bool Wait(int nTimeoutMs)
            {
                Success = GotEvent.WaitOne(nTimeoutMs);
                return Success;
            }

            private SerializationMethod m_eSerializationMethod = SerializationMethod.MessageXMLProperty;

            public SerializationMethod SerializationMethod
            {
                get { return m_eSerializationMethod; }
                set { m_eSerializationMethod = value; }
            }

            #region IDisposable Members

            bool m_bIsDisposed = false;
            public void Dispose()
            {
                if (m_bIsDisposed == false)
                {
                    GotEvent.Close();
                    GotEvent.Dispose();
                    m_bIsDisposed = true;
                }
            }

            #endregion
        }


    public class ServerSendRecvIQLogic : ServerWaitableLogic
    {
        public ServerSendRecvIQLogic(XMPPServer server, XMPPUserInstance client, IQ iq)
            : base(server, client)
        {
            SendIQ = iq;
        }

        private string m_strInnerXML = "";

        public string InnerXML
        {
            get { return m_strInnerXML; }
            set { m_strInnerXML = value; }
        }


        public bool SendReceive(int nTimeoutMs)
        {
            if (SerializationMethod == XMPP.SerializationMethod.MessageXMLProperty)
                UserInstance.SendXMPP(SendIQ);
            else
                UserInstance.SendObject(SendIQ);

            Success = GotEvent.WaitOne(nTimeoutMs);
            return Success;
        }


        IQ m_objSendIQ = null;
        public IQ SendIQ
        {
            get { return m_objSendIQ; }
            set { m_objSendIQ = value; }
        }

        private IQ m_objRecvIQ = null;
        public IQ RecvIQ
        {
            get { return m_objRecvIQ; }
            set { m_objRecvIQ = value; }
        }

        public override bool NewIQ(IQ iq, XMPPUserInstance instancefrom)
        {
            try
            {
                if (iq.ID == SendIQ.ID)
                {
                    RecvIQ = iq;
                    IsCompleted = true;
                    Success = true;
                    GotEvent.Set();
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

    }


    public class ServerWaitForMessageLogic : ServerWaitableLogic
    {
        public ServerWaitForMessageLogic(XMPPServer server, XMPPUserInstance client, Type msgtype)
            : base(server, client)
        {
            MessageType = msgtype;
        }

        Type MessageType = null;

        private string m_strInnerXML = "";

        public string InnerXML
        {
            get { return m_strInnerXML; }
            set { m_strInnerXML = value; }
        }


        private Message m_objRecvMessage = null;

        public Message RecvMessage
        {
            get { return m_objRecvMessage; }
            set { m_objRecvMessage = value; }
        }

        public override bool NewMessage(Message iq, XMPPUserInstance instancefrom)
        {
            try
            {
                if (iq.GetType() == MessageType)
                {
                    RecvMessage = iq;
                    IsCompleted = true;
                    Success = true;
                    GotEvent.Set();
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }


    }

}
