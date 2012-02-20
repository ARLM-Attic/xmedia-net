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

using System.IO.IsolatedStorage;
using System.Runtime.Serialization;

namespace System.Net.XMPP
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public LoginWindow()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }


        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }


        private void SurfaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveAccount != null)
                ActiveAccount.Password = this.TextBoxPassword.Password;

            SaveAccounts();

            this.DialogResult = true;
            this.Close();
        }

        public System.Net.XMPP.XMPPAccount m_objActiveAccount = null;

        public System.Net.XMPP.XMPPAccount ActiveAccount
        {
            get { return m_objActiveAccount; }
            set { m_objActiveAccount = value; }
        }

        public List<System.Net.XMPP.XMPPAccount> AllAccounts = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (AllAccounts == null)
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
                {
                    // Load from storage
                    IsolatedStorageFileStream location = null;
                    try
                    {
                        location = new IsolatedStorageFileStream("xmppcred.item", System.IO.FileMode.Open, storage);
                        DataContractSerializer ser = new DataContractSerializer(typeof(List<XMPPAccount>));

                        AllAccounts = ser.ReadObject(location) as List<XMPPAccount>;
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

            if (AllAccounts == null)
                AllAccounts = new List<XMPPAccount>();


            if (AllAccounts.Count <= 0)
                this.AllAccounts.Add(ActiveAccount);

            this.ComboBoxAccounts.ItemsSource = AllAccounts;
            if (this.ComboBoxAccounts.Items.Contains(ActiveAccount) == true)
                this.ComboBoxAccounts.SelectedItem = ActiveAccount;
            else
                this.ComboBoxAccounts.SelectedIndex = 0;
        }

        void SaveAccounts()
        {

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                // Load from storage
                IsolatedStorageFileStream location = new IsolatedStorageFileStream("xmppcred.item", System.IO.FileMode.Create, storage);
                DataContractSerializer ser = new DataContractSerializer(typeof(List<XMPPAccount>));

                try
                {
                    ser.WriteObject(location, AllAccounts);
                }
                catch (Exception ex)
                {
                }
                location.Close();
            }
        }

        private void ComboBoxAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveAccount == null)
                return;
            ActiveAccount.Password = this.TextBoxPassword.Password;

            ActiveAccount = this.ComboBoxAccounts.SelectedItem as XMPPAccount;
            if (ActiveAccount == null)
                return;
            ActiveAccount.LastPrescence.IsDirty = true;
            this.DataContext = ActiveAccount;
            this.TextBoxPassword.Password = ActiveAccount.Password;
            SaveAccounts();
        }

        private void ButtonAddAccount_Click(object sender, RoutedEventArgs e)
        {
            ActiveAccount = new XMPPAccount();
            ActiveAccount.AccountName = "New Account";
            this.AllAccounts.Add(ActiveAccount);
            this.DataContext = ActiveAccount;
            this.ComboBoxAccounts.SelectedItem = ActiveAccount;
            this.TextBoxPassword.Password = ActiveAccount.Password;

        }


        private void TextBoxAccountName_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void TextBoxAccountName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ActiveAccount != null)
            {
                this.ActiveAccount.AccountName = this.TextBoxAccountName.Text;
                this.ComboBoxAccounts.SelectedItem = this.ActiveAccount;
                this.ComboBoxAccounts.Text = this.TextBoxAccountName.Text;
            }
        }


        
    }
}