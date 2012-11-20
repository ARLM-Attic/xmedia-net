using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.XMPP;
using System.IO;
using AudioClasses;
using System.Runtime.Serialization;

namespace WPFXMPPClient
{
    [DataContract]
    public class AudioCodec
    {
        public AudioCodec()
        {
        }

        public override string  ToString()
        {
 	         return Name;
        }
        private string m_strName = "G722";
        [DataMember]
        public string Name
        {
            get { return m_strName; }
            set { m_strName = value; }
        }

        private bool m_bActive = true;
        [DataMember]
        public bool Active
        {
            get { return m_bActive; }
            set { m_bActive = value; }
        }

       // public const AudioCodec G722 = new AudioCodec() { Name="G722" };
       // public const AudioCodec G711 = new AudioCodec() { Name="G711" };
       // public const AudioCodec SPEEX16 = new AudioCodec() { Name="SPEEX" };
    }

    [DataContract]
    public class Option
    {
        public Option()
        {
        }

        public static Option Options = new Option();

        public static void Load()
        {
            
            DataContractSerializer ser = new DataContractSerializer(typeof(Option));

            string strPath = string.Format("{0}\\WPFXMPPClient", Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));
            if (Directory.Exists(strPath) == false)
                Directory.CreateDirectory(strPath);
            string strFileName = string.Format("{0}\\config.xml", strPath);
            if (File.Exists(strFileName) == false)
                return;

            System.IO.FileStream stream = null;
            try
            {
                stream = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                Option.Options = ser.ReadObject(stream) as Option;
            }
            catch (Exception)
            {
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
        }

        public static void Save()
        {
            DataContractSerializer ser = new DataContractSerializer(typeof(Option));

            string strPath = string.Format("{0}\\WPFXMPPClient", Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));
            if (Directory.Exists(strPath) == false)
                Directory.CreateDirectory(strPath);
            string strFileName = string.Format("{0}\\config.xml", strPath);
            System.IO.FileStream stream = new FileStream(strFileName, FileMode.Create, FileAccess.Write, FileShare.None);

            ser.WriteObject(stream, Option.Options);
            stream.Close();
            stream.Dispose();

        }

        /// <summary>
        ///  Set the members of our XMPP client that we can
        /// </summary>
        /// <param name="client"></param>
        public void SetToXMPPClient(XMPPClient client)
        {
        }

        


        private AnswerType m_eAnswerType = AnswerType.AcceptToConference;
        [DataMember]
        public AnswerType AnswerType
        {
            get { return m_eAnswerType; }
            set { m_eAnswerType = value; }
        }

        private Guid m_guidSpeakerDevice = Guid.Empty;
        [DataMember]
        public Guid SpeakerDevice
        {
            get { return m_guidSpeakerDevice; }
            set { m_guidSpeakerDevice = value; }
        }

        private Guid m_guidMicrophoneDevice = Guid.Empty;
        [DataMember]
        public Guid MicrophoneDevice
        {
            get { return m_guidMicrophoneDevice; }
            set { m_guidMicrophoneDevice = value; }
        }

        private AudioCodec m_objAudioCodec = null;
        [DataMember]
        public AudioCodec AudioCodec
        {
            get { return m_objAudioCodec;  }
            set { m_objAudioCodec = value; }
        }

        private Guid m_guidVideoDevice = Guid.Empty;
        [DataMember]
        public Guid VideoDevice
        {
            get { return m_guidVideoDevice; }
            set { m_guidVideoDevice = value; }
        }

        private VideoCaptureRate m_objVideoCaptureRate = new VideoCaptureRate();
        [DataMember]
        public VideoCaptureRate VideoCaptureRate
        {
            get { return m_objVideoCaptureRate; }
            set { m_objVideoCaptureRate = value; }
        }

        private string m_strVideoCodec = "MJPEG";
        [DataMember]
        public string VideoCodec
        {
            get { return m_strVideoCodec; }
            set { m_strVideoCodec = value; }
        }

        private bool m_bUseLocalSOCKS5Proxy = true;
        [DataMember]
        public bool UseLocalSOCKS5Proxy
        {
            get { return m_bUseLocalSOCKS5Proxy; }
            set 
            { 
                m_bUseLocalSOCKS5Proxy = value;
                FileTransferManager.UseLocalSOCKS5Server = value;
            }
        }

        private string m_strProxyPublicIP = null;
        [DataMember]
        public string ProxyPublicIP
        {
            get { return m_strProxyPublicIP; }
            set 
            { 
                m_strProxyPublicIP = value;
                FileTransferManager.SOCKS5ByteServerPublicIP = value;
            }
        }

        private int m_nProxyPort = 0;
        [DataMember]
        public int ProxyPort
        {
            get { return m_nProxyPort; }
            set 
            { 
                m_nProxyPort = value;
                FileTransferManager.SOCKS5ByteServerPort = value;
            }
        }


        private bool m_bUseIBBOnly = false;
        [DataMember]
        public bool UseIBBOnly
        {
            get { return m_bUseIBBOnly; }
            set 
            { 
                m_bUseIBBOnly = value;
                FileTransferManager.UseIBBOnly = value;
            }
        }

        private string m_strFileTransferDirectory = string.Format("{0}\\XMPPFiles\\", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
        [DataMember]
        public string FileTransferDirectory
        {
            get { return m_strFileTransferDirectory; }
            set 
            { 
                m_strFileTransferDirectory = value; 
            }
        }

        private bool m_bSingleRosterItemMap = true;
        [DataMember]
        public bool SingleRosterItemMap
        {
            get { return m_bSingleRosterItemMap; }
            set
            {
                m_bSingleRosterItemMap = value;
                LocationClasses.MapManager.SingleRosterItemMap = value;
            }
        }

        private bool m_bUseLegacyMapWindow = true;
        [DataMember]
        public bool UseLegacyMapWindow
        {
            get { return m_bUseLegacyMapWindow; }
            set
            {
                m_bUseLegacyMapWindow = value;
                
            }
        }

        private bool m_bMapDebugOn = false;
        [DataMember]
        public bool MapDebugOn
        {
            get { return m_bMapDebugOn; }
            set
            {
                m_bMapDebugOn = value;

            }
        }

        [DataMember]
        public int StartPort
        {
            get { return AudioMuxerWindow.FirstPort; }
            set 
            { 
                AudioMuxerWindow.FirstPort = value;
                AudioMuxerWindow.PortOn = value;

            }
        }

        [DataMember]
        public int EndPort
        {
            get { return AudioMuxerWindow.LastPort; }
            set 
            { 
                AudioMuxerWindow.LastPort = value;
            }
        }
    }

}
