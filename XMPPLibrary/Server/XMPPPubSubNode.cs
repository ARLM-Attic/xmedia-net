using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;

namespace System.Net.XMPP.Server
{
    public interface IPubSubChangeEvents
    {
        void ItemAdded(XMPPPubSubNode node, PubSubItem item);
        void ItemDeleted(XMPPPubSubNode node, PubSubItem item);
    }
    /// <summary>
    ///  A server side pub sub node
    /// </summary>
    public class XMPPPubSubNode
    {
        public XMPPPubSubNode(IPubSubChangeEvents notify)
        {
            NotifyHandler = notify;
        }

        IPubSubChangeEvents NotifyHandler = null;

        private PubSubConfigForm m_objNodeInfo = new PubSubConfigForm();
        public PubSubConfigForm NodeInfo
        {
            get { return m_objNodeInfo; }
            set { m_objNodeInfo = value; }
        }

        private Dictionary<string, JID> m_dicSubscribers = new Dictionary<string, JID>();
        public Dictionary<string, JID> Subscribers
        {
            get { return m_dicSubscribers; }
            set { m_dicSubscribers = value; }
        }

        private List<PubSubItem> m_listItems = new List<PubSubItem>();
        public List<PubSubItem> Items
        {
            get { return m_listItems; }
            set { m_listItems = value; }
        }

    }

    public class PubSubNodeList
    {
        public PubSubNodeList()
        {
        }


        //List<XMPPUser> Users = new List<XMPPUser>();
        Dictionary<string, XMPPPubSubNode> m_dicPubSubNodes = new Dictionary<string, XMPPPubSubNode>();
        object m_objLockNodes = new object();

        public XMPPPubSubNode[] GetAllNodes()
        {
            lock (m_objLockNodes)
            {
                return m_dicPubSubNodes.Values.ToArray();
            }
        }

        public XMPPPubSubNode FindNode(string strNodeName)
        {
            lock (m_objLockNodes)
            {
                if (m_dicPubSubNodes.ContainsKey(strNodeName) == true)
                    return m_dicPubSubNodes[strNodeName];
            }
            return null;
        }

        public XMPPPubSubNode AddNode(XMPPPubSubNode objNode)
        {
            lock (m_objLockNodes)
            {
                if (m_dicPubSubNodes.ContainsKey(objNode.NodeInfo.NodeName) == false)
                    m_dicPubSubNodes.Add(objNode.NodeInfo.NodeName, objNode);
            }

            return objNode;
        }

        public XMPPPubSubNode RemoveNode(string strNodeName)
        {
            lock (m_objLockNodes)
            {
                if (m_dicPubSubNodes.ContainsKey(strNodeName) == true)
                {
                    XMPPPubSubNode objUser = m_dicPubSubNodes[strNodeName];
                    m_dicPubSubNodes.Remove(strNodeName);
                    return objUser;
                }
            }
            return null;
        }

    }

    public class DomainPubSubLogic : XMPPServerLogic, IPubSubChangeEvents
    {
        public DomainPubSubLogic(XMPPServer server)
            : base(server, null)
        {
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#manage-subscriptions"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#modify-affiliations"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#retrieve-default"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#collections"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#config-node"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#item-ids"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#create-and-configure"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#publish"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#subscribe"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#retract-items"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#retrieve-items"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#create-nodes"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#persistent-items"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#presence-notifications"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#retrieve-affiliation"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#delete-nodes"));
            XMPPServer.Domain.OurServiceDiscoveryFeatureList.AddFeature(new feature("http://jabber.org/protocol/pubsub#purge-nodes"));

        }

        public override XMPPServerLogic Clone(XMPPUserInstance newclient)
        {
            return this;
        }

        public override bool NewIQ(IQ iq, XMPPUserInstance instancefrom)
        {
            if (iq is PubSubIQ)
            {
                // Look for pub sub create, subscribe, add, retract, etc operations

                return true;
            }

            return false;
        }

        public override bool NewMessage(Message iq, XMPPUserInstance instancefrom)
        {
            if (iq is PubSubEventMessage)
            {

                return true;
            }

            return false;
        }


        public void ItemAdded(XMPPPubSubNode node, PubSubItem item)
        {
            
        }

        public void ItemDeleted(XMPPPubSubNode node, PubSubItem item)
        {
            
        }
    }
}
