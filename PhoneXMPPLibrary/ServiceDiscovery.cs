using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;
using System.Xml.Serialization;

namespace PhoneXMPPLibrary
{
    public class feature
    {
        public feature()
        {
        }

        public feature(string strVar)
        {
            Var = strVar;
        }
        private string m_strVar = "";

        [XmlAttribute(AttributeName = "var")]
        public string Var
        {
            get { return m_strVar; }
            set { m_strVar = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj is feature)
            {
                feature sobj = obj as feature;
                if (sobj.Var == this.Var)
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Var.GetHashCode();
        }

    }

    public class identity
    {
        public identity()
        {
        }
        public identity(string strCategory, string strType, string strName)
        {
            Category = strCategory;
            Type = strType;
            Name = strName;
        }

        private string m_strName;
        [XmlAttribute(AttributeName = "name")]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        private string m_strCategory;
        [XmlAttribute(AttributeName = "category")]
        public string Category
        {
            get { return m_strCategory; }
            set { m_strCategory = value; }
        }

        private string m_strType;
        [XmlAttribute(AttributeName = "type")]
        public string Type
        {
            get { return m_strType; }
            set { m_strType = value; }
        }
    }

    public class ServiceDiscoveryFeatureList
    {
        public ServiceDiscoveryFeatureList()
        {
        }

        public void AddFeature(feature feature)
        {
            /// Make sure this feature doesn't exists
            /// 
            lock (m_LockFeatures)
            {
                foreach (feature fea in Features)
                {
                    if (fea.Equals(feature) == true)
                        return;
                }

                Features.Add(feature);
            }
        }


        public void RemoveFeature(feature feature)
        {
            /// Make sure this feature doesn't exists
            /// 
            lock (m_LockFeatures)
            {
                feature foundfeature = null;
                foreach (feature fea in Features)
                {
                    if (fea.Equals(feature) == true)
                    {
                        foundfeature = fea;
                        break;
                    }
                }

                if (foundfeature != null)
                   Features.Remove(foundfeature);
            }
        }

        public feature[] ToArray()
        {
            return Features.ToArray();
        }

        object m_LockFeatures = new object();

        List<feature> m_listFeatures = new List<feature>();

        public List<feature> Features
        {
            get { return m_listFeatures; }
            set { m_listFeatures = value; }
        }
    }

    public enum ServiceDiscoveryType
    {
        info,
        items,
    }

    public class ServiceDiscoveryIQ : IQ
    {
        public ServiceDiscoveryIQ()
            : base()
        {
        }
        public ServiceDiscoveryIQ(ServiceDiscoveryType type)
            : base()
        {
            ServiceDiscoveryType = type;
        }

        public ServiceDiscoveryIQ(string strXML)
            : base(strXML)
        {
        }


        public ServiceDiscoveryIQ(string strXML, ServiceDiscoveryType type)
            : base(strXML)
        {
            ServiceDiscoveryType = type;
        }


        private feature[] m_afeatures = null;

        public feature[] Features
        {
            get { return m_afeatures; }
            set { m_afeatures = value; }
        }

        private identity[] m_aidentities = null;

        public identity[] Identities
        {
            get { return m_aidentities; }
            set { m_aidentities = value; }
        }

        private ServiceDiscoveryType m_eServiceDiscoveryType = ServiceDiscoveryType.info;

        public ServiceDiscoveryType ServiceDiscoveryType
        {
            get { return m_eServiceDiscoveryType; }
            set { m_eServiceDiscoveryType = value; }
        }

        private string m_strNode = null;

        public string Node
        {
            get { return m_strNode; }
            set { m_strNode = value; }
        }

        public const string ServiceDiscoverString = "<query xmlns='http://jabber.org/protocol/disco###TYPE##' />";
        public const string ServiceDiscoverStringWithNode = "<query xmlns='http://jabber.org/protocol/disco###TYPE##' node='##NODE'/>";

        public override void AddInnerXML(System.Xml.Linq.XElement elemMessage)
        {

            /// Again, can't build xml programmatically here because adding the namespace dynamically causes a crash
            XElement xelemquery = null;
            if (Node == null)
               xelemquery = XElement.Parse(ServiceDiscoverString.Replace("##TYPE##", ServiceDiscoveryType.ToString()));
            else
                xelemquery = XElement.Parse(ServiceDiscoverString.Replace("##TYPE##", ServiceDiscoveryType.ToString()).Replace("##NODE##", Node));

            elemMessage.Add(xelemquery);

            /// Add our features and identities to our list
            /// 
            if ((Features != null) && (Features.Length > 0))
            {
                foreach (feature fe in Features)
                {
                    string strXMLFeature = Utility.GetXMLStringFromObject(fe);
                    xelemquery.Add(XElement.Parse(strXMLFeature));
                }
            }
            if ((Identities != null) && (Identities.Length > 0))
            {
                foreach (identity id in Identities)
                {
                    string strXMLId = Utility.GetXMLStringFromObject(id);
                    xelemquery.Add(XElement.Parse(strXMLId));
                }
            }

            base.AddInnerXML(elemMessage);
        }

        public override void ParseInnerXML(System.Xml.Linq.XElement elem)
        {
            List<feature> FeatureList = new List<feature>();
            List<identity> IdentityList = new List<identity>();

            foreach (XElement node in elem.Nodes())
            {
                if (node.Name == "identity")
                {
                    identity id = Utility.ParseObjectFromXMLString(node.Value, typeof(identity)) as identity;
                    if (id != null)
                        IdentityList.Add(id);
                }
                if (node.Name == "feature")
                {
                    feature fea = Utility.ParseObjectFromXMLString(node.Value, typeof(feature)) as feature;
                    if (fea != null)
                        FeatureList.Add(fea);
                }
            }

            if (IdentityList.Count > 0)
               Identities = IdentityList.ToArray();
            if (FeatureList.Count > 0)
                Features = FeatureList.ToArray();

            base.ParseInnerXML(elem);
        }
    }

}
