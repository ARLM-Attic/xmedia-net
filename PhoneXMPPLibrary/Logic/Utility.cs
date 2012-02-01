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

using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;

namespace PhoneXMPPLibrary
{
    public class Utility
    {
        /// <summary>
        /// ﻿<geoloc xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="http://jabber.org/protocol/geoloc"><lat>32.234</lat><lon>-97.3453</lon></geoloc>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetXMLStringFromObject(object obj)
        {
            //MemoryStream stream = new MemoryStream();
            StringWriter stream = new StringWriter();
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            //namespaces.
            /// Get the default namespace
            /// 
            Type type = obj.GetType();
            object[] attr = type.GetCustomAttributes(typeof(System.Xml.Serialization.XmlRootAttribute), true);
            if ((attr != null) && (attr.Length > 0))
            {
                System.Xml.Serialization.XmlRootAttribute xattr = attr[0] as System.Xml.Serialization.XmlRootAttribute;
                namespaces.Add("", xattr.Namespace);
            }

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create(stream,settings);
            

            XmlSerializer ser = new XmlSerializer(obj.GetType());
            ser.Serialize(writer, obj, namespaces);

            writer.Flush();
            writer.Close();

            string strRet = stream.ToString();

            //stream.Seek(0, SeekOrigin.Begin);
            //byte[] bData = new byte[stream.Length];
            //stream.Read(bData, 0, bData.Length);

            stream.Close();
            stream.Dispose();

           // string strRet = System.Text.UTF8Encoding.UTF8.GetString(bData, 0, bData.Length);

            //strRet = strRet.Replace(@"<?xml version=""1.0""?>", "");
            return strRet;
        }

        public static object ParseObjectFromXMLString(string strXML, Type objType)
        {
            object objRet = null;
            if ((strXML == null) || (strXML.Length <= 0))
                return null;
            MemoryStream stream = new MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(strXML));
            XmlSerializer ser = new XmlSerializer(objType);
            try
            {
                objRet = ser.Deserialize(stream);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
            return objRet;
        }
    }
}
