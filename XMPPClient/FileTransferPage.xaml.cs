using System;
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

using System.Net.XMPP;
using Microsoft.Xna.Framework.Media;

namespace XMPPClient
{
    public partial class FileTransferPage : PhoneApplicationPage
    {
        public FileTransferPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.DataContext = App.XMPPClient;
            this.ListBoxFileTransfers.ItemsSource = App.XMPPClient.FileTransferManager.FileTransfers;
        }

        private void ButtonCancelSend_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                App.XMPPClient.FileTransferManager.CancelSendFile(trans);
            }
        }

        private void ButtonAcceptTransfer_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                App.XMPPClient.FileTransferManager.AcceptFileDownload(trans);
            }

        }

        private void ButtonDeclineTransfer_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                App.XMPPClient.FileTransferManager.DeclineFileDownload(trans);
            }

        }

        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                var library = new MediaLibrary();
                library.SavePicture(trans.FileName, trans.Bytes);
                MessageBox.Show("File saved to 'Saved Pictures'", "File Saved", MessageBoxButton.OK);
            }
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}