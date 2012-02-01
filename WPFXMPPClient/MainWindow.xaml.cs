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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

using PhoneXMPPLibrary;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

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

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            if (SendRawXMLWindow.IsLoaded == true)
                SendRawXMLWindow.Close();


            base.OnClosed(e);

            Application.Current.Shutdown();
        }


        public XMPPClient XMPPClient = new XMPPClient();


        private void HyperlinkConnect_Click(object sender, RoutedEventArgs e)
        {
            if (XMPPClient.XMPPState == XMPPState.Unknown)
            {
                XMPPClient.UserName = Properties.Settings.Default.UserName;
                XMPPClient.Server = Properties.Settings.Default.Server;
                XMPPClient.Domain = Properties.Settings.Default.Domain;
                XMPPClient.PresenceStatus.Priority = 10;
                XMPPClient.Resource = System.Environment.MachineName;
                XMPPClient.Port = Properties.Settings.Default.Port;
                XMPPClient.UseOldStyleTLS = Properties.Settings.Default.UseOldTLS;
                XMPPClient.AutoReconnect = true;

                XMPPLibrary.LoginWindow loginwin = new XMPPLibrary.LoginWindow();
                loginwin.XMPPClient = XMPPClient;
                if (loginwin.ShowDialog() == false)
                    Application.Current.Shutdown();


                XMPPClient.Connect();

                Properties.Settings.Default.UserName = XMPPClient.UserName;
                Properties.Settings.Default.Server = XMPPClient.Server;
                Properties.Settings.Default.Domain = XMPPClient.Domain;
                Properties.Settings.Default.Status = XMPPClient.PresenceStatus.Status;
                Properties.Settings.Default.Port = XMPPClient.Port;
                Properties.Settings.Default.UseOldTLS = XMPPClient.UseOldStyleTLS;
                Properties.Settings.Default.Save();
            }
            else if (XMPPClient.XMPPState > XMPPState.Connected)
            {
                XMPPClient.Disconnect();
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

        private void SurfaceWindow_Loaded(object sender, RoutedEventArgs e)
        {

            this.RectangleConnect.DataContext = this;
            this.DataContext = XMPPClient;
            this.ListBoxRoster.ItemsSource = XMPPClient.RosterItems;
            XMPPClient.PresenceStatus.Status = Properties.Settings.Default.Status;
            XMPPClient.OnRetrievedRoster += new EventHandler(RetrievedRoster);
            XMPPClient.OnRosterItemsChanged += new EventHandler(RosterChanged);
            XMPPClient.OnStateChanged += new EventHandler(XMPPStateChanged);
            XMPPClient.OnNewConversationItem += new PhoneXMPPLibrary.XMPPClient.DelegateNewConversationItem(XMPPClient_OnNewConversationItem);
            XMPPClient.OnUserSubscriptionRequest += new PhoneXMPPLibrary.XMPPClient.DelegateShouldSubscribeUser(XMPPClient_OnUserSubscriptionRequest);

            XMPPClient.OnDownloadFinished += new PhoneXMPPLibrary.XMPPClient.DelegateDownloadFinished(XMPPClient_OnDownloadFinished);
            XMPPClient.OnDownloadProgress += new PhoneXMPPLibrary.XMPPClient.DelegateProgress(XMPPClient_OnDownloadProgress);
            XMPPClient.OnNewIncomingFileTransferRequest += new PhoneXMPPLibrary.XMPPClient.DelegateIncomingFile(XMPPClient_IncomingFile);

            SendRawXMLWindow.SetXMPPClient(XMPPClient);
        }


        void XMPPClient_OnDownloadFinished(string strRequestId, string strLocalFileName, RosterItem itemfrom)
        {
            this.Dispatcher.Invoke(new PhoneXMPPLibrary.XMPPClient.DelegateDownloadFinished(SafeDownloadFinished), strRequestId, strLocalFileName, itemfrom);
        }
        void SafeDownloadFinished(string strRequestId, string strLocalFileName, RosterItem itemfrom)
        {
            ChatWindow window = FindOrCreateChatWIndow(itemfrom);
            window.DownloadFinished(strRequestId, strLocalFileName, itemfrom);
        }

        void XMPPClient_OnDownloadProgress(string strRequestId, RosterItem itemfrom, int nBytes, int nTotal)
        {
            this.Dispatcher.Invoke(new PhoneXMPPLibrary.XMPPClient.DelegateProgress(SafeDownloadProgress), strRequestId, itemfrom, nBytes, nTotal);

        }
        void SafeDownloadProgress(string strRequestId, RosterItem itemfrom, int nBytes, int nTotal)
        {
            ChatWindow window = FindOrCreateChatWIndow(itemfrom);
            window.DownloadProgres(strRequestId, nBytes, nTotal);
        }

        void XMPPClient_IncomingFile(string strRequestId, string strFileName, int nFileSize, RosterItem itemfrom)
        {
            if (itemfrom == null)
                XMPPClient.DeclineFileDownload(strRequestId);

            this.Dispatcher.Invoke(new PhoneXMPPLibrary.XMPPClient.DelegateIncomingFile(SafeIncomingFile), strRequestId, strFileName, nFileSize, itemfrom);
        }

        void SafeIncomingFile(string strRequestId, string strFileName, int nFileSize, RosterItem itemfrom)
        {
                
            /// New incoming file request... accept or reject it... pass this off to the appropriate window
            /// 
            ChatWindow window = FindOrCreateChatWIndow(itemfrom);
            window.IncomingFileRequest(strRequestId, strFileName, nFileSize);
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
            base.OnPreviewKeyDown(e);
        }

        SendRawXMLWindow SendRawXMLWindow = new SendRawXMLWindow();

        void XMPPClient_OnUserSubscriptionRequest(PresenceMessage pres)
        {
            Dispatcher.Invoke(new PhoneXMPPLibrary.XMPPClient.DelegateShouldSubscribeUser(DoOnUserSubscriptionRequest), pres);
            
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

        void XMPPClient_OnNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {
            // Load any old messages if we haven't yet
         

            ChatWindow.SaveConversation(item);

            if (bReceived == false)
                return;

            FindOrCreateChatWIndow(item);
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

        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        public void RosterChanged(object obj, EventArgs arg)
        {
            this.Dispatcher.Invoke(new DelegateVoid(SetFuckingRoster));
        }

        public delegate void DelegateVoid();
        public void RetrievedRoster(object obj, EventArgs arg)
        {
            // Load all our our existing conversations

            foreach (RosterItem item in XMPPClient.RosterItems)
            {
                if (item.HasLoadedConversation == false)
                {
                    item.HasLoadedConversation = true;

                    string strFilename = string.Format("{0}_conversation.item", item.JID.BareJID);
                    using (IsolatedStorageFile storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
                    {
                        // Load from storage
                        IsolatedStorageFileStream location = null;
                        try
                        {
                            location = new IsolatedStorageFileStream(strFilename, System.IO.FileMode.Open, storage);
                            DataContractSerializer ser = new DataContractSerializer(typeof(PhoneXMPPLibrary.Conversation));

                            item.Conversation = ser.ReadObject(location) as PhoneXMPPLibrary.Conversation;
                        }
                        catch (Exception ex)
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

            this.Dispatcher.Invoke(new DelegateVoid(SetFuckingRoster));
        }

        void SetFuckingRoster()
        {
            this.ListBoxRoster.ItemsSource = null;
            this.ListBoxRoster.ItemsSource = XMPPClient.RosterItems;

           

            
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
            }
            else if (XMPPClient.XMPPState == XMPPState.Ready)
            {
                this.FirePropertyChanged("ConnectedStateBrush");
                ComboBoxPresence.IsEnabled = true;
                ButtonAddBuddy.IsEnabled = true;
                //XMPPClient.SetGeoLocation(32.234, -97.3453);
            }
            else if (XMPPClient.XMPPState == XMPPState.AuthenticationFailed)
            {
                MessageBox.Show("Incorrect username or password", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                XMPPClient.Disconnect();
            }
            else
            {
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



        private void TextBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (XMPPClient.XMPPState == XMPPState.Ready)
            {
                XMPPClient.UpdatePresence();
            }
            Properties.Settings.Default.Status = XMPPClient.PresenceStatus.Status;
            Properties.Settings.Default.Save();
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

        private void ButtonStartAudioCall_Click(object sender, RoutedEventArgs e)
        {

        }


    }
}