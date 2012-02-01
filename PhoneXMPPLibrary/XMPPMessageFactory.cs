using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Xml.Linq;

namespace PhoneXMPPLibrary
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
            if ( (elem.FirstNode != null) && (elem.FirstNode is XElement) )
            {
               if (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/disco#info}query")
               {
                   return new ServiceDiscoveryIQ(strXML, ServiceDiscoveryType.info);
               }
               else if (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/disco#items}query")
               {
                   return new ServiceDiscoveryIQ(strXML, ServiceDiscoveryType.items);
               }
               else if (((XElement)elem.FirstNode).Name == "{jabber:iq:roster}query")
               {
                   return new RosterIQ(strXML);
               }
               else if (((XElement)elem.FirstNode).Name == "{urn:xmpp:jingle:1}jingle")
               {
                   return new JingleMessage(strXML);
               }
               else if (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/si}si")
               {
                   return new StreamInitIQ(strXML);
               }
               else if (((XElement)elem.FirstNode).Name == "{http://jabber.org/protocol/pubsub}pubsub")
               {
                   return new PubSubPublishIQ(strXML);
               }

                
            }

            return new IQ(strXML);
        }
    }
}
