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

namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for AddNewRosterItemWindow.xaml
    /// </summary>
    public partial class AddNewRosterItemWindow : Window
    {
        public AddNewRosterItemWindow()
        {
            InitializeComponent();
        }

        public XMPPClient client = null;
        private void SurfaceButton_Click(object sender, RoutedEventArgs e)
        {
            client.AddToRoster(this.TextBoxJID.Text, this.TextBoxNickname.Text, this.TextBoxGroup.Text);
            this.DialogResult = true;
            this.Close();
        }
    }
}
