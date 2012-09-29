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
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

using System.Runtime.InteropServices;

using System.Text.RegularExpressions;

using System.Speech.Synthesis;
using xmedianet.socketserver;

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

        System.Threading.Thread ThreadSpeak = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadSpeak = new System.Threading.Thread(new System.Threading.ThreadStart(SpeakThread));
            ThreadSpeak.Name = "Speak Thread";
            ThreadSpeak.IsBackground = true;
            ThreadSpeak.Start();


            /// See if we have this conversation in storage if there are no messages
            if (OurRosterItem.HasLoadedConversation == false)
            {
                OurRosterItem.HasLoadedConversation = true;

                string strFilename = string.Format("{0}_conversation.item", OurRosterItem.JID.BareJID);

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

            this.DialogControlLastMessage.OurRosterItem = this.OurRosterItem;
            this.DialogControlLastMessage.ShowTimeStamps = true;

            OurRosterItem.HasNewMessages = false; /// We just viewed new messages
            this.DataContext = OurRosterItem;
            XMPPClient.OnNewConversationItem += new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(XMPPClient_OnNewConversationItem);
        }


        public static void SaveConversation(RosterItem item)
        {
            /// Save this conversation so it can be restored later... save it under the JID name
            string strFilename = string.Format("{0}_conversation.item", item.JID.BareJID);

            //string strPath = string.Format("{0}\\conversations", Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
            string strPath = string.Format("{0}\\conversations\\{1}", Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), item.XMPPClient.JID.BareJID.Replace("@", ""));
            if (System.IO.Directory.Exists(strPath) == false)
                System.IO.Directory.CreateDirectory(strPath);

            string strFullFileName = string.Format("{0}\\{1}", strPath, strFilename);
            FileStream location = null;
            try
            {
                location = new FileStream(strFullFileName, System.IO.FileMode.Create);

                DataContractSerializer ser = new DataContractSerializer(typeof(System.Net.XMPP.Conversation));
                ser.WriteObject(location, item.Conversation);
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


        void XMPPClient_OnNewConversationItem(RosterItem item, bool bReceived, TextMessage msg)
        {
            Dispatcher.Invoke(new System.Net.XMPP.XMPPClient.DelegateNewConversationItem(DoOnNewConversationItem), item, bReceived, msg);
        }

        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        void SpeakThread()
        {
            while (true)
            {
                string strSpeak = PhrasesToSpeak.WaitNext(5000);
                if (strSpeak != null)
                {
                    syn.Speak(strSpeak);
                }
            }
        }

        System.Speech.Synthesis.SpeechSynthesizer syn = new SpeechSynthesizer();
        EventQueueWithNotification<string> PhrasesToSpeak = new EventQueueWithNotification<string>();
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
                if (bReceived == true)
                {
                    if (this.CheckBoxUseSpeech.IsChecked == true)
                    {
                        if ((msg.Thread.ToLower() == "quiet" || msg.Thread.ToLower() == "silent" || msg.Thread.ToLower() == "nospeak") == false)
                        {
                            PhrasesToSpeak.Enqueue(DialogControl.ReplaceWebLinksWithPhrase(msg.Message, "hyperlink specified here"));
                        }
                    }
                    else
                    {
                        if ((msg.Thread.ToLower() == "quiet" || msg.Thread.ToLower() == "silent") == false)
                        {
                            System.Media.SoundPlayer player = new System.Media.SoundPlayer("Sounds/ding.wav");
                            player.Play();
                        }
                    }
                    if ((msg.Thread.ToLower() == "noflash" || msg.Thread.ToLower() == "unseen") == false)
                    {
                        IntPtr windowHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                        FlashWindow(windowHandle, true);
                    }
                }

                /// Clear our new message flag for this roster user as long as this window is open

                if (this.IsActive == true)
                {
                    OurRosterItem.HasNewMessages = false;
                }
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

        public void Clear()
        {
            OurRosterItem.Conversation.Clear();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();            
        }

        private void TextBoxChatToSend_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                DoSend();
                e.Handled = true;
            }

        }

        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            DoSend();
        }

        const string HideMe = "[minimize]";
        const string ClearMe = "[clear]";
        void DoSend()
        {
            /// Send to our selected item
            /// 
            string strText = this.TextBoxChatToSend.Text;
            if (this.ListBoxInstances.SelectedItems.Count > 0)
            {
                foreach (RosterItemPresenceInstance instance in this.ListBoxInstances.SelectedItems)
                {
                    if (strText == HideMe)
                    {
                        PrivacyService serv = XMPPClient.FindService(typeof(PrivacyService)) as PrivacyService;
                        if (serv != null)
                        {
                            serv.ForceUserToMinimizeMyWindow(instance.FullJID);
                        }
                    }
                    else if (strText == ClearMe)
                    {
                        PrivacyService serv = XMPPClient.FindService(typeof(PrivacyService)) as PrivacyService;
                        if (serv != null)
                        {
                            serv.ForceUserToClearMyHistory(instance.FullJID);
                        }
                    }
                    else
                    {
                        ThreadedMessage threadedMessage = ConversationThreadManager.GetThreadedMessage(strText);
                        PlaceChatInAppropriateStream(threadedMessage);    
                        XMPPClient.SendChatMessage(strText, instance.FullJID);
                    }
                }
            }
            else
            {
                if (strText == HideMe)
                {
                    PrivacyService serv = XMPPClient.FindService(typeof(PrivacyService)) as PrivacyService;
                    if (serv != null)
                    {
                        serv.ForceUserToMinimizeMyWindow(OurRosterItem.LastFullJIDToGetMessageFrom);
                    }
                }
                else if (strText == ClearMe)
                {
                    PrivacyService serv = XMPPClient.FindService(typeof(PrivacyService)) as PrivacyService;
                    if (serv != null)
                    {
                        serv.ForceUserToClearMyHistory(OurRosterItem.LastFullJIDToGetMessageFrom);
                    }
                }
                else
                {
                    ThreadedMessage threadedMessage = ConversationThreadManager.GetThreadedMessage(this.TextBoxChatToSend.Text);
                    PlaceChatInAppropriateStream(threadedMessage);    
                    OurRosterItem.SendChatMessage(this.TextBoxChatToSend.Text, MessageSendOption.SendToLastRecepient);
                }
            }

           
            this.TextBoxChatToSend.Text = "";
        }

        private void PlaceChatInAppropriateStream(ThreadedMessage threadedMessage)
        {
            TabItem tabItemThread = GetTabItemForThreadedMessage(threadedMessage);
            if (tabItemThread == null)
            {
                // create it
                tabItemThread = CreateTabItem(threadedMessage);
            }
            if (tabItemThread == null)
            {
                // error
                return;
            }
            // Add text to this tab.


        }

        private TabItem CreateTabItem(ThreadedMessage threadedMessage)
        {
            TabItem newtabItem = new TabItem();
            newtabItem.Header = threadedMessage.ThreadName;
            return newtabItem;
        }

        private TabItem GetTabItemForThreadedMessage(ThreadedMessage threadedMessage)
        {
            var matchingItem =
            from TabItem t in TabControlThreads.Items where t.Name == threadedMessage.ThreadName select t;

            if (matchingItem.Count() != 0)
            {
                return matchingItem.FirstOrDefault();
               //  TabControlThreads.SelectedItem = matchingItem.ElementAt(0);
            }
            // if it doesn't exist create it!
            return null;
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

     
        private void ButtonSendFile_Click(object sender, RoutedEventArgs e)
        {
            if ((this.ListBoxInstances.SelectedItems.Count <= 0) && (this.ListBoxInstances.Items.Count > 0))
                this.ListBoxInstances.SelectedIndex = 0;

            List<string> JIDS = new List<string>();
            if (this.ListBoxInstances.SelectedItems.Count > 0)
            {
                foreach (RosterItemPresenceInstance instance in this.ListBoxInstances.SelectedItems)
                {
                    JIDS.Add(instance.FullJID);
                }
            }

            if (JIDS.Count > 0)
            {
                SendFile(XMPPClient, JIDS.ToArray());
            }
        }

        public static void SendFile(XMPPClient XMPPClient, string [] saJIDS)
        {
            // Find the file to send

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                string strFileName = dlg.FileName;

                foreach (string strJID in saJIDS)
                {
                    /// Just send it to 1 recepient for now, must have a full jid
                    string strSendID = XMPPClient.FileTransferManager.SendFile(strFileName, strJID);

                    foreach (Window wind in Application.Current.Windows)
                    {
                        if (wind is MainWindow)
                        {
                            ((MainWindow)wind).ShowFileTransfer();
                            break;
                        }
                    }
                }
            }
        }

        private void ButtonSendScreenCapture_Click(object sender, RoutedEventArgs e)
        {
            if ((this.ListBoxInstances.SelectedItems.Count <= 0) && (this.ListBoxInstances.Items.Count > 0))
                this.ListBoxInstances.SelectedIndex = 0;

            List<string> JIDS = new List<string>();
            if (this.ListBoxInstances.SelectedItems.Count > 0)
            {
                foreach (RosterItemPresenceInstance instance in this.ListBoxInstances.SelectedItems)
                {
                    JIDS.Add(instance.FullJID);
                }
            }

            if (JIDS.Count > 0)
            {
                SendScreenCapture(XMPPClient, JIDS.ToArray());
            }
        }

        public static void SendScreenCapture(XMPPClient XMPPClient, string [] saJIDS)
        {
            List<Window> RestoreList = new List<Window>();
            foreach (Window win in Application.Current.Windows)
            {
                if (win.WindowState != System.Windows.WindowState.Minimized)
                {
                    win.WindowState = System.Windows.WindowState.Minimized;
                    RestoreList.Add(win);
                }
            }

            byte [] bPng = WPFImageWindows.ScreenGrabUtility.GetScreenPNG();

            foreach (Window win in RestoreList)
            {
                win.WindowState = System.Windows.WindowState.Normal;
            }

            if (bPng != null)
            {
                string strFileName = string.Format("sc_{0}.png", Guid.NewGuid());

                foreach (string strJID in saJIDS)
                {
                    /// Just send it to 1 recepient for now, must have a full jid
                    string strSendID = XMPPClient.FileTransferManager.SendFile(strFileName, bPng, strJID);

                    foreach (Window wind in Application.Current.Windows)
                    {
                        if (wind is MainWindow)
                        {
                            ((MainWindow)wind).ShowFileTransfer();
                            break;
                        }
                    }
                }
                
            }
        }


        public void DownloadFinished(string strRequestId, string strLocalFileName, RosterItem itemfrom)
        {
            System.Diagnostics.Process.Start(strLocalFileName);
        }


        private void ButtonStartAudioCall_Click(object sender, RoutedEventArgs e)
        {
            RosterItemPresenceInstance item = ((FrameworkElement)sender).DataContext as RosterItemPresenceInstance;
            if (item == null)
                return;

            foreach (Window win in Application.Current.Windows)
            {
                if (win is AudioMuxerWindow)
                {
                    ((AudioMuxerWindow)win).InitiateOrShowCallTo(item.FullJID);
                    break;
                }
            }
        }

        private void ButtonSendPhotoCapture_Click(object sender, RoutedEventArgs e)
        {
            if ((this.ListBoxInstances.SelectedItems.Count <= 0) && (this.ListBoxInstances.Items.Count > 0))
                this.ListBoxInstances.SelectedIndex = 0;

            List<string> JIDS = new List<string>();
            if (this.ListBoxInstances.SelectedItems.Count > 0)
            {
                foreach (RosterItemPresenceInstance instance in this.ListBoxInstances.SelectedItems)
                {
                    JIDS.Add(instance.FullJID);
                }
            }

            if (JIDS.Count > 0)
            {
                SendPhotoCapture(XMPPClient, JIDS.ToArray());
            }
            
        }

        public static void SendPhotoCapture(XMPPClient XMPPClient, string [] saJIDS)
        {
             CameraCaptureWindow camwin = new CameraCaptureWindow();
            if (camwin.ShowDialog() == true)
            {
                if (camwin.CompressedAcceptedImage != null)
                {
                    string strFileName = string.Format("camera_{0}.jpg", Guid.NewGuid());

                    foreach (string strJID in saJIDS)
                    {
                        /// Just send it to 1 recepient for now, must have a full jid
                        string strSendID = XMPPClient.FileTransferManager.SendFile(strFileName, camwin.CompressedAcceptedImage, strJID);

                        foreach (Window wind in Application.Current.Windows)
                        {
                            if (wind is MainWindow)
                            {
                                ((MainWindow)wind).ShowFileTransfer();
                                break;
                            }
                        }
                    }
               
                }
            }
        }

        private void TabControlThreads_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    
    }
}
