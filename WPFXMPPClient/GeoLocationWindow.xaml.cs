using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.Net;
//using System.Drawing;
using System.IO;
using Microsoft.Win32;
using System.Xml;
using System.Xml.Linq;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.XMPP;
using System.Xml.Serialization;
using LocationClasses;
using System.ComponentModel;

using System.Runtime.InteropServices;

namespace WPFXMPPClient
{
    public partial class GeoLocationWindow : Window
    {

        #region WPF Map App Stuff

        #region "Fields"
        private XDocument geoDoc;
        private string location;
        private int zoom;
        private SaveFileDialog saveDialog = new SaveFileDialog();
        private string mapType;
        private double lat;
        private double lng;
        private double centerLat = -1;
        private double centerLng = -1;
        private double previousCenterLat = -1;
        private double previousCenterLng = -1; 
        private string center = "";

        #endregion

        private void GetGeocodeData()
        {
            string geocodeURL = "http://maps.googleapis.com/maps/api/" + "geocode/xml?address=" + location + "&sensor=false";
            try
            {
                geoDoc = XDocument.Load(geocodeURL);
            }
            catch (WebException ex)
            {
                this.Dispatcher.BeginInvoke(new ThreadStart(HideProgressBar), DispatcherPriority.Normal, null);
                MessageBox.Show("Ensure that internet connection is available.", "Map App", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.Dispatcher.BeginInvoke(new ThreadStart(ShowGeocodeData), DispatcherPriority.Normal, null);
        }

        // GeoCode Data Example
        // <put here>
        private void ShowGeocodeData()
        {
            // XElement xmlElement = geoDoc.Root;

            // dynamic geocodeResponse = geoDoc.
            // Root;
            XElement geocodeResponseXElement = geoDoc.Element("GeocodeResponse");
            if (geocodeResponseXElement != null)
            {
                dynamic geoResponseStatus = geocodeResponseXElement.Element("status");
                if (geoResponseStatus != null)
                {
                    string strStatus = geoResponseStatus.Value;
                }
            }

            dynamic geocodeResponse = geoDoc.Element("GeocodeResponse");
            //.Element("GeneralInformation");



            // Element("GeocodeResponse").Value;

            dynamic responseStatus = geocodeResponse.Element("status").Value;

            // Single.Value();
            if ((responseStatus == "OK"))
            {
                dynamic geoResult = geocodeResponse.Element("result");

                dynamic formattedAddress = geoResult.Element("formatted_address").Value;
                // (0).Value();
                dynamic geometry = geoResult.Element("geometry");
                dynamic location = geometry.Element("location");


                dynamic latitude = location.Element("lat").Value;
                // (0).Element("lat").Value();
                dynamic longitude = location.Element("lng").Value;
                // (0).Element("lng").Value();

                string locationType = geometry.Element("location_type").Value;
                // (0).Value();

                //AddressTxtBlck.Text = formattedAddress;
                //LatitudeTxtBlck.Text = latitude;
                //LatitudeTxtBlck.Text = longitude;

                switch (locationType)
                {
                    case "APPROXIMATE":
                        //AccuracyTxtBlck.Text = "Approximate";
                        break;
                    case "ROOFTOP":
                        //AccuracyTxtBlck.Text = "Precise";
                        break;
                    default:
                        //AccuracyTxtBlck.Text = "Approximate";
                        break;
                }

                lat = double.Parse(latitude);
                lng = double.Parse(longitude);

                if ((SaveButton.IsEnabled == false))
                {
                    SaveButton.IsEnabled = true;
                    RoadmapToggleButton.IsEnabled = true;
                    TerrainToggleButton.IsEnabled = true;
                }

            }
            else if ((responseStatus == "ZERO_RESULTS"))
            {
                MessageBox.Show("Unable to show results for: " + "\r\n" + location,
                    // Constants.vbCrLf + location, 
                    "Unknown Location", MessageBoxButton.OK, MessageBoxImage.Information);
                DisplayXXXXXXs();
                AddressTxtBox.SelectAll();
            }
            ////ShowMapButton.IsEnabled = true;
            ZoomInButton.IsEnabled = true;
            ZoomOutButton.IsEnabled = true;
            //MapProgressBar.Visibility = System.Windows.Visibility.Hidden;
        }

        // Get and display map image in Image ctrl.
        private void ShowMapImage()
        {
            BitmapImage bmpImage = new BitmapImage();

            string mapURL = "http://maps.googleapis.com/maps/api/staticmap?" + "size=500x400&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";

            bmpImage.BeginInit();
            bmpImage.UriSource = new Uri(mapURL);
            bmpImage.EndInit();

            MapImage.Source = bmpImage;
        }


        private void ShowMapUsingLatLng()
        {
            if (centerLat == -1)
                centerLat = lat;
            if (centerLng == -1)
                centerLng = lng;

            BitmapImage bmpImage = new BitmapImage();
            string mapURL = "http://maps.googleapis.com/maps/api/staticmap?" + "center=" + String.Format("{0},{1}", centerLat, centerLng) + "&" + "size=500x400&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";
            bmpImage.BeginInit();
            bmpImage.UriSource = new Uri(mapURL);
            bmpImage.EndInit();

            MapImage.Source = bmpImage;
        }

        private MapProperties BuildMapProperties()
        {
            MapProperties tmpMapProperties = new MapProperties();
            tmpMapProperties.LocationParameters = new LocationParameters();
            tmpMapProperties.LocationParameters.Center = LocationParameters.SetCenterFromLatLon(lat, lng);
            tmpMapProperties.LocationParameters.Zoom = zoom;

            tmpMapProperties.MapParameters = new MapParameters();
            tmpMapProperties.MapParameters.MapFormat = MapFormat.png;
            tmpMapProperties.MapParameters.MapType = MapType.roadmap;
            tmpMapProperties.MapParameters.Scale = 800;
            tmpMapProperties.Sensor = false;

            tmpMapProperties.MarkerCollection = new MarkerCollection();

            foreach (BuddyPosition buddy in BuddyPositions)
            {
                if (buddy.bShowOnMap)
                {
                    MarkerLocation mLoc = new MarkerLocation();
                    mLoc.Location = MarkerLocation.GetLocationFromLatLon(buddy.RosterItem.GeoLoc.lat,
                        buddy.RosterItem.GeoLoc.lon);
                    mLoc.MarkerStyle = new MarkerStyle();
                    mLoc.MarkerStyle.Label = buddy.RosterItem.JID.BareJID.Substring(0, 1);
                    tmpMapProperties.MarkerCollection.MarkerLocations.Add(mLoc);
                }
            }

            return tmpMapProperties;
        }

        //private string BuildMarkerForRosterItem(RosterItem rosterItem, string strColor, string strLabel)
        //{
        //    string strLatLon = String.Format("{0},{1}", rosterItem.GeoLoc.lat, rosterItem.GeoLoc.lon);
        //    return String.Format("&markers=color:{0}%7Clabel:{1}%7C{2}", strColor, strLabel, strLatLon);

        //}

        MapProperties MapProperties = new MapProperties();

        private void ShowMapUsingLatLngForAllCheckedBuddies()
        {
            MapProperties = BuildMapProperties();

            BitmapImage bmpImage = new BitmapImage();
            string mapURL = MapProperties.URL;

            // "http://maps.googleapis.com/maps/api/staticmap?" + "center=" + lat + "," + lng + "&" + "size=500x400&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";
            bmpImage.BeginInit();
            bmpImage.UriSource = new Uri(mapURL);
            bmpImage.EndInit();

            MapImage.Source = bmpImage;
        }

        // Zoom-in on map.
        private void ZoomIn()
        {
            if ((zoom < 21))
            {
                zoom += 1;
                ShowMapUsingLatLng();

                if ((ZoomOutButton.IsEnabled == false))
                {
                    ZoomOutButton.IsEnabled = true;
                }
            }
            else
            {
                ZoomInButton.IsEnabled = false;
            }

            if (zoom < 21)
                ZoomInButton.IsEnabled = true;
            else
                ZoomInButton.IsEnabled = false;
        }

        // Zoom-out on map.
        private void ZoomOut()
        {
            if ((zoom > 0))
            {
                zoom -= 1;
                ShowMapUsingLatLng();

                if ((ZoomInButton.IsEnabled == false))
                {
                    ZoomInButton.IsEnabled = true;
                }
            }
            else
            {
                ZoomOutButton.IsEnabled = false;
            }


            if (zoom > 0)
                ZoomOutButton.IsEnabled = true;
            else
                ZoomOutButton.IsEnabled = false;
        }

        private void SaveMap()
        {
            string mapURL = "http://maps.googleapis.com/maps/api/staticmap?" + "center=" + lat + "," + lng + "&" + "size=500x400&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";
            WebClient webClient = new WebClient();
            try
            {
                byte[] imageBytes = webClient.DownloadData(mapURL);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    System.Drawing.Image.FromStream(ms).Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show("Unable to save map. Ensure that you are" + " connected to the internet.", "Error!", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
        }

        private void MoveUp()
        {
            // Default zoom is 15 and at this level changing
            // the center point is done by 0.003 degrees. 
            // Shifting the center point is done by higher values
            // at zoom levels less than 15.
            double diff = 0;
            double shift = 0;
            if (centerLat == -1)
                centerLat = lat;
            if (centerLng == -1)
                centerLng = lng;
            double saveLat = centerLat;
           

         

            // Use 88 to avoid values beyond 90 degrees of lat.
            if ((centerLat < 88))
            {
                if ((zoom == 15))
                {

                    previousCenterLat = centerLat;
                    centerLat += 0.003;
                }
                else if ((zoom > 15))
                {
                    diff = zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;

                    previousCenterLat = centerLat;
                    centerLat += shift;
                }
                else
                {
                    diff = 15 - zoom;
                    shift = ((15 + diff) * 0.003) / 15;

                    previousCenterLat = centerLat;
                    centerLat += shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {

                previousCenterLat = centerLat;
                centerLat = 90;
            }
        }

        private void MoveDown()
        {
            if (centerLat == -1)
                centerLat = lat;
            if (centerLng == -1)
                centerLng = lng;

            double saveLat = centerLat;
           

            double diff = 0;
            double shift = 0;
            if ((centerLat > -88))
            {
                if ((zoom == 15))
                {
                    previousCenterLat = centerLat;
                    centerLat -= 0.003;
                }
                else if ((zoom > 15))
                {
                    diff = zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;

                    previousCenterLat = centerLat;
                    centerLat -= shift;
                }
                else
                {
                    diff = 15 - zoom;
                    shift = ((15 + diff) * 0.003) / 15;

                    previousCenterLat = centerLat;
                    centerLat -= shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                previousCenterLat = centerLat;
                centerLat = -90;
            }
        }

        private void MoveLeft()
        {
            double saveLng = centerLng;

            double diff = 0;
            double shift = 0;
            // Use -178 to avoid negative values below -180.
            if ((centerLng > -178))
            {
                if ((zoom == 15))
                {
                    previousCenterLng = centerLng;
                    centerLng -= 0.003;
                }
                else if ((zoom > 15))
                {
                    diff = zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    previousCenterLng = centerLng;
                    centerLng -= shift;
                }
                else
                {
                    diff = 15 - zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    previousCenterLng = centerLng;
                    centerLng -= shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                previousCenterLng = centerLng;
                centerLng = 180;
            }
        }

        private void MoveRight()
        {
            double diff = 0;
            double shift = 0;
            if ((centerLng < 178))
            {
                if ((zoom == 15))
                {
                    previousCenterLng = centerLng;
                    centerLng += 0.003;
                }
                else if ((zoom > 15))
                {
                    diff = zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    previousCenterLng = centerLng;
                    centerLng += shift;
                }
                else
                {
                    diff = 15 - zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    previousCenterLng = centerLng;
                    centerLng += shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                previousCenterLng = centerLng;
                centerLng = -180;
            }
        }

        private void DisplayXXXXXXs()
        {
            //AddressTxtBlck.Text = "XXXXXXXXX, XXXXX, XXXXXX";
            //LatitudeTxtBlck.Text = "XXXXXXXXXX";
            //LatitudeTxtBlck.Text = "XXXXXXXXXX";
            //AccuracyTxtBlck.Text = "XXXXXXXXX";
        }

        private void HideProgressBar()
        {
            //MapProgressBar.Visibility = System.Windows.Visibility.Hidden;
            ////ShowMapButton.IsEnabled = true;
        }

        // ////ShowMapButton click event handler.
        private void ShowMapButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (OurRosterItem == null)
                return;

            ////ShowMapButton.IsEnabled = false;
            //MapProgressBar.Visibility = System.Windows.Visibility.Visible;
            AddressTxtBox.Text = String.Format("{0},{1}", OurRosterItem.GeoLoc.lat, OurRosterItem.GeoLoc.lon);
           TextBoxLat.Text = String.Format("{0}", OurRosterItem.GeoLoc.lat);
           TextBoxLon.Text = String.Format("{0}", OurRosterItem.GeoLoc.lon);

            if ((AddressTxtBox.Text != string.Empty))
            {
                location = AddressTxtBox.Text.Replace(" ", "+");
                zoom = 15;
                mapType = "roadmap";

                // use a real regex

                string[] parms = location.Split(',');
                if (parms.Length == 2)
                {
                    try
                    {
                        lat = Convert.ToDouble(parms[0]);
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        lng = Convert.ToDouble(parms[1]);
                    }
                    catch (Exception ex)
                    {

                    }

                    TextBlockTitleTimestamp.Text = String.Format("{0}", OurRosterItem.GeoLoc.TimeStamp);
                    TextBlockTitleBuddy.Text = String.Format("{0}", OurRosterItem.JID.ToString());

                    ShowMapImage();
                    AddressTxtBox.SelectAll();
                    ////ShowMapButton.IsEnabled = true;

                    ////AddressTxtBlck.Text = formattedAddress;
                    //LatitudeTxtBlck.Text = lat.ToString();
                    //LatitudeTxtBlck.Text = lng.ToString();
                    //AccuracyTxtBlck.Text = "Approximate";

                    //switch (locationType)
                    //{
                    //    case "APPROXIMATE":
                    //        //AccuracyTxtBlck.Text = "Approximate";
                    //       break;
                    //    case "ROOFTOP":
                    //        //AccuracyTxtBlck.Text = "Precise";
                    //        break;
                    //    default:
                    //        //AccuracyTxtBlck.Text = "Approximate";
                    //        break;
                    //}




                    //MapProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    Thread geoThread = new Thread(GetGeocodeData);
                    geoThread.Start();


                    ShowMapImage();
                    AddressTxtBox.SelectAll();
                    ////ShowMapButton.IsEnabled = false;

                    //MapProgressBar.Visibility = System.Windows.Visibility.Visible;

                }



                if ((RoadmapToggleButton.IsChecked == false))
                {
                    RoadmapToggleButton.IsChecked = true;
                    TerrainToggleButton.IsChecked = false;
                }
            }
            else
            {
                MessageBox.Show("Enter location address.", "Map App", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                AddressTxtBox.Focus();
            }
        }

        // SaveFileDialog FileOk event handler.
        private void saveDialog_FileOk(object sender, EventArgs e)
        {
            Thread td = new Thread(SaveMap);
            td.Start();
        }

        // ZoomInButton click event handler.
        private void ZoomInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ZoomIn();
        }

        // ZoomOutButton click event handler.
        private void ZoomOutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ZoomOut();

        }

        // SaveButton click event handler.
        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            saveDialog.ShowDialog();
        }

        // RoadmapToggleButton Checked event handler.
        private void RoadmapToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((mapType != "roadmap"))
            {
                mapType = "roadmap";
                ShowMapUsingLatLng();
                TerrainToggleButton.IsChecked = false;
            }
        }

        // TerrainToggleButton Checked event handler.
        private void TerrainToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((mapType != "terrain"))
            {
                mapType = "terrain";
                ShowMapUsingLatLng();
                RoadmapToggleButton.IsChecked = false;
            }
        }

        private void MapImage_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if ((location != null))
            //{
            //    string gMapURL = "http://maps.google.com/maps?q=" + location;
            //    Process.Start("IExplore.exe", gMapURL);
            //}
        }

        private void Window1_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AddressTxtBox.Focus();

            var _with1 = saveDialog;
            _with1.DefaultExt = "png";
            _with1.Title = "Save Map Image";
            _with1.OverwritePrompt = true;
            _with1.Filter = "(*.png)|*.png";

            saveDialog.FileOk += saveDialog_FileOk;

            Window_Loaded(sender, e);
        }

        private void MinimizeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        private void BgndRectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MoveUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveUp();
        }

        private void MoveDownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveDown();
        }

