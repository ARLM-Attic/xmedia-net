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
    public class AuthenticationMechanismLogic : XMPPServerLogic
    {
        public AuthenticationMechanismLogic(string strName, XMPPServer server, XMPPUserInstance client)
            : base(server, client)
        {
            Name = strName;
        }

        private string m_strName = "";
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        public override XMPPServerLogic Clone(XMPPUserInstance newclient)
        {
            return new AuthenticationMechanismLogic(this.Name, this.XMPPServer, this.UserInstance);
        }
    }

    public class PlainAuthenticationMechanism : AuthenticationMechanismLogic
    {
        public PlainAuthenticationMechanism(XMPPServer server, XMPPUserInstance client)
            : base("PLAIN", server, client)
        {
        }

        public override XMPPServerLogic Clone(XMPPUserInstance newclient)
        {
            return new PlainAuthenticationMechanism(this.XMPPServer, newclient);
        }

        public override bool NewXMLFragment(XMPPStanza stanza, XMPPUserInstance instancefrom)
        {
            string strNewXML = stanza.XML.Replace("stream:", "");  // no support for namespaces in windows phone 7, remove them
            XElement xmlElem = XElement.Parse(strNewXML);

            //if (xmlElem.Name != "{urn:ietf:params:xml:ns:xmpp-sasl}auth")

            string strPlain = xmlElem.FirstNode.ToString();
            byte [] bPlain = Convert.FromBase64String(strPlain);
            string strUserPass = System.Text.ASCIIEncoding.ASCII.GetString(bPlain);

            string strUser = null;
            string strPass = null;
            int nChar = 0;
            int nNul = 0;
            foreach (char c in strUserPass)
            {
                if ((c == '\0') && (nNul == 1))
                {
                    strUser = strUserPass.Substring(1, nChar - 1);
                    strPass = strUserPass.Substring(nChar + 1);
                    break;
                }
                else if (c == '\0')
                    nNul++;

                nChar++;
            }

            bool bAuth = UserInstance.Authenticate(strUser, strPass);
            if (bAuth == true)
            {
                
                this.IsCompleted = true;
                UserInstance.SendRawXML("<success xmlns='urn:ietf:params:xml:ns:xmpp-sasl'/>\r\n");
            }
            else
            {
                this.IsCompleted = true;
                UserInstance.SendRawXML("<failure xmlns='urn:ietf:params:xml:ns:xmpp-sasl'/>\r\n");
            }

            return true;
               
        }
    }
}
