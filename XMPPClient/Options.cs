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

namespace XMPPClient
{
    [DataContract]
    public class Options
    {
        public Options()
        {}

        private bool m_bRunWithScreenLocked = true;
        [DataMember]
        public bool RunWithScreenLocked
        {
            get { return m_bRunWithScreenLocked; }
            set { m_bRunWithScreenLocked = value; }
        }

        private bool m_bLogXML = false;
        [DataMember]
        public bool LogXML
        {
            get { return m_bLogXML; }
            set { m_bLogXML = value; }
        }

        private bool m_bSendGeoCoordinates = false;
        [DataMember]
        public bool SendGeoCoordinates
        {
            get { return m_bSendGeoCoordinates; }
            set { m_bSendGeoCoordinates = value; }
        }

        private bool m_bSavePasswords = true;
        [DataMember]
        public bool SavePasswords
        {
            get { return m_bSavePasswords; }
            set { m_bSavePasswords = value; }
        }

        private bool m_bUseOnlyIBBFileTransfer = false;
        [DataMember]
        public bool UseOnlyIBBFileTransfer
        {
            get { return m_bUseOnlyIBBFileTransfer; }
            set { m_bUseOnlyIBBFileTransfer = value; }
        }

        private string m_strSOCKS5ByteStreamProxy = null;
        [DataMember]
        public string SOCKS5ByteStreamProxy
        {
            get { return m_strSOCKS5ByteStreamProxy; }
            set { m_strSOCKS5ByteStreamProxy = value; }
        }

        private bool m_bPlaySoundOnNewMessage = true;
        [DataMember]
        public bool PlaySoundOnNewMessage
        {
            get { return m_bPlaySoundOnNewMessage; }
            set { m_bPlaySoundOnNewMessage = value; }
        }

        private bool m_BVibrateOnNewMessage = true;
        [DataMember]
        public bool VibrateOnNewMessage
        {
            get { return m_BVibrateOnNewMessage; }
            set { m_BVibrateOnNewMessage = value; }
        }
    }
}
