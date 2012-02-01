using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using PhoneXMPPLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

using System.Device.Location;


namespace XMPPClient
{
    public partial class MainPage : PhoneApplicationPage
    {
        GeoCoordinateWatcher gpswatcher;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            App.XMPPClient.OnRetrievedRoster += new EventHandler(RetrievedRoster);
            App.XMPPClient.OnStateChanged += new EventHandler(StateChanged);
            App.XMPPClient.OnNewConversationItem += new PhoneXMPPLibrary.XMPPClient.DelegateNewConversationItem(XMPPClient_OnNewConversationItem);
            App.XMPPClient.OnUserSubscriptionRequest += new PhoneXMPPLibrary.XMPPClient.DelegateShouldSubscribeUser(XMPPClient_OnUserSubscriptionRequest);

            // The watcher variable was previously declared as type GeoCoordinateWatcher. 
            if (gpswatcher == null)
            {
                gpswatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // using high accuracy
                gpswatcher.MovementThreshold = 20; // use MovementThreshold to ignore noise in the signal
            }

            //gpswatcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(gpswatcher_StatusChanged);
            gpswatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(gpswatcher_PositionChanged);


        }

        void XMPPClient_OnUserSubscriptionRequest(PresenceMessage pres)
        {
            Dispatcher.BeginInvoke(new PhoneXMPPLibrary.XMPPClient.DelegateShouldSubscribeUser(DoOnUserSubscriptionRequest), pres);
            
        }

        void DoOnUserSubscriptionRequest(PresenceMessage pres)
        {
            MessageBoxResult result = MessageBox.Show(string.Format("User '{0}; wants to see your presence.  Allow?", pres.From), "Allow User To See You?", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                App.XMPPClient.AcceptUserPresence(pres, "", "");
            }
            else
            {
                App.XMPPClient.DeclineUserPresence(pres);
            }
            
        }

        void XMPPClient_OnNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {
            /// Save the conversation first
            ChatPage.SaveConversation(item);

            Dispatcher.BeginInvoke(new PhoneXMPPLibrary.XMPPClient.DelegateNewConversationItem(DoOnNewConversationItem), item, bReceived, msg);
        }

        void DoOnNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {
            //Microsoft.Phone.PictureDecoder.DecodeJpeg(

            if (bReceived == true)
            {
                if (msg.Message != null)
                {
                    Microsoft.Phone.Shell.ShellToast toast = new Microsoft.Phone.Shell.ShellToast();
                    toast.Title = msg.Message;
                    //toast.NavigationUri = new Uri(string.Format("/ChatPage.xaml?JID={0}", msg.From.BareJID));
                    toast.Show();
                    Microsoft.Devices.VibrateController.Default.Start(TimeSpan.FromMilliseconds(200));
                }
            }
            if (this.Visibility == System.Windows.Visibility.Visible)
            {
                System.IO.Stream stream = TitleContainer.OpenStream("sounds/ding.wav");
                SoundEffect effect = SoundEffect.FromStream(stream);
                FrameworkDispatcher.Update();
                effect.Play();
                stream.Close();
            }
            
        }

       
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

