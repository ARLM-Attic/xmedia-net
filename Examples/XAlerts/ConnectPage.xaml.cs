using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using System.Net.XMPP;


namespace XAlerts
{
    public partial class ConnectPage : PhoneApplicationPage
    {
        public ConnectPage()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists("xmppcred.item") == true)
                {
                    // Load from storage
                    IsolatedStorageFileStream location = null;
                    try
                    {
                        location = new IsolatedStorageFileStream("xmppcred.item", System.IO.FileMode.Open, storage);
                        DataContractSerializer ser = new DataContractSerializer(typeof(XMPPAccount));

                        App.client.XMPPAccount = ser.ReadObject(location) as XMPPAccount;
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

            this.TextBoxPassword.Password = App.client.Password;

            if ((App.client.Resource == null) || (App.client.Resource.Length <= 0))
            {
                Random rand = new Random();
                App.client.Resource = string.Format("phone_{0}", rand.Next());
            }
            this.DataContext = App.client.XMPPAccount;
            
            base.OnNavigatedTo(e);
        }



        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (App.client.XMPPState == System.Net.XMPP.XMPPState.Connected)
                App.client.Disconnect();

            /// store the password in memory before serializing in case the user doesn't want them save to disk
            App.client.JID = this.TextBoxUserName.Text;
            App.client.Password = this.TextBoxPassword.Password;
            App.client.Server = App.client.Domain;

            string strPassword = App.client.XMPPAccount.Password;

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Load from storage
                IsolatedStorageFileStream location = new IsolatedStorageFileStream("xmppcred.item", System.IO.FileMode.Create, storage);
                DataContractSerializer ser = new DataContractSerializer(typeof(XMPPAccount));

                try
                {
                    ser.WriteObject(location, App.client.XMPPAccount);
                }
                catch (Exception)
                {
                }
                location.Close();
            }


            App.client.XMPPAccount.Password = strPassword;

            //if (e.Uri.PathAndQuery == "/MainPage.xaml")
            //{
            App.Connect();
            //}

            
          //  NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)); 
        }

   

        private void TextBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            App.client.Password = this.TextBoxPassword.Password;

        }


    }
}