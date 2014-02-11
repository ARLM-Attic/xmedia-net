
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.ComponentModel;
using System.Runtime.Serialization;
using AudioClasses;

using System;
using System.Net;
using System.IO;

namespace RTP
{
    public enum NetworkCameraType
    {
        MotionJpegHttp,
        RTSP,
    }

    [DataContract]
    public class NetworkCameraClientInformation
    {
        public NetworkCameraClientInformation()
        {
        }

        public NetworkCameraClientInformation(string strCameraName, string strCameraURLPath, string strHost, int nPort, string strUser, string strPassword)
        {
            this.CameraName = strCameraName;
            CameraURLPath = strCameraURLPath;
            Computer = strHost;
            Port = nPort;
            UserName = strUser;
            Password = strPassword;
        }

        public string URL
        {
            get
            {
                return string.Format("http://{0}:{1}/{2}", Computer, Port, CameraURLPath);
            }
        }

        private NetworkCameraType m_eNetworkCameraType = NetworkCameraType.MotionJpegHttp;
        [DataMember]
        public NetworkCameraType NetworkCameraType
        {
            get { return m_eNetworkCameraType; }
            set { m_eNetworkCameraType = value; }
        }

        private string m_strComputer = "";
        [DataMember]
        public string Computer
        {
            get { return m_strComputer; }
            set { m_strComputer = value; }
        }

        private int m_nPort = 8090;
        [DataMember]
        public int Port
        {
            get { return m_nPort; }
            set { m_nPort = value; }
        }

        private string m_strUserName = "";
        [DataMember]
        public string UserName
        {
            get { return m_strUserName; }
            set { m_strUserName = value; }
        }
        private string m_strPassword = "";
        [DataMember]
        public string Password
        {
            get { return m_strPassword; }
            set { m_strPassword = value; }
        }

        private string m_strCameraURLPath = "video.cgi?camera=1";
        [DataMember]
        public string CameraURLPath
        {
            get { return m_strCameraURLPath; }
            set { m_strCameraURLPath = value; }
        }

        private string m_strCameraName = "New Camera";
        [DataMember]
        public string CameraName
        {
            get { return m_strCameraName; }
            set { m_strCameraName = value; }
        }

        private string m_strPanLeft = "ptz.cgi?move=left&camera=1";
        [DataMember]
        public string PanLeft
        {
            get 
            {
                if (m_strPanLeft == null)
                    m_strPanLeft = "ptz.cgi?move=left&camera=1";
                return m_strPanLeft; 
            }
            set 
            {
                m_strPanLeft = value; 
            }
        }

        private string m_strPanRight = "ptz.cgi?move=right&camera=1";
        [DataMember]
        public string PanRight
        {
            get 
            {
                if (m_strPanRight == null)
                    m_strPanRight = "ptz.cgi?move=right&camera=1";

                return m_strPanRight; 
            }
            set 
            {
                m_strPanRight = value; 
            }
        }

        private string m_strPanUp = "ptz.cgi?move=up&camera=1";
        [DataMember]
        public string PanUp
        {
            get 
            {
                if (m_strPanUp == null)
                    m_strPanUp = "ptz.cgi?move=up&camera=1";
                return m_strPanUp; 
            }
            set 
            {
                m_strPanUp = value; 
            }
        }

        private string m_strPanDown = "ptz.cgi?move=down&camera=1";
        [DataMember]
        public string PanDown
        {
            get 
            {
                if (m_strPanDown == null)
                    m_strPanDown = "ptz.cgi?move=down&camera=1";
                return m_strPanDown; 
            }
            set 
            {
                m_strPanDown = value; 
            }
        }

        private string m_strFocus = "ptz.cgi?focus=##VALUE##&camera=1";
        [DataMember]
        public string Focus
        {
            get 
            {
                if (m_strFocus == null)
                    m_strFocus = "ptz.cgi?focus=##VALUE##&camera=1";
                return m_strFocus; 
            }
            set 
            {
                m_strFocus = value; 
            }
        }

