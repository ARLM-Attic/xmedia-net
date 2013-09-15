
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.IO;
using System.Net;

namespace RTP
{

    public enum ConnectionType
    {
        MotionJpeg,
        G711Audio,
    }

    public class HTTPServerConnection : System.ComponentModel.INotifyPropertyChanged
    {
        public HTTPServerConnection(HttpListenerContext context, ConnectionType type)
        {
            HttpListenerContext = context;
            ConnectionType = type;
            this.Source = context.Request.RemoteEndPoint.ToString();
        }

        private string m_strBoundary = Guid.NewGuid().ToString();

        public string Boundary
        {
            get { return m_strBoundary; }
            set { m_strBoundary = value; }
        }

        private ConnectionType m_eConnectionType = ConnectionType.MotionJpeg;

        public ConnectionType ConnectionType
        {
            get { return m_eConnectionType; }
            set { m_eConnectionType = value; }
        }

        private HttpListenerContext m_objHttpListenerContext = null;

        public HttpListenerContext HttpListenerContext
        {
            get { return m_objHttpListenerContext; }
            set { m_objHttpListenerContext = value; }
        }

        public xmedianet.socketserver.EventQueueWithNotification<byte[]> FrameQueue = new xmedianet.socketserver.EventQueueWithNotification<byte[]>();

        private string m_strSource = "unknown";

        public string Source
        {
            get { return m_strSource; }
            set { m_strSource = value; }
        }

        private DateTime m_dtConnected = DateTime.Now;

        private int m_nDiscardedFrames = 0;

        public int DiscardedFrames
        {
            get { return m_nDiscardedFrames; }
            set 
            { 
                if (value != m_nDiscardedFrames)
                {
                    m_nDiscardedFrames = value; FirePropertyChanged("DiscardedFrames");
                }
            }
        }

        public DateTime Connected
        {
            get { return m_dtConnected; }
            set { m_dtConnected = value; }
        }

        public virtual void Start()
        { 
        }

        public virtual void Stop()
        {
            try
            {
                if (HttpListenerContext != null)
                   HttpListenerContext.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                HttpListenerContext = null;
            }
        }


         #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strProp)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(strProp));
        }

        #endregion
    }


    public class MotionJpegServerClient : HTTPServerConnection
    {
        public MotionJpegServerClient(MotionJpegHttpServer server, VideoSourceWithSubscribers source, HttpListenerContext context, ConnectionType type)
            : base(context, type)
        {
            Server = server;
            Source = source;
        }

        MotionJpegHttpServer Server = null;
        VideoSourceWithSubscribers Source = null;


        Thread threadSend = null;
        bool m_bExit = false;

        public override void Start()
        {
            threadSend = new Thread(new ThreadStart(SendThread));
            threadSend.IsBackground = true;
            threadSend.Name = "JPEG socket send thread";
            threadSend.Start();

        }


        public override  void Stop()
        {
            m_bExit = true;

            base.Stop();
            if (Server != null)
            {
                Server.RemoveVideoClient(this);
                Server = null;
            }
            if (Source != null)
            {
                Source.RemoveConnection(this);
                Source = null;
            }
        }

        private bool m_bPreviewSmallImage = false;
        public bool PreviewSmallImage
        {
            get { return m_bPreviewSmallImage; }
            set { m_bPreviewSmallImage = value; }
        }
       

        private double m_nMaxFramesPerSecond = 1;
        public double MaxFramesPerSecond
        {
            get { return m_nMaxFramesPerSecond; }
            set { m_nMaxFramesPerSecond = value; }
        }

        DateTime m_dtLastFrameSent = DateTime.MinValue;
        int m_nNumberFramsSent = 0;

        void SendThread()
        {
            while (m_bExit == false)
            {
                byte[][] baFrames = FrameQueue.WaitAll(1000);
                if ((baFrames == null) || (baFrames.Length <= 0))
                    continue;

                if (MaxFramesPerSecond != -1)
                {
                    double fSeconds = (DateTime.Now - m_dtLastFrameSent).TotalSeconds;
                    double fFramesPerSeconds = 1 / fSeconds;

                    if (fFramesPerSeconds > MaxFramesPerSecond)
                        continue;
                }

                DiscardedFrames += baFrames.Length - 1;

                byte[] bLatestImage = baFrames[baFrames.Length - 1];

                byte[] bJpegMutlipartcontent = BuildJpegMutlipartSection(bLatestImage);
                try
                {
                    HttpListenerContext.Response.OutputStream.Write(bJpegMutlipartcontent, 0, bJpegMutlipartcontent.Length);
                    m_nNumberFramsSent++;
                    m_dtLastFrameSent = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Stop();
                    return;
                }
                bJpegMutlipartcontent = null;
                bLatestImage = null;
            }
        }


        public byte[] BuildJpegMutlipartSection(byte [] bImage)
        {
            //--simple boundary 
            //Content-type: image/jpg
            //Content-Length: 3453

            string strContent = string.Format("{0}\r\nContent-type: image/jpeg\r\nContent-Length: {1}\r\n\r\n",
                                            Boundary, bImage.Length);
            byte[] bContent = System.Text.ASCIIEncoding.ASCII.GetBytes(strContent);

            byte[] bTotal = new byte[bContent.Length + bImage.Length];
            Array.Copy(bContent, 0, bTotal, 0, bContent.Length);
            Array.Copy(bImage, 0, bTotal, bContent.Length, bImage.Length);

            return bTotal;
        }

    }
}
