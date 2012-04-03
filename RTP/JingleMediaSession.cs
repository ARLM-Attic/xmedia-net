/// Copyright (c) 2011 Brian Bonnett
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.XMPP;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net.XMPP.Jingle;
using RTP;
using System.ComponentModel;

namespace RTP
{
    public enum SessionState
    {
        NotEstablished,
        Connecting,
        Incoming,
        Established,
        TearingDown,
    }

    [Flags]
    public enum KnownAudioPayload
    {
        G711 = 1,
        Speex16000 = 2,
        Speex8000 = 4,
        G722 = 8,
    }

    /// <summary>
    ///  Responsible for the RTP stream to an from a remote endpoint, as well as session management
    /// </summary>
    public class JingleMediaSession :  INotifyPropertyChanged
    {
        public JingleMediaSession(string strSession, string strRemoteJID, IPEndPoint LocalEp, XMPPClient client)
        {
            AddKnownAudioPayload(KnownAudioPayload.Speex16000 | KnownAudioPayload.Speex8000 | KnownAudioPayload.G711 | KnownAudioPayload.G722);
            Initiator = true;
            LocalEndpoint = LocalEp;
            Session = strSession;
            RemoteJID = strRemoteJID;
            XMPPClient = client;
            SessionState = SessionState.Connecting;
            Bind();
        }

        public JingleMediaSession(string strRemoteJID, IPEndPoint LocalEp, XMPPClient client)
        {
            AddKnownAudioPayload(KnownAudioPayload.Speex16000 | KnownAudioPayload.Speex8000 | KnownAudioPayload.G711 | KnownAudioPayload.G722);
            Initiator = true;
            LocalEndpoint = LocalEp;
            RemoteJID = strRemoteJID;
            XMPPClient = client;
            SessionState = SessionState.Connecting;
            Bind();
        }

        public JingleMediaSession(string strSession, Jingle intialJingle, KnownAudioPayload LocalPayloads, IPEndPoint LocalEp, XMPPClient client)
        {
            AddKnownAudioPayload(LocalPayloads);

            Initiator = false;
            LocalEndpoint = LocalEp;
            Session = strSession;
            InitialJingle = intialJingle;
            XMPPClient = client;
            RemoteJID = InitialJingle.Initiator;
            SessionState = SessionState.Incoming;
            Bind();

            ParsePayloads(InitialJingle);
            /// If we don't have any candidates in our sesion-initate message, we must wait for a transport info to get the candidates
            ParseCandidates(InitialJingle);
        }

        public readonly bool Initiator = true;

        public void SessionAccepted(Jingle jingle)
        {
            ParsePayloads(jingle);
            ParseCandidates(jingle);
        }

        public void StartMedia(AudioConferenceMixer AudioMixer)
        {
            SessionState = SessionState.Established;
            AudioMixer.AddInputOutputSource(AudioRTPStream, AudioRTPStream);
            AudioRTPStream.Start(RemoteEndpoint, 20);
        }

        public void StopMedia(AudioConferenceMixer AudioMixer)
        {
            SessionState = SessionState.TearingDown;
            AudioMixer.RemoveInputOutputSource(AudioRTPStream, AudioRTPStream);
            AudioRTPStream.StopSending();
        }


        private RTPAudioStream m_objAudioRTPStream = new RTPAudioStream(0, null);

        public RTPAudioStream AudioRTPStream
        {
            get { return m_objAudioRTPStream; }
            set { m_objAudioRTPStream = value; }
        }
        List<Candidate> Candidates = new List<Candidate>();

        public Jingle InitialJingle = null;

        private string m_strSession = null;
        public string Session
        {
            get { return m_strSession; }
            set
            {
                if (m_strSession != value)
                {
                    m_strSession = value;
                    FirePropertyChanged("Session");
                }

            }

        }

        private string m_strRemoteJID = "";
        public string RemoteJID
        {
            get { return m_strRemoteJID; }
            set
            {
                if (m_strRemoteJID != value)
                {
                    m_strRemoteJID = value;
                    FirePropertyChanged("RemoteJID");
                }

            }
        }


        private SessionState m_eSessionState = SessionState.NotEstablished;
        public SessionState SessionState
        {
            get { return m_eSessionState; }
            set
            {
                if (m_eSessionState != value)
                {
                    m_eSessionState = value;
                    FirePropertyChanged("SessionState");
                }

            }
        }

        public XMPPClient XMPPClient = null;

        private IPEndPoint m_objLocalEndpoint = null;
        public IPEndPoint LocalEndpoint
        {
            get { return m_objLocalEndpoint; }
            set { m_objLocalEndpoint = value; }
        }