        private string m_strStartRecord = "action.cgi?record=true&camera=1";
        [DataMember]
        public string StartRecord
        {
            get 
            {
                if (m_strStartRecord == null)
                    m_strStartRecord = "action.cgi?record=true&camera=1";
                return m_strStartRecord; 
            }
            set 
            {
                m_strStartRecord = value; 
            }
        }

        private string m_strStopRecord = "action.cgi?record=false&camera=1";
        [DataMember]
        public string StopRecord
        {
            get 
            {
                if (m_strStopRecord == null)
                    m_strStopRecord = "action.cgi?record=false&camera=1";
                return m_strStopRecord; 
            }
            set 
            {
                m_strStopRecord = value;
            }
        }

        public static NetworkCameraClientInformation[] Load(string strFileName)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(NetworkCameraClientInformation[]));
            try
            {
                System.IO.FileStream stream = new System.IO.FileStream(strFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                NetworkCameraClientInformation[] cameras = (NetworkCameraClientInformation[])serializer.ReadObject(stream);

                stream.Close();
                return cameras;
            }
            catch (Exception ex)
            {
            }
            return null;
  
        }

        public static void Save(string strFileName, NetworkCameraClientInformation[] cameras)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(NetworkCameraClientInformation[]));
            System.IO.FileStream stream = new System.IO.FileStream(strFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            serializer.WriteObject(stream, cameras);
            stream.Close();

        }
    }


    public class MotionJpegClient : IVideoSource, ICameraControl, INotifyPropertyChanged, IVideoRecorder
    {
        public MotionJpegClient(IVideoCompressor jpegcompressor)
        {
            source = new MotionJpegStreamSource(jpegcompressor);
            source.OnNewRawData += new MotionJpegStreamSource.DelegateNewData(source_OnNewRawData);
            source.OnDisconnected += new EventHandler(source_OnDisconnected);
        }

        public MotionJpegClient(IVideoCompressor jpegcompressor, NetworkCameraClientInformation info)
        {
            source = new MotionJpegStreamSource(jpegcompressor);
            source.OnNewRawData += new MotionJpegStreamSource.DelegateNewData(source_OnNewRawData);
            NetworkCameraInformation = info;
            this.m_strName = info.CameraName;
        }

        private NetworkCameraClientInformation m_objNetworkCameraInformation = new NetworkCameraClientInformation();

        public NetworkCameraClientInformation NetworkCameraInformation
        {
            get { return m_objNetworkCameraInformation; }
            set { m_objNetworkCameraInformation = value; }
        }

        MediaSample LastSample = null;
        int nSequence = 0;
        void source_OnNewRawData(byte[] bRawData, int nWidth, int nHeight, int nBytesPerPixel)
        {
            if (OnNewFrame != null)
            {
                LastSample = new MediaSample(bRawData, new VideoCaptureRate(nWidth, nHeight, 10, 10000));
                LastSample.SequenceNumber = ++nSequence;
                /// TODO... convert this to the video format requested by the host
                OnNewFrame(bRawData, LastSample.VideoFormat, this);
            }
        }

        void source_OnDisconnected(object obj, EventArgs args)
        {
            m_bCameraActive = false;
            FirePropertyChanged("CameraActive");
        }

        MotionJpegStreamSource source = null;


     

        private bool m_bCameraActive = false;

        public bool CameraActive
        {
            get { return m_bCameraActive; }
            set 
            {
                if (value != m_bCameraActive)
                {
                    m_bCameraActive = value;

                    if (m_bCameraActive == true)
                    { 
                        /// Start our camera
                        /// 
                        source.UserName = this.NetworkCameraInformation.UserName;
                        source.Password = this.NetworkCameraInformation.Password;
                        FirePropertyChanged("CameraActive");
                        source.Start(this.NetworkCameraInformation.URL);
                    }
                    else
                    {
                        source.Stop();
                        FirePropertyChanged("CameraActive");
                    }
                }
            }
        }

        private double m_nItemWidth = 320;

        public double ItemWidth
        {
            get { return m_nItemWidth; }
            set
            {
                if (m_nItemWidth != value)
                {
                    m_nItemWidth = value;
                    FirePropertyChanged("ItemWidth");
                }
            }
        }
        private double m_nItemHeight = 180;

        public double ItemHeight
        {
            get { return m_nItemHeight; }
            set
            {
                if (m_nItemHeight != value)
                {
                    m_nItemHeight = value;
                    FirePropertyChanged("ItemHeight");
                }
            }
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
            }
        }

        #endregion

        public List<VideoCaptureRate> VideoFormats
        {
            get { return new List<VideoCaptureRate>(); }
        }

        public event DelegateRawFrame OnNewFrame = null;

        public VideoCaptureRate ActiveVideoCaptureRate
        {
            get 
            {
                if (LastSample != null)
                    return LastSample.VideoFormat;

                return new VideoCaptureRate(); 
            }
        }

        string m_strName = "";
        public string Name
        {
            get { return m_strName; }
        }

        public MediaSample PullFrame()
        {
            return LastSample;
        }

        public void PanLeft()
        {
            string strURL = string.Format("http://{0}:{1}/{2}", NetworkCameraInformation.Computer, NetworkCameraInformation.Port, NetworkCameraInformation.PanLeft);
            SendWebRequest(strURL);
        }

        public void PanRight()
        {
            string strURL = string.Format("http://{0}:{1}/{2}", NetworkCameraInformation.Computer, NetworkCameraInformation.Port, NetworkCameraInformation.PanRight);
            SendWebRequest(strURL);
        }

        public void PanRelative(int Units)
        {
            
        }

        public void TiltUp()
        {
            string strURL = string.Format("http://{0}:{1}/{2}", NetworkCameraInformation.Computer, NetworkCameraInformation.Port, NetworkCameraInformation.PanUp);
            SendWebRequest(strURL);
            
        }

        public void TiltDown()
        {
            string strURL = string.Format("http://{0}:{1}/{2}", NetworkCameraInformation.Computer, NetworkCameraInformation.Port, NetworkCameraInformation.PanDown);
            SendWebRequest(strURL);
        }

        public void TiltRelative(int Units)
        {
        }

        public void SetExposure(int nExposure)
        {
            //string strURL = string.Format("http://{0}:{1}/{2}", NetworkCameraInformation.Computer, NetworkCameraInformation.Port, NetworkCameraInformation.SetExposer);
            //SendWebRequest(strURL);
        }

        public void Zoom(int Factor)
        {
            
        }

        public void TurnOffLED()
        {
            
        }

        public bool StartRecording()
        {
            if (m_bRecording == false)
            {
                /// Send out web request to start record
                /// 
                string strURL = string.Format("http://{0}:{1}/{2}", NetworkCameraInformation.Computer, NetworkCameraInformation.Port, NetworkCameraInformation.StartRecord);
                SendWebRequest(strURL);

                m_bRecording = true;
                FirePropertyChanged("Recording");

                return true;
            }

            return false;
        }

        public string StopRecording()
        {
            if (m_bRecording == true)
            {
                string strURL = string.Format("http://{0}:{1}/{2}", NetworkCameraInformation.Computer, NetworkCameraInformation.Port, NetworkCameraInformation.StopRecord);
                SendWebRequest(strURL);

                m_bRecording = false;
                FirePropertyChanged("Recording");

                return "";
            }

            return "";
        }

        bool m_bRecording = false;
        public bool Recording
        {
            get
            {
                return m_bRecording;   
            }
            set
            {
                if (value == true)
                    StartRecording();
                else
                    StopRecording();
                
            }
        }

        protected void SendWebRequest(string strURL)
        {
            WebClient WebClient = new WebClient();
            WebClient.Credentials = new NetworkCredential(this.NetworkCameraInformation.UserName, this.NetworkCameraInformation.Password);
            try
            {
                WebClient.DownloadData(strURL);
            }
            catch (Exception ex)
            {
            }
        }


        private bool m_bVisible = true;

        public bool Visible
        {
            get { return m_bVisible; }
            set { if (m_bVisible != value) { m_bVisible = value; FirePropertyChanged("Visible"); } }
        }

    }
}
