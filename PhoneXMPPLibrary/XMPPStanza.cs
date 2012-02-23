using System;
using System.Net;

using System.Xml;

namespace System.Net.XMPP
{
    // An XML message right under the <stream> node
    // Derived classes include message, iq and presence
    public class XMPPStanza
    {
        public XMPPStanza(XMPPClient client)
        {
        }

        public XMPPStanza(string strXML)
        {
            m_strXML = strXML;
          //  Node = new XMPPXMLNode(m_strXML);
        }
        //public XMPPXMLNode Node = null;

        
        protected string m_strXML = "";
        public string XML
        {
            get
            {
                return m_strXML;
            }
            set
            {
                m_strXML = value;
                //Node = new XMPPXMLNode(m_strXML);  
            }
        }

    }

    public class OpenStreamStanza : XMPPStanza
    {
        public OpenStreamStanza(XMPPClient client) : base (client)
        {
            m_strXML =
@"<?xml version=""1.0""?>
<stream:stream xmlns:stream=""http://etherx.jabber.org/streams"" version=""1.0"" xmlns=""jabber:client"" to=""##TO##"" xml:lang=""en"" xmlns:xml=""http://www.w3.org/XML/1998/namespace"" >";
//            m_strXML = m_strXML.Replace("##FROM##", client.JID.BareJID);
            m_strXML = m_strXML.Replace("##TO##", client.Domain);

        }
    }
}
