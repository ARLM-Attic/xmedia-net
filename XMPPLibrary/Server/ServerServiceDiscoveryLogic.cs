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
    public class ServerServiceDiscoveryLogic : XMPPServerLogic
    {
        public ServerServiceDiscoveryLogic(XMPPServer server, XMPPUserInstance client)
            : base(server, client)
        {
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/disco#items"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/disco#info"));
        }

        public override XMPPServerLogic Clone(XMPPUserInstance newclient)
        {
            return new ServerServiceDiscoveryLogic(XMPPServer, newclient);
        }

        public override bool NewIQ(IQ iq, XMPPUserInstance instancefrom)
        {
            if ( (iq is ServiceDiscoveryIQ) && (iq.To.Domain == this.XMPPServer.Domain.DomainName) && (iq.To.Resource.Length == 0) )
            {
                /// See if this is a servicediscovery IQ for our domain
                ServiceDiscoveryIQ siq = iq as ServiceDiscoveryIQ;

                if (siq.ServiceDiscoveryItemQuery != null)
                {
                    siq.ServiceDiscoveryItemQuery.Items = this.XMPPServer.Domain.OurServiceDiscoveryFeatureList.Items.ToArray();
                }
                else if (siq.ServiceDiscoveryInfoQuery != null)
                {
                    siq.ServiceDiscoveryInfoQuery.Features = this.XMPPServer.Domain.OurServiceDiscoveryFeatureList.Features.ToArray();
                    //siq.ServiceDiscoveryInfoQuery.Identities = Identities.ToArray();
                }

                XMPPUserInstance instance = XMPPServer.Domain.UserList.FindUserInstance(iq.From);
                if (instance != null)
                {
                    siq.To = siq.From;
                    siq.From = XMPPServer.Domain.DomainName;
                    siq.Type = IQType.result.ToString();
                    instance.SendObject(siq);
                }


                return true;
            }

            return false;
        }

        public override bool NewMessage(Message iq, XMPPUserInstance instancefrom)
        {
            return base.NewMessage(iq, instancefrom);
        }
    }

    
}
