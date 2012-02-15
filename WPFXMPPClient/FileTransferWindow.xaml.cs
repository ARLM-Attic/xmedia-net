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

namespace WPFXMPPClient
{
    /// <summary>
    /// Interaction logic for FileTransferWindow.xaml
    /// </summary>
    public partial class FileTransferWindow : Window
    {
        public FileTransferWindow()
        {
            InitializeComponent();
        }

        public XMPPClient XMPPClient = null;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ButtonState == MouseButtonState.Pressed))
                this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /// Bind to our XMPP Client filetransfer
            /// 
            this.DataContext = XMPPClient;
            this.ListBoxConversation.ItemsSource = XMPPClient.FileTransferManager.FileTransfers;
        }

        private void ButtonCancelSend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonAcceptTransfer_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                XMPPClient.FileTransferManager.AcceptFileDownload(trans);
            }
        }

        private void ButtonDeclineTransfer_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                XMPPClient.FileTransferManager.DeclineFileDownload(trans);
            }
        }

        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.InitialDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                dlg.FileName = trans.FileName;
                if (dlg.ShowDialog() == true)
                {
                    FileStream stream = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
                    stream.Write(trans.Bytes, 0, trans.Bytes.Length);
                    stream.Close();
                }
                
            }
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            FileTransfer trans = ((FrameworkElement)sender).DataContext as FileTransfer;
            if (trans != null)
            {
                string strFullFileName = string.Format("{0}\\{1}", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), trans.FileName);
                if (File.Exists(strFullFileName) == false)
                {
                    FileStream stream = new FileStream(strFullFileName, FileMode.Create, FileAccess.Write);
                    stream.Write(trans.Bytes, 0, trans.Bytes.Length);
                    stream.Close();
                }
                System.Diagnostics.Process.Start(strFullFileName);
            }
        }
    }
}
