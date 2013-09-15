using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;

namespace System.Net.XMPP.Server
{
    
    /// <summary>
    /// An instance of a user on the server
    ///  Implements XMPPServerLogic as a way of handling messages addresses to this user
    /// </summary>
    public class XMPPUser : XMPPServerLogic
    {
        public XMPPUser(XMPPServer server)
            : base(server, null)
        {
        }

        private string m_strUserName = "";
        [DataMember]
        public string UserName
        {
            get { return m_strUserName; }
            set { m_strUserName = value; }
        }

        private string m_strPassword = "";
        [DataMember]
        public string Password
        {
            get { return m_strPassword; }
            set { m_strPassword = value; }
        }

        private XMPPUserInstanceList m_objUserInstances = new XMPPUserInstanceList();
        public XMPPUserInstanceList UserInstances
        {
            get { return m_objUserInstances; }
            set { m_objUserInstances = value; }
        }

        private List<rosteritem> m_listRoster = new List<rosteritem>();
        /// <summary>
        /// A list of objects subscribed to this user
        /// </summary>
        [DataMember]
        public List<rosteritem> Roster
        {
            get { return m_listRoster; }
            set { m_listRoster = value; }
        }

        public rosteritem FindRosterItem(JID barejid)
        {
            foreach (rosteritem item in Roster)
            {
                if (item.JID == barejid)
                    return item;
            }

            return null;
        }

        public bool AuthenticateUser(string strPassword)
        {
            if (Password == strPassword)
                return true;

            return false;
        }

        public static string ValidateResource(string strResource)
        {
            strResource = strResource.Replace("@", "");
            strResource = strResource.Replace("/", "");

            strResource = strResource.Trim();
            if (strResource.Length <= 0)
                strResource = Guid.NewGuid().ToString();

            return strResource;
        }

        public XMPPUserInstance BindUserInstance(XMPPUserInstance instance, string strPreferredResource)
        {
            strPreferredResource = ValidateResource(strPreferredResource);

            XMPPUserInstance existinginstance = UserInstances.FindUserInstance(strPreferredResource);
            if (existinginstance == null)
            {
                instance.JID.Resource = strPreferredResource;
                UserInstances.AddUserInstance(instance);
                return instance;
            }

            instance.JID.Resource = Guid.NewGuid().ToString();
            UserInstances.AddUserInstance(instance);

            return instance;
        }

        public void RemoveInstance(XMPPUserInstance instance)
        {
            UserInstances.RemoveUserInstance(instance.JID.Resource);
        }

        public XMPPUserInstance FindBestJID(JID jidfind)
        {
            XMPPUserInstance instance = UserInstances.FindUserInstance(jidfind.Resource);
            if (instance != null)
                return instance;

            // Find one with highest priority
            return UserInstances.FindHighestPriorityUserInstance(jidfind.Resource);
        }

        public override bool NewIQ(IQ iq, XMPPUserInstance instancefrom)
        {
            XMPPUserInstance instance = FindBestJID(iq.To);
            if (instance != null)
                instance.SendObject(iq);
            else
            {
                /// Send this back to the user that sent it, we don't know where it came from
                /// 
                instance = XMPPServer.Domain.UserList.FindUserInstance(iq.From);
                if (instance != null)
                {
                    iq.To = iq.From;
                    iq.From = XMPPServer.Domain.DomainName;
                    iq.Type = IQType.error.ToString();
                    instance.SendObject(iq);
                }
                else
                {
                    // Ignore
                }
            }
            return true;
        }

        public override bool NewMessage(Message iq, XMPPUserInstance instancefrom)
        {
            XMPPUserInstance instance = FindBestJID(iq.To);
            if ( (iq is ChatMessage) && (instance != null) )
                instance.SendXMPP(iq); // Chat message uses internal serializer
            else if (instance != null)
                instance.SendObject(iq);
           
            return true;
        }

        public override bool NewPresence(PresenceMessage iq, XMPPUserInstance instancefrom)
        {
            // Should never hit this
            return true;
        }
    }


    public class XMPPUserList 
    {
        public XMPPUserList()
        {
        }


        //List<XMPPUser> Users = new List<XMPPUser>();
        Dictionary<string, XMPPUser> m_dicUsers = new Dictionary<string, XMPPUser>();
        object m_objLockUsers = new object();

        public XMPPUser [] GetAllUsers()
        {
            lock (m_objLockUsers)
            {
                return m_dicUsers.Values.ToArray();
            }
        }

        public XMPPUser FindUser(string strUserName)
        {
            lock (m_objLockUsers)
            {
                if (m_dicUsers.ContainsKey(strUserName) == true)
                    return m_dicUsers[strUserName];
            }
            return null;
        }

        public XMPPUserInstance FindUserInstance(JID fulljid)
        {
            XMPPUser user = FindUser(fulljid.User);
            if (user != null)
                return user.UserInstances.FindUserInstance(fulljid.Resource);
            return null;
        }

        public XMPPUserInstance FindBestUserInstance(JID fulljid)
        {
            XMPPUser user = FindUser(fulljid.User);
            if (user != null)
                return user.UserInstances.FindHighestPriorityUserInstance(fulljid.Resource);
            return null;
        }

        public XMPPUser AddUser(XMPPUser objUser)
        {
            lock (m_objLockUsers)
            {
                if (m_dicUsers.ContainsKey(objUser.UserName) == false)
                    m_dicUsers.Add(objUser.UserName, objUser);
            }

            return objUser;
        }

        public XMPPUser RemoveUser(string strUserName)
        {
            lock (m_objLockUsers)
            {
                if (m_dicUsers.ContainsKey(strUserName) == true)
                {
                    XMPPUser objUser = m_dicUsers[strUserName];
                    m_dicUsers.Remove(strUserName);
                    return objUser;
                }
            }
            return null;
        }

    }
   
}
