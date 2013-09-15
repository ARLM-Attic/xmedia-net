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
                if (iq.Type == IQType.get.ToString())
                {
                    instancefrom.HasRequestRoster = true;
                    RosterIQ riq = iq as RosterIQ;
                    riq.Query.RosterItems = instancefrom.User.Roster.ToArray();
                    riq.To = instancefrom.JID;
                    riq.From = null;
                    riq.Type = IQType.result.ToString();

                    instancefrom.SendObject(riq);

                    return true;
                }
                else if (iq.Type == IQType.set.ToString())
                {
                    /// SEnd response
                    IQ response = new IQ();
                    response.ID = iq.ID;
                    response.To = instancefrom.JID.FullJID;
                    response.From = null;
                    response.Type = IQType.result.ToString();
                    instancefrom.SendObject(response);


                    /// Client is about to subscribe to a new roster item... tell all our other instances about it
                    /// 
                    RosterIQ riq = iq as RosterIQ;


                    if ((riq.Query != null) && (riq.Query.RosterItems != null) && (riq.Query.RosterItems.Length == 1) )
                    {

                        rosteritem rostersubscribeto = instancefrom.User.FindRosterItem(riq.Query.RosterItems[0].JID);
                        if (rostersubscribeto == null)
                        {
                            rostersubscribeto = new rosteritem() { JID = riq.Query.RosterItems[0].JID, Subscription = "none", Ask = "subscribe", Name = riq.Query.RosterItems[0].Name, Groups = riq.Query.RosterItems[0].Groups };
                            instancefrom.User.Roster.Add(rostersubscribeto);
                        }

                        foreach (XMPPUserInstance nextinstance in instancefrom.User.UserInstances.GetAllUserInstances())
                        {
                            //if (nextinstance == instancefrom)
                            //    continue;
                            if (nextinstance.HasRequestRoster == false)
                                continue;

                            riq.Query.RosterItems[0].Subscription = "none";
                            riq.To = nextinstance.JID;
                            riq.From = null;
                            riq.Type = IQType.result.ToString();
                            nextinstance.SendObject(riq);
                        }
                        return true;
                    }

                }
            }
            return false;
        }

    }
}
