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
                            presencefrom.To = instance.JID;
                            presencefrom.PresenceStatus = instancefrom.PresenceStatus;
                            instance.SendObject(presencefrom);

                            presenceto.From = instance.JID;
                            presenceto.PresenceStatus = instance.PresenceStatus;
                            instancefrom.SendObject(presenceto);

                        }
                        else if ((instance.PresenceStatus.IsOnline == true) && (instancefrom.PresenceStatus.IsOnline == false))
                        {
                            presencefrom.To = instance.JID;
                            presencefrom.PresenceStatus = instancefrom.PresenceStatus;
                            instance.SendObject(presencefrom);
                        }
                    }
                }
            }

            return true;
        }

    }
}
