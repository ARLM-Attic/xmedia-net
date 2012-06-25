using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocationClasses
{
    public class MapBuilder
    {
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

        public string BuildJavaScriptSourceCode(MapProperties MapProperties)
        {
            return BuildJavaScriptSourceCode(MapProperties, null);
        }

        public string BuildJavaScriptSourceCode(MapProperties MapProperties, System.Net.XMPP.RosterItem rosterItem)
        {
            //bool bSampleMap = false;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<script src=\"http://maps.google.com/maps/api/js?sensor={0}\" type=\"text/javascript\"></script>\r\n", MapProperties.Sensor.ToString().ToLower());
            //sb.AppendLine("<script src=\"http://maps.gstatic.com/intl/en_us/mapfiles/api-3/9/5/main.js\" type=\"text/javascript\"></script>");

            sb.AppendLine();

            sb.AppendLine("<script type=\"text/javascript\">");

            sb.AppendLine(BuildInitializeFunction(MapProperties));
            if (rosterItem != null)
                sb.AppendLine(BuildMarker(MapProperties, rosterItem));


            sb.AppendLine();
            sb.AppendLine("google.maps.event.addDomListener(window, 'load', initialize);");
            sb.AppendLine("</script>");

            sb.AppendFormat("<div id=\"map-canvas\" style=\"{0}\"></div>\r\n", MapProperties.MapParameters.Size.ToJavaScriptString());
            GoogleMapsSourceCode = sb.ToString();

            return sb.ToString();
        }

        public string BuildInitializeFunction(MapProperties MapProperties)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("function initialize() {");
            sb.AppendLine("     var mapDiv = document.getElementById('map-canvas');");
            sb.AppendLine("     var map = new google.maps.Map(mapDiv, {");

            //if (bSampleMap)
            //{
            //    sb.AppendFormat("                 center: new google.maps.LatLng(37.4419, -122.1419),");
            //    sb.AppendFormat("                 zoom: 13,");
            //}
            //else
            sb.AppendLine(MapProperties.LocationParameters.ToJavaScriptString());

            //if (bSampleMap)
            //    sb.AppendFormat("                 mapTypeId: google.maps.MapTypeId.ROADMAP");
            //else
            sb.AppendLine(MapProperties.MapParameters.ToJavaScriptString());

            sb.AppendLine("     });");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public string BuildMarker(MapProperties MapProperties, System.Net.XMPP.RosterItem rosterItem)
        {
            StringBuilder sb = new StringBuilder();
            if (rosterItem != null)
            {
                sb.AppendFormat("   var image = 'http://code.google.com/apis/maps/documentation/javascript/examples/images/beachflag.png';");
                sb.AppendLine();
                sb.AppendFormat("   var myLatLng = new google.maps.LatLng({0}, {1});", rosterItem.GeoLoc.lat, rosterItem.GeoLoc.lon);

                sb.AppendFormat("   var beachMarker = new google.maps.Marker({");
                sb.AppendFormat("     position: latLng,");
                sb.AppendFormat("     map: map,");
                sb.AppendFormat("     icon: image");
                sb.AppendFormat("   });");
            }
            return sb.ToString();
        }
    }
}
