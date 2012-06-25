using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Net.XMPP;

namespace LocationClasses
{
    public class KMLBuilderForRosterItem
    {
        private RosterItem m_RosterItem = new RosterItem();

        public RosterItem RosterItem
        {
            get { return m_RosterItem; }
            set { m_RosterItem = value; }
        }

        private List<GeoCoordinate> m_CoordinateList = new List<GeoCoordinate>();

        public List<GeoCoordinate> CoordinateList
        {
            get { return m_CoordinateList; }
            set { m_CoordinateList = value; }
        }


        private bool m_IsNotRecordingKML = true;

        public bool IsNotRecordingKML
        {
            get { return m_IsNotRecordingKML; }
            set { m_IsNotRecordingKML = value; }
        }

        private bool m_IsRecordingKML = false;

        public bool IsRecordingKML
        {
            get { return m_IsRecordingKML; }
            set { m_IsRecordingKML = value; }
        }

      
    }

    public class KMLBuilder
    {
        private Dictionary<RosterItem, KMLBuilderForRosterItem> m_Dictionary = new Dictionary<RosterItem, KMLBuilderForRosterItem>();

        public Dictionary<RosterItem, KMLBuilderForRosterItem> Dictionary
        {
            get { return m_Dictionary; }
            set { m_Dictionary = value; }
        }

        private bool m_IsNotRecordingKML = true;

        public bool IsNotRecordingKML
        {
            get { return m_IsNotRecordingKML; }
            set { m_IsNotRecordingKML = value; }
        }

        private bool m_IsRecordingKML = false;

        public bool IsRecordingKML
        {
            get { return m_IsRecordingKML; }
            set { m_IsRecordingKML = value; }
        }


