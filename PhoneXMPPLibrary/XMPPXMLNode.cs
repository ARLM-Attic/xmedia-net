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
using System.Text.RegularExpressions;

namespace PhoneXMPPLibrary
{
    /// Simple XML parser to pull data off the incoming stream
    /// XMLReader classes expect to have complete XML, or they block on the stream, which is unacceptable
    public class XMPPXMLNode
    {
        public XMPPXMLNode(string strXMLFragment)
        {
            ParseXMLNode(strXMLFragment);
        }

        string m_strOuterXML = "";
        public string OuterXML
        {
            get
            {
                return m_strOuterXML;
            }
        }

        string m_strName = "";
        public string Name
        {
            get
            {
                return m_strName;
            }
        }


        XmlNodeType m_XmlNodeType = XmlNodeType.None;
        public XmlNodeType NodeType
        {
            get
            {
                return m_XmlNodeType;
            }
        }

        public static Regex RegexDeclaration = new Regex(@"\<\?xml [^\<\>]* \?\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        public static Regex RegexComment = new Regex(@"\< \s* !-- [^\<\>]* \>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        public static Regex RegexEndElement = new Regex(@"\<\/ (?<name>[^\<\>]+) \>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        public static Regex RegexStartElement = new Regex(@"\< (?<name>\S+) [^\<\>]* \>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        public static Regex RegexCompleteElement = new Regex(@"\< (?<name>\S+) [^\<\>]* \/\>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        void ParseXMLNode(string strXML)
        {
            m_strOuterXML = strXML;

            Match matchman = RegexDeclaration.Match(strXML);
            if (matchman.Success == true)
            {
                m_XmlNodeType |= XmlNodeType.XmlDeclaration;
                return;
            }

            matchman = RegexComment.Match(strXML);
            if (matchman.Success == true)
            {
                return;
            }

            matchman = RegexEndElement.Match(strXML);
            if (matchman.Success == true)
            {
                m_strName = matchman.Groups["name"].Value;
                m_XmlNodeType |= XmlNodeType.EndElement;
                return;
            }

            matchman = RegexCompleteElement.Match(strXML);
            if (matchman.Success == true)
            {
                m_strName = matchman.Groups["name"].Value;
                m_XmlNodeType |= XmlNodeType.EndElement;
                return;
            }

            matchman = RegexStartElement.Match(strXML);
            if (matchman.Success == true)
            {
                m_strName = matchman.Groups["name"].Value;
                return;
            }



            // Don't care about anything else
            return;



        }

        public string GetAttribute(string strAttributeName)
        {
            string strExp = string.Format("{0}=\"(?<value>.*?)\"", strAttributeName);
            Regex RegexAttribute = new Regex(strExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            Match matchman = RegexAttribute.Match(m_strOuterXML);

            if (matchman.Success == true)
            {
                string strValue = matchman.Groups["value"].Value;
                //strValue = strValue.Trim(' ', '\"');
                strValue = strValue.Trim();
                return strValue;
            }

            return "";
        }



    }
}
