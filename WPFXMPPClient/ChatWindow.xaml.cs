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

using System.Text.RegularExpressions;

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
            this.DataContext = OurRosterItem;
            XMPPClient.OnNewConversationItem += new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(XMPPClient_OnNewConversationItem);
            SetConversation();
        }

        Regex reghyperlink = new Regex(@"\w+\://\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

        Paragraph MainParagraph = new Paragraph();

        public void SetConversation()
        {
            MainParagraph.Inlines.Clear();
            TextBlockChat.Document.Blocks.Clear();
            TextBlockChat.Document.Blocks.Add(MainParagraph);
            
            foreach (TextMessage msg in OurRosterItem.Conversation.Messages)
            {
                AddInlinesForMessage(msg);
            }

            this.TextBlockChat.ScrollToEnd();
        }

        const double FontSizeFrom = 10.0f;
        const double FontSizeMessage = 14.0f;
        void AddInlinesForMessage(TextMessage msg)
        {
            Span msgspan = new Span();
            string strRun = string.Format("{0} to {1} - {2}", msg.From, msg.To, msg.Received);
            Run runfrom = new Run(strRun);
            runfrom.Foreground = Brushes.Gray;
            runfrom.FontSize = FontSizeFrom;
            msgspan.Inlines.Add(runfrom);

            msgspan.Inlines.Add(new LineBreak());


            Span spanmsg = new Span();
            spanmsg.Foreground = Brushes.Gray;
            spanmsg.FontSize = FontSizeMessage;
            msgspan.Inlines.Add(spanmsg);

            /// Look for hyperlinks in our run
            /// 
            string strMessage = msg.Message;
            int nMatchAt = 0;
            Match matchype = reghyperlink.Match(strMessage, nMatchAt);
            while (matchype.Success == true)
            {
                string strHyperlink = matchype.Value;

                /// Add everything before this as a normal run
                /// 
                if (matchype.Index > nMatchAt)
                {
                    Run runtext = new Run(strMessage.Substring(nMatchAt, (matchype.Index - nMatchAt)));
                    runtext.Foreground = msg.TextColor;
                    msgspan.Inlines.Add(runtext);
                }

                Hyperlink link = new Hyperlink();
                link.Inlines.Add(strMessage.Substring(matchype.Index, matchype.Length));
                link.Foreground = Brushes.Blue;
                link.TargetName = "_blank";
                try
                {
                    link.NavigateUri = new Uri(strMessage.Substring(matchype.Index, matchype.Length));
                }
                catch (Exception ex)
                {
                }
                link.Click += new RoutedEventHandler(link_Click);
                msgspan.Inlines.Add(link);

                nMatchAt = matchype.Index + matchype.Length;

                if (nMatchAt >= (strMessage.Length - 1))
                    break;

                matchype = reghyperlink.Match(strMessage, nMatchAt);
            }

            /// see if we have any remaining text
            /// 
            if (nMatchAt < strMessage.Length)
            {
                Run runtext = new Run(strMessage.Substring(nMatchAt, (strMessage.Length - nMatchAt)));
                runtext.Foreground = msg.TextColor;
                msgspan.Inlines.Add(runtext);
            }
            msgspan.Inlines.Add(new LineBreak());

            this.MainParagraph.Inlines.Add(msgspan);
        }

        void link_Click(object sender, RoutedEventArgs e)
        {
            /// Navigate to this link
            /// 
            System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());
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
                AddInlinesForMessage(msg);

                if (bReceived == true)
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
                }

                this.TextBlockChat.ScrollToEnd();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            OurRosterItem.HasNewMessages = false;
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
            SetConversation();
            
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
            //foreach (TextMessage msg in this.ListBoxConversation.SelectedItems)
            //{
            //    if (msg.Sent == false)
            //       sb.AppendFormat("From {0} at {1}\r\n{2}", msg.From, msg.Received, msg.Message);
            //    else
            //        sb.AppendFormat("To {0} at {1}\r\n{2}", msg.To, msg.Received, msg.Message);
            //}
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

                        break;
                    }
                }
                

            }
        }

        public void DownloadFinished(string strRequestId, string strLocalFileName, RosterItem itemfrom)
        {
            System.Diagnostics.Process.Start(strLocalFileName);
        }

        private void TextBlockChat_TouchDown(object sender, TouchEventArgs e)
        {
            /// scroll our window
            /// 
            
        }

        private void TextBlockChat_TouchMove(object sender, TouchEventArgs e)
        {

        }

    }
}