        public static string WriteSampleKML()
        {
            MyKML kml = new MyKML();
            kml.Document.Name = "brian";

            List<GeoCoordinate> Coords = new List<GeoCoordinate>();
            Coords.Add(new GeoCoordinate(32.816929, -96.757835, DateTime.Now - TimeSpan.FromMinutes(10)));
            Coords.Add(new GeoCoordinate(32.815437, -96.75758399999999, DateTime.Now - TimeSpan.FromMinutes(5)));
            Coords.Add(new GeoCoordinate(32.817078, -96.757721, DateTime.Now));

            int i = 1;
            foreach (GeoCoordinate coord in Coords)
            {
                string strNextName = i.ToString();
                kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
                i++;
            }
            kml.Document.Placemarks.Add(new Placemark("Total Path", Coords));

            //kml.Document.Placemark.LineString.Coordinates = "-96.757835,32.816929,0 -96.75775899999999,32.817066,0 -96.75785100000002,32.817116,0 -96.75856,32.817074,0 -96.75862100000001,32.817039,0 -96.758636,32.81638,0 -96.75861399999999,32.816273,0 -96.75855300000001,32.816238,0 -96.758095,32.81609,0 -96.757668,32.816059,0 -96.75759100000001,32.815968,0 -96.75758399999999,32.815437,0 -96.757645,32.815018,0 -96.75759100000001,32.814468,0 -96.75753,32.814243,0 -96.75756800000002,32.814007,0 -96.75752300000001,32.813923,0 -96.75755299999999,32.813671,0 -96.75752300000001,32.813156,0 -96.757538,32.812908,0 -96.75753,32.812859,0 -96.757431,32.812717,0 -96.757042,32.812649,0 -96.756218,32.812725,0 -96.75525700000002,32.81271,0 -96.754898,32.812431,0 -96.75482900000002,32.812344,0 -96.75436400000001,32.812031,0 -96.75415000000001,32.811966,0 -96.753975,32.811852,0 -96.75372299999998,32.811626,0 -96.75347099999999,32.811337,0 -96.753265,32.811169,0 -96.753113,32.8111,0 -96.752945,32.811203,0 -96.75266299999998,32.811287,0 -96.75241100000001,32.811539,0 -96.752312,32.811611,0 -96.75219,32.811653,0 -96.751953,32.811535,0 -96.75168600000001,32.811291,0 -96.75146499999998,32.811161,0 -96.75053399999999,32.810883,0 -96.750305,32.810844,0 -96.75000799999999,32.810886,0 -96.74977899999999,32.81097,0 -96.74961899999998,32.811192,0 -96.74949600000001,32.811234,0 -96.74943500000001,32.811359,0 -96.74928300000001,32.811394,0 -96.749146,32.811371,0 -96.74900100000001,32.811241,0 -96.748772,32.811096,0 -96.748711,32.810997,0 -96.748634,32.810989,0 -96.74844400000001,32.810745,0 -96.748322,32.81068,0 -96.74813100000002,32.810513,0 -96.747856,32.810192,0 -96.747124,32.809586,0 -96.746758,32.809338,0 -96.746239,32.808823,0 -96.74600199999999,32.808758,0 -96.74588,32.808681,0 -96.74496499999998,32.807957,0 -96.744781,32.807858,0 -96.744728,32.807743,0 -96.74465900000001,32.807732,0 -96.74453,32.807781,0 -96.74427799999999,32.807976,0 -96.743866,32.808182,0 -96.74283599999998,32.808537,0 -96.74215700000001,32.808681,0 -96.74197399999999,32.808762,0 -96.741699,32.808811,0 -96.74120300000001,32.80899,0 -96.74092899999999,32.809025,0 -96.740578,32.80917,0 -96.73992200000001,32.809277,0 -96.739555,32.809303,0 -96.739334,32.809284,0 -96.739212,32.809319,0 -96.73904400000001,32.809807,0 -96.738998,32.81015,0 -96.738884,32.810326,0 -96.738861,32.81049,0 -96.7388,32.810555,0 -96.73877,32.810741,0 -96.738708,32.81086,0 -96.73854799999999,32.811695,0 -96.738274,32.812561,0 -96.73819000000002,32.813019,0 -96.73814400000001,32.813068,0 -96.73803700000001,32.813412,0 -96.73795300000001,32.813572,0 -96.73782300000001,32.814171,0 -96.73773199999998,32.814392,0 -96.73773199999998,32.814617,0 -96.73769400000001,32.814663,0 -96.73762499999998,32.814957,0 -96.737633,32.815063,0 -96.73733500000002,32.816135,0 -96.73733500000002,32.817547,0 -96.73743399999999,32.81805,0 -96.737572,32.818424,0 -96.737724,32.819038,0 -96.737816,32.819466,0 -96.73786200000001,32.819923,0 -96.738007,32.820564,0 -96.738281,32.821457,0 -96.73838000000001,32.822159,0 -96.738472,32.822559,0 -96.73850299999999,32.822598,0 -96.73854799999999,32.823063,0 -96.73865499999999,32.823513,0 -96.738708,32.823597,0 -96.73878500000001,32.823593,0 -96.73932600000001,32.823532,0 -96.73949399999999,32.82349,0 -96.740532,32.823387,0 -96.74086800000001,32.823414,0 -96.741173,32.823326,0 -96.74131000000001,32.82333,0 -96.741287,32.823372,0 -96.74130200000001,32.823757,0 -96.741264,32.823795,0 -96.741249,32.823982,0 -96.741165,32.824337,0 -96.741158,32.824493,0 -96.74124099999999,32.824738,0 -96.74155399999999,32.825054,0 -96.74156199999999,32.82518,0 -96.74121899999999,32.825611,0 -96.74086800000001,32.825974,0 -96.74066899999998,32.826118,0 -96.74037199999999,32.826221,0 -96.74031100000001,32.826416,0 -96.74012000000001,32.827511,0 -96.74026499999999,32.827854,0 -96.740517,32.828201,0 -96.740852,32.828568,0 -96.740921,32.828671,0 -96.741074,32.828785,0 -96.74128,32.829052,0 -96.741821,32.829548,0 -96.74215700000001,32.829929,0 -96.74232499999999,32.830257,0 -96.742706,32.830872,0 -96.743011,32.831516,0 -96.74308000000002,32.831528,0 -96.743622,32.831371,0 -96.74408700000001,32.831276,0 -96.74440800000001,32.831158,0 -96.744942,32.831154,0 -96.74533099999999,32.831184,0 -96.745468,32.831226,0 -96.74567399999999,32.831211,0 -96.745689,32.831142,0 -96.74572000000001,32.830605,0 -96.745865,32.829601,0 -96.745811,32.829388,0 -96.745598,32.829266,0 -96.74546099999999,32.829247,0 -96.745346,32.829189,0 -96.74521599999999,32.829208,0 -96.74485,32.829063,0 -96.74444599999998,32.828785,0 -96.74435400000002,32.82869,0 -96.743622,32.828201,0 -96.74355300000001,32.82811,0 -96.74355300000001,32.82806,0 -96.743858,32.827766,0 -96.744064,32.827499,0 -96.744072,32.827358,0 -96.74397999999999,32.827133,0 -96.744438,32.826843,0 -96.744614,32.82679,0 -96.74530799999999,32.826759,0 -96.74543799999999,32.826778,0 -96.74597900000001,32.82674,0 -96.746246,32.826759,0 -96.74664300000002,32.826714,0 -96.747192,32.826736,0 -96.74733000000001,32.826721,0 -96.74750500000002,32.826637,0 -96.74758900000001,32.826653,0 -96.747765,32.826595,0 -96.74854300000001,32.826481,0 -96.748802,32.826477,0 -96.749374,32.826542,0 -96.749916,32.826534,0 -96.74999200000002,32.82629,0 -96.75,32.825863,0 -96.75003100000001,32.825699,0 -96.75007600000001,32.824848,0 -96.749878,32.824417,0 -96.74986300000001,32.824219,0 -96.749931,32.823101,0 -96.74988599999999,32.822636,0 -96.74987000000002,32.82169,0 -96.749931,32.821434,0 -96.749878,32.821346,0 -96.74987000000002,32.821236,0 -96.74990099999999,32.821007,0 -96.75004600000001,32.820808,0 -96.751091,32.819702,0 -96.75135799999998,32.819351,0 -96.75135,32.819294,0 -96.751289,32.819286,0 -96.750336,32.819458,0 -96.75013,32.819431,0 -96.749786,32.819542,0 -96.749847,32.819256,0 -96.749893,32.819153,0 -96.749893,32.819019,0 -96.75,32.818542,0 -96.75,32.818428,0 -96.750069,32.818329,0 -96.750328,32.81823,0 -96.75046500000001,32.818104,0 -96.751076,32.817898,0 -96.75205200000002,32.817497,0 -96.752296,32.817371,0 -96.752495,32.81723,0 -96.75278500000002,32.817131,0 -96.753693,32.817112,0 -96.753838,32.817139,0 -96.75398999999999,32.817116,0 -96.755409,32.817139,0 -96.755737,32.81712,0 -96.755852,32.817078,0 -96.75592000000002,32.817093,0 -96.755951,32.817039,0 -96.75611899999998,32.817005,0 -96.75694300000001,32.817165,0 -96.757469,32.817154,0 -96.757721,32.817078,0";

            string strXML = GetXMLStringFromObject(kml);
            System.IO.FileStream output = new FileStream("c:/temp/new.kml", FileMode.Create);
            byte[] bXML = System.Text.UTF8Encoding.UTF8.GetBytes(strXML);
            output.Write(bXML, 0, bXML.Length);
            output.Close();
            return strXML;
        }

