using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.XMPP;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestIQErrorParsing();
            TestPubSubParsing();
        }

        static void TestIQErrorParsing()
        {
            IQ iqerror = new IQ();
            iqerror.From = "me@no.where";
            iqerror.To = "you@over.there";
            iqerror.Type = IQType.result.ToString();
            iqerror.Error = new Error(ErrorType.badformat);
            iqerror.Error.Code = "405";
            iqerror.Error.Type = "typeeror";

            string strXML = Utility.GetXMLStringFromObject(iqerror);
            Console.WriteLine(strXML);

            IQ iqnew = Utility.ParseObjectFromXMLString(strXML, typeof(IQ)) as IQ;
            ErrorType type = iqnew.Error.ErrorDescription.ErrorType;

            System.Diagnostics.Debug.Assert(iqnew.Error.Code == "405");
            System.Diagnostics.Debug.Assert(type == ErrorType.badformat);

        }

        static void TestPubSubParsing()
        {
            PubSubIQ iq = new PubSubIQ();
            iq.From = "me@no.where";
            iq.To = "you@over.there";
            iq.Type = IQType.set.ToString();

            GroceryItem item = new GroceryItem() { Name = "onions", Person = "Me", Price = "$10.02" };

            iq.PubSub.Publish = new Publish();
            iq.PubSub.Publish.Item = new PubSubItem();
            iq.PubSub.Publish.Item.SetNodeFromObject(item);
            iq.PubSub.Publish.Item.Id = "my_super_id";



            string strXML = Utility.GetXMLStringFromObject(iq);
            Console.WriteLine(strXML);

            PubSubIQ iqnew = Utility.ParseObjectFromXMLString(strXML, typeof(PubSubIQ)) as PubSubIQ;
            GroceryItem newitem = iqnew.PubSub.Publish.Item.GetObjectFromXML<GroceryItem>();

            System.Diagnostics.Debug.Assert(newitem.Name == item.Name);
        }
    }


    [DataContract]
    [XmlRoot(ElementName = "groceryitem", Namespace="http://testnamsapce.com")]
    public class GroceryItem
    {
        public GroceryItem()
        {
        }

        private string m_strName = null;
        [XmlElement(ElementName = "name")]
        [DataMember]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        private bool m_bIsAccountedFor = false;
        [XmlElement(ElementName = "isaccountedfor")]
        [DataMember]
        public bool IsAccountedFor
        {
            get { return m_bIsAccountedFor; }
            set { m_bIsAccountedFor = value; }
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

        private string m_strPrice = "";
        /// <summary>
        /// The price that was paid or will be paid
        /// </summary>
        [XmlElement(ElementName = "price")]
        [DataMember]
        public string Price
        {
            get { return m_strPrice; }
            set { m_strPrice = value; }
        }

        private string m_strItemId = Guid.NewGuid().ToString();
        [XmlElement(ElementName = "itemid")]
        [DataMember]
        public string ItemId
        {
            get { return m_strItemId; }
            set { m_strItemId = value; }
        }
    }
}
