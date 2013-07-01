
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.ComponentModel;
using System.Runtime.Serialization;
using AudioClasses;

namespace RTP
{
    [DataContract]
    public class MotionJpegClientInformation
    {
        public MotionJpegClientInformation()
        {
        }

        public MotionJpegClientInformation(string strCameraName, string strCameraURLPath, string strHost, int nPort, string strUser, string strPassword)
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

        public static MotionJpegClientInformation[] Load(string strFileName)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(MotionJpegClientInformation[]));
            try
            {
                System.IO.FileStream stream = new System.IO.FileStream(strFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                MotionJpegClientInformation[] cameras = (MotionJpegClientInformation[])serializer.ReadObject(stream);

                stream.Close();
                return cameras;
            }
            catch (Exception ex)
            {
            }
            return null;
  
        }

        public static void Save(string strFileName, MotionJpegClientInformation[] cameras)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(MotionJpegClientInformation[]));
            System.IO.FileStream stream = new System.IO.FileStream(strFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            serializer.WriteObject(stream, cameras);
            stream.Close();

        }
    }


    public class MotionJpegClient : IVideoSource, ICameraControl, INotifyPropertyChanged
    {
        public MotionJpegClient(IVideoCompressor jpegcompressor)
        {
            source = new MotionJpegStreamSource(jpegcompressor);
            source.OnNewRawData += new MotionJpegStreamSource.DelegateNewData(source_OnNewRawData);
            source.OnDisconnected += new EventHandler(source_OnDisconnected);
        }

        public MotionJpegClient(IVideoCompressor jpegcompressor, MotionJpegClientInformation info)
        {
            source = new MotionJpegStreamSource(jpegcompressor);
            source.OnNewRawData += new MotionJpegStreamSource.DelegateNewData(source_OnNewRawData);
            NetworkCameraInformation = info;
            this.m_strName = info.CameraName;
        }

        private MotionJpegClientInformation m_objNetworkCameraInformation = new MotionJpegClientInformation();

        public MotionJpegClientInformation NetworkCameraInformation
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
            
        }

        public void PanRight()
        {
            
        }

        public void PanRelative(int Units)
        {
            
        }

        public void TiltUp()
        {
            
        }

        public void TiltDown()
        {
            
        }

        public void TiltRelative(int Units)
        {
            
        }

        public void Zoom(int Factor)
        {
            
        }

        public void TurnOffLED()
        {
            
        }
    }
}
