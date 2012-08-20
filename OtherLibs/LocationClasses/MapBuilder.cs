using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.XMPP;
using System.Runtime.Serialization;


namespace LocationClasses
{
    public enum GoogleEarthFeatureType 
    {
        [EnumMember]
        PlayingTours
    }

    [DataContract]
    public class GoogleEarthMapBuilder
    {
        

        private GoogleEarthFeatureType m_GoogleEarthFeatureType = GoogleEarthFeatureType.PlayingTours;
        [DataMember]
        public GoogleEarthFeatureType GoogleEarthFeatureType
        {
            get { return m_GoogleEarthFeatureType; }
            set { m_GoogleEarthFeatureType = value; }
        }

        public string GetHtml(GoogleEarthFeatureType featureType)
        {
            string retStr = "";
            switch (featureType)
            {
                case LocationClasses.GoogleEarthFeatureType.PlayingTours:
                    retStr = System.IO.File.ReadAllText(@"SampleHTML/GoogleEarth/PlayingTours.html");
                    break;
                default:
                    break;
            }
            return retStr;
        }
    }

    [DataContract]
    public enum MapProviderType
    {
        [EnumMember]
        GoogleMaps,
        [EnumMember]
        GoogleEarth
    }

    [DataContract]
    public class MapBuilder
    {
        private Dictionary<string, List<string>> m_MapExamples = new Dictionary<string, List<string>>();

        public Dictionary<string, List<string>> MapExamples
        {
            get { InitializeMapExamples(); return m_MapExamples; }
            set { m_MapExamples = value; }
        }



        public void InitializeMapExamples()
        {
            MapExamples.Add("GoogleEarth", new List<string>());
            MapExamples.Add("GoogleMaps", new List<string>());
            MapExamples["GoogleEarth"].Add("PlayingTours");
            MapExamples["GoogleMap"].Add("Default");
        }

        public MapBuilder()
        {

        }

        private string m_GoogleMapsSourceCode = "";

        public string GoogleMapsSourceCode
        {
            get
            {
                return m_GoogleMapsSourceCode;
            }
            set { m_GoogleMapsSourceCode = value; }
        }

        private string m_GoogleEarthSourceCode = "";

        public string GoogleEarthSourceCode
        {
            get
            {
                return m_GoogleEarthSourceCode;
            }
            set { m_GoogleEarthSourceCode = value; }
        }

        private MapProvider m_MapProvider = new MapProvider();

        public MapProvider MapProvider
        {
            get { return m_MapProvider; }
            set { m_MapProvider = value; }
        }

        private string m_MapFeature = "Default";

        public string MapFeature
        {
            get { return m_MapFeature; }
            set { m_MapFeature = value; }
        }

        public string BuildJavaScriptSourceCode(MapProperties MapProperties)
        {
            return BuildJavaScriptSourceCode(MapProperties, null);
        }

        public string BuildJavaScriptSourceCode(MapProperties MapProperties, System.Net.XMPP.RosterItem rosterItem)
        {
            string retStr = "";
            GoogleEarthMapBuilder GoogleEarthMapBuilder = new GoogleEarthMapBuilder();
           

            switch (MapProvider.MapProviderType)
            {
                case MapProviderType.GoogleMaps:
                    retStr = BuildJavaScriptSourceCodeForGoogleMaps(MapProperties, rosterItem);
                    break;
                case MapProviderType.GoogleEarth:
                    retStr = BuildJavaScriptSourceCodeForGoogleEarth(MapProperties, rosterItem);
                    break;
                default:
                    break;
            }
            return retStr;
        }

        #region Google Earth
        public string BuildJavaScriptSourceCodeForGoogleEarth(MapProperties MapProperties, System.Net.XMPP.RosterItem rosterItem)
        {
            //bool bSampleMap = false;

            StringBuilder sb = new StringBuilder();
           
            string strFileContents = System.IO.File.ReadAllText("SampleHTML/GoogleEarth/showing_time_ui_by_parsing_time-aware_kml.html");

            sb.AppendLine(strFileContents);

            return sb.ToString();
        }

        #endregion

