using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.XMPP;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace SensorLibrary
{
    [DataContract]
    [XmlRoot(ElementName = "sensorevent", Namespace = "homeautomation:sensor")]
    public class SensorEvent
    {
        public SensorEvent()
        {
        }

        public override string ToString()
        {
            return m_strEvent;
        }

        private string m_strSource = null;
        [XmlElement(ElementName = "source")]
        [DataMember]
        public string Source
        {
            get { return m_strSource; }
            set { m_strSource = value; }
        }

        private string m_strEvent = null;
        [XmlElement(ElementName = "event")]
        [DataMember]
        public string Event
        {
            get { return m_strEvent; }
            set { m_strEvent = value; }
        }

        private DateTime m_dtStart;
        [XmlElement(ElementName = "start")]
        [DataMember]
        public DateTime Start
        {
            get { return m_dtStart; }
            set { m_dtStart = value; }
        }

        private DateTime m_dtEnd;
        [XmlElement(ElementName = "end")]
        [DataMember]
        public DateTime End
        {
            get { return m_dtEnd; }
            set { m_dtEnd = value; }
        }

        private string m_strValue = "0.0";
        [XmlElement(ElementName = "value")]
        [DataMember]
        public string Value
        {
            get { return m_strValue; }
            set { m_strValue = value; }
        }

        [XmlIgnore]
        public Guid GuidPublished = Guid.Empty;

    }

    /// <summary>
    /// the description of a property of an object, or a parameter to a function
    /// </summary>
    [XmlRoot(ElementName = "methodinformation", Namespace = "homeautomation:sensor")]
    public class PropertyInformation
    {
        public PropertyInformation()
            : base()
        {
        }

        private string m_strName = "";
        [XmlAttribute("name")]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        private string m_strType = "";
        [XmlAttribute("type")]
        public string Type
        {
            get { return m_strType; }
            set { m_strType = value; }
        }

    }

    /// <summary>
    /// The description of a method supported by an object, or an event
    /// </summary>
    [XmlRoot(ElementName = "methodinformation", Namespace = "homeautomation:sensor")]
    public class MethodInformation
    {
        public MethodInformation()
            : base()
        {
        }

        private string m_strName = "";
        [XmlAttribute("name")]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        PropertyInformation[] m_aParameters = new PropertyInformation[] { };
        [XmlElement("parameter")]
        public PropertyInformation[] Parameters
        {
            get { return m_aParameters; }
            set { m_aParameters = value; }
        }
    }

    /// <summary>
    /// Information about the methods and properties and events of a class
    /// </summary>
    [XmlRoot(ElementName = "classinformation", Namespace = "homeautomation:sensor")]
    public class ClassInformation
    {
        public ClassInformation()
            : base()
        {
        }

        private string m_strName = "";
        [XmlAttribute("name")]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        MethodInformation[] m_aMethods = new MethodInformation[] { };
        [XmlElement("method")]
        public MethodInformation[] Methods
        {
            get { return m_aMethods; }
            set { m_aMethods = value; }
        }

        PropertyInformation[] m_aProperties = new PropertyInformation[] { };
        [XmlElement("property")]
        public PropertyInformation[] Properties
        {
            get { return m_aProperties; }
            set { m_aProperties = value; }
        }

        MethodInformation[] m_aEvents = new MethodInformation[] { };
        [XmlElement("event")]
        public MethodInformation[] Events
        {
            get { return m_aEvents; }
            set { m_aEvents = value; }
        }
    }

    [XmlRoot(ElementName = "parameter", Namespace = "homeautomation:sensor")]
    public class Parameter
    {
        public Parameter()
            : base()
        {
        }

        private string m_strName = "";
        [XmlAttribute("name")]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        private string m_strValue = "";
        [XmlAttribute("value")]
        public string Value
        {
            get { return m_strValue; }
            set { m_strValue = value; }
        }
    }



    public enum Verb
    {
        call,   // call a method, parameters must be populated
        get,    // get a property
        set,    // set a property, first parameter is the value
        query,  // query a list of functions and properties for this device, returns ClassInformation
    }


    [XmlRoot(ElementName = "action", Namespace = "homeautomation:sensor")]
    public class Action
    {
        public Action()
            : base()
        {
        }

        public Action(string strVerb)
            : base()
        {
            MethodOrMember = strVerb;
        }

        private Verb m_eVerb = Verb.call;
        [XmlElement("verb")]
        public Verb Verb
        {
            get { return m_eVerb; }
            set { m_eVerb = value; }
        }

        private string m_strMethodOrMember = "none";
        [XmlElement("methodormember")]
        public string MethodOrMember
        {
            get { return m_strMethodOrMember; }
            set { m_strMethodOrMember = value; }
        }


        private Parameter[] m_aParameters = new Parameter[] { };
        [XmlElement("parameter")]
        public Parameter[] Parameters
        {
            get { return m_aParameters; }
            set { m_aParameters = value; }
        }


    }

    [XmlRoot(ElementName = "result", Namespace = "homeautomation:sensor")]
    public class Result
    {
        public Result()
            : base()
        {
        }

        public Result(string strError)
            : base()
        {
            Error = strError;
        }



        private string m_strError = "";
        [XmlElement("error")]
        public string Error
        {
            get { return m_strError; }
            set { m_strError = value; }
        }

        private Parameter[] m_aParameters = new Parameter[] { };
        [XmlElement("parameter")]
        public Parameter[] Parameters
        {
            get { return m_aParameters; }
            set { m_aParameters = value; }
        }


    }


    [XmlRoot(ElementName = "iq")]
    public class SensorActionIQ : IQ
    {
        public SensorActionIQ()
            : base()
        {
            this.Type = IQType.set.ToString();
        }

        private Action m_objAction = new Action();
        [XmlElement("action", Namespace = "homeautomation:sensor")]
        public Action Action
        {
            get { return m_objAction; }
            set { m_objAction = value; }
        }

    }

    [XmlRoot(ElementName = "iq")]
    public class SensorResultIQ : IQ
    {
        public SensorResultIQ()
            : base()
        {
            this.Type = IQType.result.ToString();
        }


        private Result m_objResults = null;
        [XmlElement("result", Namespace = "homeautomation:sensor")]
        public Result Result
        {
            get { return m_objResults; }
            set { m_objResults = value; }
        }

        private ClassInformation m_objClassInfo = null;
        [XmlElement("classinformation", Namespace = "homeautomation:sensor")]
        public ClassInformation ClassInfo
        {
            get { return m_objClassInfo; }
            set { m_objClassInfo = value; }
        }

    }


    [XmlRoot(ElementName = "message")]
    public class SensorEventMessage : Message
    {
        public SensorEventMessage()
            : base()
        {
            this.Delivered = null;
        }


        private SensorEvent m_objEvent = new SensorEvent();
        [XmlElement("event", Namespace = "homeautomation:sensor")]
        public SensorEvent Event
        {
            get { return m_objEvent; }
            set { m_objEvent = value; }
        }
    }


    public class SensorMessagBuilder : Logic, IXMPPMessageBuilder
    {
        public SensorMessagBuilder(XMPPClient client)
            : base(client)
        {
            if (XMPPClient.OurServiceDiscoveryFeatureList.HasFeature(ServiceString) == false) //register this with service discover
                XMPPClient.OurServiceDiscoveryFeatureList.AddFeature(new feature(ServiceString));

            /// Register this object as a message parser, and add this logic to our XMPP client instance
            /// Other user services don't have to follow this pattern, then can add these two steps where ever
            XMPPClient.XMPPMessageFactory.AddMessageBuilder(this);
            XMPPClient.AddLogic(this);

        }

        public const string ServiceString = "homeautomation:sensor/v1.0";

        #region IXMPPMessageBuilder Members

        public Message BuildMessage(System.Xml.Linq.XElement elem, string strXML)
        {
            foreach (XElement nextelem in elem.Descendants())
            {
                if (nextelem.Name == "{homeautomation:sensor}event")
                {
                    SensorEventMessage actionmsg = Utility.ParseObjectFromXMLString(strXML, typeof(SensorEventMessage)) as SensorEventMessage;
                    return actionmsg;
                }
            }

            return null;
        }

        public IQ BuildIQ(System.Xml.Linq.XElement elem, string strXML)
        {

            if ((elem.FirstNode != null) && (elem.FirstNode is XElement) && (((XElement)(elem.FirstNode)).Name == "{homeautomation:sensor}action"))
            {
                SensorActionIQ actioniq = Utility.ParseObjectFromXMLString(strXML, typeof(SensorActionIQ)) as SensorActionIQ;
                return actioniq;
            }
            else if ((elem.FirstNode != null) && (elem.FirstNode is XElement) && (((XElement)(elem.FirstNode)).Name == "{homeautomation:sensor}result"))
            {
                SensorResultIQ resultiq = Utility.ParseObjectFromXMLString(strXML, typeof(SensorResultIQ)) as SensorResultIQ;
                return resultiq;
            }
            else if ((elem.FirstNode != null) && (elem.FirstNode is XElement) && (((XElement)(elem.FirstNode)).Name == "{homeautomation:sensor}classinformation"))
            {
                SensorResultIQ resultiq = Utility.ParseObjectFromXMLString(strXML, typeof(SensorResultIQ)) as SensorResultIQ;
                return resultiq;
            }

            return null;
        }



        public PresenceMessage BuildPresence(XElement elem, string strXML)
        {
            return null;
        }

        #endregion

        public delegate void DelegateSensorEvent(SensorEvent sevent, RosterItem item, XMPPClient client);
        public event DelegateSensorEvent OnSensorEvent = null;

        public override bool NewMessage(Message msg)
        {
            if (msg is SensorEventMessage)
            {

                SensorEventMessage pmsg = msg as SensorEventMessage;
                RosterItem item = XMPPClient.FindRosterItem(msg.From);
                if (OnSensorEvent != null)
                    OnSensorEvent(pmsg.Event, item, XMPPClient);

                return true;
            }

            return false;
        }

    }

    public delegate void DelegateSensorEvent(object obj, SensorEvent eventinfo);

    public class SensorPropertyAttribute : Attribute
    {
        public SensorPropertyAttribute()
            : base()
        {
        }

    }

    public class SensorActionAttribute : Attribute
    {
        public SensorActionAttribute()
            : base()
        {

        }

    }

    public class SensorEventAttribute : Attribute
    {
        public SensorEventAttribute()
            : base()
        {

        }

    }

}
