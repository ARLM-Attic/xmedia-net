using System;
using System.Net;

using System.Xml.Linq;
using System.Collections.Generic;

namespace System.Net.XMPP
{
    public interface IXMPPMessageBuilder
    {
        /// <summary>
        ///  Builds a message from the incoming XML, or null if it can't
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="strXML"></param>
        /// <returns></returns>
        Message BuildMessage(XElement elem, string strXML);
        
        /// <summary>
        /// Builds an IQ derived object from the incoming XML, or null if it can't
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="strXML"></param>
        /// <returns></returns>
        IQ BuildIQ(XElement elem, string strXML);
    }

    /// <summary>
    /// Creates a Message object or derived message object depending on the incoming xml
    /// </summary>
    public class XMPPMessageFactory
    {
        public XMPPMessageFactory()
        {
            this.AddMessageBuilder(new IncludedServicesMessageBuilder());
        }

        public void AddMessageBuilder(IXMPPMessageBuilder builder)
        {
            lock (BuilderLock)
            {
                if (m_listBuilders.Contains(builder) == false)
                    m_listBuilders.Insert(0, builder);
            }
        }

        public void RemoveMessageBuilder(IXMPPMessageBuilder builder)
        {
            lock (BuilderLock)
            {
                if (m_listBuilders.Contains(builder) == false)
                    m_listBuilders.Remove(builder);
            }
        }

        protected List<IXMPPMessageBuilder> m_listBuilders = new List<IXMPPMessageBuilder>();
        protected object BuilderLock = new object();

        public Message BuildMessage(XElement elem, string strXML)
        {
            lock (BuilderLock)
            {
                foreach (IXMPPMessageBuilder builder in m_listBuilders)
                {
                    Message msg = builder.BuildMessage(elem, strXML);
                    if (msg != null)
                        return msg;
                }
            }
            return null;
        }
        public IQ BuildIQ(XElement elem, string strXML)
        {

            lock (BuilderLock)
            {
                foreach (IXMPPMessageBuilder builder in m_listBuilders)
                {
                    IQ iq = builder.BuildIQ(elem, strXML);
                    if (iq != null)
                        return iq;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Knows how to parse messages for all services included in our library.  External services will need to provide their own
    /// and add them to the XMPPClient.XMPPMessageFactory
    /// </summary>
    public class IncludedServicesMessageBuilder : IXMPPMessageBuilder
    {
        public IncludedServicesMessageBuilder()
        {
        }

        public Message BuildMessage(XElement elem, string strXML)
        {
            /// Examine the type and see if we have classes for any of these
            XAttribute attrType = elem.Attribute("type");
            if (attrType != null)
            {
                if (attrType.Value == "chat")
                    return new ChatMessage(strXML);
            }
            else if (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/pubsub#event}event")
            {
                return new PubSubEventMessage(strXML);
            }

            return new Message(strXML);
        }

        public IQ BuildIQ(XElement elem, string strXML)
        {
            /// Check out our first node
            /// 

            string strType = "";
            if (elem.Attribute("type") != null)
                strType = elem.Attribute("type").Value;

            if ( (elem.FirstNode != null) && (elem.FirstNode is XElement) )
            {
               if (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/disco#info}query")
               {
                   ServiceDiscoveryIQ query = Utility.ParseObjectFromXMLString(strXML, typeof(ServiceDiscoveryIQ)) as ServiceDiscoveryIQ;
                   return query;
               }
               else if (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/disco#items}query")
               {
                   ServiceDiscoveryIQ query = Utility.ParseObjectFromXMLString(strXML, typeof(ServiceDiscoveryIQ)) as ServiceDiscoveryIQ;
                   return query;
               }
               else if (((XElement)elem.FirstNode).Name == "{jabber:iq:roster}query")
               {
                   return new RosterIQ(strXML);
               }
               else if (((XElement)elem.FirstNode).Name == "{urn:xmpp:jingle:1}jingle")
               {
                   Jingle.JingleIQ query = Utility.ParseObjectFromXMLString(strXML, typeof(Jingle.JingleIQ)) as Jingle.JingleIQ;
                   return query;
               }
               else if (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/si}si")
               {
                   return new StreamInitIQ(strXML);
               }
               else if ( (strType == "set") && (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/pubsub}pubsub") )
               {
                   return new PubSubPublishIQ(strXML);
               }
               else if ((strType == "get") && (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/pubsub}pubsub"))
               {
                   return new PubSubGetIQ(strXML);
               }
               else if ((strType == "result") && (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/pubsub}pubsub"))
               {
                   return new PubSubResultIQ(strXML);
               }
               else if (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/bytestreams}query")
               {
                   ByteStreamQueryIQ query = Utility.ParseObjectFromXMLString(strXML, typeof(ByteStreamQueryIQ)) as ByteStreamQueryIQ;
                   return query;
               }
                
            }

#if !WINDOWS_PHONE
            IQ iqret = Utility.ParseObjectFromXMLString(strXML, typeof(IQ)) as IQ;
            iqret.InitalXMLElement = elem;
            if (elem.FirstNode != null)
               iqret.InnerXML = elem.FirstNode.ToString();
            return iqret;
#else
            return new IQ(strXML);
#endif
        }
    }
}
