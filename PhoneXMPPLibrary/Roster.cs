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
using System.Collections.ObjectModel;

namespace System.Net.XMPP
{
    /// <summary>
    /// An instance of this class is made for every presence full jid.  We need this
    /// since we can login from many different places, and we want to be aware of all instances of this roster item
    /// </summary>
    public class RosterItemPresenceInstance : System.ComponentModel.INotifyPropertyChanged
    {
        public RosterItemPresenceInstance(JID fulljid)
        {
            FullJID = fulljid;
        }

        private JID m_jidFullJID = null;

        public JID FullJID
        {
            get { return m_jidFullJID; }
            set { m_jidFullJID = value; }
        }

        private PresenceStatus m_objPresence = new PresenceStatus();

        public PresenceStatus Presence
        {
            get { return m_objPresence; }
            set
            {
                if (m_objPresence != value)
                {
                    m_objPresence = value;
                    FirePropertyChanged("Presence");
                }
            }
        }

        #region INotifyPropertyChanged Members

        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
            {
#if WINDOWS_PHONE
               Deployment.Current.Dispatcher.BeginInvoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#else
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#endif
            }
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged = null;

        #endregion
    }

    public enum MessageSendOption
    {
        SendPriority,
        SendToLastRecepient,
        SendToAll,
    }

    public class RosterItem : System.ComponentModel.INotifyPropertyChanged
    {
        public RosterItem(XMPPClient client)
        {
            XMPPClient = client;
        }

        public RosterItem()
        {
        }



        private XMPPClient m_objXMPPClient = null;
        public XMPPClient XMPPClient
        {
          get { return m_objXMPPClient; }
          set { m_objXMPPClient = value; }
        }

        public override string ToString()
        {
               return string.Format("{0} ({1}), {2}", JID, Name, Presence);
        }

        private JID m_objJID = null;

        public JID JID
        {
            get { return m_objJID; }
            set 
            { 
                m_objJID = value;
                Conversation.JID = value;
                LastFullJIDToGetMessageFrom = value;
            }
        }
        private string m_strName = "";

        public string Name
        {
            get 
            {
                int nInstances = m_listClientInstances.Count;
                if (nInstances <= 1)
                    return m_strName;
                else
                    return string.Format("{0} ({1} places)", m_strName, nInstances);
            }
            set
            {
                if (m_strName != value)
                {
                    m_strName = value;
                    FirePropertyChanged("Name");
                }
            }
        }

        private geoloc m_objGeoLoc = new geoloc() { lat = 0.0f, lon = 0.0f };
        /// <summary>
        ///  The current location of this roster item
        /// </summary>
        public geoloc GeoLoc
        {
            get { return m_objGeoLoc; }
            set { m_objGeoLoc = value; FirePropertyChanged("GeoString"); }
        }

        public string GeoString
        {
            get
            {
                if ((m_objGeoLoc.lat == 0) && (m_objGeoLoc.lon == 0))
                    return "unknown location";
                else
                    return string.Format("lat: {0:N2}, lon: {1:N2}", m_objGeoLoc.lat, m_objGeoLoc.lon);
            }
            set
            {
                
            }
        }

        private TuneItem m_objTune = new TuneItem();
        /// <summary>
        /// The tune this roster item is listening to
        /// </summary>
        public TuneItem Tune
        {
            get { return m_objTune; }
            set { m_objTune = value; }
        }


        private vcard m_objvCard = new vcard();

        public vcard vCard
        {
            get { return m_objvCard; }
            set 
            { 
                m_objvCard = value; 

                /// See if we have a new photo
                /// 
                if ((m_objvCard.Photo != null) && (m_objvCard.Photo.Bytes != null))
                {
                    /// compute the hash, save the file, set the image
                    /// 
                    string strHash = XMPPClient.AvatarStorage.WriteAvatar(m_objvCard.Photo.Bytes);
                    this.AvatarImagePath = strHash;
                }
            }
        }


        public JID LastFullJIDToGetMessageFrom = "";

        public void AddSendTextMessage(TextMessage msg)
        {
            Conversation.AddMessage(msg);
        }
        public void AddRecvTextMessage(TextMessage msg)
        {
            LastFullJIDToGetMessageFrom = msg.From;
            Conversation.AddMessage(msg);
        }

        public void SendChatMessage(string strMessage, MessageSendOption option)
        {
            if (option == MessageSendOption.SendToAll)
            {
                SendChatMessageToAllAvailableInstances(strMessage);
                return;
            }

            TextMessage txtmsg = new TextMessage();
            txtmsg.Received = DateTime.Now;
            txtmsg.From = XMPPClient.JID;

            if (option == MessageSendOption.SendPriority)
               txtmsg.To = this.JID.BareJID;
            else
               txtmsg.To = LastFullJIDToGetMessageFrom;

            txtmsg.Message = strMessage;
            XMPPClient.SendChatMessage(txtmsg);
        }

        public void SendChatMessageToAllAvailableInstances(string strMessage)
        {
            foreach (RosterItemPresenceInstance instance in ClientInstances)
            {
                TextMessage txtmsg = new TextMessage();
                txtmsg.Received = DateTime.Now;
                txtmsg.From = XMPPClient.JID;
                txtmsg.To = instance.FullJID;
                txtmsg.Message = strMessage;
                XMPPClient.SendChatMessage(txtmsg);
            }
        }

   
     
        private string m_strSubscription = "";

        public string Subscription
        {
            get { return m_strSubscription; }
            set
            {
                if (m_strSubscription != value)
                {
                    m_strSubscription = value;
                    FirePropertyChanged("Subscription");
                }
            }
        }

