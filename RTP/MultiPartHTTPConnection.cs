using System;
using System.Net;
using System.Collections.Generic;

using System.Net.Sockets;
using System.Text.RegularExpressions;
using xmedianet.socketserver;

namespace RTP
{

    public enum HTTPState
    {
        None,
        SentFirstRequest,
        GotFirstChallenge,
        SentChallengeRequest,
        ReadingContent,
    }

    /// <summary>
    ///  Just a hack to make our camera work with crippled silverlight (supports basic authentication)
    /// </summary>
    public class MultiPartHTTPConnection
    {

        public MultiPartHTTPConnection()
        {
        }

        private Uri m_objUri = null;

        public Uri Uri
        {
            get { return m_objUri; }
            set { m_objUri = value; }
        }

        public const string GETNoAuth =
@"GET /$DIRECTORY HTTP/1.1
Host: $HOST:$PORT
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:2.0.1) Gecko/20100101 Firefox/4.0.1
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
Accept-Language: en-us,en;q=0.5
Accept-Encoding: gzip, deflate
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7
Keep-Alive: 115
Connection: keep-alive

";

        //HTTP/1.1 401 Unauthorized
        //Content-Length: 0
        //Server: Microsoft-HTTPAPI/2.0
        //WWW-Authenticate: Basic realm=""
        //Date: Sun, 11 Sep 2011 15:22:05 GMT

        public const string GETAuth =
@"GET /$DIRECTORY HTTP/1.1
Host: $HOST:$PORT
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:2.0.1) Gecko/20100101 Firefox/4.0.1
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
Accept-Language: en-us,en;q=0.5
Accept-Encoding: gzip, deflate
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7
Keep-Alive: 115
Connection: keep-alive
Authorization: Basic $AUTHORIZATION

