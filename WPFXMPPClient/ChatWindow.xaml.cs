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
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

using System.Runtime.InteropServices;


namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();
        }

        public XMPPClient XMPPClient = null;
        public RosterItem OurRosterItem = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /// See if we have this conversation in storage if there are no messages
            if (OurRosterItem.HasLoadedConversation == false)
            {
                OurRosterItem.HasLoadedConversation = true;


                string strFilename = string.Format("{0}_conversation.item", OurRosterItem.JID.BareJID);

                using (IsolatedStorageFile storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User|IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
                {
                    // Load from storage
                    IsolatedStorageFileStream location = null;
                    try
                    {
                        location = new IsolatedStorageFileStream(strFilename, System.IO.FileMode.Open, storage);
                        DataContractSerializer ser = new DataContractSerializer(typeof(System.Net.XMPP.Conversation));

                        OurRosterItem.Conversation = ser.ReadObject(location) as System.Net.XMPP.Conversation;
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


            OurRosterItem.HasNewMessages = false; /// We just viewed new messages
            /// 

            this.DataContext = OurRosterItem;
            this.ListBoxConversation.ItemsSource = OurRosterItem.Conversation.Messages;
            //this.ListBoxInstances.ItemsSource = OurRosterItem.ClientInstances;

            XMPPClient.OnNewConversationItem += new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(XMPPClient_OnNewConversationItem);

        }


        public static void SaveConversation(RosterItem item)
        {
            /// Save this conversation so it can be restored later... save it under the JID name

            string strFilename = string.Format("{0}_conversation.item", item.JID.BareJID);



            using (IsolatedStorageFile storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                // Load from storage
                IsolatedStorageFileStream location = new IsolatedStorageFileStream(strFilename, System.IO.FileMode.Create, storage);
                DataContractSerializer ser = new DataContractSerializer(typeof(System.Net.XMPP.Conversation));

                try
                {
                    ser.WriteObject(location, item.Conversation);
                }
                catch (Exception)
                {
                }
                location.Close();
            }

        }


        void XMPPClient_OnNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {
            Dispatcher.Invoke(new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(DoOnNewConversationItem), item, bReceived, msg);
        }

        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        bool m_bHasAckedMessage = false;

        void DoOnNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {
            if (bReceived == true)
            {
                foreach (RosterItemPresenceInstance instance in  this.ListBoxInstances.Items)
                {
                    if (instance.FullJID.Equals(msg.From) == true)
                    {
                        this.ListBoxInstances.SelectedItem = instance;
                        break;
                    }
                }
            }

            if (item.JID.BareJID.Equals(OurRosterItem.JID.BareJID) == true)
            {

                
                if (m_bHasAckedMessage == false)
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer("Sounds/ding.wav");
                    player.Play();

                    IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                    FlashWindow(windowHandle, true);
                }
                
                /// Clear our new message flag for this roster user as long as this window is open

                if (this.IsActive == true)
                {
                    OurRosterItem.HasNewMessages = false;
                    m_bHasAckedMessage = true;
                }

                this.ListBoxConversation.UpdateLayout();
                if (this.ListBoxConversation.Items.Count > 0)
                    this.ListBoxConversation.ScrollIntoView(this.ListBoxConversation.Items[this.ListBoxConversation.Items.Count - 1]);

            }
        }

        protected override void OnActivated(EventArgs e)
        {
            OurRosterItem.HasNewMessages = false;
            m_bHasAckedMessage = true;
            base.OnActivated(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            XMPPClient.OnNewConversationItem -= new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(XMPPClient_OnNewConversationItem);
            SaveConversation(OurRosterItem);
            base.OnClosing(e);
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            OurRosterItem.Conversation.Clear();
        }

        private void TextBoxChatToSend_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                DoSend();
            }

        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            DoSend();
        }

        void DoSend()
        {
            /// Send to our selected item
            /// 
            if (this.ListBoxInstances.SelectedItems.Count > 0)
            {
                foreach (RosterItemPresenceInstance instance in this.ListBoxInstances.SelectedItems)
                {
                    XMPPClient.SendChatMessage(this.TextBoxChatToSend.Text, instance.FullJID);
                }
            }
            else
               OurRosterItem.SendChatMessage(this.TextBoxChatToSend.Text, MessageSendOption.SendToLastRecepient);

            //XMPPClient.SendChatMessage(, OurRosterItem.JID);
            this.TextBoxChatToSend.Text = "";
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
                this.DragMove();
        }


        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            SaveConversation(OurRosterItem);
            this.Close();
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            /// Copy all the selected text 
            /// 
            StringBuilder sb = new StringBuilder();
            foreach (TextMessage msg in this.ListBoxConversation.SelectedItems)
            {
                if (msg.Sent == false)
                   sb.AppendFormat("From {0} at {1}\r\n{2}", msg.From, msg.Received, msg.Message);
                else
                    sb.AppendFormat("To {0} at {1}\r\n{2}", msg.To, msg.Received, msg.Message);
            }
            Clipboard.SetText(sb.ToString());
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void ButtonSendFile_Click(object sender, RoutedEventArgs e)
        {
            // Find the file to send

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                string strFileName = dlg.FileName;

                if ((this.ListBoxInstances.SelectedItems.Count <= 0) && (this.ListBoxInstances.Items.Count > 0))
                    this.ListBoxInstances.SelectedIndex = 0;

                if (this.ListBoxInstances.SelectedItems.Count > 0)
                {
                    foreach (RosterItemPresenceInstance instance in this.ListBoxInstances.SelectedItems)
                    {
                        /// Just send it to 1 recepient for now, must have a full jid
                        string strSendID = XMPPClient.FileTransferManager.SendFile(strFileName, instance.FullJID);

                        FileInfo info = new FileInfo(strFileName);
                        ProgressBarDownload.Minimum = 0;
                        ProgressBarDownload.Maximum = info.Length;

                        break;
                    }
                }
                

            }
        }


        public void DownloadProgres(string strRequestId, int nBytes, int nTotal)
        {
            ProgressBarDownload.Value = nBytes;
        }

        public void DownloadFinished(string strRequestId, string strLocalFileName, RosterItem itemfrom)
        {
            System.Diagnostics.Process.Start(strLocalFileName);
        }

    }
}
