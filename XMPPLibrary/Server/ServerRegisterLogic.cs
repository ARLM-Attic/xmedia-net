
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

    /// <summary>
    /// Allows in-place user registration
    /// </summary>
    public class ServerRegisterLogic : XMPPServerLogic, IXMPPMessageBuilder
    {
        public ServerRegisterLogic(XMPPServer server, XMPPUserInstance client)
            : base(server, client)
        {
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("jabber:iq:register"));
            XMPPServer.XMPPMessageFactory.AddMessageBuilder(this);
        }

        public override XMPPServerLogic Clone(XMPPUserInstance newclient)
        {
            return this;
        }

        public override bool NewIQ(IQ iq, XMPPUserInstance instancefrom)
        {
            if (iq is RegisterQueryIQ)
            {
                if (instancefrom.IsAuthenticated == true)
                    return false;

                RegisterQueryIQ riq = iq as RegisterQueryIQ;

                if (riq.RegisterQuery != null)
                {
                    XMPPUser user = XMPPServer.Domain.UserList.FindUser(riq.RegisterQuery.UserName);
                    if (user == null)
                    {
                        user = new XMPPUser(XMPPServer) { UserName = riq.RegisterQuery.UserName, Password = riq.RegisterQuery.Password };
                        XMPPServer.Domain.UserList.AddUser(user);

                        IQ iqresult = new IQ();
                        iqresult.ID = iq.ID;
                        iqresult.To = null;// string.Format("{0}/{1}", XMPPServer.Domain.DomainName, instancefrom.JID.Resource);
                        iqresult.From = null;
                        iqresult.Type = IQType.result.ToString();
                        instancefrom.SendObject(iqresult);

                        /// TODO. Do we close the stream here, or the client??
                        return true;
                    }
                }

                IQ iqerror = new IQ();
                iqerror.ID = iq.ID;
                iqerror.To = null;// string.Format("{0}/{1}", XMPPServer.Domain.DomainName, instancefrom.JID.Resource);
                iqerror.From = null;
                iqerror.Type = IQType.error.ToString();
                instancefrom.SendObject(iqerror);
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
             (((XElement)(elem.FirstNode)).Name == "{jabber:iq:register}query"))
            {
                RegisterQueryIQ query = Utility.ParseObjectFromXMLString(strXML, typeof(RegisterQueryIQ)) as RegisterQueryIQ;
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
