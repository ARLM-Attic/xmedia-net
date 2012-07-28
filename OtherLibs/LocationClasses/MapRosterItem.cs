using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Net.XMPP;
using System.Web.Script.Serialization;

namespace LocationClasses
{
    // MapRosterItem is BuddyPosition - maybe switch to use BuddyPosition!
    [DataContract]
    public class MapRosterItem : INotifyPropertyChanged, IComparable
    {
        public MapRosterItem()
        {

        }

        public MapRosterItem(XMPPAccount account)
        {
            // create roster item from account

        }

        public MapRosterItem(RosterItem item)
        {
            RosterItem = item;
            ((INotifyPropertyChanged)item).PropertyChanged += new PropertyChangedEventHandler(MapRosterItem_PropertyChanged);
        }

        public static MapRosterItem Create(RosterItem rosterItem)
        {
            MapRosterItem mapRosterItem = new MapRosterItem() { RosterItem = rosterItem };
            return mapRosterItem;
        }

        void MapRosterItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GeoLoc")
            {
                /// New geolocation, add it to our list
                /// 
                GeoCoordinate coord = new GeoCoordinate(RosterItem.GeoLoc.lat, RosterItem.GeoLoc.lon, RosterItem.GeoLoc.TimeStamp);
                CoordinateList.Add(coord);
                FirePropertyChanged("Count");

                //FirePropertyChanged("MapImage");
                FirePropertyChanged("RosterItem");

            }
            else if (e.PropertyName == "Avatar" || e.PropertyName == "Presence" || e.PropertyName == "AvatarPath" || e.PropertyName == "Name")
            {

            }
        }

        private List<GeoCoordinate> m_listCoordinateList = new List<GeoCoordinate>();

        public List<GeoCoordinate> CoordinateList
        {
            get { return m_listCoordinateList; }
            set { m_listCoordinateList = value; }
        }

        public int Count
        {
            get
            {
                return CoordinateList.Count;
            }
            set
            {
            }
        }

        private List<JSONSerializedObject> m_JSONList = new List<JSONSerializedObject>();

        public List<JSONSerializedObject> JSONList
        {
            get { return m_JSONList; }
            set { m_JSONList = value; }
        }

        private string m_strRosterItemJSONSerialized = "";
        [DataMember]
        public string strRosterItemJSONSerialized
        {
            get { return m_strRosterItemJSONSerialized; }
            set { m_strRosterItemJSONSerialized = value; }
        }

        private string m_strMapRosterItemJSONSerialized = "";
        [DataMember]
        public string strMapRosterItemJSONSerialized
        {
            get
            {

                JavaScriptSerializer ser = new JavaScriptSerializer();
                m_strMapRosterItemJSONSerialized = ser.Serialize(this);
                System.Console.WriteLine("serialized roster item: " + m_strMapRosterItemJSONSerialized);



                return m_strMapRosterItemJSONSerialized;
            }
            // set { m_strMapRosterItemJSONSerialized = value; }
        }

        private KMLBuilderForRosterItem m_KMLBuilderForRosterItem = new KMLBuilderForRosterItem();
        [DataMember]
        public KMLBuilderForRosterItem KMLBuilderForRosterItem
        {
            get { return m_KMLBuilderForRosterItem; }
            set { m_KMLBuilderForRosterItem = value; }
        }

        public MyKML BuildKML()
        {
            string strXML = "";
            MyKML kml = new MyKML();

            kml.Document.Name = string.Format("Buddy {0} coordinates for {1}", RosterItem.JID.BareJID, DateTime.Now);

            int i = 1;
            foreach (GeoCoordinate coord in KMLBuilderForRosterItem.CoordinateList)
            {
                string strNextName = i.ToString();
                kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
                i++;
            }
            kml.Document.Placemarks.Add(new Placemark("Total Path", KMLBuilderForRosterItem.CoordinateList));
            return kml;
            // strXML = GetXMLStringFromObject(kml);
            // return strXML;
        }

