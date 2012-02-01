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

using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using System.Text.RegularExpressions;


namespace PhoneXMPPLibrary
{
    // We have iq's, responses, and messages, each with their own xml
    public enum IQType
    {
        get,
        set,
        result,
        error,
    }

    public class XMPPMessageBase
    {
        public XMPPMessageBase(string strXML, string strNodeName)
        {
            NodeName = strNodeName;
            MessageXML = strXML;
        }

        private XElement m_objInitalXMLElement = null;
        [XmlIgnore()]
        public XElement InitalXMLElement
        {
            get { return m_objInitalXMLElement; }
            protected set { m_objInitalXMLElement = value; }
        }

        private string m_strNodeName = "unknown";
        [XmlIgnore()]
        public string NodeName
        {
            get { return m_strNodeName; }
            set { m_strNodeName = value; }
        }


        [XmlAttribute(AttributeName = "from")]  /// can't serialize JID as an attribute, so add this
        public string FromString
        {
            get
            {
                return m_objJIDFrom;
            }
            set
            {
                m_objJIDFrom = value;
            }
        }

        private JID m_objJIDFrom = new JID();
        [XmlIgnore()]
        public JID From
        {
            get { return m_objJIDFrom; }
            set { m_objJIDFrom = value; }
        }

        [XmlAttribute(AttributeName = "to")]
        public string ToString
        {
            get
            {
                return m_objJIDTo;
            }
            set
            {
                m_objJIDTo = value;
            }
        }

        private JID m_objJIDTo = new JID();
        [XmlIgnore()]
        public JID To
        {
            get { return m_objJIDTo; }
            set { m_objJIDTo = value; }
        }

        private string m_strID = Guid.NewGuid().ToString();
        [XmlAttribute(AttributeName = "id")]
        public string ID
        {
            get { return m_strID; }
            set { m_strID = value; }
        }


        private string m_strType = null;
        [XmlAttribute(AttributeName = "type")]
        public string Type
        {
            get { return m_strType; }
            set { m_strType = value; }
        }

        private string m_strxmlns = "";
        /// <summary>
        /// The namespace to set in an outgoing message
        /// </summary>
        [XmlIgnore()]
        public string xmlns
        {
            get { return m_strxmlns; }
            set { m_strxmlns = value; }
        }

        private string m_strxmlnsfrom = "";
        /// <summary>
        /// The namespace in a parsed message
        /// </summary>
        [XmlIgnore()]
        public string xmlnsfrom
        {
            get { return m_strxmlnsfrom; }
            set { m_strxmlnsfrom = value; }
        }

        private string m_strInnerXML = "";
        [XmlIgnore()]
        public string InnerXML
        {
            get { return m_strInnerXML; }
            set { m_strInnerXML = value; }
        }

        /// <summary>
        /// When overriden in a derived class, gives the class an opportunity to build the InnerXML string from its data members
        /// </summary>
        public virtual void AddInnerXML(XElement elemMessage)
        {
            if ((InnerXML != null) && (InnerXML.Length > 0)) 
                elemMessage.Add(XElement.Parse(InnerXML));

        }

        /// <summary>
        /// When overriden in a derived class, gives the object an opportunity to parse the InnerXML string and set its data members
        /// </summary>
        public virtual void ParseInnerXML(XElement elem)
        {
            if (elem.FirstNode != null)
            {
                InnerXML = elem.FirstNode.ToString();
            }
        }

        [XmlIgnore()] /// stop recursion
        public virtual string MessageXML
        {
            get
            {
                if ((xmlns != null) && (xmlns.Length > 0))
                {
                    XNamespace xn = xmlns;
                    XDocument doc = new XDocument();

                    XElement elemMessage = new XElement(xn + NodeName);

                    doc.Add(elemMessage);

                    if (Type.Length > 0)
                        elemMessage.Add(new XAttribute("type", m_strType));
                    if (ID.Length > 0)
                        elemMessage.Add(new XAttribute("id", ID));
                    if (From != null)
                        elemMessage.Add(new XAttribute("from", From));
                    if (To != null)
                        elemMessage.Add(new XAttribute("to", To));

                    AddInnerXML(elemMessage);

                    /// Rebuild our InitialXMLElement
                    InitalXMLElement = XElement.Parse(doc.ToString());
                    return doc.ToString();
                }
                else
                {
                    InitalXMLElement = new XElement(NodeName);

                    if (Type.Length > 0)
                        InitalXMLElement.Add(new XAttribute("type", m_strType));
                    if (ID.Length > 0)
                        InitalXMLElement.Add(new XAttribute("id", ID));
                    if (From != null)
                        InitalXMLElement.Add(new XAttribute("from", From));
                    if (To != null)
                        InitalXMLElement.Add(new XAttribute("to", To));

                    AddInnerXML(InitalXMLElement);

                    return InitalXMLElement.ToString();
                }
 
            }
            set
            {
                if ((value == null) || (value.Length <= 0))
                    return;

                /// Parse the xml fragment
                /// 
                InitalXMLElement = XElement.Parse(value);
                XAttribute attrType = InitalXMLElement.Attribute("type");
                if (attrType != null) Type = attrType.Value;

                XAttribute attrId = InitalXMLElement.Attribute("id");
                if (attrId != null) ID = attrId.Value;

                XAttribute attrFrom = InitalXMLElement.Attribute("from");
                if (attrFrom != null) From = attrFrom.Value;

                XAttribute attrTo = InitalXMLElement.Attribute("to");
                if (attrTo != null) To = attrTo.Value;

                XAttribute attrxmlns = InitalXMLElement.Attribute("xmlns");
                if (attrxmlns != null) xmlnsfrom = attrxmlns.Value;

                ParseInnerXML(InitalXMLElement);
            }
        }
    }

