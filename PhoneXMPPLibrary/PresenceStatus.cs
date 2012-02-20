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

using System.Runtime.Serialization;

namespace System.Net.XMPP
{
    public enum PresenceType
    {
        available, // use Empty instead of this... there is no available value
        error,
        probe,
        subscribe,
        subscribed,
        unavailable,
        unsubscribe,
        unsubscribed
    }

    public enum PresenceShow
    {
        unknown,
        away,
        chat,
        dnd,
        xa
    }

    [DataContract]
    public class PresenceStatus : System.ComponentModel.INotifyPropertyChanged
    {
        public PresenceStatus()
        {
        }

        public override string ToString()
        {
            return string.Format("Presence: {0}, Show: {1}, Status: {2}", PresenceType, PresenceShow, Status);
        }

        private bool m_bIsDirty = true;

        public bool IsDirty
        {
            get { return m_bIsDirty; }
            set { m_bIsDirty = value; }
        }

        private PresenceType m_ePresence = PresenceType.unavailable;
        [DataMember]
        public PresenceType PresenceType
        {
            get { return m_ePresence; }
            set
            {
                if (m_ePresence != value)
                {
                    m_ePresence = value;
                    FirePropertyChanged("PresenceType");
                    FirePropertyChanged("Presence");
                    FirePropertyChanged("PresenceColor");
                    FirePropertyChanged("PresenceBrush");
                    IsDirty = true;
                }
            }
        }

        public Color  PresenceColor
        {
            get
            {
                if (m_ePresenceShow == System.Net.XMPP.PresenceShow.unknown)
                    return Color.FromArgb(0, 0, 0, 0);
                if (m_ePresenceShow == System.Net.XMPP.PresenceShow.dnd)
                    return Colors.Red;
                else if (m_ePresenceShow == System.Net.XMPP.PresenceShow.away)
                    return Colors.Orange;
                else if (m_ePresenceShow == System.Net.XMPP.PresenceShow.xa)
                {
#if WINDOWS_PHONE
                    if (string.Compare(Status, "online", StringComparison.CurrentCultureIgnoreCase) == 0)
#else
                    if (string.Compare(Status, "online", true) == 0)
#endif
                        return Color.FromArgb(255, 64, 255, 64);
#if WINDOWS_PHONE
                    if (string.Compare(Status, "extended away", StringComparison.CurrentCultureIgnoreCase) == 0)
#else
                    if (string.Compare(Status, "extended away", true) == 0)
#endif
                        return Colors.Orange;


                    return Colors.Purple;
                }
                else if (m_ePresenceShow == System.Net.XMPP.PresenceShow.chat)
                    return Color.FromArgb(255, 128, 255, 128);
                else
                    return Colors.Purple;
            }
            set
            {
            }
        }

        public Brush PresenceBrush
        {
            get
            {
               return new SolidColorBrush(PresenceColor);
            }
            set
            {
            }
        }

        private PresenceShow m_ePresenceShow = PresenceShow.unknown;
        [DataMember]
        public PresenceShow PresenceShow
        {
            get { return m_ePresenceShow; }
            set 
            {
                if (m_ePresenceShow != value)
                {
                    m_ePresenceShow = value;
                    FirePropertyChanged("PresenceShow");
                    FirePropertyChanged("PresenceColor");
                    FirePropertyChanged("PresenceBrush");
                    IsDirty = true;
                }
            }

        }

        private string m_strStatus = "unknown";
        [DataMember]
        public string Status
        {
            get { return m_strStatus; }
            set
            {
                if (m_strStatus != value)
                {
                    m_strStatus = value;
                    FirePropertyChanged("Status");
                    FirePropertyChanged("PresenceColor");
                    FirePropertyChanged("PresenceBrush");
                    IsDirty = true;
                }
            }
        }

        private sbyte m_nPriority = 0;
        [DataMember]
        public sbyte Priority
        {
            get { return m_nPriority; }
            set
            {
                if (m_nPriority != value)
                {
                    m_nPriority = value;
                    FirePropertyChanged("Priority");
                    IsDirty = true;
                }
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
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(PropertyChanged, this, new System.ComponentModel.PropertyChangedEventArgs(strName));
#endif
            }
            

            //if (PropertyChanged != null)
            //    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strName));
        }
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged = null;

        #endregion
    }
}
