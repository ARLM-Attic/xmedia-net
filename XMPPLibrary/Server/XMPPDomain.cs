using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;

namespace System.Net.XMPP.Server
{
    /// <summary>
    ///  Holds an XMPP domain and all the users, nodes, etc.  These are loaded from database
    ///  Implements XMPPServerLogic as a way of handling messages addresses to this domain
    /// </summary>
    public class XMPPDomain : XMPPServerLogic
    {
        public XMPPDomain(XMPPServer server) : base(server, null)
        {
           
        }

        /// <summary>
        ///  Features and Items
        /// </summary>
        public ServiceDiscoveryFeatureList OurServiceDiscoveryFeatureList = new ServiceDiscoveryFeatureList();

        public override XMPPServerLogic Clone(XMPPUserInstance newclient)
        {
            return this; /// Give a copy of us to each incoming connection to handle message routing... Don't care about the specific instance
        }

        public XMPPDomain(string strName, XMPPServer server) : this(server)
        {
            DomainName = strName;
        }

        public void AddServiceLogic(XMPPServerLogic logic)
        {
            lock (LogicLock)
            {
                ActiveServices.Add(logic);
            }
        }

        protected List<XMPPServerLogic> ActiveServices = new List<XMPPServerLogic>();
        protected object LogicLock = new object();


        private string m_strDomainName = "";
        public string DomainName
        {
            get { return m_strDomainName; }
            set 
            { 
                m_strDomainName = value; 
            }
        }

        private XMPPUserList m_objUserList = new XMPPUserList();
        public XMPPUserList UserList
        {
            get { return m_objUserList; }
            set { m_objUserList = value; }
        }

        private PubSubNodeList m_objNodeList = new PubSubNodeList();
        public PubSubNodeList NodeList
        {
            get { return m_objNodeList; }
            set { m_objNodeList = value; }
        }

        public bool AuthenticateUser(string strUserName, string strPassword)
        {
            XMPPUser user = UserList.FindUser(strUserName);
            if (user == null)
                return false;
            return user.AuthenticateUser(strPassword);
        }

        public override bool NewIQ(IQ iq, XMPPUserInstance instancefrom)
        {
            if ((iq.To == null) || (iq.To == "") )
               return HandleDomainIQ(iq, instancefrom);

            if (string.Compare(iq.To.Domain, this.DomainName, true) == 0)
            {
                if ((iq.To.User == null) || (iq.To.User.Length <= 0)) 
                {
                    // A message for us
                    return HandleDomainIQ(iq, instancefrom);
                }
                else
                {
                    XMPPUser user = UserList.FindUser(iq.To.User);
                    if (user != null)
                        return user.NewIQ(iq, instancefrom);
                    else
                    {

                    }
                }

            }
            return true;
        }

        public override bool NewMessage(Message iq, XMPPUserInstance instancefrom)
        {
            if (iq.To == null)
                return false;

            if (string.Compare(iq.To.Domain, this.DomainName, true) == 0)
            {
                if ((iq.To.User == null) || (iq.To.User.Length <= 0))
                {
                    // A message for us
                    return HandleDomainMessage(iq, instancefrom);
                }
                else
                {
                    XMPPUser user = UserList.FindUser(iq.To.User);
                    if (user != null)
                        return user.NewMessage(iq, instancefrom);
                    else
                    {

                    }
                }

            }
            return true;
        }

        public override bool NewPresence(PresenceMessage iq, XMPPUserInstance instancefrom)
        {
            if (iq.From == null)
                return false;


            HandleDomainPresence(iq, instancefrom);
          

            return true;
        }

        public bool HandleDomainIQ(IQ iq, XMPPUserInstance instancefrom)
        {
            bool bHandled = false;
            XMPPServerLogic[] LogicList = null;
            List<XMPPServerLogic> RemoveList = new List<XMPPServerLogic>();

            lock (LogicLock)
            {
                LogicList = ActiveServices.ToArray();
            }

            foreach (XMPPServerLogic log in LogicList)
            {
                bHandled = log.NewIQ(iq, instancefrom);

                if (log.IsCompleted == true)
                    RemoveList.Add(log);
                if (bHandled == true)
                    break;

            }

            if (RemoveList.Count > 0)
            {
                lock (LogicLock)
                {
                    foreach (XMPPServerLogic log in RemoveList)
                    {
                        if (ActiveServices.Contains(log) == true)
                            ActiveServices.Remove(log);
                    }
                }
            }

            if (bHandled == false)
            {
                /// Default
                /// 
                XMPPUserInstance ui = instancefrom; // UserList.FindUserInstance(iq.From);
                if (ui != null)
                {
                    iq.To = iq.From;
                    iq.Type = IQType.error.ToString();
                    iq.From = DomainName;
                    ui.SendObject(iq);
                }
            }

            return true;
        }

        public bool HandleDomainMessage(Message iq, XMPPUserInstance instancefrom)
        {
            bool bHandled = false;
            XMPPServerLogic[] LogicList = null;
            List<XMPPServerLogic> RemoveList = new List<XMPPServerLogic>();

            lock (LogicLock)
            {
                LogicList = ActiveServices.ToArray();
            }

            foreach (XMPPServerLogic log in LogicList)
            {
                bHandled = log.NewMessage(iq, instancefrom);

                if (log.IsCompleted == true)
                    RemoveList.Add(log);
                if (bHandled == true)
                    break;

            }

            if (RemoveList.Count > 0)
            {
                lock (LogicLock)
                {
                    foreach (XMPPServerLogic log in RemoveList)
                    {
                        if (ActiveServices.Contains(log) == true)
                            ActiveServices.Remove(log);
                    }
                }
            }

            return true;
        }

        public bool HandleDomainPresence(PresenceMessage iq, XMPPUserInstance instancefrom)
        {
            bool bHandled = false;
            XMPPServerLogic[] LogicList = null;
            List<XMPPServerLogic> RemoveList = new List<XMPPServerLogic>();

            lock (LogicLock)
            {
                LogicList = ActiveServices.ToArray();
            }

            foreach (XMPPServerLogic log in LogicList)
            {
                bHandled = log.NewPresence(iq, instancefrom);

                if (log.IsCompleted == true)
                    RemoveList.Add(log);
                if (bHandled == true)
                    break;

            }

            if (RemoveList.Count > 0)
            {
                lock (LogicLock)
                {
                    foreach (XMPPServerLogic log in RemoveList)
                    {
                        if (ActiveServices.Contains(log) == true)
                            ActiveServices.Remove(log);
                    }
                }
            }

            return true;
        }
       
    }
}
