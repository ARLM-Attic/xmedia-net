using System;
using System.Runtime.Serialization;
using System.ServiceModel;

using System.Xml;
using System.Xml.Serialization;


namespace System.Net.XMPP
{
  

    /// <summary>
    /// An example of a class to use for pubsub.  A general alert event class.   This can be extended for other needs, but the basic members may cover the needs below
    /// Examples:
    /// Hardware outgage - Event: Outage, Message: "Hardware Failure",  Device: "Power supply xxx on computer yyy", Source: "Hardware Monitor"
    /// Rebooting server - Event: Maintenance, Message: "Server going down for 10 minutes", Device: "Server xyz", Source: me
    /// Door Bell Pressed- Event: Pressed, Message: "Front door bell pressed", Device: "house@xmedianet.com\frontdoorbell", Source: "HomeAutomation Server"
    /// PIR Motion detected- Event: Motion, Message: "Motion detected on front porch", Device: "house@xmedianet.com\frontdoorbell", Source: "HomeAutomation Server"
    /// Oil Change Due - Event: Appointment, Message: "Oil change due now", Device: "Nissan Altima", Source: "AppointmentManger"
    /// Dog left back yard - Event: GeoLeave, Message: "Dog is out chasing cats", Device: "Bad Dog", Source: "Dog monitoring system"
    /// </summary>
#if !WINDOWS_PHONE
    [DataContractFormat]
#endif
    [XmlRoot(ElementName = "Alert", Namespace = null)]
    public class AlertMessage : System.ComponentModel.INotifyPropertyChanged, IComparable<AlertMessage>
    {
        public AlertMessage()
        {
        }

        public AlertMessage(string strAlertNode, string strEvent, string strSource, string strDevice, string strMessage)
        {
            AlertNode = strAlertNode;
            Device = strDevice;
            Source = strSource;
            Event = strEvent;
            Message = strMessage;
        }

        ///  Event Ideas - not complete
        public static string EventLogin = "Login";
        public static string EventOutage = "Outage";
        public static string EventMotion = "Motion";
        public static string EventMaintenance = "Maintenance";
        public static string EventAppointment = "Appointment";
        public static string EventGeoEnter = "GeoEnter";
        public static string EventGeoLeave = "GeoLeave";


        private string m_strAlertNode = "Alerts";
        [XmlElement(ElementName = "AlertNode")]
        [DataMember]
        public string AlertNode
        {
            get { return m_strAlertNode; }
            set
            {
                if (m_strAlertNode != value)
                {
                    m_strAlertNode = value;
                    FirePropertyChanged("AlertNode");
                }
            }
        }

        

        public string m_strGuid = System.Guid.NewGuid().ToString();
        [XmlElement(ElementName = "Guid")]
        [DataMember]
        public string Guid
        {
            get
            {
                return m_strGuid;
            }
            set
            {
                if (m_strGuid != value)
                {
                    m_strGuid = value;
                    FirePropertyChanged("Guid");
                }
            }
        }

        public string m_strEvent = "";
        [XmlElement(ElementName = "Event")]
        [DataMember]
        public string Event
        {
            get
            {
                return m_strEvent;
            }
            set
            {
                if (m_strEvent != value)
                {
                    m_strEvent = value;
                    FirePropertyChanged("Event");
                }
            }
        }


        public string m_strMessage = "";
        [XmlElement(ElementName = "Message")]
        [DataMember]
        public string Message
        {
            get
            {
                return m_strMessage;
            }
            set
            {
                if (m_strMessage != value)
                {
                    m_strMessage = value;
                    FirePropertyChanged("Message");
                }
            }
        }

        public System.DateTime m_dtTime = System.DateTime.UtcNow;
        [XmlElement(ElementName = "Time")]
        [DataMember]
        public DateTime Time
        {
            get
            {
                return m_dtTime;
            }
            set
            {
                if (m_dtTime != value)
                {
                    m_dtTime = value;
                    FirePropertyChanged("Time");
                }
            }
        }

        public string m_strSource = "";
        /// <summary>
        /// The user/system that originated this event
        /// </summary>
        [XmlElement(ElementName = "Source")]
        [DataMember]
        public string Source
        {
            get
            {
                return m_strSource;
            }
            set
            {
                if (m_strSource != value)
                {
                    m_strSource = value;
                    FirePropertyChanged("Source");
                }
            }
        }

        public string m_strDevice = "";
        /// <summary>
        /// The device this event is for
        /// </summary>
        [XmlElement(ElementName = "Device")]
        [DataMember]
        public string Device
        {
            get
            {
                return m_strDevice;
            }
            set
            {
                if (m_strDevice != value)
                {
                    m_strDevice = value;
                    FirePropertyChanged("Device");
                }
            }
        }

        private string m_strHandledBy = null;
        /// <summary>
        /// If set, shows that the event has handled by the specified user/system, so others know this event needs no response
        /// </summary>
        [XmlElement(ElementName = "HandledBy")]
        [DataMember]
        public string HandledBy
        {
            get { return m_strHandledBy; }
            set
            {
                if (m_strHandledBy != value)
                {
                    m_strHandledBy = value;
                    FirePropertyChanged("HandledBy");
                    FirePropertyChanged("HandledVisible");
                    FirePropertyChanged("HandledNotVisible");
                    FirePropertyChanged("IsHandled");
                }
            }
        }

        private DateTime m_dtHandledDateUTC ;
        [XmlElement(ElementName = "HandledDate")]
        [DataMember]
        public DateTime HandldeDate
        {
            get { return m_dtHandledDateUTC; }
            set 
            {
                if (m_dtHandledDateUTC != value)
                {
                    m_dtHandledDateUTC = value;
                    FirePropertyChanged("HandledDate");
                }
            }
        }

        private bool m_bIsHandled = false;

        public bool IsHandled
        {
            get 
            { 
                if ( (HandledBy == null) || (HandledBy.Length <= 0))
                    return false;
                return true;
            }
            set {}
        }

#if !MONO
        public System.Windows.Visibility HandledVisible
        {
            get
            {
                return IsHandled ? System.Windows.Visibility.Visible : Windows.Visibility.Collapsed;
            }
            set { }
        }

        public System.Windows.Visibility HandledNotVisible
        {
            get
            {
                return IsHandled ? System.Windows.Visibility.Collapsed : Windows.Visibility.Visible;
            }
            set { }
        }

#endif


        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged = null;
        void FirePropertyChanged(string strProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strProperty));
            }
        }


        public int CompareTo(AlertMessage other)
        {
            return this.Time.CompareTo(other.Time);
        }
    }
}
