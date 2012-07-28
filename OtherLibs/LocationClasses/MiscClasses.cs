using System.Runtime.Serialization;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LocationClasses
{
    [DataContract]
    public enum Language
    {
        [Description("JavaScript")]
        [EnumMember]
        JavaScript
    }

    [DataContract]
    public class Signature : INotifyPropertyChanged
    {
        private string m_Name = "";
        [DataMember]
        public string Name
        {
            get { return m_Name; }
            set
            {
                if (m_Name != value)
                {
                    m_Name = value;
                    FirePropertyChanged("Name");
                }
            }
        }

        private string m_serializedParameterList = "";
        [DataMember]
        public string SerializedParameterList
        {
            get { return m_serializedParameterList; }
            set
            {
                if (m_serializedParameterList != value)
                {
                    m_serializedParameterList = value;
                    FirePropertyChanged("SerializedParameterList");
                }
            }
        }

        private object[] m_Parameters = null;

        public object[] Parameters
        {
            get { return m_Parameters; }
            set
            {
                if (m_Parameters != value)
                {
                    m_Parameters = value;
                    FirePropertyChanged("Parameters");
                }
            }
        }
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }

        #endregion
    }

    [DataContract]
    public class Function : INotifyPropertyChanged
    {
        public Function()
        {

        }

        private Language m_Language = Language.JavaScript;
        [DataMember]
        public Language ScriptType
        {
            get { return m_Language; }
            set { m_Language = value; }
        }

        private Signature m_Signature = new Signature();
        [DataMember]
        public Signature Signature
        {
            get { return m_Signature; }
            set { m_Signature = value; }
        }

        private string m_RawContents = "";
        [DataMember]
        public string RawContents
        {
            get { return m_RawContents; }
            set { m_RawContents = value; }
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }

        #endregion
    }

    [DataContract]
    public class JSONSerializedObject
    {
        private string m_strJSONSerializedString = "";

        public string strJSONSerializedString
        {
            get { return m_strJSONSerializedString; }
            set { m_strJSONSerializedString = value; }
        }

        private string m_strName = "";

        public string strName
        {
            get { return m_strName; }
            set { m_strName = value; }
        }
    }

    public enum MarkerInfoWindowStyleType
    {
        MarkerOnly,
        InfoWindowWithAvatarOnly,
        InfoWindowWithAvatarAndText,
        InfoWindowWithTextOnly
    }

    //public enum GoogleEarthFeatureType
    //{
    //    [EnumMember]
    //    PlayingTours
    //}

    //[DataContract]
    //public class GoogleEarthMapBuilder
    //{
    //    private GoogleEarthFeatureType m_GoogleEarthFeatureType = GoogleEarthFeatureType.PlayingTours;
    //    [DataMember]
    //    public GoogleEarthFeatureType GoogleEarthFeatureType
    //    {
    //        get { return m_GoogleEarthFeatureType; }
    //        set { m_GoogleEarthFeatureType = value; }
    //    }

    //    public string GetHtml(GoogleEarthFeatureType featureType)
    //    {
    //        string retStr = "";
    //        switch (featureType)
    //        {
    //            case LocationClasses.GoogleEarthFeatureType.PlayingTours:
    //                retStr = System.IO.File.ReadAllText(@"SampleHTML/GoogleEarth/PlayingTours.html");
    //                break;
    //            default:
    //                break;
    //        }
    //        return retStr;
    //    }
    //}

    //[DataContract]
    //public enum MapProviderType
    //{
    //    [EnumMember]
    //    [Description("Google Maps")]
    //    GoogleMaps,
    //    [EnumMember]
    //    [Description("Google Earth")]
    //    GoogleEarth
    //}

    public class MapProvider
    {
        private MapProviderType m_MapProviderType = MapProviderType.GoogleMaps;

        public MapProviderType MapProviderType
        {
            get { return m_MapProviderType; }
            set { m_MapProviderType = value; }
        }

        private string m_Name = "";

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }
    }

    public class API
    {
        private string m_Name = "";

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }
    }

    public static class DataManager
    {
        public static ObservableCollection<MapProvider> GetAllMapProviders()
        {
            ObservableCollection<MapProvider> retList = new ObservableCollection<MapProvider>();
            MapProvider googleEarth = new MapProvider() { MapProviderType = MapProviderType.GoogleEarth, Name = "Google Earth" };
            MapProvider googleMaps = new MapProvider() { MapProviderType = MapProviderType.GoogleMaps, Name = "Google Maps" };
            retList.Add(googleEarth);
            retList.Add(googleMaps);
            return retList;
        }

        public static ObservableCollection<API> GetAllAPIs()
        {
            ObservableCollection<API> retList = new ObservableCollection<API>();
            API googleEarth = new API() { Name = "Google Earth" };
            API googleMaps = new API() { Name = "Google Maps" };
            retList.Add(googleEarth);

            return retList;
        }
    }
}