        public string BuildJavaScriptSourceCodeForGoogleMaps(MapProperties MapProperties, System.Net.XMPP.RosterItem rosterItem)
        {
            //bool bSampleMap = false;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<script src=\"http://maps.google.com/maps/api/js?sensor={0}\" type=\"text/javascript\"></script>\r\n", MapProperties.Sensor.ToString().ToLower());
            //sb.AppendLine("<script src=\"http://maps.gstatic.com/intl/en_us/mapfiles/api-3/9/5/main.js\" type=\"text/javascript\"></script>");

            sb.AppendLine();

            sb.AppendLine("<script type=\"text/javascript\">");

            sb.AppendLine(BuildVariableList());
            sb.AppendLine(BuildInitializeFunction(MapProperties, rosterItem));
           
            sb.AppendLine();
            sb.AppendLine("google.maps.event.addDomListener(window, 'load', initialize);");
            sb.AppendLine("</script>");

            sb.AppendFormat("<div id=\"map-canvas\" style=\"{0}\"></div>\r\n", MapProperties.MapParameters.Size.ToJavaScriptString());
            GoogleMapsSourceCode = sb.ToString();

            return sb.ToString();
        }

        public string BuildAddMarkerFunction(MapProperties MapProperties, RosterItem rosterItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("function addMarker() {");
            sb.AppendLine("   var bounds = map.getBounds();");
            sb.AppendLine("   var southWest = bounds.getSouthWest();");
            sb.AppendLine("   var northEast = bounds.getNorthEast();");
            sb.AppendLine("   var lngSpan = northEast.lng() - southWest.lng();");
            sb.AppendLine("   var latSpan = northEast.lat() - southWest.lat();");
            
            //sb.AppendLine("   for (var i = 0; i < 10; i++) {");
            //sb.AppendLine(String.Format("      var latLng = new google.maps.LatLng(southWest.lat() + latSpan * Math.random(),"));
            //sb.AppendLine(String.Format("                    southWest.lng() + lngSpan * Math.random());"));

            sb.AppendLine(String.Format("   var latLng = new google.maps.LatLng({0}, {1});", rosterItem.GeoLoc.lat, rosterItem.GeoLoc.lon));
      
            
            sb.AppendLine("   var marker = new google.maps.Marker({");
            sb.AppendLine("            position: latLng,");
            sb.AppendLine("            map: map");
            sb.AppendLine("   });");
            //sb.AppendLine("   }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public string BuildAddMarkersFunction(MapProperties MapProperties, System.Net.XMPP.RosterItem rosterItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("function addMarkers() {");
            sb.AppendLine("   var bounds = map.getBounds();");
            sb.AppendLine("   var southWest = bounds.getSouthWest();");
            sb.AppendLine("   var northEast = bounds.getNorthEast();");
            sb.AppendLine("   var lngSpan = northEast.lng() - southWest.lng();");
            sb.AppendLine("   var latSpan = northEast.lat() - southWest.lat();");
            sb.AppendLine("   for (var i = 0; i < 10; i++) {");
            sb.AppendLine("      var latLng = new google.maps.LatLng(southWest.lat() + latSpan * Math.random(),");
            sb.AppendLine("                    southWest.lng() + lngSpan * Math.random());");
            sb.AppendLine("      var marker = new google.maps.Marker({");
            sb.AppendLine("            position: latLng,");
            sb.AppendLine("            map: map");
            sb.AppendLine("         });");
            sb.AppendLine("   }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public string BuildAddInfoWindowFunction(MapProperties MapProperties, RosterItem rosterItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("function addInfoWindow() {");
            sb.AppendLine("   var infoWindow = new google.maps.InfoWindow({");
            //sb.AppendLine(String.Format("      position: {0},{1},", rosterItem.GeoLoc.lat, rosterItem.GeoLoc.lat));
            // map.getCenter(),");
            //sb.AppendLine(String.Format("      content: '<table><tr><td><img height=\"40\" src=\"http://blog.greenearthbamboo.com/wp-content/uploads/2012/03/Miranda-Kerr.jpg\" /></td><td><h3>{0}</h3></td></tr><tr><td colspan=\"2\">{1}<br>{2}</td></tr></table>'", 
            //    rosterItem.Name, rosterItem.JID.ToString(), rosterItem.GeoLoc.TimeStamp));
            sb.AppendLine(String.Format("      content: '<table><tr><td><h3>{0}</h3></td></tr><tr><td colspan=\"2\">{1}<br>{2}</td></tr></table>'",
               rosterItem.Name, rosterItem.JID.ToString(), rosterItem.GeoLoc.TimeStamp)); sb.AppendLine("   });");

            sb.AppendLine(String.Format("   var latLng = new google.maps.LatLng({0}, {1});", rosterItem.GeoLoc.lat, rosterItem.GeoLoc.lon));
      
            sb.AppendLine("   var marker = new google.maps.Marker({");
            sb.AppendLine("     position: latLng,");
            sb.AppendLine("     map: map");
            
            sb.AppendLine("   });");

            sb.AppendLine("infoWindow.open(map, marker);");
            sb.AppendLine("}");

  //           var content = '<strong>A info window!</strong><br/>That is bound to a marker';

  //var infowindow = new google.maps.InfoWindow({
  //  content: content
  //});

  //var marker = new google.maps.Marker({
  //  map: map,
  //  position: map.getCenter(),
  //  draggable: true
  //});

  //infowindow.open(map, marker);


            return sb.ToString();
        }

        public string BuildVariableList()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("var map;");
            return sb.ToString();
        }

        public string BuildInitializeFunction(MapProperties MapProperties)
        {
            return BuildInitializeFunction(MapProperties, null);
        }

        public string BuildInitializeFunction(MapProperties MapProperties, System.Net.XMPP.RosterItem rosterItem)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("function initialize() {");
            sb.AppendLine("     var mapDiv = document.getElementById('map-canvas');");
            sb.AppendLine("     map = new google.maps.Map(mapDiv, {");

            //if (bSampleMap)
            //{
            //    sb.AppendFormat("                 center: new google.maps.LatLng(37.4419, -122.1419),");
            //    sb.AppendFormat("                 zoom: 13,");
            //}
            //else
            if (MapProperties.LocationParameters.CenterGeoCoordinate == null)
                MapProperties.LocationParameters.CenterGeoCoordinate = new GeoCoordinate(rosterItem.GeoLoc.lat,
                    rosterItem.GeoLoc.lon, rosterItem.GeoLoc.TimeStamp);
            sb.AppendLine(MapProperties.LocationParameters.ToJavaScriptString());

            //if (bSampleMap)
            //    sb.AppendFormat("                 mapTypeId: google.maps.MapTypeId.ROADMAP");
            //else
            sb.AppendLine(MapProperties.MapParameters.ToJavaScriptString());

            sb.AppendLine("     });");

            //f (rosterItem != null)
            //    sb.AppendLine(BuildMarker(MapProperties, rosterItem));
            //sb.AppendLine(BuildVariableList());
            //sb.AppendLine(BuildAddMarkerFunction(MapProperties, rosterItem));
            sb.AppendLine(BuildAddInfoWindowFunction(MapProperties, rosterItem));

            sb.AppendLine();
            // sb.AppendLine("google.maps.event.addListenerOnce(map, 'tilesloaded', addMarker);");
            sb.AppendLine("google.maps.event.addListenerOnce(map, 'tilesloaded', addInfoWindow);");

            sb.AppendLine("}");

            return sb.ToString();
        }

        public string BuildMarker(MapProperties MapProperties, System.Net.XMPP.RosterItem rosterItem)
        {
            StringBuilder sb = new StringBuilder();
            if (rosterItem != null)
            {
                sb.AppendLine("   var image = 'http://code.google.com/apis/maps/documentation/javascript/examples/images/beachflag.png';");
                sb.AppendLine();
                sb.AppendLine(String.Format("   var myLatLng = new google.maps.LatLng({0}, {1});", rosterItem.GeoLoc.lat, rosterItem.GeoLoc.lon));

                sb.AppendLine("   var beachMarker = new google.maps.Marker({");
                sb.AppendLine("     position: myLatLng,");
                sb.AppendLine("     map: map,");
                sb.AppendLine("     icon: image");
                sb.AppendLine("   });");
            }
            return sb.ToString();
        }
    }
}
