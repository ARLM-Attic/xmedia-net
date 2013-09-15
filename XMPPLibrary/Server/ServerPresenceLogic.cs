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
    /// Handles presence.   This is an instance based logic, so it is created when a new connection is made and destroyed when
    /// that connection leaves
    /// </summary>
    public class ServerPresenceLogic : XMPPServerLogic
    {
        public ServerPresenceLogic(XMPPServer server, XMPPUserInstance client)
            : base(server, client)
        {
            if (this.UserInstance != null)
                this.UserInstance.OnPresenceStatusChanged += UserInstance_OnPresenceStatusChanged;
        }

        void UserInstance_OnPresenceStatusChanged(XMPPUserInstance instance)
        {
            PresenceChanged(instance.PresenceStatus, instance);
        }


        public override void Close()
        {
            if (this.UserInstance != null)
               this.UserInstance.OnPresenceStatusChanged -= UserInstance_OnPresenceStatusChanged;
        }


        public override XMPPServerLogic Clone(XMPPUserInstance newclient)
        {
            return new ServerPresenceLogic(XMPPServer, newclient);
        }

        protected bool PresenceChanged(PresenceStatus status, XMPPUserInstance instancefrom)
        {
            /// User has just come online, notify members of their roster they are online
            /// then notify them of all the roster member instances that are online

            PresenceMessage presencefrom = new PresenceMessage(); /// sent to our roster items
            PresenceMessage presenceto = new PresenceMessage();   /// sent back to us

            presencefrom.From = instancefrom.JID;
            presenceto.To = instancefrom.JID;

            foreach (rosteritem item in instancefrom.User.Roster)
            {
                XMPPUser objUser = XMPPServer.Domain.UserList.FindUser(new JID(item.JID).User);
                if (objUser != null)
                {
                    XMPPUserInstance  [] instances = objUser.UserInstances.GetAllUserInstances();
                    foreach (XMPPUserInstance instance in instances)
                    {
                        /// Send a presence to both sender and roster instance
                        /// 
                        if ((instance.PresenceStatus.IsOnline == true) && (instancefrom.PresenceStatus.IsOnline == true))
                        {
                            if ((item.Subscription == "to") || (item.Subscription == "both"))
                            {
                                presencefrom.To = instance.JID;
                                presencefrom.PresenceStatus = instancefrom.PresenceStatus;
                                instance.SendObject(presencefrom);
                            }

                            if ((item.Subscription == "from") || (item.Subscription == "both"))
                            {
                                presenceto.From = instance.JID;
                                presenceto.PresenceStatus = instance.PresenceStatus;
                                instancefrom.SendObject(presenceto);
                            }

                        }
                        else if ((instance.PresenceStatus.IsOnline == true) && (instancefrom.PresenceStatus.IsOnline == false))
                        {
                            if ((item.Subscription == "to") || (item.Subscription == "both"))
                            {
                                presencefrom.To = instance.JID;
                                presencefrom.PresenceStatus = instancefrom.PresenceStatus;
                                instance.SendObject(presencefrom);
                            }
                        }
                    }
                }
            }

            return true;
        }

        public override bool NewPresence(PresenceMessage iq, XMPPUserInstance instancefrom)
        {
            // look for subscription presence messages.  Update the corresponding roster item if accepted

            if (iq.Type == "probe")
            {
            }
            else if ((iq.Type == "subscribe") && (iq.To != null) )
            {
                /// Forward on to the correct user.  
                /// TODO...If not online, we must save this and send when they come online
                /// TODO...If they are in a different domain, do we add the from 
                /// field before sending it to the server?
                JID jidto = iq.To;
                if (jidto.Domain == XMPPServer.Domain.DomainName)
                {
                    iq.From = instancefrom.JID;

                    rosteritem rostersubscribeto = instancefrom.User.FindRosterItem(iq.To.BareJID); /// May be in here if user sent a roster set before this presence message
                    if (rostersubscribeto == null)
                    {
                        rostersubscribeto = new rosteritem() { JID = iq.To.BareJID, Name = instancefrom.JID.User, Subscription = "none", Ask = "subscribe" };
                        instancefrom.User.Roster.Add(rostersubscribeto);
                    }

                    /// Tell all our instances about this subscription
                    RosterIQ riq = new RosterIQ();
                    riq.Query.RosterItems = new rosteritem[] { rostersubscribeto };
                    riq.From = null;
                    riq.Type = IQType.set.ToString();
                    foreach (XMPPUserInstance nextinstance in instancefrom.User.UserInstances.GetAllUserInstances())
                    {
                        //if (nextinstance == instancefrom)
                        //    continue;
                        if (nextinstance.HasRequestRoster == false)
                            continue;

                        riq.To = nextinstance.JID;
                        nextinstance.SendObject(riq);
                    }



                    XMPPUser objUser = XMPPServer.Domain.UserList.FindUser(jidto.User);
                    rosteritem rostersubscriber = objUser.FindRosterItem(instancefrom.JID.BareJID);
                    if (rostersubscriber == null)
                    {
                        rostersubscriber = new rosteritem() { JID = instancefrom.JID.BareJID, Name = instancefrom.JID.User, Subscription = "subscribe" };
                        objUser.Roster.Add(rostersubscriber);
                    }

                    XMPPUserInstance instance = XMPPServer.Domain.UserList.FindBestUserInstance(jidto);
                    if ( (instance != null) && (instance.HasRequestRoster == true) )
                    {
                        instance.SendObject(iq);
                        rostersubscriber.Subscription = "none + pending in";
                    }

                    
                }

            }
            else if (iq.Type == "subscribed")
            {
                /// User has accepted a subscription... Change the state of the to user to "from", and notify all instances
                /// TODO...If they are in a different domain, do we add the from field before sending it to the server?
                JID jidto = iq.To;
                if (jidto.Domain == XMPPServer.Domain.DomainName)
                {
                    iq.From = instancefrom.JID;
                    XMPPUser objUserFrom = XMPPServer.Domain.UserList.FindUser(instancefrom.JID.User);
                    rosteritem rosterfrom = objUserFrom.FindRosterItem(iq.To.BareJID);
                    if (rosterfrom != null)
                    {
                        if (rosterfrom.Subscription.IndexOf("none") >= 0)
                            rosterfrom.Subscription = "to";
                        else if (rosterfrom.Subscription == "from")
                            rosterfrom.Subscription = "both";
                    }

                    XMPPUser objUserTo = XMPPServer.Domain.UserList.FindUser(jidto.User);
                    rosteritem rosterto = objUserFrom.FindRosterItem(instancefrom.JID.BareJID);
                    if (rosterto != null)
                    {
                        if (rosterto.Subscription.IndexOf("none") >= 0)
                            rosterto.Subscription = "from";
                        else if (rosterto.Subscription == "to")
                            rosterto.Subscription = "both";
                    }

                    /// Tell all the subscribed instances about our new roster item
                    RosterIQ riq = new RosterIQ();
                    riq.Query.RosterItems = new rosteritem[] { rosterfrom };
                    riq.From = null;
                    riq.Type = IQType.set.ToString();
                    foreach (XMPPUserInstance nextinstance in objUserFrom.UserInstances.GetAllUserInstances())
                    {
                        if (nextinstance.HasRequestRoster == false)
                            continue;

                        riq.To = nextinstance.JID;
                        nextinstance.SendObject(riq);
                    }

                    /// Tell all the subscriber instances about our new roster item
                    riq = new RosterIQ();
                    riq.Query.RosterItems = new rosteritem[] { rosterto };
                    riq.From = null;
                    riq.Type = IQType.set.ToString();
                    foreach (XMPPUserInstance nextinstance in objUserTo.UserInstances.GetAllUserInstances())
                    {
                        if (nextinstance.HasRequestRoster == false)
                            continue;

                        riq.To = nextinstance.JID;
                        nextinstance.SendObject(riq);
                    }
                }
            }

            return true;
        }
    }
}
