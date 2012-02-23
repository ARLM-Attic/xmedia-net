using System;
using System.Net;

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Net.XMPP
{
    /// <summary>
    /// Manages adding and removing items from a pub sub node of type 'T'.  Node must already exist
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PubSubNodeManager<T> : Logic
    {
        public PubSubNodeManager(string strNode, XMPPClient client) : base(client)
        {
            Node = strNode;
        }

        private string m_strNode = "";
        public string Node
        {
          get { return m_strNode; }
          set { m_strNode = value; }
        }

        public void AddItem(string strItemId, T item)
        {
            string strItemXML = Utility.GetXMLStringFromObject(item);

            PubSubPublishIQ iq = new PubSubPublishIQ();
            iq.To = new JID(string.Format("pubsub.{0}", XMPPClient.Domain));
            iq.From = XMPPClient.JID;
            iq.Node = Node;

            iq.Item.Id = strItemId;
            iq.Item.InnerItemXML = strItemXML;

            Items.Add(item);
            ItemIdToObject.Add(strItemId, item);

            ListSentIQs.Add(iq.ID);

            XMPPClient.SendXMPP(iq);
        }

        public string GetPubSubIdForItem(T item)
        {
            foreach (string strKey in ItemIdToObject.Keys)
            {
                if (ItemIdToObject[strKey].Equals(item) == true)
                    return strKey;
            }
            return null;
        }

        public void UpdateItem(string strItemId, T item)
        {
            string strItemXML = Utility.GetXMLStringFromObject(item);
            PubSubPublishIQ iq = new PubSubPublishIQ();
            iq.To = new JID(string.Format("pubsub.{0}", XMPPClient.Domain));
            iq.From = XMPPClient.JID;
            iq.Node = Node;

            iq.Item.Id = strItemId;
            iq.Item.InnerItemXML = strItemXML;

            ListSentIQs.Add(iq.ID);

            XMPPClient.SendXMPP(iq);
        }

        public void DeleteItem(string strItemId, T item)
        {
            IQ pubsub = new IQ();
            pubsub.Type = IQType.set.ToString();
            pubsub.From = XMPPClient.JID;
            pubsub.To = new JID(string.Format("pubsub.{0}", XMPPClient.Domain));

            string strXML = PubSubOperation.RetractNodeXML.Replace("#NODE#", this.Node);
            strXML = strXML.Replace("#ITEM#", strItemId);
            pubsub.InnerXML = strXML;

            Items.Remove(item);
            ItemIdToObject.Remove(strItemId);

            ListSentIQs.Add(pubsub.ID);

            XMPPClient.SendXMPP(pubsub);
        }

        PubSubGetIQ IQGetAll = null;
        public void GetAllItems(string strSubId)
        {
            IQGetAll = new PubSubGetIQ();
            IQGetAll.To = new JID(string.Format("pubsub.{0}", XMPPClient.Domain));
            IQGetAll.From = XMPPClient.JID;
            IQGetAll.Node = Node;
            IQGetAll.SubId = strSubId;
            IQGetAll.Item = null;
            

            XMPPClient.SendXMPP(IQGetAll);
            
        }


        List<string> ListSentIQs = new List<string>();

        //public void Remove

        Dictionary<string, T> ItemIdToObject = new Dictionary<string, T>();
#if WINDOWS_PHONE
        private ObservableCollection<T> m_listItems = new ObservableCollection<T>();
        public ObservableCollection<T> Items
        {
            get { return m_listItems; }
            set { m_listItems = value; }
        }
#elif MONO
        private ObservableCollection<T> m_listItems = new ObservableCollection<T>();
        public ObservableCollection<T> Items
        {
            get { return m_listItems; }
            set { m_listItems = value; }
        }
#else
        private ObservableCollectionEx<T> m_listItems = new ObservableCollectionEx<T>();
        public ObservableCollectionEx<T> Items
        {
            get { return m_listItems; }
            set { m_listItems = value; }
        }
#endif

        public override bool NewIQ(IQ iq)
        {
            if (ListSentIQs.Contains(iq.ID) == true)
            {
                ListSentIQs.Remove(iq.ID);
                return true;
            }
            else if ((IQGetAll != null) && (IQGetAll.ID == iq.ID))
            {
                if (iq is PubSubResultIQ)
                {
                    PubSubResultIQ psem = iq as PubSubResultIQ;
                    if (psem != null)
                    {
                        if (psem.Items.Count > 0)
                        {
                            m_listItems.Clear();
                            ItemIdToObject.Clear();

                            if (psem.Node == Node)
                            {
                                foreach (PubSubItem psi in psem.Items)
                                {
                                    T item = (T)Utility.ParseObjectFromXMLString(psi.InnerItemXML, typeof(T));
                                    if (item != null)
                                    {
                                        Items.Add(item);
                                        ItemIdToObject.Add(psi.Id, item);

                                    }
                                }
                            }
                        }
                    }
                }
            }

            return base.NewIQ(iq);
        }

        public override bool NewMessage(Message iq)
        {
            if (iq is PubSubEventMessage)
            {
                PubSubEventMessage psem = iq as PubSubEventMessage;
                if (psem.Node == Node)
                {
                    foreach (PubSubItem psi in psem.Items)
                    {
                        T item = (T)Utility.ParseObjectFromXMLString(psi.InnerItemXML, typeof(T));
                        if (item != null)
                        {
                            if (ItemIdToObject.ContainsKey(psi.Id) == false)
                            {
                                Items.Add(item);
                                ItemIdToObject.Add(psi.Id, item);
                            }
                            else  /// item with this id already exists, replace it with the new version
                            {
                                T itemtoremove = ItemIdToObject[psi.Id];
                                Items.Remove(itemtoremove);
                                Items.Add(item);
                                ItemIdToObject[psi.Id] = item;
                            }

                        }
                    }
                    foreach (string strRetract in psem.RetractIds)
                    {
                        if (ItemIdToObject.ContainsKey(strRetract) == true)
                        {
                            T itemtoremove = ItemIdToObject[strRetract];
                            Items.Remove(itemtoremove);
                            ItemIdToObject.Remove(strRetract);
                        }
                    }
                }
                 
            }
            return base.NewMessage(iq);
        }

    }
}
