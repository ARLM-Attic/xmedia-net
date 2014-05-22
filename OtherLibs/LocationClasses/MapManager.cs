using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Net.XMPP;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Web.Script.Serialization;

namespace LocationClasses
{
    #region For future use if want to split XMPP functionality from MapManager
    //[DataContract]
    //public class XMPPMapManager
    //{
    //    #region Constructor
    //    public XMPPMapManager()
    //    {

    //    }
    //    #endregion

    //    #region Properties
    //    public XMPPClient XMPPClient = null;
    //    #endregion

    //    #region Methods

    //    #region Initialization
    //    public void RegisterXMPPClient(XMPPClient client)
    //    {
    //        XMPPClient = client;
    //    }

    //    public void Start()
    //    {
    //        // source.Source = XMPPClient.RosterItems;

    //        //this.ListBoxRoster.ItemsSource = XMPPClient.RosterItems;
    //        XMPPClient.OnRetrievedRoster += new EventHandler(XMPPClient_OnRetrievedRoster);
    //        XMPPClient.OnRosterItemsChanged += new EventHandler(XMPPClient_OnRosterItemsChanged);
    //        XMPPClient.OnStateChanged += new EventHandler(XMPPClient_OnStateChanged);
    //    }

    //    void XMPPClient_OnStateChanged(object sender, EventArgs e)
    //    {
    //        // throw new NotImplementedException();
    //    }

    //    void XMPPClient_OnRosterItemsChanged(object sender, EventArgs e)
    //    {
    //       // throw new NotImplementedException();
    //    }

    //    void XMPPClient_OnRetrievedRoster(object sender, EventArgs e)
    //    {
    //        // throw new NotImplementedException();
    //    }

    //    #endregion


    //    #endregion

