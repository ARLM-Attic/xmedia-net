﻿using System;
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

namespace GroceryList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GroceryNode = new PubSubNodeManager<GroceryItem>(NodeName, XMPPClient);
        }

        public XMPPClient XMPPClient = new XMPPClient();
        PubSubNodeManager<GroceryItem> GroceryNode = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginWindow loginwin = new LoginWindow();
            loginwin.ActiveAccount = XMPPClient.XMPPAccount;
            //loginwin.AllAccounts = AllAccounts;
            if (loginwin.ShowDialog() == false)
                return;
            if (loginwin.ActiveAccount == null)
            {
                MessageBox.Show("Login window returned null account", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            XMPPClient.XMPPAccount = loginwin.ActiveAccount;
          //  AllAccounts = loginwin.AllAccounts;
            //PubSubListManagerWindow.Closed += new EventHandler(PubSubListManagerWindow_Closed);
            //PubSubListManagerWindow.XMPPClient = XMPPClient;

            ///// TODO, user your own server and accounts, not mine :)
            //XMPPClient.XMPPAccount.User = "test";
            //XMPPClient.XMPPAccount.Password = "test";
            //XMPPClient.XMPPAccount.Server = "ninethumbs.com";
            //XMPPClient.XMPPAccount.Domain = "ninethumbs.com";
            //XMPPClient.XMPPAccount.Resource = Guid.NewGuid().ToString();
            //XMPPClient.XMPPAccount.Port = 5222;

            //XMPPClient.AutoAcceptPresenceSubscribe = false;
            //XMPPClient.AutomaticallyDownloadAvatars = false;
            //XMPPClient.RetrieveRoster = false;

            XMPPClient.OnStateChanged += new EventHandler(XMPPClient_OnStateChanged);
            XMPPClient.Connect();

            

            this.ListViewGroceryList.ItemsSource = GroceryNode.Items;
        }

        void PubSubListManagerWindow_Closed(object sender, EventArgs e)
        {
            PubSubListManagerWindow = null;
            // throw new NotImplementedException();
        }

        void XMPPClient_OnStateChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("XMPPClient State Changed: " + XMPPClient.XMPPState.ToString());
            Console.WriteLine("XMPPClient State Changed: " + XMPPClient.XMPPState.ToString());

            if (XMPPClient.XMPPState == XMPPState.Ready)
            {
              

                XMPPClient.AddLogic(GroceryNode);
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(CheckCreateNode));
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            bool bUnsubscribed = PubSubOperation.UnsubscribeNode(XMPPClient, NodeName, XMPPClient.JID, subid, true);
            base.OnClosing(e);
        }

        public const string NodeName = "GroceryList";

        public void CheckCreateNode(object obj)
        {
            bool bExists = PubSubOperation.NodeExists(XMPPClient, NodeName);
            if (bExists == false)
            {
                PubSubConfigForm config = new PubSubConfigForm();
                config.AccessModel = "open";
                config.AllowSubscribe = true;
                config.DeliverNotifications = true;
                config.DeliverPayloads = true;
                config.MaxItems = "500";
                config.MaxPayloadSize = "16384";
                config.NodeName = NodeName;
                config.NodeType = "leaf";
                config.NotifyConfig = true;
                config.NotifyRetract = true;
                config.PublishModel = "open";
                config.PersistItems = true;
                config.ChildNodes = null;
                config.Collection = null;
                config.ItemExpire = "86400";
                config.Title = "My grocery list";
                PubSubOperation.CreateNode(XMPPClient, NodeName, null, config);
            }

             subid = PubSubOperation.SubscribeNode(XMPPClient, NodeName, XMPPClient.JID, true);
             GroceryNode.GetAllItems(subid);

        }
        string subid = null;

        private void ButtonAddToGroceryList_Click(object sender, RoutedEventArgs e)
        {
            GroceryItem item = new GroceryItem() { Name = this.TextBoxNewGroceryItem.Text, Price=this.TextBoxPrice.Text, Person=XMPPClient.JID };

            GroceryNode.AddItem(item.ItemId, item);
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            GroceryItem item = ((FrameworkElement)sender).DataContext as GroceryItem;
            if (item != null)
            {
                GroceryNode.DeleteItem(item.ItemId, item);
            }
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            GroceryItem item = ((FrameworkElement)sender).DataContext as GroceryItem;
            if (item != null)
            {
                item.Person = XMPPClient.JID;
                GroceryNode.UpdateItem(item.ItemId, item);
            }
        }

        private void ButtonLoadPubSubManager_Click(object sender, RoutedEventArgs e)
        {
            ShowPubSubListManagerWindow();
        }

        private void ShowPubSubListManagerWindow()
        {
            if (PubSubListManagerWindow == null)
            {
                PubSubListManagerWindow = new GroceryList.PubSubListManagerWindow();
                PubSubListManagerWindow.Closed += new EventHandler(PubSubListManagerWindow_Closed);
                PubSubListManagerWindow.XMPPClient = XMPPClient;
            }

            if (PubSubListManagerWindow.IsLoaded == false)
            {
                PubSubListManagerWindow.Show();
            }
            else
            {
                PubSubListManagerWindow.Visibility = System.Windows.Visibility.Visible;
            }
        }

        PubSubListManagerWindow PubSubListManagerWindow = null;

        private void Window_Closed(object sender, EventArgs e)
        {
            if (PubSubListManagerWindow != null && PubSubListManagerWindow.IsLoaded)
                PubSubListManagerWindow.Close();
        }
    }
}