        private void MoveLeftButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveLeft();
        }

        private void MoveRightButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MoveRight();
        }

        public GeoLocationWindow()
        {
            InitializeComponent();
            //Loaded += Window1_Loaded;
        }

        private void ShowBuddiesOnMap()
        {
            ShowMapUsingLatLngForAllCheckedBuddies();
        }

        private void buttonShowLatLon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lat = Convert.ToDouble(TextBoxLat.Text);
            }
            catch (Exception ex)
            {
                return;
            }

            try
            {
                lng = Convert.ToDouble(TextBoxLon.Text);
            }
            catch (Exception ex)
            {
                return;
            }

            location = String.Format("{0},{1}", lat, lng);
            //LatitudeTxtBlck.Text = lat.ToString();
            //LatitudeTxtBlck.Text = lng.ToString();

            //AddressTxtBlck.Text = String.Format("{0},{1}", lat, lng);





            ////ShowMapButton.IsEnabled = true;
            ZoomInButton.IsEnabled = true;
            ZoomOutButton.IsEnabled = true;

            TerrainToggleButton.IsEnabled = true;
            RoadmapToggleButton.IsEnabled = true;

            ShowMapUsingLatLng();
        }

        #endregion


        public XMPPClient XMPPClient = new XMPPClient();
        //public RosterItem OurRosterItem = new RosterItem();
        public BuddyPosition MyBuddyPosition = null;
        public RosterItem MyRosterItem = null;

        public void LoadBuddyMapImages()
        {

        }
        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        //{
        //    var percentWidthChange = Math.Abs(sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width) / sizeInfo.PreviousSize.Width;
        //    var percentHeightChange = Math.Abs(sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height) / sizeInfo.PreviousSize.Height;

        //    if (percentWidthChange > percentHeightChange)
        //        this.Height = sizeInfo.NewSize.Width / _aspectRatio;
        //    else
        //        this.Width = sizeInfo.NewSize.Height * _aspectRatio;

        //    base.OnRenderSizeChanged(sizeInfo);
        //}

        //private int _aspectRatio = 10;
        #region Google Earth Friends Stuff
        //EARTHLib.ApplicationGEClass earth = new EARTHLib.ApplicationGEClass();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MapWindow win = new MapWindow();
            win.XMPPClient = XMPPClient;
            win.OurRosterItem = OurRosterItem;
            win.Show();

            RosterItem MyRosterItem = new RosterItem(XMPPClient, XMPPClient.XMPPAccount.JID);
            MyBuddyPosition = new BuddyPosition(MyRosterItem) { bIsMe = true };
            OurRosterItem.PropertyChanged += new PropertyChangedEventHandler(OurRosterItem_PropertyChanged);

            if (OurRosterItem == null)
            {
                MyBuddyPosition.bCenterOnBuddy = true;
                MyBuddyPosition.bShowOnMap = true;
            }
            this.DataContext = XMPPClient;
            //  MapUserControl1.XMPPClient = XMPPClient;
            // MapUserControl1.OurRosterItem = this.OurRosterItem;
            //   this.ListBoxConversation.ItemsSource = XMPPClient.FileTransferManager.FileTransfers;

           AddressTxtBox.Focus();

            var _with1 = saveDialog;
            _with1.DefaultExt = "png";
            _with1.Title = "Save Map Image";
            _with1.OverwritePrompt = true;
            _with1.Filter = "(*.png)|*.png";

            saveDialog.FileOk += saveDialog_FileOk;

            if (OurRosterItem != null)
            {
                this.Title = String.Format("Buddy Map - {0}", OurRosterItem.JID.ToString());
                //this.TextBlockTitle.Text = this.Title;
            }
            /* 
            XMPPClient.AutomaticallyDownloadAvatars = false;
            XMPPClient.AutoAcceptPresenceSubscribe = false;
            XMPPClient.FileTransferManager.AutoDownload = true;
            // XMPPClient.FileTransferManager.OnTransferFinished += new FileTransferManager.DelegateDownloadFinished(FileTransferManager_OnTransferFinished);
            XMPPClient.AutoQueryServerFeatures = false;
            */

            // this will never happen, so do this on startup. 

            // in case it changes it'll be here 
            XMPPClient.OnRetrievedRoster += new EventHandler(client_OnRetrievedRoster);
            /* XMPPClient.OnStateChanged += new EventHandler(client_OnStateChanged); */

            //System.Net.XMPP.LoginWindow login = new LoginWindow();
            //login.ActiveAccount = XMPPClient.XMPPAccount;
            //if (login.ShowDialog() == true)
            //{
            //    XMPPClient.XMPPAccount = login.ActiveAccount;
            //    XMPPClient.PresenceStatus.Priority = -10;
            //    XMPPClient.Connect();
            //}
            //else
            //{
            //    System.Environment.Exit(0);
            //}

            // Make my own BuddyPosition

            client_OnRetrievedRoster(sender, e);

            WriteSampleKML();

        }

        void OurRosterItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ButtonRefresh_Click(sender, null);
            // ////ShowMapButton_Click(sender, null);
            // throw new NotImplementedException();
        }


        void client_OnStateChanged(object sender, EventArgs e)
        {

        }

        ObservableCollectionEx<BuddyPosition> BuddyPositions = new ObservableCollectionEx<BuddyPosition>();

        void client_OnRetrievedRoster(object sender, EventArgs e)
        {
            /// bind to ourlist
            /// 
            // Add myself as my own buddy in the BuddyPositions list
            BuddyPositions.Add(MyBuddyPosition);

            foreach (RosterItem item in XMPPClient.RosterItems)
            {
                BuddyPositions.Add(new BuddyPosition(item));
                item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
            }

         //   this.Dispatcher.Invoke(new Action(() => { this.ListViewBuddies.ItemsSource = BuddyPositions; this.DataContext = this; this.ListViewBuddies.DataContext = BuddyPositions; }));

            // ok to put here, without a dispatcher?
            // Setup property changed handlers for all buddies in list. 
            SetUpRosterItemNotifications();
            ButtonRefresh_Click(sender, null);
        }
        void rosterItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                // WebBrowserMap.Navigate(strURL);
                if (e.PropertyName == "GeoLoc")
                {

                }
            })
                  );
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RosterItem item = sender as RosterItem;
            if (item == OurRosterItem)
            {
                if (e.PropertyName == "GeoLoc")
                {
                    ShowMapButton_Click(sender, null);
                }
            }
            // throw new NotImplementedException();
        }



        private void SetUpRosterItemNotifications()
        {
            if (XMPPClient == null)
                return;

            foreach (BuddyPosition buddyPosition in BuddyPositions)
            {
                buddyPosition.PropertyChanged += new PropertyChangedEventHandler(buddyPosition_PropertyChanged);
            }

            foreach (RosterItem item in XMPPClient.RosterItems)
            {
                item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
            }
        }

        void buddyPosition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ButtonRefresh_Click(sender, null);
            // throw new NotImplementedException();
        }

        void WriteSampleKML()
        {
            MyKML kml = new MyKML();
            kml.Document.Name = "brian";

            List<GeoCoordinate> Coords = new List<GeoCoordinate>();
            Coords.Add(new GeoCoordinate(32.816929, -96.757835, DateTime.Now - TimeSpan.FromMinutes(10)));
            Coords.Add(new GeoCoordinate(32.815437, -96.75758399999999, DateTime.Now - TimeSpan.FromMinutes(5)));
            Coords.Add(new GeoCoordinate(32.817078, -96.757721, DateTime.Now));

            int i = 1;
            foreach (GeoCoordinate coord in Coords)
            {
                string strNextName = i.ToString();
                kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
                i++;
            }
            kml.Document.Placemarks.Add(new Placemark("Total Path", Coords));

            //kml.Document.Placemark.LineString.Coordinates = "-96.757835,32.816929,0 -96.75775899999999,32.817066,0 -96.75785100000002,32.817116,0 -96.75856,32.817074,0 -96.75862100000001,32.817039,0 -96.758636,32.81638,0 -96.75861399999999,32.816273,0 -96.75855300000001,32.816238,0 -96.758095,32.81609,0 -96.757668,32.816059,0 -96.75759100000001,32.815968,0 -96.75758399999999,32.815437,0 -96.757645,32.815018,0 -96.75759100000001,32.814468,0 -96.75753,32.814243,0 -96.75756800000002,32.814007,0 -96.75752300000001,32.813923,0 -96.75755299999999,32.813671,0 -96.75752300000001,32.813156,0 -96.757538,32.812908,0 -96.75753,32.812859,0 -96.757431,32.812717,0 -96.757042,32.812649,0 -96.756218,32.812725,0 -96.75525700000002,32.81271,0 -96.754898,32.812431,0 -96.75482900000002,32.812344,0 -96.75436400000001,32.812031,0 -96.75415000000001,32.811966,0 -96.753975,32.811852,0 -96.75372299999998,32.811626,0 -96.75347099999999,32.811337,0 -96.753265,32.811169,0 -96.753113,32.8111,0 -96.752945,32.811203,0 -96.75266299999998,32.811287,0 -96.75241100000001,32.811539,0 -96.752312,32.811611,0 -96.75219,32.811653,0 -96.751953,32.811535,0 -96.75168600000001,32.811291,0 -96.75146499999998,32.811161,0 -96.75053399999999,32.810883,0 -96.750305,32.810844,0 -96.75000799999999,32.810886,0 -96.74977899999999,32.81097,0 -96.74961899999998,32.811192,0 -96.74949600000001,32.811234,0 -96.74943500000001,32.811359,0 -96.74928300000001,32.811394,0 -96.749146,32.811371,0 -96.74900100000001,32.811241,0 -96.748772,32.811096,0 -96.748711,32.810997,0 -96.748634,32.810989,0 -96.74844400000001,32.810745,0 -96.748322,32.81068,0 -96.74813100000002,32.810513,0 -96.747856,32.810192,0 -96.747124,32.809586,0 -96.746758,32.809338,0 -96.746239,32.808823,0 -96.74600199999999,32.808758,0 -96.74588,32.808681,0 -96.74496499999998,32.807957,0 -96.744781,32.807858,0 -96.744728,32.807743,0 -96.74465900000001,32.807732,0 -96.74453,32.807781,0 -96.74427799999999,32.807976,0 -96.743866,32.808182,0 -96.74283599999998,32.808537,0 -96.74215700000001,32.808681,0 -96.74197399999999,32.808762,0 -96.741699,32.808811,0 -96.74120300000001,32.80899,0 -96.74092899999999,32.809025,0 -96.740578,32.80917,0 -96.73992200000001,32.809277,0 -96.739555,32.809303,0 -96.739334,32.809284,0 -96.739212,32.809319,0 -96.73904400000001,32.809807,0 -96.738998,32.81015,0 -96.738884,32.810326,0 -96.738861,32.81049,0 -96.7388,32.810555,0 -96.73877,32.810741,0 -96.738708,32.81086,0 -96.73854799999999,32.811695,0 -96.738274,32.812561,0 -96.73819000000002,32.813019,0 -96.73814400000001,32.813068,0 -96.73803700000001,32.813412,0 -96.73795300000001,32.813572,0 -96.73782300000001,32.814171,0 -96.73773199999998,32.814392,0 -96.73773199999998,32.814617,0 -96.73769400000001,32.814663,0 -96.73762499999998,32.814957,0 -96.737633,32.815063,0 -96.73733500000002,32.816135,0 -96.73733500000002,32.817547,0 -96.73743399999999,32.81805,0 -96.737572,32.818424,0 -96.737724,32.819038,0 -96.737816,32.819466,0 -96.73786200000001,32.819923,0 -96.738007,32.820564,0 -96.738281,32.821457,0 -96.73838000000001,32.822159,0 -96.738472,32.822559,0 -96.73850299999999,32.822598,0 -96.73854799999999,32.823063,0 -96.73865499999999,32.823513,0 -96.738708,32.823597,0 -96.73878500000001,32.823593,0 -96.73932600000001,32.823532,0 -96.73949399999999,32.82349,0 -96.740532,32.823387,0 -96.74086800000001,32.823414,0 -96.741173,32.823326,0 -96.74131000000001,32.82333,0 -96.741287,32.823372,0 -96.74130200000001,32.823757,0 -96.741264,32.823795,0 -96.741249,32.823982,0 -96.741165,32.824337,0 -96.741158,32.824493,0 -96.74124099999999,32.824738,0 -96.74155399999999,32.825054,0 -96.74156199999999,32.82518,0 -96.74121899999999,32.825611,0 -96.74086800000001,32.825974,0 -96.74066899999998,32.826118,0 -96.74037199999999,32.826221,0 -96.74031100000001,32.826416,0 -96.74012000000001,32.827511,0 -96.74026499999999,32.827854,0 -96.740517,32.828201,0 -96.740852,32.828568,0 -96.740921,32.828671,0 -96.741074,32.828785,0 -96.74128,32.829052,0 -96.741821,32.829548,0 -96.74215700000001,32.829929,0 -96.74232499999999,32.830257,0 -96.742706,32.830872,0 -96.743011,32.831516,0 -96.74308000000002,32.831528,0 -96.743622,32.831371,0 -96.74408700000001,32.831276,0 -96.74440800000001,32.831158,0 -96.744942,32.831154,0 -96.74533099999999,32.831184,0 -96.745468,32.831226,0 -96.74567399999999,32.831211,0 -96.745689,32.831142,0 -96.74572000000001,32.830605,0 -96.745865,32.829601,0 -96.745811,32.829388,0 -96.745598,32.829266,0 -96.74546099999999,32.829247,0 -96.745346,32.829189,0 -96.74521599999999,32.829208,0 -96.74485,32.829063,0 -96.74444599999998,32.828785,0 -96.74435400000002,32.82869,0 -96.743622,32.828201,0 -96.74355300000001,32.82811,0 -96.74355300000001,32.82806,0 -96.743858,32.827766,0 -96.744064,32.827499,0 -96.744072,32.827358,0 -96.74397999999999,32.827133,0 -96.744438,32.826843,0 -96.744614,32.82679,0 -96.74530799999999,32.826759,0 -96.74543799999999,32.826778,0 -96.74597900000001,32.82674,0 -96.746246,32.826759,0 -96.74664300000002,32.826714,0 -96.747192,32.826736,0 -96.74733000000001,32.826721,0 -96.74750500000002,32.826637,0 -96.74758900000001,32.826653,0 -96.747765,32.826595,0 -96.74854300000001,32.826481,0 -96.748802,32.826477,0 -96.749374,32.826542,0 -96.749916,32.826534,0 -96.74999200000002,32.82629,0 -96.75,32.825863,0 -96.75003100000001,32.825699,0 -96.75007600000001,32.824848,0 -96.749878,32.824417,0 -96.74986300000001,32.824219,0 -96.749931,32.823101,0 -96.74988599999999,32.822636,0 -96.74987000000002,32.82169,0 -96.749931,32.821434,0 -96.749878,32.821346,0 -96.74987000000002,32.821236,0 -96.74990099999999,32.821007,0 -96.75004600000001,32.820808,0 -96.751091,32.819702,0 -96.75135799999998,32.819351,0 -96.75135,32.819294,0 -96.751289,32.819286,0 -96.750336,32.819458,0 -96.75013,32.819431,0 -96.749786,32.819542,0 -96.749847,32.819256,0 -96.749893,32.819153,0 -96.749893,32.819019,0 -96.75,32.818542,0 -96.75,32.818428,0 -96.750069,32.818329,0 -96.750328,32.81823,0 -96.75046500000001,32.818104,0 -96.751076,32.817898,0 -96.75205200000002,32.817497,0 -96.752296,32.817371,0 -96.752495,32.81723,0 -96.75278500000002,32.817131,0 -96.753693,32.817112,0 -96.753838,32.817139,0 -96.75398999999999,32.817116,0 -96.755409,32.817139,0 -96.755737,32.81712,0 -96.755852,32.817078,0 -96.75592000000002,32.817093,0 -96.755951,32.817039,0 -96.75611899999998,32.817005,0 -96.75694300000001,32.817165,0 -96.757469,32.817154,0 -96.757721,32.817078,0";

            string strXML = GetXMLStringFromObject(kml);
            System.IO.FileStream output = new FileStream("c:/temp/new.kml", FileMode.Create);
            byte[] bXML = System.Text.UTF8Encoding.UTF8.GetBytes(strXML);
            output.Write(bXML, 0, bXML.Length);
            output.Close();

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            XMPPClient.Disconnect();
        }

        private void ButtonClearKML_Click(object sender, RoutedEventArgs e)
        {
            BuddyPosition buddy = ((Button)sender).DataContext as BuddyPosition;
            if (buddy != null)
            {
                buddy.ClearCoordinates();
            }
        }

        #endregion

        private void ButtonViewMap_Click(object sender, RoutedEventArgs e)
        {
            BuddyPosition buddy = ((Button)sender).DataContext as BuddyPosition;
            if (buddy != null)
            {
                OurRosterItem = buddy.RosterItem;
                ShowMapButton_Click(sender, e);
            }
        }

        private RosterItem m_OurRosterItem = new RosterItem();

        public RosterItem OurRosterItem
        {
            get { return m_OurRosterItem; }
            set { m_OurRosterItem = value; }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxShowOnMap_Checked(object sender, RoutedEventArgs e)
        {
            BuddyPosition buddy = ((CheckBox)sender).DataContext as BuddyPosition;
            if (buddy != null)
            {
                buddy.bShowOnMap = true;
                OurRosterItem = buddy.RosterItem;
                ShowBuddiesOnMap();
            }
        }

        private void CheckBoxCenterOnBuddy_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            BuddyPosition buddy = ((CheckBox)sender).DataContext as BuddyPosition;
            if (buddy != null)
            {

                // buddy.bCenterOnBuddy = 
                // clear all other buddies center on me fields
                foreach (BuddyPosition buddyPosition in BuddyPositions)
                {
                    buddyPosition.bCenterOnBuddy = false;
                }
                buddy.bCenterOnBuddy = true;
                OurRosterItem = buddy.RosterItem;
                ShowMapButton_Click(sender, e);
            }
        }

        private void CheckboxShowMe_Checked(object sender, RoutedEventArgs e)
        {
            //BuddyPosition buddy = ((CheckBox)sender).DataContext as BuddyPosition;
            //if (buddy != null)
            //{
            //    buddy.bShowOnMap = true;
            //    OurRosterItem = buddy.RosterItem;
            //    ////ShowMapButton_Click(sender, e);
            //}
            // this one is show ME not a buddy

        }

        private void ButtonClearTransfers_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CheckboxCenterOnMe_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleButtonStartRecording_Click(object sender, RoutedEventArgs e)
        {

            //ToggleButtonStartRecording_Click.I
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            ShowMapButton_Click(sender, e);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
                this.DragMove();
        }

        private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
        }

        private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
        }

        private void ButtonClearMessages_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ButtonStartAudio_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void HyperlinkRosterItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TextBoxChatToSend_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TextBoxChatToSend_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void ListBoxRoster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        BlankWindow OptionsWindow = new BlankWindow();

        private void ButtonOptions_Click(object sender, RoutedEventArgs e)
        {
            //RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            //if (item == null)
            //    return;

            ShowOptionsWindow();
        }

        public void ShowOptionsWindow()
        {
            if (OptionsWindow.IsLoaded == false)
            {
                OptionsWindow = new BlankWindow();
                OptionsWindow.XMPPClient = this.XMPPClient;
                OptionsWindow.MapProperties = this.MapProperties;
                //OptionsWindow.OurRosterItem = rosterItem;


                // if a buddy is highlighted, center the map on them (or only show them),
                // or you are going to track/create KML for them
                //if (ListBoxRoster.SelectedItems.Count >= 0)
                //{
                //    // can't multi-select so we will only be able to select one buddy at a time
                //    MapWindow.OurRosterItem = ListBoxRoster.SelectedItem as RosterItem;
                //}

                OptionsWindow.Show();
                //  MapWindow.MapUserControl1.Refresh();
            }
            else
            {
                OptionsWindow = new BlankWindow();

                OptionsWindow.XMPPClient = this.XMPPClient;
                //OptionsWindow.OurRosterItem = rosterItem;

                //if (ListBoxRoster.SelectedItems.Count >= 0)
                //{
                //    // can't multi-select
                //    MapWindow.OurRosterItem = ListBoxRoster.SelectedItem as RosterItem;
                //}
                OptionsWindow.Show();
                //  MapWindow.MapUserControl1.Refresh();
                // Activate();
                IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(OptionsWindow).Handle;
                FlashWindow(windowHandle, true);
            }
        }
        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        private void MapImage_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private System.Windows.Point mouseClick;

        private double canvasLeft;

        private double canvasTop;

        private TranslateTransform _translateT = new TranslateTransform();
        private System.Windows.Point _lastMousePos = new System.Windows.Point();

        private void MapImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
          
           // MoveDown();
        }

        private void MapImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
           // MoveUp();
        }

        private void MapImageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
            {
                // get current location
               
            }
        }

        private void MapImageCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed
           // if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
           //     this.DragMove();
        }

        private void MapImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void MapImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            // Adjust map image resolution according to window size change.
            var previousSize = sizeInfo.PreviousSize;
            var newSize = sizeInfo.NewSize;
            var widthChanged = sizeInfo.WidthChanged;
            var heightChanged = sizeInfo.HeightChanged;

            //base.OnRenderSizeChanged(sizeInfo);
        }

    }

}