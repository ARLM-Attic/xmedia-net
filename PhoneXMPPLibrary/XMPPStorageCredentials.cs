﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Runtime.Serialization;
namespace System.Net.XMPP
{
    [DataContract]
    public class XMPPAccount : System.ComponentModel.INotifyPropertyChanged
    {
        public XMPPAccount()
        {
        }

        public override string ToString()
        {
            return AccountName;
        }

        private bool m_bHaveSuccessfullyConnectedAndAuthenticated = false;

        public bool HaveSuccessfullyConnectedAndAuthenticated
        {
            get { return m_bHaveSuccessfullyConnectedAndAuthenticated; }
            set { m_bHaveSuccessfullyConnectedAndAuthenticated = value; }
        }

        private string m_strAccountName = null;
        [DataMember]
        public string AccountName
        {
            get { return m_strAccountName; }
            set 
            { 
                m_strAccountName = value;
                HaveSuccessfullyConnectedAndAuthenticated = false;
                FirePropertyChanged("AccountName");
            }
        }

        
        public string m_strAvatarHash = null;
        [DataMember]
        public string AvatarHash
        {
            get { return m_strAvatarHash; }
            set 
            { 
                m_strAvatarHash = value; 
            }
        }

        
        public string m_strServer = "talk.google.com";
        [DataMember]
        public string Server
        {
            get { return m_strServer; }
            set
            {
                HaveSuccessfullyConnectedAndAuthenticated = false;
                m_strServer = value; 
            }
        }

        
        public string m_strPassword = "";
        [DataMember]
        public string Password
        {
            get { return m_strPassword; }
            set
            {
                HaveSuccessfullyConnectedAndAuthenticated = false;
                m_strPassword = value; 
            }
        }

        public int m_nPort = 5222;
        [DataMember]
        public int Port
        {
          get { return m_nPort; }
          set 
          {
              HaveSuccessfullyConnectedAndAuthenticated = false;
              m_nPort = value; 
          }

        }

        private bool m_bUseOldSSLMethod = false;
        [DataMember]
        public bool UseOldSSLMethod
        {
            get { return m_bUseOldSSLMethod; }
            set 
            {
                HaveSuccessfullyConnectedAndAuthenticated = false;
                m_bUseOldSSLMethod = value; 
            }
        }

        
        public bool m_bUseTLSMethod = true;
        [DataMember]
        public bool UseTLSMethod
        {
            get { return m_bUseTLSMethod; }
            set 
            {
                HaveSuccessfullyConnectedAndAuthenticated = false;
                m_bUseTLSMethod = value; 
            }
        }
        
        public JID m_objJID = new JID();
        [DataMember]
        public JID JID
        {
            get { return m_objJID; }
            set 
            {
                HaveSuccessfullyConnectedAndAuthenticated = false;
                m_objJID = value; 
            }
        }

        public string User
        {
            get
            {
                return m_objJID.User;
            }
            set
            {
                m_objJID.User = value;
                HaveSuccessfullyConnectedAndAuthenticated = false;
            }
        }

        public string Domain
        {
            get
            {
                return m_objJID.Domain;
            }
            set
            {
                m_objJID.Domain = value;
                HaveSuccessfullyConnectedAndAuthenticated = false;
            }
        }

        public string Resource
        {
            get
            {
                return m_objJID.Resource;
            }
            set
            {
                m_objJID.Resource = value;
                HaveSuccessfullyConnectedAndAuthenticated = false;
            }
        }


        private bool m_bUseSOCKSProxy = false;
        [DataMember]
        public bool UseSOCKSProxy
        {
            get { return m_bUseSOCKSProxy; }
            set { m_bUseSOCKSProxy = value; }
        }

        private string m_strProxyName = "";
        [DataMember]
        public string ProxyName
        {
            get { return m_strProxyName; }
            set { m_strProxyName = value; }
        }

        private int m_nProxyPort = 8080;
        [DataMember]
        public int ProxyPort
        {
            get { return m_nProxyPort; }
            set { m_nProxyPort = value; }
        }

        private int m_nSOCKSVersion = 5;
        [DataMember]
        public int SOCKSVersion
        {
            get { return m_nSOCKSVersion; }
            set { m_nSOCKSVersion = value; }
        }


        #region INotifyPropertyChanged Members

        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
            {
#if WINDOWS_PHONE
                Deployment.Current.Dispatcher.BeginInvoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#else
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#endif
            }

            //if (PropertyChanged != null)
            //    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strName));
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged = null;

        #endregion

    }
}
