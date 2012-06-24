using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LocationClasses
{
    // https://developers.google.com/maps/documentation/staticmaps/
    public class MapProperties
    {
        private static string m_URLBase = "http://maps.googleapis.com/maps/api/staticmap?";

        private string m_URL = "http://maps.googleapis.com/maps/api/staticmap?";

        public string URL
        {
            get
            {
                m_URL = m_URLBase;
                m_URL += BuildParameters();
                return m_URL;
            }
            set { m_URL = value; }
        }

        private LocationParameters m_LocationParameters = new LocationParameters();

        public LocationParameters LocationParameters
        {
            get { return m_LocationParameters; }
            set { m_LocationParameters = value; }
        }

        private MapParameters m_MapParameters = new MapParameters();

        public MapParameters MapParameters
        {
            get { return m_MapParameters; }
            set { m_MapParameters = value; }
        }

        // The markers parameter takes set of value assignments (marker descriptors) of the following format:
        // markers=markerStyles|markerLocation1| markerLocation2|... etc.

        private MarkerCollection m_MarkerCollection = new MarkerCollection();

        public MarkerCollection MarkerCollection
        {
            get { return m_MarkerCollection; }
            set { m_MarkerCollection = value; }
        }


        private bool m_Sensor = false;

        public bool Sensor
        {
            get { return m_Sensor; }
            set { m_Sensor = value; }
        }

        public string BuildParameters()
        {
            string retStr = "";
            retStr += LocationParameters.ToString();
            retStr += MapParameters.ToString();
            retStr += String.Format("&sensor={0}", Sensor.ToString().ToLower());
            return retStr;
        }

        public override string ToString()
        {
            return URL;
            //string retStr = "";
            //retStr += LocationParameters.ToString();
            //retStr += MapParameters.ToString();
            //retStr += String.Format("sensor={0}", Sensor.ToString());
            //return retStr;
            // return base.ToString();
        }

    }

    public class MarkerCollection
    {
        private MarkerStyle m_MarkerStyle = null;

        public MarkerStyle MarkerStyle
        {
            get { return m_MarkerStyle; }
            set { m_MarkerStyle = value; }
        }

        private List<MarkerLocation> m_MarkerLocations = new List<MarkerLocation>();

        public List<MarkerLocation> MarkerLocations
        {
            get { return m_MarkerLocations; }
            set { m_MarkerLocations = value; }
        }

        public override string ToString()
        {
            string retStr = "markers=";
            if (MarkerStyle != null)
            {
                retStr += MarkerStyle.ToString();
            }
            foreach (MarkerLocation location in MarkerLocations)
            {
                retStr += location.ToString();
            }
            return retStr;
        }
    }

    //size: (optional) specifies the size of marker from the set {tiny, mid, small}. If no size parameter is set, the marker will appear in its default (normal) size.
    //color: (optional) specifies a 24-bit color (example: color=0xFFFFCC) or a predefined color from the set {black, brown, green, purple, yellow, blue, gray, orange, red, white}.

    //Note that transparencies (specified using 32-bit hex color values) are not supported in markers, though they are supported for paths.

    //label: (optional) specifies a single uppercase alphanumeric character from the set {A-Z, 0-9}. (The requirement for uppercase characters is new to this version of the API.) Note that default and mid sized markers are the only markers capable of displaying an alphanumeric-character parameter. tiny and small markers are not capable of displaying an alphanumeric-character.
    public enum MarkerSize
    {
        tiny, mid, small
    }

    public class MarkerStyle
    {
        private MarkerSize m_MarkerSize = MarkerSize.mid;

        public MarkerSize MarkerSize
        {
            get { return m_MarkerSize; }
            set { m_MarkerSize = value; }
        }

        private System.Drawing.Color m_Color = System.Drawing.Color.Red;

        public System.Drawing.Color Color
        {
            get { return m_Color; }
            set
            { m_Color = value; }
        }

        private string m_Label = "";

        public string Label
        {
            get { return m_Label; }
            set { m_Label = value; }
        }
    }

    public class MarkerLocation
    {
        private MarkerStyle m_MarkerStyle = null;
        public MarkerStyle MarkerStyle
        {
            get { return m_MarkerStyle; }
            set { m_MarkerStyle = value; }
        }

        private string m_Location = "";

        public string Location
        {
            get { return m_Location; }
            set { m_Location = value; }
        }

        public static string GetLocationFromLatLon(double lat, double lon)
        {
            string retStr = "";
            retStr = String.Format("{0},{1}", lat.ToString(), lon.ToString());
            return retStr;
        }

        public override string ToString()
        {
            string retStr = "markers=";
            if (MarkerStyle != null)
            {
                retStr += MarkerStyle.ToString();
                retStr += String.Format("%7C{0}", Location);
            }
            else
                retStr += String.Format("{0}", Location);

            return retStr;
        }
    }

    // Location Parameters:

    // center (required if markers not present) defines the center of the map, equidistant from all edges of the map. This parameter takes a location as either a comma-separated {latitude,longitude} pair (e.g. "40.714728,-73.998672") or a string address (e.g. "city hall, new york, ny") identifying a unique location on the face of the earth. For more information, see Locations below.
    // zoom (required if markers not present) defines the zoom level of the map, which determines the magnification level of the map. This parameter takes a numerical value corresponding to the zoom level of the region desired. For more information, see zoom levels below.
    //      Maps on Google Maps have an integer "zoom level" which defines the resolution of the current view. Zoom levels between 0 (the lowest zoom level, 
    //      in which the entire world can be seen on one map) to 21+ (down to individual buildings) are possible within the default roadmap maps view.
    // center=texas&size=500x300&zoom=12&sensor=false

    public class LocationParameters
    {
        public static string SetCenterFromLatLon(double lat, double lon)
        {
            return String.Format("{0},{1}", lat.ToString(), lon.ToString());
        }

        private string m_Center = "";

        public string Center
        {
            get { return m_Center; }
            set { m_Center = value; }
        }

        // possible values are 0 to 21
        private int m_Zoom = 15;

        public int Zoom
        {
            get { return m_Zoom; }
            set { m_Zoom = value; }
        }

        public const int ZoomMin = 0;
        public const int ZoomMax = 21;

        public override string ToString()
        {
            // only print if value is not blank
            string retStr = "";
            if (Center != null && Center.Length > 0)
                retStr += String.Format("&center={0}", Center);
            retStr += String.Format("&zoom={0}", Zoom);

            return retStr;
        }
    }

    //Map Parameters:

    //size (required) defines the rectangular dimensions of the map image. This parameter takes a string of the form {horizontal_value}x{vertical_value}. For example, 500x400 defines a map 500 pixels wide by 400 pixels high. Maps smaller than 180 pixels in width will display a reduced-size Google logo. This parameter is affected by the scale parameter, described below; the final output size is the product of the size and scale values.
    //scale (optional) affects the number of pixels that are returned. scale=2 returns twice as many pixels as scale=1 while retaining the same coverage area and level of detail (i.e. the contents of the map don't change). This is useful when developing for high-resolution displays, or when generating a map for printing. The default value is 1. Accepted values are 2 and 4 (4 is only available to Maps API for Business customers.) See Scale Values for more information.
    // (Default scale value is 1; accepted values are 1, 2, and (for Maps API for Business customers only) 4).

    //format (optional) defines the format of the resulting image. By default, the Static Maps API creates PNG images. There are several possible formats including GIF, JPEG and PNG types. Which format you use depends on how you intend to present the image. JPEG typically provides greater compression, while GIF and PNG provide greater detail. For more information, see Image Formats.
    //maptype (optional) defines the type of map to construct. There are several possible maptype values, including roadmap, satellite, hybrid, and terrain. For more information, see Static Maps API Maptypes below.
    //language (optional) defines the language to use for display of labels on map tiles. Note that this parameter is only supported for some country tiles; if the specific language requested is not supported for the tile set, then the default language for that tileset will be used.
    //region (optional) defines the appropriate borders to display, based on geo-political sensitivities. Accepts a region code specified as a two-character ccTLD ('top-level domain') value.

    //    The table below shows the maximum allowable values for the size parameter at each scale value.

    //API	                        scale=1	    scale=2	                                scale=4
    //Free	                        640x640	    640x640 (returns 1280x1280 pixels)	    Not available.
    //Google Maps API for Business	2048x2048	1024x1024 (returns 2048x2048 pixels)	512x512 (returns 2048x2048 pixels)

    public class MapParameters
    {
        private SizeParameters m_Size = new SizeParameters();

        public SizeParameters Size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        private int m_Scale = 1;

        public int Scale
        {
            get { return m_Scale; }
            set { m_Scale = value; }
        }

        private MapFormat m_MapFormat = MapFormat.png;

        public MapFormat MapFormat
        {
            get { return m_MapFormat; }
            set { m_MapFormat = value; }
        }

        private MapType m_MapType = MapType.roadmap;

        public MapType MapType
        {
            get { return m_MapType; }
            set { m_MapType = value; }
        }

        public override string ToString()
        {
            string retStr = "";

            retStr += String.Format("&size={0}", Size.ToString());
            retStr += String.Format("&scale={0}", Scale);
            if (MapFormat == MapFormat.jpg_baseline)
                retStr += String.Format("&format={0}", "jpg-baseline");
            else
                retStr += String.Format("&format={0}", MapFormat.ToString());
            retStr += String.Format("&maptype={0}", MapType.ToString());

            return retStr;
        }

        public const int _ScaleMin = 1;
        public const int _ScaleMax = 2;
    }

    public enum MapType
    {
        roadmap, // default
        satellite,
        hybrid,
        terrain
    }


    // png8 or png (default) specifies the 8-bit PNG format.
    // png32 specifies the 32-bit PNG format.
    // gif specifies the GIF format.
    // jpg specifies the JPEG compression format.
    // jpg-baseline specifies a non-progressive JPEG compression format.

    public enum MapFormat
    {
        png,
        png32,
        gif,
        jpg,
        // jpg-baseline
        jpg_baseline
    }

    public class SizeParameters
    {
        private int m_Horizontal = 800;

        public int Horizontal
        {
            get { return m_Horizontal; }
            set { m_Horizontal = value; }
        }

        private int m_Vertical = 800;

        public int Vertical
        {
            get { return m_Vertical; }
            set { m_Vertical = value; }
        }

        //private string m_strHorizontal = "";

        public string strHorizontal
        {
            get { return Horizontal.ToString(); }
            // set { m_strHorizontal = value; }
        }

        //private string m_strVertical = "";

        public string strVertical
        {
            get { return Vertical.ToString(); }
            //set { m_strVertical = value; }
        }
        
        public override string ToString()
        {
            return String.Format("{0}x{1}", Horizontal, Vertical);
        }

        public OperationResult ValidateAndSaveSize(string strSizeHoriz, string strSizeVert)
        {
            // validate values.
            int nSizeHoriz = -1;
            int nSizeVert = -1;
            string strMessage = "";

            OperationResult result = new OperationResult();

            if (Int32.TryParse(strSizeHoriz, out nSizeHoriz))
            {
                this.Horizontal = nSizeHoriz;
            }
            else
            {
                result.strMessage = "Please enter a numerical value for the horizontal component of the size.";
                result.bSuccess = false;
                return result;
            }

            if (Int32.TryParse(strSizeVert, out nSizeVert))
            {
                this.Vertical = nSizeVert;
            }
            else
            {
                result.strMessage = "Please enter a numerical value for the vertical component of the size.";
                result.bSuccess = false;
                return result;
            }
            return result;
        }


    }

    public class OperationResult
    {
        private bool m_bSuccess = true;

        public bool bSuccess
        {
            get { return m_bSuccess; }
            set { m_bSuccess = value; }
        }

        private string m_strMessage = "";

        public string strMessage
        {
            get { return m_strMessage; }
            set { m_strMessage = value; }
        }
    }
}