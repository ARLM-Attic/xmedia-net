
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;
using System.Net;
using System.Collections.ObjectModel;
using AudioClasses;
using System.Runtime.InteropServices;


namespace RTP
{
    public class VideoSourceWithSubscribers
    {
        public VideoSourceWithSubscribers(IVideoSource source, IVideoCompressor jpegcompressor, int nIndex)
        {
            Source = source;
            Index = nIndex;
            JpegCompressor = jpegcompressor;
            Source.OnNewFrame += source_OnNewFrame;
        }


        public IVideoSource Source = null;
        public IVideoCompressor JpegCompressor = null;
        private int m_nIndex = -1;

        private byte[] m_bLastImage = null;
        public byte[] LastImage
        {
            get { return m_bLastImage; }
            set { m_bLastImage = value; }
        }

        public int Index
        {
            get { return m_nIndex; }
            set { m_nIndex = value; }
        }


        public ObservableCollection<HTTPServerConnection> MJPGConnections = new ObservableCollection<HTTPServerConnection>();
        object m_LockMJPEGConnections = new object();

        public void AddConnection(HTTPServerConnection connection)
        {
            lock (m_LockMJPEGConnections)
            {
                if (MJPGConnections.Contains(connection) == false)
                    MJPGConnections.Add(connection);
            }
        }

        public void RemoveConnection(HTTPServerConnection connection)
        {
            lock (m_LockMJPEGConnections)
            {
                if (MJPGConnections.Contains(connection) == true)
                {
                    MJPGConnections.Remove(connection);
                }
            }
        }
        public void RemoveAllConnections()
        {
            lock (m_LockMJPEGConnections)
            {
                MJPGConnections.Clear();
            }
        }

        void source_OnNewFrame(byte[] bRawData, VideoCaptureRate format, object objSource)
        {
            /// Queue frame to all clients
            //}
            if (JpegCompressor == null)
                return;



            byte [] bJpeg = JpegCompressor.CompressFrame(bRawData);
            LastImage = bJpeg;
            bRawData = null;

            if (MJPGConnections.Count <= 0)
                return;


            HTTPServerConnection[] ActiveConnections = null;
            lock (m_LockMJPEGConnections)
            {
                ActiveConnections = this.MJPGConnections.ToArray();
            }

            if (bJpeg != null)
            {
                foreach (HTTPServerConnection con in ActiveConnections)
                {
                    con.FrameQueue.Enqueue(bJpeg);
                }
                bJpeg = null;
            }


        }

    }

    public enum AuthenticationMethod
    {
        Windows,
        Internal,
        None
    }

    /// <summary>
    /// Modeled after axis
    /// Video request:
    /// GET /mjpg/video.cgi?camera=1&showlength=1 HTTP/1.1 
    /// Audio Request:
    /// GET /axis-cgi/audio/receive.cgi?camera=1&httptype=singlepart HTTP/1.1 
    /// 
    /// We use basic authentication, so you must add a windows users to get access
    /// </summary>
    public class MotionJpegHttpServer 
    {
        /// <summary>
        ///  Have to pass the jpeg compressor in since it's different on different OS's.  WPFImageWindows.InterFrameCompressor is an example of WPF jpeg compression/decompression
        /// </summary>
        /// <param name="JpegCompressor"></param>
        public MotionJpegHttpServer()
        {
        }

        object objOurCLientLock = new object();
        List<VideoSourceWithSubscribers> VideoSources = new List<VideoSourceWithSubscribers>();

        public int AddVideoSource(IVideoSource source, IVideoCompressor compressor)
        {
            lock (objOurCLientLock)
            {
                VideoSourceWithSubscribers ss = new VideoSourceWithSubscribers(source, compressor, VideoSources.Count);
                VideoSources.Add(ss);

                return ss.Index;
            }
        }

