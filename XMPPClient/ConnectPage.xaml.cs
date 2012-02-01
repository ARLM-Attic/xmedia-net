﻿using System;
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

using System.Runtime.Serialization;
using System.IO.IsolatedStorage;
using PhoneXMPPLibrary;

namespace XMPPClient
{

    public partial class ConnectPage : PhoneApplicationPage
    {
        public ConnectPage()
        {
            InitializeComponent();
        }

        List<XMPPAccount> Accounts = new List<XMPPAccount>();

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            XMPPAccount cred = null;
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Load from storage
                IsolatedStorageFileStream location = null;
                try
                {
                    location = new IsolatedStorageFileStream("xmppcred.item", System.IO.FileMode.Open, storage);
                    DataContractSerializer ser = new DataContractSerializer(typeof(List<XMPPAccount>));

                    Accounts = ser.ReadObject(location) as List<XMPPAccount>;
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

            if (Accounts.Count <= 0)
            {
                AccountNameInputControl.InputValue = "New Account";
                AccountNameInputControl.ShowAndGetItems();
                this.MainScrollViewer.IsEnabled = false;
                this.AccountPicker.IsEnabled = false;
                base.OnNavigatedTo(e);
                return;
            }

            this.AccountPicker.ItemsSource = Accounts;
            this.AccountPicker.SelectedItem = Accounts[0];
            //this.AccountPicker.SelectedItem = App.XMPPClient.XMPPAccount; /// can't do this until we hash/equal by name, 

            base.OnNavigatedTo(e);
        }


        private void AccountPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (AccountPicker == null)
                return;

            XMPPAccount acc = AccountPicker.SelectedItem as XMPPAccount;
            if (acc == null)
                return;
            this.TextBoxPassword.Password = acc.Password;
            this.DataContext = acc;
        }


        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Load from storage
                IsolatedStorageFileStream location = new IsolatedStorageFileStream("xmppcred.item", System.IO.FileMode.Create, storage);
                DataContractSerializer ser = new DataContractSerializer(typeof(List<XMPPAccount>));

                try
                {
                    ser.WriteObject(location, Accounts);
                }
                catch (Exception ex)
                {
                }
                location.Close();
            }


            base.OnNavigatedFrom(e);
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            App.XMPPClient.XMPPAccount = this.AccountPicker.SelectedItem as XMPPAccount;

            if (App.XMPPClient.XMPPState == PhoneXMPPLibrary.XMPPState.Connected)
                App.XMPPClient.Disconnect();



            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Load from storage
                IsolatedStorageFileStream location = new IsolatedStorageFileStream("xmppcred.item", System.IO.FileMode.Create, storage);
                DataContractSerializer ser = new DataContractSerializer(typeof(List<XMPPAccount>));

                try
                {
                    ser.WriteObject(location, Accounts);
                }
                catch (Exception ex)
                {
                }
                location.Close();
            }



            App.XMPPClient.Connect();
            //NavigationService.GoBack();
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative)); 


                
        }

        private void ButtonNewAccount_Click(object sender, EventArgs e)
        {
            AccountNameInputControl.InputValue = "New Account";
            AccountNameInputControl.ShowAndGetItems();
            this.MainScrollViewer.IsEnabled = false;
            this.AccountPicker.IsEnabled = false;
           
        }

        private void AccountNameInputControl_OnInputSaved(object sender, EventArgs e)
        {
            this.MainScrollViewer.IsEnabled = true;
            this.AccountPicker.IsEnabled = true;
            XMPPAccount XMPPAccount = new XMPPAccount();
            XMPPAccount.JID = "user@gmail.com/phone";
            XMPPAccount.Server = "talk.google.com";
            XMPPAccount.Port = 5223;
            XMPPAccount.UseOldSSLMethod = true;
            XMPPAccount.UseTLSMethod = true;
            XMPPAccount.AccountName = AccountNameInputControl.InputValue;
            Accounts.Add(XMPPAccount);

            List<XMPPAccount> NewAccounts = new List<XMPPAccount>();
            NewAccounts.AddRange(Accounts);

            this.AccountPicker.ItemsSource = NewAccounts;
            Accounts = NewAccounts;
            this.AccountPicker.SelectedItem = XMPPAccount;
            this.DataContext = XMPPAccount;
        }

        private void TextBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            XMPPAccount acc = AccountPicker.SelectedItem as XMPPAccount;
            if (acc == null)
                return;
            acc.Password = this.TextBoxPassword.Password;

        }

     
    }
}