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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Net.XMPP;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

using System.Windows.Interop;

namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

       
        public XMPPClient XMPPClient = new XMPPClient();
        List<System.Net.XMPP.XMPPAccount> AllAccounts = null;

        PrivacyService PrivService = null;
        AudioMuxerWindow AudioMuxerWindow = new AudioMuxerWindow();
        BrowserPopupWindow MapBrowserWindow = new BrowserPopupWindow();
        MapWindow MapWindow = new MapWindow();

        private void SurfaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Option.Load();

            PrivService = new PrivacyService(XMPPClient);
            PrivService.OnMustClearUserHistory += new DelegateRosterItemAction(PrivService_OnMustClearUserHistory);
            PrivService.OnMustHideMyChatWindow += new DelegateRosterItemAction(PrivService_OnMustHideMyChatWindow);

            SensorMessageBuilder = new SensorLibrary.SensorMessagBuilder(XMPPClient);
            SensorMessageBuilder.OnSensorEvent += SensorMessageBuilder_OnSensorEvent;

            //this.RectangleConnect.DataContext = this;
            this.DataContext = XMPPClient;
            this.ListBoxRoster.DataContext = this;
            this.CheckBoxSHowAll.DataContext = this;
           
            CollectionViewSource source = FindResource("SortedRosterItems") as CollectionViewSource;
            source.Source = XMPPClient.RosterItems;

            //this.ListBoxRoster.ItemsSource = XMPPClient.RosterItems;
            XMPPClient.OnRetrievedRoster += new EventHandler(RetrievedRoster);
            XMPPClient.OnRosterItemsChanged += new EventHandler(RosterChanged);
            XMPPClient.OnStateChanged += new EventHandler(XMPPStateChanged);
            XMPPClient.OnNewConversationItem += new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(XMPPClient_OnNewConversationItem);
            XMPPClient.OnUserSubscriptionRequest += new System.Net.XMPP.XMPPClient.DelegateShouldSubscribeUser(XMPPClient_OnUserSubscriptionRequest);

            XMPPClient.FileTransferManager.OnNewIncomingFileTransferRequest += new FileTransferManager.DelegateIncomingFile(FileTransferManager_OnNewIncomingFileTransferRequest);
            XMPPClient.FileTransferManager.OnTransferFinished += new FileTransferManager.DelegateDownloadFinished(FileTransferManager_OnTransferFinished);


            AudioMuxerWindow.RegisterXMPPClient(XMPPClient);
            MapBrowserWindow.RegisterXMPPClient(XMPPClient);
            MapBrowserWindow.Closed += new EventHandler(browserWin_Closed);
            MapWindow.Closed += new EventHandler(mapWin_Closed);
            MapWindow.XMPPClient = XMPPClient;

            SendRawXMLWindow.SetXMPPClient(XMPPClient);

            //AppBarFunctions.SetAppBar(this, ABEdge.Right);
            SetDesktopBackgroundEffects();

 
        }

        void SensorMessageBuilder_OnSensorEvent(SensorLibrary.SensorEvent sevent, RosterItem item, XMPPClient client)
        {
            /// New sensor event, report it
            /// 
            this.Dispatcher.Invoke(new Action(() =>
            {
                string strMessage = string.Format("Sensor Event {0} from {1}, Value {2}", sevent.Event, sevent.Source, sevent.Value);
                item.Conversation.AddMessage(new TextMessage() { From = item.JID, To=XMPPClient.JID, MessageType= TextMessageType.ChatMessage, Received=DateTime.Now, Sent=false, Message=strMessage});
                
            })
            );

        }

        SensorLibrary.SensorMessagBuilder SensorMessageBuilder = null;

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
         //   SetDesktopBackgroundEffects();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;      // width of left border that retains its size
            public int cxRightWidth;     // width of right border that retains its size
            public int cyTopHeight;      // height of top border that retains its size
            public int cyBottomHeight;   // height of bottom border that retains its size
        };

     
        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);

        [Flags]
        private enum DwmBlurBehindFlags : uint
        {
            /// <summary>
            /// Indicates a value for fEnable has been specified.
            /// </summary>
            DWM_BB_ENABLE = 0x00000001,

            /// <summary>
            /// Indicates a value for hRgnBlur has been specified.
            /// </summary>
            DWM_BB_BLURREGION = 0x00000002,

            /// <summary>
            /// Indicates a value for fTransitionOnMaximized has been specified.
            /// </summary>
            DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public DwmBlurBehindFlags dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        [DllImport("dwmapi.dll")]
        private static extern IntPtr DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);

   
        void SetDesktopBackgroundEffects()
        {
            try
            {
                
                // Obtain the window handle for WPF application
                IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

                // Get System Dpi
                System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                float DesktopDpiX = desktop.DpiX;
                float DesktopDpiY = desktop.DpiY;

                DWM_BLURBEHIND blur = new DWM_BLURBEHIND();
                blur.fEnable = true;
                blur.dwFlags = DwmBlurBehindFlags.DWM_BB_ENABLE;
                blur.hRgnBlur = IntPtr.Zero;
                blur.fTransitionOnMaximized = true;
             //   DwmEnableBlurBehindWindow(mainWindowSrc.Handle, ref blur);

                //// Set Margins
                //MARGINS margins = new MARGINS();

                //// Extend glass frame into client area
                //// Note that the default desktop Dpi is 96dpi. The  margins are
                //// adjusted for the system Dpi.
                //margins.cxLeftWidth = 0; //Convert.ToInt32(5 * (DesktopDpiX / 96));
                //margins.cxRightWidth = 0; //Convert.ToInt32(5 * (DesktopDpiX / 96));
                //margins.cyTopHeight = 0;// Convert.ToInt32(((int)ActualHeight + 5) * (DesktopDpiX / 96));
                //margins.cyBottomHeight = 25; // Convert.ToInt32(5 * (DesktopDpiX / 96));

                //int hr = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                ////
                //if (hr < 0)
                //{
                //    //DwmExtendFrameIntoClientArea Failed
                //}
            }
            // If not Vista, paint background white.
            catch (DllNotFoundException)
            {
               // Application.Current.MainWindow.Background = Brushes.White;
            }

        }

        void PrivService_OnMustHideMyChatWindow(RosterItem item, XMPPClient client)
        {
            this.Dispatcher.Invoke( new Action(() => 
                {
                    ChatWindow win = FindOrCreateChatWIndow(item);
                    if ((win != null) && (win.IsLoaded == true))
                        win.WindowState = System.Windows.WindowState.Minimized;
                } ) 
            );
        }

        void PrivService_OnMustClearUserHistory(RosterItem item, XMPPClient client)
        {
            this.Dispatcher.Invoke(new Action(() =>
                {
                    ChatWindow win = FindOrCreateChatWIndow(item);
                    if ((win != null) && (win.IsLoaded == true))
                        win.Clear();
                })
            );
        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            if (SendRawXMLWindow.IsLoaded == true)
                SendRawXMLWindow.Close();
            if (AudioMuxerWindow.IsLoaded == true)
                AudioMuxerWindow.Close();
            if (MapBrowserWindow.IsLoaded == true)
                MapBrowserWindow.Close();
            base.OnClosed(e);
            Application.Current.Shutdown();
        }


        private void HyperlinkConnect_Click(object sender, RoutedEventArgs e)
        {
            if (XMPPClient.XMPPState == XMPPState.Unknown)
            {
                XMPPClient.AutoReconnect = true;

                LoginWindow loginwin = new LoginWindow();
                loginwin.ActiveAccount = XMPPClient.XMPPAccount;
                loginwin.AllAccounts = AllAccounts;
                if (loginwin.ShowDialog() == false)
                    return;
                if (loginwin.ActiveAccount == null)
                {
                    MessageBox.Show("Login window returned null account", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                XMPPClient.XMPPAccount = loginwin.ActiveAccount;
                AllAccounts = loginwin.AllAccounts;

                XMPPClient.XMPPAccount.Capabilities = new Capabilities();
                XMPPClient.XMPPAccount.Capabilities.Node = "http://xmedianet.codeplex.com/wpfclient/caps";
                XMPPClient.XMPPAccount.Capabilities.Version = "1.0";
                XMPPClient.XMPPAccount.Capabilities.Extensions = "voice-v1 video-v1 camera-v1"; /// google talk capabilities
                //XMPPClient.XMPPAccount.Capabilities.Extensions = "voice-v1"; /// google talk capabilities
                
                //XMPPClient.XMPPAccount.Capabilities.Node = "http://www.apple.com/ichat/caps";
                //XMPPClient.XMPPAccount.Capabilities.Version = "800";
                //XMPPClient.XMPPAccount.Capabilities.Extensions = "ice recauth rdserver maudio audio rdclient mvideo auxvideo rdmuxing avcap avavail video"; /// mac iChat capabilities

                XMPPClient.FileTransferManager.AutoDownload = true;
                XMPPClient.Connect();

            }
            else if (XMPPClient.XMPPState > XMPPState.Connected)
            {
                XMPPClient.Disconnect();
            }

        }

        void SaveAccounts()
        {

            string strPath = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string strFileName = string.Format("{0}\\{1}", strPath, "xmppcred.item");
            FileStream location = null;

            try
            {
                location = new FileStream(strFileName, System.IO.FileMode.Create);
                DataContractSerializer ser = new DataContractSerializer(typeof(List<XMPPAccount>));
                ser.WriteObject(location, AllAccounts);
            }
            catch (Exception)
            {
            }
            finally
            {
                if (location != null)
                    location.Close();
            }
            
        }

        public Brush ConnectedStateBrush
        {
            get
            {
                if (XMPPClient.XMPPState == XMPPState.Ready)
                   return new SolidColorBrush(Color.FromArgb(0xFF, 0x22, 0xFF, 0x22));
                else
                    return new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
            }
            set
            {
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if ((e.Key == Key.D) && ((Keyboard.Modifiers&ModifierKeys.Control) == ModifierKeys.Control) )
            {
                if (SendRawXMLWindow.IsLoaded == false)
                    SendRawXMLWindow.Show();
                else if (this.SendRawXMLWindow.Visibility == System.Windows.Visibility.Hidden)
                    this.SendRawXMLWindow.Visibility = System.Windows.Visibility.Visible;

                SendRawXMLWindow.Activate();
            }
            else if ((e.Key == Key.M) && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                ShowMapBrowserWindow();
            }
            base.OnPreviewKeyDown(e);
        }

        SendRawXMLWindow SendRawXMLWindow = new SendRawXMLWindow();

        void XMPPClient_OnUserSubscriptionRequest(PresenceMessage pres)
        {
            Dispatcher.Invoke(new System.Net.XMPP.XMPPClient.DelegateShouldSubscribeUser(DoOnUserSubscriptionRequest), pres);
            
        }

        void DoOnUserSubscriptionRequest(PresenceMessage pres)
        {
            MessageBoxResult result = MessageBox.Show(string.Format("User '{0}; wants to see your presence.  Allow?", pres.From), "Allow User To See You?", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                XMPPClient.AcceptUserPresence(pres, "", "");
            }
            else
            {
                XMPPClient.DeclineUserPresence(pres);
            }
            
        }

        public Dictionary<string, ChatWindow> ChatWindows = new Dictionary<string, ChatWindow>();
        public Dictionary<string, BrowserPopupWindow> BrowserPopupWindows = new Dictionary<string, BrowserPopupWindow>();
        public Dictionary<string, MapWindow> MapWindows = new Dictionary<string, MapWindow>();

        void XMPPClient_OnNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {
            // Load any old messages if we haven't yet
            Dispatcher.Invoke(new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(DoNewConversationItem), item, bReceived, msg);
        }

        void DoNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {

            ChatWindow.SaveConversation(item);

            if (bReceived == false)
                return;

            ChatWindow windowfound = null;
            if (ChatWindows.ContainsKey(item.JID.BareJID) == true)
            {
                windowfound = ChatWindows[item.JID.BareJID];
                
            }
            
            if ( (windowfound == null) || (windowfound.Visibility != System.Windows.Visibility.Visible))
            {
                /// Make a sound and notify this window
                /// 

                if ((msg.Thread.ToLower() == "quiet" || msg.Thread.ToLower() == "silent" || msg.Thread.ToLower() == "nospeak") == false)
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer("Sounds/ding.wav");
                    player.Play();
                }

                if ((msg.Thread.ToLower() == "noflash" || msg.Thread.ToLower() == "unseen") == false)
                {
                    IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                    FlashWindow(windowHandle, true);
                }
            }
        }

        ChatWindow FindOrCreateChatWIndow(RosterItem item)
        {
            if (item == null)
                return null;

            if (ChatWindows.ContainsKey(item.JID.BareJID) == true)
            {
                ChatWindow exwin = ChatWindows[item.JID.BareJID];
                return exwin;
            }
            else
            {
                ChatWindow win = new ChatWindow();
                win.XMPPClient = this.XMPPClient;
                win.OurRosterItem = item;
                win.Closed += new EventHandler(win_Closed);
                ChatWindows.Add(item.JID.BareJID, win);
                win.Show();
                return win;
            }
        }

        MapWindow FindOrCreateMapWIndow(RosterItem item)
        {
            if (item == null)
                return null;

            if (MapWindows.ContainsKey(item.JID.BareJID) == true)
            {
                MapWindow exwin = MapWindows[item.JID.BareJID];
                return exwin;
            }
            else
            {
                MapWindow win = new MapWindow();
                win.XMPPClient = this.XMPPClient;
                win.OurRosterItem = item;
                win.Closed += new EventHandler(win_Closed);
                MapWindows.Add(item.JID.BareJID, win);
                win.Show();
                return win;
            }
        }


        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        public void RosterChanged(object obj, EventArgs arg)
        {
            this.Dispatcher.Invoke(new DelegateVoid(SetRoster));
        }

        public delegate void DelegateVoid();
        public void RetrievedRoster(object obj, EventArgs arg)
        {
            // Load all our our existing conversations

            foreach (RosterItem item in XMPPClient.RosterItems)
            {
                // Build map window and place in dictionary of map windows. 
                // Hope this isn't a memory hog; would prefer loading when user's map icon is clicked on.
                // Having trouble loading the window now though.

              

                if (item.HasLoadedConversation == false)
                {
                    item.HasLoadedConversation = true;

                    string strFilename = string.Format("{0}_conversation.item", item.JID.BareJID);

                    //string strPath = string.Format("{0}\\conversations", Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
                    string strPath = string.Format("{0}\\conversations\\{1}", Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), XMPPClient.JID.BareJID.Replace("@", ""));

                    if (System.IO.Directory.Exists(strPath) == false)
                        System.IO.Directory.CreateDirectory(strPath);

                    string strFullFileName = string.Format("{0}\\{1}", strPath, strFilename);
                    FileStream location = null;
                    if (File.Exists(strFullFileName) == true)
                    {
                        try
                        {

                            location = new FileStream(strFullFileName, System.IO.FileMode.Open);
                            DataContractSerializer ser = new DataContractSerializer(typeof(System.Net.XMPP.Conversation));

                            item.Conversation = ser.ReadObject(location) as System.Net.XMPP.Conversation;
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            if (location != null)
                                location.Close();
                        }
                    }
                }
            }

            this.Dispatcher.Invoke(new DelegateVoid(SetRoster));
        }

        private bool m_bShowAll = false;
        /// <summary>
        /// Used in GUI's to show all roster items, even those not online
        /// </summary>
        public bool ShowAll
        {
            get 
            { 
                return m_bShowAll; 
            }
            set
            {
                if (m_bShowAll != value)
                {
                    m_bShowAll = value;
                    FirePropertyChanged("ShowAll");
                }
            }
        }

        private void AddPresenceUpdateLine()
        {

        }

        void SetRoster()
        {
        //    CollectionViewSource source = FindResource("SortedRosterItems") as CollectionViewSource;
          //  source.View.Refresh();

           AddPresenceUpdateLine();
           ///foreach (RosterItem item in XMPPClient.RosterItems)
         for (int i = 0; i < XMPPClient.RosterItems.Count; i++)
           {
               RosterItem item = XMPPClient.RosterItems[i];
                if (BuildMapBrowserWindowForRosterItem(item))
                {

                }
                else
                {

                }
            }
        }

        public void XMPPStateChanged(object obj, EventArgs arg)
        {
            Dispatcher.Invoke(new DelegateVoid(HandleStateChanged));
        }

        void HandleStateChanged()
        {
            

            if (XMPPClient.XMPPState == XMPPState.Connected)
            {
                
            }
            else if (XMPPClient.XMPPState == XMPPState.Unknown)
            {
                this.FirePropertyChanged("ConnectedStateBrush");
                ComboBoxPresence.IsEnabled = false;
                ButtonAddBuddy.IsEnabled = false;
                SaveAccounts();

                if (AudioMuxerWindow.IsLoaded == true)
                    AudioMuxerWindow.CloseAllSessions();
            }
            else if (XMPPClient.XMPPState == XMPPState.Ready)
            {
                this.FirePropertyChanged("ConnectedStateBrush");
                ComboBoxPresence.IsEnabled = true;
                ButtonAddBuddy.IsEnabled = true;
                this.ImageAvatar.Source = XMPPClient.Avatar;

                XMPPClient.StartPinging(120000);
                //XMPPClient.SetGeoLocation(0, 0);


                //XMPPClient.SetGeoLocation(32.234, -97.3453);
            }
            else if (XMPPClient.XMPPState == XMPPState.AuthenticationFailed)
            {
                if (MessageBox.Show(string.Format("Incorrect username or password.  Would you like to create user '{0}'?", XMPPClient.UserName), "Authentication Failed", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(CreateAccountAsync));
                }
                else
                {
                    XMPPClient.Disconnect();
                }
            }
            else
            {
            }
        }

        void CreateAccountAsync(object obj)
        {
            bool bCreated = XMPPClient.CreateAccount(XMPPClient.UserName, XMPPClient.Password, "");
            if (bCreated == true)
            {
                //MessageBox.Show("Created User!", "New User Added", MessageBoxButton.OK, MessageBoxImage.None);
                XMPPClient.Disconnect();
                XMPPClient.Connect();
            }
            else
            {
                //MessageBox.Show("Failed to create user!", "User may already exists or server cannot create new users", MessageBoxButton.OK, MessageBoxImage.None);
                XMPPClient.Disconnect();
            }

        }

        private void HyperlinkRosterItem_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            if (ChatWindows.ContainsKey(item.JID.BareJID) == true)
            {
                ChatWindow exwin = ChatWindows[item.JID.BareJID];
                exwin.WindowState = System.Windows.WindowState.Normal;
                exwin.Activate();
                return;
            }

            ChatWindow win = new ChatWindow();
            win.XMPPClient = this.XMPPClient;
            win.OurRosterItem = item;
            win.Closed += new EventHandler(win_Closed);
            ChatWindows.Add(item.JID.BareJID, win);
            win.Show();

            //NavigationService.Navigate(new Uri(string.Format("/ChatPage.xaml?JID={0}", item.JID), UriKind.Relative)); 

        }

        void win_Closed(object sender, EventArgs e)
        {
            ((ChatWindow)sender).Closed -= new EventHandler(win_Closed); 
            string strRemove = null;
            foreach (string strKey in ChatWindows.Keys)
            {
                if (ChatWindows[strKey] == sender)
                {
                    strRemove = strKey;
                    break;
                }
            }
            if (strRemove != null)
                ChatWindows.Remove(strRemove);
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            //NavigationService.GoBack();
        }

        private void ButtonAddBuddy_Click(object sender, RoutedEventArgs e)
        {
            AddNewRosterItemWindow win = new AddNewRosterItemWindow();
            win.client = this.XMPPClient;
            win.ShowDialog();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
                this.DragMove();

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



       

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strName));
        }

        #endregion

        private void ImageAvatar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (XMPPClient.XMPPState != XMPPState.Ready)
                return;

           
            VCardWindow win = new VCardWindow();
            win.vcard = XMPPClient.vCard;
            win.XMPPClient = this.XMPPClient;
            win.CurrentSource = XMPPClient.Avatar;

            if (win.ShowDialog() == true)
            {
                XMPPClient.UpdatevCard();
                //XMPPClient.SetAvatar(bData, nWidth, nHeight, strContentType);
            }
        }

        private void ButtonViewMessages_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            if (ChatWindows.ContainsKey(item.JID.BareJID) == true)
            {
                ChatWindow exwin = ChatWindows[item.JID.BareJID];
                exwin.Activate();
                return;
            }

            ChatWindow win = new ChatWindow();
            win.XMPPClient = this.XMPPClient;
            win.OurRosterItem = item;
            win.Closed += new EventHandler(win_Closed);
            ChatWindows.Add(item.JID.BareJID, win);
            win.Show();

        }


        FileTransferWindow FileTransferWindow  = new FileTransferWindow();
        private void ButtonTransfers_Click(object sender, RoutedEventArgs e)
        {
            FileTransferWindow.XMPPClient = this.XMPPClient;

            ShowFileTransfer();
        }

        public void ShowFileTransfer()
        {
            if (FileTransferWindow.IsLoaded == false)
            {
                FileTransferWindow = new FileTransferWindow();
                FileTransferWindow.XMPPClient = this.XMPPClient;
                FileTransferWindow.Show();
                IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(FileTransferWindow).Handle;
                FlashWindow(windowHandle, true);
            }
            else
            {
                FileTransferWindow.Activate();
                IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(FileTransferWindow).Handle;
                FlashWindow(windowHandle, true);
            }
        }

        void FileTransferManager_OnTransferFinished(FileTransfer trans)
        {
            /// Save the file automatically
            /// 
            /// save this file to our directory
            /// 
            if ( (trans != null) && (trans.DataStream != null) && (trans.DataStream.Length > 0) )
            {
                //string strDir = string.Format("{0}\\XMPPFiles\\", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
                string strDir = Option.Options.FileTransferDirectory;
                if (Directory.Exists(strDir) == false)
                    Directory.CreateDirectory(strDir);
                string strFullFileName = System.IO.Path.Combine(strDir, trans.FileName);
                if (File.Exists(strFullFileName) == false)
                {
                    FileStream stream = new FileStream(strFullFileName, FileMode.Create, FileAccess.Write);
                    byte[] bStreamCopy = new byte[128000];
                    while (trans.DataStream.Position < trans.DataStream.Length)
                    {
                        int nRead = trans.DataStream.Read(bStreamCopy, 0, bStreamCopy.Length);
                        stream.Write(bStreamCopy, 0, nRead);
                    }
                    stream.Close();
                    trans.FileName = strFullFileName;
                } 
                else
                {
                    trans.FileName = strFullFileName;
                }
                
                trans.Close();
            }
        }

        void FileTransferManager_OnNewIncomingFileTransferRequest(FileTransfer trans, RosterItem itemfrom)
        {
            this.Dispatcher.Invoke(new System.Net.XMPP.FileTransferManager.DelegateIncomingFile(SafeIncomingFile), trans, itemfrom);

        }
        void SafeIncomingFile(FileTransfer trans, RosterItem itemfrom)
        {

            ButtonTransfers_Click(this, new RoutedEventArgs());
            ///// New incoming file request... accept or reject it... pass this off to the appropriate window
            ///// 
            //ChatWindow window = FindOrCreateChatWIndow(itemfrom);
            //window.IncomingFileRequest(strRequestId, strFileName, nFileSize);
        }

        void XMPPClient_OnDownloadFinished(string strRequestId, string strLocalFileName, RosterItem itemfrom)
        {
        }


        private void TextBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
           
            /// Should save our accounts here
        }

        private void ButtonSetStatus_Click(object sender, RoutedEventArgs e)
        {
            if (XMPPClient.XMPPState == XMPPState.Ready)
            {

                ComboBoxItem item = this.ComboBoxPresence.SelectedItem as ComboBoxItem;
                XMPPClient.PresenceStatus.PresenceType = (PresenceType) item.Content;
                XMPPClient.PresenceStatus.Status = this.TextBoxStatus.Text;
                XMPPClient.UpdatePresence();
            }
        }

        public void StartAudioCall(string strFullJID)
        {
            ShowAudioMuxer();

            AudioMuxerWindow.InitiateOrShowCallTo(strFullJID);
        }

        private void ButtonMediaSessions_Click(object sender, RoutedEventArgs e)
        {
            ShowAudioMuxer();
        }

        void ShowAudioMuxer()
        {
            if (AudioMuxerWindow.IsLoaded == false)
            {
                AudioMuxerWindow.Show();
            }
            else
            {
                AudioMuxerWindow.Visibility = System.Windows.Visibility.Visible;
            }
        }

        bool ShowMapBrowserWindow()
        {
            if (MapBrowserWindow.IsLoaded == false)
            {
                MapBrowserWindow.RegisterXMPPClient(XMPPClient);
                MapBrowserWindow.Show();
            }
            else
            {
                MapBrowserWindow.Visibility = System.Windows.Visibility.Visible;
            }
            return true;
        }

        bool ShowMapWindow()
        {
            if (MapWindow.IsLoaded == false)
            {
                MapWindow.XMPPClient = XMPPClient;
                //MapWindow.RegisterXMPPClient(XMPPClient);
                MapWindow.Show();
            }
            else
            {
                MapWindow.Visibility = System.Windows.Visibility.Visible;
            }
            return true;
        }

        private void MenuItemSubscribe_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            if (XMPPClient.XMPPState != XMPPState.Ready)
                return;

            XMPPClient.PresenceLogic.SubscribeToPresence(item.JID);

        }

        private void MenuItemUnsubscribe_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            if (XMPPClient.XMPPState != XMPPState.Ready)
                return;

            XMPPClient.PresenceLogic.UnsubscribeToPresence(item.JID);

        }

        //private void ButtonShowOnMap_Click(object sender, RoutedEventArgs e)
        //{
        //    /// Start audio on this resource... First find the resource that can do audio.
        //    /// If there are multiple instances that can do audio, prompt the user for which one
        //    /// 
        //    RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
        //    if (item == null)
        //        return;

        //    // Eventually, we might want to use instances, but for now, just the RosterItem.
        //    // RosterItemPresenceInstance inst = item.FindAudioPresenceInstance();
        //    // if (inst != null)
        //    //{
        //    if (ShowMapBrowserWindow())
        //        MapBrowserWindow.AddRosterItem(item);
        //    // AudioMuxerWindow.InitiateOrShowCallTo(inst.FullJID);
        //    //}
        //}


        //private void ButtonViewMap_Click(object sender, RoutedEventArgs e)
        //{
        //    RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
        //    if (item == null)
        //        return;

        //    if (BrowserPopupWindows.ContainsKey(item.JID.BareJID) == true)
        //    {
        //        BrowserPopupWindow exwin = BrowserPopupWindows[item.JID.BareJID];
        //        exwin.Activate();
        //        return;
        //    }

        //    BrowserPopupWindow win = new BrowserPopupWindow();
        //    win.XMPPClient = this.XMPPClient;
        //    win.OurRosterItem = item;
        //    win.Closed += new EventHandler(win_Closed);
        //    BrowserPopupWindows.Add(item.JID.BareJID, win);
        //    win.Show();






        //    //BrowserPopupWindow.XMPPClient = this.XMPPClient;

        //    //RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
        //    //if (item == null)
        //    //    return;

        //    //ShowBrowserPopupWindow(item);
        //}

        //public void ShowBrowserPopupWindow(RosterItem item)
        //{
        //    if (BrowserPopupWindow.IsLoaded == false)
        //    {
        //        BrowserPopupWindow = new BrowserPopupWindow();
        //        BrowserPopupWindow.XMPPClient = this.XMPPClient;
        //        BrowserPopupWindow.OurRosterItem = item;

        //        //// if a buddy is highlighted, center the map on them (or only show them),
        //        //// or you are going to track/create KML for them
        //        //if (ListBoxRoster.SelectedItems.Count >= 0)
        //        //{
        //        //    // can't multi-select so we will only be able to select one buddy at a time
        //        //    BrowserPopupWindow.OurRosterItem = ListBoxRoster.SelectedItem as RosterItem;
        //        //}
             
        //        BrowserPopupWindow.Show();
                
        //    }
        //    else
        //    {
        //        //if (ListBoxRoster.SelectedItems.Count >= 0)
        //        //{
        //        //    // can't multi-select
        //        //    BrowserPopupWindow.OurRosterItem = ListBoxRoster.SelectedItem as RosterItem;
        //        //}
        //        BrowserPopupWindow.OurRosterItem = item;
                
        //        BrowserPopupWindow.Activate();
        //        IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(BrowserPopupWindow).Handle;
        //        FlashWindow(windowHandle, true);
        //    }
        //}

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            /// Find our chat text box
            /// 
            Grid g = VisualTreeHelper.GetParent( ((FrameworkElement)sender)) as Grid;
            if (g == null)
                return;
            foreach (UIElement elem in g.Children)
            {
                if (elem is TextBox)
                {
                    TextBox text = elem as TextBox;
                    XMPPClient.SendChatMessage(text.Text, item.LastFullJIDToGetMessageFrom);
                    text.Text = "";
                    return;
                }
            }

            
        }

        private void TextBoxChatToSend_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
                if (item == null)
                    return;

                TextBox text = sender as TextBox;
                XMPPClient.SendChatMessage(text.Text, item.LastFullJIDToGetMessageFrom);
                text.Text = "";
            }
            else
            {
                RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
                if (item == null)
                    return;
                item.HasNewMessages = false;
            }

        }

        private void ButtonExpandChat_Checked(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            this.ListBoxRoster.SelectedItem = item;
        }

        private void ButtonExpandChat_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ListBoxRoster.SelectedItem = null;

        }

        private void ButtonClearMessages_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            item.Conversation.Clear();
            ChatWindow.SaveConversation(item);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HyperlinkRosterItem_Click(sender, new RoutedEventArgs());
        }

        private void ButtonStartAudio_Click(object sender, RoutedEventArgs e)
        {
            /// Start audio on this resource... First find the resource that can do audio.
            /// If there are multiple instances that can do audio, prompt the user for which one
            /// 
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            RosterItemPresenceInstance inst = item.FindAudioPresenceInstance();
            if (inst != null)
            {
                ShowAudioMuxer();
                AudioMuxerWindow.InitiateOrShowCallTo(inst.FullJID);
            }
        }
    
        private void ButtonStartVideo_Click(object sender, RoutedEventArgs e)
        {
            /// Start audio on this resource... First find the resource that can do audio.
            /// If there are multiple instances that can do audio, prompt the user for which one
            /// 
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            RosterItemPresenceInstance inst = item.FindAudioPresenceInstance();
            if (inst != null)
            {
               // ShowAudioMuxer();
                //AudioMuxerWindow.InitiateOrShowCallTo(inst.FullJID);
            }
        }

        private void ListBoxRoster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RosterItem item = this.ListBoxRoster.SelectedItem as RosterItem;
            if (item == null)
                return;

            item.HasNewMessages = false;
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            foreach (RosterItem item in this.ListBoxRoster.SelectedItems)
            {
                item.HasNewMessages = false;
            }
        }

        private void TextBoxChatToSend_GotFocus(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;
            item.HasNewMessages = false;
        }

        private void DialogControlLastMessage_GotFocus(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;
            item.HasNewMessages = false;
        }

        private void ButtonOptions_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow opt = new OptionsWindow();
            opt.Owner = this;
            opt.ShowDialog();
            Option.Options.SetToXMPPClient(this.XMPPClient);
        }

        private void ButtonSendFromCamera_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            ChatWindow.SendPhotoCapture(this.XMPPClient, new string[] { item.LastFullJIDToGetMessageFrom });
        }

        private void ButtonSendFile_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            ChatWindow.SendFile(this.XMPPClient, new string[] { item.LastFullJIDToGetMessageFrom });
        }

        private void ButtonSendScreenCapture_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            ChatWindow.SendScreenCapture(this.XMPPClient, new string[] { item.LastFullJIDToGetMessageFrom });
        }

        private void ButtonRosterItem_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkRosterItem_Click(sender, e);
        }


        private void ButtonMap_Click(object sender, RoutedEventArgs e)
        {
            ShowMapBrowserWindow();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void ButtonViewMap_Click2(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
                {

                    // 2 options here. 
                    if (Option.Options.UseLegacyMapWindow == true)
                    {
                        if (Option.Options.SingleRosterItemMap == true)
                            ButtonViewMap_Click(sender, e);
                        else
                            ButtonViewMapConsolidated_Click(sender, e);
                    }
                    else
                    {
                        if (Option.Options.SingleRosterItemMap == true)
                            ButtonViewMapBrowserWindow_Click(sender, e);
                        else
                            ButtonViewMapBrowserWindowConsolidatedMap_Click(sender, e);
                    }
                })
            );
        }

        // This is the one map per user method
        private void ButtonViewMapBrowserWindow_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            if (BrowserPopupWindows.ContainsKey(item.JID.BareJID) == true)
            {
                BrowserPopupWindow exwin = BrowserPopupWindows[item.JID.BareJID];
               /* if (exwin.Visibility == System.Windows.Visibility.Collapsed)
                    exwin.Visibility = System.Windows.Visibility.Visible;
                else */
                {
                    try
                    {
                        exwin.Show();
                        exwin.Focus();
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error showing map window: " + ex.Message);
                    }
                }
/*                exwin.AddRosterItem(item);

                exwin.AddRosterItemBody();

                //exwin.Title = "Buddy Map - " + item.JID.BareJID;

                //exwin.Activate();
              
                */

                IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(exwin).Handle;
                FlashWindow(windowHandle, true);
                return;
            }

            BrowserPopupWindow mapWin = new BrowserPopupWindow();
            mapWin.XMPPClient = this.XMPPClient;
            mapWin.OurRosterItem = item;
            mapWin.Closed += new EventHandler(browserWin_Closed);
            mapWin.AddRosterItem(item);
            mapWin.AddRosterItemBody();
            mapWin.Title = "Buddy Map - " + item.JID.BareJID;
            mapWin.SingleRosterItemMap = false;

            // AudioMuxerWindow.InitiateOrShowCallTo(inst.FullJID);

            BrowserPopupWindows.Add(item.JID.BareJID, mapWin);
          
            mapWin.ButtonLoad_Click(null, null);


            //mapWin.AddRosterItemBody();
            //BrowserPopupWindow.XMPPClient = this.XMPPClient;

            //RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            //if (item == null)
            //    return;

            //ShowBrowserPopupWindow(item);
        }

        private bool BuildMapBrowserWindowForRosterItem(RosterItem item)
        {
            bool bRes = true;
            BrowserPopupWindow mapWin = new BrowserPopupWindow();
            mapWin.XMPPClient = this.XMPPClient;
            mapWin.OurRosterItem = item;
            mapWin.Closed += new EventHandler(browserWin_Closed);
            mapWin.AddRosterItem(item);
            mapWin.AddRosterItemBody();
            mapWin.Title = "Buddy Map - " + item.JID.BareJID;
            mapWin.SingleRosterItemMap = false;
            // mapWin.Visibility = System.Windows.Visibility.Collapsed;
            // mapWin.Show();

            if (BrowserPopupWindows.ContainsKey(item.JID.BareJID) == false)
            {
                BrowserPopupWindows.Add(item.JID.BareJID, mapWin);
            }
            else
            {
                // replace/ overwrite?
                BrowserPopupWindows[item.JID.BareJID] = mapWin;
            }
            return bRes;
        }

        // This is the one map per user method
        private void ButtonViewMap_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            if (MapWindows.ContainsKey(item.JID.BareJID) == true)
            {
                MapWindow exwin = MapWindows[item.JID.BareJID];
                // his "RosterItem" should already be me.
                if (exwin.OurRosterItem != item)
                    exwin.OurRosterItem = item;

                exwin.Title = "Buddy Map - " + item.JID.BareJID;

                exwin.Activate();
                IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(exwin).Handle;
                FlashWindow(windowHandle, true);
                return;
            }

            MapWindow mapWin = new MapWindow();
            mapWin.XMPPClient = this.XMPPClient;
            mapWin.OurRosterItem = item;
            mapWin.Closed += new EventHandler(mapWin_Closed);
            mapWin.OurRosterItem = item;
            mapWin.Title = "Buddy Map - " + item.JID.BareJID;
            mapWin.SingleRosterItemMap = false;

            // AudioMuxerWindow.InitiateOrShowCallTo(inst.FullJID);

            MapWindows.Add(item.JID.BareJID, mapWin);
            mapWin.Show();

            //mapWin.AddRosterItemBody();
            //BrowserPopupWindow.XMPPClient = this.XMPPClient;

            //RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            //if (item == null)
            //    return;

            //ShowBrowserPopupWindow(item);
        }

        // This is the everybody on one map method

        private void ButtonViewMapBrowserWindowConsolidatedMap_Click(object sender, RoutedEventArgs e)
        {
            /// Start audio on this resource... First find the resource that can do audio.
            /// If there are multiple instances that can do audio, prompt the user for which one
            /// 
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            // Eventually, we might want to use instances, but for now, just the RosterItem.
            // RosterItemPresenceInstance inst = item.FindAudioPresenceInstance();
            // if (inst != null)
            //{

            MapBrowserWindow.SingleRosterItemMap = false;
            MapBrowserWindow.AddRosterItem(item);

            if (ShowMapBrowserWindow())
            {
                MapBrowserWindow.AddRosterItemBody();
            } 
        }
 
        private void ButtonViewMapConsolidated_Click(object sender, RoutedEventArgs e)
        {
            /// Start audio on this resource... First find the resource that can do audio.
            /// If there are multiple instances that can do audio, prompt the user for which one
            /// 
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            // Eventually, we might want to use instances, but for now, just the RosterItem.
            // RosterItemPresenceInstance inst = item.FindAudioPresenceInstance();
            // if (inst != null)
            //{
            MapWindow.SingleRosterItemMap = false;
            MapWindow.OurRosterItem = item;

            if (ShowMapWindow())
            {

                //MapBrowserWindow.AddRosterItemBody();
            }  // AudioMuxerWindow.InitiateOrShowCallTo(inst.FullJID);
            //}
        }

        void mapWin_Closed(object sender, EventArgs e)
        {
            ((MapWindow)sender).Closed -= new EventHandler(mapWin_Closed);
            string strRemove = null;
            foreach (string strKey in MapWindows.Keys)
            {
                if (MapWindows[strKey] == sender)
                {
                    strRemove = strKey;
                    break;
                }
            }
            if (strRemove != null)
                MapWindows.Remove(strRemove);
        }

        void browserWin_Closed(object sender, EventArgs e)
        {
            ((BrowserPopupWindow)sender).Closed -= new EventHandler(browserWin_Closed);

           /* BrowserPopupWindow win = sender as BrowserPopupWindow;
            if (win != null)
            {
                win.Visibility = System.Windows.Visibility.Collapsed;
            } */
            string strRemove = null;
            foreach (string strKey in BrowserPopupWindows.Keys)
            {
                if (BrowserPopupWindows[strKey] == sender)
                {
                    strRemove = strKey;
                    break;
                }
            }
            if (strRemove != null)
                BrowserPopupWindows.Remove(strRemove);
        }
    }

}