        public VideoSourceWithSubscribers GetVideoSource(int nOneBasedIndex)
        {
            int nIndex = nOneBasedIndex - 1;
            if (nIndex < 0)
                return null;

            lock (objOurCLientLock)
            {
                if (VideoSources.Count < nOneBasedIndex)
                    return null;
                return VideoSources[nIndex];
            }
        }

        private int m_nPort = 8090;

        public int Port
        {
            get { return m_nPort; }
            set { m_nPort = value; }
        }

        System.Net.HttpListener Listener = new System.Net.HttpListener();

        private string m_strPrefix = "http://*:8090/";

        public string Prefix
        {
            get { return m_strPrefix; }
        }


        private AuthenticationMethod m_eAuthenticationMethod = AuthenticationMethod.Windows;
        public AuthenticationMethod AuthenticationMethod
        {
            get { return m_eAuthenticationMethod; }
            set { m_eAuthenticationMethod = value; }
        }

        private string m_strUserName = "User";
        /// <summary>
        ///  User Name if the AuthenticationMethod is set to internal
        /// </summary>
        public string UserName
        {
            get { return m_strUserName; }
            set { m_strUserName = value; }
        }

        private string m_strPassword = ";lkjsdf;lakjfew,mn";
        /// <summary>
        /// Password if the AuthenticationMethod is set to internal
        /// </summary>
        public string Password
        {
            get { return m_strPassword; }
            set { m_strPassword = value; }
        }

        private int m_nMaxConnections = 10;

        public int MaxConnections
        {
            get { return m_nMaxConnections; }
            set { m_nMaxConnections = value; }
        }

        Thread m_threadHttpListener;
        bool m_bExit = false;
        int nFrameNumber = 0;

        Thread m_threadAudioServer;

        private bool m_bAllowAudio = false;

        public bool AllowAudio
        {
            get { return m_bAllowAudio; }
            set { m_bAllowAudio = value; }
        }

        public ObservableCollection<HTTPServerConnection> MJPGConnections = new ObservableCollection<HTTPServerConnection>();
        object m_LockMJPEGConnections = new object();

        //public ObservableCollectionEx<DotNetHTTPConnection> AudioConnections = new ObservableCollectionEx<DotNetHTTPConnection>();
        //object m_LockAudioConnections = new object();

        //[DllImport("user32.dll", SetLastError = false)]
        //static extern IntPtr GetDesktopWindow();

        public void Start()
        {
            Stop();

            m_bExit = false;
            Listener.Prefixes.Clear();
            m_strPrefix = string.Format("http://*:{0}/", m_nPort);
            Listener.Prefixes.Add(m_strPrefix);


            m_threadHttpListener = new Thread(HttpListenThread);
            m_threadHttpListener.IsBackground = true;
            m_threadHttpListener.Name = "MJPEG Http Listener thread";
            m_threadHttpListener.Start();

            //if (m_bAllowAudio == true)
            //{
            //    m_threadAudioServer = new Thread(AudioServerThread);
            //    m_threadAudioServer.IsBackground = true;
            //    m_threadAudioServer.Name = "Audio Serving Listener thread";
            //    m_threadAudioServer.Start();
            //}

        }

        //void AudioServerThread()
        //{
        //    DirectShowFilters.MicrophoneFilter Microphone = null;
        //    Microphone = new DirectShowFilters.MicrophoneFilter(Guid.Empty, 30, AudioLibrary.AudioFormat.SixteenByEightThousandMono, GetDesktopWindow());
            
        //    /// Trick the mic into thinking it's connected
        //    /// 
        //    //MediaFilter filter = new AudioFilter("Dummy filter", FilterDirection.Both);
        //    AudioPin pin = new AudioPin(null, AudioFormat.SixteenByEightThousandMono, PinDirection.Output);
        //    AudioPin micout = Microphone.FindOrCreateCompatibleOutputPin(AudioFormat.SixteenByEightThousandMono);
        //    micout.Connect(pin);

        //   // Filters.SoundGeneratorFilter soundgen = new Filters.SoundGeneratorFilter(AudioFormat.SixteenByEightThousandMono);

