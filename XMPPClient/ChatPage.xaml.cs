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

using System.Net.XMPP;
using System.Runtime.Serialization;
using System.IO.IsolatedStorage;

using Microsoft.Phone.Tasks;

namespace XMPPClient
{
    public partial class ChatPage : PhoneApplicationPage
    {
        public ChatPage()
        {
            InitializeComponent();
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
        }

        PhotoChooserTask photoChooserTask = null;
        private void ButtonSendMessage_Click(object sender, EventArgs e)
        {
            App.XMPPClient.SendChatMessage(this.TextBoxChatToSend.Text, OurRosterItem.JID);
            this.TextBoxChatToSend.Text = "";
            this.Focus();
            this.ListBoxConversation.Focus();
            //this.TextBoxChatToSend.Focus();
        }


        bool m_bInFileTransferMode = false;
        public bool InFileTransferMode
        {
            get { return m_bInFileTransferMode; }
            set { m_bInFileTransferMode = value; }
        }

        string m_strFileTransferSID = "";

        public string FileTransferSID
        {
            get { return m_strFileTransferSID; }
            set { m_strFileTransferSID = value; }
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string strJID = NavigationContext.QueryString["JID"];

            OurRosterItem = App.XMPPClient.FindRosterItem(new JID(strJID));

            if (this.InFileTransferMode == true)
            {
                this.NavigationService.Navigate(new Uri("/FileTransferPage.xaml", UriKind.Relative));
                this.InFileTransferMode = false;
            }

            if ((OurRosterItem == null) || (App.XMPPClient.XMPPState != XMPPState.Ready) )
            {

                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                return;
            }

            /// See if we have this conversation in storage if there are no messages
            if (OurRosterItem.Conversation.Messages.Count <= 0)
            {

                string strFilename = string.Format("{0}_conversation.item", OurRosterItem.JID.BareJID);

                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // Load from storage
                    IsolatedStorageFileStream location = null;
                    try
                    {
                        location = new IsolatedStorageFileStream(strFilename, System.IO.FileMode.Open, storage);
                        DataContractSerializer ser = new DataContractSerializer(typeof(System.Net.XMPP.Conversation));

                        OurRosterItem.Conversation = ser.ReadObject(location) as System.Net.XMPP.Conversation;
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


            OurRosterItem.HasNewMessages = false; /// We just viewed new messages
                                                  /// 

            this.DataContext = OurRosterItem;
            this.ListBoxConversation.ItemsSource = OurRosterItem.Conversation.Messages;
            this.TextBlockConversationTitle.Text = OurRosterItem.Name;

            App.XMPPClient.OnNewConversationItem += new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(XMPPClient_OnNewConversationItem);

         

        }

        

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.XMPPClient.OnNewConversationItem -= new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(XMPPClient_OnNewConversationItem);
            if (OurRosterItem != null)
               SaveConversation(OurRosterItem);
        }

        public static void SaveConversation(RosterItem item)
        {
            /// Save this conversation so it can be restored later... save it under the JID name

            string strFilename = string.Format("{0}_conversation.item", item.JID.BareJID);
            


            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Load from storage
                IsolatedStorageFileStream location = new IsolatedStorageFileStream(strFilename, System.IO.FileMode.Create, storage);
                DataContractSerializer ser = new DataContractSerializer(typeof(System.Net.XMPP.Conversation));

                try
                {
                    ser.WriteObject(location, item.Conversation);
                }
                catch (Exception ex)
                {
                }
                location.Close();
            }

        }

        void XMPPClient_OnNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {
            Dispatcher.BeginInvoke(new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(DoOnNewConversationItem), item, bReceived, msg);
        }

        void DoOnNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {
            if (item.JID.BareJID == OurRosterItem.JID.BareJID)
            {
                /// Clear our new message flag for this roster user as long as this window is open
                item.HasNewMessages = false;

                this.ListBoxConversation.UpdateLayout();
                if (this.ListBoxConversation.Items.Count > 0)
                   this.ListBoxConversation.ScrollIntoView(this.ListBoxConversation.Items[this.ListBoxConversation.Items.Count - 1]);

            }
        }

        RosterItem OurRosterItem;

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ListBoxConversation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ButtonClearMessages_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear these items?", "Confirm Clear", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                OurRosterItem.Conversation.Clear();
                this.Focus();
            }
        }

        public string LastFullJIDBeforeMSDecidedToScrewUs = null;

        private void ButtonSendPhoto_Click(object sender, EventArgs e)
        {
            try
            {
                if (OurRosterItem.LastFullJIDToGetMessageFrom.Resource.Length <= 0)
                {
                    if (OurRosterItem.ClientInstances.Count > 0)
                    {
                        LastFullJIDBeforeMSDecidedToScrewUs = OurRosterItem.ClientInstances[0].FullJID;
                    }
                }
                else
                    LastFullJIDBeforeMSDecidedToScrewUs = OurRosterItem.LastFullJIDToGetMessageFrom;
                photoChooserTask.ShowCamera = true;
                photoChooserTask.Show();
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("Unable to select a photo");
            }

        }

        void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            App.WaitConnected();

            if (e.TaskResult == TaskResult.OK)
            {
                byte[] bStream = new byte[e.ChosenPhoto.Length];
                e.ChosenPhoto.Read(bStream, 0, bStream.Length);

                this.InFileTransferMode = true;
                this.FileTransferSID = App.XMPPClient.FileTransferManager.SendFile(e.OriginalFileName, bStream, LastFullJIDBeforeMSDecidedToScrewUs);

                //Code to display the photo on the page in an image control named myImage.
                //System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                //bmp.SetSource(e.ChosenPhoto);
                //myImage.Source = bmp;
            }
        }

        private void TextBoxChatToSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                App.XMPPClient.SendChatMessage(this.TextBoxChatToSend.Text, OurRosterItem.JID);
                this.TextBoxChatToSend.Text = "";
                this.Focus();
                this.ListBoxConversation.Focus();
            }
        }

    }
}
