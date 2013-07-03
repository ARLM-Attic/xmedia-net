
using System;
using System.Net;

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using System.Threading;
using AudioClasses;

namespace RTP
{
    public class MultipartPart
    {
        public MultipartPart()
        {
        }

        public MultipartPart(string strHeaders, byte[] bData)
        {
            Headers = strHeaders;
            Data = bData;
        }

        private string m_strHeaders = "";

        public string Headers
        {
            get { return m_strHeaders; }
            set { m_strHeaders = value; }
        }


        private byte[] m_bData = new byte[] { };

        public byte[] Data
        {
            get { return m_bData; }
            set { m_bData = value; }
        }
    }

    public class MultipartStreamParser
    {

        public MultipartStreamParser()
        {
        }

        public MultipartStreamParser(string strBoundary)
        {
            Boundary = strBoundary;
        }


        xmedianet.socketserver.ByteBuffer ReceiveBuffer = new xmedianet.socketserver.ByteBuffer();

        private string m_strHeaders = "";

        private string m_strBoundary = "";
        public string Boundary
        {
            get { return m_strBoundary; }
            set { m_strBoundary = value; }
        }

        int m_nWaitingForContentLength = 0;

        public List<MultipartPart> AddDataWithCheck(byte[] bNewData, int nOffset, int nLength)
        {
            AddData(bNewData, nOffset, nLength);
            return CheckData();
        }

        public void AddData(byte[] bNewData, int nOffset, int nLength)
        {
            ReceiveBuffer.AppendData(bNewData, nOffset, nLength);
        }

        public List<MultipartPart> CheckData()
        {
            List<MultipartPart> RetArrays = new List<MultipartPart>();
            while (true)
            {
                if (m_nWaitingForContentLength == 0)
                {
                    byte[] bNextHeader = ReceiveBuffer.FindString("\r\n\r\n");
                    if (bNextHeader == null)
                        return RetArrays;

                    m_strHeaders = System.Text.Encoding.UTF8.GetString(bNextHeader, 0, bNextHeader.Length);
                    Match match = Regex.Match(m_strHeaders, @"Content-length:\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                    if (match.Success == true)
                    {
                        m_nWaitingForContentLength = Convert.ToInt32(match.Groups[1].Value);
                    }
                }
                else if (ReceiveBuffer.Size >= m_nWaitingForContentLength)
                {
                    byte[] bData = ReceiveBuffer.GetNSamples(m_nWaitingForContentLength);
                    string strData = System.Text.Encoding.UTF8.GetString(bData, 0, bData.Length);
                    if (strData.Length == 0)
                        System.Diagnostics.Debug.WriteLine("");
                    RetArrays.Add(new MultipartPart(m_strHeaders, bData));
                    m_nWaitingForContentLength = 0;
                }
                else
                {
                    return RetArrays;
                }
            }
        }

    }

    /// <summary>
    ///  An HTTP connection to a mjpeg camera.  We use OurHttpConnection instead of Webclient so it will work on more platforms, but WebClient can be adding the USEWEBCLIENT preprocessor
    /// </summary>
    public class MotionJpegStreamSource 
    {

        public MotionJpegStreamSource(IVideoCompressor jpegcompressor)
            : base()
        {
            JpegCompressor = jpegcompressor;

        }

        public IVideoCompressor JpegCompressor = null;

#if USEWEBCLIENT
        System.Net.WebClient WebClient = null;
#else
        RTP.MultiPartHTTPConnection WebClient = null;
#endif
        MultipartStreamParser parser = new MultipartStreamParser();

        private string m_strUserName = "camera";

        public string UserName
        {
            get { return m_strUserName; }
            set { m_strUserName = value; }
        }
        private string m_strPassword = "camera";

        public string Password
        {
            get { return m_strPassword; }
            set { m_strPassword = value; }
        }

        private string m_strURL = "";

        public string URL
        {
            get { return m_strURL; }
            set { m_strURL = value; }
        }

        public void Start(string strURL)
        {
            /// Try to connect
            /// 
            URL = strURL;

            Uri uri = new Uri(URL);
#if USEWEBCLIENT
            WebThread = new Thread(new ThreadStart(ReadThread));
            WebThread.IsBackground = true;
            WebThread.Name = "MJPEG receive thread";
#else

            WebClient = new RTP.MultiPartHTTPConnection();
            WebClient.UserName = UserName;
            WebClient.Password = Password;
            WebClient.Uri = uri;
            WebClient.OnNewHTTPFragment += new MultiPartHTTPConnection.DelegateNewHTTPFragment(WebClient_OnNewHTTPFragment);
            WebClient.Start();
#endif

            m_bExit = false;
            Running = true;

#if USEWEBCLIENT
            WebThread.Start();
#endif

        }