        private geoloc m_PreviousLocation = new geoloc();

        public geoloc PreviousLocation
        {
            get { return m_PreviousLocation; }
            set
            {
                m_PreviousLocation = value;

                FirePropertyChanged("PreviousLocation");
            }
        }

        private geoloc m_CurrentLocation = new geoloc();

        public geoloc CurrentLocation
        {
            get { return m_CurrentLocation; }
            set
            {
                m_CurrentLocation = value;
                FirePropertyChanged("CurrentLocation");
            }
        }


        private RosterItem m_RosterItem = null;
        [DataMember]
        public RosterItem RosterItem
        {
            get { return m_RosterItem; }
            set
            {
                if (m_RosterItem != value)
                {
                    m_RosterItem = value;
                    FirePropertyChanged("RosterItem");
                }
            }
        }

        private bool m_bIsDisplayedInViewableWindow = false;
        [DataMember]
        public bool IsDisplayedInViewableWindow
        {
            get { return m_bIsDisplayedInViewableWindow; }
            set
            {
                if (m_bIsDisplayedInViewableWindow != value)
                {
                    m_bIsDisplayedInViewableWindow = value;
                    FirePropertyChanged("IsDisplayedInViewableWindow");
                }
            }
        }

        private bool m_bIsDisplayed = true;
        [DataMember]
        public bool IsDisplayed
        {
            get { return m_bIsDisplayed; }
            set
            {
                if (m_bIsDisplayed != value)
                {
                    m_bIsDisplayed = value;
                    FirePropertyChanged("IsDisplayed");
                }
            }
        }

        private bool m_bIsMe = false;
        [DataMember]
        public bool IsMe
        {
            get { return m_bIsMe; }
            set
            {
                if (m_bIsMe != value)
                {
                    m_bIsMe = value;
                    FirePropertyChanged("IsMe");
                }
            }
        }

        private bool m_bIsTheMainRosterItem = false;
        [DataMember]
        public bool IsTheMainRosterItem
        {
            get { return m_bIsTheMainRosterItem; }
            set
            {
                if (m_bIsTheMainRosterItem != value)
                {
                    m_bIsTheMainRosterItem = value;
                    FirePropertyChanged("IsTheMainRosterItem");
                }
            }
        }

        private DateTime m_dtDateTimeEnqueued = DateTime.MinValue;
        [DataMember]
        public DateTime DateTimeEnqueued
        {
            get { return m_dtDateTimeEnqueued; }
            set
            {
                if (m_dtDateTimeEnqueued != value)
                {
                    m_dtDateTimeEnqueued = value;
                    FirePropertyChanged("DateTimeEnqueued");
                }
            }
        }

        private int m_zIndex = 0;
        [DataMember]
        public int zIndex
        {
            get { return m_zIndex; }
            set
            {
                if (m_zIndex != value)
                {
                    m_zIndex = value;
                    FirePropertyChanged("zIndex");
                }
            }
        }

        private MarkerInfoWindowStyleType m_MarkerInfoWindowStyleType = MarkerInfoWindowStyleType.InfoWindowWithAvatarAndText;
        [DataMember]
        public MarkerInfoWindowStyleType MarkerInfoWindowStyleType
        {
            get { return m_MarkerInfoWindowStyleType; }
            set
            {
                if (m_MarkerInfoWindowStyleType != value)
                {
                    m_MarkerInfoWindowStyleType = value;
                    FirePropertyChanged("MarkerInfoWindowStyleType");
                }
            }
        }

        private string m_LocalAvatarPath = "";
        [DataMember]
        public string LocalAvatarPath
        {
            get { return m_LocalAvatarPath; }
            set { m_LocalAvatarPath = value; }
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            MapRosterItem otherMapRosterItem = obj as MapRosterItem;
            if (otherMapRosterItem != null)
                return this.zIndex.CompareTo(otherMapRosterItem.zIndex);
            else
                throw new ArgumentException("Object is not a MapRosterItem");
        }
        #endregion
    }


}
