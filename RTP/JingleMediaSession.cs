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
    public class MediaSession :  INotifyPropertyChanged, IComparer<Candidate>
    {
        public MediaSession(string strSession, string strRemoteJID, IPEndPoint LocalEp, XMPPClient client)
        {
            m_objAudioRTPStream.OnUnhandleSTUNMessage += new RTPStream.DelegateSTUNMessage(m_objAudioRTPStream_OnUnhandleSTUNMessage);
            AddKnownAudioPayload(KnownAudioPayload.Speex16000 | KnownAudioPayload.Speex8000 | KnownAudioPayload.G711 | KnownAudioPayload.G722);
            Initiator = true;
            LocalEndpoint = LocalEp;
            Session = strSession;
            RemoteJID = strRemoteJID;
            XMPPClient = client;
            SessionState = SessionState.Connecting;
            Bind();
        }

        public MediaSession(string strRemoteJID, IPEndPoint LocalEp, XMPPClient client)
        {
            m_objAudioRTPStream.OnUnhandleSTUNMessage += new RTPStream.DelegateSTUNMessage(m_objAudioRTPStream_OnUnhandleSTUNMessage);
            AddKnownAudioPayload(KnownAudioPayload.Speex16000 | KnownAudioPayload.Speex8000 | KnownAudioPayload.G711 | KnownAudioPayload.G722);
            Initiator = true;
            LocalEndpoint = LocalEp;
            RemoteJID = strRemoteJID;
            XMPPClient = client;
            SessionState = SessionState.Connecting;
            Bind();
        }

        public MediaSession(string strSession, IQ intialJingle, KnownAudioPayload LocalPayloads, IPEndPoint LocalEp, XMPPClient client)
        {
            m_objAudioRTPStream.OnUnhandleSTUNMessage += new RTPStream.DelegateSTUNMessage(m_objAudioRTPStream_OnUnhandleSTUNMessage);
            AddKnownAudioPayload(LocalPayloads);

            Initiator = false;
            LocalEndpoint = LocalEp;
            Session = strSession;
            XMPPClient = client;
            RemoteJID = intialJingle.From;
            SessionState = SessionState.Incoming;
            Bind();

            ParsePayloads(intialJingle);
            /// If we don't have any candidates in our sesion-initate message, we must wait for a transport info to get the candidates
            ParseCandidates(intialJingle);
        }

        public readonly bool Initiator = true;

     

      

        private RTPAudioStream m_objAudioRTPStream = new RTPAudioStream(0, null);

        public RTPAudioStream AudioRTPStream
        {
            get { return m_objAudioRTPStream; }
            set { m_objAudioRTPStream = value; }
        }

        protected List<Candidate> LocalCandidates = new List<Candidate>();
        protected List<Candidate> RemoteCandidates = new List<Candidate>();

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

        public DateTime m_dtStartTime = DateTime.Now;
        public TimeSpan CallDuration
        {
            get
            {
                return DateTime.Now - m_dtStartTime;
            }
            set
            {
                FirePropertyChanged("CallDuration");
            }
        }


        public virtual string SendInitiateSession()
        {
            return null;
        }

        public void GotNewSessionAck()
        {
            if (Initiator == true)
            {
                XMPPClient.JingleSessionManager.SendTransportInfo(this.Session, BuildOutgoingTransportInfo());
            }
        }
        /// <summary>
        /// Accept the session as soon as we agree on media.  Candidates are then tried
        /// </summary>
        protected virtual void SendAcceptSession()
        {
            if (RemoteCandidates.Count < 0)
                return;
        }
     
        public virtual void GotTransportInfo(IQ iq)
        {
            /// Ignore additional transport candidates.  If they don't send it to us on the first one, we don't want them (probably TURN)
            /// 
            if (RemoteCandidates.Count > 0)
                return;

            ParseCandidates(iq);
            if (RemoteCandidates.Count > 0)
                SendOutBogusSTUNPackets();

             /// If we not the initiator, it's now time to send our candidates
             /// 
            if (Initiator == true)
            {
                PerformICEStunProcedures(); /// Should have all our transport info  now
            }
            else
            {
                XMPPClient.JingleSessionManager.SendTransportInfo(this.Session, BuildOutgoingTransportInfo());
            }
        }

        public void GotSendTransportInfoAck()
        {
            if (Initiator == true)
            {
                /// The terminator got our transport info, now lets wait for theirs
            }
            else
            {
                /// originator got our transport info, they should be starting ICE procedures now
                /// 
                /// Not sure how stun is supposed to work, but unless we send out packets to, the firewall is never opened on the receiver side
                /// 
                
            }

        }

        protected void ICEDoneStartRTP()
        {
            if (Initiator == false)
            {
                SendAcceptSession();
            }
        }

        public void SessionAccepted(IQ iq, AudioConferenceMixer AudioMixer)
        {
            ParsePayloads(iq);
            

            SessionState = SessionState.Established;
            if (AudioMixer != null)
                AudioMixer.AddInputOutputSource(AudioRTPStream, AudioRTPStream);
            AudioRTPStream.Start(RemoteEndpoint, 20);
        }

        public virtual void GotAcceptSessionAck(AudioConferenceMixer AudioMixer)
        {
            SessionState = SessionState.Established;
            if (AudioMixer != null)
                AudioMixer.AddInputOutputSource(AudioRTPStream, AudioRTPStream);
            AudioRTPStream.Start(RemoteEndpoint, 20);
        }



        public void StopMedia(AudioConferenceMixer AudioMixer)
        {
            SessionState = SessionState.TearingDown;
            if (AudioMixer != null)
                AudioMixer.RemoveInputOutputSource(AudioRTPStream, AudioRTPStream);
            AudioRTPStream.StopSending();
        }


        void Bind()
        {
            AudioRTPStream.Bind(LocalEndpoint);  // For ICE we should have one rtp stream for each candidate, and they should receive stun packets as well as rtp
            BuildLocalCandidates();
        }

        private bool m_bUseGoogleTalkProtocol = false;
        public bool UseGoogleTalkProtocol
        {
            get { return m_bUseGoogleTalkProtocol; }
            set { m_bUseGoogleTalkProtocol = value; }
        }


        protected virtual void ParsePayloads(IQ iq)
        {
        }

        protected virtual void ParseCandidates(IQ iq)
        {

        }

        // some protocols want to send candidate separately for some strange reason
        protected virtual Jingle BuildOutgoingTransportInfo()
        {
            Jingle jingleinfo = new Jingle();
            jingleinfo.Content = new Content();
            jingleinfo.Action = Jingle.TransportInfo;
            jingleinfo.Content.Name = "audio";
            jingleinfo.Content.Description = null;
            if (Initiator == true)
                jingleinfo.Content.Creator = "initiator";
            else
                jingleinfo.Content.Creator = "responder";

            if (UseGoogleTalkProtocol == true)
            {
                jingleinfo.Content.ICETransport = null;
                jingleinfo.Content.GoogleTransport = new Transport();
                jingleinfo.Content.GoogleTransport.Candidates.AddRange(LocalCandidates);
            }
            else
            {
                jingleinfo.Content.ICETransport = new Transport();
                jingleinfo.Content.GoogleTransport = null;
                jingleinfo.Content.ICETransport.Candidates.AddRange(LocalCandidates);
            }

            return jingleinfo;
        }


        protected virtual void BuildLocalCandidates()
        {
            LocalCandidates.Clear();
            if (UseStun == true)
            {
                IPEndPoint publicep = this.PerformSTUNRequest(STUNServer, 1000);
                if (publicep != null)
                {

                    Candidate stuncand = new Candidate() { ipaddress = publicep.Address.ToString(), port = publicep.Port, type = "stun" };
                    stuncand.username = GenerateRandomString(16);
                    stuncand.password = GenerateRandomString(16);
                    CalculatePriority(50, 100, stuncand);
                    if (UseGoogleTalkProtocol == true)
                    {
                        stuncand.foundation = null;
                        stuncand.preference = "1.0";
                    }

                    LocalCandidates.Add(stuncand);
                }
            }

            Candidate cand = new Candidate() { ipaddress = this.AudioRTPStream.LocalEndpoint.Address.ToString(), port = this.AudioRTPStream.LocalEndpoint.Port, type = "local" };
            cand.username = GenerateRandomString(16);
            cand.password = GenerateRandomString(16);
            CalculatePriority(70, 500, cand);
            if (UseGoogleTalkProtocol == true)
            {
                cand.foundation = null;
                cand.preference = "1.0";
            }
            LocalCandidates.Add(cand);
        }

        protected void SetCodecsFromPayloads()
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

        protected void SendOutBogusSTUNPackets()
        {
            foreach (Candidate nextcand in this.RemoteCandidates)
            {
                if (nextcand.name == "rtcp")
                    continue;

                if ((nextcand.type == "stun") || (nextcand.type == "local"))
                {

                    STUNMessage msgRequest = new STUNMessage();
                    msgRequest.Method = StunMethod.Binding;
                    msgRequest.Class = StunClass.Inidication;

                    this.AudioRTPStream.SendSTUNMessage(msgRequest, nextcand.IPEndPoint);
                }
            }
        }

        System.Threading.ManualResetEvent EventWaitForInitiatedToRespond = new System.Threading.ManualResetEvent(false);
        protected virtual void PerformICEStunProcedures()
        {
            Candidate CurrentLocalCandidate = null;
            Candidate CurrentRemoteCandidate = null;

            IPEndPoint epRemoteStun = null;

            //TODO, sort, priority, all other bs
            //Candidates.Sort(this); 
            bool GotBothResponse = false;

            foreach (Candidate nextlocalcand in this.LocalCandidates)
            {
                CurrentLocalCandidate = nextlocalcand;
                // Now perform ICE tests to each of our candidates, see which one we get a response from.
                foreach (Candidate nextcand in this.RemoteCandidates)
                {
                    CurrentRemoteCandidate = nextcand;

                    if (nextcand.name == "rtcp")
                        continue;
                    if (nextcand.type == "stun")
                        epRemoteStun = nextcand.IPEndPoint;

                    if ((nextcand.type == "stun") || (nextcand.type == "local"))
                    {
                        IPEndPoint epresponded = null;
                        string strUserName = string.Format("{0}:{1}", nextcand.username, nextlocalcand.username);
                        string strPassword = nextcand.password;
                        EventWaitForInitiatedToRespond.Reset();

                        epresponded = this.PerformSTUNRequest(nextcand.IPEndPoint, 200, true, true, nextcand.priority, strUserName, strPassword);
                        if (epresponded != null)
                        {
                            this.RemoteEndpoint = nextcand.IPEndPoint;

                            /// Now we must wait until we get an incoming request from the remote end
                            /// 
                            if (EventWaitForInitiatedToRespond.WaitOne(2000) == true)
                            {
                                GotBothResponse = true;
                                break;
                            }
                        }
                    }
                }

                if (GotBothResponse == true)
                    break;
            }

            CurrentLocalCandidate = null;
            CurrentRemoteCandidate = null;
        
            if (this.RemoteEndpoint == null)
                this.RemoteEndpoint = epRemoteStun;

            ICEDoneStartRTP();
        }


        void m_objAudioRTPStream_OnUnhandleSTUNMessage(STUNMessage smsg, IPEndPoint epfrom)
        {
            if (Initiator == false)
            {
                /// We are the receiver in the call.  Here is the STUN exchange
                /// Initiator         Receiver (local)
                /// 
                /// -->     binding     -->   could be here, getting the initial binding request from the initiator
                /// <--     response    <--
                ///     
                /// <--     binding     <--
                /// -->     response    -->   could be here, extract/verify info

                
                /// Our RTPStream received a STUN message.
                if (smsg.Class == StunClass.Request)
                {
                    if (smsg.Method == StunMethod.Binding)
                    {

                        STUNMessage sresp = new STUNMessage();
                        sresp.TransactionId = smsg.TransactionId;
                        sresp.Method = StunMethod.Binding;
                        sresp.Class = StunClass.Success;

                        MappedAddressAttribute attr = new MappedAddressAttribute();
                        attr.Port = (ushort)epfrom.Port;
                        attr.IPAddress = epfrom.Address;
                        attr.Type = StunAttributeType.MappedAddress;
                        attr.AddressFamily = StunAddressFamily.IPv4;
                        sresp.AddAttribute(attr);

                        IceControlledAttribute iattr = new IceControlledAttribute();
                        sresp.AddAttribute(iattr);

                        AudioRTPStream.SendSTUNMessage(sresp, epfrom);

                        Candidate CurrentLeftCandidate = null;
                        Candidate CurrentRightCandidate = null;


                        /// Find this candidate pair so we can send to it
                        foreach (STUNAttributeContainer cont in smsg.Attributes)
                        {
                            if (cont.ParsedAttribute.Type == StunAttributeType.UserName)
                            {

                                UserNameAttribute unameattrib = cont.ParsedAttribute as UserNameAttribute;
                                int nColonAt = unameattrib.UserName.IndexOf(":");
                                if (nColonAt > 0)
                                {
                                    /// should be rusername:lusername
                                    /// 
                                    string strrfrag = unameattrib.UserName.Substring(0, nColonAt);
                                    string strlfrag = unameattrib.UserName.Substring(nColonAt + 1);
                                    // try to find these candidate
                                    foreach (Candidate can in this.LocalCandidates)
                                    {
                                        if (can.username == strrfrag)
                                        {
                                            CurrentRightCandidate = can;
                                            break;
                                        }
                                    }
                                    foreach (Candidate can in this.RemoteCandidates)
                                    {
                                        if (can.username == strlfrag)
                                        {
                                            CurrentLeftCandidate = can;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if ((CurrentLeftCandidate != null) && (CurrentRightCandidate != null))
                        {
                            /// Send out a binding with our candidate
                            string strUserName = string.Format("{0}:{1}", CurrentLeftCandidate.username, CurrentRightCandidate.username);
                            string strPassword = CurrentLeftCandidate.password;
                            /// CurrentLeftCandidate.IPEndPoint should be epfrom

                            IPEndPoint ep = this.PerformSTUNRequest(CurrentLeftCandidate.IPEndPoint, 200, true, true, CurrentLeftCandidate.priority, strUserName, strPassword);
                            if (ep != null)
                            {
                                RemoteEndpoint = CurrentLeftCandidate.IPEndPoint;
                                EventWaitForInitiatedToRespond.Set();
                                ICEDoneStartRTP();
                            }
                        }
                        else
                        {
                            // Shouldn't happen
                            IPEndPoint ep = this.PerformSTUNRequest(epfrom, 200, true, true, 0, null, null);
                            RemoteEndpoint = epfrom;
                            EventWaitForInitiatedToRespond.Set();
                            ICEDoneStartRTP();
                        }
                    }
                }
            }
            else 
            {
                /// We are the initiator in the call, here is the STUN exchange
                /// Initiator         Receiver (local)
                /// 
                /// -->     binding     -->
                /// <--     response    <--   (won't be here, because that message is returned as a response to a send and will never hit this event)
                ///     
                /// <--     binding     <--   could be here, receiving the binding request from the receiver..  check all the passwords/etc, if we want
                /// -->     response    -->    
                /// 

                /// Our RTPStream received a STUN message.
                if (smsg.Class == StunClass.Request)
                {
                    if (smsg.Method == StunMethod.Binding)
                    {

                        STUNMessage sresp = new STUNMessage();
                        sresp.TransactionId = smsg.TransactionId;
                        sresp.Method = StunMethod.Binding;
                        sresp.Class = StunClass.Success;

                        MappedAddressAttribute attr = new MappedAddressAttribute();
                        attr.Port = (ushort)epfrom.Port;
                        attr.IPAddress = epfrom.Address;
                        attr.Type = StunAttributeType.MappedAddress;
                        attr.AddressFamily = StunAddressFamily.IPv4;
                        sresp.AddAttribute(attr);

                        IceControlledAttribute iattr = new IceControlledAttribute();
                        sresp.AddAttribute(iattr);

                        AudioRTPStream.SendSTUNMessage(sresp, epfrom);

                    }
                }
            }
        }

        public const ushort StunPort = 3478;
        public IPEndPoint PublicIPEndpoint = null;

        public IPEndPoint PerformSTUNRequest(string strStunServer, int nTimeout)
        {
            IPEndPoint epStun = SocketServer.ConnectMgr.GetIPEndpoint(strStunServer, StunPort);
            return PerformSTUNRequest(epStun, nTimeout);
        }

        public IPEndPoint PerformSTUNRequest(IPEndPoint epStun, int nTimeout)
        {
            return PerformSTUNRequest(epStun, nTimeout, false, false, 0, null, null);
        }

        /// <summary>
        ///  Send out a stun request to discover our IP address transalation
        /// </summary>
        /// <param name="strStunServer"></param>
        /// <returns></returns>
        public IPEndPoint PerformSTUNRequest(IPEndPoint epStun, int nTimeout, bool bICE, bool bIsControlling, int nPriority, string strUsername, string strPassword)
        {
            STUNMessage msgRequest = new STUNMessage();
            msgRequest.Method = StunMethod.Binding;
            msgRequest.Class = StunClass.Request;


            MappedAddressAttribute mattr = new MappedAddressAttribute();
            mattr.IPAddress = LocalEndpoint.Address;
            mattr.Port = (ushort)LocalEndpoint.Port;

            msgRequest.AddAttribute(mattr);

            if (bICE == true)
            {
                PriorityAttribute pattr = new PriorityAttribute();
                pattr.Priority = nPriority;
                msgRequest.AddAttribute(pattr);

                UseCandidateAttribute uattr = new UseCandidateAttribute();
                msgRequest.AddAttribute(uattr);

                if (strUsername != null)
                {
                    UserNameAttribute unameattr = new UserNameAttribute();
                    unameattr.UserName = strUsername;
                    msgRequest.AddAttribute(unameattr);
                }
                if (strPassword != null)
                {
                    PasswordAttribute passattr = new PasswordAttribute();
                    passattr.Password = strPassword;
                    msgRequest.AddAttribute(passattr);
                }

                if (bIsControlling == true)
                {
                    IceControllingAttribute cattr = new IceControllingAttribute();
                    msgRequest.AddAttribute(cattr);
                }
                else
                {
                    IceControlledAttribute cattr = new IceControlledAttribute();
                    msgRequest.AddAttribute(cattr);
                }
            }

            STUNMessage ResponseMessage = this.AudioRTPStream.SendRecvSTUN(epStun, msgRequest, nTimeout);

            IPEndPoint retep = null;
            if (ResponseMessage != null)
            {
                foreach (STUNAttributeContainer cont in ResponseMessage.Attributes)
                {
                    if (cont.ParsedAttribute.Type == StunAttributeType.MappedAddress)
                    {

                        MappedAddressAttribute attrib = cont.ParsedAttribute as MappedAddressAttribute;
                        retep = new IPEndPoint(attrib.IPAddress, attrib.Port);
                    }
                }
            }
            return retep;
        }

        public static uint CalculatePriority(int typepref, int localpref, int nComponentid)
        {
              // priority = (2^24)*(type preference) +
              //(2^8)*(local preference) +
              //(2^0)*(256 - component ID)

            uint nPriority = (uint) (((typepref & 0x7E) << 24) | ((localpref & 0xFFFF) >> 8) | (256 - nComponentid & 0xFF));
            return nPriority;
        }

        public static void CalculatePriority(int typepref, int localpref, Candidate cand)
        {
            // priority = (2^24)*(type preference) +
            //(2^8)*(local preference) +
            //(2^0)*(256 - component ID)
            cand.priority = (int)(((typepref & 0x7E) << 24) | ((localpref & 0xFFFF) >> 8) | (256 - cand.component & 0xFF));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        void FirePropertyChanged(string strName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(strName));
        }

        #endregion

        #region IComparer<Candidate> Members

        public int Compare(Candidate x, Candidate y)
        {
            return x.priority.CompareTo(y.priority);
        }

        #endregion
    }

    public class JingleMediaSession : MediaSession
    {
        public JingleMediaSession(string strSession, string strRemoteJID, IPEndPoint LocalEp, XMPPClient client) : 
            base(strSession, strRemoteJID, LocalEp, client) 
        {
        }

        public JingleMediaSession(string strRemoteJID, IPEndPoint LocalEp, XMPPClient client) : 
            base(strRemoteJID, LocalEp, client)
        {
        }

        public JingleMediaSession(string strSession, IQ intitialIQ, KnownAudioPayload LocalPayloads, IPEndPoint LocalEp, XMPPClient client) :
            base(strSession, intitialIQ, LocalPayloads, LocalEp, client)
        {
        }

        public override string SendInitiateSession()
        {
            Jingle jingleinfo = BuildOutgoingAudioRequest(true, false);

            string strSession = XMPPClient.JingleSessionManager.InitiateNewSession(RemoteJID, jingleinfo);
            Session = strSession;
            return Session;
        }

        bool m_bHasAccepted = false;
        protected override void SendAcceptSession()
        {
            if (RemoteCandidates.Count < 0)
                return;
            if (m_bHasAccepted == true)
                return;

            m_bHasAccepted = true;
            Jingle jingleinfo = BuildOutgoingAudioRequest(false, false);
            XMPPClient.JingleSessionManager.SendAcceptSession(this.Session, jingleinfo);
        }


        protected override void ParsePayloads(IQ iq)
        {
            JingleIQ jingleiq = iq as JingleIQ;
            if (jingleiq == null)
                return;
            Jingle jingle = jingleiq.Jingle;
            if (jingle == null)
                return;

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


        protected override void ParseCandidates(IQ iq)
        {
            JingleIQ jingleiq = iq as JingleIQ;
            if (jingleiq == null)
                return;
            Jingle jingle = jingleiq.Jingle;
            if (jingle == null)
                return;

            if (jingle != null)
            {
                if ((jingle.Content != null) && (jingle.Content.ICETransport != null))
                {
                    if (jingle.Content.ICETransport.Candidates.Count > 0)
                    {
                        UseGoogleTalkProtocol = false;
                        foreach (Candidate nextcand in jingle.Content.ICETransport.Candidates)
                        {
                            nextcand.IPEndPoint = new IPEndPoint(IPAddress.Parse(nextcand.ipaddress), nextcand.port);
                            RemoteCandidates.Add(nextcand);
                        }
                    }

                }
                else if ((jingle.Content != null) && (jingle.Content.GoogleTransport != null))
                {
                    if (jingle.Content.GoogleTransport.Candidates.Count > 0)
                    {
                        UseGoogleTalkProtocol = true;
                        foreach (Candidate nextcand in jingle.Content.GoogleTransport.Candidates)
                        {
                            nextcand.IPEndPoint = new IPEndPoint(IPAddress.Parse(nextcand.ipaddress), nextcand.port);
                            RemoteCandidates.Add(nextcand);
                        }
                    }

                }
            }
        }


        protected Jingle BuildOutgoingAudioRequest(bool IsInitiation, bool bAddCandidates)
        {
            Jingle jingleinfo = JingleSessionManager.CreateBasicOutgoingAudioRequest(this.AudioRTPStream.LocalEndpoint.Address.ToString(), this.AudioRTPStream.LocalEndpoint.Port);
            jingleinfo.Content.Description.Payloads.Clear();
            jingleinfo.Content.Name = "audio";

            if (IsInitiation == true)
            {
                jingleinfo.Content.Description.Payloads.AddRange(LocalPayloads);
                jingleinfo.Content.Creator = "initiator";
            }

            else
            {
                jingleinfo.Content.Creator = "responder";
                SetCodecsFromPayloads();
                jingleinfo.Content.Description.Payloads.Add(AgreedPayload);
            }

            if (UseGoogleTalkProtocol == true)
            {
                jingleinfo.Content.ICETransport = null;
                jingleinfo.Content.GoogleTransport = new Transport();
                if (bAddCandidates == true)
                    jingleinfo.Content.GoogleTransport.Candidates.AddRange(LocalCandidates);
            }
            else
            {
                jingleinfo.Content.ICETransport = new Transport();
                jingleinfo.Content.GoogleTransport = null;
                if (bAddCandidates == true)
                    jingleinfo.Content.ICETransport.Candidates.AddRange(LocalCandidates);
            }

            return jingleinfo;
        }

    
    }
}
