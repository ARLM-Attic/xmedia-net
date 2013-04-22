/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Net;


using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace System.Net.XMPP
{
    [XmlRoot(ElementName = "ping", Namespace="urn:xmpp:ping")]
    public class PingParameter
    {
        public PingParameter()
        {
        }

    }


    [XmlRoot(ElementName = "iq")]
    public class PingIQ : IQ
    {
        public PingIQ()
            : base()
        {
        }

        private PingParameter m_objPing = new PingParameter();
        [XmlElement("ping", Namespace = "urn:xmpp:ping")]
        public PingParameter Ping
        {
            get { return m_objPing; }
            set { m_objPing = value; }
        }
    }

    [XmlRoot(ElementName = "session", Namespace = "urn:ietf:params:xml:ns:xmpp-session")]
    public class SessionParameter
    {
        public SessionParameter()
        {
        }

    }


    [XmlRoot(ElementName = "iq")]
    public class SessionIQ : IQ
    {
        public SessionIQ()
            : base()
        {
            this.Type = IQType.set.ToString();
        }

        private SessionParameter m_objSession = new SessionParameter();
        [XmlElement("session", Namespace = "urn:ietf:params:xml:ns:xmpp-session")]
        public SessionParameter Session
        {
            get { return m_objSession; }
            set { m_objSession = value; }
        }
    }

   

    public class GenericIQLogic : Logic, IXMPPMessageBuilder
    {
        public GenericIQLogic(XMPPClient client)
            : base(client)
        {
            BindIQ = new IQ();
            BindIQ.Type = IQType.set.ToString();
            BindIQ.To = null;
            BindIQ.From = null;

            XMPPClient.XMPPMessageFactory.AddMessageBuilder(this);
            
            /// Already added by default
            //XMPPClient.AddLogic(this);

        }

        #region IXMPPMessageBuilder Members

        public Message BuildMessage(System.Xml.Linq.XElement elem, string strXML)
        {
            return null;
        }

        public IQ BuildIQ(System.Xml.Linq.XElement elem, string strXML)
        {
            if ((elem.FirstNode != null) && (elem.FirstNode is XElement) &&
                (((XElement)(elem.FirstNode)).Name == "{urn:xmpp:ping}ping"))
            {
                PingIQ query = Utility.ParseObjectFromXMLString(strXML, typeof(PingIQ)) as PingIQ;
                return query;
            }
            else if ((elem.FirstNode != null) && (elem.FirstNode is XElement) &&
                (((XElement)(elem.FirstNode)).Name == "{urn:ietf:params:xml:ns:xmpp-session}session"))
            {
                SessionIQ query = Utility.ParseObjectFromXMLString(strXML, typeof(SessionIQ)) as SessionIQ;
                return query;
            }
            else if ((elem.FirstNode != null) && (elem.FirstNode is XElement) &&
           (((XElement)(elem.FirstNode)).Name == "{urn:ietf:params:xml:ns:xmpp-bind}bind"))
            {
                BindIQ query = Utility.ParseObjectFromXMLString(strXML, typeof(BindIQ)) as BindIQ;
                return query;
            }
            return null;
        }


        public PresenceMessage BuildPresence(XElement elem, string strXML)
        {
            return null;
        }
        #endregion

        private string m_strInnerXML = "";

        public string InnerXML
        {
            get { return m_strInnerXML; }
            set { m_strInnerXML = value; }
        }

        public override void Start()
        {
            base.Start();
            Bind();
        }

        public const string BindXML = @"<bind xmlns=""urn:ietf:params:xml:ns:xmpp-bind""><resource>##RESOURCE##</resource></bind>";
        
        IQ BindIQ = null;

        void Bind()
        {
            BindIQ.InnerXML = BindXML.Replace("##RESOURCE##", XMPPClient.JID.Resource);

            XMPPClient.XMPPState = XMPPState.Binding;
            XMPPClient.SendXMPP(BindIQ);
        }

        SessionIQ sessioniq = null;
        internal void StartSession()
        {
            sessioniq = new SessionIQ();
            //sessioniq = new IQ();
            //sessioniq.InnerXML = "<session xmlns='urn:ietf:params:xml:ns:xmpp-session'/>";
            sessioniq.From = null;
            sessioniq.To = null;
            sessioniq.Type = IQType.set.ToString();
            //XMPPClient.SendXMPP(sessioniq);
            XMPPClient.SendObject(sessioniq);
        }

        
        public override bool NewIQ(IQ iq)
        {
            try
            {
                if ( (BindIQ != null) && (iq.ID == BindIQ.ID))
                {
                    /// Extract our jid incase it changed
                    /// <iq type="result" id="bind_1" to="ninethumbs.com/7b5005e1"><bind xmlns="urn:ietf:params:xml:ns:xmpp-bind"><jid>test@ninethumbs.com/hypnotoad</jid></bind></iq>
                    /// 
                    if (iq.Type == IQType.result.ToString())
                    {
                        /// bound, now do toher things
                        /// 
                        if (iq is BindIQ)
                        {
                            XMPPClient.JID = ((BindIQ)iq).Bind.JID;
                            //XElement elembind = XElement.Parse(iq.InnerXML);
                            //XElement nodejid = elembind.FirstNode as XElement;
                            //if ((nodejid != null) && (nodejid.Name == "{urn:ietf:params:xml:ns:xmpp-bind}jid"))
                            //{
                            //    XMPPClient.JID = nodejid.Value;
                            //}
                        }
                        XMPPClient.XMPPState = XMPPState.Bound;
                    }
                    return true;
                }
                else if ((sessioniq != null) && (iq.ID == sessioniq.ID))
                {
                    XMPPClient.XMPPState = XMPPState.Session;
                    return true;
                }

                if (iq is PingIQ)
                {
                    // Send pong
                    iq.Type = IQType.result.ToString();
                    iq.To = iq.From;
                    iq.From = XMPPClient.JID.BareJID;
                    iq.InnerXML = "";
                    XMPPClient.SendXMPP(iq);
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public IQ SendPing(JID to, bool bWaitForResponse, int nTimeOutMs)
        {
            PingIQ iq = new PingIQ();
            iq.Type = IQType.get.ToString();
            iq.From = XMPPClient.JID.BareJID;
            iq.To = to;
            if (bWaitForResponse == true)
                return XMPPClient.SendRecieveIQ(iq, nTimeOutMs, SerializationMethod.XMLSerializeObject);
            else
                XMPPClient.SendObject(iq);
            return null;
        }

    }

    /// <summary>
    /// The method that is used to get xml from the object
    /// </summary>
    public enum SerializationMethod
    {
        /// <summary>
        /// Use the XMLSerializer to get xml from the object
        /// </summary>
        XMLSerializeObject,

        /// <summary>
        /// Use the virtual MessageXML property to xml from the object
        /// </summary>
        MessageXMLProperty,
    }

    public class SendRecvIQLogic : WaitableLogic
    {
        public SendRecvIQLogic(XMPPClient client, IQ iq)
            : base(client)
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
                XMPPClient.SendXMPP(SendIQ);
            else
                XMPPClient.SendObject(SendIQ);

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

        public override bool NewIQ(IQ iq)
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


    public class WaitForMessageLogic : WaitableLogic
    {
        public WaitForMessageLogic(XMPPClient client, Type msgtype)
            : base(client)
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

        public override bool NewMessage(Message iq)
        {
            try
            {
                if (iq.GetType() ==  MessageType)
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