    //    #region INotifyPropertyChanged Members
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    void FirePropertyChanged(string strName)
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(strName));
    //    }
    //    #endregion
    //}
    #endregion

    [DataContract]
    public class MapOptions : INotifyPropertyChanged 
    {
        public MapOptions()
        {

        }
        
        private bool m_bShowUserLocation = true;
        [DataMember]
        public bool ShowUserLocation
        {
            get { return m_bShowUserLocation; }
            set
            {
                if (m_bShowUserLocation != value)
                {
                    m_bShowUserLocation = value;
                    FirePropertyChanged("ShowUserLocation");
                } 
            }
        }

       #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }
        #endregion
    }

    [DataContract]
    public class DynamicMapAttributes : INotifyPropertyChanged
    {
        public DynamicMapAttributes()
        {

        }

        private geoloc m_MapCenter = new geoloc();
        [DataMember]
        public geoloc MapCenter
        {
            get { return m_MapCenter; }
            set
            {
                if (m_MapCenter != value)
                {
                    m_MapCenter = value;
                    FirePropertyChanged("MapCenter");
                }
            }
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }
        #endregion
    }

    [DataContract]
    public class MapManager : INotifyPropertyChanged
    {
        public XMPPClient XMPPClient = null;

        #region Keep

        #region Constructor
        public MapManager()
        {

        }
        #endregion

        #region Properties

        public static bool ShowAll = true;

        public static bool SingleRosterItemMap = true;

        public static bool SeparateMapForEachRosterItem = true;

        private MapOptions m_MapOptions = new MapOptions();
        [DataMember]
        public MapOptions MapOptions
        {
            get { return m_MapOptions; }
            set { m_MapOptions = value; }
        }
       
        private DynamicMapAttributes m_DynamicMapAttributes = new DynamicMapAttributes();
        [DataMember]
        public DynamicMapAttributes DynamicMapProperties
        {
            get { return m_DynamicMapAttributes; }
            set { m_DynamicMapAttributes = value; }
        }

        #endregion

        #region Initialization
        public void RegisterXMPPClient(XMPPClient client)
        {
            XMPPClient = client;
            XMPPClient.OnRetrievedRoster += new EventHandler(XMPPClient_OnRetrievedRoster);
            XMPPClient.OnRosterItemsChanged += new EventHandler(XMPPClient_OnRosterItemsChanged);
            //Initialize();
        }

        public void Start()
        {
           
            // need to bind the dictionary now.
            if (AddUserToMap())
            {

            }
            bInitialized = true;
        }

        bool bInitialized = false;

        public bool AddUserToMap()
        {
            if (MapOptions.ShowUserLocation == false)
                return false;

            if (SetMyMapRosterItem())
            {
                FireAddMarker(MyMapRosterItem);
                //OnAddMarker(MyMapRosterItem);
            }

            
            return true;
        }

        #endregion

        #region Events and Delegates

        #region Event: Map Initialization Complete
        public delegate void DelegateMapInitializationComplete();
        public event DelegateMapInitializationComplete OnMapInitializationComplete = null;
        internal void FireMapInitializationComplete()
        {
            if (OnMapInitializationComplete != null)
                OnMapInitializationComplete();
        }
        #endregion

        #region Event: Add Marker
        public delegate void DelegateAddMarker(MapRosterItem mapRosterItem);
        public event DelegateAddMarker OnAddMarker = null;
        internal void FireAddMarker(MapRosterItem mapRosterItem)
        {
            if (SeparateMapForEachRosterItem && MainRosterItem.JID == null)
                return;

            if (SeparateMapForEachRosterItem && mapRosterItem.RosterItem.JID.BareJID != MainRosterItem.JID.BareJID)
                return;

            if (OnAddMarker != null)
                OnAddMarker(mapRosterItem);
        }
        #endregion

        //#region Event: Reload Browser
        //public delegate void ReloadBrowserEvent(object sender, BrowserArgs e);
        //public event ReloadBrowserEvent ReloadBrowser;
        //#endregion

        #region Invoke Javascript Function
        public delegate void DelegateInvokeFunction(Function function);
        public event DelegateInvokeFunction OnInvokeFunction = null;
        internal void FireInvokeFunction(Function function)
        {
            if (OnInvokeFunction != null)
                OnInvokeFunction(function);
        }
        #endregion

        #region  Load KML
        public delegate void DelegateLoadKML(MyKML myKML);
        public event DelegateLoadKML OnLoadKML = null;
        internal void FireLoadKML(MyKML myKML)
        {
            if (OnLoadKML != null)
                OnLoadKML(myKML);
        }       
        #endregion

        #endregion

        #region Methods

        void UpdateDictionaries()
        {
            if (XMPPClient == null)
                return;

            // will the RosterItem in its wrapper BuddyPosition naturally change?
            foreach (RosterItem rosterItem in XMPPClient.RosterItems)
            {
                BuddyPosition buddyPosition = null;
                MapRosterItem mapRosterItem = null;

                if (BuddyPositionDictionary.ContainsKey(rosterItem.JID.BareJID) == false)
                {
                    buddyPosition = new BuddyPosition(rosterItem);
                    rosterItem.PropertyChanged += new PropertyChangedEventHandler(rosterItem_PropertyChanged);
                    buddyPosition.PropertyChanged += new PropertyChangedEventHandler(buddyPosition_PropertyChanged);
                    BuddyPositionDictionary.Add(rosterItem.JID.BareJID, buddyPosition);
                }
                else
                {
                    // Do I update the rosterItem? Or does it update itself when it changes?
                    BuddyPositionDictionary[rosterItem.JID.BareJID].RosterItem = rosterItem;
                }

                if (MapRosterItemDictionary.ContainsKey(rosterItem.JID.BareJID) == false)
                {
                    mapRosterItem = new MapRosterItem(rosterItem);
                    rosterItem.PropertyChanged += new PropertyChangedEventHandler(rosterItem_PropertyChanged);
                    mapRosterItem.PropertyChanged += new PropertyChangedEventHandler(mapRosterItem_PropertyChanged);
                    MapRosterItemDictionary.Add(rosterItem.JID.BareJID, mapRosterItem);
                }
                else
                {
                    // Do I update the rosterItem? Or does it update itself when it changes?
                    MapRosterItemDictionary[rosterItem.JID.BareJID].RosterItem = rosterItem;
                }

                MapRosterItem tempMRI = AddRosterItem(rosterItem, true, false, true);
            }
        }
       
        #endregion

        #endregion

        #region Decide
        //public MapManager(XMPPClient _XMPPClient) // , RosterItem _mainRosterItem)
        //{
        //    XMPPClient = _XMPPClient;
        //    // MainRosterItem = _mainRosterItem;

        //    // Create MapRosterItem (or BuddyPosition) for each RosterItem)
        //    //  UpdateDictionaries();

           

        //}

        // key is the BareJID
        Dictionary<string, BuddyPosition> m_BuddyPositionDictionary = new Dictionary<string, BuddyPosition>();

        public Dictionary<string, BuddyPosition> BuddyPositionDictionary
        {
            get { return m_BuddyPositionDictionary; }
            set
            {
                if (m_BuddyPositionDictionary != value)
                {
                    m_BuddyPositionDictionary = value;
                    FirePropertyChanged("BuddyPositionDictionary");
                }
            }
        }

        // key is the BareJID
        Dictionary<string, MapRosterItem> m_MapRosterItemDictionary = new Dictionary<string, MapRosterItem>();

        public Dictionary<string, MapRosterItem> MapRosterItemDictionary
        {
            get { return m_MapRosterItemDictionary; }
            set
            {
                if (m_MapRosterItemDictionary != value)
                {
                    m_MapRosterItemDictionary = value;
                    FirePropertyChanged("MapRosterItemDictionary");
                }
            }
        }


        void mapRosterItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            FireAddMarker(sender as MapRosterItem);
             //OnAddMarker(sender as MapRosterItem);
            // throw new NotImplementedException();
        }

        void buddyPosition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            FireAddMarker(sender as MapRosterItem);
            //OnAddMarker(sender as MapRosterItem);
            // throw new NotImplementedException();
        }

        void XMPPClient_OnRosterItemsChanged(object sender, EventArgs e)
        {
            // This may take care of hitting everything
            if (bInitialized)
            UpdateDictionaries();
            // throw new NotImplementedException();
        }

        void XMPPClient_OnRetrievedRoster(object sender, EventArgs e)
        {
            if (bInitialized)
            UpdateDictionaries();
            // throw new NotImplementedException();
        }

        void rosterItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RosterItem item = sender as RosterItem;

            switch (e.PropertyName)
            {
                case "GeoLoc":
                case "Avatar":
                case "AvatarPath":
                case "Name":
                case "BareJID":
                    {
                        MapRosterItem mri = null;
                        if (DictMapRosterItems.ContainsKey(item.JID.BareJID))
                        {
                            mri = DictMapRosterItems[item.JID.BareJID];
                        }
                        else
                        {
                            // Add
                            mri = AddRosterItem(item, true, false, true);
                        }
                        FireAddMarker(mri);
                       //OnAddMarker(mri);
                    }
                    break;
                default:
                    break;
            }
        }

     

        private ObservableCollection<MapRosterItem> m_listMapRosterItems = new ObservableCollection<MapRosterItem>();

        public ObservableCollection<MapRosterItem> ListMapRosterItems
        {
            get { return m_listMapRosterItems; }
            set
            {
                if (m_listMapRosterItems != value)
                {
                    m_listMapRosterItems = value;
                    FirePropertyChanged("ListMapRosterItems");
                }
            }
        }

        private MapRosterItem m_MyMapRosterItem = new MapRosterItem();

        public MapRosterItem MyMapRosterItem
        {
            get { return m_MyMapRosterItem; }
            set
            {
                if (m_MyMapRosterItem != value)
                {
                    m_MyMapRosterItem = value;
                    FirePropertyChanged("MyMapRosterItem");
                }
            }
        }

        private RosterItem m_MainRosterItem = new RosterItem();

        public RosterItem MainRosterItem
        {
            get { return m_MainRosterItem; }
            set
            {
                if (m_MainRosterItem != value)
                {
                    // create MapRosterItem for me.
                    m_MainRosterItem = value;
                    FirePropertyChanged("MainRosterItem");
                }
            }
        }


        private MarkerInfoWindowStyleType m_DefaultMarkerInfoWindowStyleType = MarkerInfoWindowStyleType.InfoWindowWithAvatarAndText;

        public MarkerInfoWindowStyleType DefaultMarkerInfoWindowStyleType
        {
            get { return m_DefaultMarkerInfoWindowStyleType; }
            set
            {
                if (m_DefaultMarkerInfoWindowStyleType != value)
                {
                    m_DefaultMarkerInfoWindowStyleType = value;
                    FirePropertyChanged("DefaultMarkerInfoWindowStyleType");
                }
            }
        }

        private Dictionary<string, MapRosterItem> m_dictMapRosterItems = new Dictionary<string, MapRosterItem>();

        // use BareJID as key
        public Dictionary<string, MapRosterItem> DictMapRosterItems
        {
            get { return m_dictMapRosterItems; }
            set
            {
                if (m_dictMapRosterItems != value)
                {
                    m_dictMapRosterItems = value;
                    FirePropertyChanged("DictMapRosterItems");
                }
            }
        }

        //private ObservableCollection<MapRosterItem> m_MapRosterItems = new ObservableCollection<MapRosterItem>();

        //public ObservableCollection<MapRosterItem> MapRosterItems
        //{
        //    get { return m_MapRosterItems; }
        //    set
        //    {
        //        if (m_MapRosterItems != value)
        //        {
        //            m_MapRosterItems = value;
        //            FirePropertyChanged("MapRosterItems");
        //        }
        //    }
        //}


        public bool SetMyMapRosterItem() // XMPPAccount xmppAccount)
        {
            MyMapRosterItem = new MapRosterItem()
            {
                DateTimeEnqueued = DateTime.Now,
                zIndex = 0,
                IsDisplayedInViewableWindow = true,
                IsDisplayed = true,
                IsTheMainRosterItem = true,
                RosterItem = new RosterItem() { JID = XMPPClient.XMPPAccount.JID, GeoLoc = XMPPClient.GeoLocation, Tune = XMPPClient.Tune },
                IsMe = true,
                KMLBuilderForRosterItem = new KMLBuilderForRosterItem(),
                MarkerInfoWindowStyleType = this.DefaultMarkerInfoWindowStyleType
            };
           


            return true;
        }

        string SaveImageToFile(RosterItem rosterItem)
        {
            string strFileName = "";

            ImageSource avatarImage = rosterItem.Avatar;
            if (avatarImage != null)
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                //Guid photoID = System.Guid.NewGuid();


                string strPath = string.Format("{0}\\WPFXMPPClient\\Avatars", Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));
                if (Directory.Exists(strPath) == false)
                    Directory.CreateDirectory(strPath);

                //if (File.Exists(strFileName) == false)
                //    return;

                //System.IO.FileStream stream = null;
                //try
                //{
                //    stream = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                //    Option.Options = ser.ReadObject(stream) as Option;
                //}
                //catch (Exception)
                //{
                //}
                //finally
                //{
                //    if (stream != null)
                //    {
                //        stream.Close();
                //        stream.Dispose();
                //    }
                //}

                string strFileNameWithoutExtension = string.Format("{0}\\{1}", strPath, rosterItem.JID.BareJID.ToString());

                strFileName = string.Format("{0}.jpg", strFileNameWithoutExtension);

                //strFileName = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + "Avatars" + "\\" +
                //    rosterItem.JID.BareJID.ToString() + ".jpg";  //file name 

                try
                {
                    encoder.Frames.Add(BitmapFrame.Create((BitmapImage)avatarImage));
                }
                catch (Exception ex)
                {
                }
                bool bFileExists = false;

                // backup existing avatar
                if (System.IO.File.Exists(strFileName))
                {
                    bFileExists = true;
                    string strBackupFileName = String.Format("{0}.{1}.jpg", strFileNameWithoutExtension, String.Format("{0:yyyyMMddHHmmss}", DateTime.Now));
                    // strFileName + "." + String.Format("{0:s}", DateTime.Now).Replace(":", "");
                    System.IO.File.Copy(strFileName, strBackupFileName, true);
                    if (System.IO.File.Exists(strBackupFileName))
                    {
                        System.IO.File.Delete(strFileName);
                        bFileExists = false;
                    }
                }
                //hopefully this works.
                if (!bFileExists)
                {
                    try
                    {
                        using (var filestream = new FileStream(strFileName, FileMode.Create))
                            encoder.Save(filestream);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return strFileName;
        }

        //public bool AdjustMapWindowRosterItemList(RosterItem item)
        //{

        //    return true;
        //}

        public bool AdjustMapWindowRosterItemList(RosterItem item)
        {
            MapRosterItem newMapRosterItem = null;

            if (DictMapRosterItems.ContainsKey(item.JID.BareJID) == true)
            {
                DictMapRosterItems[item.JID.BareJID].IsDisplayedInViewableWindow = true;
                DictMapRosterItems[item.JID.BareJID].IsTheMainRosterItem = true;
                DictMapRosterItems[item.JID.BareJID].DateTimeEnqueued = DateTime.Now;
                DictMapRosterItems[item.JID.BareJID].zIndex = 0;
                newMapRosterItem = DictMapRosterItems[item.JID.BareJID];



            }
            else
            {
                newMapRosterItem = new LocationClasses.MapRosterItem()
                {
                    RosterItem = item,
                    IsTheMainRosterItem = true,
                    IsDisplayedInViewableWindow = true,
                    DateTimeEnqueued = DateTime.Now,
                    zIndex = 0,
                    MarkerInfoWindowStyleType = this.DefaultMarkerInfoWindowStyleType
                };

                string strLocalAvatarPath = SaveImageToFile(item);
                //System.Diagnostics.Debug.WriteLine("Saved image for " + item.JID.ToString() + " to file " + strLocalAvatarPath);

                if (strLocalAvatarPath != null && strLocalAvatarPath != "")
                {
                    newMapRosterItem.LocalAvatarPath = strLocalAvatarPath;
                }

                // now we need to save the thumbmail (google latitude-looking image) to file too.


                DictMapRosterItems.Add(item.JID.BareJID, newMapRosterItem);


                //JavaScriptSerializer ser = new JavaScriptSerializer();
                //string strRosterItem = ser.Serialize(item);
                //System.Console.WriteLine("serialized roster item: " + strRosterItem);

            }
            if (newMapRosterItem != null)
            {

                JavaScriptSerializer ser = new JavaScriptSerializer();
                //string strRosterItem = ser.Serialize(MainRosterItem);
                //System.Console.WriteLine("serialized roster item: " + strRosterItem);
                //DictMapRosterItems[item.JID.BareJID].strRosterItemJSONSerialized = strRosterItem;


                //string strGeoLoc = ser.Serialize(MainRosterItem.GeoLoc);
                //System.Console.WriteLine("serialized geoloc: " + strGeoLoc);
                newMapRosterItem.JSONList.Add(new JSONSerializedObject()
                {
                    strName = "GeoLoc",
                    strJSONSerializedString = ser.Serialize(MainRosterItem.GeoLoc)
                });

                newMapRosterItem.JSONList.Add(new JSONSerializedObject()
                {
                    strName = "JID",
                    strJSONSerializedString = ser.Serialize(newMapRosterItem.RosterItem.JID)
                });

                //newMapRosterItem.JSONList.Add(new JSONSerializedObject()
                //{
                //    strName = "Presence",
                //    strJSONSerializedString = ser.Serialize(newMapRosterItem.RosterItem.Presence)
                //});



            }

            MainRosterItem = item;



            foreach (var kvp in DictMapRosterItems)
            {
                if (kvp.Key != item.JID.BareJID)
                {
                    kvp.Value.IsTheMainRosterItem = false;
                    kvp.Value.IsDisplayedInViewableWindow = false; // maybe not though.... 
                    // bump down the display priority, this could get too big, but probably not, and should work. 
                    kvp.Value.zIndex++; // as long as the new guy is 0! everyone was 0 when they were enqueued, and when a new guy comes along, they are ++'d.
                    // now if i get picked again, i get reset to 0, but everyone is still bumped up, and the priorities they hold should still work!
                }
            }

            // populate list from values
            ListMapRosterItems = new ObservableCollection<MapRosterItem>(DictMapRosterItems.Values.ToList());

            // new ObservableCollectionEx<MapRosterItem
            //           DictMapRosterItems.Values.ToList();


            // sort list based on displaypriority

            return true;
        }

        private void MergeImages(System.Drawing.Image frame)
        {
            //using (frame)
            //{
            //    using (var bitmap = new Bitmap(width, height))
            //    {
            //        using (var canvas = Graphics.FromImage(bitmap))
            //        {
            //                canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //                canvas.DrawImage(Frame, new Rectangle(0, 0, width, height), new Rectangle(0, 0, Frame.Width, Frame.Height), GraphicsUnit.Pixel);
            //                canvas.DrawImage(Playbutton, (bitmap.Width / 2) - (playbutton_width / 2 + 5), (bitmap.Height / 2) - (playbutton_height / 2 + 5));
            //                canvas.Save();
            //        }
            //        try
            //        {
            //                bitmap.Save(/*somekindofpath*/, ImageFormat.Jpeg);
            //        }
            //        catch (Exception ex) { }
            //    }
            //            }
        }

        public DrawingImage CreateThumbnail(string strPath1, string strPath2)
        {
            DrawingImage myImage = new DrawingImage();

            ImageDrawing Drawing1 = new ImageDrawing(new BitmapImage(new Uri(strPath1,
                                                                     UriKind.Absolute)),
                                                             new System.Windows.Rect(0, 0, 40, 130));

            ImageDrawing Drawing2 = new ImageDrawing(new BitmapImage(new Uri(strPath2,
                                                                             UriKind.Absolute)),
                                                                     new System.Windows.Rect(40, 0, 45, 130));

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(@"c:\overlay.bmp");

            bitmap.MakeTransparent();

            ImageDrawing Drawing3 = new ImageDrawing(System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                                                                                           IntPtr.Zero,
                                                                                           System.Windows.Int32Rect.Empty,
                                                                                           BitmapSizeOptions.FromEmptyOptions()),
                                                     new System.Windows.Rect(40, 0, 45, 130));

            DrawingGroup myDrawingGroup = new DrawingGroup();
            myDrawingGroup.Children.Add(Drawing1);
            myDrawingGroup.Children.Add(Drawing2);
            myDrawingGroup.Children.Add(Drawing3);

            DrawingImage source = new DrawingImage(myDrawingGroup);
            return source;

            //           System.Windows.Media.Imaging.BitmapSource OurImage = null;
            //           System.Windows.Media.ImageSource Avatar = null;



            //                if (OurImage == null)
            //                {
            //                    Uri uri = null;
            //#if WINDOWS_PHONE
            //                    uri = new Uri("[Application Name];component/Avatars/avatar.png", UriKind.Relative);
            //#else
            //                    uri = new Uri("Avatars/avatar.png", UriKind.Relative);
            //#endif
            //                    OurImage = new System.Windows.Media.Imaging.BitmapImage(uri);
            //                }


            //                return OurImage;



            //           ImageSource avatarImage = rosterItem.Avatar
        }

        public bool MoveMarker(MapRosterItem mapRosterItem)
        {
            if (mapRosterItem == null)
            {
                return false;
            }
            geoloc previousLocation = mapRosterItem.CurrentLocation;
            if (mapRosterItem.CurrentLocation == mapRosterItem.RosterItem.GeoLoc)
            {
                return false;
                // false alarm - they really didn't move!
            }
            else
            {
                // did they move off the viewable window (and is their IsDisplayedOnViewableWindow set to true?)
                // could pull some stats, but could do that in MapManager too!jav
                // this is where would enqueue the location into the coordinates list
                // and calculate current mph, based on this location and previous location
                // and even average mph  (but how far back to go - might be route-related)

            }
            return true;
        }

        public MapRosterItem CenterOnMe(MapRosterItem mapRosterItem)
        {
            MapRosterItem newOrUpdatedItem = AddRosterItem(mapRosterItem.RosterItem, true, true, true);
            return newOrUpdatedItem;
        }

        //public bool DisplayOnMap(MapRosterItem mapRosterItem)
        //{
        //    if (DictMapRosterItems.ContainsKey(mapRosterItem.RosterItem.JID.BareJID) == true)
        //    {
        //        DictMapRosterItems[mapRosterItem.RosterItem.JID.BareJID].IsDisplayed = true;
        //    }
        //    return true;
        //}

        public bool DisplayInViewableWindow(MapRosterItem mapRosterItem)
        {
            if (DictMapRosterItems.ContainsKey(mapRosterItem.RosterItem.JID.BareJID) == true)
            {
                DictMapRosterItems[mapRosterItem.RosterItem.JID.BareJID].IsDisplayedInViewableWindow = true;
            }
            return true;
        }

        public bool DisplayOnMap(MapRosterItem mapRosterItem)
        {
            if (DictMapRosterItems.ContainsKey(mapRosterItem.RosterItem.JID.BareJID) == true)
            {
                DictMapRosterItems[mapRosterItem.RosterItem.JID.BareJID].IsDisplayed = true;
            }
            else
            {
                DictMapRosterItems.Add(mapRosterItem.RosterItem.JID.BareJID, new LocationClasses.MapRosterItem()
                {
                    RosterItem = mapRosterItem.RosterItem,
                    //IsTheMainRosterItem = bIsTheMainRosterItem,
                    //IsDisplayedInViewableWindow = bIsDisplayedInViewableWindow,
                    DateTimeEnqueued = DateTime.Now,
                    zIndex = 0,
                    IsDisplayed = true,
                    MarkerInfoWindowStyleType = this.DefaultMarkerInfoWindowStyleType
                    // MarkerInfoWindowStyleType.InfoWindowWithTextOnly
                });
            }

            return true;
        }

        public MapRosterItem AddRosterItem(RosterItem item, bool bIsDisplayedInViewableWindow, bool bIsTheMainRosterItem, bool bIsDisplayed)
        {
            MapRosterItem newMapRosterItem = null;
            if (SingleRosterItemMap)
            {
               

            }

            if (ItemExists(item) == true)
            {
                DictMapRosterItems[item.JID.BareJID].IsDisplayedInViewableWindow = bIsDisplayedInViewableWindow;
                DictMapRosterItems[item.JID.BareJID].IsDisplayed = bIsDisplayed;
                DictMapRosterItems[item.JID.BareJID].IsTheMainRosterItem = bIsTheMainRosterItem;
                DictMapRosterItems[item.JID.BareJID].DateTimeEnqueued = DateTime.Now;
                DictMapRosterItems[item.JID.BareJID].zIndex = 0;
                newMapRosterItem = DictMapRosterItems[item.JID.BareJID];
            }
            else
            {
                newMapRosterItem = new LocationClasses.MapRosterItem()
                {
                    RosterItem = item,
                    IsTheMainRosterItem = bIsTheMainRosterItem,
                    IsDisplayedInViewableWindow = bIsDisplayedInViewableWindow,
                    DateTimeEnqueued = DateTime.Now,
                    zIndex = 0,
                    IsDisplayed = bIsDisplayed,
                    MarkerInfoWindowStyleType = this.DefaultMarkerInfoWindowStyleType,
                    // MarkerInfoWindowStyleType.InfoWindowWithTextOnly
                };

                string strLocalAvatarPath = SaveImageToFile(item);
                System.Diagnostics.Debug.WriteLine("Saved image for " + item.JID.ToString() + " to file " + strLocalAvatarPath);
                System.Console.WriteLine("Saved image for " + item.JID.ToString() + " to file " + strLocalAvatarPath);

                if (strLocalAvatarPath != null && strLocalAvatarPath != "")
                {
                    newMapRosterItem.LocalAvatarPath = strLocalAvatarPath;
                }

                DictMapRosterItems.Add(item.JID.BareJID, newMapRosterItem);
            }

            if (SingleRosterItemMap)
            {
                if (SetAsMainRosterItem(newMapRosterItem))
                {

                }
            }

            // populate list from values
            ListMapRosterItems = new ObservableCollection<MapRosterItem>(DictMapRosterItems.Values.ToList());

            // new ObservableCollectionEx<MapRosterItem
            //           DictMapRosterItems.Values.ToList();


            // sort list based on displaypriority

            return newMapRosterItem;
        }

        public bool ItemExists(RosterItem item)
        {
            return DictMapRosterItems.ContainsKey(item.JID.BareJID);
        }

        public bool SetAsMainRosterItem(MapRosterItem mapRosterItem)
        {
            MainRosterItem = mapRosterItem.RosterItem;
            mapRosterItem.IsTheMainRosterItem = true;
            mapRosterItem.IsDisplayed = true;
            mapRosterItem.IsDisplayedInViewableWindow = true;
            mapRosterItem.zIndex = 0;

            // now set everyone else to NOT be the main guy!
            foreach (var kvp in DictMapRosterItems)
            {
                if (kvp.Key != mapRosterItem.RosterItem.JID.BareJID)
                {
                    kvp.Value.IsTheMainRosterItem = false;
                    // kvp.Value.IsDisplayedInViewableWindow = false; // maybe not though.... 
                    // bump down the display priority, this could get too big, but probably not, and should work. 
                    kvp.Value.zIndex++; // as long as the new guy is 0! everyone was 0 when they were enqueued, and when a new guy comes along, they are ++'d.
                    // now if i get picked again, i get reset to 0, but everyone is still bumped up, and the priorities they hold should still work!
                }
            }
            return true;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }

        #endregion

        #endregion
    }

}