        private PresenceStatus m_objPresence = new PresenceStatus();

        public PresenceStatus Presence
        {
            get { return m_objPresence; }
            set
            {
                if (m_objPresence != value)
                {
                    m_objPresence = value;
                    FirePropertyChanged("Presence");
                }
            }
        }

        public void SetPresence(PresenceMessage pres)
        {
           

            if ((pres.From.Resource != null) && (pres.From.Resource.Length > 0))
            {
                RosterItemPresenceInstance instance = FindInstance(pres.From);
                if (instance != null)
                {
                    instance.Presence = pres.PresenceStatus;
                    if (pres.PresenceStatus.PresenceType == PresenceType.unavailable)
                    {
                        lock (m_lockClientInstances)
                        {
                            m_listClientInstances.Remove(instance);
                        }

                        FirePropertyChanged("ClientInstances");
                        FirePropertyChanged("Name");
                    }
                }
                else
                {
                    instance = new RosterItemPresenceInstance(pres.From);
                    instance.Presence = pres.PresenceStatus;
                    lock (m_lockClientInstances)
                    {
                        m_listClientInstances.Add(instance);
                    }
                    FirePropertyChanged("ClientInstances");
                    FirePropertyChanged("Name");
                }
            }


            /// Get the precense of the most available and latest client instance
            /// 
            PresenceStatus beststatus = pres.PresenceStatus;
            if (pres.PresenceStatus.PresenceType != PresenceType.available)
            {
                lock (m_lockClientInstances)
                {
                    foreach (RosterItemPresenceInstance instance in m_listClientInstances)
                    {
                        if (instance.Presence.PresenceType == PresenceType.available)
                        {
                            beststatus = instance.Presence;
                            break;
                        }
                    }
                }
            }

            Presence = beststatus;

            
            //System.Diagnostics.Debug.WriteLine(item.ToString());
            XMPPClient.FireListChanged(1);
        }

        RosterItemPresenceInstance FindInstance(JID jid)
        {
            lock (m_lockClientInstances)
            {
                foreach (RosterItemPresenceInstance instance in m_listClientInstances)
                {
                    if (jid.Equals(instance.FullJID) == true)
                        return instance;
                }
            }
            return null;
        }


        object m_lockClientInstances = new object();

#if WINDOWS_PHONE
        private ObservableCollection<RosterItemPresenceInstance> m_listClientInstances = new ObservableCollection<RosterItemPresenceInstance>();
        public ObservableCollection<RosterItemPresenceInstance> ClientInstances
        {
            get { return m_listClientInstances; }
            set { m_listClientInstances = value; }
        }
#else
        private ObservableCollectionEx<RosterItemPresenceInstance> m_listClientInstances = new ObservableCollectionEx<RosterItemPresenceInstance>();
        public ObservableCollectionEx<RosterItemPresenceInstance> ClientInstances
        {
            get { return m_listClientInstances; }
            set { m_listClientInstances = value; }
        }
#endif



        private string m_strGroup = "Unknown";

        public string Group
        {
            get { return m_strGroup; }
            set 
            {
                if (m_strGroup != value)
                {
                    m_strGroup = value;
                    FirePropertyChanged("Group");
                }
            }
        }


        private List<string> m_listGroups = new List<string>();

        public List<string> Groups
        {
            get { return m_listGroups; }
            set 
            {
                if (m_listGroups != value)
                {
                    m_listGroups = value;
                    FirePropertyChanged("Groups");
                }
            }
        }


        public bool HasLoadedConversation = false;

        private Conversation m_objConversation = new Conversation("null@null");

        public Conversation Conversation
        {
            get { return m_objConversation; }
            set { m_objConversation = value; }
        }

        private bool m_bHasNewMessages = false;

        public bool HasNewMessages
        {
            get { return m_bHasNewMessages; }
            set
            {
                if (m_bHasNewMessages != value)
                {
                    m_bHasNewMessages = value;
                    FirePropertyChanged("HasNewMessages");
                    FirePropertyChanged("NewMessagesVisible");
                }
            }
        }

        public Visibility NewMessagesVisible
        {
            get
            {
                if (m_bHasNewMessages == true)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            set { }
        }

        private string m_strImagePath = null;

        public string AvatarImagePath
        {
            get { return m_strImagePath; }
            set 
            {
                if (m_strImagePath != value)
                {
                    m_strImagePath = value;
                    FirePropertyChanged("Avatar");
                }
                else
                {
                }
            }
        }

        /// <summary>
        /// Must keep this bitmapimage as a class member or it won't appear.  Not sure why it's going out of scope
        /// when it should be referenced by WPF
        /// </summary>
        System.Windows.Media.Imaging.BitmapImage OurImage = null;
        public ImageSource Avatar
        {
            get
            {

                if (m_strImagePath != null)
                    OurImage = XMPPClient.AvatarStorage.GetAvatarImage(m_strImagePath);

                if (OurImage == null)
                {
                    Uri uri  = null;
                    uri = new Uri("Avatars/darkavatar.png", UriKind.Relative);
                    OurImage = new System.Windows.Media.Imaging.BitmapImage(uri);
                }


                return OurImage;
            }
        }


        public rosteritem Node { get; set; }

        #region INotifyPropertyChanged Members

        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
            {
#if WINDOWS_PHONE
               Deployment.Current.Dispatcher.BeginInvoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#else
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#endif
            }
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged = null;

        #endregion
    }
}