    //<iq type="get" id="791-126" from="ninethumbs.com" to="ninethumbs.com/411f8597"><ping xmlns="urn:xmpp:ping"/></iq>

    public class IQ : XMPPMessageBase
    {
        public IQ(string strXML) : base(strXML, "iq")
        {
        }
        public IQ()
            : base(null, "iq")
        {
            this.Type = IQType.get.ToString();
        }

        public string error = null;

        public override void AddInnerXML(XElement elemMessage)
        {
            if (error != null)
                elemMessage.Add(new XElement("error", error));
            base.AddInnerXML(elemMessage);

        }

        public override void ParseInnerXML(XElement elem)
        {
            foreach (XElement elemerror in elem.Descendants("error"))
            {
                error = elemerror.Value;
                break;
            }
            base.ParseInnerXML(elem);
        }
    }



    public class Message : XMPPMessageBase
    {
        public Message()
            : base(null, "message")
        {
        }

        public Message(string strXML)
            : base(strXML, "message")
        {
        }

        public override void AddInnerXML(XElement elemMessage)
        {
            if (Delivered > DateTime.MinValue)
            {
                elemMessage.Add(new XElement("delay", new XAttribute("stamp", Delivered.ToString()), new XAttribute("from", From), new XAttribute("xmlns", "urn:xmpp:delay")));
            }
            if ((InnerXML != null) && (InnerXML.Length > 0))
                elemMessage.Add(XElement.Parse(InnerXML));

            base.AddInnerXML(elemMessage);
        }

        public override void ParseInnerXML(XElement elem)
        {
            foreach (XElement node in elem.Nodes())
            {
                if (node.Name == "{urn:xmpp:delay}delay")
                {
                    XAttribute attrts = node.Attribute("stamp");
                    if (attrts != null)
                    {
                        Delivered = DateTime.Parse(attrts.Value);
                    }
                }
            }

            base.ParseInnerXML(elem);
        }

     

        private DateTime m_dtDelivered = DateTime.MinValue;

        public DateTime Delivered
        {
            get { return m_dtDelivered; }
            set { m_dtDelivered = value; }
        }

    }

    public class ChatMessage : Message
    {

        public ChatMessage(string strXML)
            : base(strXML)
        {
        }

        public override void AddInnerXML(XElement elemMessage)
        {
            if ((Body != null) && (Body.Length > 0))
            {
                elemMessage.Add(new XElement("body", new XText(Body)));
            }
            base.AddInnerXML(elemMessage);
        }

        public override void ParseInnerXML(XElement elem)
        {
            foreach (XElement node in elem.Nodes())
            {
                if (node.Name == "body")
                {
                    Body = node.Value;
                }
                else if (node.Name == "{http://jabber.org/protocol/chatstates}active")
                {
                    ConversationState = ConversationState.active;
                }
                else if (node.Name == "{http://jabber.org/protocol/chatstates}paused")
                {
                    ConversationState = ConversationState.paused;
                }
                else if (node.Name == "{http://jabber.org/protocol/chatstates}composing")
                {
                    ConversationState = ConversationState.composing;
                }
            }
            base.ParseInnerXML(elem);
        }

        ConversationState m_eConversationState = ConversationState.none;

        public ConversationState ConversationState
        {
            get { return m_eConversationState; }
            set { m_eConversationState = value; }
        }

        private string m_strBody = null;

        public string Body
        {
            get { return m_strBody; }
            set { m_strBody = value; }
        }
    }




}
