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

using System.Xml.Linq;
using System.Collections.Generic;

namespace System.Net.XMPP
{
    public class PubSubItem
    {
        public PubSubItem()
        {
        }

        public PubSubItem(string strId)
        {
            Id = strId;
        }

        private string m_strId = null;

        public string Id
        {
            get { return m_strId; }
            set { m_strId = value; }
        }

        public virtual void AddXML(System.Xml.Linq.XElement elemItem)
        {
            //if ((InnerItemXML == null) || (elemItem == null))
              //  return;

            XElement item = new XElement("{http://jabber.org/protocol/pubsub}item");

            if (Id != null)
                item.Add(new XAttribute("id", Id));

            elemItem.Add(item);
            if ( (InnerItemXML != null) && ( InnerItemXML.Length > 0) )
            {
                XElement innerelem = XElement.Parse(InnerItemXML);
                item.Add(innerelem);
            }

        }

        public virtual void ParseXML(System.Xml.Linq.XElement elemItems)
        {

            if (elemItems.FirstNode == null)
                return;

            XElement elemitem = elemItems.FirstNode as XElement;
            if (elemitem != null)
            {
                if (elemitem.Name == "{http://jabber.org/protocol/pubsub}item")
                {
                    if (elemitem.Attributes("id") != null)
                        Id = elemitem.Attribute("id").Value;

                    XElement elemdistinct = elemitem.FirstNode as XElement;
                    if (elemdistinct != null)
                    {
                        InnerItemXML = elemdistinct.ToString();
                    }
                }
                else if (elemitem.Name == "{http://jabber.org/protocol/pubsub#event}item")
                {
                    if (elemitem.Attributes("id") != null)
                        Id = elemitem.Attribute("id").Value;

                    XElement elemdistinct = elemitem.FirstNode as XElement;
                    if (elemdistinct != null)
                    {
                        InnerItemXML = elemdistinct.ToString();
                    }
                }
                else
                   InnerItemXML = elemitem.ToString();
            }
        }

        private string m_strItemXML= null;
        public string InnerItemXML
        {
          get { return m_strItemXML; }
          set { m_strItemXML = value; }
        }
      
    }

    public class PubSubPublishIQ : IQ
    {
        public PubSubPublishIQ()
            : base()
        {
            this.Type = IQType.set.ToString();
        }
      

        public PubSubPublishIQ(string strXML)
            : base(strXML)
        {
        }

        private string m_strNode = "";

        public string Node
        {
            get { return m_strNode; }
            set { m_strNode = value; }
        }

        private PubSubItem m_objItem = new PubSubItem();

        public PubSubItem Item
        {
            get { return m_objItem; }
            set { m_objItem = value; }
        }
       

        public override void AddInnerXML(System.Xml.Linq.XElement elemMessage)
        {
            XElement pubsub = new XElement("{http://jabber.org/protocol/pubsub}pubsub");
            elemMessage.Add(pubsub);

            XElement publish = new XElement("{http://jabber.org/protocol/pubsub}publish");
            publish.Add(new XAttribute("node", Node));
            pubsub.Add(publish);

            if (Item != null)
                Item.AddXML(publish);

            base.AddInnerXML(elemMessage);
        }

        public override void ParseInnerXML(System.Xml.Linq.XElement elemMessage)
        {
            /// Extract pubsub element, publish element, then item element
            /// 
            foreach (XElement pubsub in elemMessage.Descendants("{http://jabber.org/protocol/pubsub}pubsub"))
            {
                XElement publish = pubsub.FirstNode as XElement;
                if ((publish != null) && (publish.Name == "{http://jabber.org/protocol/pubsub}publish"))
                {
                    if (publish.Attribute("node") != null)
                        Node = publish.Attribute("node").Value;

                    if (Item != null)
                    {
                        Item.ParseXML(publish);
                    }
                }
                break;
            }


            base.ParseInnerXML(elemMessage);
        }
    }

    public class PubSubGetIQ : IQ
    {
        public PubSubGetIQ ()
            : base()
        {
            this.Type = IQType.get.ToString();
        }


        public PubSubGetIQ(string strXML)
            : base(strXML)
        {
        }

        private string m_strNode = "";

        public string Node
        {
            get { return m_strNode; }
            set { m_strNode = value; }
        }

        private PubSubItem m_objItem = new PubSubItem();

        public PubSubItem Item
        {
            get { return m_objItem; }
            set { m_objItem = value; }
        }

        private string m_strSubId = null;

        public string SubId
        {
            get { return m_strSubId; }
            set { m_strSubId = value; }
        }

        public override void AddInnerXML(System.Xml.Linq.XElement elemMessage)
        {
            XElement pubsub = new XElement("{http://jabber.org/protocol/pubsub}pubsub");
            elemMessage.Add(pubsub);

            XElement publish = new XElement("{http://jabber.org/protocol/pubsub}items");
            publish.Add(new XAttribute("node", Node));
            if (SubId != null)
               publish.Add(new XAttribute("subid", SubId));

            pubsub.Add(publish);


            if (Item != null)
                Item.AddXML(publish);

            base.AddInnerXML(elemMessage);
        }

