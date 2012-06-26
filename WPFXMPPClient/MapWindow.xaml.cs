///// Copyright (c) 2011 Brian Bonnett
///// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
///// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

//using System.Net.XMPP;
//using System.IO;

//namespace WPFXMPPClient
//{
//    /// <summary>
//    /// Interaction logic for FileTransferWindow.xaml
//    /// </summary>
//    public partial class MapWindow : Window
//    {
//        public MapWindow()
//        {
//            InitializeComponent();
//        }

//        public XMPPClient XMPPClient = null;

//        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
//        {
//            if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
//                this.DragMove();
//        }

//        private void Window_Loaded(object sender, RoutedEventArgs e)
//        {
//            /// Bind to our XMPP Client filetransfer
//            /// 
//            this.DataContext = XMPPClient;
//            this.ListBoxConversation.ItemsSource = XMPPClient.FileTransferManager.FileTransfers;
//        }

//        private void ButtonCancelSend_Click(object sender, RoutedEventArgs e)
//        {

//        }

//        private void ButtonClose_Click(object sender, RoutedEventArgs e)
//        {
//            this.Close();
//        }

//        private void ButtonAcceptTransfer_Click(object sender, RoutedEventArgs e)
//        {
//            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
//            if (trans != null)
//            {
//                XMPPClient.FileTransferManager.AcceptFileDownload(trans);
//            }
//        }

//        private void ButtonDeclineTransfer_Click(object sender, RoutedEventArgs e)
//        {
//            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
//            if (trans != null)
//            {
//                XMPPClient.FileTransferManager.DeclineFileDownload(trans);
//            }
//        }

//        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
//        {
//            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
//            if (trans != null)
//            {
//                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
//                dlg.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
//                dlg.FileName = trans.FileName;
//                if (dlg.ShowDialog() == true)
//                {
//                    FileStream stream = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
//                    stream.Write(trans.Bytes, 0, trans.Bytes.Length);
//                    stream.Close();
//                }
                
//            }
//        }

//        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
//        {
//            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
//            if (trans != null)
//            {
//                if (File.Exists(trans.FileName) == true)
//                {
//                    /// We've set auto save - which changes the full file name to the full directory
//                    System.Diagnostics.Process.Start(trans.FileName);
//                }
//            }
//        }

//        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
//        {
//            //List<FileTransfer> ClearList = new List<FileTransfer>();
//            //foreach (FileTransfer trans in XMPPClient.FileTransferManager.FileTransfers)
//            //{
//            //    if ((trans.FileTransferState == FileTransferState.Done) || (trans.FileTransferState == FileTransferState.Error))
//            //    {
//            //        ClearList.Add(trans);
//            //    }
//            //}

//            //foreach (FileTransfer trans in ClearList)
//            //{
//            //    XMPPClient.FileTransferManager.FileTransfers.Remove(trans);
//            //    trans.Close();
//            //}

//        }
//    }
//}





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
using Microsoft.VisualBasic;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.Net;
//using System.Drawing;
using Microsoft.Win32;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Navigation;
using System.Xml.Serialization;
using LocationClasses;
using System.ComponentModel;

namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        public MapWindow()
        {
            InitializeComponent();
        }

        //// public XMPPClient XMPPClient = null;
        //private RosterItem m_OurRosterItem = null;

        //public RosterItem OurRosterItem
        //{
        //    get { return m_OurRosterItem; }
        //    set
        //    {
        //        m_OurRosterItem = value;
        //        //MapUserControl1.OurRosterItem = value;
        //    }
        //}

        //private bool m_SingleRosterItemMap = true;

        //public bool SingleRosterItemMap
        //{
        //    get { return m_SingleRosterItemMap; }
        //    set
        //    {
        //        //MapUserControl1.SingleRosterItemMap = m_SingleRosterItemMap;
        //        m_SingleRosterItemMap = value;
        //    }
        //}
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
                this.DragMove();
        }

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    /// Bind to our XMPP Client filetransfer
        //    /// 
        //    this.DataContext = XMPPClient;
            
        //    // MapUserControl1.XMPPClient = XMPPClient;
        //     // MapUserControl1.OurRosterItem = this.OurRosterItem;
        //    //   this.ListBoxConversation.ItemsSource = XMPPClient.FileTransferManager.FileTransfers;

        //  ////  AddressTxtBox.Focus();

        //    var _with1 = saveDialog;
        //    _with1.DefaultExt = "png";
        //    _with1.Title = "Save Map Image";
        //    _with1.OverwritePrompt = true;
        //    _with1.Filter = "(*.png)|*.png";

        //    saveDialog.FileOk += saveDialog_FileOk;


        //}

        private void ButtonCancelSend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonAcceptTransfer_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                XMPPClient.FileTransferManager.AcceptFileDownload(trans);
            }
        }

        private void ButtonDeclineTransfer_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                XMPPClient.FileTransferManager.DeclineFileDownload(trans);
            }
        }

        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                dlg.FileName = trans.FileName;
                if (dlg.ShowDialog() == true)
                {
                    FileStream stream = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
                    stream.Write(trans.Bytes, 0, trans.Bytes.Length);
                    stream.Close();
                }

            }
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                string strFullFileName = string.Format("{0}\\{1}", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), trans.FileName);
                if (File.Exists(strFullFileName) == false)
                {
                    FileStream stream = new FileStream(strFullFileName, FileMode.Create, FileAccess.Write);
                    stream.Write(trans.Bytes, 0, trans.Bytes.Length);
                    stream.Close();
                }
                System.Diagnostics.Process.Start(strFullFileName);
            }
        }

       // GeoLocationWindow GeoLocationWindow = new GeoLocationWindow();
        private void MapUserControl1_Loaded(object sender, RoutedEventArgs e)
        {
         //   GeoLocationWindow.XMPPClient = this.XMPPClient;
        //  GeoLocationWindow.OurRosterItem = OurRosterItem;

            if (OurRosterItem != null)
            {
                TextBlockTitle.Text = String.Format("Buddy Map - {0}", OurRosterItem.JID.ToString());
                this.Title = String.Format("Buddy Map - {0}", OurRosterItem.JID.ToString());
            }

            // MapUserControl1.Refresh();
        }


        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();

            //MapUserControl1.Refresh();
            //ButtonLoadURL_Click(null, e);
        }

        private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn(1);
        }

        private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut(1);
        }

     

        //private void LoadMap()
        //{
        //    WebBrowserMap.Navigate("http://www.yahoo.com");
        //}

        //private void ButtonLoadURL_Click(object sender, RoutedEventArgs e)
        //{
        //    string strURL = TextBoxURL.Text;

        //    WebBrowserMap.Navigate(strURL);
        //}


        #region "Fields"
        private XDocument geoDoc;
        private string location;
        private int zoom;
        private SaveFileDialog saveDialog = new SaveFileDialog();
        private string mapType;
        private double lat;
        private double lng;
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
                //LongitudeTxtBlck.Text = longitude;

                //switch (locationType)
                //{
                //    case "APPROXIMATE":
                //        AccuracyTxtBlck.Text = "Approximate";
                //        break;
                //    case "ROOFTOP":
                //        AccuracyTxtBlck.Text = "Precise";
                //        break;
                //    default:
                //        AccuracyTxtBlck.Text = "Approximate";
                //        break;
                //}

                //lat = double.Parse(latitude);
                //lng = double.Parse(longitude);

                //if ((SaveButton.IsEnabled == false))
                //{
                //    SaveButton.IsEnabled = true;
                //    RoadmapToggleButton.IsEnabled = true;
                //    TerrainToggleButton.IsEnabled = true;
                //}

            }
            else if ((responseStatus == "ZERO_RESULTS"))
            {
                MessageBox.Show("Unable to show results for: " + "\r\n" + location,
                    // Constants.vbCrLf + location, 
                    "Unknown Location", MessageBoxButton.OK, MessageBoxImage.Information);
                DisplayXXXXXXs();
              ///  AddressTxtBox.SelectAll();
            }
            //ShowMapButton.IsEnabled = true;
            //ZoomInButton.IsEnabled = true;
            //ZoomOutButton.IsEnabled = true;
            //MapProgressBar.Visibility = System.Windows.Visibility.Hidden;
        }

        // Get and display map image in Image ctrl.
        private void ShowMapImage()
        {
            Refresh();
            return;

            BitmapImage bmpImage = new BitmapImage();
            string mapURL = "http://maps.googleapis.com/maps/api/staticmap?" + "size=500x400&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";

            bmpImage.BeginInit();
            bmpImage.UriSource = new Uri(mapURL);
            bmpImage.EndInit();

     ///       MapImage.Source = bmpImage;
        }

        private void ShowMapUsingLatLng()
        {
            Refresh();
            return;

            BitmapImage bmpImage = new BitmapImage();
            string mapURL = "http://maps.googleapis.com/maps/api/staticmap?" + "center=" + lat + "," + lng + "&" + "size=500x400&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";
            bmpImage.BeginInit();
            bmpImage.UriSource = new Uri(mapURL);
            bmpImage.EndInit();

        ////    MapImage.Source = bmpImage;
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

        //MapProperties MapProperties = new MapProperties();

        private void ShowMapUsingLatLngForAllCheckedBuddies()
        {
            MapProperties = BuildMapProperties();

            BitmapImage bmpImage = new BitmapImage();
            string mapURL = MapProperties.URL;

            // "http://maps.googleapis.com/maps/api/staticmap?" + "center=" + lat + "," + lng + "&" + "size=500x400&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";
            bmpImage.BeginInit();
            bmpImage.UriSource = new Uri(mapURL);
            bmpImage.EndInit();

         /////   MapImage.Source = bmpImage;
        }

        // Zoom-in on map.
        private void ZoomIn()
        {
            if ((MapProperties.LocationParameters.Zoom < 21))
            {
                MapProperties.LocationParameters.Zoom += 1;
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

            if (MapProperties.LocationParameters.Zoom < 21)
                ZoomInButton.IsEnabled = true;
            else
                ZoomInButton.IsEnabled = false;


        }

        // Zoom-out on map.
        private void ZoomOut()
        {
            if ((MapProperties.LocationParameters.Zoom > 0))
            {
                MapProperties.LocationParameters.Zoom -= 1;
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


            if (MapProperties.LocationParameters.Zoom > 0)
                ZoomOutButton.IsEnabled = true;
            else
                ZoomOutButton.IsEnabled = false;
        
        }

        private void SaveMap()
        {
            // string mapURL = "http://maps.googleapis.com/maps/api/staticmap?" + "center=" + lat + "," + lng + "&" + "size=500x400&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";
            string mapURL = MapProperties.URL;

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
            if (MapProperties.LocationParameters.CenterGeoCoordinate.Latitude == -1)
                MapProperties.LocationParameters.CenterGeoCoordinate.Latitude = OurRosterItem.GeoLoc.lat;

            // Default zoom is 15 and at this level changing
            // the center point is done by 0.003 degrees. 
            // Shifting the center point is done by higher values
            // at zoom levels less than 15.
            double diff = 0;
            double shift = 0;
            // Use 88 to avoid values beyond 90 degrees of lat.
            if ((MapProperties.LocationParameters.CenterGeoCoordinate.Latitude < 88))
            {
                if ((MapProperties.LocationParameters.Zoom == 15))
                {
                    MapProperties.LocationParameters.CenterGeoCoordinate.Latitude += 0.003;
                }
                else if ((MapProperties.LocationParameters.Zoom > 15))
                {
                    diff = MapProperties.LocationParameters.Zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    MapProperties.LocationParameters.CenterGeoCoordinate.Latitude += shift;
                }
                else
                {
                    diff = 15 - MapProperties.LocationParameters.Zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    MapProperties.LocationParameters.CenterGeoCoordinate.Latitude += shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                MapProperties.LocationParameters.CenterGeoCoordinate.Latitude = 90;
            }
        }

        private void MoveDown()
        {
            if (MapProperties.LocationParameters.CenterGeoCoordinate.Latitude == -1)
                MapProperties.LocationParameters.CenterGeoCoordinate.Latitude = OurRosterItem.GeoLoc.lat;

            double diff = 0;
            double shift = 0;
            if ((MapProperties.LocationParameters.CenterGeoCoordinate.Latitude > -88))
            {
                if ((MapProperties.LocationParameters.Zoom == 15))
                {
                    MapProperties.LocationParameters.CenterGeoCoordinate.Latitude -= 0.003;
                }
                else if ((MapProperties.LocationParameters.Zoom > 15))
                {
                    diff = MapProperties.LocationParameters.Zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    MapProperties.LocationParameters.CenterGeoCoordinate.Latitude -= shift;
                }
                else
                {
                    diff = 15 - MapProperties.LocationParameters.Zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    MapProperties.LocationParameters.CenterGeoCoordinate.Latitude -= shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                MapProperties.LocationParameters.CenterGeoCoordinate.Latitude = -90;
            }
        }

        private void MoveLeft()
        {
            if (MapProperties.LocationParameters.CenterGeoCoordinate.Longitude == -1)
                MapProperties.LocationParameters.CenterGeoCoordinate.Longitude = OurRosterItem.GeoLoc.lon;

            double diff = 0;
            double shift = 0;
            // Use -178 to avoid negative values below -180.
            if ((MapProperties.LocationParameters.CenterGeoCoordinate.Longitude > -178))
            {
                if ((MapProperties.LocationParameters.Zoom == 15))
                {
                    MapProperties.LocationParameters.CenterGeoCoordinate.Longitude -= 0.003;
                }
                else if ((MapProperties.LocationParameters.Zoom > 15))
                {
                    diff = MapProperties.LocationParameters.Zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    MapProperties.LocationParameters.CenterGeoCoordinate.Longitude -= shift;
                }
                else
                {
                    diff = 15 - MapProperties.LocationParameters.Zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    MapProperties.LocationParameters.CenterGeoCoordinate.Longitude -= shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                MapProperties.LocationParameters.CenterGeoCoordinate.Longitude = 180;
            }
        }

        private void MoveRight()
        {
            if (MapProperties.LocationParameters.CenterGeoCoordinate.Longitude == -1)
                MapProperties.LocationParameters.CenterGeoCoordinate.Longitude = OurRosterItem.GeoLoc.lon;

            double diff = 0;
            double shift = 0;
            if ((MapProperties.LocationParameters.CenterGeoCoordinate.Longitude < 178))
            {
                if ((MapProperties.LocationParameters.Zoom == 15))
                {
                    MapProperties.LocationParameters.CenterGeoCoordinate.Longitude += 0.003;
                }
                else if ((MapProperties.LocationParameters.Zoom > 15))
                {
                    diff = MapProperties.LocationParameters.Zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    MapProperties.LocationParameters.CenterGeoCoordinate.Longitude += shift;
                }
                else
                {
                    diff = 15 - MapProperties.LocationParameters.Zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    MapProperties.LocationParameters.CenterGeoCoordinate.Longitude += shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                MapProperties.LocationParameters.CenterGeoCoordinate.Longitude = -180;
            }
        }

        private void DisplayXXXXXXs()
        {
            //AddressTxtBlck.Text = "XXXXXXXXX, XXXXX, XXXXXX";
            //LatitudeTxtBlck.Text = "XXXXXXXXXX";
            //LongitudeTxtBlck.Text = "XXXXXXXXXX";
            //AccuracyTxtBlck.Text = "XXXXXXXXX";
        }

        private void HideProgressBar()
        {
            //MapProgressBar.Visibility = System.Windows.Visibility.Hidden;
            //ShowMapButton.IsEnabled = true;
        }

        // ShowMapButton click event handler.
        private void ShowMapButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //ShowMapButton.IsEnabled = false;
            //MapProgressBar.Visibility = System.Windows.Visibility.Visible;
            //AddressTxtBox.Text = String.Format("{0},{1}", OurRosterItem.GeoLoc.lat, OurRosterItem.GeoLoc.lon);

            //if ((AddressTxtBox.Text != string.Empty))
            //{
            //    location = AddressTxtBox.Text.Replace(" ", "+");
            //    zoom = 15;
            //    mapType = "roadmap";

            //    // use a real regex

            //    string[] parms = location.Split(',');
            //    if (parms.Length == 2)
            //    {
            //        try
            //        {
            //            lat = Convert.ToDouble(parms[0]);
            //        }
            //        catch (Exception ex)
            //        {

            //        }
            //        try
            //        {
            //            lng = Convert.ToDouble(parms[1]);
            //        }
            //        catch (Exception ex)
            //        {

            //        }

            //        ShowMapImage();
            //        AddressTxtBox.SelectAll();
            //        ShowMapButton.IsEnabled = true;

            //        //AddressTxtBlck.Text = formattedAddress;
            //        LatitudeTxtBlck.Text = lat.ToString();
            //        LongitudeTxtBlck.Text = lng.ToString();
            //        AccuracyTxtBlck.Text = "Approximate";

            //        //switch (locationType)
            //        //{
            //        //    case "APPROXIMATE":
            //        //        AccuracyTxtBlck.Text = "Approximate";
            //        //        break;
            //        //    case "ROOFTOP":
            //        //        AccuracyTxtBlck.Text = "Precise";
            //        //        break;
            //        //    default:
            //        //        AccuracyTxtBlck.Text = "Approximate";
            //        //        break;
            //        //}




            //        MapProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            //    }
            //    else
            //    {
            //        Thread geoThread = new Thread(GetGeocodeData);
            //        geoThread.Start();

            //        ShowMapImage();
            //        AddressTxtBox.SelectAll();
            //        ShowMapButton.IsEnabled = false;

            //        MapProgressBar.Visibility = System.Windows.Visibility.Visible;

            //    }



            //    if ((RoadmapToggleButton.IsChecked == false))
            //    {
            //        RoadmapToggleButton.IsChecked = true;
            //        TerrainToggleButton.IsChecked = false;
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Enter location address.", "Map App", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            //    AddressTxtBox.Focus();
            //}
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
          /////      TerrainToggleButton.IsChecked = false;
            }
        }

        // TerrainToggleButton Checked event handler.
        private void TerrainToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((mapType != "terrain"))
            {
                mapType = "terrain";
                ShowMapUsingLatLng();
 ////               RoadmapToggleButton.IsChecked = false;
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

  //      private void Window1_Loaded(object sender, System.Windows.RoutedEventArgs e)
  //      {
          
  ///////AddressTxtBox.Focus();

  //          var _with1 = saveDialog;
  //          _with1.DefaultExt = "png";
  //          _with1.Title = "Save Map Image";
  //          _with1.OverwritePrompt = true;
  //          _with1.Filter = "(*.png)|*.png";

  //          saveDialog.FileOk += saveDialog_FileOk;

  //          //Window_Loaded(sender, e);
  //      }

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



        private void ShowBuddiesOnMap()
        {
            ShowMapUsingLatLngForAllCheckedBuddies();
        }

        private void buttonShowLatLon_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    lat = Convert.ToDouble(TextBoxLat.Text);
            //}
            //catch (Exception ex)
            //{
            //    return;
            //}

            //try
            //{
            //    lng = Convert.ToDouble(TextBoxLon.Text);
            //}
            //catch (Exception ex)
            //{
            //    return;
            //}

            //location = String.Format("{0},{1}", lat, lng);
            //LatitudeTxtBlck.Text = lat.ToString();
            //LongitudeTxtBlck.Text = lng.ToString();

            //AddressTxtBlck.Text = String.Format("{0},{1}", lat, lng);





            //ShowMapButton.IsEnabled = true;
            //ZoomInButton.IsEnabled = true;
            //ZoomOutButton.IsEnabled = true;

            //TerrainToggleButton.IsEnabled = true;
            //RoadmapToggleButton.IsEnabled = true;

            ShowMapUsingLatLng();
        }



        //public XMPPClient XMPPClient = new XMPPClient();
        ////public RosterItem OurRosterItem = new RosterItem();
        //public BuddyPosition MyBuddyPosition = null;
        //public RosterItem MyRosterItem = null;


        //EARTHLib.ApplicationGEClass earth = new EARTHLib.ApplicationGEClass();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (OurRosterItem != null)
            {
                //TextBlockTitle.Text = String.Format("Buddy Map - {0}", OurRosterItem.JID.ToString());
                this.Title = String.Format("Buddy Map - {0}", OurRosterItem.JID.ToString());
                TextBlockTitleBuddy.Text = OurRosterItem.JID.ToString();
                TextBlockTitleTimestamp.Text = OurRosterItem.GeoLoc.TimeStamp.ToString();
            }


            RosterItem MyRosterItem = new RosterItem(XMPPClient, XMPPClient.XMPPAccount.JID);
            MyBuddyPosition = new BuddyPosition(MyRosterItem) { bIsMe = true };

            if (OurRosterItem == null)
            {
                MyBuddyPosition.bCenterOnBuddy = true;
                MyBuddyPosition.bShowOnMap = true;
            }

            /* 
            XMPPClient.AutomaticallyDownloadAvatars = false;
            XMPPClient.AutoAcceptPresenceSubscribe = false;
            XMPPClient.FileTransferManager.AutoDownload = true;
            // XMPPClient.FileTransferManager.OnTransferFinished += new FileTransferManager.DelegateDownloadFinished(FileTransferManager_OnTransferFinished);
            XMPPClient.AutoQueryServerFeatures = false;
            */

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

           // WriteSampleKML();

            InitializeValues();
            ComboBoxZoom.ItemsSource = ZoomLevels;
            ComboBoxZoom.SelectedIndex = 15;

            ComboBoxScale.ItemsSource = ScaleValues;
            ComboBoxScale.SelectedIndex = 1;
            //ComboBoxZoom.DataContext = MapProperties.LocationParameters.Zoom;

            ComboBoxMapType.ItemsSource = Enum.GetValues(typeof(MapType));
            ComboBoxMapType.SelectedIndex = 0;

            MapProperties.MapParameters = new MapParameters() { MapType = (MapType)ComboBoxMapType.SelectedValue, Scale = (int)ComboBoxScale.SelectedValue };
            MapProperties.MapParameters.Size = new SizeParameters(); //  { Horizontal = 1000, Vertical = 1000 };
            TextBoxSizeHorizontal.Text = String.Format("{0}", MapProperties.MapParameters.Size.Horizontal);
            TextBoxSizeVertical.Text = String.Format("{0}", MapProperties.MapParameters.Size.Vertical);

           // MapProperties.MapParameters.Size.Horizontal = Convert.ToInt32(TextBoxSizeHorizontal.Text);
           // MapProperties.MapParameters.Size.Vertical = Convert.ToInt32(TextBoxSizeVertical.Text);

            MapProperties.LocationParameters = new LocationParameters(); 
            // { Zoom = (int)ComboBoxZoom.SelectedValue };
            ComboBoxZoom.SelectedValue = MapProperties.LocationParameters.Zoom;

            //BuildURL();
            //LoadURL();
            //if (XMPPClient != null)
            //{
            //    XMPPClient.OnXMLReceived += new System.Net.XMPP.XMPPClient.DelegateString(XMPPClient_OnXMLReceived);
            //    XMPPClient.OnXMLSent += new System.Net.XMPP.XMPPClient.DelegateString(XMPPClient_OnXMLSent);
            //}
            SetUpRosterItemNotifications();
           //  ButtonLoadLocation_Click(null, e);

            Refresh();
 
        }

        private string BuildHtml()
        {
            StringBuilder html = new StringBuilder();
           
            //string jqueryPath = System.IO.Path.Combine(appPath, "jquery-1.7.2.js"); 
            //html.AppendFormat("<script type=\"text/javascript\" src=\"{0}\"; ></script>", jqueryPath);
            html.AppendLine("<html>");
            html.AppendLine("hello world");
            html.AppendLine("</html>");

            return html.ToString();
        }

        void client_OnStateChanged(object sender, EventArgs e)
        {

        }

        KMLBuilder KMLBuilder = new KMLBuilder();

        ObservableCollectionEx<BuddyPosition> BuddyPositions = new ObservableCollectionEx<BuddyPosition>();
        BuddyPosition MyBuddyPosition = null;

        void client_OnRetrievedRoster(object sender, EventArgs e)
        {
            /// bind to ourlist
            /// 
            // Add myself as my own buddy in the BuddyPositions list
            BuddyPositions.Add(MyBuddyPosition);

            foreach (RosterItem item in XMPPClient.RosterItems)
            {
                BuddyPositions.Add(new BuddyPosition(item));
            }

            this.Dispatcher.Invoke(new Action(() => { this.ListViewBuddies.ItemsSource = BuddyPositions; this.DataContext = this; this.ListViewBuddies.DataContext = BuddyPositions; }));
           
        }

 
        //void WriteBuddyKML(string strFileName, BuddyPosition buddy)
        //{
        //    MyKML kml = new MyKML();
        //    kml.Document.Name = string.Format("Buddy {0} coordinates for {1}", buddy.RosterItem.JID.BareJID, DateTime.Now);

        //    int i = 1;
        //    foreach (GeoCoordinate coord in buddy.CoordinateList1)
        //    {
        //        string strNextName = i.ToString();
        //        kml.Document.Placemarks.Add(new Placemark(strNextName, coord));
        //        i++;
        //    }
        //    kml.Document.Placemarks.Add(new Placemark("Total Path", buddy.CoordinateList1));

        //    string strXML = GetXMLStringFromObject(kml);
        //    System.IO.FileStream output = new FileStream(strFileName, FileMode.Create);
        //    byte[] bXML = System.Text.UTF8Encoding.UTF8.GetBytes(strXML);
        //    output.Write(bXML, 0, bXML.Length);
        //    output.Close();

        //}
        ///{"Member DocumentType.Items of type AbstractFeatureType[] hides 
        ///base class member AbstractFeatureType.Items of type AbstractStyleSelectorType[]. 
        /// Use XmlElementAttribute or XmlAttributeAttribute to specify a new name."}
        //public static string GetXMLStringFromObject(object obj)
        //{
        //    StringWriter stream = new StringWriter();
        //    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
        //    namespaces.Add("gx", "http://www.google.com/kml/ext/2.2");
        //    XmlWriterSettings settings = new XmlWriterSettings();
        //    settings.OmitXmlDeclaration = true;
        //    settings.Indent = true;
        //    XmlWriter writer = XmlWriter.Create(stream, settings);


        //    XmlSerializer ser = new XmlSerializer(obj.GetType());
        //    ser.Serialize(writer, obj, namespaces);

        //    writer.Flush();
        //    writer.Close();

        //    string strRet = stream.ToString();

        //    stream.Close();
        //    stream.Dispose();

        //    return strRet;
        //}

        private void ButtonSaveKML_Click(object sender, RoutedEventArgs e)
        {
            BuddyPosition buddy = ((Button)sender).DataContext as BuddyPosition;
            if (buddy != null)
            {
                string strDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\GeoTracks";
                if (System.IO.Directory.Exists(strDirectory) == false)
                    System.IO.Directory.CreateDirectory(strDirectory);

                string strFileName = string.Format("{0}/{1}_{2}.kml", strDirectory, buddy.RosterItem.JID.User, Guid.NewGuid());
                KMLBuilder.WriteBuddyKML(strFileName, buddy);
                System.Diagnostics.Process.Start(strFileName);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //XMPPClient.Disconnect();
        }

        private void ButtonClearKML_Click(object sender, RoutedEventArgs e)
        {
            BuddyPosition buddy = ((Button)sender).DataContext as BuddyPosition;
            if (buddy != null)
            {
                buddy.ClearCoordinates();
            }
        }

        private void ButtonViewMap_Click(object sender, RoutedEventArgs e)
        {
            BuddyPosition buddy = ((Button)sender).DataContext as BuddyPosition;
            if (buddy != null)
            {
                OurRosterItem = buddy.RosterItem;
                ShowMapButton_Click(sender, e);
            }
        }

        //private RosterItem m_OurRosterItem = new RosterItem();

        //public RosterItem OurRosterItem
        //{
        //    get { return m_OurRosterItem; }
        //    set { m_OurRosterItem = value; }
        //}

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxShowOnMap_Checked(object sender, RoutedEventArgs e)
        {
            BuddyPosition buddy = ((CheckBox)sender).DataContext as BuddyPosition;
            if (buddy != null)
            {
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
            //    ShowMapButton_Click(sender, e);
            //}
            // this one is show ME not a buddy

        }

        private void ButtonClearTransfers_Click(object sender, RoutedEventArgs e)
        {

        }

       

        private void CheckboxCenterOnMe_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleButtonStartRecording_Click(object sender, RoutedEventArgs e)
        {

            //ToggleButtonStartRecording_Click.I
        }



         

        //private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
        //        this.DragMove();
        //}
    

    //public class BuddyPosition : INotifyPropertyChanged
    //{
    //    public bool bShowOnMap = false;
    //    public bool bCenterOnBuddy = false;
    //    public bool bIsMe = false;

    //    public BuddyPosition(RosterItem item)
    //    {
    //        RosterItem = item;
    //        ((INotifyPropertyChanged)item).PropertyChanged += new PropertyChangedEventHandler(BuddyPosition_PropertyChanged);
    //    }

    //    void BuddyPosition_PropertyChanged(object sender, PropertyChangedEventArgs e)
    //    {
    //        if (e.PropertyName == "GeoLoc")
    //        {
    //            /// New geolocation, add it to our list
    //            /// 
    //            GeoCoordinate coord = new GeoCoordinate(RosterItem.GeoLoc.lon, RosterItem.GeoLoc.lat, RosterItem.GeoLoc.TimeStamp);
    //            CoordinateList1.Add(coord);
    //            FirePropertyChanged("Count");



    //        }
    //    }

    //    private RosterItem m_objRosterItem = null;

    //    public RosterItem RosterItem
    //    {
    //        get { return m_objRosterItem; }
    //        set
    //        {
    //            m_objRosterItem = value;
    //            FirePropertyChanged("RosterItem");
    //        }
    //    }

    //    public void ClearCoordinates()
    //    {
    //        m_listCoordinateList.Clear();
    //        FirePropertyChanged("Count");
    //    }

    //    private List<GeoCoordinate> m_listCoordinateList = new List<GeoCoordinate>();

    //    public List<GeoCoordinate> CoordinateList1
    //    {
    //        get { return m_listCoordinateList; }
    //        set { m_listCoordinateList = value; }
    //    }

    //    public int Count
    //    {
    //        get
    //        {
    //            return CoordinateList1.Count;
    //        }
    //        set
    //        {
    //        }
    //    }



    //    #region INotifyPropertyChanged Members

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    void FirePropertyChanged(string strName)
    //    {
    //        if (PropertyChanged != null)
    //            PropertyChanged(this, new PropertyChangedEventArgs(strName));
    //    }

    //    #endregion
    //}

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


        bool bLoaded = false;

        public string strURL = "http://maps.googleapis.com/maps/api/staticmap?";

        public RosterItem OurRosterItem = null;
        public XMPPClient XMPPClient = null;
        public MapProperties MapProperties = new MapProperties();
       
        private bool m_SingleRosterItemMap = true;

        public List<int> ZoomLevels = new List<int>();
        public List<int> ScaleValues = new List<int>();

        public bool SingleRosterItemMap
        {
            get { return m_SingleRosterItemMap; }
            set { m_SingleRosterItemMap = value; }
        }

        private void InitializeValues()
        {
            ZoomLevels.Clear();
            for (int i = 1; i <= 21; i++)
            {
                ZoomLevels.Add(i);
            }

            ScaleValues.Clear();
            for (int i = 1; i <= 2; i++)
            {
                ScaleValues.Add(i);
            }
        }


        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    InitializeValues();
        //    ComboBoxZoom.ItemsSource = ZoomLevels;
        //    ComboBoxZoom.SelectedIndex = 15;

        //    ComboBoxScale.ItemsSource = ScaleValues;
        //    ComboBoxScale.SelectedIndex = 1;
        //    //ComboBoxZoom.DataContext = MapProperties.LocationParameters.Zoom;

        //    ComboBoxMapType.ItemsSource = Enum.GetValues(typeof(MapType));
        //    ComboBoxMapType.SelectedIndex = 0;

        //    MapProperties.MapParameters = new MapParameters() { MapType = (MapType)ComboBoxMapType.SelectedValue, Scale = (int)ComboBoxScale.SelectedValue };
        //    MapProperties.MapParameters.Size = new SizeParameters() { Horizontal = 1000, Vertical = 1000 };
        //    TextBoxSizeHorizontal.Text = String.Format("{0}", 1000);
        //    TextBoxSizeVertical.Text = String.Format("{0}", 1000);

        //    MapProperties.MapParameters.Size.Horizontal = Convert.ToInt32(TextBoxSizeHorizontal.Text);
        //    MapProperties.MapParameters.Size.Vertical = Convert.ToInt32(TextBoxSizeVertical.Text);

        //    MapProperties.LocationParameters = new LocationParameters() { Zoom = (int)ComboBoxZoom.SelectedValue };

        //    //BuildURL();
        //    //LoadURL();
        //    if (XMPPClient != null)
        //    {
        //        XMPPClient.OnXMLReceived += new System.Net.XMPP.XMPPClient.DelegateString(XMPPClient_OnXMLReceived);
        //        XMPPClient.OnXMLSent += new System.Net.XMPP.XMPPClient.DelegateString(XMPPClient_OnXMLSent);
        //    }
        //    SetUpRosterItemNotifications();
        //    ButtonLoadLocation_Click(null, e);
        //}

        private geoloc ExtractGeoLoc(string strXML)
        {
            geoloc newGeoLoc = null;
            //<geoloc xmlns=\"http://jabber.org/protocol/geoloc\">
            //<lat>32.816849551194174</lat>
            //<lon>-96.757696867079247</lon>
            //<acurracy>0</acurracy>
            //<timestamp>2012-03-20T16:47:23.379-05:00</timestamp>
            //<geoloc>";
            return newGeoLoc;
        }

        void XMPPClient_OnXMLSent(XMPPClient client, string strXML)
        {

            if (strXML.Contains("GeoLoc"))
            {

            }

            // throw new NotImplementedException();
        }

        void XMPPClient_OnXMLReceived(XMPPClient client, string strXML)
        {
            if (strXML.Contains("GeoLoc"))
            {

            }
            // throw new NotImplementedException();
        }

        private void SetUpRosterItemNotifications()
        {
            if (XMPPClient == null)
                return;

            foreach (RosterItem rosterItem in XMPPClient.RosterItems)
            {
                rosterItem.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(rosterItem_PropertyChanged);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if ((e.Key == Key.F5))
            {
                //                BuildURL();
                //                LoadURL();
                ButtonLoadLocation_Click(null, null);
            }

            base.OnPreviewKeyDown(e);
        }

        void rosterItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                RosterItem item = sender as RosterItem;
                if (item == null)
                    return;

                if (item != OurRosterItem)
                    return;

                // WebBrowserMap.Navigate(strURL);
                if (e.PropertyName == "GeoLoc")
                {
                    // increment the counter. not the right way to determine when to move the map but a way for now.
                    nMovementCounter++;
                    // determine how many steps puts us too far, based on the size and the current location and last center, etc.
                    if (nMovementCounter >= 10)
                    {
                        bCenterOnMap = true;
                        nMovementCounter = 0;
                    }

                    if (SingleRosterItemMap)
                    {
                        //RosterItem item = sender as RosterItem;

                        //if (item.JID.User == "brianbonnett")
                        {
                            if (item.GeoLoc.lat == 0.0 && item.GeoLoc.lon == 0.0)
                                return;

                            BuildURL();

                            TextBlockLastLocationTimestamp.Text = String.Format("{0} {1}", item.GeoLoc.TimeStamp.ToShortDateString(),
                                item.GeoLoc.TimeStamp.ToShortTimeString());

                            //strURL = BuildURLForRosterItem(item, "blue", "B");
                            Console.WriteLine(String.Format("{0}: {1}, {2}", item.JID.User, item.GeoLoc.lat, item.GeoLoc.lon));
                            // MessageBox.Show("updating location!");
                            if (Paths.ContainsKey(item) == false)
                                Paths.Add(item, new List<geoloc>());
                            Paths[item].Add(item.GeoLoc);

                            TextBoxURL.Text = strURL;
                            ///TextBoxTimeStamp.Text = String.Format("{0}'s Location ({1}): {2}, {3}", item.JID.BareJID, item.GeoLoc.TimeStamp, item.GeoLoc.lat, item.GeoLoc.lon);
                            if (item.GeoLoc.TimeStamp != null) 
                                TextBoxTimeStamp.Text = String.Format("{0}", item.GeoLoc.TimeStamp).Replace("Timestamp: ", "");
                            RosterItems.Add(item);
                           // LocationsList.ItemsSource = RosterItems;
                            LoadURL();


                            return;
                        }
                    }
                    else
                    {

                        // throw new NotImplementedException();
                        //if (e.PropertyName == "GeoLoc")
                        {
                            string strURLUpdated = BuildURLForAllRosterItems();
                            if (String.Compare(strURLUpdated, strURL, true) == 0)
                            {
                                strURL = strURLUpdated;
                                LoadURL();
                            }
                        }
                    }
                }
            })
                  );

        }

        List<RosterItem> RosterItems = new List<RosterItem>();

        private void LoadURL()
        {
            if (MapProperties.LocationParameters.CenterGeoCoordinate == null)
                MapProperties.LocationParameters.CenterGeoCoordinate = new GeoCoordinate(OurRosterItem.GeoLoc.lat, OurRosterItem.GeoLoc.lon, OurRosterItem.GeoLoc.TimeStamp);

            if (OurRosterItem != null)
            {
                // Build Javascript HTML
                strJavaScriptHtml = MapBuilder.BuildJavaScriptSourceCode(MapProperties, OurRosterItem);
                TextBoxBrowserSourceCode.Text = strJavaScriptHtml;


                //LoadImageFromURL();
                this.Dispatcher.Invoke(new Action(() =>
                {
                   // LoadImageFromURL();
                    WebBrowserMap.NavigateToString(TextBoxBrowserSourceCode.Text);
                    // WebBrowserMap.Navigate(strURL);
                })
                );


                //if (OurRosterItem != null)
                {
                    //TextBlockTitle.Text = String.Format("Buddy Map - {0}", OurRosterItem.JID.ToString());
                    this.Title = String.Format("Buddy Map - {0}", OurRosterItem.JID.ToString());
                    TextBlockTitleBuddy.Text = OurRosterItem.JID.ToString();
                    TextBlockTitleTimestamp.Text = OurRosterItem.GeoLoc.TimeStamp.ToString();
                }

            }


        }

        private void LoadImageFromURL()
        {
            BitmapImage _image = new BitmapImage();
            _image.BeginInit();
            //if (bLoaded == false)
            {
                _image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                _image.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.None;

                //_image.CacheOption = BitmapCacheOption.None;
                //_image.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                //_image.CacheOption = BitmapCacheOption.OnLoad;
                //_image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            }

            _image.UriSource = new Uri(strURL, UriKind.RelativeOrAbsolute);
            _image.EndInit();
            MapImage.Source = _image;

            if (bLoaded == false)
            {
                bLoaded = true;
                LoadImageFromURL();
            }

            //BitmapImage bmpImage = new BitmapImage();
            ////string mapURL = "http://maps.googleapis.com/maps/api/staticmap?" + "center=" + lat + "," + lng + "&" + "size=500x400&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";
            //bmpImage.BeginInit();
            //bmpImage.UriSource = new Uri(strURL);
            //bmpImage.EndInit();

            //MapImage.Source = bmpImage;
        }


        private string BuildURLForAllRosterItems()
        {
            string strGoogleMapsApiURL = "http://maps.googleapis.com/maps/api/staticmap?";
            strGoogleMapsApiURL += String.Format("&zoom={0}", MapProperties.LocationParameters.Zoom);

            strGoogleMapsApiURL += String.Format("&maptype={0}", MapProperties.MapParameters.MapType);

            strGoogleMapsApiURL += String.Format("&size=800x800");
            strGoogleMapsApiURL += String.Format("&sensor=false");
            string strMyLatLon = String.Format("{0},{1}", this.XMPPClient.GeoLocation.lat, this.XMPPClient.GeoLocation.lon);
            string strMyColor = "red";
            string strMyLabel = "A";
            if (this.XMPPClient.JID != null && this.XMPPClient.JID.User != null && this.XMPPClient.JID.User.Length >= 1)
                strMyLabel = this.XMPPClient.JID.User[0].ToString();

            if (strMyLatLon != "0,0")
                strGoogleMapsApiURL += String.Format("&center={0}", strMyLatLon);
            if (strMyLatLon != "0,0")
                strGoogleMapsApiURL += String.Format("&markers=color:{0}%7Clabel:{1}%7C{2}", strMyColor, strMyLabel, strMyLatLon);

            // BuildMarkerForRosterItem(rosterItem, strColor, strLabel);

            string strMarkers = "";
            List<string> colors = new List<string>() { "red", "blue", "yellow", "green" };
            List<string> labels = new List<string>() { "B", "C", "D", "E", "F", "G", "H", "I", "J" };

            int nColorIndex = 0;
            int nLabelIndex = 0;

            foreach (RosterItem rosterItem in XMPPClient.RosterItems)
            {
                string strColor = "";
                string strLabel = "";
                if (!(nColorIndex < colors.Count()))
                    nColorIndex++;
                if (!(nLabelIndex < labels.Count()))
                    nLabelIndex++;

                strColor = colors[nColorIndex];
                strLabel = labels[nLabelIndex];

                //string strLabel = labels[nColorIndex];

                if (rosterItem != null && rosterItem.JID != null && rosterItem.JID.User != null && rosterItem.JID.User.Length >= 1)
                    strLabel = rosterItem.JID.User[0].ToString();

                strMarkers += BuildMarkerForRosterItem(rosterItem, strColor, strLabel);





            }

            strGoogleMapsApiURL += strMarkers;


            return strGoogleMapsApiURL;
        }

        private string BuildMarkerForRosterItem(RosterItem rosterItem, string strColor, string strLabel)
        {
            string strLatLon = String.Format("{0},{1}", rosterItem.GeoLoc.lat, rosterItem.GeoLoc.lon);
            return String.Format("&markers=color:{0}%7Clabel:{1}%7C{2}", strColor, strLabel, strLatLon);

        }

        private string BuildURLForRosterItem(RosterItem rosterItem, string strColor, string strLabel)
        {
            string strGoogleMapsApiURL = "http://maps.googleapis.com/maps/api/staticmap?";

            if (rosterItem != null)
            {
                string strLatLon = String.Format("{0},{1}", rosterItem.GeoLoc.lat, rosterItem.GeoLoc.lon);
                // OurRosterItem.GeoString;

                strGoogleMapsApiURL += String.Format("center={0}", strLatLon);

                strGoogleMapsApiURL += BuildMarkerForRosterItem(rosterItem, strColor, strLabel);

                TextBoxTimeStamp.Text = String.Format("{0}'s Location ({1}): ", rosterItem.JID.ToString(), rosterItem.GeoLoc.TimeStamp);
                TextBoxGeoLoc.Text = strLatLon;

                //strURL = strGoogleMapsApiURL;
                //TextBoxURL.Text = strURL;
                // center=Williamsburg,Brooklyn,NY
                // &zoom=13
                // &size=800x800&
                // markers=color:blue%7Clabel:S%7C11211%7C11206%7C11222&sensor=false";

            }
            return strGoogleMapsApiURL;
        }

        public void Refresh()
        {
            ButtonUpdate_Click(null, null);
            //ButtonLoadLocation_Click(null, null);
        }

        private int nMovementCounter = 0;
        private bool bCenterOnMap = true;
        private bool bDirty = false;

        private void ComboBoxMapType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bDirty = true;
        }

        private void TextBoxSizeHorizontal_TextChanged(object sender, TextChangedEventArgs e)
        {
            bDirty = true;
        }

        private void TextBoxSizeVertical_TextChanged(object sender, TextChangedEventArgs e)
        {
            bDirty = true;
        }
        private void BuildURL()
        {
            // validate values.
           // TextBoxSizeHorizontal.Text = "500";
           // TextBoxSizeVertical.Text = "500";

            //OperationResult result = MapProperties.MapParameters.Size.ValidateAndSaveSize(TextBoxSizeHorizontal.Text, TextBoxSizeVertical.Text);

            //if (result.bSuccess == false)
            //{
            //    if (result.strMessage != "")
            //    {
            //        MessageBox.Show(result.strMessage);
            //        return;
            //    }
            //}

        // was this ok? 
         //   MapProperties.MapParameters.Scale = (int)ComboBoxScale.SelectedValue;
            // MapProperties.LocationParameters.Zoom = (int)ComboBoxZoom.SelectedValue;


            string strGoogleMapsApiURL = "http://maps.googleapis.com/maps/api/staticmap?";

            if (OurRosterItem != null)
            {
                if (OurRosterItem.GeoLoc.lat == 0.0 && OurRosterItem.GeoLoc.lon == 0.0)
                    return;

                string strLatLon = String.Format("{0},{1}", OurRosterItem.GeoLoc.lat, OurRosterItem.GeoLoc.lon);

                // OurRosterItem.GeoString;

                if (bCenterOnMap)
                {
                    MapProperties.LocationParameters.Center = strLatLon;
                    MapProperties.LocationParameters.CenterGeoCoordinate = new GeoCoordinate(OurRosterItem.GeoLoc.lat, OurRosterItem.GeoLoc.lon, OurRosterItem.GeoLoc.TimeStamp);
                }
             
                //// Use previous center if it's not time to recenter again.
                //strGoogleMapsApiURL += String.Format("center={0}", strLatLon);
                
                //// if you don't send the center parameter what happens?

                //strGoogleMapsApiURL += String.Format("&zoom={0}", MapProperties.LocationParameters.Zoom);
                //strGoogleMapsApiURL += String.Format("&maptype={0}", MapProperties.MapParameters.MapType);
                //strGoogleMapsApiURL += String.Format("&size={0}x{1}", 
                //    MapProperties.MapParameters.Size.Horizontal, MapProperties.MapParameters.Size.Vertical);
                //    // "800", "800");
                //strGoogleMapsApiURL += BuildMarkerForRosterItem(OurRosterItem, "red", "");
                //// += String.Format("&markers=color:blue%7Clabel:B%7C{0}", strLatLon);
                //strGoogleMapsApiURL += String.Format("&sensor={0}", MapProperties.Sensor.ToString().ToLower());


                ////string strURLFromObject = MapProperties.URL;

                //strURL = strGoogleMapsApiURL;
                //if (MapProperties.URL == strURL)
                //{

                //}

                //// false");

                TextBoxURL.Text = MapProperties.URL;
                    //strURL;


                TextBoxTimeStamp.Text = String.Format("{0} ", OurRosterItem.GeoLoc.TimeStamp);
                TextBoxGeoLoc.Text = strLatLon;
                TextBlockTitleBuddy.Text = String.Format("{0}", OurRosterItem.JID.ToString());
                TextBlockTitleTimestamp.Text = String.Format("{0} ", OurRosterItem.GeoLoc.TimeStamp);



                // Build Javascript HTML
                MapProperties.MapParameters.Size.Horizontal = Convert.ToInt32(GridBrowser.ActualHeight * .98);
                MapProperties.MapParameters.Size.Vertical = Convert.ToInt32(GridBrowser.ActualWidth * .98);

                if (ComboBoxMapProvider.SelectedValue.ToString() == "Google Earth")
                    MapBuilder.MapProvider = MapProviderType.GoogleEarth;
                else if (ComboBoxMapProvider.SelectedValue.ToString() == "Google Maps")
                    MapBuilder.MapProvider = MapProviderType.GoogleMaps;

                if (MapProperties.LocationParameters.CenterGeoCoordinate == null)
                    MapProperties.LocationParameters.CenterGeoCoordinate = new GeoCoordinate(OurRosterItem.GeoLoc.lat, OurRosterItem.GeoLoc.lon,
                        OurRosterItem.GeoLoc.TimeStamp);

                strJavaScriptHtml = MapBuilder.BuildJavaScriptSourceCode(MapProperties, OurRosterItem);
                TextBoxBrowserSourceCode.Text = strJavaScriptHtml;

                // center=Williamsburg,Brooklyn,NY
                // &zoom=13
                // &size=800x800&
                // markers=color:blue%7Clabel:S%7C11211%7C11206%7C11222&sensor=false";
            }
        }

        //make sure they aer updated when I update mine!
        public MapBuilder MapBuilder = new MapBuilder();
        private string strJavaScriptHtml = "";


        private void ButtonLoadURL_Click(object sender, RoutedEventArgs e)
        {
            strURL = TextBoxURL.Text;
            LoadURL();

        }

        private void ButtonLoadLocation_Click(object sender, RoutedEventArgs e)
        {
            // ButtonLoadLocationAll_Click(sender, e);

            BuildURL();
            LoadURL();
        }

        private void ButtonLoadLocationAll_Click(object sender, RoutedEventArgs e)
        {
            string strURLUpdated = BuildURLForAllRosterItems();
            if (String.Compare(strURLUpdated, strURL, true) != 0)
            {
                strURL = strURLUpdated;
                LoadURL();
            }
        }

        private Dictionary<RosterItem, List<geoloc>> m_Paths = new Dictionary<RosterItem, List<geoloc>>();

        public Dictionary<RosterItem, List<geoloc>> Paths
        {
            get { return m_Paths; }
            set { m_Paths = value; }
        }

        private void HyperlinkRosterItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonViewMessages_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (OurRosterItem == null)
                return;

            try
            {
                MapProperties.LocationParameters.Zoom = (int)ComboBoxZoom.SelectedValue;
            }
            catch (Exception ex)
            {

            }

            //if (ComboBoxMapType.SelectedValue.ToString() == "roadmap")
            //    MapProperties.MapParameters.MapType = MapType.roadmap;
            //else if (ComboBoxMapType.SelectedValue.ToString() == "satellite")
            //    MapProperties.MapParameters.MapType = MapType.satellite;

            //else if (ComboBoxMapType.SelectedValue.ToString() == "terrain")
            //    MapProperties.MapParameters.MapType = MapType.terrain;

            //else if (ComboBoxMapType.SelectedValue.ToString() == "hybrid")
            //    MapProperties.MapParameters.MapType = MapType.hybrid;

            MapProperties.MapParameters.MapType = MapType.roadmap;
                // (MapType)ComboBoxMapType.SelectedValue;

            ButtonLoadLocation_Click(sender, e);
        }

        public void ZoomIn(int delta)
        {
            MapProperties.LocationParameters.Zoom += delta;

            if (MapProperties.LocationParameters.Zoom < 0)
                MapProperties.LocationParameters.Zoom = 0;
            if (MapProperties.LocationParameters.Zoom > 21)
                MapProperties.LocationParameters.Zoom = 21;

            ComboBoxZoom.SelectedItem = MapProperties.LocationParameters.Zoom;
            Refresh();
        }

        public void ZoomOut(int delta)
        {
            MapProperties.LocationParameters.Zoom -= delta;

            if (MapProperties.LocationParameters.Zoom < 0)
                MapProperties.LocationParameters.Zoom = 0;
            if (MapProperties.LocationParameters.Zoom > 21)
                MapProperties.LocationParameters.Zoom = 21;
            ComboBoxZoom.SelectedItem = MapProperties.LocationParameters.Zoom;
            Refresh();
        }

        private void MapImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                ZoomIn(1);
            else if (e.Delta < 0)
                ZoomOut(1);

            //MapProperties.LocationParameters.Zoom = Convert.ToInt32(Math.Max(MapProperties.LocationParameters.Zoom + (0.1F * e.Delta / 120.0F), 0.01F));

            //if (MapProperties.LocationParameters.Zoom < 0)
            //    MapProperties.LocationParameters.Zoom = 0;
            //if (MapProperties.LocationParameters.Zoom > 21)
            //    MapProperties.LocationParameters.Zoom = 21;

            //Refresh();
        }

        private void ButtonStartRecording_Click(object sender, RoutedEventArgs e)
        {
            KMLBuilder.IsRecordingKML = true;
            KMLBuilder.IsNotRecordingKML = false;

            IsRecordingKML = true;
            IsNotRecordingKML = false;

            if (KMLBuilder.Dictionary.ContainsKey(OurRosterItem) == false)
            {
                KMLBuilder.Dictionary.Add(OurRosterItem, new KMLBuilderForRosterItem() { IsRecordingKML = true, IsNotRecordingKML = false, CoordinateList = new List<GeoCoordinate>() });
            }
        }

        private bool IsRecordingKML = false;
        private bool IsNotRecordingKML = true;

        private void ButtonStopRecording_Click(object sender, RoutedEventArgs e)
        {
            KMLBuilder.IsNotRecordingKML = true;
            KMLBuilder.IsRecordingKML = false;

            IsRecordingKML = false;
            IsNotRecordingKML = true;
        }

        private void ButtonButtonLoadKMLFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonReloadBrowser_Click(object sender, RoutedEventArgs e)
        {
            LoadURL();
        }

        private void ComboBoxMapProvider_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb != null)
            {
                if (cb.SelectedValue.ToString() == "Google Earth")
                {
                    ComboBoxMapFeature.ItemsSource = MapBuilder.MapExamples["GoogleEarth"];

                    ComboBoxMapFeature.SelectedIndex = 0;
                    MapBuilder.MapProvider = MapProviderType.GoogleEarth;
                }
                else if (cb.SelectedValue.ToString() == "Google Maps")
                {
                    ComboBoxMapFeature.ItemsSource = MapBuilder.MapExamples["GoogleMaps"];

                    ComboBoxMapFeature.SelectedIndex = 0;
                    MapBuilder.MapProvider = MapProviderType.GoogleMaps;
                }
               
               
                Refresh();
            }
        }

        Dictionary<string, List<string>> MapExamples = new Dictionary<string,List<string>>();

        private void ComboBoxMapFeature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}