            this.DataContext = App.XMPPClient;
            this.ListBoxRoster.DataContext = App.XMPPClient;
            var selected = from c in App.XMPPClient.RosterItems group c by c.Group into n select new GroupingLayer<string, RosterItem>(n);
            this.ListBoxRoster.ItemsSource = selected;

        }

        DateTime m_dtLastPositionUpdate = DateTime.MinValue;
        void gpswatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            // Update our xmpp client's position

            if (App.XMPPClient.XMPPState == XMPPState.Ready)
            {
                if (m_dtLastPositionUpdate < (DateTime.Now - TimeSpan.FromMinutes(1)))
                {
                    App.XMPPClient.SetGeoLocation(e.Position.Location.Latitude, e.Position.Location.Longitude);
                    m_dtLastPositionUpdate = DateTime.Now;
                }
            }
        }

        void gpswatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
        }

     

        public void RetrievedRoster(object obj, EventArgs arg)
        {
            this.Dispatcher.BeginInvoke(SetFuckingRoster);   
        }

        void SetFuckingRoster()
        {
            this.ListBoxRoster.ItemsSource = null;
            this.ListBoxRoster.DataContext = App.XMPPClient;
            //this.ListBoxRoster.ItemsSource = App.XMPPClient.RosterItems;

            var selected = from c in App.XMPPClient.RosterItems group c by c.Group into n select new GroupingLayer<string, RosterItem>(n);
            this.ListBoxRoster.ItemsSource = selected;

        }

        public void StateChanged(object obj, EventArgs arg)
        {
            Dispatcher.BeginInvoke(HandleStateChanged);
        }

        void HandleStateChanged()
        {
            if (App.XMPPClient.XMPPState == XMPPState.Ready)
            {
                gpswatcher.Start();

                _performanceProgressBar.IsIndeterminate = false;
                //this.HyperlinkConnect.Content = "Disconnect";
            }
            else if (App.XMPPClient.XMPPState == XMPPState.Unknown)
            {
                gpswatcher.Stop();

                _performanceProgressBar.IsIndeterminate = false;
                //this.HyperlinkConnect.Content = "Connect";
            }
            else if (App.XMPPClient.XMPPState == XMPPState.AuthenticationFailed)
            {
                gpswatcher.Stop();
                _performanceProgressBar.IsIndeterminate = false;
                //this.HyperlinkConnect.Content = "Connect";
            }
            else
            {
                gpswatcher.Stop();
            }
        }

        private void HyperlinkRosterItem_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            NavigationService.Navigate(new Uri(string.Format("/ChatPage.xaml?JID={0}", item.JID), UriKind.Relative)); 
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            QuitException.Quit();
            //NavigationService.GoBack();
            //float fzero = 0.0f;
            //float x = 1 / fzero;
        }

        private void HyperlinkConnect_Click(object sender, RoutedEventArgs e)
        {
            if (App.XMPPClient.XMPPState == XMPPState.Unknown)
            {
                NavigationService.Navigate(new Uri("/ConnectPage.xaml", UriKind.Relative));
                _performanceProgressBar.IsIndeterminate = true;
            }
            else if (App.XMPPClient.XMPPState > XMPPState.Connected)
            {
                App.XMPPClient.Disconnect();
            }

        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
                QuitException.Quit();
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //      if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Refresh)
            //         App.EnsureConnected();

            base.OnNavigatedTo(e);
        }

        private void ButtonAddAccount_Click(object sender, EventArgs e)
        {

        }

        private void ButtonOptions_Click(object sender, EventArgs e)
        {

        }

        private void ImageAvatar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void ButtonViewMessages_Click(object sender, RoutedEventArgs e)
        {
            RosterItem item = ((FrameworkElement)sender).DataContext as RosterItem;
            if (item == null)
                return;

            NavigationService.Navigate(new Uri(string.Format("/ChatPage.xaml?JID={0}", item.JID), UriKind.Relative)); 

        }

        private void ButtonStartAudioCall_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonSwitch_Click(object sender, RoutedEventArgs e)
        {
            _performanceProgressBar.IsIndeterminate = true;
            //Microsoft.Phone.Shell.SystemTray.ProgressIndicator = _performanceProgressBar;
        }


        private void MenuItemOnline_Click(object sender, RoutedEventArgs e)
        {
            if (App.XMPPClient.XMPPState == XMPPState.Ready)
            {
                App.XMPPClient.PresenceStatus.PresenceShow = PresenceShow.chat;
                App.XMPPClient.PresenceStatus.Status = "Online";
                App.XMPPClient.PresenceStatus.PresenceType = PresenceType.available;
                App.XMPPClient.UpdatePresence();
            }
        }

        private void MenuItemBusy_Click(object sender, RoutedEventArgs e)
        {
            if (App.XMPPClient.XMPPState == XMPPState.Ready)
            {
                App.XMPPClient.PresenceStatus.PresenceShow = PresenceShow.away;
                App.XMPPClient.PresenceStatus.Status = "Busy";
                App.XMPPClient.PresenceStatus.PresenceType = PresenceType.available;
                App.XMPPClient.UpdatePresence();
            }
        }

        private void MenuItemDND_Click(object sender, RoutedEventArgs e)
        {
            if (App.XMPPClient.XMPPState == XMPPState.Ready)
            {
                App.XMPPClient.PresenceStatus.PresenceShow = PresenceShow.dnd;
                App.XMPPClient.PresenceStatus.Status = "DO NOT DISTURB!!";
                App.XMPPClient.PresenceStatus.PresenceType = PresenceType.available;
                App.XMPPClient.UpdatePresence();
            }
        }

        private void MenuItemAway_Click(object sender, RoutedEventArgs e)
        {
            if (App.XMPPClient.XMPPState == XMPPState.Ready)
            {
                App.XMPPClient.PresenceStatus.PresenceShow = PresenceShow.xa;
                App.XMPPClient.PresenceStatus.Status = "away";
                App.XMPPClient.PresenceStatus.PresenceType = PresenceType.available;
                App.XMPPClient.UpdatePresence();
            }
        }

    }


    public class GroupingLayer<TKey, TElement> : IGrouping<TKey, TElement>
    {

        private readonly IGrouping<TKey, TElement> grouping;

        public GroupingLayer(IGrouping<TKey, TElement> unit)
        {
            grouping = unit;
        }

        public TKey Key
        {
            get { return grouping.Key; }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return grouping.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return grouping.GetEnumerator();
        }
    }

    class QuitException : Exception
    {
        public QuitException()
            : base()
        {
        }

        public static void Quit()
        {
            throw new QuitException();
        }
    }
}