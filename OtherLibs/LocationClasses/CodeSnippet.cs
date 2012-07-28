using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace LocationClasses
{

    [DataContract]
    [XmlRoot(ElementName = "codesnippet", Namespace = null)]
    public class CodeSnippet
    {
        public CodeSnippet()
        {
        }


        private string m_strHtmlBody = null;
        [XmlElement(ElementName = "htmlbody")]
        [DataMember]
        public string HtmlBody
        {
            get { return m_strHtmlBody; }
            set { m_strHtmlBody = value; }
        }

        private string m_strHtmlHead = null;
        [XmlElement(ElementName = "htmlhead")]
        [DataMember]
        public string HtmlHead
        {
            get { return m_strHtmlHead; }
            set { m_strHtmlHead = value; }
        }

        private string m_strName = null;
        [XmlElement(ElementName = "name")]
        [DataMember]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        private string m_strPerson = "";
        /// <summary>
        /// The person who has last modified this item
        /// </summary>
        [XmlElement(ElementName = "person")]
        [DataMember]
        public string Person
        {
            get { return m_strPerson; }
            set { m_strPerson = value; }
        }

        private string m_strBody = "";
        /// <summary>
        /// The price that was paid or will be paid
        /// </summary>
        [XmlElement(ElementName = "body")]
        [DataMember]
        public string Body
        {
            get { return m_strBody; }
            set { m_strBody = value; }
        }

        private string m_strHashedBody = "";
        /// <summary>
        /// The price that was paid or will be paid
        /// </summary>
        [XmlElement(ElementName = "hashedbody")]
        [DataMember]
        public string HashedBody
        {
            get { return m_strHashedBody; }
            set { m_strHashedBody = value; }
        }

        private string m_strLocalFileName = "";
        /// <summary>
        /// The price that was paid or will be paid
        /// </summary>
        [XmlElement(ElementName = "localfilename")]
        [DataMember]
        public string LocalFileName
        {
            get { return m_strLocalFileName; }
            set { m_strLocalFileName = value; }
        }

        private string m_strRelativePath = "";
        /// <summary>
        /// The price that was paid or will be paid
        /// </summary>
        [XmlElement(ElementName = "relativepath")]
        [DataMember]
        public string RelativePath
        {
            get { return m_strRelativePath; }
            set { m_strRelativePath = value; }
        }

        private string m_strSourceURL = "";
        /// <summary>
        /// The price that was paid or will be paid
        /// </summary>
        [XmlElement(ElementName = "sourceURL")]
        [DataMember]
        public string SourceURL
        {
            get { return m_strSourceURL; }
            set { m_strSourceURL = value; }
        }


        private string m_strItemId = Guid.NewGuid().ToString();
        [XmlElement(ElementName = "itemid")]
        [DataMember]
        public string ItemId
        {
            get { return m_strItemId; }
            set { m_strItemId = value; }
        }


        private string m_strApiName = "";
        [XmlElement(ElementName = "apiname")]
        [DataMember]
        public string ApiName
        {
            get { return m_strApiName; }
            set { m_strApiName = value; }
        }

        private string m_strHtml = "";
        [XmlElement(ElementName = "html")]
        [DataMember]
        public string Html
        {
            get { return m_strHtml; }
            set { m_strHtml = value; }
        }

        public string BuildHtml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"
            <!--
You are free to copy and use this sample in accordance with the terms of the
Apache license (http://www.apache.org/licenses/LICENSE-2.0.html)
-->
");

            sb.AppendLine(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">");
            sb.AppendLine(@"<html xmlns=""http://www.w3.org/1999/xhtml"">");

            sb.AppendLine(this.HtmlHead);
            sb.AppendLine(this.HtmlBody);

            sb.AppendLine("</html>");
            Html = sb.ToString();
            return sb.ToString();
        }
    }   
}
