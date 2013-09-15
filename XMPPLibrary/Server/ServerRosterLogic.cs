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
    /// Handles service discoveryqueries
    /// </summary>
    public class ServerRosterLogic : XMPPServerLogic
    {
        public ServerRosterLogic(XMPPServer server, XMPPUserInstance client)
            : base(server, client)
        {
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("jabber:iq:roster"));
        }

        public override XMPPServerLogic Clone(XMPPUserInstance newclient)
        {
            return this; /// Only 1 instance of this running in domain
        }

        public override bool NewIQ(IQ iq, XMPPUserInstance instancefrom)
        {
            if (iq is RosterIQ)
            {
                RosterIQ riq = iq as RosterIQ;
                riq.Query.RosterItems = instancefrom.User.Roster.ToArray();
                riq.To = instancefrom.JID;
                riq.From = null;
                riq.Type = IQType.result.ToString();

                instancefrom.SendObject(riq);

                return true;
            }
            return false;
        }

    }
}