";

        //HTTP/1.1 200 OK
        //Transfer-Encoding: chunked
        //Content-Type: multipart/x-mixed-replace;boundary=6d4a4d7c-edd3-437a-a96f-5c4f714fead6
        //Server: Microsoft-HTTPAPI/2.0
        //Date: Sun, 11 Sep 2011 15:22:10 GMT

        //12ccd
        //6d4a4d7c-edd3-437a-a96f-5c4f714fead6
        //Content-type: image/jpeg
        //Content-Length: 76916
        //
        //......JFIF.....`.`.....C..............
        //..
        //................. $.' ",#..(7),01444.'9=82<.342...C........
        //.2!.

        // ...

        //6d4a4d7c-edd3-437a-a96f-5c4f714fead6
        //Content-type: image/jpeg
        //Content-Length: 76923

        //......JFIF.....`.`.....C..............
        //..


        protected HTTPState HTTPState = HTTPState.None;

        private string m_strUserName = "";

        public string UserName
        {
            get { return m_strUserName; }
            set { m_strUserName = value; }
        }
        private string m_strPassword = "";

        public string Password
        {
            get { return m_strPassword; }
            set { m_strPassword = value; }
        }

        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public void Start()
        {
            socket.NoDelay = true;
            socket.SendBufferSize = 1024 * 16;
            socket.ReceiveBufferSize = 1024 * 16;
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            DnsEndPoint hostEntry = new DnsEndPoint(Uri.Host, Uri.Port);
            args.RemoteEndPoint = hostEntry;
            args.UserToken = socket;
//#if !WINDOWS_PHONE
//            args.SocketClientAccessPolicyProtocol = SocketClientAccessPolicyProtocol.Tcp;
//#endif
            args.Completed += new EventHandler<SocketAsyncEventArgs>(SocketEvent_Completed);
            socket.ConnectAsync(args);
        }

        private bool m_bIsConnected = false;

        public bool IsConnected
        {
            get { return m_bIsConnected; }
            protected set { m_bIsConnected = value; }
        }

        public void Stop()
        {

            if (IsConnected == true)
            {
                IsConnected = false;
                socket.Shutdown(SocketShutdown.Send);
                socket.Close();
            }
        }

        void SocketEvent_Completed(object obj, SocketAsyncEventArgs args)
        {
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    {
                        ProcessConnect(args);
                    }
                    break;
                case SocketAsyncOperation.Receive:
                    {
                        ProcessReceive(args);
                    }
                    break;
                case SocketAsyncOperation.Send:
                    {
                        ProcessSend(args);
                    }
                    break;
                default:
                    throw new Exception("Invalid operation completed");
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            /// Connected, send our first HTTP request..  We should get a challenge
            if (e.SocketError == SocketError.Success)
            {
                IsConnected = true;
                string strReq1 = GETNoAuth.Replace("$HOST", Uri.Host);
                strReq1 = strReq1.Replace("$PORT", Uri.Port.ToString());
                strReq1 = strReq1.Replace("$DIRECTORY", Uri.LocalPath);

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strReq1);
                e.SetBuffer(buffer, 0, buffer.Length);
                Socket sock = e.UserToken as Socket;
                bool bWillRaiseEvent = sock.SendAsync(e);
                if (!bWillRaiseEvent)
                {
                    ProcessSend(e);
                }
            }
            else
            {
                IsConnected = false;
                throw new SocketException((int)e.SocketError);
            }
        }


        ByteBuffer ReceiveBuffer = new ByteBuffer();
        int nTotal = 0;
        MultipartStreamParser parser = new MultipartStreamParser();

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (HTTPState == HTTPState.ReadingContent)
                {
                    /// Check existing data
                    byte[] bReceiveRemaining = ReceiveBuffer.GetAllSamples();
                    if ((bReceiveRemaining != null) && (bReceiveRemaining.Length > 0))
                    {
                        parser.AddData(bReceiveRemaining, 0, bReceiveRemaining.Length);
                    }

                    /// Now add data we just got and let the mutlipart parser handle it
                    List<MultipartPart> parts2 = parser.AddDataWithCheck(e.Buffer, e.Offset, e.BytesTransferred);
                    foreach (MultipartPart part in parts2)
                    {
                        if ((part.Data != null) && (part.Data.Length > 0) && (OnNewHTTPFragment != null))
                            OnNewHTTPFragment(part.Data);
                    }

                    DoReceive(e);

                }
                else
                {
                    ReceiveBuffer.AppendData(e.Buffer, e.Offset, e.BytesTransferred);
                    nTotal += e.BytesTransferred;

                    byte[] bNextHttpResponse = ReceiveBuffer.FindString("\r\n\r\n");
                    if (bNextHttpResponse == null)
                    {
                        DoReceive(e);
                    }
                    else
                    {
                        nTotal -= bNextHttpResponse.Length;
                        string strHttpResponse = System.Text.UTF8Encoding.UTF8.GetString(bNextHttpResponse, 0, bNextHttpResponse.Length);
                        HandleHTTPResponse(e, strHttpResponse);
                    }
                }

            }
            else
            {
                //throw new SocketException((int)e.SocketError);
                OnNewHTTPFragment(null);
                Stop();
            }
        }

        void DoReceive(SocketAsyncEventArgs e)
        {

            Socket sock = e.UserToken as Socket;
            if (sock.Connected == false)
                return;

            /// Keep receiving
            bool willRaiseEvent = sock.ReceiveAsync(e);
            if (!willRaiseEvent)
            {
                ProcessReceive(e);
            }
        }

        public delegate void DelegateNewHTTPFragment(byte[] bData);
        public event DelegateNewHTTPFragment OnNewHTTPFragment = null;


        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (HTTPState == HTTPState.None)
                    HTTPState = HTTPState.SentFirstRequest;
                else if (HTTPState == HTTPState.GotFirstChallenge)
                    HTTPState = HTTPState.SentChallengeRequest;

                //Read data sent from the server
                DoReceive(e);
            }
            else
            {
                throw new SocketException((int)e.SocketError);
            }
        }

        void HandleHTTPResponse(SocketAsyncEventArgs e, string strResponse)
        {
            Match matchman = Regex.Match(strResponse, @"HTTP/1.1 \s+ (\d+) \s+ (\w+)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            if (matchman.Success == false)
            {
                throw new Exception(string.Format("Unable to parse HTTP Response: {0}", strResponse));
            }

            int nCode = Convert.ToInt32(matchman.Groups[1].Value);
            if (nCode == 401)
            {
                // Got a challenge, send the next Get with username and password
                if (HTTPState != HTTPState.SentFirstRequest)
                {
                    throw new Exception("Password incorrect or something");
                }

                string strReq1 = GETAuth.Replace("$HOST", Uri.Host);
                strReq1 = strReq1.Replace("$PORT", Uri.Port.ToString());
                strReq1 = strReq1.Replace("$DIRECTORY", Uri.LocalPath);
                strReq1 = strReq1.Replace("$AUTHORIZATION", Base64Text(string.Format("{0}:{1}", this.UserName, this.Password)));

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strReq1);
                e.SetBuffer(buffer, 0, buffer.Length);
                Socket sock = e.UserToken as Socket;
                HTTPState = HTTPState.GotFirstChallenge;
                bool willRaiseEvent = sock.SendAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessSend(e);
                }
            }
            else if (nCode == 200) ///TODO... make this more robust to handle standard http as well by searching for content-length
            {
                HTTPState = HTTPState.ReadingContent;
                /// We should be mutli-part, so parse out the boundary
                /// 
                Match matchcontenttype = Regex.Match(strResponse, @"Content-Type\: \s* (\S+);boundary\=(\S+)", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchcontenttype.Success == false)
                {
                    throw new Exception(string.Format("Unable to parse HTTP Response, could not find content-type header: {0}", strResponse));
                }

                //string strContentType = matchcontenttype.Groups[1].Value;
                parser.Boundary = string.Format("{0}\r\n", matchcontenttype.Groups[2].Value);

                DoReceive(e);
            }

        }

        public static string Base64Text(string strText)
        {
            byte[] bString = System.Text.Encoding.UTF8.GetBytes(strText);
            return Convert.ToBase64String(bString);
        }

    }
}
