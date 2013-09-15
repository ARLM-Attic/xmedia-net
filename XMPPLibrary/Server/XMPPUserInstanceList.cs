using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Net.XMPP.Server
{
    public class XMPPUserInstanceList
    {
        public XMPPUserInstanceList()
        {
        }


        //List<XMPPUser> Users = new List<XMPPUser>();
        Dictionary<string, XMPPUserInstance> m_dicUserInstances = new Dictionary<string, XMPPUserInstance>();
        object m_objLockUsers = new object();

        public XMPPUserInstance[] GetAllUserInstances()
        {
            lock (m_objLockUsers)
            {
                return m_dicUserInstances.Values.ToArray();
            }
        }

        public XMPPUserInstance FindUserInstance(string strResource)
        {
            lock (m_objLockUsers)
            {
                if (m_dicUserInstances.ContainsKey(strResource) == true)
                    return m_dicUserInstances[strResource];
            }
            return null;
        }

        public XMPPUserInstance FindHighestPriorityUserInstance(string strResource)
        {
            XMPPUserInstance Bestest = null;
            lock (m_objLockUsers)
            {
                foreach (XMPPUserInstance ins in m_dicUserInstances.Values)
                {
                    if (ins.PresenceStatus.PresenceType == PresenceType.unavailable)
                        continue;

                    if (Bestest == null)
                        Bestest = ins;
                    else if (Bestest.Priority < ins.Priority)
                        Bestest = ins;
                }
            }
            return Bestest;
        }

        public void AddUserInstance(XMPPUserInstance objUser)
        {
            lock (m_objLockUsers)
            {
                if (m_dicUserInstances.ContainsKey(objUser.JID.Resource) == false)
                    m_dicUserInstances.Add(objUser.JID.Resource, objUser);
            }
        }

        public XMPPUserInstance RemoveUserInstance(string strResource)
        {
            lock (m_objLockUsers)
            {
                if (m_dicUserInstances.ContainsKey(strResource) == true)
                {
                    XMPPUserInstance objUser = m_dicUserInstances[strResource];
                    m_dicUserInstances.Remove(strResource);
                    return objUser;
                }
            }
            return null;
        }

    }
}
