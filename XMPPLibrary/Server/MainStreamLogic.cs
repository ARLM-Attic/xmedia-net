using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace System.Net.XMPP.Server
{
    public enum StreamState
    {
        None,
        WaitingOnClientFeatureResponse,
        RunningAuthenticationLogic,
        WaitingOnBind,
        WaitingOnSession
    }
    /// <summary>
    ///  Main stream negotiation logic.   This logic is top level and handles tls, authentication, compression, , and regsiter
    /// </summary>
    public class MainStreamLogic : XMPPServerLogic, IXMPPMessageBuilder
    {
        public MainStreamLogic(XMPPServer server, XMPPUserInstance client)
            : base(server, client)
        {
            sf.mechanisms = new mechanisms();

            foreach (AuthenticationMechanismLogic auth in this.XMPPServer.AuthenticationMethods)
            {
                AuthenticationMethods.Add( (AuthenticationMechanismLogic) auth.Clone(client));
                sf.mechanisms.Mechanisms.Add(auth.Name);
            }

            if ((this.XMPPServer.XMPPServerConfig.AllowTLS == true) || (this.XMPPServer.XMPPServerConfig.TLSRequired == true))
            {
                sf.starttls = new starttls();
                if (this.XMPPServer.XMPPServerConfig.TLSRequired == true)
                    sf.starttls.required = ""; 
            }
            else
                sf.starttls = null;

            // TODO... add compression and it's list of methods;

            /// Add parsing of Bind and Session IQ's
            /// 
            XMPPServer.XMPPMessageFactory.AddMessageBuilder(this);
        }

        public List<AuthenticationMechanismLogic> AuthenticationMethods = new List<AuthenticationMechanismLogic>();

        protected XMPPServerLogic ActiveLogic = null;
        private StreamState m_eStreamState = StreamState.None;
        protected StreamState StreamState
        {
            get { return m_eStreamState; }
            set { m_eStreamState = value; }
        }

        protected bool Authenticated = false;

        public streamfeatures sf = new streamfeatures();

        public override XMPPServerLogic Clone(XMPPUserInstance newclient)
        {
            return new MainStreamLogic(base.XMPPServer, newclient);
        }


        public override bool NewStream(string strTo, string strFrom, string strId, string strVersion, string strLanguage, XMPPUserInstance instancefrom)
        {
            /// See where we are in our state, usually send a response stream and then our feature list that haven't been activated

            ActiveLogic = null;
            StreamState = Server.StreamState.WaitingOnClientFeatureResponse;

            /// Send our features
            /// 
            //this.Client.SendObject(sf);
            
            string strXML = Utility.GetXMLStringFromObject(sf);
            strXML = strXML.Replace("_x003A_", ":");
            this.UserInstance.SendRawXML(strXML);

            return true;
        }

        public override bool NewXMLFragment(XMPPStanza stanza, XMPPUserInstance instancefrom)
        {
            if (stanza.XML == "</stream>")
            {
                StreamState = Server.StreamState.None;
                instancefrom.Disconnect();
                return true;
            }

            if (StreamState == Server.StreamState.None)
                return false;

            else if (StreamState == Server.StreamState.WaitingOnClientFeatureResponse)
            {
                string strNewXML = stanza.XML.Replace("stream:", "");  // no support for namespaces in windows phone 7, remove them
                XElement xmlElem = XElement.Parse(strNewXML);

                /// We will handle TLS our self, logic is simple
                if (xmlElem.Name == "{urn:ietf:params:xml:ns:xmpp-tls}starttls")
                {
                    /// Give a proceed, wait for a new stream
                    /// 
                    ActiveLogic = null;
                    StreamState = Server.StreamState.None;

                    /// TODO.. Tell server to start TLS
                    UserInstance.SendRawXML("<proceed xmlns=\"urn:ietf:params:xml:ns:xmpp-tls\"/>");
                    UserInstance.StartTLS(XMPPServer.ServerCertificate);
                    sf.starttls = null;
                }
                else if (xmlElem.Name == "{urn:ietf:params:xml:ns:xmpp-sasl}auth")
                {
                    string strMechanism = xmlElem.Attribute("mechanism").Value;

                    /// See if we have this auth mechanism, if so, start that logic
                    /// 
                    foreach (AuthenticationMechanismLogic mech in AuthenticationMethods)
                    {
                        if (strMechanism == mech.Name)
                        {
                            ActiveLogic = mech;
                            StreamState = Server.StreamState.RunningAuthenticationLogic;

                            bool bRet = ActiveLogic.NewXMLFragment(stanza, instancefrom);
                            if (ActiveLogic.IsCompleted == true)
                            {
                                ActiveLogic = null;
                                StreamState = Server.StreamState.None;
                                sf.mechanisms = null;
                                if (UserInstance.IsAuthenticated == true)
                                {
                                    sf.bind = new bind();
                                    sf.session = new session();
                                }
                            }
                            return bRet;
                        }
                    }
                }
            }
            else if ((StreamState == Server.StreamState.RunningAuthenticationLogic) && (ActiveLogic != null))
            {
                bool bRet = ActiveLogic.NewXMLFragment(stanza, instancefrom);
                if (ActiveLogic.IsCompleted == true)
                {
                    if ((ActiveLogic is AuthenticationMechanismLogic) && (UserInstance.IsAuthenticated == true))
                    {
                        /// we may now active bind and session and deactive auth
                        /// 
                        sf.bind = new bind();
                        sf.session = new session();
                        StreamState = Server.StreamState.WaitingOnBind;
                        ActiveLogic = null;
                        sf.mechanisms = null;
                    }
                    else
                    {
                        ActiveLogic = null;
                        StreamState = Server.StreamState.None;
                        sf.mechanisms = null;
                    }


                }
                return bRet;
            }
            else if (StreamState == Server.StreamState.WaitingOnBind) /// will be handled in OnIq
            {
                return false;
            }
            else if (StreamState == Server.StreamState.WaitingOnSession) /// will be handled in OnIq
            {
                return false;
            }
            else
            {
                StreamState = Server.StreamState.None;
                /// Something wrong, logic should never be null if we are active
                /// 
                this.UserInstance.Disconnect();
                return true;
            }


            return false;
        }

        /// <summary>
        /// We can handle Bind and Session IQ's
        /// </summary>
        /// <param name="iq"></param>
        /// <returns></returns>
        public override bool NewIQ(IQ iq, XMPPUserInstance instancefrom)
        {
            if (iq is BindIQ) 
            {
                BindIQ bind = iq as BindIQ;
                
                string strResource = bind.Bind.Resource;

                bool bBound = UserInstance.Bind(strResource);

                /// Find a compatible resource and send it back
                /// 
                bind.Bind.Resource = null;
                bind.Bind.JID = UserInstance.JID;
                bind.From = null;
                bind.To = null;
                bind.Type = IQType.result.ToString();

                UserInstance.SendObject(bind);
                StreamState = Server.StreamState.WaitingOnSession;

                return true;
            }
            else if (iq is SessionIQ)
            {

                SessionIQ session = iq as SessionIQ;
                session.To = session.From;
                session.From = UserInstance.JID;
                session.Type = IQType.result.ToString();
                UserInstance.SendObject(session);

                StreamState = Server.StreamState.None;

                return true;
            }
            return false;
        }


        public Message BuildMessage(XElement elem, string strXML)
        {
            return null;
        }

        public IQ BuildIQ(XElement elem, string strXML)
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
    }
}
