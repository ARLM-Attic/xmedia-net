using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Net.XMPP
{
    /// <summary>
    /// Handles services for personnel event.  Music, avatars and geo location go here
    /// </summary>
    public class PersonalEventingLogic : Logic
    {
        public PersonalEventingLogic(XMPPClient client)
            : base(client)
        {
        }

        List<string> ListSentIQs = new List<string>();

        public void PublishTuneInfo(TuneItem item)
        {
            string strTuneXML = Utility.GetXMLStringFromObject(item);

            PubSubPublishIQ iq = new PubSubPublishIQ();
            iq.To = null; /// null for personal eventing pub sub
            iq.From = XMPPClient.JID;
            iq.Node = "http://jabber.org/protocol/tune";

            iq.Item.InnerItemXML = strTuneXML;

            ListSentIQs.Add(iq.ID);
            XMPPClient.SendXMPP(iq);
        }

        public void PublishGeoInfo(geoloc item)
        {
            string strGeoInfo = Utility.GetXMLStringFromObject(item);

            PubSubPublishIQ iq = new PubSubPublishIQ();
            iq.To = null; /// null for personal eventing pub sub
            iq.From = XMPPClient.JID;
            iq.Node = "http://jabber.org/protocol/geoloc";
            iq.Item.InnerItemXML = strGeoInfo;

            ListSentIQs.Add(iq.ID);
            XMPPClient.SendXMPP(iq);
        }

         public void PublishAvatarData(byte [] bImageData, int nWidth, int nHeight)
        {
             // publish avatar data node
            avatardata data = new avatardata();
            data.ImageData = bImageData;
            string strAvatarInfo = Utility.GetXMLStringFromObject(data);

            string strHash = XMPPClient.AvatarStorage.WriteAvatar(bImageData);

            PubSubPublishIQ iq = new PubSubPublishIQ();
            iq.To = null; /// null for personal eventing pub sub
            iq.From = XMPPClient.JID;
            iq.Node = "urn:xmpp:avatar:data";
            iq.Item.InnerItemXML = strAvatarInfo;
            iq.Item.Id = strHash;
            ListSentIQs.Add(iq.ID);
            XMPPClient.SendXMPP(iq);


             // publish avatar meta data node
            avatarmetadata metadata = new avatarmetadata();
            metadata.ImageInfo.ByteLength = bImageData.Length;
            metadata.ImageInfo.Width = nWidth;
            metadata.ImageInfo.Height = nHeight;
            string strAvatarMetaData = Utility.GetXMLStringFromObject(metadata);
            PubSubPublishIQ iqmeta = new PubSubPublishIQ();
            iqmeta.To = null; /// null for personal eventing pub sub
            iqmeta.From = XMPPClient.JID;
            iqmeta.Node = "urn:xmpp:avatar:metadata";
            iqmeta.Item.InnerItemXML = strAvatarMetaData;
            iqmeta.Item.Id = strHash;
            ListSentIQs.Add(iqmeta.ID);
            XMPPClient.SendXMPP(iqmeta);

        }

         public void DownloadDataNode(JID jidto, string strNodeName, string strItem)
         {

             PubSubGetIQ iq = new PubSubGetIQ();
             iq.To = jidto; /// null for personal eventing pub sub
             iq.From = XMPPClient.JID;
             iq.Node = strNodeName;
             iq.Item.Id = strItem;
             
             ListSentIQs.Add(iq.ID);
             XMPPClient.SendXMPP(iq);
         }


        public override bool NewIQ(IQ iq)
        {
            if (ListSentIQs.Contains(iq.ID) == true)
            {
                ListSentIQs.Remove(iq.ID);
                return true;
            }

            return base.NewIQ(iq);
        }

        public override bool NewMessage(Message iq)
        {
            /// Look for pubsub events
            /// 
            if (iq is PubSubEventMessage)
            {
                PubSubEventMessage psem = iq as PubSubEventMessage;
                if (psem.Items.Count > 0)
                {

                    if (psem.Node == "http://jabber.org/protocol/tune")
                    {
                        TuneItem item = Utility.ParseObjectFromXMLString(psem.Items[0].InnerItemXML, typeof(TuneItem)) as TuneItem;
                        if (item != null)
                        {
                            /// find the roster item, set the tune item
                            RosterItem rosteritem = XMPPClient.FindRosterItem(iq.From);
                            if (rosteritem != null)
                            {
                                rosteritem.Tune = item;
                            }
                        }
                    }
                    else if (psem.Node == "http://jabber.org/protocol/geoloc")
                    {
                        geoloc item = Utility.ParseObjectFromXMLString(psem.Items[0].InnerItemXML, typeof(geoloc)) as geoloc;
                        if (item != null)
                        {
                            /// find the roster item, set the tune item
                            RosterItem rosteritem = XMPPClient.FindRosterItem(iq.From);
                            if (rosteritem != null)
                            {
                                rosteritem.GeoLoc = item;
                            }
                        }
                    }
                    else if (psem.Node == "http://jabber.org/protocol/mood")
                    {
                    }
                    else if (psem.Node == "urn:xmpp:avatar:metadata") /// item avatar metadata
                    {
                        /// We have update avatar info for this chap, we should then proceed to get the avatar data
                        /// 
                        foreach (PubSubItem objItem in psem.Items)
                        {
                            avatarmetadata meta = Utility.ParseObjectFromXMLString(objItem.InnerItemXML, typeof(avatarmetadata)) as avatarmetadata;
                            if (meta != null)
                            {
                                /// Request this node ? maybe we get it automatically?
                                /// 

                            }
                            /// Not sure why they would have more than 1 avatar item, so we'll ignore fom now
                            /// 
                            break;
                        }

                    }
                    else if (psem.Node == "urn:xmpp:avatar:data") /// item avatar
                    {
                        /// We have update avatar info for this chap, we should then proceed to get the avatar data
                        /// 
                        /// Works, but let's comment out for now to focus on more supported avatar methods
                        //foreach (PubSubItem objItem in psem.Items)
                        //{
                        //    avatardata data = Utility.ParseObjectFromXMLString(objItem.InnerItemXML, typeof(avatardata)) as avatardata;
                        //    if (data != null)
                        //    {
                        //        string strHash = XMPPClient.AvatarStorage.WriteAvatar(data.ImageData);
                        //        RosterItem item = XMPPClient.FindRosterItem(psem.From);
                        //        if (item != null)
                        //        {
                        //            item.AvatarImagePath = strHash;
                        //        }
                        //    }
                        //    /// Not sure why they would have more than 1 avatar item, so we'll ignore fom now
                        //    /// 
                        //    break;
                        //}

                    }

                }
            }

            return base.NewMessage(iq);
        }
    }
}
