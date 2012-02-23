using System;
using System.Net;

using System.Xml.Linq;

namespace System.Net.XMPP
{
    /// <summary>
    /// Creates a Message object or derived message object depending on the incoming xml
    /// </summary>
    public class XMPPMessageFactory
    {

        public virtual Message BuildMessage(XElement elem, string strXML)
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

        public virtual IQ BuildIQ(XElement elem, string strXML)
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

            return new IQ(strXML);
        }
    }
}
