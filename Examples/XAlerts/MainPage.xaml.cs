using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Net.XMPP;

namespace XAlerts
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.PubSubSubscriptions;
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.panoramaMain.ItemsSource = App.PubSubSubscriptions;
            //foreach (Subscription sub in App.Subscriptions)
            //{
            //    sub.Events.Items.CollectionChanged += Items_CollectionChanged;
            //}
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //foreach (Subscription sub in App.Subscriptions)
            //{
            //    sub.Events.Items.CollectionChanged -= Items_CollectionChanged;
            //}
            base.OnNavigatedFrom(e);
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.panoramaMain.ItemsSource = null;
            this.panoramaMain.ItemsSource = App.PubSubSubscriptions;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
           

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/ConnectPage.xaml"), UriKind.Relative)); 
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            if (App.client.XMPPState != System.Net.XMPP.XMPPState.Ready)
                return;


            AccountNameInputControl.InputValue = "";
            AccountNameInputControl.ShowAndGetItems();
            this.panoramaMain.IsEnabled = false;

        }

        private void AccountNameInputControl_OnInputSaved(object sender, EventArgs e)
        {
            this.AccountNameInputControl.IsEnabled = true;

            string strNode = AccountNameInputControl.InputValue;
            App.AddNewNode(strNode);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /// Clear
            /// 
            AlertMessage msg = ((Button)sender).DataContext as AlertMessage;
            if (msg == null)
                return;

            foreach (Subscription sub in App.PubSubSubscriptions)
            {
                if (sub.NodeName == msg.AlertNode)
                {
                    sub.Events.DeleteItem(msg.Guid, msg);
                    break;
                }
            }
        }

       
    }
}