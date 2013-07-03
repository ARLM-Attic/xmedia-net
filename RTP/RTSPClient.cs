//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using System.Net;
//using xmedianet.socketserver;
//using System.ComponentModel;
//using System.Runtime.Serialization;
//using AudioClasses;

//namespace RTP
//{
//    [DataContract]
//    public class RTSPClient : IVideoSource, ICameraControl, INotifyPropertyChanged
//    {
//        public RTSPClient()
//        {
//        }

//        private string m_strURL = "rtsp://192.168.1.140:554/4";
//        [DataMember]
//        public string URL
//        {
//            get { return m_strURL; }
//            set { m_strURL = value; }
//        }

//        private NetworkCameraClientInformation m_objNetworkCameraInformation = new NetworkCameraClientInformation();

//        public NetworkCameraClientInformation NetworkCameraInformation
//        {
//            get { return m_objNetworkCameraInformation; }
//            set { m_objNetworkCameraInformation = value; }
//        }


//        private string m_strUserAgent = "xmedianet client";
//        public string UserAgent
//        {
//            get { return m_strUserAgent; }
//            set { m_strUserAgent = value; }
//        }

//        xmedianet.socketserver.SocketClient Client = new SocketClient();

//        public RTSPBaseMessage Describe()
//        {
//            if (Client.Connected == false)
//                Client.Connect(
//        }


//        public List<VideoCaptureRate> VideoFormats
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public event DelegateRawFrame OnNewFrame;

//        public VideoCaptureRate ActiveVideoCaptureRate
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public string Name
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public MediaSample PullFrame()
//        {
//            throw new NotImplementedException();
//        }

//        public void PanLeft()
//        {
//            throw new NotImplementedException();
//        }

//        public void PanRight()
//        {
//            throw new NotImplementedException();
//        }

//        public void PanRelative(int Units)
//        {
//            throw new NotImplementedException();
//        }

//        public void TiltUp()
//        {
//            throw new NotImplementedException();
//        }

//        public void TiltDown()
//        {
//            throw new NotImplementedException();
//        }

//        public void TiltRelative(int Units)
//        {
//            throw new NotImplementedException();
//        }

//        public void Zoom(int Factor)
//        {
//            throw new NotImplementedException();
//        }

//        public void TurnOffLED()
//        {
//            throw new NotImplementedException();
//        }

//        public event PropertyChangedEventHandler PropertyChanged;
//    }
//}
