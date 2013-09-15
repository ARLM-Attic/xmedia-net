using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace System.Net.XMPP.Server
{

    /// <summary>
    /// Handles service discoveryqueries
    /// </summary>
    public class ServerPingLogic : XMPPServerLogic
    {
        public ServerPingLogic(XMPPServer server, XMPPUserInstance client)
            : base(server, client)
        {
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("urn:xmpp:ping"));
        }

        public override XMPPServerLogic Clone(XMPPUserInstance newclient)
        {
            return this; /// Only 1 instance of this running in domain
        }

        public override bool NewIQ(IQ iq, XMPPUserInstance instancefrom)
        {
            //<iq type="get" id="919-1764" from="ninethumbs.com" to="brianbonnett@ninethumbs.com/calculon"><ping xmlns="urn:xmpp:ping"/></iq>
            //<iq type="result" id="919-1764" from="brianbonnett@ninethumbs.com" to="ninethumbs.com" />

            if (iq is PingIQ)
            {
                IQ iqresult = new IQ();
                iqresult.To = iq.From;
                iqresult.From = XMPPServer.Domain.DomainName;
                iqresult.Type = IQType.result.ToString();
                iqresult.ID = iq.ID;
                instancefrom.SendObject(iqresult);
               return true;
            }
           return false;
        }
    }
}