        //   // soundgen.Volume = 30;
        //    //soundgen.StartGeneratingContinuousTone(new MultiToneFrequency(new LowestLevelLibrary.Frequency(350, "350"), new LowestLevelLibrary.Frequency(440, "440"), "dialtone"));

        //    Microphone.Start();

        //    TimeSpan tsAxis = new TimeSpan(0, 0, 0, 0, 30);
        //    while (m_bExit == false)
        //    {

        //        //MediaSample sample = soundgen.PullSample(tsAxis, null); 
        //        MediaSample sample = Microphone.PullSample(tsAxis, null);

        //        DotNetHTTPConnection[] ActiveConnections = null;
        //        lock (m_LockAudioConnections)
        //        {
        //            ActiveConnections = this.AudioConnections.ToArray();
        //        }

        //        if (sample != null)
        //        {
        //            byte [] bG711 = WaveFileLibrary.WaveWriter.ConvertShortPCMToULAW(sample.GetShortData());
        //            List<DotNetHTTPConnection> RemoveList = new List<HTTPConnection>();
        //            foreach (DotNetHTTPConnection con in ActiveConnections)
        //            {
        //                try
        //                {
        //                    con.HttpListenerContext.Response.OutputStream.Write(bG711, 0, bG711.Length);
        //                }
        //                catch (Exception ex)
        //                {
        //                    RemoveList.Add(con);
        //                }
        //            }

        //            if (RemoveList.Count > 0)
        //            {
        //                lock (m_LockAudioConnections)
        //                {
        //                    foreach (DotNetHTTPConnection con in RemoveList)
        //                    {
        //                        if (AudioConnections.Contains(con) == true)
        //                            AudioConnections.Remove(con);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    //soundgen.StopGeneratingContinuousTone();
        //    //soundgen.Close();
        //    Microphone.Stop();
        //    Microphone.Close();
        //    Microphone = null;
        //}


        internal void RemoveVideoClient(HTTPServerConnection con)
        {
            lock (m_LockMJPEGConnections)
            {
                if (MJPGConnections.Contains(con) == true)
                    MJPGConnections.Remove(con);
            }
        }



        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;
        const int SecurityImpersonation = 2;
        public const String policyStr = @"<?xml version=""1.0"" encoding=""utf-8"" ?> 
                                        <access-policy> 
                                            <cross-domain-access> 
                                                <policy> 
                                                    <allow-from> 
                                                        <domain uri=""*"" /> 
                                                    </allow-from> 
                                                    <grant-to> 
                                                        <socket-resource port=""*"" protocol=""tcp"" /> 
                                                    </grant-to> 
                                                </policy> 
                                            </cross-domain-access> 
                                        </access-policy>";
        private byte[] policy = Encoding.ASCII.GetBytes(policyStr);
        private static string policyRequestString = "<policy-file-request/>";