        IPEndPoint m_objRemoteEndpoint = null;
        public IPEndPoint RemoteEndpoint
        {
            get { return m_objRemoteEndpoint; }
            set
            {
                if (m_objRemoteEndpoint != value)
                {
                    m_objRemoteEndpoint = value;
                    FirePropertyChanged("RemoteEndpoint");
                }
            }
        }

        public bool UseStun = true;

        string strPassword = null;
        string strUser = null;

        public DateTime m_dtStartTime = DateTime.Now;
        public TimeSpan CallDuration
        {
            get
            {
                return DateTime.Now - m_dtStartTime;
            }
        }

        void ParsePayloads(Jingle jingle)
        {
            if (this.RemotePayloads.Count > 0)
                return;

            if (jingle != null)
            {
                if ((jingle.Content != null) && (jingle.Content.Description != null) && (jingle.Content.Description.Payloads != null) && (jingle.Content.Description.Payloads.Count > 0))
                {
                    foreach (Payload pay in jingle.Content.Description.Payloads)
                    {
                        this.RemotePayloads.Add(pay);
                    }
                }
            }

            if (this.RemotePayloads.Count >= 0)
            {
                SetCodecsFromPayloads();
            }
            
        }

        void SetCodecsFromPayloads()
        {
            if ((LocalPayloads.Count <= 0) || (RemotePayloads.Count <= 0))
                return;

            bool bFoundAgreeableCodec = false;
            /// Send back the first codec we agree upon
            /// 
            foreach (Payload remotepayload in RemotePayloads)
            {
                foreach (Payload localpayload in LocalPayloads)
                {
                    if ((remotepayload.Name == localpayload.Name) && (remotepayload.ClockRate == localpayload.ClockRate))
                    {
                        bFoundAgreeableCodec = true;
                        AgreedPayload = remotepayload;
                        break;
                    }
                }
                if (bFoundAgreeableCodec == true)
                    break;
            }

            if (bFoundAgreeableCodec == false)
            {
                /// Call must die, no agreeable codecs
                /// 
                throw new Exception("No agreeable codecs found");
            }
        }

        void ParseCandidates(Jingle jingle)
        {
            if (jingle != null)
            {
                if ((jingle.Content != null) && (jingle.Content.ICETransport != null))
                {
                    if (jingle.Content.ICETransport.Candidates.Count > 0)
                    {
                        IPEndPoint epRemoteStun = null;
                        IPEndPoint epRemoteLocal = null;

                        foreach (Candidate nextcand in jingle.Content.ICETransport.Candidates)
                        {
                            if ( (nextcand.type == "stun") && (epRemoteStun == null) )
                                epRemoteStun = new IPEndPoint(IPAddress.Parse(nextcand.ipaddress),
                                    nextcand.port);
                            else if ( (nextcand.type == "local") && (epRemoteLocal == null) )
                                epRemoteLocal = new IPEndPoint(IPAddress.Parse(nextcand.ipaddress),
                                    nextcand.port);
                        }

                        // Now perform ICE tests to each of our candidates, see which one we get a response from.
                        IPEndPoint epresponded = null;
                        IPEndPoint epToUse = null;
                        if (epRemoteLocal != null)
                        {
                            epToUse = epRemoteLocal;
                            epresponded = this.AudioRTPStream.PerformSTUNRequest(epRemoteLocal, 1000);
                            if ((epresponded == null) && (epRemoteStun != null))
                            {
                                epToUse = epRemoteStun;
                                epresponded = this.AudioRTPStream.PerformSTUNRequest(epRemoteStun, 1000);
                            }

                        }

                        if (epToUse != null)
                        {
                            this.RemoteEndpoint = epToUse;
                        }
                        else
                            this.RemoteEndpoint = epRemoteStun;

                        /// Now let's determine which one to connect to. ICE says we must try each one, but that's too complicated
                        /// If we are on a public network, 
                        /// 

                        //Candidates = jingle.Content.ICETransport.Candidates;
                        //RemoteEndpoint = new IPEndPoint(IPAddress.Parse(jingle.Content.ICETransport.Candidates[0].ipaddress),
                        //    jingle.Content.ICETransport.Candidates[0].port);
                    }

                }
            }
        }
        public void GotTransportInfo(Jingle initialjingle)
        {
            //InitialJingle = initialjingle;
            ParseCandidates(initialjingle);
            //if (Candidates.Count > 0)
            //    SendOurTransportInfo(initialjingle); // not here, 
        }

        void Bind()
        {
            AudioRTPStream.Bind(LocalEndpoint);  // For ICE we should have one rtp stream for each candidate, and they should receive stun packets as well as rtp
        }

