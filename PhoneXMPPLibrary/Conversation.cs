using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Reflection;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Threading;
using System.Collections.Generic;

namespace PhoneXMPPLibrary
{
    [DataContract]
    public class TextMessage : System.ComponentModel.INotifyPropertyChanged
    {
        public TextMessage()
        {
        }

        private JID m_objFrom = new JID();
        [DataMember]
        public JID From
        {
            get { return m_objFrom; }
            set
            {
                if (m_objFrom != value)
                {
                    m_objFrom = value;
                    FirePropertyChanged("From");
                }
            }
        }
        
        private JID m_objTo = new JID();
        [DataMember]
        public JID To
        {
            get { return m_objTo; }
            set
            {
                if (m_objTo != value)
                {
                    m_objTo = value;
                    FirePropertyChanged("To");
                }
            }
        }

        public string RemoteEnd
        {
            get
            {
                if (Sent == true)
                    return To.FullJID;
                else
                    return From.FullJID;
            }
            set
            {
            }
        }

        private DateTime m_dtReceived = DateTime.Now;
        [DataMember]
        public DateTime Received
        {
            get { return m_dtReceived; }
            set
            {
                if (m_dtReceived != value)
                {
                    m_dtReceived = value;
                    FirePropertyChanged("Received");
                }
            }
        }

        private string m_strMessage = "";
        [DataMember]
        public string Message
        {
            get { return m_strMessage; }
            set
            {
                if (m_strMessage != value)
                {
                    m_strMessage = value;
                    FirePropertyChanged("Message");
                }
            }
        }


        private string m_strThread = "";
        [DataMember]
        public string Thread
        {
            get { return m_strThread; }
            set
            {
                if (m_strThread != value)
                {
                    m_strThread = value;
                    FirePropertyChanged("Thread");
                }
            }
        }

        private bool m_bSent = false;
        [DataMember]
        public bool Sent
        {
            get { return m_bSent; }
            set { m_bSent = value; }
        }

        public Brush TextColor
        {
            get
            {
                if (Sent == true)
                    return new SolidColorBrush(Colors.Purple);
                else
                    return new SolidColorBrush(Colors.Orange);
            }
            set
            {
            }
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
    
    [DataContract]
    public class Conversation : System.ComponentModel.INotifyPropertyChanged
    {

        public Conversation(JID jid)
        {
            JID = jid;
        }

        private JID m_objJID = null;

        public JID JID
        {
            get { return m_objJID; }
            set { m_objJID = value; }
        }

        delegate void DelegateClear();
        public void Clear()
        {
#if WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(new DelegateClear(DoClear));
#else
            m_listMessages.Clear();
#endif

        }

        void DoClear()
        {
            m_listMessages.Clear();
        }

        delegate void DelegateAddMessage(TextMessage msg);
        public void AddMessage(TextMessage msg)
        {
#if WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(new DelegateAddMessage(DoAddMessage), msg);
#else
            m_listMessages.Add(msg);
#endif
        }

        void DoAddMessage(TextMessage msg)
        {
            m_listMessages.Add(msg);
        }

        private ConversationState m_eConversationState = ConversationState.active;

        public ConversationState ConversationState
        {
            get { return m_eConversationState; }
            set 
            {
                if (m_eConversationState != value)
                {
                    m_eConversationState = value;
                    FirePropertyChanged("ConversationState");
                }
            }
        }



#if WINDOWS_PHONE
        private ObservableCollection<TextMessage> m_listMessages = new ObservableCollection<TextMessage>();
        [DataMember]
        public ObservableCollection<TextMessage> Messages
        {
            get { return m_listMessages; }
            set { m_listMessages = value; }
        }
#else
        private ObservableCollectionEx<TextMessage> m_listMessages = new ObservableCollectionEx<TextMessage>();
        [DataMember]
        public ObservableCollectionEx<TextMessage> Messages
        {
            get { return m_listMessages; }
            set { m_listMessages = value; }
        }
#endif

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

    public enum ConversationState
    {
        none,
        active,
        paused,
        composing,
    }
}