        string m_strBoundary = "";
        void HttpListenThread()
        {
            m_strBoundary = Guid.NewGuid().ToString();

            if (AuthenticationMethod == AuthenticationMethod.Windows)
                Listener.AuthenticationSchemes = AuthenticationSchemes.Basic; // AuthenticationSchemes.Digest | AuthenticationSchemes.IntegratedWindowsAuthentication;
            else
                Listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            //Listener.ExtendedProtectionSelectorDelegate = 
            Listener.UnsafeConnectionNtlmAuthentication = true;
            Listener.Start();
            //m_objActiveCameraNumber.NewFrame += new DelegateBitmapFrame(m_objActiveCameraNumber_NewFrame);

            try
            {
                while (m_bExit == false)
                {
                    HttpListenerContext cont = Listener.GetContext();

                    if (cont.Request.RawUrl.IndexOf("access") >= 0)
                    {
                        cont.Response.OutputStream.Write(policy, 0, policy.Length);
                        cont.Response.StatusCode = 200;
                        cont.Response.StatusDescription = "OK";
                        cont.Response.Close();
                    }

                    //System.Security.Principal.WindowsIdentity id = new System.Security.Principal.WindowsIdentity(cont.User.Identity.Name);
                    System.Net.HttpListenerBasicIdentity iden = null;
                    if (cont.User != null)
                       iden = cont.User.Identity as System.Net.HttpListenerBasicIdentity;
                    
                    IntPtr token = IntPtr.Zero;
                    bool bAuthenticated = false;
                    if (AuthenticationMethod == AuthenticationMethod.Windows)
                        bAuthenticated = LogonUser(iden.Name, "", iden.Password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT, ref token);
                    else if (AuthenticationMethod == AuthenticationMethod.Internal)
                        bAuthenticated = ((string.Compare(iden.Name, this.UserName, false) == 0) && (string.Compare(iden.Password, this.Password, false) == 0));
                    else
                        bAuthenticated = true;
                    
                    if (bAuthenticated == false)
                    {
                        cont.Response.StatusCode = 401;
                        cont.Response.StatusDescription = "You're not authorized!";
                        cont.Response.Close();
                        continue;
                    }
                    
                    if (cont.Request.ProtocolVersion.Minor != 1)
                    {
                        cont.Response.StatusCode = 581;
                        cont.Response.StatusDescription = "Unsupported protocol version";
                        cont.Response.Close();
                        continue;
                    }


                    if (MJPGConnections.Count >= MaxConnections)
                    {
                        cont.Response.StatusCode = 582;
                        cont.Response.StatusDescription = "Maximum number of connections has been exceeded";
                        cont.Response.Close();

                        continue;
                    }
                    else
                    {
                        cont.Response.SendChunked = true;
                        cont.Response.KeepAlive = true;

                        if (cont.Request.RawUrl.IndexOf("video") >= 0)
                        {
                            /// Look for the specified camera index
                            /// 
                            int nCamera = 1;
                            if (cont.Request.QueryString["camera"] != null)
                                nCamera = Convert.ToInt32(cont.Request.QueryString["camera"]);

                            double nMaxFPS = 1;
                            if (cont.Request.QueryString["fps"] != null)
                                nMaxFPS = Convert.ToDouble(cont.Request.QueryString["fps"]);

                            /// Find the specified camera - 1 based, not 0 based
                            /// 
                            VideoSourceWithSubscribers source = GetVideoSource(nCamera);
                            if (source == null)
                            {
                                cont.Response.StatusCode = 587;
                                cont.Response.StatusDescription = "Camera not found";
                                cont.Response.Close();
                                continue;
                            }

                            MotionJpegServerClient client = null;
                            lock (m_LockMJPEGConnections)
                            {
                                client = new MotionJpegServerClient(this, source, cont, ConnectionType.MotionJpeg);
                                client.MaxFramesPerSecond = nMaxFPS;
                                cont.Response.ContentType = string.Format("multipart/x-mixed-replace;boundary={0}", client.Boundary);
                        
                                source.AddConnection(client);
                                MJPGConnections.Add(client);
                                client.Start();
                            }

                        }
                        else if (cont.Request.RawUrl.IndexOf("jpeg") >= 0) /// take a snapshot image
                        {
                            /// Look for the specified camera index
                            /// 
                            int nCamera = 1;
                            if (cont.Request.QueryString["camera"] != null)
                                nCamera = Convert.ToInt32(cont.Request.QueryString["camera"]);

                            /// Find the specified camera - 1 based, not 0 based
                            /// 
                            VideoSourceWithSubscribers source = GetVideoSource(nCamera);
                            if (source == null)
                            {
                                cont.Response.StatusCode = 587;
                                cont.Response.StatusDescription = "Camera not found";
                                cont.Response.Close();
                                continue;
                            }
                            cont.Response.ContentType = string.Format("image/jpeg");
                            byte[] bLastImage = source.LastImage;
                            if (bLastImage == null)
                            {
                                System.Threading.Thread.Sleep(1000);
                                bLastImage = source.LastImage;
                            }
                            if (bLastImage == null)
                            {
                                cont.Response.StatusCode = 588;
                                cont.Response.StatusDescription = "Image not found";
                                cont.Response.Close();
                                continue;
                            }

                            cont.Response.ContentLength64 = bLastImage.Length;

                            cont.Response.StatusCode = 200;
                            cont.Response.StatusDescription = "OK";
                            cont.Response.OutputStream.Write(bLastImage, 0, bLastImage.Length);
                            cont.Response.Close();

                        }
                        //else if ((cont.Request.RawUrl.IndexOf("audio") >= 0) && (AllowAudio == true) )
                        //{
                        //    cont.Response.ContentType = string.Format("audio/basic");

                        //    lock (m_LockAudioConnections)
                        //    {
                        //        AudioConnections.Add(new DotNetHTTPConnection(cont, ConnectionType.G711Audio));
                        //    }
                        //}
                        else if (cont.Request.RawUrl.IndexOf("ptz.cgi") >= 0) /// pan tilt zoom - described in axis VAPIX HTTP API document
                        {
                            int nCamera = 1;
                            if (cont.Request.QueryString["camera"] != null)  
                                nCamera = Convert.ToInt32(cont.Request.QueryString["camera"]);



                            /// Find the specified camera - 1 based, not 0 based
                            /// 
                            VideoSourceWithSubscribers source = GetVideoSource(nCamera);
                            if (source == null)
                            {
                                cont.Response.StatusCode = 587;
                                cont.Response.StatusDescription = "Camera not found";
                                cont.Response.Close();
                                continue;
                            }

                            ICameraController Controller = null;
                            if (source.Source is ICameraController)
                                Controller = source.Source as ICameraController;
                            else
                            {
                                cont.Response.StatusCode = 200;
                                cont.Response.StatusDescription = "OK";
                                cont.Response.Close();
                                continue;
                            }


                            if (cont.Request.QueryString["pan"] != null)  //pan=-180 to 180
                            {
                              //  double fPan = Convert.ToDouble(cont.Request.QueryString["pan"]);
                               // Controller.PanLeft((int)fPan);
                            }
                            if (cont.Request.QueryString["move"] != null)  //pan=-180 to 180
                            {
                                string strMove = cont.Request.QueryString["move"];
                                if (strMove == "right")
                                    Controller.PanRight();
                                else if (strMove == "left")
                                    Controller.PanLeft();
                                if (strMove == "up")
                                    Controller.TiltUp();
                                if (strMove == "down")
                                    Controller.TiltDown();
                            }
                            if (cont.Request.QueryString["tilt"] != null)  //tilt=-180 to 180
                            {
                              //  double fTilt = Convert.ToDouble(cont.Request.QueryString["tilt"]);
                              //  Controller.TiltRelative((int)fTilt);
                            }
                            if (cont.Request.QueryString["zoom"] != null)  //zoom 1.... 9999
                            {
                                //double fZoom = Convert.ToDouble(cont.Request.QueryString["zoom"]);
                                //if (fZoom == 1500) fZoom = 2;
                                //if (fZoom == -1500) fZoom = 1;
                                //Controller.Zoom((int)fZoom);
                            }
                            if (cont.Request.QueryString["focus"] != null)  //zoom 1.... 9999
                            {
                                string strFocus = cont.Request.QueryString["focus"];

                                //double fZoom = Convert.ToDouble(cont.Request.QueryString["zoom"]);
                                //if (fZoom == 1500) fZoom = 2;
                                //if (fZoom == -1500) fZoom = 1;
                                Controller.SetFocus(Convert.ToInt32(strFocus));
                            }
                            if (cont.Request.QueryString["record"] != null)  //zoom 1.... 9999
                            {
                                string strRecord  = cont.Request.QueryString["record"];

                                
                                //double fZoom = Convert.ToDouble(cont.Request.QueryString["zoom"]);
                                //if (fZoom == 1500) fZoom = 2;
                                //if (fZoom == -1500) fZoom = 1;
                                
                            }
                            cont.Response.StatusCode = 200;
                            cont.Response.StatusDescription = "OK";
                            cont.Response.Close();

                        }
                        else if (cont.Request.RawUrl.IndexOf("action.cgi") >= 0) /// pan tilt zoom - described in axis VAPIX HTTP API document
                        {
                            int nCamera = 1;
                            if (cont.Request.QueryString["camera"] != null)
                                nCamera = Convert.ToInt32(cont.Request.QueryString["camera"]);



                            /// Find the specified camera - 1 based, not 0 based
                            /// 
                            VideoSourceWithSubscribers source = GetVideoSource(nCamera);
                            if (source == null)
                            {
                                cont.Response.StatusCode = 587;
                                cont.Response.StatusDescription = "Camera not found";
                                cont.Response.Close();
                                continue;
                            }

                            IVideoRecorder Controller = null;
                            if (source.Source is IVideoRecorder)
                                Controller = source.Source as IVideoRecorder;
                            else
                            {
                                cont.Response.StatusCode = 200;
                                cont.Response.StatusDescription = "OK";
                                cont.Response.Close();
                                continue;
                            }


                            if (cont.Request.QueryString["record"] != null)  //zoom 1.... 9999
                            {
                                bool bRecord = Convert.ToBoolean(cont.Request.QueryString["record"]);

                                if (bRecord == true)
                                    Controller.StartRecording();
                                else
                                    Controller.StopRecording();

                                

                            }
                            cont.Response.StatusCode = 200;
                            cont.Response.StatusDescription = "OK";
                            cont.Response.Close();

                        }
                        else
                        {
                            cont.Response.StatusCode = 404;
                            cont.Response.StatusDescription = "Not Found";
                            cont.Response.Close();

                        }
                    }
                }

            }
            catch (Exception ex)
            {
            }


            Listener.Stop();

            CloseAllVideoConnections();
            //CloseAllAudioConnections();
        }

