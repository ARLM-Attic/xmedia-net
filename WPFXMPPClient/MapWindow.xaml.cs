﻿///// Copyright (c) 2011 Brian Bonnett
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
            BitmapImage bmpImage = new BitmapImage();
            string mapURL = "http://maps.googleapis.com/maps/api/staticmap?" + "size=500x400&markers=size:mid%7Ccolor:red%7C" + location + "&zoom=" + zoom + "&maptype=" + mapType + "&sensor=false";

            bmpImage.BeginInit();
            bmpImage.UriSource = new Uri(mapURL);
            bmpImage.EndInit();

     ///       MapImage.Source = bmpImage;
        }

        private void ShowMapUsingLatLng()
        {
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
            // Use 88 to avoid values beyond 90 degrees of lat.
            if ((lat < 88))
            {
                if ((MapProperties.LocationParameters.Zoom == 15))
                {
                    lat += 0.003;
                }
                else if ((MapProperties.LocationParameters.Zoom > 15))
                {
                    diff = MapProperties.LocationParameters.Zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    lat += shift;
                }
                else
                {
                    diff = 15 - MapProperties.LocationParameters.Zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    lat += shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                lat = 90;
            }
        }

        private void MoveDown()
        {
            double diff = 0;
            double shift = 0;
            if ((lat > -88))
            {
                if ((MapProperties.LocationParameters.Zoom == 15))
                {
                    lat -= 0.003;
                }
                else if ((MapProperties.LocationParameters.Zoom > 15))
                {
                    diff = MapProperties.LocationParameters.Zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    lat -= shift;
                }
                else
                {
                    diff = 15 - MapProperties.LocationParameters.Zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    lat -= shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                lat = -90;
            }
        }

        private void MoveLeft()
        {
            double diff = 0;
            double shift = 0;
            // Use -178 to avoid negative values below -180.
            if ((lng > -178))
            {
                if ((MapProperties.LocationParameters.Zoom == 15))
                {
                    lng -= 0.003;
                }
                else if ((MapProperties.LocationParameters.Zoom > 15))
                {
                    diff = MapProperties.LocationParameters.Zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    lng -= shift;
                }
                else
                {
                    diff = 15 - MapProperties.LocationParameters.Zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    lng -= shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                lng = 180;
            }
        }

        private void MoveRight()
        {
            double diff = 0;
            double shift = 0;
            if ((lng < 178))
            {
                if ((zoom == 15))
                {
                    lng += 0.003;
                }
                else if ((zoom > 15))
                {
                    diff = zoom - 15;
                    shift = ((15 - diff) * 0.003) / 15;
                    lng += shift;
                }
                else
                {
                    diff = 15 - zoom;
                    shift = ((15 + diff) * 0.003) / 15;
                    lng += shift;
                }
                ShowMapUsingLatLng();
            }
            else
            {
                lng = -180;
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
                TextBlockTitle.Text = String.Format("Buddy Map - {0}", OurRosterItem.JID.ToString());
                this.Title = String.Format("Buddy Map - {0}", OurRosterItem.JID.ToString());
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
            MapProperties.MapParameters.Size = new SizeParameters() { Horizontal = 400, Vertical = 400 };
            TextBoxSizeHorizontal.Text = String.Format("{0}", 400);
            TextBoxSizeVertical.Text = String.Format("{0}", 400);

            MapProperties.MapParameters.Size.Horizontal = Convert.ToInt32(TextBoxSizeHorizontal.Text);
            MapProperties.MapParameters.Size.Vertical = Convert.ToInt32(TextBoxSizeVertical.Text);

            MapProperties.LocationParameters = new LocationParameters() { Zoom = (int)ComboBoxZoom.SelectedValue };

            //BuildURL();
            //LoadURL();
            if (XMPPClient != null)
            {
                XMPPClient.OnXMLReceived += new System.Net.XMPP.XMPPClient.DelegateString(XMPPClient_OnXMLReceived);
                XMPPClient.OnXMLSent += new System.Net.XMPP.XMPPClient.DelegateString(XMPPClient_OnXMLSent);
            }
            SetUpRosterItemNotifications();
            ButtonLoadLocation_Click(null, e);


        }


        void client_OnStateChanged(object sender, EventArgs e)
        {

        }

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

        void WriteSampleKML()
        {
            MyKML kml = new MyKML();
            kml.Document.Name = "brian";

            List<GeoCoordinate> Coords = new List<GeoCoordinate>();
            Coords.Add(new GeoCoordinate(-96.757835, 32.816929, DateTime.Now - TimeSpan.FromMinutes(10)));
            Coords.Add(new GeoCoordinate(-96.75758399999999, 32.815437, DateTime.Now - TimeSpan.FromMinutes(5)));
            Coords.Add(new GeoCoordinate(-96.757721, 32.817078, DateTime.Now));

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
    //            CoordinateList.Add(coord);
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

    //    public List<GeoCoordinate> CoordinateList
    //    {
    //        get { return m_listCoordinateList; }
    //        set { m_listCoordinateList = value; }
    //    }

    //    public int Count
    //    {
    //        get
    //        {
    //            return CoordinateList.Count;
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
                        RosterItem item = sender as RosterItem;

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
            //LoadImageFromURL();
            this.Dispatcher.Invoke(new Action(() =>
            {
                LoadImageFromURL();
                // WebBrowserMap.Navigate(strURL);
            })
            );


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
            ButtonLoadLocation_Click(null, null);
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
            OperationResult result = MapProperties.MapParameters.Size.ValidateAndSaveSize(TextBoxSizeHorizontal.Text, TextBoxSizeVertical.Text);

            if (result.bSuccess == false)
            {
                if (result.strMessage != "")
                {
                    MessageBox.Show(result.strMessage);
                    return;
                }
            }

        // was this ok? 
            MapProperties.MapParameters.Scale = (int)ComboBoxScale.SelectedValue;
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
                }
             
                // Use previous center if it's not time to recenter again.
                strGoogleMapsApiURL += String.Format("center={0}", strLatLon);
                
                // if you don't send the center parameter what happens?

                strGoogleMapsApiURL += String.Format("&zoom={0}", MapProperties.LocationParameters.Zoom);
                strGoogleMapsApiURL += String.Format("&maptype={0}", MapProperties.MapParameters.MapType);
                strGoogleMapsApiURL += String.Format("&size={0}x{1}", 
                    MapProperties.MapParameters.Size.Horizontal, MapProperties.MapParameters.Size.Vertical);
                    // "800", "800");
                strGoogleMapsApiURL += BuildMarkerForRosterItem(OurRosterItem, "red", "");
                // += String.Format("&markers=color:blue%7Clabel:B%7C{0}", strLatLon);
                strGoogleMapsApiURL += String.Format("&sensor={0}", MapProperties.Sensor.ToString().ToLower());


                //string strURLFromObject = MapProperties.URL;

                strURL = strGoogleMapsApiURL;
                if (MapProperties.URL == strURL)
                {

                }

                // false");

                TextBoxURL.Text = MapProperties.URL;
                    //strURL;


                TextBoxTimeStamp.Text = String.Format("{0} ", OurRosterItem.GeoLoc.TimeStamp);
                TextBoxGeoLoc.Text = strLatLon;
                
                // center=Williamsburg,Brooklyn,NY
                // &zoom=13
                // &size=800x800&
                // markers=color:blue%7Clabel:S%7C11211%7C11206%7C11222&sensor=false";

            }

        }

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

            MapProperties.MapParameters.MapType = (MapType)ComboBoxMapType.SelectedValue;

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
            IsRecordingKML = true;
            IsNotRecordingKML = false;
        }

        private bool IsRecordingKML = false;
        private bool IsNotRecordingKML = true;

        private void ButtonStopRecording_Click(object sender, RoutedEventArgs e)
        {
            IsRecordingKML = false;
            IsNotRecordingKML = true;
        }

        private void ButtonButtonLoadKMLFile_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}