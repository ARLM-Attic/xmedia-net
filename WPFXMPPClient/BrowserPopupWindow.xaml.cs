/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Net.XMPP;
using System.IO;
using LocationClasses;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using LocationClasses;

using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;



namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for FileTransferWindow.xaml
    /// </summary>
    public partial class BrowserPopupWindow : Window
    {
        #region Google Earth Friends
        ObservableCollectionEx<BuddyPosition> BuddyPositions = new ObservableCollectionEx<BuddyPosition>();


        #endregion

        #region KEEP
        public BrowserPopupWindow()
        {
            InitializeComponent();
            
        }

        /// <summary>
        /// Our XMPP client
        /// </summary>
        public XMPPClient XMPPClient = null;
       

        void HandleStateChanged()
        {


            if (XMPPClient.XMPPState == XMPPState.Connected)
            {

            }
            else if (XMPPClient.XMPPState == XMPPState.Unknown)
            {
                //this.FirePropertyChanged("ConnectedStateBrush");
                //ComboBoxPresence.IsEnabled = false;
                //ButtonAddBuddy.IsEnabled = false;
                //SaveAccounts();

                //if (AudioMuxerWindow.IsLoaded == true)
                //    AudioMuxerWindow.CloseAllSessions();
            }
            else if (XMPPClient.XMPPState == XMPPState.Ready)
            {
                //this.FirePropertyChanged("ConnectedStateBrush");
                //ComboBoxPresence.IsEnabled = true;
                //ButtonAddBuddy.IsEnabled = true;
                //this.ImageAvatar.Source = XMPPClient.Avatar;
                //XMPPClient.SetGeoLocation(0, 0);


                //XMPPClient.SetGeoLocation(32.234, -97.3453);
            }
            else if (XMPPClient.XMPPState == XMPPState.AuthenticationFailed)
            {
                //if (MessageBox.Show(string.Format("Incorrect username or password.  Would you like to create user '{0}'?", XMPPClient.UserName), "Authentication Failed", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                //{
                //    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(CreateAccountAsync));
                //}
                //else
                //{
                //    XMPPClient.Disconnect();
                //}
            }
            else
            {
            }
        }

        public void RegisterXMPPClient(XMPPClient client)
        {
            XMPPClient = client;

            CollectionViewSource source = FindResource("SortedRosterItems") as CollectionViewSource;
            source.Source = BuddyPositions;
                // XMPPClient.RosterItems;


            MapManager.RegisterXMPPClient(client);
            XMPPClient.OnRetrievedRoster += new EventHandler(XMPPClient_OnRetrievedRoster);
            XMPPClient.OnStateChanged += new EventHandler(XMPPClient_OnStateChanged);
            XMPPClient.OnRosterItemsChanged += new EventHandler(XMPPClient_OnRosterItemsChanged);
            //addresses = AudioMuxerWindow.FindAddresses();

            
            //MapManager.RegisterXMPPClient(XMPPClient);

            //XMPPClient.JingleSessionManager.OnNewSession += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventWithInfo(JingleSessionManager_OnNewSession);
            //XMPPClient.JingleSessionManager.OnNewSessionAckReceived += new JingleSessionManager.DelegateJingleSessionEventBool(JingleSessionManager_OnNewSessionAckReceived);
            //XMPPClient.JingleSessionManager.OnSessionAcceptedAckReceived += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventBool(JingleSessionManager_OnSessionAcceptedAckReceived);
            //XMPPClient.JingleSessionManager.OnSessionAcceptedReceived += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventWithInfo(JingleSessionManager_OnSessionAcceptedReceived);
            //XMPPClient.JingleSessionManager.OnSessionTerminated += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEvent(JingleSessionManager_OnSessionTerminated);
            //XMPPClient.JingleSessionManager.OnSessionTransportInfoReceived += new System.Net.XMPP.Jingle.JingleSessionManager.DelegateJingleSessionEventWithInfo(JingleSessionManager_OnSessionTransportInfoReceived);
            //XMPPClient.JingleSessionManager.OnSessionTransportInfoAckReceived += new JingleSessionManager.DelegateJingleSessionEventBool(JingleSessionManager_OnSessionTransportInfoAckReceived);


            ///// Get all our speaker and mic devices
            ///// 
            //MicrophoneDevices = ImageAquisition.NarrowBandMic.GetMicrophoneDevices();
            //SpeakerDevices = ImageAquisition.NarrowBandMic.GetSpeakerDevices();
            //Init();
        
            // Go ahead and load html into browser
            LoadSourceCode();
        }

        void XMPPClient_OnRosterItemsChanged(object sender, EventArgs e)
        {          
            this.Dispatcher.Invoke(new DelegateVoid(SetRoster));
        }

        void SetRoster()
        {
            //this.ListBoxRoster.ItemsSource = null;
            CollectionViewSource source = FindResource("SortedRosterItems") as CollectionViewSource;
            source.View.Refresh();
            //source.Source = null;
            //source.Source = XMPPClient.RosterItems;
            //source.DeferRefresh();

            //this.ListBoxRoster.ItemsSource = XMPPClient.RosterItems;
            XMPPClient_OnRetrievedRoster(null, null);

            
            //var source = new CollectionViewSource();
            //source.SortDescriptions.Add(new System.ComponentModel.SortDescription("Name", System.ComponentModel.ListSortDirection.Ascending));
            //source.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("Group"));

            //source.Source = XMPPClient.RosterItems;
            ////var selected = from c in XMPPClient.RosterItems group c by c.Group into n select new GroupingLayer<string, RosterItem>(n);
            //this.ListBoxRoster.ItemsSource = source;
        }

        public delegate void DelegateVoid();
        void XMPPClient_OnStateChanged(object sender, EventArgs e)
        { 
            Dispatcher.Invoke(new DelegateVoid(HandleStateChanged));
        }

        private void LoadSourceCode()
        {
            string strContents = OpenFile("Code/BrowserWindowSource.html");
            TextBoxBrowserSourceCode.Text = strContents;
            WebBrowserMain.NavigateToString(TextBoxBrowserSourceCode.Text);
            loadCompleted = true;
        }

        public void ZoomToRosterItem(RosterItem item)
        {

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
                this.DragMove();
        }

        #endregion

        #region Determine whether to keep or not
        ObservableCollection<MapProvider> MapProviders = new ObservableCollection<MapProvider>();
        private MapProperties m_MapProperties = new MapProperties();

        public MapProperties MapProperties
        {
            get { return m_MapProperties; }
            set { m_MapProperties = value; }
        }

        private bool m_bSingleRosterItemMap = true;

        public bool SingleRosterItemMap
        {
            get { return m_bSingleRosterItemMap; }
            set { m_bSingleRosterItemMap = value; }
        }

        private RosterItem m_OurRosterItem = new RosterItem();

        public RosterItem OurRosterItem
        {
            get { return m_OurRosterItem; }
            set
            {
                m_OurRosterItem = value;
 
            }
        }

        public void SetMainRosterItem(RosterItem rosterItem)
        {
            OurRosterItem = rosterItem;
            //CenterMap(rosterItem);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           

            bWindowLoaded = true;
            
            {
                XMPPClient_OnRetrievedRoster(null, null);
                bInitialRosterRetrievalComplete = true;
            }

            Init();
        }

        bool bWindowLoaded = false;

        public void Init()
        {
            Init(false);
        }

        public void Init(bool bForceRefresh) 
        {
            if (bForceRefresh || MapManager == null)
            {
                MapManager = new LocationClasses.MapManager(); // , OurRosterItem);  
                MapManager.OnAddMarker += new LocationClasses.MapManager.DelegateAddMarker(MapManager_OnAddMarker);
                MapManager.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(MapManager_PropertyChanged);
                MapManager.RegisterXMPPClient(XMPPClient);
                MapManager.Start();
            }

            // MapManager.AddRosterItem(OurRosterItem, true, true, true);
            MapProviders = DataManager.GetAllMapProviders();
            ComboBoxMapProvider.ItemsSource = MapProviders;
            ComboBoxMapProvider.SelectedIndex = 0;

            CodeSnippets = CodeBuilder.GetAllCodeSnippets();

 
            // First time we build the source code, 
            // set the size of the map-canvas to the size of its outlying control.
            //var parent = WebBrowserMain.Parent;
            //if (parent.GetType() == typeof(FrameworkElement))
            //{
            //    FrameworkElement fe = parent as FrameworkElement;
            //    double actualHeight = fe.ActualHeight;
            //    double actualWidth = fe.ActualWidth;
            //    // Get the webbrowser's padding values 
            //    Thickness thickness = WebBrowserMain.Margin;
            //    double browserHeight = actualHeight - thickness.Left - thickness.Right;
            //    double browserWidth = actualWidth - thickness.Top - thickness.Bottom;


            //}




            // howevroster is received.er we also need something to load the current roster, since this window will not be opened until *after* the  
            // Generate the source code initially upon load.
            // ignore the old map then.

            // CodeSnippet sourceCode = CodeBuilder.LoadFromFile(TextBoxRelativePath.Text);

            string strContents = OpenFile("Code/BrowserWindowSource.html");

            // populate code so when navigate to, it's there.

            TextBoxBrowserSourceCode.Text = strContents;


            // TextBoxBrowserSourceCode.Text = sourceCode.Html;

            ListViewCodeSnippets.ItemsSource = CodeSnippets;

            WebBrowserMain.NavigateToString(TextBoxBrowserSourceCode.Text);
            loadCompleted = true;

            // When is dictionary set up? 
            RefreshRosterItemList();

            // this will be called every time the roster is received.
            XMPPClient.OnRetrievedRoster += new EventHandler(XMPPClient_OnRetrievedRoster);

            SetupRosterItemNotifications();


            //if (AddOrMoveMarker(OurRosterItem))
            //{

            //}

           // CenterMap(OurRosterItem);
        }

        void MapManager_OnAddMarker(LocationClasses.MapRosterItem mapRosterItem)
        {
            // add soemwhere else , cheating for now
            ListViewRosterItems.ItemsSource = MapManager.MapRosterItemDictionary.Values;

            if (AddMarker(mapRosterItem))
            {

            }
            else
            {

            }
            // throw new NotImplementedException();
        }

        void MapManager_AddMarker(object sender, MarkerArgs e)
        {
            if (AddMarker(e.MapRosterItem))
            {

            }
            else
            {

            }
            // throw new NotImplementedException();
        }

        // I need a callback for the MapManager to invoke when one of its guys changes, that tells this browser to call a javascript function.
        // Could I just call that javascript function from the MapManager class? Not sure. Seems to be a UI specific thing.
        // Then no matter the front end, I could just hook into that and use it.


        void MapManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null)
            {
                // Can I figure out which guy changed?
            }

            // throw new NotImplementedException();
        }

        bool bInitialSourceCodeBuildComplete = false;
        bool bInitialRosterRetrievalComplete = false;
        XMPPState currentState = XMPPState.Unknown;

        private bool IsBuddyAlreadyInRoster(RosterItem item)
        {
            var q = from buddy in BuddyPositions where buddy.RosterItem.JID != null && buddy.RosterItem.JID.BareJID != item.JID.BareJID select buddy;
            if (q != null && q.Count() > 0)
            {
                return true;
            }
            return false;
        }

        private BuddyPosition FindBuddy(RosterItem item)
        {
            var q = from buddy in BuddyPositions where buddy.RosterItem.JID != null && buddy.RosterItem.JID.BareJID != item.JID.BareJID select buddy;
            if (q != null && q.Count() > 0)
            {
                return q.FirstOrDefault() as BuddyPosition;
            }
            return null;
        }

        void XMPPClient_OnRetrievedRoster(object sender, EventArgs e)
        {           
            #region Google Earth Friends
            /// bind to ourlist
            /// 
            if (bWindowLoaded)
                // (XMPPClient.XMPPState == XMPPState.Ready)
            {
                foreach (RosterItem item in XMPPClient.RosterItems)
                {
                    if (IsBuddyAlreadyInRoster(item) == false)
                    {
                        BuddyPosition buddy = new LocationClasses.BuddyPosition(item);
                        buddy.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(buddy_PropertyChanged);
                        BuddyPositions.Add(buddy);
                        
                    }
                }

                this.Dispatcher.Invoke(new Action(() => { this.ListViewBuddies.ItemsSource = BuddyPositions; this.DataContext = this; this.ListViewBuddies.DataContext = BuddyPositions; }));
            }
            #endregion






            if (SetupRosterItemNotifications())
            {
                //// Initial time... 
                //string strJavaScriptHtml = MapBuilder.BuildJavaScriptSourceCode(MapProperties, OurRosterItem);
                //    // MapManager);

                //if (TextBoxBrowserSourceCode == null)
                //    return; // refreshing too early. control not built yet. 

                //TextBoxBrowserSourceCode.Text = strJavaScriptHtml;

                // initialize dictionary?
            }
            RefreshRosterItemList();
            // throw new NotImplementedException();
        }

        void buddy_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {         
            if (e.PropertyName == "IsDirty")
            {
                BuddyPosition buddy = sender as BuddyPosition;
                if (buddy == null)
                    return;
                AddMarker(buddy);
            }
        }

        private bool SetupRosterItemNotifications()
        {
            if (XMPPClient == null)
                return false;

            // Set up event callbacks and add to MapManager Or do I populate MapManager when ListViewItems loads?
            foreach (RosterItem rosterItem in XMPPClient.RosterItems)
            {
                rosterItem.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(rosterItem_PropertyChanged);
            }
            return true;
        }

        void rosterItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
               RosterItem item = sender as RosterItem;
               if (item == null)
                   return;

               // if (item != OurRosterItem)
               //     return;

               // WebBrowserMap.Navigate(strURL);
               if (e.PropertyName == "GeoLoc" || e.PropertyName == "Name" || e.PropertyName == "JID" || e.PropertyName == "Avatar" || e.PropertyName == "Presence")
               // i.e. not at 2 places anymore              
               {
                   // This is the method that is called when (a) a user moves, or (b) the map is initially loaded -  we call it on the existing roster.
                   // When someone is designated as "the main buddy to show" we call CenterMap instead of AddMarker 
                   // (AddMarker is used for Add/Move/Update, it's really all the same.)
                   if (AddOrMoveMarker(item))
                   {
                       this.Dispatcher.Invoke(new Action(() =>
                       {
                        RefreshRosterItemList();
                       }));
                   }

                   //MapRosterItem mapRosterItem = null;
                   //if (MapManager.DictMapRosterItems.ContainsKey(item.JID.BareJID))
                   //{
                   //    mapRosterItem = MapManager.DictMapRosterItems[item.JID.BareJID];
                   //}
                   //else
                   //{

                   //    mapRosterItem = MapManager.AddRosterItem(item, true, false, true); // not sure what defaults should be!
                   //}
                   //if (mapRosterItem == null)
                   //{
                   //    /// did not find it? 
                   //    /// 
                   //    MessageBox.Show("Problem accessing the map roster item for " + item.JID.BareJID);
                   //    return;
                   //}

                   //// maybe set it and keep it in MapRosterItem, in case I need it!

                   //if (mapRosterItem == null)
                   //{
                   //    MessageBox.Show("This map roster item should not be null for user: " + item.JID.BareJID);
                   //    return;
                   //}
                   //if (MoveMarker(mapRosterItem))
                   //{
                   //    System.Console.WriteLine("Moved marker for " + item.JID.BareJID.ToString() + " from " + mapRosterItem.PreviousLocation + " to " + mapRosterItem.CurrentLocation);
                   //}
               }
               //else if (e.PropertyName == "Name" || e.PropertyName == "JID" || e.PropertyName == "Avatar") // i.e. not at 2 places anymore
               //{
               //    // update infowindow or marker
               //}
            // throw new NotImplementedException();
        }

        //private bool UpdateMarker(RosterItem item)
        //{

        //    bool bExists = false;
        //    MapRosterItem mapRosterItem = null;
        //    if (MapManager.DictMapRosterItems.ContainsKey(item.JID.BareJID))
        //    {
        //        mapRosterItem = MapManager.DictMapRosterItems[item.JID.BareJID];
        //        bExists = true;
        //    }
        //    else
        //    {
        //        mapRosterItem = MapManager.AddRosterItem(item, true, false, true); // not sure what defaults should be!
        //    }
        //    if (mapRosterItem == null)
        //    {
        //        /// did not find it? 
        //        /// 
        //        MessageBox.Show("Problem accessing the map roster item for " + item.JID.BareJID);
        //        return false;
        //    }

        //    //if (mapRosterItem)
        //    {

        //        string strBareJID = mapRosterItem.RosterItem.JID.BareJID;
        //        int bPanTo = mapRosterItem.IsTheMainRosterItem ? 1 : 0;
        //        // TODO : Add back in after get it working with global array!

        //        //callScriptFunctionWithParamButton(item.RosterItem);

        //        string strAvatarPath = String.Format(
        //            "var avatarPath=\"{0}\";\r\n", "file://" + mapRosterItem.LocalAvatarPath.Replace("\\", "\\\\"));

        //        if (CallJavaScriptFunction(WebBrowserMain, "moveMarker", new object[] { 
        //            strBareJID, mapRosterItem.RosterItem.JID.ToString(), mapRosterItem.RosterItem.GeoLoc.lat,
        //            mapRosterItem.RosterItem.GeoLoc.lon, 
        //            mapRosterItem.RosterItem.GeoLoc.TimeStamp, strAvatarPath, 0 }))
        //        {
        //            //function addMarker(BareJID, name, lat, lng, timestamp, avatarPath, markerStyleType) {


        //        }
        //    }
        //    return true;
        //}

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if ((e.Key == Key.D) && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                TabItemKML.Visibility = System.Windows.Visibility.Visible;
                TabItemSourceCode.Visibility = System.Windows.Visibility.Visible;
                //if (SendRawXMLWindow.IsLoaded == false)
                //    SendRawXMLWindow.Show();
                //else if (this.SendRawXMLWindow.Visibility == System.Windows.Visibility.Hidden)
                //    this.SendRawXMLWindow.Visibility = System.Windows.Visibility.Visible;

                //SendRawXMLWindow.Activate();
            }

            base.OnPreviewKeyDown(e);
        }

        private bool AddOrMoveMarker(RosterItem item)
        {

            bool bExists = false;
            MapRosterItem mapRosterItem = null;
            mapRosterItem = MapManager.AddRosterItem(item, true, true, true);

            //if (MapManager.DictMapRosterItems.ContainsKey(item.JID.BareJID))
            //{
            //    mapRosterItem = MapManager.DictMapRosterItems[item.JID.BareJID];
            //    bExists = true;
            //}
            //else
            //{
            //    mapRosterItem = MapManager.AddRosterItem(item, true, false, true); // not sure what defaults should be!
            //}

            //if (MapManager.AdjustMapWindowRosterItemList(item))
            //{
            //    mapRosterItem = MapManager.DictMapRosterItems[item.JID.BareJID];
            //}

            //if (mapRosterItem == null)
            //{
            //    MessageBox.Show("This map roster item should not be null for user: " + item.JID.BareJID);
            //    return false;
            //}

            //System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
            //string strMainRosterItem = ser.Serialize(mapRosterItem.RosterItem);
            //System.Console.WriteLine("serialized roster item: " + strMainRosterItem);

            //mapRosterItem.strRosterItemJSONSerialized = strMainRosterItem;
            bool bRes = true;

            if (loadCompleted)
            {
                bRes = AddMarker(mapRosterItem);
                if (bRes)
                {
                    System.Console.WriteLine("Added marker for " + item.JID.BareJID.ToString() + " from " + mapRosterItem.PreviousLocation + " to " + mapRosterItem.CurrentLocation);
                }
                else
                {
                    System.Console.WriteLine("Error adding marker for " + item.JID.BareJID);
                }


            }
            //if (bExists)
            //{
            //    if (MoveMarker(mapRosterItem))
            //    {
            //        System.Console.WriteLine("Moved marker for " + item.JID.BareJID.ToString() + " from " + mapRosterItem.PreviousLocation + " to " + mapRosterItem.CurrentLocation);
            //    }
            //}
            //else
            //{
            //    if (AddMarker(mapRosterItem))
            //    {
            //        System.Console.WriteLine("Added marker for " + item.JID.BareJID.ToString()); //  + " from " + mapRosterItem.PreviousLocation + " to " + mapRosterItem.CurrentLocation);

            //    }
            //}
            SetBounds();
            return bRes;
        }

        private bool SetBounds()
        {
            return true;
            // not time yet... 
            if (loadCompleted == false)
                return false;

            bool bResult = true;

            bResult = (CallJavaScriptFunction(WebBrowserMain, "setBounds", null));

            if (!bResult)
            {
                // try again
                bResult = (CallJavaScriptFunction(WebBrowserMain, "setBounds", null));
            }
            return bResult;
        }

        private bool CenterMap(RosterItem item)
        {
            return true;

            // not time yet... 
            if (loadCompleted == false)
                return false;

            bool bResult = true;
            string strBareJID = item.JID.BareJID;
            double lat = item.GeoLoc.lat;
            double lon = item.GeoLoc.lon;

            bResult = (CallJavaScriptFunction(WebBrowserMain, "centerMap", new object[] { 
                    strBareJID }));
            if (!bResult)
            {
                // try again
                bResult = (CallJavaScriptFunction(WebBrowserMain, "centerMapOnLatLon", new object[] { 
                    lat, lon }));
            }
            return bResult;
        }

        private bool MoveMarker(MapRosterItem item)
        {
            return AddMarker(item);

            bool bResult = true;

            if (MapManager.MoveMarker(item))
            {
                string strBareJID = item.RosterItem.JID.BareJID;
                int bPanTo = item.IsTheMainRosterItem ? 1 : 0;
                // TODO : Add back in after get it working with global array!

                //callScriptFunctionWithParamButton(item.RosterItem);

                string strAvatarPath = String.Format("{0}", "file://" + item.LocalAvatarPath.Replace("\\", "\\\\"));
                double dbTimestamp = DateTimeToDouble(item.RosterItem.GeoLoc.TimeStamp);
                //addMarker(BareJID, name, lat, lng, avatarPath, bShowInfoWindow)

                bResult = (CallJavaScriptFunction(WebBrowserMain, "moveMarker", new object[] { 
                    strBareJID, item.RosterItem.JID.ToString(), item.RosterItem.GeoLoc.lat, item.RosterItem.GeoLoc.lon, 
                    dbTimestamp, strAvatarPath, 0 }));
                {
                    //function addMarker(BareJID, name, lat, lng, timestamp, avatarPath, markerStyleType) {


                }
                if (!bResult)
                {
                    MessageBox.Show("Error calling MoveMarker for " + item.RosterItem.JID.BareJID);
                }
            }
            return true;
        }

        private double DateTimeToDouble(DateTime dt)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);

            return ts.TotalMilliseconds;
            // return dt.ToOADate();
                
                // DateTime.UtcNow
      //         .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
      //         .TotalMilliseconds;
        }


        private bool AddMarker(MapRosterItem item) // , MapTriggerType mapTriggerType)
        {
            if (!loadCompleted)
                return false;

            if (WebBrowserMain.Document == null)
                return false;
            bool bResult = true;

            if (item.RosterItem.GeoLoc.lat == 0 && item.RosterItem.GeoLoc.lon == 0)
                return false;
           
            this.Dispatcher.Invoke((Action)delegate()
            {
                // if text thing is empty then that's another way to know the code isn't there yet.... 
                //if (TextBoxBrowserSourceCode.Text == null || TextBoxBrowserSourceCode.Text.Length < 1)
                //    return false;

                // if (MapManager.AddMarker(item))

                string strBareJID = item.RosterItem.JID.BareJID;
                int bPanTo = item.IsTheMainRosterItem ? 1 : 0;
                // TODO : Add back in after get it working with global array!

                //callScriptFunctionWithParamButton(item.RosterItem);

                string strAvatarPath = String.Format("{0}", "file://" + item.LocalAvatarPath.Replace("\\", "\\\\"));
                // String.Format("var avatarPath=\"{0}\";\r\n", "file://" + item.LocalAvatarPath.Replace("\\", "\\\\"));
                double dbTimestamp = DateTimeToDouble(item.RosterItem.GeoLoc.TimeStamp);

                // addMarker(BareJID, name, lat, lng, avatarPath, bShowInfoWindow)
                try
                {
                    // System.Threading.Thread.Sleep(2000);
                    bResult = (CallJavaScriptFunction(WebBrowserMain, "addMarker", new object[] { 
                    strBareJID, item.RosterItem.JID.ToString(), item.RosterItem.GeoLoc.lat, item.RosterItem.GeoLoc.lon, 
                    dbTimestamp, strAvatarPath, bPanTo }));
                    {
                        //function addMarker(BareJID, name, lat, lng, timestamp, avatarPath, markerStyleType) {
                    }
                    if (bResult == false)
                    {
                        bResult = (CallJavaScriptFunction(WebBrowserMain, "addMarker", new object[] { 
                    strBareJID, item.RosterItem.JID.ToString(), item.RosterItem.GeoLoc.lat, item.RosterItem.GeoLoc.lon, 
                    dbTimestamp, strAvatarPath, bPanTo }));
                        if (bResult == false)
                        {
                            bResult = (CallJavaScriptFunction(WebBrowserMain, "addMarker", new object[] { 
                    strBareJID, item.RosterItem.JID.ToString(), item.RosterItem.GeoLoc.lat, item.RosterItem.GeoLoc.lon, 
                    dbTimestamp, strAvatarPath, bPanTo }));
                            // if (bResult== false)

                            //MessageBox.Show("Error calling AddMarker for " + item.RosterItem.JID.BareJID);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error calling AddMarker for " + item.RosterItem.JID.BareJID + ". " + ex.Message);
                }
            }
            );


            return bResult;
            //return true;
        }
 
        private bool AddMarker(BuddyPosition item) // , MapTriggerType mapTriggerType)
        {
            if (!loadCompleted)
                return false;

            if (WebBrowserMain.Document == null)
                return false;

            // if text thing is empty then that's another way to know the code isn't there yet.... 
            //if (TextBoxBrowserSourceCode.Text == null || TextBoxBrowserSourceCode.Text.Length < 1)
            //    return false;

            // if (MapManager.AddMarker(item))

            string strBareJID = item.RosterItem.JID.BareJID;
            // int bPanTo = item.IsTheMainRosterItem ? 1 : 0;
            // TODO : Add back in after get it working with global array!

            //callScriptFunctionWithParamButton(item.RosterItem);

            string strAvatarPath = String.Format("{0}", "file://" + item.LocalAvatarPath.Replace("\\", "\\\\"));
            // String.Format("var avatarPath=\"{0}\";\r\n", "file://" + item.LocalAvatarPath.Replace("\\", "\\\\"));
            double dbTimestamp = DateTimeToDouble(item.RosterItem.GeoLoc.TimeStamp);

            // addMarker(BareJID, name, lat, lng, avatarPath, bShowInfoWindow)
            bool bResult = true;
            try
            {
                // System.Threading.Thread.Sleep(2000);
                bResult = (CallJavaScriptFunction(WebBrowserMain, "addMarker", new object[] { 
                    strBareJID, item.RosterItem.JID.ToString(), item.RosterItem.GeoLoc.lat, item.RosterItem.GeoLoc.lon, 
                    dbTimestamp, strAvatarPath }));
               
                if (bResult == false)
                {
                    MessageBox.Show("Error calling AddMarker for " + item.RosterItem.JID.BareJID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calling AddMarker for " + item.RosterItem.JID.BareJID + ". " + ex.Message);
            }

            return bResult;
        }

        //private bool MoveMarker(MapRosterItem item)
        //{
        //    if (MapManager.MoveMarker(item))
        //    {
        //        string strMarker = "marker0"; // change to use javascript array - keyed by BareJID
        //        int bPanTo = item.IsTheMainRosterItem ? 1 : 0;
        //        {

        //        }
        //    }
        //    return true;
        //}

        private void callScriptFunctionNoParamButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure the HTML document has loaded before attempting to
            // invoke script of the document page. You could set loadCompleted
            // to true when the LoadCompleted event on the WebBrowser fires.
            if (this.loadCompleted)
            {
                try
                {
                    this.WebBrowserMain.InvokeScript("JavaScriptFunctionWithoutParameters");
                }
                catch (Exception ex)
                {
                    string msg = "Could not call script: " +
                                 ex.Message +
                                "\n\nPlease click the 'Load HTML Document with Script' button to load.";
                    MessageBox.Show(msg);
                }
            }
        }

        private void callScriptFunctionWithParamButton_Click(object sender, RoutedEventArgs e)
        {
            callScriptFunctionWithParamButton(MapManager.MainRosterItem);
        }

        private void callScriptFunctionWithParamButton(RosterItem rosterItem)
        {
            // Make sure the HTML document has loaded before attempting to
            // invoke script of the document page. You could set loadCompleted
            // to true when the LoadCompleted event on the WebBrowser fires.
            if (this.loadCompleted)
            {
                try
                {
                    // this.webBrowser.InvokeScript("JavaScriptFunctionWithParameters", new object[] { "sharechiwai" });

                    this.WebBrowserMain.InvokeScript("JavaScriptFunctionWithParameters", rosterItem.JID.ToString());
                }
                catch (Exception ex)
                {
                    string msg = "Could not call script: " +
                                 ex.Message +
                                "\n\nPlease click the 'Load HTML Document with Script' button to load.";
                    MessageBox.Show(msg);
                }
            }

        }

        public void AddRosterItemBody()
        {
            this.TextBoxTitle.Text = "Buddy Map - " + OurRosterItem.JID.BareJID;
            this.Title = "Buddy Map - " + OurRosterItem.JID.BareJID;

            if (MapManager == null)
            {
                MapManager = new MapManager(); // , m_OurRosterItem);
                MapManager.RegisterXMPPClient(XMPPClient);

                //MapManager.MainRosterItem = m_OurRosterItem;
            }
            //if (MapManager.AdjustMapWindowRosterItemList(m_OurRosterItem))
            {
                AddOrMoveMarker(m_OurRosterItem);
            }

            // CenterMap(m_OurRosterItem);

            // for a test. try to call this function here... 
            // get the map roster item for this... 
            if (MapManager.DictMapRosterItems.ContainsKey(m_OurRosterItem.JID.BareJID))
            {


                MapRosterItem mri = MapManager.DictMapRosterItems[m_OurRosterItem.JID.BareJID];
                if (mri != null)
                {
                    //if (CallJavaScriptFunction(WebBrowserMain, "addSerializedMarker", new object[] { mri.strRosterItemJSONSerialized }))
                    {

                    }
                    //else
                    {

                    }
                }
            }
            // ReloadBrowser();
            //ListViewMapRosterItems.ItemsSource = MapManager.DictMapRosterItems.Values;
        }

        public bool AddRosterItem(RosterItem rosterItem)
        {
            OurRosterItem = rosterItem;
            SetMainRosterItem(rosterItem);
           // AddRosterItemBody();

            BuddyPosition buddy = FindBuddy(rosterItem);
            if (buddy != null)
            {
                buddy.ShowOnMap = true;
            }

            return true;
        }

        private bool BuildOrUpdateMapRosterItemDictionary()
        {

            return true;
        }

        public MapManager MapManager = new LocationClasses.MapManager();
        //Delegate that will be the pointer for the event, contains two arguments 
        //sender (object that raised it) and OperationEventArgs for the event arguments
        public delegate void ReloadBrowserEvent(object sender, BrowserArgs e);

        //Event name 
        public event ReloadBrowserEvent ReloadBrowser;

        //Delegate that will be the pointer for the event, contains two arguments 
        //sender (object that raised it) and OperationEventArgs for the event arguments
        public delegate void InvokeJavaScriptScriptEvent(object sender, ScriptArgs e);
        //Event name 
        public event InvokeJavaScriptScriptEvent InvokeJavaScriptScript;

        //Delegate that will be the pointer for the event, contains two arguments 
        //sender (object that raised it) and OperationEventArgs for the event arguments
        public delegate void LoadKMLEvent(object sender, BrowserArgs e);
        //Event name 
        public event LoadKMLEvent LoadKML;

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserMain.NavigateToString(TextBoxBrowserSourceCode.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading source code into browser: " + ex.Message);
            }

            if (ReloadBrowser != null)
            {
                BrowserArgs ba = new BrowserArgs(TextBoxBrowserSourceCode.Text);

                ReloadBrowser(sender, ba);
            }

            //LoadImageFromURL();
            //this.Dispatcher.Invoke(new Action(() =>
            //{
            // LoadImageFromURL();

            // WebBrowserMap.Navigate(strURL);
            //})
            //);
        }

        private void WebBrowserMain_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            loadCompleted = true;
        }

        bool loadCompleted = false;

        private void ButtonLoadKML_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListViewCodeSnippets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ListViewCodeSnippets_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void ButtonLoadHTML_Click(object sender, RoutedEventArgs e)
        {
            CodeSnippet codeSnippet = ((FrameworkElement)sender).DataContext as CodeSnippet;
            if (codeSnippet == null)
                return;

            TextBoxBrowserSourceCode.Text = codeSnippet.Html;
            // now reload browser
            ReloadBrowser(sender, new BrowserArgs(codeSnippet.Html));
        }

        private void ComboBoxMapProviderType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            var selectedItem = cb.SelectedItem;
            MapProvider selectedProvider = selectedItem as MapProvider;
            if (selectedProvider == null)
                return;

            MapBuilder.MapProvider = selectedProvider;
            CodeSnippets = CodeBuilder.GetAllCodeSnippets();
            var q = from s in CodeSnippets where s.ApiName == selectedProvider.Name select s;
            if (q != null && q.Count() > 0)
            {
                VisibleCodeSnippets = q as ObservableCollection<CodeSnippet>;
                ListViewCodeSnippets.ItemsSource = VisibleCodeSnippets;
                if (ListViewCodeSnippets.Items.Count > 0)
                    ListViewCodeSnippets.SelectedIndex = 0;

            }


        }

        private CodeBuilder m_CodeBuilder = new CodeBuilder();

        public CodeBuilder CodeBuilder
        {
            get { return m_CodeBuilder; }
            set { m_CodeBuilder = value; }
        }

        ObservableCollection<CodeSnippet> m_CodeSnippets = new ObservableCollection<CodeSnippet>();

        public ObservableCollection<CodeSnippet> CodeSnippets
        {
            get { return m_CodeSnippets; }
            set { m_CodeSnippets = value; }
        }

        ObservableCollection<CodeSnippet> m_VisibleCodeSnippets = new ObservableCollection<CodeSnippet>();

        public ObservableCollection<CodeSnippet> VisibleCodeSnippets
        {
            get { return m_VisibleCodeSnippets; }
            set { m_VisibleCodeSnippets = value; }
        }



        private MapBuilder m_MapBuilder = new MapBuilder();

        public MapBuilder MapBuilder
        {
            get { return m_MapBuilder; }
            set { m_MapBuilder = value; }
        }

        private void TextBoxSizeVertical_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBoxSizeHorizontal_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ComboBoxMapType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TabItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TabItem tabItem = sender as TabItem;
            // should be the correct one!



            // base.OnRenderSizeChanged(sizeInfo);
        }

        private void WebBrowserMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size previousSize = e.PreviousSize;
            Size newSize = e.NewSize;

            double actualHeight = newSize.Height;
            double actualWidth = newSize.Width;

            // Get the webbrowser's padding values 
            //Thickness thickness = WebBrowserMain.Margin;
            //double browserHeight = actualHeight - thickness.Left - thickness.Right;
            //double browserWidth = actualWidth - thickness.Top - thickness.Bottom;



        }

        private bool CallJavaScriptFunction(WebBrowser webBrowser, string strFunctionName, object[] parameters)
        {
            if (webBrowser.Document == null)
                return false;

            //if (TextBoxBrowserSourceCode.Text == null || TextBoxBrowserSourceCode.Text.Length < 1)
            //{
            //    return false;
            //}

            bool bSuccess = true;
            // call javascript function resize map
            // Make sure the HTML document has loaded before attempting to
            // invoke script of the document page. You could set loadCompleted
            // to true when the LoadCompleted event on the WebBrowser fires.
            if (this.loadCompleted) // or just check the webBrowser.Document variable!
            {
                try
                {
                    if (parameters == null || parameters.Count() < 1)
                        webBrowser.InvokeScript(strFunctionName);
                    else
                        webBrowser.InvokeScript(strFunctionName, parameters);
                }
                catch (Exception ex)
                {
                    bSuccess = false;
                    string msg = "Could not call function " + strFunctionName + ": " +
                                 ex.Message;
                   // MessageBox.Show("Error calling function " + strFunctionName);
                    //"\n\nPlease click the 'Load HTML Document with Script' button to load.";
                    // MessageBox.Show(msg);

                }
            }
            return bSuccess;
        }

        private void CheckBoxRosterShowAll_Checked(object sender, RoutedEventArgs e)
        {
            // Add the rest of the roster into the list. Could also do on load. But rather not.
            foreach (RosterItem rosterItem in XMPPClient.RosterItems)
            {
                if (MapManager.DictMapRosterItems.ContainsKey(rosterItem.JID.ToString()) == false)
                {
                    MapManager.DictMapRosterItems.Add(rosterItem.JID.ToString(), new MapRosterItem()
                    {
                        RosterItem = rosterItem,
                        DateTimeEnqueued = DateTime.Now,
                        IsDisplayed = false,
                        IsDisplayedInViewableWindow = false,
                        IsTheMainRosterItem = false,
                        IsMe = false,
                        KMLBuilderForRosterItem = new KMLBuilderForRosterItem(),
                        MarkerInfoWindowStyleType = MarkerInfoWindowStyleType.InfoWindowWithTextOnly
                    });


                }
            }

            foreach (var kvp in MapManager.DictMapRosterItems)
            {
                MapRosterItem item = kvp.Value;

            }
        }

        private void ListViewRosterItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MapRosterItem mapRosterItem = ((FrameworkElement)sender).DataContext as MapRosterItem;
            if (mapRosterItem == null)
                return;
            if (MapManager.CenterOnMe(mapRosterItem) == null)
            {

            }
        }

        private void CenterOnRosterItem(MapRosterItem mapRosterItem)
        {
            if (MapManager.CenterOnMe(mapRosterItem) == null)
            {

            }
        }

        private void CheckBoxIsDisplayed_Checked(object sender, RoutedEventArgs e)
        {
            MapRosterItem mapRosterItem = ((FrameworkElement)sender).DataContext as MapRosterItem;
            if (mapRosterItem == null)
                return;
            // make sure roster item is in list of roster items to be displayed. 
            // if isDisplayed was false, refresh the map.
            // perhaps this means we want to zoom in on the item? not sure.

            if (MapManager.DictMapRosterItems.ContainsKey(mapRosterItem.RosterItem.JID.BareJID))
            {
                if (MapManager.DictMapRosterItems[mapRosterItem.RosterItem.JID.BareJID].IsDisplayed == false)
                {
                    MapManager.DictMapRosterItems[mapRosterItem.RosterItem.JID.BareJID].IsDisplayed = true;
                    //Refresh();
                }
            }
            else
            {
                // it should contain it! unless we start showing the entire roster in the control.
                MapManager.DictMapRosterItems.Add(mapRosterItem.RosterItem.JID.BareJID, mapRosterItem);
                MapManager.DictMapRosterItems[mapRosterItem.RosterItem.JID.BareJID].IsDisplayed = true;
                //Refresh();
            }
            //OurRosterItem = mapRosterItem.RosterItem;
            // does this refresh itself?
        }


        private void DisplayRosterItem(MapRosterItem mapRosterItem)
        {
            if (MapManager.CenterOnMe(mapRosterItem) == null)
            {
                // something went wrong.
            }
        }

        private void ButtonRecordKML_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Google Earth Friends
        private void ButtonSaveKML_Click(object sender, RoutedEventArgs e)
        {
            BuddyPosition buddy = ((Button)sender).DataContext as BuddyPosition;
            if (buddy != null)
            {
                string strDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\GeoTracks";
                if (System.IO.Directory.Exists(strDirectory) == false)
                    System.IO.Directory.CreateDirectory(strDirectory);

                string strFileName = string.Format("{0}/{1}_{2}.kml", strDirectory, buddy.RosterItem.JID.User, Guid.NewGuid());
                WriteBuddyKML(strFileName, buddy);
                System.Diagnostics.Process.Start(strFileName);
            }
        }

        private void ButtonClearKML_Click(object sender, RoutedEventArgs e)
        {
            BuddyPosition buddy = ((Button)sender).DataContext as BuddyPosition;
            if (buddy != null)
            {
                buddy.ClearCoordinates();
            }
        }

        void WriteBuddyKML(string strFileName, BuddyPosition buddy)
        {
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

            string strXML = GetXMLStringFromObject(kml);
            System.IO.FileStream output = new FileStream(strFileName, FileMode.Create);
            byte[] bXML = System.Text.UTF8Encoding.UTF8.GetBytes(strXML);
            output.Write(bXML, 0, bXML.Length);
            output.Close();

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

        #endregion


        private void ListViewRosterItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ButtonViewKML_Click(object sender, RoutedEventArgs e)
        {

        }

        private int RefreshRosterItemList()
        {
            return 0;

            int nNumberOfMapRosterItemsInDictionary = 0;

            // Add the rest of the roster into the list. Could also do on load. But rather not.
            // only add if I'm online!
            // this could be based on if Show All is checked.
            foreach (RosterItem rosterItem in XMPPClient.RosterItems)
            {
                if (rosterItem.Presence.IsOnline == false)
                    if (CheckBoxRosterShowAll != null && (bool)CheckBoxRosterShowAll.IsChecked == false)
                        continue;

                if (MapManager.ItemExists(rosterItem) == false)
                {
                    MapRosterItem newMapRosterItem = MapManager.AddRosterItem(rosterItem, false, false, false);
                    if (newMapRosterItem != null)
                    {
                        Console.WriteLine("Map Roster Item created for user: " + rosterItem.JID.BareJID);
                    }
                    else
                    {
                        Console.WriteLine("Map Roster Item was *not* created for user: " + rosterItem.JID.BareJID);
                    }
                }
            }

            nNumberOfMapRosterItemsInDictionary = MapManager.DictMapRosterItems.Count();


            ListViewRosterItems.ItemsSource = MapManager.DictMapRosterItems.Values;


            return nNumberOfMapRosterItemsInDictionary;
        }

        private void ListViewRosterItems_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshRosterItemList();
        }

        private void CheckBoxIsDisplayedInViewableWindow_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonCallJavaScriptFunction_Click(object sender, RoutedEventArgs e)
        {
            if (!loadCompleted)
                return;

            string strContents = TextBoxJavaScriptFunctionName.Text;
            string test = @"public static SomeReturnType GetSomething(string param1, int param2)";
            //string strPattern = @"(?<scope>\w+)\s+(?<static>static\s+)?(?<return>\w+)\s+(?<name>\w+)\((?<parms>[^)]+)\)";
            string strPattern = @"(?<functionName>[^\(]+)\((?<parameters>[^\)]*)\)";
            string strPatternParameters = @"\([^\)]*\)";


            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(strPattern);

            var match = regex.Match(strContents);
            string strFunctionName = "";
            string strParameters = "";
            List<string> strParameterList = new List<string>();

            List<object> parameterList = new List<object>();
            if (match.Success)
            {
                if (match.Groups["functionName"].Length > 0)
                {
                    strFunctionName = match.Groups["functionName"].Value;
                }

                if (match.Groups["parameters"].Length > 0)
                {
                    strParameters = match.Groups["parameters"].Value;

                    string[] parms = strParameters.Split(',');
                    strParameterList = new List<string>(parms);

                    foreach (string parm in parms)
                    {
                        string strParm = parm;
                        strParm = strParm.TrimEnd();
                        strParm = strParm.TrimStart();
                        parameterList.Add(strParm);
                    }
                }
            }
            // (test, @"(?<scope>\w+)\s+(?<static>static\s+)?(?<return>\w+)\s+(?<name>\w+)\((?<parms>[^)]+)\)");

            // parse out parameters: 
            object[] parameters = null;
            if (parameterList.Count() > 0)
            {
                parameters = parameterList.ToArray();
            }
            try
            {
                CallJavaScriptFunction(WebBrowserMain, strFunctionName, parameters);

                if (InvokeJavaScriptScript != null)
                    InvokeJavaScriptScript(sender, new ScriptArgs()
                    {
                        FunctionName = strFunctionName,
                        ParameterList = strParameters,
                        UnparsedContents = strContents,
                        Parameters = parameters
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error invoking function: " + strContents + ". Error: " + ex.Message);
            }
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            string strFileName = "";
            if (TextBoxBrowserSourceCode.Text.Length > 0)
                strFileName = SaveExistingCode();

            string strContents = OpenFile();
            if (strContents != null && strContents.Length > 0)
            {
                TextBoxBrowserSourceCode.Text = strContents;
            }
            else
            {
                MessageBox.Show(String.Format("File contents are empty"));
            }
            //{



            //// Create OpenFileDialog

            //Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            //// Set filter for file extension and default file extension
            //dlg.DefaultExt = ".html";
            //dlg.Filter = "Text documents (.txt)|*.txt|HTML documents (.html)|*.html|JavaScript documents (.js)|*.js";


            //// Display OpenFileDialog by calling ShowDialog method
            //Nullable<bool> result = dlg.ShowDialog();

            //// Get the selected file name and display in a TextBox
            //if (result == true)
            //{
            //    // Open document
            //    string filename = dlg.FileName;
            //    TextBoxFileName.Content = String.Format("File: {0}", filename);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       


            //    //Paragraph paragraph = new Paragraph();
            //    //paragraph.Inlines.Add(System.IO.File.ReadAllText(filename));

            //    //FlowDocument document = new FlowDocument(paragraph);
            //    //FlowDocReader.Document = document;

            //    // Open the file into a StreamReader
            //    StreamReader file = File.OpenText(filename);
            //    // Read the file into a string
            //    string s = file.ReadToEnd();

            //    TextBoxBrowserSourceCode.Text = s;
            //}
        }

        //private bool SaveTextBox()
        //{
        //    return true;
        //}

        private string SaveExistingCode()
        {
            string strFileName = "";

            if (TextBoxBrowserSourceCode.Text.Length > 0)
            {
                // Configure the message box to be displayed
                string messageBoxText = "Do you want to save the current source code?";
                string caption = "Save";
                MessageBoxButton button = MessageBoxButton.YesNoCancel;
                MessageBoxImage icon = MessageBoxImage.Warning;
                // Display message box
                //MessageBox.Show(messageBoxText, caption, button, icon);
                // Display message box
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);

                // Process message box results
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        // User pressed Yes button
                        // ...
                        strFileName = SaveFile();
                        break;
                    case MessageBoxResult.No:
                        // User pressed No button
                        // ...
                        break;
                    case MessageBoxResult.Cancel:
                        // User pressed Cancel button
                        // ...
                        break;
                }
            }
            //string strContent = OpenFile();

            return strFileName;
        }

        private string SaveFile()
        {
            string strFileName = "";

            Stream myStream;
            Microsoft.Win32.SaveFileDialog saveFileDialog1 = new Microsoft.Win32.SaveFileDialog();

            saveFileDialog1.Filter = "html files (*.html)|*.html|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.DefaultExt = "html";

            if (saveFileDialog1.ShowDialog() == true)
            {
                using (StreamWriter outfile = new StreamWriter(saveFileDialog1.FileName))
                {
                    outfile.Write(TextBoxBrowserSourceCode.Text);
                }

                MessageBox.Show("Contents written to file " + saveFileDialog1.FileName);


                //if ((myStream = saveFileDialog1.OpenFile()) != null)
                //{
                //    // Code to write the stream goes here.
                //    myStream.Close();
                //}
            }
            return strFileName;
        }

        private string OpenFile(string strFileName)
        {
            StringBuilder sb = new StringBuilder();
            string str = "";
            Nullable<bool> result2 = null;
            //string filename = "";
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Stream myStream = null;
            FileStream fileStream = null;

            //if (strFilename == "")
            //{
            //    // Configure open file dialog box

            //    //dlg.FileName = "SourceCode_" + DateTime.Now.ToString("{0:yyyymmDD_HHmmss}"); // Default file name
            //    dlg.DefaultExt = ".html"; // Default file extension
            //    dlg.Filter = "HTML documents (.html)|*.html"; // Filter files by extension

            //    // Show open file dialog box
            //    result2 = dlg.ShowDialog();
            //    if (result2 == true)
            //        strFilename = dlg.FileName;
            //}
            //else
            //{
            //    result2 = true;
            //}


            // Process open file dialog box results
            // if (result2 == true)


            //OpenFileDialog openFileDialog1 = new OpenFileDialog();

            ////openFileDialog1.InitialDirectory = "c:\\" ;
            //openFileDialog1.Filter = "html files (*.html)|*.html|All files (*.*)|*.*";
            //openFileDialog1.FilterIndex = 2;
            //openFileDialog1.RestoreDirectory = true;

            //if (openFileDialog1.ShowDialog() == true)
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                try
                {
                    using (StreamReader sr = new StreamReader(strFileName))
                    {
                        String line;
                        // Read and display lines from the file until the end of
                        // the file is reached.
                        while ((line = sr.ReadLine()) != null)
                        {
                            Console.WriteLine(line);
                            sb.AppendLine(line);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return sb.ToString();
        }

        private string OpenFile() // string strFilename)
        {
            StringBuilder sb = new StringBuilder();
            string str = "";
            Nullable<bool> result2 = null;
            //string filename = "";
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Stream myStream = null;
            FileStream fileStream = null;

            //if (strFilename == "")
            //{
            //    // Configure open file dialog box

            //    //dlg.FileName = "SourceCode_" + DateTime.Now.ToString("{0:yyyymmDD_HHmmss}"); // Default file name
            //    dlg.DefaultExt = ".html"; // Default file extension
            //    dlg.Filter = "HTML documents (.html)|*.html"; // Filter files by extension

            //    // Show open file dialog box
            //    result2 = dlg.ShowDialog();
            //    if (result2 == true)
            //        strFilename = dlg.FileName;
            //}
            //else
            //{
            //    result2 = true;
            //}


            // Process open file dialog box results
            // if (result2 == true)


            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //openFileDialog1.InitialDirectory = "c:\\" ;
            openFileDialog1.Filter = "html files (*.html)|*.html|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == true)
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    String line;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                        sb.AppendLine(line);
                    }
                }
            }
            return sb.ToString();
        }
        //catch (Exception e)
        //{
        //    // Let the user know what went wrong.
        //    Console.WriteLine("The file could not be read:");
        //    Console.WriteLine(e.Message);
        //}


        //  if ((fileStream = (FileStream)dlg.OpenFile()) != null)
        //// if ((myStream = dlg.OpenFile()) != null)
        //  {
        //      using (fileStream)
        //      {
        //          str =fileStream.ToString=R
        //          // Insert code to read the stream here.
        //      }
        //  }


        // Open document

        //str = TextBoxBrowserSourceCode.Text;
        // Hook a write to the text file.
        // StreamWriter writer = new StreamWriter(strFilename);
        // Rewrite the entire value of s to the file
        // writer.Write(str);

        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            string strFileName = SaveFile();

            //SaveExistingCode();
        }

        private void CheckBoxZoomHere_Checked(object sender, RoutedEventArgs e)
        {
            MapRosterItem mapRosterItem = ((FrameworkElement)sender).DataContext as MapRosterItem;
            if (mapRosterItem == null)
                return;
            if (MapManager.CenterOnMe(mapRosterItem) == null)
            {

            }
        }
    }
        #endregion

}