        public override void ParseInnerXML(System.Xml.Linq.XElement elemMessage)
        {
            /// Extract pubsub element, publish element, then item element
            /// 
            foreach (XElement pubsub in elemMessage.Descendants("{http://jabber.org/protocol/pubsub}pubsub"))
            {
                XElement publish = pubsub.FirstNode as XElement;
                if ((publish != null) && (publish.Name == "{http://jabber.org/protocol/pubsub}items"))
                {
                    if (publish.Attribute("node") != null)
                        Node = publish.Attribute("node").Value;
                    if (publish.Attribute("subid") != null)
                        Node = publish.Attribute("subid").Value;

                    if (Item != null)
                    {
                        Item.ParseXML(publish);
                    }
                }
                break;
            }


            base.ParseInnerXML(elemMessage);
        }
    }

    public class PubSubResultIQ : IQ
    {
        public PubSubResultIQ()
            : base()
        {
            this.Type = IQType.result.ToString();
        }
        public PubSubResultIQ(string strXML)
            : base(strXML)
        {
            this.Type = IQType.result.ToString();
        }

        public List<PubSubItem> Items = new List<PubSubItem>();
        public string Node = "";


        public override void AddInnerXML(System.Xml.Linq.XElement elemMessage)
        {
            XElement itemsnode = new XElement("{http://jabber.org/protocol/pubsub}items");
            itemsnode.Add(new XAttribute("node", Node));
            elemMessage.Add(itemsnode);

            foreach (PubSubItem item in Items)
            {
                item.AddXML(itemsnode);
            }

            base.AddInnerXML(elemMessage);
        }

        public override void ParseInnerXML(System.Xml.Linq.XElement elemMessage)
        {
            /// Extract pubsub element, publish element, then item element
            /// 

            Items.Clear();

            foreach (XElement pubsub in elemMessage.Descendants("{http://jabber.org/protocol/pubsub}pubsub"))
            {
                XElement publish = pubsub.FirstNode as XElement;
                if ((publish != null) && (publish.Name == "{http://jabber.org/protocol/pubsub}items"))
                {
                    if (publish.Attribute("node") != null)
                        Node = publish.Attribute("node").Value;

                    foreach (XElement elemitem in publish.Descendants("{http://jabber.org/protocol/pubsub}item"))
                    {
                        PubSubItem newitem = new PubSubItem();
                        if (elemitem.Attributes("id") != null)
                            newitem.Id = elemitem.Attribute("id").Value;

                        newitem.ParseXML(elemitem);
                        Items.Add(newitem);
                    }
                }
                break;
            }


            base.ParseInnerXML(elemMessage);
        }
    }

    public class PubSubEventMessage : Message
    {
        public PubSubEventMessage()
            : base()
        {
        }
        public PubSubEventMessage(string strXML)
            : base(strXML)
        {
        }

        public List<PubSubItem> Items = new List<PubSubItem>();
        public List<string> RetractIds = new List<string>();
        public string Node = "";


        public override void AddInnerXML(System.Xml.Linq.XElement elemMessage)
        {
            XElement pubsubevent = new XElement("{http://jabber.org/protocol/pubsub#event}event");
            elemMessage.Add(pubsubevent);

            XElement itemsnode = new XElement("{http://jabber.org/protocol/pubsub#event}items");
            itemsnode.Add(new XAttribute("node", Node));
            pubsubevent.Add(itemsnode);

            foreach (PubSubItem item in Items)
            {
                item.AddXML(itemsnode);
            }

            base.AddInnerXML(elemMessage);
        }

        public override void ParseInnerXML(System.Xml.Linq.XElement elemMessage)
        {
            /// Extract pubsub element, publish element, then item element
            /// 

            //<event xmlns="http://jabber.org/protocol/pubsub#event">
            //  <items node="GroceryList">
            //    <retract id="c62a3de9-4d88-493f-bbcb-3338dd18b7f4" />
            //  </items>
            //</event>

            Items.Clear();

            foreach (XElement pubsub in elemMessage.Descendants("{http://jabber.org/protocol/pubsub#event}event"))
            {
                XElement publish = pubsub.FirstNode as XElement;
                if ((publish != null) && (publish.Name == "{http://jabber.org/protocol/pubsub#event}items"))
                {
                    if (publish.Attribute("node") != null)
                        Node = publish.Attribute("node").Value;

                    foreach (XElement elemitem in publish.Descendants("{http://jabber.org/protocol/pubsub#event}item"))
                    {
                        PubSubItem newitem = new PubSubItem();
                        if (elemitem.Attributes("id") != null)
                            newitem.Id = elemitem.Attribute("id").Value;

                        newitem.ParseXML(publish);
                        Items.Add(newitem);
                    }
                    foreach (XElement elemitem in publish.Descendants("{http://jabber.org/protocol/pubsub#event}retract"))
                    {
                        if (elemitem.Attributes("id") != null)
                        {
                            RetractIds.Add(elemitem.Attribute("id").Value);
                        }
                    }
                }
                break;
            }


            base.ParseInnerXML(elemMessage);
        }
    }
}