        public string SendInitiateSession()
        {
            Jingle jingleinfo = BuildOutgoingAudioRequest(true);
            string strSession = XMPPClient.JingleSessionManager.InitiateNewSession(RemoteJID, jingleinfo);
            Session = strSession;
            return Session;
        }

        public void SendAcceptSession()
        {
            if (Candidates.Count < 0)
                return;

            Jingle jingleinfo = BuildOutgoingAudioRequest(false);
            XMPPClient.JingleSessionManager.SendAcceptSession(this.Session, jingleinfo);
        }

        public List<Payload> RemotePayloads = new List<Payload>();

        public List<Payload> LocalPayloads = new List<Payload>();
        public void ClearAllPayloads()
        {
            LocalPayloads.Clear();
        }

        private Payload m_objAgreedPayload = null;

        public Payload AgreedPayload
        {
            get { return m_objAgreedPayload; }
            set 
            { 
                m_objAgreedPayload = value;
                AudioRTPStream.Payload = (byte) m_objAgreedPayload.PayloadId;
                if ((m_objAgreedPayload.Name == "G722") && (m_objAgreedPayload.ClockRate == "16000"))
                    AudioRTPStream.AudioCodec = new G722CodecWrapper();
                else if ((m_objAgreedPayload.Name == "speex") && (m_objAgreedPayload.ClockRate == "16000") )
                   AudioRTPStream.AudioCodec = new SpeexCodec(NSpeex.BandMode.Wide);
                else if ((m_objAgreedPayload.Name == "speex") && (m_objAgreedPayload.ClockRate == "8000") )
                   AudioRTPStream.AudioCodec = new SpeexCodec(NSpeex.BandMode.Narrow);
                else if ((m_objAgreedPayload.Name == "PCMU") && (m_objAgreedPayload.ClockRate == "8000"))
                   AudioRTPStream.AudioCodec = new G711Codec();
            }
        }

        public void AddKnownAudioPayload(KnownAudioPayload payload)
        {
            if ((payload & KnownAudioPayload.G722) == KnownAudioPayload.G722)
                LocalPayloads.Add(new Payload() { PayloadId = 9, Channels = "1", ClockRate = "16000", Name = "G722" });
            if ((payload & KnownAudioPayload.Speex16000) == KnownAudioPayload.Speex16000)
               LocalPayloads.Add(new Payload() { PayloadId = 96, Channels = "1", ClockRate = "16000", Name = "speex" });
            if ((payload & KnownAudioPayload.Speex8000) == KnownAudioPayload.Speex8000)
                LocalPayloads.Add(new Payload() { PayloadId = 97, Channels = "1", ClockRate = "8000", Name = "speex" });
            if ((payload & KnownAudioPayload.G711) == KnownAudioPayload.G711)
                LocalPayloads.Add(new Payload() { PayloadId = 0, Channels = "1", ClockRate = "8000", Name = "PCMU" });
        }


        public static string STUNServer = "stun.ekiga.net";

        protected Jingle BuildOutgoingAudioRequest(bool IsInitiation)
        {
            Jingle jingleinfo = JingleSessionManager.CreateBasicOutgoingAudioRequest(this.AudioRTPStream.LocalEndpoint.Address.ToString(), this.AudioRTPStream.LocalEndpoint.Port);
            jingleinfo.Content.Description.Payloads.Clear();
            if (IsInitiation == true)
                jingleinfo.Content.Description.Payloads.AddRange(LocalPayloads);
            else
            {
                SetCodecsFromPayloads();
                jingleinfo.Content.Description.Payloads.Add(AgreedPayload);
            }


            jingleinfo.Content.ICETransport.Candidates.Clear();

            if (UseStun == true)
            {
                IPEndPoint publicep = this.AudioRTPStream.PerformSTUNRequest(STUNServer, 5000);
                if (publicep != null)
                    jingleinfo.Content.ICETransport.Candidates.Add(new Candidate() { ipaddress = publicep.Address.ToString(), port = publicep.Port, type = "stun" });
            }
            jingleinfo.Content.ICETransport.Candidates.Add(new Candidate() { ipaddress = this.AudioRTPStream.LocalEndpoint.Address.ToString(), port = this.AudioRTPStream.LocalEndpoint.Port, type = "local" });
            return jingleinfo;
        }
    

        static Random rand = new Random();
        public static string GenerateRandomString(int nLength)
        {
            /// 48-57, 65-90, 97-122
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < nLength; i++)
            {
                int c = rand.Next(62);
                if (c < 10)
                    c += 48;
                else if ((c >= 10) && (c < 36))
                    c += (65 - 10);
                else if (c >= 36)
                    c += (97 - 36);

                sb.Append((char)c);
            }

            return sb.ToString();
        }

     

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }

        #endregion
    }
}
