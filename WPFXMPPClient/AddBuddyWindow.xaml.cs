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

namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for AddBuddyWindow.xaml
    /// </summary>
    public partial class AddBuddyWindow : Window
    {
        public AddBuddyWindow()
        {
            InitializeComponent();
        }

        private string m_strJID = "";

        public string JID
        {
            get { return m_strJID; }
            set { m_strJID = value; }
        }
        
        private string m_strNickName = "";

        public string NickName
        {
            get { return m_strNickName; }
            set { m_strNickName = value; }
        }

        private string m_strGroup = "";

        public string Group
        {
            get { return m_strGroup; }
            set { m_strGroup = value; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NickName = JID;
            this.DataContext = this;
            this.LabelMessage.Content = string.Format("{0} would like to see your presence, Allow?", JID);
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            this.Close();
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;

            this.Close();
        }
    }
}