        public static  string BuildKML(RosterItem rosterItem, List<GeoCoordinate> CoordinateList)
        {
            string strXML = "";
            MyKML kml = new MyKML();

            kml.Document.Name = string.Format("Buddy {0} coordinates for {1}", rosterItem.JID.BareJID, DateTime.Now);

            int i = 1;
            foreach (GeoCoordinate coord in CoordinateList)
            {
                string strNextName = i.ToString();
                kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
                i++;
            }
            kml.Document.Placemarks.Add(new Placemark("Total Path", CoordinateList));

            strXML = GetXMLStringFromObject(kml);
            return strXML;
        }

        public static string BuildKML(BuddyLocationPosition buddy)
        {
            string strXML = "";
            MyKML kml = new MyKML();

            kml.Document.Name = string.Format("Buddy {0} coordinates for {1}", buddy.RosterItem.JID.BareJID, DateTime.Now);

            int i = 1;
            foreach (GeoCoordinate coord in buddy.CoordinateList)
            {
                string strNextName = i.ToString();
                kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
                i++;
            }
            kml.Document.Placemarks.Add(new Placemark("Total Path", buddy.CoordinateList));

            strXML = GetXMLStringFromObject(kml);
            return strXML;
        }
  
        public static string BuildKML(BuddyPosition buddy)
        {
            string strXML = "";
            MyKML kml = new MyKML();

            kml.Document.Name = string.Format("Buddy {0} coordinates for {1}", buddy.RosterItem.JID.BareJID, DateTime.Now);

            int i = 1;
            foreach (GeoCoordinate coord in buddy.CoordinateList)
            {
                string strNextName = i.ToString();
                kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
                i++;
            }
            kml.Document.Placemarks.Add(new Placemark("Total Path", buddy.CoordinateList));

            strXML = GetXMLStringFromObject(kml);
            return strXML;
        }