        public void Stop()
        {
            if (m_bRunning == true)
            {
                m_bExit = true;
                if (WebClient != null)
                {
                    try
                    {
#if USEWEBCLIENT
                        WebClient.CancelAsync();
                        WebClient.Dispose();
#else
                        WebClient.Stop();
#endif
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        WebClient = null;
                    }
                }
            }
        }

#if USEWEBCLIENT
        Thread WebThread = null;
#endif
        bool m_bExit = false;

        xmedianet.socketserver.ByteBuffer ReceiveBuffer = new xmedianet.socketserver.ByteBuffer();

        public event EventHandler OnDisconnected = null;

        private bool m_bRunning = false;

        public bool Running
        {
            get { return m_bRunning; }
            protected set 
            { 
                m_bRunning = value;
                if ( (m_bRunning == false) && (OnDisconnected != null) )
                    OnDisconnected(this, new EventArgs());
            }
        }

        void WebClient_OnNewHTTPFragment(byte[] bJPEG)
        {
            NewJpeg(bJPEG);
        }

#if USEWEBCLIENT
        void ReadThread()
        {
            Uri uri = new Uri(URL);
            
            
            WebClient = new WebClient();
            WebClient.Credentials = new NetworkCredential(UserName, Password);
            try
            {
                Stream stream = WebClient.OpenRead(uri);
                string strContentType = WebClient.ResponseHeaders["Content-Type"];
                Match matchcontenttype = Regex.Match(strContentType, @"boundary\=(\S+)", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchcontenttype .Success == false)
                {
                    Running = false;
                    throw new Exception(string.Format("Unable to parse HTTP Response, could not find content-type header: {0}", strContentType));
                }
                parser.Boundary = string.Format("{0}\r\n", matchcontenttype.Groups[1].Value);

                while (m_bExit == false)
                {

                    int nRead = stream.Read(bBuffer, 0, bBuffer.Length);
                    if (nRead > 0)
                    {
                        List<MultipartPart> parts2 = parser.AddDataWithCheck(bBuffer, 0, nRead);
                        foreach (MultipartPart part in parts2)
                        {
                            if ((part.Data != null) && (part.Data.Length > 0))
                                NewJpeg(part.Data);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
            }
            Running = false;
            NewJpeg(null);
        }
#endif

        int m_nWaitingForContentLength = 0;


        void NewJpeg(byte[] bData)
        {
            if ((bData == null) && (OnNewRawData != null))
            {
                OnNewRawData(null, 0, 0, 4);
                return;
            }

            if (!((bData[0] == 0xff) && (bData[1] == 0xd8)))
            {
                xmedianet.socketserver.ByteBuffer tempbuffer = new xmedianet.socketserver.ByteBuffer();
                tempbuffer.AppendData(bData);
                int nAt = tempbuffer.FindBytes(new byte[] { 0xff, 0xd8 });
                tempbuffer.GetNSamples(nAt);
                bData = tempbuffer.GetAllSamples();
            }
            else
            {
                System.Threading.Thread.Sleep(0);
            }


            int nWidth = 0;
            int nHeight = 0;
            int nBytesPerPixel = 3;
            byte [] bRGBData = JpegCompressor.DecompressFrameWithDimensions(bData, out nWidth, out nHeight, out nBytesPerPixel);
            if (OnNewRawData != null)
            {
                if (nBytesPerPixel == 3)
                {
                    bRGBData = RGB24ToRGB32(bRGBData, nWidth, nHeight);
                    nBytesPerPixel = 4;
                }
                OnNewRawData(bRGBData, nWidth, nHeight, nBytesPerPixel);
            }
        }

        public static byte[] RGB24ToRGB32(byte[] bRGB, int nWidth, int nHeight)
        {
            byte[] bDest = new byte[nWidth * 4 * nHeight];
            unsafe
            {
                int nSpan3 = nWidth * 3;
                int nSpan4 = nWidth * 4;
                fixed (byte* source = &bRGB[0], dest = &bDest[0])
                {
                    for (int h = 0; h < nHeight; h++)
                    {
                        for (int w = 0; w < nWidth; w++)
                        {
                            *(dest + nSpan4 * h + w * 4 + 0) = *(source + h * nSpan3 + w * 3 + 0);
                            *(dest + nSpan4 * h + w * 4 + 1) = *(source + h * nSpan3 + w * 3 + 1);
                            *(dest + nSpan4 * h + w * 4 + 2) = *(source + h * nSpan3 + w * 3 + 2);
                            *(dest + nSpan4 * h + w * 4 + 3) = 0;
                        }
                    }
                }
            }
            return bDest;
        }

        public delegate void DelegateNewData(byte[] bRawData, int nWidth, int nHeight, int nBytesPerPixel);
        public event DelegateNewData OnNewRawData = null;
        

        byte[] bBuffer = new byte[4000000];

        MemoryStream readstream = new MemoryStream();

        int m_nWidth = 640;
        int m_nHeight = 480;
        

      

    }
}