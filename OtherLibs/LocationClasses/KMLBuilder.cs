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
    public static class KMLBuilder
    {
        public static string BuildKML(RosterItem rosterItem, List<GeoCoordinate> CoordinateList)
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
            //foreach (GeoCoordinate coord in buddy.CoordinateList)
            //{
            //    string strNextName = i.ToString();
            //    kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
            //    i++;
            //}
            //kml.Document.Placemarks.Add(new Placemark("Total Path", buddy.CoordinateList));

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
            //foreach (GeoCoordinate coord in buddy.CoordinateList)
            //{
            //    string strNextName = i.ToString();
            //    kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
            //    i++;
            //}
            //kml.Document.Placemarks.Add(new Placemark("Total Path", buddy.CoordinateList));

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
