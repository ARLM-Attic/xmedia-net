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
    /// Interaction logic for SendRawXMLWindow.xaml
    /// </summary>
    public partial class SendRawXMLWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        public SendRawXMLWindow()
        {
            InitializeComponent();
        }

        XMPPClient XMPPClient = null;
        public void SetXMPPClient(XMPPClient client)
        {
            this.DataContext = this;
            XMPPClient = client;
            XMPPClient.OnXMLReceived += new System.Net.XMPP.XMPPClient.DelegateString(XMPPClient_OnXMLReceived);
            XMPPClient.OnXMLSent += new System.Net.XMPP.XMPPClient.DelegateString(XMPPClient_OnXMLSent);
        }

        private string m_strLogText = "";

        public string LogText
        {
            get { return m_strLogText; }
            set { m_strLogText = value; }
        }

        void XMPPClient_OnXMLSent(XMPPClient client, string strXML)
        {
            m_strLogText += "\r\n -->" + strXML;
            if (m_strLogText.Length > 8000)
                m_strLogText = m_strLogText.Substring(m_strLogText.Length - 7000);

            AsyncScrollToEnd();
            FirePropertyChanged("LogText");
        }

        void AsyncScrollToEnd()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ScrollToEnd), 1);
        }

        delegate void DelegateObject(object obj);
        void ScrollToEnd(object obj)
        {
            this.Dispatcher.Invoke(new DelegateObject(DoScrollToEnd), obj);
        }

        void DoScrollToEnd(object obj)
        {
            if (this.IsLoaded == true)
                this.textBoxLog.ScrollToEnd();

        }

        void XMPPClient_OnXMLReceived(XMPPClient client, string strXML)
        {
            m_strLogText += "\r\n <--" + strXML;
            if (m_strLogText.Length > 8000)
                m_strLogText = m_strLogText.Substring(m_strLogText.Length - 7000);

            AsyncScrollToEnd(); 
            FirePropertyChanged("LogText");
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            string strSend = this.textBoxSend.Text;
            XMPPClient.SendRawXML(strSend);

            this.textBoxSend.Text = "";
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = System.Windows.Visibility.Hidden;
            base.OnClosing(e);
        }


        
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
            }
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged = null;

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            m_strLogText = "";
            FirePropertyChanged("LogText");
        }

    }
}