        public static string WriteBuddyKML(string strFileName, BuddyLocationPosition buddy)
        {
            //MyKML kml = new MyKML();

            //kml.Document.Name = string.Format("Buddy {0} coordinates for {1}", buddy.RosterItem.JID.BareJID, DateTime.Now);

            //int i = 1;
            //foreach (GeoCoordinate coord in buddy.CoordinateList1)
            //{
            //    string strNextName = i.ToString();
            //    kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
            //    i++;
            //}
            //kml.Document.Placemarks.Add(new Placemark("Total Path", buddy.CoordinateList1));

            //string strXML = GetXMLStringFromObject(kml);

            string strXML = BuildKML(buddy);

            System.IO.FileStream output = new FileStream(strFileName, FileMode.Create);
            byte[] bXML = System.Text.UTF8Encoding.UTF8.GetBytes(strXML);
            output.Write(bXML, 0, bXML.Length);
            output.Close();
            return strXML;
        }
   
        public static string WriteBuddyKML(string strFileName, BuddyPosition buddy)
        {
            //MyKML kml = new MyKML();

            //kml.Document.Name = string.Format("Buddy {0} coordinates for {1}", buddy.RosterItem.JID.BareJID, DateTime.Now);

            //int i = 1;
            //foreach (GeoCoordinate coord in buddy.CoordinateList1)
            //{
            //    string strNextName = i.ToString();
            //    kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
            //    i++;
            //}
            //kml.Document.Placemarks.Add(new Placemark("Total Path", buddy.CoordinateList1));

            //string strXML = GetXMLStringFromObject(kml);

            string strXML = BuildKML(buddy);

            System.IO.FileStream output = new FileStream(strFileName, FileMode.Create);
            byte[] bXML = System.Text.UTF8Encoding.UTF8.GetBytes(strXML);
            output.Write(bXML, 0, bXML.Length);
            output.Close();
            return strXML;
        }

        ///{"Member DocumentType.Items of type AbstractFeatureType[] hides 
        ///base class member AbstractFeatureType.Items of type AbstractStyleSelectorType[]. 
        /// Use XmlElementAttribute or XmlAttributeAttribute to specify a new name."}
        public static string GetXMLStringFromObject(object obj)
        {
            StringWriter stream = new StringWriter();
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("gx", "http://www.google.com/kml/ext/2.2");
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(stream, settings);


            XmlSerializer ser = new XmlSerializer(obj.GetType());
            ser.Serialize(writer, obj, namespaces);

            writer.Flush();
            writer.Close();

            string strRet = stream.ToString();

            stream.Close();
            stream.Dispose();

            return strRet;
        }

    }
}