        public void CloseAllVideoConnections()
        {

            HTTPServerConnection[] connections = null;
            lock (m_LockMJPEGConnections)
            {
                connections = MJPGConnections.ToArray();
            }

            foreach (HTTPServerConnection con in connections)
            {
                con.Stop();
               
            }

            lock (objOurCLientLock)
            {

                foreach (VideoSourceWithSubscribers ss in VideoSources)
                {
                    ss.RemoveAllConnections();
                }
            }
            /// Con't lock on this since we dispatch it
            MJPGConnections.Clear();
        }

        //public void CloseAllAudioConnections()
        //{

        //    DotNetHTTPConnection[] connections = null;
        //    lock (m_LockAudioConnections)
        //    {
        //        connections = this.AudioConnections.ToArray();
        //    }

        //    foreach (DotNetHTTPConnection con in connections)
        //    {
        //        con.Stop();
        //    }

        //    /// Con't lock on this since we dispatch it
        //    AudioConnections.Clear();
        //}

        public void CloseConnection(HTTPServerConnection connection)
        {
            lock (m_LockMJPEGConnections)
            {
                connection.Stop();

                if (MJPGConnections.Contains(connection) == true)
                {
                    MJPGConnections.Remove(connection);
                }
            }

            /// Remove from the appropriate candle
            /// 
            lock (objOurCLientLock)
            {

                foreach (VideoSourceWithSubscribers ss in VideoSources)
                {
                    ss.RemoveConnection(connection);
                }
            }

            //lock (m_LockAudioConnections)
            //{
            //    connection.Stop();

            //    if (AudioConnections.Contains(connection) == true)
            //    {
            //        AudioConnections.Remove(connection);
            //    }
            //}
        }


        public void Stop()
        {
            m_bExit = true;
            Listener.Stop();
            if (m_threadHttpListener != null)
            {
                //m_threadHttpListener.Join();
                m_threadHttpListener = null;
            }
        }



    }

  
}
